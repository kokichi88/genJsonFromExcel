using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class WindInfo : ModifierInfo {
		private Target target;
		private WindModifierConfig wmc;

		public WindInfo(Target target, WindModifierConfig wmc) {
			this.target = target;
			this.wmc = wmc;
		}

		public ModifierType ShowType() {
			return ModifierType.Wind;
		}

		public float ShowSuccessRate() {
			return wmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(wmc.delayToApplyInFrames);
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
			return wmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return wmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return wmc.lifetimes;
		}

		public WindModifierConfig Wmc => wmc;
	}
}