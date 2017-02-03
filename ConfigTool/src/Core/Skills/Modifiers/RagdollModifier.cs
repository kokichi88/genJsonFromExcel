using System;
using System.Collections.Generic;
using Artemis;
using Com.LuisPedroFonseca.ProCamera2D;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Skills.Projectiles.Entity.Components;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Core.Utils.Extensions;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MEC;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using Ssar.Combat.Skills.Projectiles.Entity.Components;
using UnityEngine;
using Utils;
using Utils.Gizmos;
using Handles = Core.Utils.Handles;
using Event = Ssar.Combat.Skills.Events.Actions.JumpAction.Event;
using AnimProfile = Core.Skills.Modifiers.BlastModifier.AnimProfile;
using FarAnimProfile = Core.Skills.Modifiers.BlastModifier.FarAnimProfile;
using HighAnimProfile = Core.Skills.Modifiers.BlastModifier.HighAnimProfile;
using MediumAnimProfile = Core.Skills.Modifiers.BlastModifier.MediumAnimProfile;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class RagdollModifier : BaseModifier {
		private readonly Entity casterEntity;
		private readonly Entity targetEntity;
		private readonly Camera camera;
		private readonly SkillId skillId;
		private readonly Environment environment;
		private readonly WallHitConfig whc;
		private readonly float damageScale;
		private readonly ProjectileComponent projectile;
		private RagdollInfo info;
		private float duration;
		private float timeUntilGround;
		private float timeUntilFall;

		private float elapsed;
		private Transform targetChest;
		private Transform casterWristTransform;
		private GameObject targetGo;
		private EasingFunctions.EasingFunc easingFunc;
		private bool isPeaking = true;
		private float previousTraveledDistance;
		private bool isPaused;
		private AnimationComponent targetAnimation;
		private bool isRotated;
		private Quaternion targetQuaternion;
		private bool isRotationPivotChanged;
		private float timeUntilReturnToIdle;
		private bool isReturnToIdleStarted;
		private DrawRequest dr;
		private bool isOnGroundAnimationPlayed;
		private Transform targetRenderer;
		private List<Event> processedEvents = new List<Event>();
		private Vfxs.Vfx.Logic vfxLogic;
		private CoroutineHandle ch;
		private FallRequest fallRequest;
		private bool isEventTriggered;
		private MovementComponent targetMovementComponent;
		private bool isWallHit;
		private bool shouldGraduallyTranslateToDragTarget;
		private UnpredictableDurationLifetime lifetime;
		private AnimProfile animProfile;
		private Character targetCharacter;
		private bool isGrounded;
		private BakedStatsContainer characterStats;
		private MainModifierState state = MainModifierState.Grab;
		private bool isDynamicBonesPreparedForAttachment;
		private bool justChanged;

		public RagdollModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                       Camera camera, SkillId skillId, Environment environment,
		                       CollectionOfInteractions modifierInteractionCollection,
		                       WallHitConfig wallHitConfig, float damageScale,
		                       ProjectileComponent projectile) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;
			this.camera = camera;
			this.skillId = skillId;
			this.environment = environment;
			this.whc = wallHitConfig;
			this.damageScale = damageScale;
			this.projectile = projectile;
			this.info = (RagdollInfo) info;

			easingFunc = EasingFunctions.GetEasingFunction(EasingFunctions.Functions.EaseOutQuad);
			CalculateDurations(this.info);
			switch (this.info.RagdollModifierConfig.ShowAnimationProfile()) {
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
			targetCharacter = targetEntity.GetComponent<SkillComponent>().Character;
			StatsComponent casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			characterStats = casterStatsComponent.CharacterStats;
			//DLog.Log("RagdollModifier: state: " + state);
		}

		private void CalculateDurations(RagdollInfo ri) {
			RagdollModifierConfig rmc = ri.RagdollModifierConfig;
			timeUntilFall = rmc.timeToGrab - ri.ProjectileAge;
			timeUntilGround = timeUntilFall + rmc.timeToFall;
			timeUntilReturnToIdle = timeUntilGround + rmc.timeToLie;
			duration = timeUntilReturnToIdle + rmc.lieToIdleDuration;
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.Ragdoll;
		}

		protected override void OnUpdate(float dt) {
			justChanged = false;
			if (IsFinish()) return;

			elapsed += dt;

			if (elapsed >= timeUntilReturnToIdle && !isReturnToIdleStarted) {
				isReturnToIdleStarted = true;
				if (targetEntity.GetComponent<HealthComponent>().IsAlive()) {
					targetAnimation.Animation.PlayAnimation(
						animProfile.LieToIdle(), 1, PlayMethod.Crossfade, .1f
					);
					targetAnimation.Animation.PlayAnimation(
						AnimationName.IDLE, 1, PlayMethod.Queue, 0
					);
				}
				//DLog.Log("Return to idle");
			}

			if (elapsed >= timeUntilFall) {
				if (!isOnGroundAnimationPlayed) {
					justChanged = true;
					state = MainModifierState.Air;
					//DLog.Log("RagdollModifier: state: " + state);
					isOnGroundAnimationPlayed = true;
					JointHierarchyAdjustment jha = targetGo.GetComponentInChildren<JointHierarchyAdjustment>();
					if (jha != null) {
						jha.UndoAdjustment();
					}
					targetAnimation.Animation.UnpauseAnimation();
					targetAnimation.Animation.PlayAnimation(animProfile.FallLoop());
					SetEnableDynamicBones(false);

					MovementComponent targetMc = targetEntity.GetComponent<MovementComponent>();
					targetMc.UpdateGround();
					float s = targetMc.Position.y;
					float t = info.RagdollModifierConfig.timeToFall;
					float v0 = info.RagdollModifierConfig.initSpeedOfFall;
					float gravity = 2 * (s - v0 * t) / (t * t);
					fallRequest = new FallRequest(gravity);
					targetMc.AbortAllRequests();
					targetMc.AddMovementRequest(fallRequest);
					//DLog.Log("Fall");
				}
			}

			if (elapsed >= timeUntilGround && !isGrounded) {
				justChanged = true;
				state = MainModifierState.Ground;
				//DLog.Log("RagdollModifier: state: " + state);
				isGrounded = true;
				targetAnimation.Animation.PlayAnimation(animProfile.FallToLie());
				targetAnimation.Animation.JumpToFrame(info.RagdollModifierConfig.onGroundAnimationStartFrame);
				targetAnimation.Animation.QueueAnimation(animProfile.LieLoop());
				//DLog.Log("Lie");
			}

			if (fallRequest != null) {
				if (fallRequest.IsCompleted()) {
					if (!isEventTriggered) {
						isEventTriggered = true;
						TriggerEvents();
					}
				}
			}

			if (vfxLogic != null) {
				vfxLogic.Update(dt);
			}

			if (elapsed >= duration) {
				lifetime.End();
			}
		}

		protected override void OnLateUpdate(float dt) {
			if (!isDynamicBonesPreparedForAttachment) {
				isDynamicBonesPreparedForAttachment = true;
				JointHierarchyAdjustment jha = targetGo.GetComponentInChildren<JointHierarchyAdjustment>();
				if (jha != null) {
					jha.MakeAdjustment();
				}
				SetEnableDynamicBones(true);
			}
			base.OnLateUpdate(dt);
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetMovementComponent.WallCollisionHandler -= OnWallCollision;
			targetAnimation.Animation.UnpauseAnimation();
			Timing.KillCoroutines(ch);
			JointHierarchyAdjustment jha = targetGo.GetComponentInChildren<JointHierarchyAdjustment>();
			if (jha != null) {
				jha.UndoAdjustment();
			}
			SetEnableDynamicBones(false);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetMovementComponent.WallCollisionHandler -= OnWallCollision;
			targetAnimation.Animation.UnpauseAnimation();
			Timing.KillCoroutines(ch);
			JointHierarchyAdjustment jha = targetGo.GetComponentInChildren<JointHierarchyAdjustment>();
			if (jha != null) {
				jha.UndoAdjustment();
			}
			SetEnableDynamicBones(false);
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			target.InterruptChannelingSkill();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			targetMovementComponent.WallCollisionHandler += OnWallCollision;

			if (info.RagdollModifierConfig.forceFacingTowardCaster) {
				MovementComponent casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
				Direction facingDirection = ((Vector2) (casterMovementComponent.Position - targetMovementComponent.Position))
					.ToLeftOrRightDirectionEnum();
				targetMovementComponent.FacingDirection = facingDirection;
			}

			targetGo = target.GameObject();
#if RAGDOLL_MODIFIER
			WorldPositionDisplay wpd = targetGo.GetComponent<WorldPositionDisplay>();
			if (wpd == null) {
				wpd = targetGo.AddComponent<WorldPositionDisplay>();
			}
			wpd.enableHandle = true;
#endif
			targetQuaternion = targetGo.transform.rotation;

			targetAnimation = targetEntity.GetComponent<AnimationComponent>();
			targetAnimation.Animation.Stop();
			targetAnimation.Animation.PlayAnimation(info.RagdollModifierConfig.posingAnimation, 1, PlayMethod.Play, 0);
			// targetAnimation.Animation.JumpToFrame(info.RagdollModifierConfig.posingFrame);
			// targetAnimation.Animation.PauseAnimation();
			// isPaused = true;

			targetChest = targetGo.transform.FindDeepChild(info.RagdollModifierConfig.childJointName);
			if (targetChest == null) {
				DLog.LogError(string.Format(
					"Cannot find joint of name '{0}'", info.RagdollModifierConfig.childJointName
				));
			}
#if RAGDOLL_MODIFIER
						wpd = targetChest.gameObject.GetComponent<WorldPositionDisplay>();
			if (wpd == null) {
				wpd = targetChest.gameObject.AddComponent<WorldPositionDisplay>();
			}
			wpd.enableHandle = true;
#endif

			if (info.RagdollModifierConfig.projectile) {
				casterWristTransform = projectile.Entity.GetComponent<EntityGameObjectComponent>()
					.GameObject.transform;
			}
			else {
				casterWristTransform = casterEntity.GetComponent<EntityGameObjectComponent>()
					.GameObject.transform.FindDeepChild(info.RagdollModifierConfig.parentJointName);
			}

#if RAGDOLL_MODIFIER
						wpd = casterWristTransform.gameObject.GetComponent<WorldPositionDisplay>();
			if (wpd == null) {
				wpd = casterWristTransform.gameObject.AddComponent<WorldPositionDisplay>();
			}
			wpd.enableHandle = true;
			wpd.color = Color.red;
#endif

			targetRenderer = targetGo.transform.GetChild(0);
#if RAGDOLL_MODIFIER
			targetRenderer.gameObject.AddComponent<WorldPositionDisplay>().enableHandle = true;
#endif

			shouldGraduallyTranslateToDragTarget = true;
			ch = Timing.RunCoroutine(_LateUpdate(), Segment.LateUpdate);
			//DLog.Log("Start to drag");
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			RagdollInfo ri = (RagdollInfo) modifierInfo;
			lifetime = new UnpredictableDurationLifetime();
			return new List<Lifetime>(new[] {lifetime});
		}

		private void OnWallCollision(object sender, WallCollisionEventArgs e) {
			if (!info.RagdollModifierConfig.enableWallHit) return;

			if (elapsed < timeUntilGround && !isWallHit) {
				isWallHit = true;
				lifetime.End();
				bool found;
				Stats casterAtkStats = casterEntity.GetComponent<StatsComponent>().CharacterStats.FindStats(StatsType.RawAtk, out found);
				DamageFromAttack dfa = new DamageFromAttack(
					new SourceHistory(Source.FromSkill(info.ShowParentSkill(), skillId)),
					whc.damageScale * damageScale, false, 1, 1, casterEntity.Id, targetCharacter.Position(),
					targetCharacter.Position(),
					characterStats
				);
				for (int kIndex = 0; kIndex < whc.modifiers.Count; kIndex++) {
					dfa.AddModifierInfo(
						CastProjectileAction.OnHitPhase.Damaged,
						((DefaultSkillCharacter) targetCharacter).CreateModifierInfo(
							info.ShowParentSkill(),  whc.modifiers[kIndex]
						)
					);
				}
				targetEntity.GetComponent<HealthComponent>().ReceiveDamage(dfa);
			}
		}

		private IEnumerator<float> _LateUpdate() {
			MovementComponent casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
			Direction casterFacingDirection = casterMovementComponent.FacingDirection;
			Vector2 flippedOffset = info.RagdollModifierConfig.grabOffset.FlipFollowDirection(casterFacingDirection);
			Vector3 originalPos = targetRenderer.localPosition;
			targetRenderer.localPosition = Vector3.zero;
			Vector2 dragTarget = (Vector2) casterWristTransform.position + flippedOffset;
			float initialRadius = (dragTarget - (Vector2) targetChest.position).magnitude;
			float radiusReductionElapsed = 0;
			targetRenderer.localPosition = originalPos;
			Vector2 previousPosOfHierarchyRootOfTargetTransform = targetGo.transform.position;
			while (true) {
				if (lifetime.IsEnd()) break;
				yield return Timing.WaitForOneFrame;
				originalPos = targetRenderer.localPosition;
				targetRenderer.localPosition = Vector3.zero;
				if (elapsed < timeUntilFall) {
					casterFacingDirection = casterMovementComponent.FacingDirection;
					flippedOffset = info.RagdollModifierConfig.grabOffset.FlipFollowDirection(casterFacingDirection);
					dragTarget = (Vector2) casterWristTransform.position + flippedOffset;
					if (shouldGraduallyTranslateToDragTarget) {
						radiusReductionElapsed += Time.deltaTime;
						float progress = radiusReductionElapsed / info.RagdollModifierConfig.timeToReachDragTargetIfBehind;
						progress = Math.Min(1, progress);
						float radius = initialRadius * (1 - progress);
						Vector2 currentPosOfHierarchyRootOfTargetTransform = targetGo.transform.position;
						Vector2 diff = currentPosOfHierarchyRootOfTargetTransform -
						               previousPosOfHierarchyRootOfTargetTransform;
						previousPosOfHierarchyRootOfTargetTransform = currentPosOfHierarchyRootOfTargetTransform;
						if (info.RagdollModifierConfig.rootCheck) {
							if (diff == Vector2.zero) continue;
						}
						Vector2 direction = ((Vector2) targetChest.position - dragTarget).normalized;
						Vector2 currentDragPos = dragTarget + direction * radius;
						Vector2 translation = currentDragPos - (Vector2) targetChest.position;
						targetMovementComponent.Move(translation);
					}
					else {
						Vector2 offset = dragTarget - (Vector2) targetChest.position;
						targetMovementComponent.Move(offset);
					}
				}
				targetRenderer.localPosition = originalPos;
			}
		}

		private IEnumerator<float> _WaitThenInvoke(float waitTime, Action action) {
			yield return Timing.WaitForSeconds(waitTime);
			action();
		}

		private void SetEnableDynamicBones(bool enabled) {
			DynamicBone[] targetDynamicBones = targetGo.GetComponentsInChildren<DynamicBone>();
			for (int i = 0; i < targetDynamicBones.Length; i++) {
				targetDynamicBones[i].enabled = enabled;
			}
		}

		private void TriggerEvents() {
			List<Event> events = info.RagdollModifierConfig.ListAllEnabledEvents();
			for (int kIndex = 0; kIndex < events.Count; kIndex++) {
				Event e = events[kIndex];
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
									new DefaultVfxGameObjectFactory(),
									targetEntity.GetComponent<SkillComponent>().Character,
									SkillCastingSource.FromUserInput(),
									environment.GetCamera(), environment, null
								);
								break;
						}
						break;
				}
			}
		}

		private class DefaultVfxGameObjectFactory : Vfxs.Vfx.VfxGameObjectFactory {
			public GameObject Instantiate(GameObject prefab) {
				return GameObject.Instantiate(prefab);
			}
		}

		public override bool TryQueryingState(out MainModifierState value, out bool justChanged) {
			value = state;
			justChanged = this.justChanged;
			return true;
		}

		public MainModifierState State => state;
	}
}