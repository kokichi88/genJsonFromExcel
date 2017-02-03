using Core.Utils;

namespace Core.Skills.Cooldowns {
	public class NonRecoverableChargeResource : Resource {
		private static string reason = "Not yet cooldown";

		private NonRecoverableCharge nonRecoverableCharge;

		public NonRecoverableChargeResource(int maxCharge) {
			nonRecoverableCharge = new NonRecoverableCharge(maxCharge);
		}

		public override Name ShowName() {
			return Name.Charge;
		}

		public override bool IsAvailable() {
			return nonRecoverableCharge.IsComplete() || nonRecoverableCharge.IsRecastable();
		}

		public override void Update(float dt) {
			nonRecoverableCharge.Update(dt);
		}

		public override string ShowReasonForUnavailability() {
			return reason;
		}

		protected override void DoConsume() {
			nonRecoverableCharge.Start();
		}

		public int GetCurrentCharge()
		{
			return nonRecoverableCharge.GetCurrentCharge();
		}

		public void SetRecastWindow(AcceptWindow aw) {
			nonRecoverableCharge.SetRecastWindow(aw);
		}

		public void SetMaxRecast(int value) {
			nonRecoverableCharge.SetMaxRecast(value);
		}

		public int GetCurrentRecast() {
			return nonRecoverableCharge.CurrentRecast;
		}
	}
}