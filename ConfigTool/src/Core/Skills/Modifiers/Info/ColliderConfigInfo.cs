using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class ColliderConfigInfo : ModifierInfo {
		private Target target;
		private readonly List<VfxConfig> vfxs;
		private ColliderConfigModifierConfig ccmc;

		public ColliderConfigInfo(Target target, List<VfxConfig> vfxs, ColliderConfigModifierConfig ccmc) {
			this.target = target;
			this.vfxs = vfxs;
			this.ccmc = ccmc;
		}

		public ModifierType ShowType() {
			return ModifierType.ColliderConfig;
		}

		public float ShowSuccessRate() {
			return ccmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(ccmc.delayToApplyInFrames);
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
			return ccmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return ccmc.lifetimes;
		}

		public ColliderConfigModifierConfig ColliderConfigModifierConfig {
			get => ccmc;
		}
	}
}