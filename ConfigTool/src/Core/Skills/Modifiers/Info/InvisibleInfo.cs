using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class InvisibleInfo : ModifierInfo {
		private Target target;
		private readonly List<VfxConfig> vfxs;
		private InvisibleModifierConfig imc;
		private Skill parentSkill;

		public InvisibleInfo(Target target, List<VfxConfig> vfxs, InvisibleModifierConfig imc,
		                     Skill parentSkill) {
			this.target = target;
			this.vfxs = vfxs;
			this.imc = imc;
			this.parentSkill = parentSkill;
		}

		public ModifierType ShowType() {
			return ModifierType.Invisible;
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
			return imc.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return parentSkill;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return vfxs;
		}

		public string ShowIcon() {
			return imc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return imc.lifetimes;
		}

		public InvisibleModifierConfig InvisibleModifierConfig {
			get { return imc; }
		}
	}
}