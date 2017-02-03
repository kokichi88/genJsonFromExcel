using System.Collections.Generic;
using Artemis;
using Com.LuisPedroFonseca.ProCamera2D;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils.DataStruct;
using Animation = Ssar.Combat.HeroStateMachines.Animation;
using AnimMix = Combat.Skills.ModifierConfigs.AnimationMix;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class BlastModifier : BaseModifier {
		private BlastInfo info;
		private Entity caster;
		private Entity targetEntity;
		private Camera camera;
		private SkillId skillId;
		private readonly Vector3 collidedProjectilePosition;
		private readonly Environment environment;
		private readonly WallHitConfig wallHitConfig;
		private readonly float damageScale;

		private float elapsed;
		private Character character;
		private State state;
		private float stateElapsed;
		private Animation targetAnimation;
		private BlastRequest blastRequest;
		private List<JumpAction.Event> processedEvents = new List<JumpAction.Event>();
		private BlastRequest.Stage stageFromPreviousCheck;
		private Vfxs.Vfx.Logic vfxLogic;
		private Character targetCharacter;
		private DurationBasedLifetime lifetime;
		private AnimProfile animProfile;
		private Stats extraLyingDurationStats;
		private MovementComponent targetMovementComponent;
		private bool isWallHit;
		private MovementComponent casterMovementComponent;
		private bool justChangedState = false;
		private DefaultUserInput targetDefaultUserInput;

		public BlastModifier(BlastInfo info, Entity caster, Entity target,
		                     Camera camera, SkillId skillId, Vector3 collidedProjectilePosition,
		                     Environment environment,
		                     CollectionOfInteractions modifierInteractionCollection,
		                     WallHitConfig wallHitConfig, float damageScale) : base(info, caster, target, environment, modifierInteractionCollection) {
			this.info = info;
			this.caster = caster;
			targetEntity = target;
			this.camera = camera;
			this.skillId = skillId;
			this.collidedProjectilePosition = collidedProjectilePosition;
			this.environment = environment;
			this.wallHitConfig = wallHitConfig;
			this.damageScale = damageScale;
			this.targetAnimation = target.GetComponent<AnimationComponent>().Animation;
			targetCharacter = target.GetComponent<SkillComponent>().Character;
			UserInput userInput = target.GetComponent<HeroStateMachineComponent>().UserInput;
			targetDefaultUserInput = (DefaultUserInput) userInput;
			switch (this.info.AnimationProfile) {
				case BlastModifierConfig.AnimationProfile.Far:
					animProfile = new FarAnimProfile();
					break;
				case BlastModifierConfig.AnimationProfile.High:
					animProfile = new HighAnimProfile();
					break;
				case BlastModifierConfig.AnimationProfile.Medium:
					animProfile = new MediumAnimProfile();
					break;
			}

			StatsComponent targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			bool found;
			extraLyingDurationStats = targetStatsComponent.CharacterStats.FindStats(
				StatsType.ExtraLyingDuration, out found
			);
			if (found) {
				lifetime.DynamicExtraDuration += extraLyingDurationStats.BakedFloatValue;
			}
			casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
		}

		public override string Name() {
			return string.Format("{0}({1}/{2})", Type(), attachType, state);
		}

		public override ModifierType Type() {
			return ModifierType.Blast;
		}

		protected override void OnUpdate(float dt) {
			justChangedState = false;
			if (IsFinish()) return;
			if (attachType != ModifierAttachType.Main) return;

			elapsed += dt;
			stateElapsed += dt;
			if (state == State.Peaking) {
				if (stateElapsed >= info.TimeToPeak) {
					state = State.Grounding;
					stateElapsed -= info.TimeToPeak;
					SsarTuple<AnimMix, PlayMethod, float> mix = info.FindAnimMixing(AnimMix.UpperToFallLoop);
					targetAnimation.PlayAnimation(
						animProfile.FallLoop(), 1, mix.Element2, mix.Element3
					);
				}
			}

			if (state == State.Grounding) {
				if (character.IsOnGround()) {
					justChangedState = true;
					state = State.Rolling;
					stateElapsed -= (info.TimeToGround);
					SsarTuple<AnimMix, PlayMethod, float> mix = info.FindAnimMixing(AnimMix.FallLoopToLie);
					targetAnimation.PlayAnimation(
						animProfile.FallToLie(), 1, mix.Element2, mix.Element3
					);
					character.QueueAnimation(animProfile.LieLoop());
					TriggerEvents();
				}
			}

			if (state == State.Rolling) {
				if (stateElapsed >= info.TimeToRoll) {
					state = State.Lying;
					stateElapsed -= (info.TimeToRoll);
				}
			}

			if (state == State.Lying) {
				float extraLyingDuration = 0;
				if (extraLyingDurationStats != null) {
					extraLyingDuration += extraLyingDurationStats.BakedFloatValue;
				}
				if (stateElapsed >= info.TimeToLie + extraLyingDuration) {
					justChangedState = true;
					state = State.LieToIdle;
					stateElapsed = 0;
					if (targetEntity.GetComponent<HealthComponent>().IsAlive()) {
						character.PlayAnimation(animProfile.LieToIdle());
					}
					targetMovementComponent.MovingDirection = targetMovementComponent.FacingDirection.ToNormalizedVector2();
					targetDefaultUserInput.SetRunDirection(targetMovementComponent.FacingDirection);
				}
			}

			if (state == State.LieToIdle) {
				if (stateElapsed >= info.LieToIdleDuration) {
					lifetime.End();
				}
			}

			if (vfxLogic != null) {
				vfxLogic.Update(dt);
			}
		}

		private void TriggerEvents() {
			for (int kIndex = 0; kIndex < info.Events.Count; kIndex++) {
				JumpAction.Event e = info.Events[kIndex];
				if (e.ShowTriggerType() != JumpAction.Event.TriggerType.OnGroundLanding) continue;
				if (processedEvents.Contains(e)) continue;

				processedEvents.Add(e);
				JumpAction.Event.ActionType at = e.ShowActionType();
				switch (at) {
					case JumpAction.Event.ActionType.CameraFx:
						JumpAction.CameraFxEvent cfe = (JumpAction.CameraFxEvent) e;
						CameraAction.FxType ft = cfe.fx.ShowFxType();
						switch (ft) {
							case CameraAction.FxType.Shake:
								CameraAction.ShakeFx sf = (CameraAction.ShakeFx) cfe.fx;
								ProCamera2DShake proCamera2DShake;
								proCamera2DShake = camera.GetComponent<ProCamera2DShake>();
								if (proCamera2DShake == null) {
									proCamera2DShake = camera.gameObject.AddComponent<ProCamera2DShake>();
								}

								proCamera2DShake.Strength = sf.strength;
								proCamera2DShake.Duration = sf.duration;
								proCamera2DShake.Vibrato = sf.vibrato;
								proCamera2DShake.Smoothness = sf.smoothness;
								proCamera2DShake.Randomness = sf.randomness;
								proCamera2DShake.UseRandomInitialAngle = sf.useRandomInitialAngel;
								proCamera2DShake.Rotation = sf.rotation;
								proCamera2DShake.Shake();
								break;
						}
						break;
					case JumpAction.Event.ActionType.Vfx:
						JumpAction.VfxEvent ve = (JumpAction.VfxEvent) e;
						VfxAction.VfxType vt = ve.fx.ShowVfxType();
						switch (vt) {
							case VfxAction.VfxType.SpawnPrefab:
								vfxLogic = new Vfxs.Vfx.SpawnPrefab(
									10, (VfxAction.SpawnPrefabVfx) ve.fx,
									new DefaultVfxGameObjectFactory(), targetCharacter,
									SkillCastingSource.FromUserInput(), environment.GetCamera(),
									environment, null
								);
								break;
						}
						break;
				}
			}

			stageFromPreviousCheck = blastRequest.ShowStage();
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character character, Modifier byModifier) {
			base.OnBeReplaced(character, byModifier);
			targetMovementComponent.WallCollisionHandler -= OnWallCollision;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			this.character = target;
			Character casterCharacter = caster.GetComponent<SkillComponent>().Character;
			Direction casterFacingDirection = casterCharacter.FacingDirection();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			targetMovementComponent.WallCollisionHandler += OnWallCollision;
			Direction movementDirection = StaggerModifier.CalculateMovementDirection(
				casterMovementComponent, targetMovementComponent,
				collidedProjectilePosition, info.MovementBehavior
			);
			Direction facingDirection = StaggerModifier.CalculateFacingDirection(
				casterMovementComponent, targetMovementComponent,
				collidedProjectilePosition, info.FacingBehavior, movementDirection
			);
			targetMovementComponent.MovingDirection = movementDirection.ToNormalizedVector2();
			targetMovementComponent.FacingDirection = facingDirection;


			target.InterruptChannelingSkill();
			target.PlayAnimation(animProfile.Upper());
			target.JumpToFrame(4);
			//DLog.LogError("Jump on knockdown");
			blastRequest = (BlastRequest) DoBlast(target);
			stageFromPreviousCheck = blastRequest.ShowStage();
			state = State.Peaking;
		}

		private void OnWallCollision(object sender, WallCollisionEventArgs e) {
			if (!info.EnableWallHit) return;
			if (state != State.Peaking) return;

			if (!isWallHit) {
				isWallHit = true;
				bool found;
				StatsComponent casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
				Stats casterAtkStats = casterStatsComponent.CharacterStats.FindStats(StatsType.RawAtk, out found);
				DamageFromAttack dfa = new DamageFromAttack(
					new SourceHistory(Source.FromSkill(info.ShowParentSkill(), skillId)),
					wallHitConfig.damageScale * damageScale, false, 1, 1, caster.Id,
					targetCharacter.Position(), targetCharacter.Position(),
					casterStatsComponent.CharacterStats
				);
				for (int kIndex = 0; kIndex < wallHitConfig.modifiers.Count; kIndex++) {
					dfa.AddModifierInfo(
						CastProjectileAction.OnHitPhase.Damaged,
						((DefaultSkillCharacter) targetCharacter).CreateModifierInfo(
							info.ShowParentSkill(),  wallHitConfig.modifiers[kIndex]
						)
					);
				}
				targetEntity.GetComponent<HealthComponent>().ReceiveDamage(dfa);
			}
		}

		protected virtual Request DoBlast(Character target) {
			return target.Blast(
				info.Height, info.TimeToPeak, info.TimeToGround, info.FlightDistance,
				info.FlightMinSpeed, info.RollDistance, info.TimeToRoll
			);
		}

		public override void OnDetach(Character character) {
			targetMovementComponent.WallCollisionHandler -= OnWallCollision;
			blastRequest.Abort();
		}

		public override object[] Cookies() {
			return new object[0];
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			BlastInfo bi = (BlastInfo) modifierInfo;
			float duration = bi.TimeToPeak + bi.TimeToGround + bi.TimeToRoll + bi.TimeToLie;
			lifetime = new DurationBasedLifetime(duration);
			return new List<Lifetime>(new [] {lifetime});
		}

		public override bool TryQueryingState(out MainModifierState value, out bool justChanged) {
			switch (state) {
				case State.Peaking:
				case State.Grounding:
					value = MainModifierState.Air;
					justChanged = justChangedState;
					return true;
				case State.Rolling:
				case State.Lying:
					value = MainModifierState.Ground;
					justChanged = justChangedState;
					return true;
				case State.LieToIdle:
					value = MainModifierState.LieToIdle;
					justChanged = justChangedState;
					return true;
			}

			return base.TryQueryingState(out value, out justChanged);
		}

		private enum State {
			Peaking,
			Grounding,
			Rolling,
			Lying,
			LieToIdle
		}

		private class DefaultVfxGameObjectFactory : Vfxs.Vfx.VfxGameObjectFactory {
			public GameObject Instantiate(GameObject prefab) {
				return GameObject.Instantiate(prefab);
			}
		}

		public interface AnimProfile {
			string Upper();
			string FallLoop();
			string FallToLie();
			string LieLoop();
			string LieToIdle();
		}

		public class MediumAnimProfile : AnimProfile {
			public string Upper() {
				return AnimationName.Knockdown.Medium.UPPER;
			}

			public string FallLoop() {
				return AnimationName.Knockdown.Medium.FALL_LOOP;
			}

			public string FallToLie() {
				return AnimationName.Knockdown.Medium.FALL_TO_LIE;
			}

			public string LieLoop() {
				return AnimationName.Knockdown.Medium.LIE_LOOP;
			}

			public string LieToIdle() {
				return AnimationName.Knockdown.Medium.LIE_TO_IDLE;
			}
		}

		public class HighAnimProfile : AnimProfile {
			public string Upper() {
				return AnimationName.Knockdown.High.UPPER;
			}

			public string FallLoop() {
				return AnimationName.Knockdown.High.FALL_LOOP;
			}

			public string FallToLie() {
				return AnimationName.Knockdown.High.FALL_TO_LIE;
			}

			public string LieLoop() {
				return AnimationName.Knockdown.High.LIE_LOOP;
			}

			public string LieToIdle() {
				return AnimationName.Knockdown.High.LIE_TO_IDLE;
			}
		}

		public class FarAnimProfile : AnimProfile {
			public string Upper() {
				return AnimationName.Knockdown.Far.UPPER;
			}

			public string FallLoop() {
				return AnimationName.Knockdown.Far.FALL_LOOP;
			}

			public string FallToLie() {
				return AnimationName.Knockdown.Far.FALL_TO_LIE;
			}

			public string LieLoop() {
				return AnimationName.Knockdown.Far.LIE_LOOP;
			}

			public string LieToIdle() {
				return AnimationName.Knockdown.Far.LIE_TO_IDLE;
			}
		}
	}
}