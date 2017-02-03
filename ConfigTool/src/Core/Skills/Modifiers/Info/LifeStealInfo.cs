using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class LifeStealInfo : ModifierInfo {
		private Target target;
		private LifeStealModifierConfig lsmc;

		public LifeStealInfo(Target target, LifeStealModifierConfig lsmc) {
			this.target = target;
			this.lsmc = lsmc;
		}

		public ModifierType ShowType() {
			return ModifierType.LifeSteal;
		}

		public float ShowSuccessRate() {
			return lsmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(lsmc.delayToApplyInFrames);
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
			return lsmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return lsmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return lsmc.lifetimes;
		}

		public LifeStealModifierConfig Lsmc {
			get { return lsmc; }
		}
	}
}