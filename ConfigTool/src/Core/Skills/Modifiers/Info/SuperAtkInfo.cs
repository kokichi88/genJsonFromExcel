using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class SuperAtkInfo : ModifierInfo {
		private Target target;
		private SuperAtkModifierConfig samc;

		public SuperAtkInfo(Target target, SuperAtkModifierConfig samc) {
			this.target = target;
			this.samc = samc;
		}

		public ModifierType ShowType() {
			return ModifierType.SuperAtk;
		}

		public float ShowSuccessRate() {
			return samc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(samc.delayToApplyInFrames);
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
			return samc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return samc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return samc.lifetimes;
		}

		public SuperAtkModifierConfig Samc => samc;
	}
}