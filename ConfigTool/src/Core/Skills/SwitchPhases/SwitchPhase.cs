namespace Core.Skills.SwitchPhases {
	public class SwitchPhase : Loopable {
		private Skill skill;

		private bool isSwitched;

		public SwitchPhase(Skill skill) {
			this.skill = skill;
		}

		public void Update(float dt) {
			skill.SwitchToNextPhase();
			isSwitched = true;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return isSwitched;
		}
	}
}