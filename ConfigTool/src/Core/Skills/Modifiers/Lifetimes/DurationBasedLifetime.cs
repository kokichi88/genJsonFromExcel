using UnityEngine;

namespace Core.Skills.Modifiers.Lifetimes {
	public class DurationBasedLifetime : Lifetime {
		private float duration;
		private bool manuallyControlEnd;
		private float delay;

		private float elapsed;
		private float totalDuration;
		private bool end;
		private float dynamicExtraDuration;

		public DurationBasedLifetime(float duration) {
			this.duration = duration;
			totalDuration = duration;
		}

		public DurationBasedLifetime(float duration, bool manuallyControlEnd) {
			this.duration = duration;
			this.manuallyControlEnd = manuallyControlEnd;
			totalDuration = duration;
		}

		public LifetimeType ShowType() {
			return LifetimeType.Duration;
		}

		public void Update(float dt) {
			if (end) return;

			elapsed += dt;
			Check();
		}

		public void Check() {
			if (!manuallyControlEnd && elapsed >= CalculateDuration()) {
				end = true;
			}
		}

		public bool IsEnd() {
			return end;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
		}

		public void ScaleDurationBy(float factor) {
			duration *= factor;
			totalDuration *= factor;
		}

		public void End() {
			//DLog.Log("DurationBasedLifetime:End()");
			end = true;
		}

		public void SetDelay(float v) {
			delay = v;
			totalDuration += delay;
		}

		public float ShowRemainingDuration() {
			return CalculateDuration() - elapsed;
		}

		public float DynamicExtraDuration {
			get => dynamicExtraDuration;
			set => dynamicExtraDuration = value;
		}

		public void ResetElapsedTime() {
			elapsed = 0;
		}

		private float CalculateDuration() {
			return totalDuration + dynamicExtraDuration;
		}
	}
}