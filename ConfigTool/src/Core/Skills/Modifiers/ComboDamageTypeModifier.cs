using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class ComboDamageTypeModifier : BaseModifier {
		private ComboDamageTypeModifierInfo info;

		private uint trackerId;
		private DamageTypeComponent targetDamageTypeComponent;
		private static HashSet<SkillCategory> skillCategories= new HashSet<SkillCategory>(new [] {
			SkillCategory.NormalAttack,
			SkillCategory.DashAttack,
			SkillCategory.AirAttack
		});

		public ComboDamageTypeModifier(ModifierInfo info, Entity casterEntity,
		                               Entity targetEntity, Environment environment,
		                               CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ComboDamageTypeModifierInfo) info;
			targetDamageTypeComponent = targetEntity.GetComponent<DamageTypeComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.ComboDamageType;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			trackerId = targetDamageTypeComponent.OverrideSkillDamageType(
				info.Cdtmc.ShowSkillDamageType(), skillCategories
			);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);

			targetDamageTypeComponent.RemoveOverriddenSkillDamageType(trackerId);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);

			targetDamageTypeComponent.RemoveOverriddenSkillDamageType(trackerId);
		}
	}
}