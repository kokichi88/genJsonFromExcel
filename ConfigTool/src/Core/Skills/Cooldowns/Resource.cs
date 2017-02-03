using System;

namespace Core.Skills.Cooldowns {
	public abstract class Resource {
		public abstract Name ShowName();
		public abstract bool IsAvailable();
		public abstract void Update(float dt);
		public abstract string ShowReasonForUnavailability();
		protected abstract void DoConsume();

		public void Consume() {
			if (!IsAvailable()) {
				throw new Exception("Resource is unavailable. Reason: " + ShowReasonForUnavailability());
			}

			DoConsume();
		}

		public enum Name {
			Aether,
			Cooldown,
			Charge
		}
	}
}