using MovementSystem.Components;
using MovementSystem.Requests;
using UnityEngine;

namespace Core.Skills.Dashes {
	public class FixedUpdateDashRequest : DashRequest {
		public FixedUpdateDashRequest(float dashDistance, float dashDuration, float blendTime, bool constantSpeed = false) : base(dashDistance, dashDuration, blendTime, constantSpeed) {
		}

		public override Vector2 Displacement(MovementComponent movementComponent, float dt) {
			return Vector2.zero;
		}

		public Vector2 Displacement_(MovementComponent movementComponent, float dt) {
			return base.Displacement(movementComponent, dt);
		}
	}
}