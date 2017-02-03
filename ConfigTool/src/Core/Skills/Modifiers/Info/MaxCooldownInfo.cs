using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class MaxCooldownInfo : ModifierInfo {
		private Target target;
		private MaxCooldownModifierConfig cmc;

		public MaxCooldownInfo(Target target, MaxCooldownModifierConfig cmc) {
			this.target = target;
			this.cmc = cmc;
		}

		public ModifierType ShowType() {
			return ModifierType.MaxCooldown;
		}

		public float ShowSuccessRate() {
			return cmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(cmc.delayToApplyInFrames);
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
			return cmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return cmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return cmc.lifetimes;
		}

		public MaxCooldownModifierConfig Cmc {
			get { return cmc; }
		}
	}
}