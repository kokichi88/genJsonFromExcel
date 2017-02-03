using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Cameras {
	public class CameraFade : Loopable {
		private Environment environment;
		private BaseEvent eventFrame;

		public CameraFade(Environment environment, BaseEvent eventFrame) {
			this.environment = environment;
			this.eventFrame = eventFrame;

			CameraAction ca = (CameraAction) eventFrame.action;
			CameraAction.FadeFx ff = (CameraAction.FadeFx) ca.fx;
			environment.FadeCamera(ff.duration, ff.color, ff.alphaCurve);
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