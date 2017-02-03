using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class HpShieldInfo : ModifierInfo {
		private Target target;
		private HpShieldModifierConfig hsmc;

		public HpShieldInfo(Target target, HpShieldModifierConfig hsmc) {
			this.target = target;
			this.hsmc = hsmc;
		}

		public ModifierType ShowType() {
			return ModifierType.HpShield;
		}

		public float ShowSuccessRate() {
			return hsmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(hsmc.delayToApplyInFrames);
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
			return hsmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return hsmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return hsmc.lifetimes;
		}

		public HpShieldModifierConfig Hsmc {
			get { return hsmc; }
		}
	}
}