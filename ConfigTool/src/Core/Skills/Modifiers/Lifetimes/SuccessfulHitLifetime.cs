using Core.Utils.Extensions;

namespace Core.Skills.Modifiers.Lifetimes {
	public class SuccessfulHitLifetime : Lifetime {
		private int count;
		private SkillCategory[] interestedCategories;
		private Character modifierTarget;

		private int countSoFar;

		public SuccessfulHitLifetime(int count, SkillCategory[] interestedCategories, Character modifierTarget) {
			this.count = count;
			this.interestedCategories = interestedCategories;
			this.modifierTarget = modifierTarget;
		}

		public LifetimeType ShowType() {
			return LifetimeType.SuccessfulHit;
		}

		public void Update(float dt) {
		}

		public void Check() {
		}

		public bool IsEnd() {
			return countSoFar >= count;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			if (caster == modifierTarget && fromSkill != null) {
				SkillId skillId = null;
				if (caster.SkillId(fromSkill, ref skillId) && interestedCategories.Contains(skillId.Category)) {
					countSoFar++;
				}
			}
		}
	}
}