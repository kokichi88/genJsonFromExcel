using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Core.Utils.Extensions;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils;
using Animation = Ssar.Combat.HeroStateMachines.Animation;
using Behavior = Core.Skills.Modifiers.Info.StaggerInfo.Behavior;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.MovementBehavior;
using FacingBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.FacingBehavior;

namespace Core.Skills.Modifiers {
	public class StaggerModifier : BaseModifier {
		private readonly StaggerInfo info;
		private Vector3 collidedProjectilePosition;

		private readonly MovementComponent targetMovementComponent;
		private float elapsed;
		private float playLoopAnimationAt;
		private float playIdleAnimationAt;
		private bool isLoopAnimationPlayed;
		private bool isIdleAnimationPlayed;
		private AnimationComponent targetAnimationComponent;
		private MovementComponent casterMovementComponent;
		protected DurationBasedLifetime lifetime;

		public StaggerModifier(StaggerInfo info, Entity casterEntity, Entity targetEntity,
		                       Vector3 collidedProjectilePosition, Environment environment,
		                       CollectionOfInteractions modifierInteractionCollection)
			: base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = info;
			this.collidedProjectilePosition = collidedProjectilePosition;

			float duration = info.ShowDuration();

			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			playLoopAnimationAt = Mathf.Max(0, info.MovementDuration - info.CrossfadeLength);
			playIdleAnimationAt = Mathf.Max(0, duration - info.CrossfadeLength);
			targetAnimationComponent = targetEntity.GetComponent<AnimationComponent>();
			casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
		}

		public override string Name() {
			return string.Format("{0}({1})", Type(), attachType);
		}

		public override ModifierType Type() {
			return ModifierType.Stagger;
		}

		protected override void OnUpdate(float dt) {
			if (IsFinish()) return;

			elapsed += dt;

			if (elapsed >= playLoopAnimationAt && !isLoopAnimationPlayed) {
				isLoopAnimationPlayed = true;
				if (ShouldPlayAnimation() && !string.IsNullOrEmpty(info.LoopAnimation)) {
					targetAnimationComponent.Animation.PlayAnimation(
						info.LoopAnimation, 1, PlayMethod.Crossfade, info.CrossfadeLength
					);
				}
			}

			if (elapsed >= playIdleAnimationAt && !isIdleAnimationPlayed) {
				isIdleAnimationPlayed = true;
				// List<string> currentAnimations = new List<string>();
				// try {
				// 	currentAnimations = targetAnimationComponent.Animation.CurrentAnimationNames();
				// }
				// catch (Exception e) {
				// }

				// if (currentAnimations.Contains(AnimationName.Stagger.SOFT))
				// 	targetAnimationComponent.Animation.PlayAnimation(
				// 		AnimationName.IDLE, 1, PlayMethod.Crossfade, crossfadeLength
				// 	);
			}

			PlayVfx();
		}

		private void PlayVfx() {
		}

		private void ReturnVfxToPool() {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			Log("OnAttach", target);
			Character casterCharacter = casterEntity.GetComponent<SkillComponent>().Character;

			if (attachType == ModifierAttachType.Main) {
				Direction movementDirection = CalculateMovementDirection(
					casterMovementComponent, targetMovementComponent,
					collidedProjectilePosition, info.MovementBehavior
				);
				Direction facingDirection = CalculateFacingDirection(
					casterMovementComponent, targetMovementComponent,
					collidedProjectilePosition, info.FacingBehavior, movementDirection
				);
				targetMovementComponent.MovingDirection = movementDirection.ToNormalizedVector2();
				targetMovementComponent.FacingDirection = facingDirection;

				float gravity = targetMovementComponent.Gravity;
				for (int kIndex = 0; kIndex < targetMovementComponent.MovementRequests.Count; kIndex++) {
					Request r = targetMovementComponent.MovementRequests[kIndex];
					RequestType requestType = r.ShowRequestType();
					if (requestType == RequestType.StationaryJump
					    || requestType == RequestType.MovingJump) {
						gravity = ((StationaryJumpRequest) r).GroundingGravity;
					}
				}
				FallRequest fallRequest = new FallRequest(gravity, targetMovementComponent.Velocity.magnitude);
				if (info.Distance != 0) {
					DashRequest dashRequest = new DashRequest(info.Distance, info.MovementDuration, 0);
					dashRequest.AddCorunningRequest(fallRequest);
					targetMovementComponent.AddMovementRequest(dashRequest);
				}
				if (targetMovementComponent.ConfigData.isFallable) {
					targetMovementComponent.AddMovementRequest(fallRequest);
				}

				if (ShouldInterruptTargetSkill()) {
					target.InterruptChannelingSkill();
				}
				string animationName = AnimationName.Stagger.SOFT;
				if (info.Behaviors.Contains(Behavior.InterruptTargetSkill)) {
					animationName = AnimationName.Stagger.HARD;
				}
				if (!string.IsNullOrEmpty(info.OverrideAnimation)) {
					animationName = info.OverrideAnimation;

					string[] split = info.OverrideAnimation.Split(',');
					if (split.Length > 0) {
						animationName = split[BattleUtils.RandomRangeInt(0, split.Length)].Trim();
					}
				}

				if (ShouldPlayAnimation()) {
					targetAnimationComponent.Animation.PlayAnimation(animationName, 1, PlayMethod.Play, 0);
					targetAnimationComponent.Animation.JumpToFrame(info.AnimFrame);
				}
			}
		}

		public static Direction CalculateMovementDirection(MovementComponent casterMovementComponent,
		                                                   MovementComponent targetMovementComponent,
		                                                   Vector3 collidedProjectilePosition,
		                                                   MovementBehavior movementBehavior) {
			Vector3 casterPosition = casterMovementComponent.Position;
			Vector3 targetPosition = targetMovementComponent.Position;
			Vector2 directionFromTargetToCaster = casterPosition - targetPosition;
			Vector2 directionFromTargetToProjectile = collidedProjectilePosition - targetPosition;
			Direction direction = Direction.Right;
			switch (movementBehavior) {
				case MovementBehavior.TowardCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirectionEnum();
					break;
				case MovementBehavior.AwayFromCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirectionEnum().Opposite();
					break;
				case MovementBehavior.TowardProjectile:
					direction = directionFromTargetToProjectile.ToLeftOrRightDirectionEnum();
					break;
				case MovementBehavior.AwayFromProjectile:
					direction = directionFromTargetToProjectile.ToLeftOrRightDirectionEnum().Opposite();
					break;
				case MovementBehavior.FollowCasterFacing:
					direction = casterMovementComponent.FacingDirection;
					break;
				case MovementBehavior.OppositeCasterFacing:
					direction = casterMovementComponent.FacingDirection.Opposite();
					break;
				default:
					throw new Exception("Missing logic to calculate movement direction of " + movementBehavior);
			}

			return direction;
		}

		public static Direction CalculateFacingDirection(MovementComponent casterMovementComponent,
		                                                 MovementComponent targetMovementComponent,
		                                                 Vector3 collidedProjectilePosition,
		                                                 FacingBehavior facingBehavior,
		                                                 Direction movementDirection) {
			Vector3 casterPosition = casterMovementComponent.Position;
			Vector3 targetPosition = targetMovementComponent.Position;
			Vector2 directionFromTargetToCaster = casterPosition - targetPosition;
			Vector2 directionFromTargetToProjectile = collidedProjectilePosition - targetPosition;
			Direction direction = Direction.Right;
			switch (facingBehavior) {
				case FacingBehavior.TowardCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirectionEnum();
					break;
				/*case FacingBehavior.AwayFromCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirectionEnum().Opposite();
					break;*/
				case FacingBehavior.TowardProjectile:
					direction = directionFromTargetToProjectile.ToLeftOrRightDirectionEnum();
					break;
				/*case FacingBehavior.AwayFromProjectile:
					direction = directionFromTargetToProjectile.ToLeftOrRightDirectionEnum().Opposite();
					break;*/
				case FacingBehavior.FollowCasterFacing:
					direction = casterMovementComponent.FacingDirection;
					break;
				case FacingBehavior.OppositeCasterFacing:
					direction = casterMovementComponent.FacingDirection.Opposite();
					break;
				/*case FacingBehavior.FollowMovement:
					direction = movementDirection;
					break;
				case FacingBehavior.OppositeMovement:
					direction = movementDirection.Opposite();
					break;*/
				case FacingBehavior.Current:
					direction = targetMovementComponent.FacingDirection;
					break;
				default:
					throw new Exception("Missing logic to calculate facing direction of " + facingBehavior);
			}

			return direction;
		}

		public override object[] Cookies() {
			return new object[0];
		}

		private void Log(string methodName, Character character) {
			//  DLog.Log(GetType().Name + " " + methodName + ": character: group: " + character.Group() + " id: " + character.Id());
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			foreach (Lifetime l in lifetimes) {
				if (l is DurationBasedLifetime) {
					lifetime = (DurationBasedLifetime) l;
				}
			}
			return lifetimes;
		}

		protected virtual bool ShouldInterruptTargetSkill() {
			return true;
		}

		protected virtual bool ShouldPlayAnimation() {
			return true;
		}
	}
}