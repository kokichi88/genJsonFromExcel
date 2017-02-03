using Combat.Stats;

namespace Core.Utils {
	public class ComboAttackAcceptWindow : AcceptWindow {
		private readonly Stats attackSpeed;

		private float startOffset;

		public ComboAttackAcceptWindow(float start, float end, Stats attackSpeed) : base(start, end) {
			this.attackSpeed = attackSpeed;
		}

		public override bool IsAccept(float time) {
			return CalculateBakedStart() <= time && time <= CalculateBakedEnd();
		}

		public override float Start() {
			return CalculateBakedStart();
		}

		public override float End() {
			return CalculateBakedEnd();
		}

		public override void StartSoonerBy(float value) {
			startOffset = value;
		}

		public override void ReturnToOriginalValue() {
			startOffset = 0;
		}

		private float CalculateBakedStart() {
			return base.Start() / attackSpeed.BakedFloatValue - startOffset;
		}

		private float CalculateBakedEnd() {
			return base.End() / attackSpeed.BakedFloatValue;
		}
	}
}