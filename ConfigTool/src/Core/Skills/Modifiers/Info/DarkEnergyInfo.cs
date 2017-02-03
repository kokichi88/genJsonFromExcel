using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class DarkEnergyInfo : ModifierInfo {
		private Target target;
		private DarkEnergyModifierConfig config;

		public DarkEnergyInfo(Target target, DarkEnergyModifierConfig config) {
			this.target = target;
			this.config = config;
		}

		public ModifierType ShowType() {
			return ModifierType.DarkEnergy;
		}

		public float ShowSuccessRate() {
			return config.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(config.delayToApplyInFrames);
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
			return config.ListEnabledVfx();
		}

		public string ShowIcon() {
			return config.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return config.lifetimes;
		}
	}
}