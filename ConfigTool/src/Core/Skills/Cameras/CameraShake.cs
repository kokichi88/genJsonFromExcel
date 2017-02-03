using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Cameras {
	public class CameraShake : Loopable {
		public CameraShake(Environment environment, CameraAction.ShakeFx sf) {
			environment.ShakeCamera(
				sf.strength, sf.duration, sf.vibrato, sf.smoothness, sf.randomness,
				sf.useRandomInitialAngel, sf.rotation
			);
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