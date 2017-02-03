using System.Collections.Generic;

namespace Core.Skills {
	public enum SkillCategory {
#if UNITY_EDITOR
		Macro = -2,
#endif
		Undefined = -1,

		AirAttack = 0,
		DashAttack,
		DashBackwardAttack,
		NormalAttack,
		ChargedAttack,
		AirChargedAttack,

		AirDash = 50,
		Dash,
		DashBackward,
		Jump,
		SuperJump,
		Move,
		MoveBackward,
		Spawn,
		Fall,

		Skill = 100,
		FinishMove,
		Resurrect,
		Swap,
		AirSwap,
		PetSummon,
		ConsumableItem,
		Weapon,

		Passive = 150,
		PassiveEvent,
		PassiveStat,
		PassiveRecovery,
		Die,
		GuardBreak,
		PassiveTraitPoison,
		PassiveTraitShock,
		PassiveTraitBleed,
		PassiveTraitDark,
		Equipment,
		CcBreak
	}

	public enum ParentSkillCategory {
#if UNITY_EDITOR
		Macro,
#endif
		Undefined,
		Combo,
		Move,
		Active,
		Passive
	}

	public enum SubParentSkillCategory {
		Undefined,
		Passive,
		PassiveTrait
	}

	public static class SkillCategoryMethods {
		private static Dictionary<SkillCategory, ParentSkillCategory> parentByChild;
		private static Dictionary<SkillCategory, SubParentSkillCategory> subParentByChild;

		private static Dictionary<ParentSkillCategory, int> MASK_BY_PARENT_SKILL_CATEGORY = new Dictionary<ParentSkillCategory, int> {
			{ParentSkillCategory.Combo, 	1 << 0},
			{ParentSkillCategory.Move, 		1 << 1},
			{ParentSkillCategory.Active, 	1 << 2},
			{ParentSkillCategory.Passive, 	1 << 3}
		};

		static SkillCategoryMethods() {
			parentByChild = new Dictionary<SkillCategory, ParentSkillCategory>();

#if UNITY_EDITOR
			parentByChild[SkillCategory.Macro] = ParentSkillCategory.Macro;
#endif

			parentByChild[SkillCategory.Undefined] = ParentSkillCategory.Undefined;

			parentByChild[SkillCategory.AirAttack] = ParentSkillCategory.Combo;
			parentByChild[SkillCategory.DashAttack] = ParentSkillCategory.Combo;
			parentByChild[SkillCategory.DashBackwardAttack] = ParentSkillCategory.Combo;
			parentByChild[SkillCategory.NormalAttack] = ParentSkillCategory.Combo;
			parentByChild[SkillCategory.ChargedAttack] = ParentSkillCategory.Combo;
			parentByChild[SkillCategory.AirChargedAttack] = ParentSkillCategory.Combo;

			parentByChild[SkillCategory.AirDash] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.Dash] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.DashBackward] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.Jump] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.SuperJump] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.Move] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.MoveBackward] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.Spawn] = ParentSkillCategory.Move;
			parentByChild[SkillCategory.Fall] = ParentSkillCategory.Move;

			parentByChild[SkillCategory.Skill] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.FinishMove] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.Resurrect] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.Swap] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.AirSwap] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.PetSummon] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.ConsumableItem] = ParentSkillCategory.Active;
			parentByChild[SkillCategory.Weapon] = ParentSkillCategory.Active;

			parentByChild[SkillCategory.Passive] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveEvent] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveStat] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveRecovery] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.Die] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.GuardBreak] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveTraitPoison] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveTraitShock] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveTraitBleed] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.PassiveTraitDark] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.Equipment] = ParentSkillCategory.Passive;
			parentByChild[SkillCategory.CcBreak] = ParentSkillCategory.Passive;

			subParentByChild = new Dictionary<SkillCategory, SubParentSkillCategory>();

			subParentByChild[SkillCategory.Passive] = SubParentSkillCategory.Passive;
			subParentByChild[SkillCategory.PassiveEvent] = SubParentSkillCategory.Passive;
			subParentByChild[SkillCategory.PassiveStat] = SubParentSkillCategory.Passive;
			subParentByChild[SkillCategory.PassiveRecovery] = SubParentSkillCategory.Passive;
			subParentByChild[SkillCategory.Equipment] = SubParentSkillCategory.Passive;
			subParentByChild[SkillCategory.CcBreak] = SubParentSkillCategory.Passive;

			subParentByChild[SkillCategory.PassiveTraitPoison] = SubParentSkillCategory.PassiveTrait;
			subParentByChild[SkillCategory.PassiveTraitShock] = SubParentSkillCategory.PassiveTrait;
			subParentByChild[SkillCategory.PassiveTraitBleed] = SubParentSkillCategory.PassiveTrait;
			subParentByChild[SkillCategory.PassiveTraitDark] = SubParentSkillCategory.PassiveTrait;
		}

		public static ParentSkillCategory ShowParentSkillCategory(this SkillCategory sc) {
			return parentByChild[sc];
		}

		public static SubParentSkillCategory ShowSubParentSkillCategory(this SkillCategory sc) {
			SubParentSkillCategory r;
			if (subParentByChild.TryGetValue(sc, out r)) {
				return r;
			}

			return SubParentSkillCategory.Undefined;;
		}

		public static bool IsParentSkillCategorySet(ParentSkillCategory category, int valueToCheck) {
			int mask = MASK_BY_PARENT_SKILL_CATEGORY[category];
			return (mask & valueToCheck) > 0;
		}

		public static int SetParentSkillCategory(ParentSkillCategory category, int valueToSet) {
			int mask = MASK_BY_PARENT_SKILL_CATEGORY[category];
			return valueToSet | mask;
		}

		public static int UnSetParentSkillCategory(ParentSkillCategory category, int valueToUnSet) {
			int mask = MASK_BY_PARENT_SKILL_CATEGORY[category];
			return valueToUnSet & (~mask);
		}
	}
}