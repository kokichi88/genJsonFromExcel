using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class HealingInfo : ModifierInfo {
		private Skill skill;
		private Target target;
		private HealingModifierConfig hmc;
		private readonly List<VfxConfig> vfxs;

		public HealingInfo(Skill skill, Target target, HealingModifierConfig hmc, List<VfxConfig> vfxs) {
			this.skill = skill;
			this.target = target;
			this.hmc = hmc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.Healing;
		}

		public float ShowSuccessRate() {
			return hmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(hmc.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return hmc.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return skill;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return vfxs;
		}

		public string ShowIcon() {
			return hmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return hmc.lifetimes;
		}

		public HealingModifierConfig Hmc {
			get { return hmc; }
		}
	}
}