using Combat.Stats;

namespace Core.Utils {
	public class ComboAttackStateWindow : AttackStateWindow {
		private ComboAttackAcceptWindow caaw;

		public ComboAttackStateWindow(float start, float end, Stats attackSpeed) : base(start, end) {
			caaw = new ComboAttackAcceptWindow(start, end, attackSpeed);
		}

		public override bool IsTransitionAvailable(float time) {
			return caaw.IsAccept(time);
		}

		public override void StartSoonerBy(float value) {
			caaw.StartSoonerBy(value);
		}

		public override void ReturnToOriginalValue() {
			caaw.ReturnToOriginalValue();
		}

		public override float Start() {
			return caaw.Start();
		}
	}
}