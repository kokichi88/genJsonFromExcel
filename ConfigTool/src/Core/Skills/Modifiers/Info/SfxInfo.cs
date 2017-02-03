using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class SfxInfo : ModifierInfo {
		private Target target;
		private SfxModifierConfig smc;

		public SfxInfo(Target target, SfxModifierConfig smc) {
			this.target = target;
			this.smc = smc;
		}

		public ModifierType ShowType() {
			return ModifierType.Sfx;
		}

		public float ShowSuccessRate() {
			return smc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(smc.delayToApplyInFrames);
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
			return smc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return smc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return smc.lifetimes;
		}

		public SfxModifierConfig Smc => smc;
	}
}