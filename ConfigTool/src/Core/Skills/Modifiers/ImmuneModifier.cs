using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class ImmuneModifier : BaseModifier {
		private ImmuneInfo info;

		private Stats immuneStats;
		private ValueModifier valueModifier;
		private DurationBasedLifetime lifetime;

		public ImmuneModifier(ModifierInfo info,
		                      Entity casterEntity, Entity targetEntity,
		                      Environment environment,
		                      CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ImmuneInfo) info;

			StatsComponent sc = targetEntity.GetComponent<StatsComponent>();
			bool found;
			immuneStats = sc.CharacterStats.FindStats(StatsType.Immune, out found);
		}

		public override ModifierType Type() {
			return ModifierType.Immune;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			foreach (Lifetime l in lifetimes) {
				if (l is DurationBasedLifetime) {
					lifetime = (DurationBasedLifetime) l;
				}
			}
			return lifetimes;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			valueModifier = immuneStats.AddModifier(StatsModifierOperator.Addition, 1);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			immuneStats.RemoveModifier(valueModifier);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			immuneStats.RemoveModifier(valueModifier);
		}

		public void ScaleLifetimeDurationBy(float value) {
			lifetime.ScaleDurationBy(value);
		}
	}
}