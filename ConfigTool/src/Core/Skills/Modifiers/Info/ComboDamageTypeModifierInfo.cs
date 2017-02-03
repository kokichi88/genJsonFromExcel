using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class ComboDamageTypeModifierInfo : ModifierInfo {
		private Target target;
		private ComboDamageTypeModifierConfig cdtmc;

		public ComboDamageTypeModifierInfo(Target target, ComboDamageTypeModifierConfig cdtmc) {
			this.target = target;
			this.cdtmc = cdtmc;
		}

		public ModifierType ShowType() {
			return ModifierType.ComboDamageType;
		}

		public float ShowSuccessRate() {
			return cdtmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(cdtmc.delayToApplyInFrames);
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
			return cdtmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return cdtmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return cdtmc.lifetimes;
		}

		public ComboDamageTypeModifierConfig Cdtmc => cdtmc;
	}
}