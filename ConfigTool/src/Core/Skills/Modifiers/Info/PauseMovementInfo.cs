using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class PauseMovementInfo : ModifierInfo {
		private Target target;
		private PauseMovementModifierConfig pmmc;
		private readonly List<VfxConfig> vfxs;

		public PauseMovementInfo(Target target, PauseMovementModifierConfig pmmc, List<VfxConfig> vfxs) {
			this.target = target;
			this.pmmc = pmmc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.PauseMovement;
		}

		public float ShowSuccessRate() {
			return pmmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(pmmc.delayToApplyInFrames);
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
			return pmmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return pmmc.lifetimes;
		}

		public PauseMovementModifierConfig PauseMovementModifierConfig {
			get { return pmmc; }
		}
	}
}