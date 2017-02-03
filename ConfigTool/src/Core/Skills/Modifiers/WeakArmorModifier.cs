using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class WeakArmorModifier : BaseModifier {
		private WeakArmorInfo info;

		private StatsComponent targetStatsComponent;
		private Stats knockbackWeightStats;
		private Stats knockdownWeightStats;
		private ValueModifier knockbackWeightValueModifier;
		private ValueModifier knockdownWeightValueModifier;

		public WeakArmorModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                          Environment environment,
		                          CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (WeakArmorInfo) info;
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			bool found;
			knockbackWeightStats = targetStatsComponent.CharacterStats.FindStats(StatsType.KnockbackWeight, out found);
			knockdownWeightStats = targetStatsComponent.CharacterStats.FindStats(StatsType.KnockdownWeight, out found);
		}

		public override ModifierType Type() {
			return ModifierType.SuperArmor;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			knockbackWeightValueModifier = knockbackWeightStats.AddModifier(
				StatsModifierOperator.Addition, -WeightLevelMethods.ConvertDeltaLevelToDeltaValue(info.Config.weightIncrement)
			);
			knockdownWeightValueModifier = knockdownWeightStats.AddModifier(
				StatsModifierOperator.Addition, -WeightLevelMethods.ConvertDeltaLevelToDeltaValue(info.Config.weightIncrement)
			);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			RemoveValueModifiers();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			RemoveValueModifiers();
		}

		private void RemoveValueModifiers() {
			knockbackWeightStats.RemoveModifier(knockbackWeightValueModifier);
			knockdownWeightStats.RemoveModifier(knockdownWeightValueModifier);
		}
	}
}