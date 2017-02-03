using Artemis;
using Core.Utils.Extensions;
using EntityComponentSystem;
using MovementSystem.Components;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using UnityEngine;
using FacingDirection = Ssar.Combat.Skills.Events.Actions.FacingDirectionAction.FacingDirection;

namespace Core.Skills.Rotations {
	public class Rotation : Loopable {
		private readonly BaseEvent be;
		private readonly Character caster;
		private readonly Environment environment;

		private RotationAction ra;
		private MovementComponent movementComponent;
		private float elapsed;
		private float duration;
		private EasingFunctions.Functions interpolationMethod;
		private EasingFunctions.EasingFunc interpolator;
		private FacingDirection movingDirection;
		private float previousAngle;
		private RotationAction.RotationMode mode;
		private RotationAction.DeltaRotationMode deltaRotationMode;
		private RotationAction.DestinationRotationMode destinationRotationMode;
		private Quaternion destinationQuaternion;
		private Quaternion deltaQuaternion;
		private Quaternion originalOrientation;
		private bool isFirstUpdate = true;

		public Rotation(BaseEvent be, Character caster, Environment environment) {
			this.be = be;
			this.caster = caster;
			this.environment = environment;
			ra = (RotationAction) be.ShowAction();
			Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
			movementComponent = entity.GetComponent<MovementComponent>();
			TimelineTrigger timelineTrigger = (TimelineTrigger) be.ShowTrigger();
			duration = ra.ShowDurationInSeconds(timelineTrigger.ShowScaleTime());
			interpolationMethod = ra.ShowInterpolation();
			interpolator = EasingFunctions.GetEasingFunction(interpolationMethod);
			movingDirection = ra.ShowMovingDirection();
			mode = ra.mode.ShowRotationMode();
			switch (mode) {
				case RotationAction.RotationMode.Delta:
					deltaRotationMode = (RotationAction.DeltaRotationMode) ra.mode;
					break;
				case RotationAction.RotationMode.Destination:
					destinationRotationMode = (RotationAction.DestinationRotationMode) ra.mode;
					break;
			}
		}

		public void Update(float dt) {
			elapsed += dt;
			float progress = elapsed / duration;
			progress = Mathf.Min(1f, progress);
			switch (mode) {
				case RotationAction.RotationMode.Delta:
					ProcessDeltaMode(progress);
					break;
				case RotationAction.RotationMode.Destination:
					ProcessDestinationMode(progress);
					break;
			}
			if (progress >= 1) {
				SetMovingDirection();
			}

			isFirstUpdate = false;
		}

		private void ProcessDestinationMode(float progress) {
			if (isFirstUpdate) {
				originalOrientation = movementComponent.Orientation;
				Vector3 desired = Vector3.right;
				switch (destinationRotationMode.ShowFacingDirection()) {
					case FacingDirection.Enemy:
						Character enemy = FindEnemy();
						MovementComponent enemyMovementComponent = enemy.GameObject().GetComponent<EntityReference>().Entity
							.GetComponent<MovementComponent>();
						Vector3 clampedEnemyPos = enemyMovementComponent.PositionV3.CloneWithNewY(movementComponent.Position.y);
						desired = clampedEnemyPos - movementComponent.PositionV3;
						break;
					case FacingDirection.Left:
						desired = Vector3.left;
						break;
					case FacingDirection.Right:
						desired = Vector3.right;
						break;
				}
				Vector3 facing = movementComponent.Orientation * Vector3.right;
				deltaQuaternion = Quaternion.FromToRotation(facing, desired);
				destinationQuaternion = originalOrientation * deltaQuaternion;
				float deltaAngle = Vector3.Angle(facing, desired);
				float dotProduct = Quaternion.Dot(originalOrientation, destinationQuaternion);
				if (dotProduct < 0) {//longer rotation path detected, switch to shorter path
					originalOrientation = originalOrientation.ScalarMultiply(-1);
					deltaQuaternion = deltaQuaternion.ScalarMultiply(-1);
				}
				if (deltaAngle > destinationRotationMode.maxAngle) {
					deltaAngle = destinationRotationMode.maxAngle;
					Vector3 axis = Vector3.up;
					float outAngle = 0;
					deltaQuaternion.ToAngleAxis(out outAngle, out axis);
					deltaQuaternion = Quaternion.AngleAxis(deltaAngle, axis);
					destinationQuaternion = originalOrientation * deltaQuaternion;
				}
			}
			movementComponent.SetOrientation(
				QuaternionExtension.Interpolate(
					originalOrientation, destinationQuaternion, interpolator, progress, true
				)
			);

			if (progress >= 1) {
				movementComponent.SetOrientation(destinationQuaternion);
			}
		}

		private void ProcessDeltaMode(float progress) {
			float angle = interpolator(0, deltaRotationMode.angle, progress);
			float deltaAngle = angle - previousAngle;
			Vector3 axis = deltaRotationMode.axis;
			if (deltaRotationMode.flip && caster.FacingDirection() == Direction.Left) {
				axis *= -1;
			}
			movementComponent.SetOrientation(
				movementComponent.Orientation * Quaternion.AngleAxis(deltaAngle, axis)
			);
			previousAngle = angle;
		}

		private void SetMovingDirection() {
			Direction dir = Direction.Right;
			switch (movingDirection) {
				case FacingDirection.Left:
					dir = Direction.Left;
					break;
				case FacingDirection.Right:
					dir = Direction.Right;
					break;
				case FacingDirection.Enemy:
					Character enemy = FindEnemy();
					Vector3 diff = enemy.Position() - caster.Position();
					if (diff.x >= 0) {
						dir = Direction.Right;
					}
					else {
						dir = Direction.Left;
					}

					break;
			}

			movementComponent.MovingDirection = dir.ToNormalizedVector2();
		}

		private Character FindEnemy() {
			Character enemy = environment.FindNearbyCharacters(
				caster, Vector3.zero, 999, new[] {
					FindingFilter.ExcludeMe, FindingFilter.ExcludeDead, FindingFilter.ExcludeAllies
				}
			)[0];
			return enemy;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return elapsed >= duration;
		}
	}
}