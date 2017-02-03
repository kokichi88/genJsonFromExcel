using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Skills.Modifiers.Lifetimes;

namespace Core.Skills.Modifiers {
	public interface Modifier {
		string Name();

		ModifierType Type();

		int SubType();

		void Update(float dt);

		void LateUpdate(float dt);

		bool IsBuff();

		void OnReplaceOtherModifiers(Character target, List<Modifier> others);

		void OnBeReplaced(Character target, Modifier byModifier);

		void OnAttachAsMain(Character target);

		bool OnAttachAsSub(Character target);

		ModifierAttachType ShowAttachType();

		void OnDetach(Character target);

		bool IsFinish();

		object[] Cookies();

		void OnCreateAsBuffFromSkill(Skill parentSkill);

		void OnCharacterDeath(Character deadCharacter);

		List<Lifetime> ShowLifetimes();

		void CheckLifetimes();

		void OnDamageDealt(Character caster, Character target,
		                   Skill fromSkill, Modifier fromModifier, int damage);

		float ShowAge();

		StackResult TryStackWithNewOne(Modifier newOne);

		int ShowStackCount();

		string ShowIcon();

		bool IsInvalidated();

		bool IsValidated();
	}

	public enum ModifierAttachType {
		Main,
		Sub
	}

	public enum StackResult {
		Manual,
		Stack,
		None
	}
}