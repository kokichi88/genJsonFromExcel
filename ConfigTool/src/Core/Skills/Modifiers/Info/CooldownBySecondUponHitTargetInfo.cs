using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class CooldownBySecondUponHitTargetInfo : ModifierInfo {
		private Target target;
		private CooldownBySecondUponHitTargetModifierConfig config;

		public CooldownBySecondUponHitTargetInfo(Target target,
		                                         CooldownBySecondUponHitTargetModifierConfig config) {
			this.target = target;
			this.config = config;
		}

		public ModifierType ShowType() {
			return ModifierType.CooldownBySecondUponHitTarget;
		}

		public float ShowSuccessRate() {
			return config.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(config.	delayToApplyInFrames);
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

		public CooldownBySecondUponHitTargetModifierConfig Config => config;
	}
}