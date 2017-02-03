using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class PauseAnimationInfo : ModifierInfo {
		private Target target;
		private PauseAnimationModifierConfig pamc;
		private readonly List<VfxConfig> vfxs;

		public PauseAnimationInfo(Target target, PauseAnimationModifierConfig pamc, List<VfxConfig> vfxs) {
			this.target = target;
			this.pamc = pamc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.PauseAnimation;
		}

		public float ShowSuccessRate() {
			return pamc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(pamc.delayToApplyInFrames);
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
			return pamc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return pamc.lifetimes;
		}

		public PauseAnimationModifierConfig PauseAnimationModifierConfig {
			get { return pamc; }
		}
	}
}