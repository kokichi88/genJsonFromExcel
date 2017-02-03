using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class CcBreakByInputInfo : ModifierInfo {
		private Target target;
		private CcBreakByInputModifierConfig config;
		private Skill parentSkill;

		public CcBreakByInputInfo(Target target, CcBreakByInputModifierConfig config,
		                          Skill parentSkill) {
			this.target = target;
			this.config = config;
			this.parentSkill = parentSkill;
		}

		public ModifierType ShowType() {
			return ModifierType.CcBreakByInput;
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
			return config.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return parentSkill;
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

		public CcBreakByInputModifierConfig Config => config;
	}
}