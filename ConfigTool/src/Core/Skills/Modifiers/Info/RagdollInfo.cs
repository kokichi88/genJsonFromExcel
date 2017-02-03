using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class RagdollInfo : ModifierInfo {
		private RagdollModifierConfig rmc;
		private readonly List<VfxConfig> vfxs;
		private float projectileAge;

		public RagdollInfo(RagdollModifierConfig rmc, List<VfxConfig> vfxs, float projectileAge = 0) {
			this.rmc = rmc;
			this.vfxs = vfxs;
			this.projectileAge = projectileAge;
		}

		public ModifierType ShowType() {
			return ModifierType.Ragdoll;
		}

		public float ShowSuccessRate() {
			return rmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(rmc.delayToApplyInFrames);
		}

		public Target Target() {
			return Info.Target.Target;
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
			return rmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return rmc.lifetimes;
		}

		public RagdollModifierConfig RagdollModifierConfig {
			get { return rmc; }
		}

		public float ProjectileAge {
			get { return projectileAge; }
		}
	}
}