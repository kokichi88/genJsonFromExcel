using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class AetherRecoveryModifier : BaseModifier {
		private AetherRecoveryInfo info;
		private AetherComponent aetherComponent;

		private float elapsed;
		private float powerScale = 1;

		public AetherRecoveryModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (AetherRecoveryInfo) info;
			aetherComponent = targetEntity.GetComponent<AetherComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.AetherRecovery;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
			if (elapsed >= info.Armc.period) {
				elapsed = 0;
				RecoverAether();
			}
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}

		public void SetPowerScale(float value) {
			powerScale = value;
		}

		private void RecoverAether() {
			if (aetherComponent != null) {
				aetherComponent.Add((int) (info.Armc.amount * powerScale));
			}
		}
	}
}