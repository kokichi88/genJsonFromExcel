using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Com.LuisPedroFonseca.ProCamera2D;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils.Extensions;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils;
using Utils.DataStruct;
using Animation = Ssar.Combat.HeroStateMachines.Animation;
using Behavior = Core.Skills.Modifiers.Info.KnockdownInfo.Behavior;
using AnimMix = Combat.Skills.ModifierConfigs.AnimationMix;
using Event = Ssar.Combat.Skills.Events.Actions.JumpAction.Event;
using Vfx = Core.Skills.Vfxs.Vfx;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.MovementBehavior;
using FacingBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.FacingBehavior;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class KnockdownModifier : BaseModifier {
		private KnockdownInfo knockdownInfo;
		private Vector3 collidedProjectilePosition;
		private readonly Vector3 collidedProjectileVelocity;
		private readonly Entity caster;
		private readonly Entity target;
		private Camera camera;
		private readonly SkillId skillId;
		private readonly Environment environment;
		private readonly WallHitConfig whc;
		private readonly float damageScale;

		private float elapsed;
		private MovementComponent movementComponent;
		private Character character;
		protected State state;
		private float stateElapsed;
		private Animation targetAnimation;
		private Request jumpRequest;
		private List<Event> processedEvents = new List<Event>();
		private Vfxs.Vfx.Logic vfxLogic;
		private Character targetCharacter;
		private DurationBasedLifetime lifetime;
		private Stats extraLyingDurationStats;
		private MovementComponent targetMovementComponent;
		private bool isWallHit;
		private BakedStatsContainer characterStats;
		private MovementComponent casterMovementComponent;
		private bool justChanged;
		private DefaultUserInput targetDefaultUserInput;

		public KnockdownModifier(KnockdownInfo knockdownInfo, Vector3 collidedProjectilePosition,
		                         Vector3 collidedProjectileVelocity,
		                         Entity caster, Entity target, Camera camera, SkillId skillId,
		                         Environment environment, CollectionOfInteractions modifierInteractionCollection,
		                         WallHitConfig wallHitConfig, float damageScale) : base(knockdownInfo, caster, target, environment, modifierInteractionCollection) {
			this.knockdownInfo = knockdownInfo;
			this.collidedProjectilePosition = collidedProjectilePosition;
			this.collidedProjectileVelocity = collidedProjectileVelocity;
			this.caster = caster;
			this.target = target;
			this.camera = camera;
			this.skillId = skillId;
			this.environment = environment;
			this.whc = wallHitConfig;
			this.damageScale = damageScale;
			movementComponent = target.GetComponent<MovementComponent>();
			targetAnimation = target.GetComponent<AnimationComponent>().Animation;
			targetCharacter = target.GetComponent<SkillComponent>().Character;
			UserInput userInput = target.GetComponent<HeroStateMachineComponent>().UserInput;
			targetDefaultUserInput = (DefaultUserInput) userInput;

			StatsComponent targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			bool found;
			extraLyingDurationStats = targetStatsComponent.CharacterStats.FindStats(
				StatsType.ExtraLyingDuration, out found
			);
			if (found) {
				lifetime.DynamicExtraDuration += extraLyingDurationStats.BakedFloatValue;
			}
			StatsComponent casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			characterStats = casterStatsComponent.CharacterStats;
			casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
		}

		public override string Name() {
			return string.Format("{0}({1}/{2})", Type(), attachType, state);
		}

		public override ModifierType Type() {
			return ModifierType.Knockdown;
		}

		protected override void OnUpdate(float dt) {
			justChanged = false;
			if (IsFinish()) return;
			if(attachType != ModifierAttachType.Main) return;

			elapsed += dt;
			stateElapsed += dt;
			if (state == State.Peaking) {
				if (stateElapsed >= knockdownInfo.TimeToPeak) {
					state = State.Floating;
					stateElapsed -= knockdownInfo.TimeToPeak;
				}
			}

			if (state == State.Floating) {
				if (stateElapsed >= knockdownInfo.FloatingDur) {
					state = State.Grounding;
					SsarTuple<AnimMix, PlayMethod, float> mix = knockdownInfo.FindAnimMixing(AnimMix.UpperToFallLoop);
					targetAnimation.PlayAnimation(AnimName().FallLoop, 1, mix.Element2, mix.Element3);
					stateElapsed -= knockdownInfo.FloatingDur;
				}
			}

			if (state == State.Grounding) {
				if (character.IsOnGround()) {
					justChanged = true;
					state = State.Lying;
					stateElapsed -= knockdownInfo.TimeToGround;
					SsarTuple<AnimMix, PlayMethod, float> mix = knockdownInfo.FindAnimMixing(AnimMix.FallLoopToLie);
					targetAnimation.PlayAnimation(AnimName().FallToLie, 1, mix.Element2, mix.Element3);
					character.QueueAnimation(AnimName().LieLoop);
					TriggerEvents();
				}
			}

			if (state == State.Lying) {
				float extraLyingDuration = 0;
				if (extraLyingDurationStats != null) {
					extraLyingDuration += extraLyingDurationStats.BakedFloatValue;
				}
				if (stateElapsed >= knockdownInfo.LieDuration + extraLyingDuration) {
					justChanged = true;
					state = State.LieToIdle;
					stateElapsed -= (knockdownInfo.LieDuration + extraLyingDuration);
					if (target.GetComponent<HealthComponent>().IsAlive()) {
						character.PlayAnimation(AnimName().LieToIdle);
					}
					targetMovementComponent.MovingDirection = targetMovementComponent.FacingDirection.ToNormalizedVector2();
					targetDefaultUserInput.SetRunDirection(targetMovementComponent.FacingDirection);
				}
			}

			if (state == State.LieToIdle) {
				if (stateElapsed >= knockdownInfo.LieToIdleDuration) {
					lifetime.End();
				}
			}

			if (vfxLogic != null) {
				vfxLogic.Update(dt);
			}
		}

		private void TriggerEvents() {
			for (int kIndex = 0; kIndex < knockdownInfo.Events.Count; kIndex++) {
				Event e = knockdownInfo.Events[kIndex];
				if (e.ShowTriggerType() != Event.TriggerType.OnGroundLanding) continue;
				if (processedEvents.Contains(e)) continue;

				processedEvents.Add(e);
				Event.ActionType at = e.ShowActionType();
				switch (at) {
					case Event.ActionType.CameraFx:
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
					case Event.ActionType.Vfx:
						JumpAction.VfxEvent ve = (JumpAction.VfxEvent) e;
						VfxAction.VfxType vt = ve.fx.ShowVfxType();
						switch (vt) {
							case VfxAction.VfxType.SpawnPrefab:
								vfxLogic = new Vfxs.Vfx.SpawnPrefab(
									10, (VfxAction.SpawnPrefabVfx) ve.fx,
									new DefaultVfxGameObjectFactory(), targetCharacter,
									SkillCastingSource.FromUserInput(),
									environment.GetCamera(), environment, null
								);
								break;
						}
						break;
				}
			}
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character character, Modifier byModifier) {
			base.OnBeReplaced(character, byModifier);
			if (targetMovementComponent != null) {
				targetMovementComponent.WallCollisionHandler -= OnWallCollision;
			}
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			this.character = target;
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			targetMovementComponent.WallCollisionHandler += OnWallCollision;
			Direction movementDirection = StaggerModifier.CalculateMovementDirection(
				casterMovementComponent, targetMovementComponent,
				collidedProjectilePosition, knockdownInfo.MovementBehavior
			);
			Direction facingDirection = StaggerModifier.CalculateFacingDirection(
				casterMovementComponent, targetMovementComponent,
				collidedProjectilePosition, knockdownInfo.FacingBehavior, movementDirection
			);
			targetMovementComponent.MovingDirection = movementDirection.ToNormalizedVector2();
			targetMovementComponent.FacingDirection = facingDirection;

			target.InterruptChannelingSkill();
			target.PlayAnimation(AnimName().Upper, GetSpeedOfUpperAnimation(), true);
			//DLog.LogError("Jump on knockdown");
			jumpRequest = target.JumpOverDistance(
				knockdownInfo.Height, knockdownInfo.TimeToPeak, knockdownInfo.Distance,
				knockdownInfo.TimeToGround, false, knockdownInfo.FloatingDur,
				knockdownInfo.StopHorizontalMovementWhenMeet, false, false,
				knockdownInfo.MoveHorizontallyWhenFloat
			);
			PlayVfx();
			state = State.Peaking;
		}

		private void OnWallCollision(object sender, WallCollisionEventArgs e) {
			if (!knockdownInfo.EnableWallHit) return;
			if (state != State.Peaking) return;

			if (!isWallHit) {
				isWallHit = true;
				bool found;
				Stats casterAtkStats = casterEntity.GetComponent<StatsComponent>().CharacterStats.FindStats(StatsType.RawAtk, out found);
				DamageFromAttack dfa = new DamageFromAttack(
					new SourceHistory(Source.FromSkill(knockdownInfo.ShowParentSkill(), skillId)),
					whc.damageScale * damageScale, false, 1, 1, caster.Id, targetCharacter.Position(),
					targetCharacter.Position(),
					characterStats
				);
				for (int kIndex = 0; kIndex < whc.modifiers.Count; kIndex++) {
					dfa.AddModifierInfo(
						CastProjectileAction.OnHitPhase.Damaged,
						((DefaultSkillCharacter) targetCharacter).CreateModifierInfo(
							knockdownInfo.ShowParentSkill(),  whc.modifiers[kIndex]
						)
					);
				}
				targetEntity.GetComponent<HealthComponent>().ReceiveDamage(dfa);
			}
		}

		protected virtual KnockdownAnimationName AnimName() {
			return new KnockdownAnimationName(
				AnimationName.Knockdown.Medium.UPPER,
				AnimationName.Knockdown.Medium.FALL_LOOP,
				AnimationName.Knockdown.Medium.FALL_TO_LIE,
				AnimationName.Knockdown.Medium.LIE_LOOP,
				AnimationName.Knockdown.Medium.LIE_TO_IDLE
			);
		}

		protected virtual float GetSpeedOfUpperAnimation() {
			return 1f;
		}

		public override void OnDetach(Character character) {
			base.OnDetach(character);
			if (targetMovementComponent != null) {
				targetMovementComponent.WallCollisionHandler -= OnWallCollision;
			}

			if (jumpRequest != null) {
				jumpRequest.Abort();
			}
		}

		public override object[] Cookies() {
			return new object[0];
		}

		private void PlayVfx() {
		}

		public State ShowState() {
			return state;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			KnockdownInfo ki = (KnockdownInfo) modifierInfo;
			float duration = ki.TimeToPeak + ki.FloatingDur + ki.TimeToGround + ki.LieDuration + ki.LieToIdleDuration;
			lifetime = new DurationBasedLifetime(duration);
			return new List<Lifetime>(new [] {lifetime});
		}

		public override bool TryQueryingState(out MainModifierState value, out bool justChanged) {
			switch (state) {
				case State.Peaking:
				case State.Floating:
				case State.Grounding:
					value = MainModifierState.Air;
					justChanged = this.justChanged;
					return true;
				case State.Lying:
					value = MainModifierState.Ground;
					justChanged = this.justChanged;
					return true;
				case State.LieToIdle:
					value = MainModifierState.LieToIdle;
					justChanged = this.justChanged;
					return true;
			}

			return base.TryQueryingState(out value, out justChanged);
		}

		public enum State {
			Peaking,
			Floating,
			Grounding,
			Lying,
			LieToIdle
		}

		protected struct KnockdownAnimationName {
			private string upper;
			private string fallLoop;
			private string fallToLie;
			private string lieLoop;
			private string lieToIdle;

			public KnockdownAnimationName(string upper, string fallLoop, string fallToLie, string lieLoop, string lieToIdle) {
				this.upper = upper;
				this.fallLoop = fallLoop;
				this.fallToLie = fallToLie;
				this.lieLoop = lieLoop;
				this.lieToIdle = lieToIdle;
			}

			public string Upper {
				get { return upper; }
			}

			public string FallLoop {
				get { return fallLoop; }
			}

			public string FallToLie {
				get { return fallToLie; }
			}

			public string LieLoop {
				get { return lieLoop; }
			}

			public string LieToIdle {
				get { return lieToIdle; }
			}
		}

		private class DefaultVfxGameObjectFactory : Vfxs.Vfx.VfxGameObjectFactory {
			public GameObject Instantiate(GameObject prefab) {
				return GameObject.Instantiate(prefab);
			}
		}
	}
}