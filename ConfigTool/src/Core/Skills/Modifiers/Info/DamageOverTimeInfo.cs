using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class DamageOverTimeInfo : ModifierInfo {
		private Target target;
		private DamageOverTimeModifierConfig dotModifierConfig;
		private readonly List<VfxConfig> vfxs;

		public DamageOverTimeInfo(Target target, DamageOverTimeModifierConfig dotModifierConfig,
		                          List<VfxConfig> vfxs) {
			this.target = target;
			this.dotModifierConfig = dotModifierConfig;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.DamageOverTime;
		}

		public float ShowSuccessRate() {
			return dotModifierConfig.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(dotModifierConfig.delayToApplyInFrames);
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
			return dotModifierConfig.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return dotModifierConfig.lifetimes;
		}

		public DamageOverTimeModifierConfig DotModifierConfig {
			get { return dotModifierConfig; }
		}
	}
}