using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;

namespace Core.Skills.Modifiers.Info {
	public class ParentSkillEventDispatcherInfo : ModifierInfo {
		private ParentSkillEventDispatcherModifierConfig psedmc;
		private Skill parentSkill;
		private Target target;

		public ParentSkillEventDispatcherInfo(ParentSkillEventDispatcherModifierConfig psedmc,
		                                      Skill parentSkill, Target target) {
			this.psedmc = psedmc;
			this.parentSkill = parentSkill;
			this.target = target;
		}

		public ModifierType ShowType() {
			return ModifierType.ParentSkillEventDispatcher;
		}

		public float ShowSuccessRate() {
			return psedmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(psedmc.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return true;
		}

		public Skill ShowParentSkill() {
			return parentSkill;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return psedmc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return psedmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return psedmc.lifetimes;
		}

		public ParentSkillEventDispatcherModifierConfig Psedmc => psedmc;
	}
}