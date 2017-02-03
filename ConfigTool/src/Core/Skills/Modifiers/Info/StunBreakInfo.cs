using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class StunBreakInfo : ModifierInfo {
		private Target target;
		private StunBreakModifierConfig sbmc;

		public StunBreakInfo(Target target, StunBreakModifierConfig sbmc) {
			this.target = target;
			this.sbmc = sbmc;
		}

		public ModifierType ShowType() {
			return ModifierType.StunBreak;
		}

		public float ShowSuccessRate() {
			return sbmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(sbmc.delayToApplyInFrames);
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
			return sbmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return sbmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return sbmc.lifetimes;
		}

		public StunBreakModifierConfig Config => sbmc;
	}
}