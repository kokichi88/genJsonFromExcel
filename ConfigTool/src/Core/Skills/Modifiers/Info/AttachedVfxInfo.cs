using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class AttachedVfxInfo : ModifierInfo {
		private Target target;
		private AttachedVfxModifierConfig avmc;

		public AttachedVfxInfo(Target target, AttachedVfxModifierConfig avmc) {
			this.target = target;
			this.avmc = avmc;
		}

		public ModifierType ShowType() {
			return ModifierType.AttachedVfx;
		}

		public float ShowSuccessRate() {
			return avmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(avmc.delayToApplyInFrames);
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
			return avmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return avmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return avmc.lifetimes;
		}

		public AttachedVfxModifierConfig Avmc => avmc;
	}
}