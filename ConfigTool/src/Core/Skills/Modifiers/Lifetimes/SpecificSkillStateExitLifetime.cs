using Combat.Skills.ModifierConfigs.Modifiers;

namespace Core.Skills.Modifiers.Lifetimes {
	public class SpecificSkillStateExitLifetime : Lifetime {
		private SpecificSkillStateExitLifetimeConfig config;
		private Character targetCharacter;

		private bool end;

		public SpecificSkillStateExitLifetime(SpecificSkillStateExitLifetimeConfig config,
		                                      Character targetCharacter) {
			this.config = config;
			this.targetCharacter = targetCharacter;
		}

		public LifetimeType ShowType() {
			return LifetimeType.SpecificSkillStateExit;
		}

		public void Update(float dt) {
			if (end) return;

			foreach (SkillId ongoingSkillId in targetCharacter.OngoingSkills()) {
				if (!config.IsInterested(ongoingSkillId.Category)) continue;

				Skill ongoingSkill = targetCharacter.FindOngoingSkill(ongoingSkillId);
				if (ongoingSkill == null) continue;

				if (ongoingSkill.IsStateBindingFinish()) {
					end = true;
				}
			}
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