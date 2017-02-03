namespace Core.Utils {
	public class AcceptWindow {
		private float start;
		private float end;

		private float originalStart;

		public AcceptWindow(float start, float end) {
			this.start = start;
			this.end = end;
			originalStart = start;
		}

		public virtual bool IsAccept(float time) {
			return start <= time && time <= end;
		}

		public virtual float Start() {
			return start;
		}

		public virtual float End() {
			return end;
		}

		public virtual void StartSoonerBy(float value) {
			start -= value;
		}

		public virtual void ReturnToOriginalValue() {
			start = originalStart;
		}
	}
}