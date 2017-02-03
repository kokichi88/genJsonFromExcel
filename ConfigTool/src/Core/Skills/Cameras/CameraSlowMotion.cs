using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Cameras {
	public class CameraSlowMotion : Loopable {
		private Environment environment;
		private BaseEvent eventFrame;

		public CameraSlowMotion(Environment environment, BaseEvent eventFrame) {
			this.environment = environment;
			this.eventFrame = eventFrame;

			CameraAction ca = (CameraAction) eventFrame.ShowAction();
			CameraAction.SlowMotionFx smf = (CameraAction.SlowMotionFx) ca.fx;
			environment.PerformCameraSlowMotion(smf.timeScale, smf.duration);
		}

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