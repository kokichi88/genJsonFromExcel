using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using ShackleModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.ShackleModifierConfig;

namespace Core.Skills.Modifiers.Info {
	public class ShackleInfo : ModifierInfo {
		private Target target;
		private ShackleModifierConfig shackleModifierConfig;
		private readonly List<VfxConfig> vfxs;

		public ShackleInfo(Target target, ShackleModifierConfig shackleModifierConfig, List<VfxConfig> vfxs) {
			this.target = target;
			this.shackleModifierConfig = shackleModifierConfig;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.Shackle;
		}

		public float ShowSuccessRate() {
			return shackleModifierConfig.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(shackleModifierConfig.delayToApplyInFrames);
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
			return vfxs;
		}

		public string ShowIcon() {
			return shackleModifierConfig.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return shackleModifierConfig.lifetimes;
		}

		public ShackleModifierConfig ShackleModifierConfig {
			get { return shackleModifierConfig; }
		}
	}
}