using UnityEngine;

namespace Core.Skills.Modifiers.Lifetimes {
	public class ParentSkillBasedLifetime : Lifetime {
		private Skill parent;

		private bool end;

		public ParentSkillBasedLifetime(Skill parent) {
			this.parent = parent;
		}

		public LifetimeType ShowType() {
			return LifetimeType.ParentSkill;
		}

		public void Update(float dt) {
			// DLog.Log("debug ParentSkillBasedLifetime:Update:IsStateBindingFinish: " + parent.IsStateBindingFinish());
			Check();
		}

		public void Check() {
			if (parent.IsStateBindingFinish()) {
				end = true;
			}
		}

		public bool IsEnd() {
			return end;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
		}
	}
}