using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class HealingModifier : BaseModifier {
		private HealingInfo info;

		private float elapsed;
		private float intervalElapsed;
		private HealthComponent targetHealthComponent;
		private float powerScale = 1;
		private float healPercentPerTickWithoutPowerScale;
		private float amplifier = 0;
		private float duration = 0;

		public HealingModifier(ModifierInfo info,
		                       Entity casterEntity, Entity targetEntity, Environment environment,
		                       CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (HealingInfo) info;

			duration = this.info.Hmc.ShowDurationInSeconds();
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			if (this.info.Hmc.isTotal) {
				int tickCount = (int) (duration / this.info.Hmc.interval) + 1;
				healPercentPerTickWithoutPowerScale = this.info.Hmc.percent / tickCount;
			}
			else {
				healPercentPerTickWithoutPowerScale = this.info.Hmc.percent;
			}
		}

		public override ModifierType Type() {
			return ModifierType.Healing;
		}

		protected override void OnUpdate(float dt) {
			if (elapsed >= duration) return;
			if (elapsed == 0) {
				Heal();
			}
			elapsed += dt;
			intervalElapsed += dt;
			if (intervalElapsed >= info.Hmc.interval) {
				intervalElapsed = 0;
				Heal();
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

		public void SetAmplifier(float value) {
			this.amplifier = value;
		}

		private void Heal() {
			if (info.Hmc.flat)
			{
				targetHealthComponent.RecoverHealthBy((int) (healPercentPerTickWithoutPowerScale * powerScale * (1 + amplifier)));
			}
			else
			{
				targetHealthComponent.RecoverHealthBy(healPercentPerTickWithoutPowerScale * powerScale * (1 + amplifier));
			}
		}
	}
}