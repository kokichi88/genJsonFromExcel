using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class SleepInfo : ModifierInfo {
		private Target target;
		private SleepModifierConfig smc;

		public SleepInfo(Target target, SleepModifierConfig smc) {
			this.target = target;
			this.smc = smc;
		}

		public ModifierType ShowType() {
			return ModifierType.Sleep;
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

		public SleepModifierConfig Smc => smc;
	}
}