using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class ImmuneInfo : ModifierInfo {
		private Target target;
		private ImmuneModifierConfig imc;

		public ImmuneInfo(Target target, ImmuneModifierConfig imc) {
			this.target = target;
			this.imc = imc;
		}

		public ModifierType ShowType() {
			return ModifierType.Immune;
		}

		public float ShowSuccessRate() {
			return imc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(imc.delayToApplyInFrames);
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
			return imc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return imc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return imc.lifetimes;
		}

		public ImmuneModifierConfig Config => imc;
	}
}