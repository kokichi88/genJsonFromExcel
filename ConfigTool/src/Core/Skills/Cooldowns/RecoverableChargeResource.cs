using Core.Utils;

namespace Core.Skills.Cooldowns {
	public class RecoverableChargeResource : Resource {
		private static string reason = "Not yet cooldown";

		private RecoverableCharge recoverableCharge;

		public RecoverableChargeResource(float duration, int maxCharge) {
			recoverableCharge = new RecoverableCharge(duration, maxCharge);
		}

		public override Name ShowName() {
			return Name.Charge;
		}

		public override bool IsAvailable() {
			return recoverableCharge.IsComplete() || recoverableCharge.IsRecastable();
		}

		public override void Update(float dt) {
			recoverableCharge.Update(dt);
		}

		public override string ShowReasonForUnavailability() {
			return reason;
		}

		protected override void DoConsume() {
			recoverableCharge.Start();
		}

		public int GetCurrentCharge()
		{
			return recoverableCharge.GetCurrentCharge();
		}

		public float RemainingPercentage()
		{
			return recoverableCharge.RemainingPercentage();
		}

		public float Duration()
		{
			return recoverableCharge.Duration();
		}

		public void AdjustDurationWithRatio(float value) {
			recoverableCharge.AdjustDurationWithRatio(value);
		}

		public void ReduceRemainingTimeBy(float value) {
			recoverableCharge.ReduceRemainingTimeBy(value);
		}

		public void SetRecastWindow(AcceptWindow aw) {
			recoverableCharge.SetRecastWindow(aw);
		}

		public void SetMaxRecast(int value) {
			recoverableCharge.SetMaxRecast(value);
		}

		public int GetCurrentRecast() {
			return recoverableCharge.CurrentRecast;
		}

		public void IncreaseMaxChargeBy(int count) {
			recoverableCharge.IncreaseMaxChargeBy(count);
		}

		public void Reset() {
			recoverableCharge.Reset();
		}
	}
}