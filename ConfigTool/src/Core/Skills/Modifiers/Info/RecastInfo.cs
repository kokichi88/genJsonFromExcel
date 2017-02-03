using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class RecastInfo : ModifierInfo {
		private Target target;
		private RecastModifierConfig rmc;

		public RecastInfo(Target target, RecastModifierConfig rmc) {
			this.target = target;
			this.rmc = rmc;
		}

		public ModifierType ShowType() {
			return ModifierType.Recast;
		}

		public float ShowSuccessRate() {
			return rmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(rmc.delayToApplyInFrames);
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
			return rmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return rmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return rmc.lifetimes;
		}

		public RecastModifierConfig Rmc => rmc;
	}
}