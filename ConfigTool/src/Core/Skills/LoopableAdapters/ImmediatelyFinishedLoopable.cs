namespace Core.Skills.LoopableAdapters {
	public class ImmediatelyFinishedLoopable : Loopable {
		public void Update(float dt) {
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return true;
		}
	}
}