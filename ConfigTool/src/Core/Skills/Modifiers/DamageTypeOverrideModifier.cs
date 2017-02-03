using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class DamageTypeOverrideModifier : BaseModifier {
		private DamageTypeOverrideModifierInfo info;

		private uint trackerId;
		private DamageTypeComponent targetDamageTypeComponent;
		private DurationBasedLifetime durationBasedLifetime;

		public DamageTypeOverrideModifier(ModifierInfo info, Entity casterEntity,
		                                  Entity targetEntity, Environment environment,
		                                  CollectionOfInteractions modifierInteractionCollection) : base(info,
			casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (DamageTypeOverrideModifierInfo) info;
			targetDamageTypeComponent = targetEntity.GetComponent<DamageTypeComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.DamageTypeOverride;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			foreach (Lifetime lifetime in lifetimes) {
				if (lifetime is DurationBasedLifetime) {
					durationBasedLifetime = (DurationBasedLifetime) lifetime;
				}
			}
			return lifetimes;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			List<ParentSkillCategory> parentCategories = info.Dtomc.ShowParentSkillCategories();
			SkillCategory[] allCategories = (SkillCategory[]) Enum.GetValues(typeof(SkillCategory));
			HashSet<SkillCategory> overriddenCategories = new HashSet<SkillCategory>();
			foreach (ParentSkillCategory parentSkillCategory in parentCategories) {
				foreach (SkillCategory skillCategory in allCategories) {
					if (skillCategory.ShowParentSkillCategory() != parentSkillCategory) continue;

					overriddenCategories.Add(skillCategory);
				}
			}
			trackerId = targetDamageTypeComponent.OverrideSkillDamageType(
				info.Dtomc.ShowSkillDamageType(), overriddenCategories
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

		public void SetPowerScale(float value) {
			durationBasedLifetime.ScaleDurationBy(value);
		}
	}
}