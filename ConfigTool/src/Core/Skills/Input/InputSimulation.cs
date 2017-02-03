using Core.Utils;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Input {
	public class InputSimulation : Loopable {
		private DefaultUserInput userInput;
		private float durationInSeconds;

		private float elapsed;
		private bool isInterrupted;
		private bool isFinished;

		public InputSimulation(InputAction inputAction, DefaultUserInput userInput) {
			this.userInput = userInput;
			durationInSeconds = FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(inputAction.duration);
			userInput.StartSkill(new SkillId(inputAction.id));
			userInput.SetSkipTime(inputAction.skipTime);
		}

		public void Update(float dt) {
			elapsed += dt;

			if (elapsed >= durationInSeconds && !IsFinished()) {
				isFinished = true;
				userInput.StopSkill();
				userInput.UnsetSkipTime();
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return isFinished || isInterrupted;
		}
	}
}