using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class SuperAtkModifier : BaseModifier {
		private SuperAtkInfo info;

		private StatsComponent targetStatsComponent;
		private Stats forceLevelStats;
		private ValueModifier valueModifier;

		public SuperAtkModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (SuperAtkInfo) info;
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			forceLevelStats = targetStatsComponent.CharacterStats.FindStats(StatsType.ForceLevel);
		}

		public override ModifierType Type() {
			return ModifierType.SuperAtk;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			valueModifier = forceLevelStats.AddModifier(
				StatsModifierOperator.Addition, info.Samc.forceIncrement
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
			forceLevelStats.RemoveModifier(valueModifier);
		}
	}
}