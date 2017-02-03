using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class WeakArmorInfo : ModifierInfo {
		private Target target;
		private WeakArmorModifierConfig wamc;

		public WeakArmorInfo(Target target, WeakArmorModifierConfig wamc) {
			this.target = target;
			this.wamc = wamc;
		}

		public ModifierType ShowType() {
			return ModifierType.WeakArmor;
		}

		public float ShowSuccessRate() {
			return wamc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(wamc.delayToApplyInFrames);
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
			return wamc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return wamc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return wamc.lifetimes;
		}

		public WeakArmorModifierConfig Config => wamc;
	}
}