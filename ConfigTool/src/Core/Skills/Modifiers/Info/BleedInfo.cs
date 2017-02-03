using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class BleedInfo : ModifierInfo {
		private Target target;
		private BleedModifierConfig bmc;

		public BleedInfo(Target target, BleedModifierConfig bmc) {
			this.target = target;
			this.bmc = bmc;
		}

		public ModifierType ShowType() {
			return ModifierType.Bleed;
		}

		public float ShowSuccessRate() {
			return bmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(bmc.delayToApplyInFrames);
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
			return bmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return bmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return bmc.lifetimes;
		}

		public BleedModifierConfig Bmc => bmc;
	}
}