using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class MaxAetherInfo : ModifierInfo {
		private Target target;
		private MaxAetherModifierConfig mamc;

		public MaxAetherInfo(Target target, MaxAetherModifierConfig mamc) {
			this.target = target;
			this.mamc = mamc;
		}

		public ModifierType ShowType() {
			return ModifierType.MaxAether;
		}

		public float ShowSuccessRate() {
			return mamc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(mamc.delayToApplyInFrames);
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
			return mamc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return mamc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return mamc.lifetimes;
		}

		public MaxAetherModifierConfig Mamc {
			get { return mamc; }
		}
	}
}