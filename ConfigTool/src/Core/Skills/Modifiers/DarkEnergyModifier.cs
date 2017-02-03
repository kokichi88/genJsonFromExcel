using Artemis;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class DarkEnergyModifier : BaseModifier {
		public DarkEnergyModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                          Environment environment,
		                          CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
		}

		public override ModifierType Type() {
			return ModifierType.DarkEnergy;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}
	}
}