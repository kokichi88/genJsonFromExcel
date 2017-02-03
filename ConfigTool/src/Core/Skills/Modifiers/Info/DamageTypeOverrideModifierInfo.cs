using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class DamageTypeOverrideModifierInfo : ModifierInfo {
		private Target target;
		private DamageTypeOverrideModifierConfig dtomc;

		public DamageTypeOverrideModifierInfo(Target target, DamageTypeOverrideModifierConfig dtomc) {
			this.target = target;
			this.dtomc = dtomc;
		}

		public ModifierType ShowType() {
			return ModifierType.DamageTypeOverride;
		}

		public float ShowSuccessRate() {
			return dtomc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(dtomc.delayToApplyInFrames);
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
			return dtomc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return dtomc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return dtomc.lifetimes;
		}

		public DamageTypeOverrideModifierConfig Dtomc => dtomc;
	}
}