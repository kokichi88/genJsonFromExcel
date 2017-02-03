using System;

namespace Core.Skills.Schedule {
	public class WaitForSeconds : Loopable {
		private float waitTime;
		private Action callback;

		private float elapsed;
		private bool isCallbackInvoked;

		public WaitForSeconds(float waitTime, Action callback) {
			this.waitTime = waitTime;
			this.callback = callback;
		}

		public void Update(float dt) {
			elapsed += dt;
			if (IsFinished() && !isCallbackInvoked) {
				isCallbackInvoked = true;
				callback();
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return elapsed >= waitTime;
		}
	}
}