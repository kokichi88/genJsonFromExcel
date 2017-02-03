using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Ssar.Combat.Skills.Interactions;
using ImpactVfxModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.ImpactVfxModifierConfig;

namespace Core.Skills.Modifiers.Info {
	public class PlayImpactVfxInfo : ModifierInfo {
		private Target target;
		private ImpactVfxModifierConfig ivmc;
		private readonly List<VfxConfig> vfxs;

		public PlayImpactVfxInfo(Target target, ImpactVfxModifierConfig ivmc, List<VfxConfig> vfxs) {
			this.target = target;
			this.ivmc = ivmc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.PlayImpactVfx;
		}

		public float ShowSuccessRate() {
			return ivmc.successRate;
		}

		public float DelayToApply() {
			return ivmc.delayToApplyInFrames;
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
			return ivmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return ivmc.lifetimes;
		}

		public ImpactVfxModifierConfig ImpactVfxModifierConfig {
			get { return ivmc; }
		}
	}
}