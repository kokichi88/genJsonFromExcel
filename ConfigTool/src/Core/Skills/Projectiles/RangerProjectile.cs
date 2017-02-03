using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Core.Skills.Projectiles {
	public class RangerProjectile : Projectile{
		public RangerProjectile(Character character, Skill skill, Collision collision, SsarCollider collider) : base(skill, collision, collider, 1) {
		}

		protected internal override SsarCollider GetCollider() {
			throw new NotImplementedException();
		}

		protected internal override List<Character> PickInterestedOnesFrom(List<Character> collidedCharacters) {
			throw new NotImplementedException();
		}

		protected internal override void UpdateTrajectory(float dt) {
			throw new NotImplementedException();
		}

		protected internal override bool IsFinish() {
			throw new NotImplementedException();
		}

		protected override void OnDestroy() {
			throw new NotImplementedException();
		}

		public override Vector3 Position() {
			throw new NotImplementedException();
		}

		public override Vector3 Velocity() {
			throw new NotImplementedException();
		}

		public override void SetVelocity(Vector3 velocity) {
			throw new NotImplementedException();
		}

		public override void SetPosition(Vector3 newPosition) {
			throw new NotImplementedException();
		}

		public override void SetTrajectory(Trajectory newTrajectory) {
			throw new NotImplementedException();
		}
	}
}
