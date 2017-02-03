using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class MaxAetherModifier : BaseModifier {
		private MaxAetherInfo info;
		private AetherComponent aetherComponent;

		public MaxAetherModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (MaxAetherInfo) info;

			aetherComponent = targetEntity.GetComponent<AetherComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.MaxAether;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			if (aetherComponent != null) {
				aetherComponent.IncreaseMaxValueBy(info.Mamc.value);
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			if (aetherComponent != null) {
				aetherComponent.IncreaseMaxValueBy(-info.Mamc.value);
			}
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			if (aetherComponent != null) {
				aetherComponent.IncreaseMaxValueBy(-info.Mamc.value);
			}
		}
	}
}