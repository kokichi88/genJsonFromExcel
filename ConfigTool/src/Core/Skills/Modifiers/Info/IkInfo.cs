using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class IkInfo : ModifierInfo {
		private Target target;
		private IkModifierConfig imc;

		public IkInfo(Target target, IkModifierConfig imc) {
			this.target = target;
			this.imc = imc;
		}

		public ModifierType ShowType() {
			return ModifierType.IkAnimation;
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

		public IkModifierConfig Config => imc;
	}
}