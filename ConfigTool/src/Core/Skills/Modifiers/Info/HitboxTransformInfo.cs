using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class HitboxTransformInfo : ModifierInfo {
		private Skill skill;
		private Target target;
		private HitboxTransformModifierConfig htmcConfig;
		private readonly List<VfxConfig> vfxs;

		public HitboxTransformInfo(Skill skill, Target target,
		                           HitboxTransformModifierConfig htmcConfig,
		                           List<VfxConfig> vfxs) {
			this.skill = skill;
			this.target = target;
			this.htmcConfig = htmcConfig;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.HitboxTransform;
		}

		public float ShowSuccessRate() {
			return htmcConfig.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(htmcConfig.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return htmcConfig.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return skill;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return vfxs;
		}

		public string ShowIcon() {
			return htmcConfig.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return htmcConfig.lifetimes;
		}

		public HitboxTransformModifierConfig HtmcConfig {
			get { return htmcConfig; }
		}
	}
}