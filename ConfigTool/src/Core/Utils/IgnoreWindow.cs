namespace Core.Utils {
	public class IgnoreWindow {
		private float start;
		private float end;

		public IgnoreWindow(float start, float end) {
			this.start = start;
			this.end = end;
		}

		public bool IsIgnore(float time) {
			return start <= time && time <= end;
		}

		public float Start() {
			return start;
		}

		public float End() {
			return end;
		}
	}
}