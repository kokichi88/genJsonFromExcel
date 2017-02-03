using Ssar.Combat.HeroStateMachines;

namespace Core.Skills.Conditions {
	public class HeroHasEnoughJumpChargeCondition : Condition {
		private Hero hero;

		public HeroHasEnoughJumpChargeCondition(Hero hero) {
			this.hero = hero;
		}

		public bool IsMeet() {
			return hero.HasEnoughJumpCharge();
		}

		public void Update(float dt) {
		}

		public string Reason() {
			return "Hero does not have enough jump charge";
		}
	}
}