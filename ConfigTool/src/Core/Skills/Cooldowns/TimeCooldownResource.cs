using Core.Utils;

namespace Core.Skills.Cooldowns {
	public class TimeCooldownResource : Resource {
		private static string reason = "Not yet cooldown";

		private TimeCooldown timeCooldown;

		public TimeCooldownResource(float duration) {
			timeCooldown = new TimeCooldown(duration);
		}

		public TimeCooldownResource(float duration, float scale = 1) {
			timeCooldown = new TimeCooldown(duration, scale);
		}

		public override Name ShowName() {
			return Name.Cooldown;
		}

		public override bool IsAvailable() {
			return timeCooldown.IsComplete() || timeCooldown.IsRecastable();
		}

		public override void Update(float dt) {
			timeCooldown.Update(dt);
		}

		public override string ShowReasonForUnavailability() {
			return reason;
		}

		protected override void DoConsume() {
			timeCooldown.Start();
		}

		public void Reset() {
			timeCooldown.Reset();
		}

		public float RemainingPercentage()
		{
			return timeCooldown.RemainingPercentage();
		}

		public float Remaining()
		{
			return timeCooldown.Remaining();
		}

		public float Duration()
		{
			return timeCooldown.Duration();
		}

		public void AdjustDurationWithRatio(float value) {
			timeCooldown.AdjustDurationWithRatio(value);
		}

		public void ReduceRemainingTimeBy(float value) {
			timeCooldown.ReduceRemainingTimeBy(value);
		}

		public void SetRecastWindow(AcceptWindow aw) {
			timeCooldown.SetRecastWindow(aw);
		}

		public void SetMaxRecast(int value) {
			timeCooldown.SetMaxRecast(value);
		}

		public int GetCurrentRecast() {
			return timeCooldown.CurrentRecast;
		}
	}
}