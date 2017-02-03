using Artemis;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class MoveSpeed2Modifier : MoveSpeedModifier {
		public MoveSpeed2Modifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                          Environment environment,
		                          CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
		}

		public override ModifierType Type() {
			return ModifierType.MovementSpeed2;
		}
	}
}