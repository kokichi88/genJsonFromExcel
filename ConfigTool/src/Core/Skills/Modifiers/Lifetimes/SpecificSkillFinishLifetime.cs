using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;

namespace Core.Skills.Modifiers.Lifetimes {
	public class SpecificSkillFinishLifetime : Lifetime {
		private SpecificSkillFinishLifetimeConfig config;
		private Character targetCharacter;

		private bool end;
		private HashSet<Skill> skillsCastedSoFar = new HashSet<Skill>();

		public SpecificSkillFinishLifetime(SpecificSkillFinishLifetimeConfig config,
		                                   Character targetCharacter) {
			this.config = config;
			this.targetCharacter = targetCharacter;
		}

		public LifetimeType ShowType() {
			return LifetimeType.SpecificSkillFinish;
		}

		public void Update(float dt) {
			if (end) return;

			foreach (SkillId ongoingSkillId in targetCharacter.OngoingSkills()) {
				if (!config.IsInterested(ongoingSkillId.Category)) continue;

				Skill ongoingSkill = targetCharacter.FindOngoingSkill(ongoingSkillId);
				if (ongoingSkill == null) continue;

				skillsCastedSoFar.Add(ongoingSkill);
			}

			foreach (Skill skill in skillsCastedSoFar) {
				if (skill.IsFinish()) {
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