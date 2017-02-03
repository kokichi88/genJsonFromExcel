using System.Collections.Generic;
using Core.Skills.Modifiers;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Conditions {
	public class CharacterIsInModifierState : Condition {
		private Character character;
		private HashSet<ModifierType> interested = new HashSet<ModifierType>();
		private readonly float age;
		private readonly HashSet<MainModifierState> ragdollStates;

		private string reason;

		public CharacterIsInModifierState(Character character, HashSet<ModifierType> interested, float age,
		                                  HashSet<MainModifierState> ragdollStates) {
			this.character = character;
			this.interested = interested;
			this.age = age;
			this.ragdollStates = ragdollStates;
		}

		public bool IsMeet() {
			List<Modifier> modifiers = character.GetListModifiers();
			foreach (Modifier m in modifiers) {
				if (interested.Contains(m.Type())) {
					if (m.Type() == ModifierType.Ragdoll) {
						RagdollModifier rm = (RagdollModifier) m;
						if (ragdollStates.Contains(rm.State)) {
							return true;
						}
					}
					else {
						if (m.ShowAge() >= age) {
							return true;
						}
					}
				}
			}
			return false;
		}

		public void Update(float dt) {
		}

		public string Reason() {
			if (reason == null) {
				reason = string.Join(", ", interested);
				reason = "There isn't any state found in list " + reason;
			}

			return reason;
		}
	}
}