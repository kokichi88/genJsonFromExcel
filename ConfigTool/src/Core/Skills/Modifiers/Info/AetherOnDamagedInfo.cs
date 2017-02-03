using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class AetherOnDamagedInfo : ModifierInfo {
		private Target target;
		private AetherOnDamagedModifierConfig aodmc;

		public AetherOnDamagedInfo(Target target, AetherOnDamagedModifierConfig aodmc) {
			this.target = target;
			this.aodmc = aodmc;
		}

		public ModifierType ShowType() {
			return ModifierType.AetherOnDamaged;
		}

		public float ShowSuccessRate() {
			return aodmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(aodmc.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return false;
		}

		public Skill ShowParentSkill() {
			return null;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return aodmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return aodmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return aodmc.lifetimes;
		}

		public AetherOnDamagedModifierConfig AetherOnDamagedModifierConfig => aodmc;
	}
}