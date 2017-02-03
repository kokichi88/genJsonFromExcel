using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class SuperArmorInfo : ModifierInfo {
		private Target target;
		private SuperArmorModifierConfig samc;
		private readonly Skill skill;

		public SuperArmorInfo(Target target, SuperArmorModifierConfig samc, Skill skill) {
			this.target = target;
			this.samc = samc;
			this.skill = skill;
		}

		public ModifierType ShowType() {
			return ModifierType.SuperArmor;
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
			return samc.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return skill;
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

		public SuperArmorModifierConfig Config => samc;
	}
}