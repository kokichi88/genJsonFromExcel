using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class VanishInfo : ModifierInfo {
		private Target target;
		private VanishModifierConfig vanishModifierConfig;
		private readonly List<VfxConfig> vfxs;

		public VanishInfo(Target target, VanishModifierConfig vanishModifierConfig, List<VfxConfig> vfxs) {
			this.target = target;
			this.vanishModifierConfig = vanishModifierConfig;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.Vanish;
		}

		public float ShowSuccessRate() {
			return vanishModifierConfig.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(vanishModifierConfig.delayToApplyInFrames);
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
			return vanishModifierConfig.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return vanishModifierConfig.lifetimes;
		}

		public VanishModifierConfig VanishModifierConfig {
			get { return vanishModifierConfig; }
		}
	}
}