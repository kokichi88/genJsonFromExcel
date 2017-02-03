using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class KnockdownWeightModifier : BaseModifier {
		private KnockdownWeightInfo info;

		private float elapsed;
		private ValueModifier weightModifier;
		private Stats targetKnockdownWeightStats;

		public KnockdownWeightModifier(ModifierInfo info, Entity casterEntity,
		                               Entity targetEntity, Environment environment,
		                               CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (KnockdownWeightInfo) info;
			bool found;
			targetKnockdownWeightStats = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.KnockdownWeight, out found);
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.KnockdownWeight;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
		}

		public override bool IsBuff() {
			return true;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetKnockdownWeightStats.RemoveModifier(weightModifier);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetKnockdownWeightStats.RemoveModifier(weightModifier);
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			int goalWeight = info.Kwmc.ShowWeightLevel().ToInt();
			int diff = goalWeight - (int) targetKnockdownWeightStats.BakedFloatValue;
			weightModifier = targetKnockdownWeightStats.AddModifier(StatsModifierOperator.Addition, diff);
		}
	}
}