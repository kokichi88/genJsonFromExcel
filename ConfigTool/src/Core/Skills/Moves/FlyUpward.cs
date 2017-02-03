using UnityEngine;

namespace Core.Skills.Moves {
	public class FlyUpward {
		private Character character;
		private float duration;
		private float originalY;

		private float speed;
		private float delayFly;
		private float elapsed;

		public FlyUpward(Character character, float duration, float originalY) {
			this.character = character;
			this.duration = duration;
			this.originalY = originalY;

			speed = (originalY - character.Position().y) / duration;
		}

		public void Update(float dt) {
			if(IsFinish()) return;

			elapsed += dt;
			if(elapsed < delayFly) return;

			Vector2 velocity = new Vector2(0, 1) * speed;
			Vector2 displacement = velocity * dt;
			character.DisplaceBy(displacement);
		}

		public bool IsFinish() {
			return character.Position().y >= originalY;
		}

		public void AdjustDelayFly(float newValue) {
			delayFly = newValue;
		}
	}
}