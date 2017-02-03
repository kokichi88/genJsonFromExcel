using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Core.Utils.Extensions;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.WindModifierConfig.MovementBehavior;

namespace Core.Skills.Modifiers {
	public class WindModifier : BaseModifier {
		private WindInfo info;

		private MovementComponent targetMovementComponent;
		private MovementComponent casterMovementComponent;
		private DirectionMoveRequest directionMoveRequest;
		private MovementBehavior movementBehavior;
		private float duration;

		public WindModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment,
		                    CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (WindInfo) info;

			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
			duration = this.info.Wmc.ShowDurationInSeconds();
		}

		public override ModifierType Type() {
			return ModifierType.Wind;
		}

		protected override void OnUpdate(float dt) {
			if (movementBehavior == MovementBehavior.BlackHoleTowardCaster) {
				Vector2 direction = Vector2.right;
				Vector3 casterPosition = casterMovementComponent.Position;
				Vector3 offsetPosition = casterPosition +
				                         info.Wmc.offset.FlipFollowDirection(casterMovementComponent.FacingDirection);
				Vector3 targetPosition = targetMovementComponent.Position;
				Vector2 directionFromTargetToCaster = offsetPosition - targetPosition;
				direction = directionFromTargetToCaster.ToLeftOrRightDirection();
				directionMoveRequest.Direction = direction;
			}
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			movementBehavior = info.Wmc.ShowMovementBehavior();
			Vector2 direction = Vector2.right;
			Vector3 casterPosition = casterMovementComponent.Position;
			Vector3 offset = Vector3.zero;
			if (movementBehavior == MovementBehavior.BlackHoleTowardCaster) {
				offset = info.Wmc.offset.FlipFollowDirection(casterMovementComponent.FacingDirection);
			}
			Vector3 offsetPosition = casterPosition + offset;
			Vector3 targetPosition = targetMovementComponent.Position;
			Vector2 directionFromTargetToCaster = offsetPosition - targetPosition;
			switch (movementBehavior) {
				case MovementBehavior.TowardCaster:
				case MovementBehavior.BlackHoleTowardCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirection();
					break;
				case MovementBehavior.AwayFromCaster:
					direction = directionFromTargetToCaster.ToLeftOrRightDirection() * -1;
					break;
				case MovementBehavior.FollowCasterFacing:
					direction = casterMovementComponent.FacingDirection.ToNormalizedVector2();
					break;
				case MovementBehavior.OppositeCasterFacing:
					direction = casterMovementComponent.FacingDirection.Opposite().ToNormalizedVector2();
					break;
				default:
					throw new Exception("Missing logic to calculate movement direction of " + movementBehavior);
			}

			if (movementBehavior == MovementBehavior.BlackHoleTowardCaster) {
				directionMoveRequest = new DirectionMoveRequest(direction, info.Wmc.speed, duration, info.Wmc.acceleration, offsetPosition);
			}
			else {
				directionMoveRequest = new DirectionMoveRequest(direction, info.Wmc.speed, duration, info.Wmc.acceleration);
			}
			targetMovementComponent.AddMovementRequest(directionMoveRequest);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);

			directionMoveRequest.Abort();
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);

			directionMoveRequest.Abort();
		}
	}
}