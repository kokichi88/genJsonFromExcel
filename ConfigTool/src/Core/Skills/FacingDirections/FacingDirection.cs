using System.Collections.Generic;
using Artemis;
using EntityComponentSystem;
using MovementSystem.Components;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.FacingDirections {
	public class FacingDirection : Loopable {
		private BaseEvent ef;
		private Character caster;
		private Environment environment;

		private float duration;
		private float interval;
		private FacingDirectionAction.FacingDirection facingDirection;
		private float elapsed;
		private bool isInterrupted;
		private MovementComponent movementComponent;

		public FacingDirection(BaseEvent ef, Character caster, Environment environment) {
			this.ef = ef;
			this.caster = caster;
			this.environment = environment;

			FacingDirectionAction fda = (FacingDirectionAction) ef.action;
			duration = fda.duration;
			interval = fda.interval;
			Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
			movementComponent = entity.GetComponent<MovementComponent>();
			facingDirection = fda.ShowDirection();
			FaceTowardDesiredDirection();
		}

		private void FaceTowardDesiredDirection() {
			switch (facingDirection) {
				case FacingDirectionAction.FacingDirection.Left:
					movementComponent.FacingDirection = Direction.Left;
					break;
				case FacingDirectionAction.FacingDirection.Right:
					movementComponent.FacingDirection = Direction.Right;
					break;
				case FacingDirectionAction.FacingDirection.Enemy:
					List<Character> enemies = environment.FindNearbyCharacters(
						caster, Vector3.zero, 999,
						new[] {
							FindingFilter.ExcludeAllies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe
						}
					);
					if (enemies.Count > 0) {
						Vector2 enemyPosition = enemies[0].Position();
						Vector2 diff = enemyPosition - (Vector2) caster.Position();
						if (diff.x >= 0) {
							movementComponent.FacingDirection = Direction.Right;
						}
						else {
							movementComponent.FacingDirection = Direction.Left;
						}
					}
					break;
				case FacingDirectionAction.FacingDirection.Opposite:
					movementComponent.FacingDirection = movementComponent.FacingDirection.Opposite();
					break;
			}
		}

		public void Update(float dt) {
			elapsed += dt;

			if (elapsed >= interval) {
				elapsed = 0;
				FaceTowardDesiredDirection();
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return elapsed >= duration || isInterrupted;
		}
	}
}