using System;
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using Behavior = Core.Skills.Modifiers.Info.StaggerInfo.Behavior;

namespace Core.Skills.Modifiers.Info {
	public class StunInfo : ModifierInfo {
		private Target target;
		private readonly StunModifierConfig smc;
		private readonly Skill parent;

		public StunInfo(Target target, StunModifierConfig smc, Skill parent) {
			this.target = target;
			this.smc = smc;
			this.parent = parent;
		}

		public ModifierType ShowType() {
			return ModifierType.Stun;
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
			return parent;
		}

		public List<VfxConfig> ShowVfxConfig() {
			return smc.ListEnabledVfx();
		}

		public string ShowIcon() {
			return smc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return smc.lifetimes;
		}

		public StunModifierConfig Config => smc;

		public Behavior[] ShowBehaviors() {
			List<Behavior> behaviors = new List<Behavior>();
			if (smc.interruptTargetSkill) {
				behaviors.Add(Behavior.InterruptTargetSkill);
			}

			return behaviors.ToArray();
		}

		public float ShowDuration() {
			return smc.ShowDurationInSeconds();
		}
	}
}