using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class CooldownBySecondInfo : ModifierInfo {
		private Target target;
		private CooldownBySecondModifierConfig cbsmc;

		public CooldownBySecondInfo(Target target, CooldownBySecondModifierConfig cbsmc) {
			this.target = target;
			this.cbsmc = cbsmc;
		}

		public ModifierType ShowType() {
			return ModifierType.CooldownBySecond;
		}

		public float ShowSuccessRate() {
			return cbsmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(cbsmc.delayToApplyInFrames);
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
			return cbsmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return cbsmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return cbsmc.lifetimes;
		}

		public CooldownBySecondModifierConfig Cbsmc => cbsmc;
	}
}