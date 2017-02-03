using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using PlayAnimationModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.PlayAnimationModifierConfig;

namespace Core.Skills.Modifiers.Info {
	public class PlayAnimationInfo : ModifierInfo {
		private PlayAnimationModifierConfig pamc;
		private readonly List<VfxConfig> vfxs;
		private readonly List<LifetimeConfig> lifetimeConfigs;

		public PlayAnimationInfo(PlayAnimationModifierConfig pamc, List<VfxConfig> vfxs,
		                         List<LifetimeConfig> lifetimeConfigs) {
			this.pamc = pamc;
			this.vfxs = vfxs;
			this.lifetimeConfigs = lifetimeConfigs;
		}

		public PlayAnimationModifierConfig PlayAnimationModifierConfig {
			get { return pamc; }
		}

		public ModifierType ShowType() {
			return ModifierType.PlayAnimation;
		}

		public float ShowSuccessRate() {
			return pamc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(pamc.delayToApplyInFrames);
		}

		public Target Target() {
			return Info.Target.Target;
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
			return lifetimeConfigs;
		}
	}
}