using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class FreezeInfo : ModifierInfo {
		private Target target;
		private FreezeModifierConfig fmc;
		private Skill parent;

		public FreezeInfo(Target target, FreezeModifierConfig fmc, Skill parent) {
			this.target = target;
			this.fmc = fmc;
			this.parent = parent;
		}

		public ModifierType ShowType() {
			return ModifierType.Freeze;
		}

		public float ShowSuccessRate() {
			return fmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(fmc.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return false;
		}

		public Skill ShowParentSkill() {
			return parent;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return fmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return fmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return fmc.lifetimes;
		}

		public FreezeModifierConfig Fmc => fmc;
	}
}