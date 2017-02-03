using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class KnockbackWeightInfo : ModifierInfo {
		private Target target;
		private KnockbackWeightModifierConfig kwmc;
		private readonly List<VfxConfig> vfxs;

		public KnockbackWeightInfo(Target target, KnockbackWeightModifierConfig kwmc,
		                           List<VfxConfig> vfxs) {
			this.target = target;
			this.kwmc = kwmc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.KnockbackWeight;
		}

		public float ShowSuccessRate() {
			return kwmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(kwmc.delayToApplyInFrames);
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
			return vfxs;
		}

		public string ShowIcon() {
			return kwmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return kwmc.lifetimes;
		}

		public KnockbackWeightModifierConfig Kwmc {
			get { return kwmc; }
		}
	}
}