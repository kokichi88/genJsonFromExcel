using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class AetherRecoveryInfo : ModifierInfo {
		private Target target;
		private AetherRecoveryModifierConfig armc;

		public AetherRecoveryInfo(Target target, AetherRecoveryModifierConfig armc) {
			this.target = target;
			this.armc = armc;
		}

		public ModifierType ShowType() {
			return ModifierType.AetherRecovery;
		}

		public float ShowSuccessRate() {
			return armc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(armc.delayToApplyInFrames);
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
			return armc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return armc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return armc.lifetimes;
		}

		public AetherRecoveryModifierConfig Armc {
			get { return armc; }
		}
	}
}