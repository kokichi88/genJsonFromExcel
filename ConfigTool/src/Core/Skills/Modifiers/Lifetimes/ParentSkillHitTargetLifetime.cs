namespace Core.Skills.Modifiers.Lifetimes {
	public class ParentSkillHitTargetLifetime : Lifetime {
		private Character modifierTarget;
		private Skill parentSkill;

		private bool end;

		public ParentSkillHitTargetLifetime(Character modifierTarget, Skill parentSkill) {
			this.modifierTarget = modifierTarget;
			this.parentSkill = parentSkill;
		}

		public LifetimeType ShowType() {
			return LifetimeType.ParentSkillHitTarget;
		}

		public void Update(float dt) {
		}

		public void Check() {
		}

		public bool IsEnd() {
			return end;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			if (caster == modifierTarget && fromSkill == parentSkill) {
				end = true;
			}
		}
	}
}