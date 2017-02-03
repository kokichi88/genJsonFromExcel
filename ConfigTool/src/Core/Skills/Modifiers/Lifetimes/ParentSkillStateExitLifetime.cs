namespace Core.Skills.Modifiers.Lifetimes {
	public class ParentSkillStateExitLifetime : Lifetime {
		private Skill parent;

		private bool end;

		public ParentSkillStateExitLifetime(Skill parent) {
			this.parent = parent;
		}

		public LifetimeType ShowType() {
			return LifetimeType.ParentSkillStateExit;
		}

		public void Update(float dt) {
			end = parent.IsStateBindingFinish();
		}

		public void Check() {
		}

		public bool IsEnd() {
			return end;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
		}
	}
}