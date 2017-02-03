using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class MoveSpeedInfo : ModifierInfo {
		private Target target;
		private MoveSpeedModifierConfig msmc;
		private readonly List<VfxConfig> vfxs;

		public MoveSpeedInfo(Target target, MoveSpeedModifierConfig msmc, List<VfxConfig> vfxs) {
			this.target = target;
			this.msmc = msmc;
			this.vfxs = vfxs;
		}

		public virtual ModifierType ShowType() {
			return ModifierType.MovementSpeed;
		}

		public float ShowSuccessRate() {
			return msmc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(msmc.delayToApplyInFrames);
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
			return msmc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return msmc.lifetimes;
		}

		public MoveSpeedModifierConfig Msmc {
			get { return msmc; }
			set { msmc = value; }
		}
	}
}