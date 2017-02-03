using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class KnockbackWeightModifier : BaseModifier {
		private KnockbackWeightInfo info;

		private float elapsed;
		private ValueModifier weightModifier;
		private Stats targetKnockbackWeightStats;

		public KnockbackWeightModifier(ModifierInfo info, Entity casterEntity,
		                               Entity targetEntity, Environment environment,
		                               CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (KnockbackWeightInfo) info;
			bool found;
			targetKnockbackWeightStats = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.KnockbackWeight, out found);
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.KnockbackWeight;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
		}

		public override bool IsBuff() {
			return true;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetKnockbackWeightStats.RemoveModifier(weightModifier);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetKnockbackWeightStats.RemoveModifier(weightModifier);
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			int goalWeight = info.Kwmc.ShowWeightLevel().ToInt();
			int diff = goalWeight - (int) targetKnockbackWeightStats.BakedFloatValue;
			weightModifier = targetKnockbackWeightStats.AddModifier(StatsModifierOperator.Addition, diff);
		}
	}
}