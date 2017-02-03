using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class LockFrameInfo : ModifierInfo {
		private Target target;
		private float successRate;
		private float delayToApply;
		private float delayForCaster;
		private float delayForTarget;
		private float durationForCaster;
		private float durationForTarget;
		private bool lockGlobally;
		private float delayForGlobal;
		private float durationForGlobal;
		private readonly List<VfxConfig> vfxs;
		private readonly List<LifetimeConfig> lifetimeConfigs;
		private readonly string icon;

		private float powerScale = 1;

		public LockFrameInfo(Target target, float successRate, float delayToApply, float delayForCaster,
		                     float delayForTarget, float durationForCaster, float durationForTarget,
		                     bool lockGlobally, float delayForGlobal, float durationForGlobal,
		                     List<VfxConfig> vfxs, List<LifetimeConfig> lifetimeConfigs,
		                     string icon = BaseModifierConfig.NO_ICON) {
			this.target = target;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.delayForCaster = delayForCaster;
			this.delayForTarget = delayForTarget;
			this.durationForCaster = durationForCaster;
			this.durationForTarget = durationForTarget;
			this.lockGlobally = lockGlobally;
			this.delayForGlobal = delayForGlobal;
			this.durationForGlobal = durationForGlobal;
			this.vfxs = vfxs;
			this.lifetimeConfigs = lifetimeConfigs;
			this.icon = icon;
		}

		public ModifierType ShowType() {
			return ModifierType.LockFrame;
		}

		public float ShowSuccessRate() {
			return successRate;
		}

		public float DelayToApply() {
			return delayToApply;
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
			return icon;
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return lifetimeConfigs;
		}

		public float DelayForCaster {
			get { return delayForCaster; }
		}

		public float DelayForTarget {
			get { return delayForTarget; }
		}

		public float DurationForCaster {
			get { return durationForCaster; }
		}

		public float DurationForTarget {
			get { return durationForTarget * powerScale; }
		}

		public bool LockGlobally {
			get { return lockGlobally; }
		}

		public float DelayForGlobal {
			get { return delayForGlobal; }
		}

		public float DurationForGlobal {
			get { return durationForGlobal; }
		}

		public void SetPowerScale(float value) {
			powerScale = value;
		}
	}
}