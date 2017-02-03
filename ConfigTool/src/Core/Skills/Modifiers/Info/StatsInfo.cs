using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class StatsInfo : ModifierInfo {
		private Skill skill;
		private Target target;
		private StatsModifierConfig smc;
		private readonly List<VfxConfig> vfxs;

		public StatsInfo(Skill skill, Target target, StatsModifierConfig smc,
		                 List<VfxConfig> vfxs) {
			this.skill = skill;
			this.target = target;
			this.smc = smc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.Stats;
		}

		public float ShowSuccessRate() {
			return smc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(smc.delayToApplyInFrames);
		}

		public Target Target() {
			return target;
		}

		public bool IsDependentOnSkill() {
			return smc.IsDependentOnParentSkill();
		}

		public Skill ShowParentSkill() {
			return skill;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return vfxs;
		}

		public string ShowIcon()
		{
			return smc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return smc.lifetimes;
		}

		public StatsModifierConfig Smc {
			get { return smc; }
		}
	}
}