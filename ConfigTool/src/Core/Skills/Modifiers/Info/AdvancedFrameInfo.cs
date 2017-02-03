using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class AdvancedFrameInfo : ModifierInfo {
		private Target target;
		private float successRate;
		private float delayToApply;
		private float value;
		private float channelingValue;
		private readonly float stateBindingValue;
		private readonly List<VfxConfig> vfxs;
		private readonly string icon;
		private readonly List<LifetimeConfig> lifetimeConfigs;

		public AdvancedFrameInfo(Target target, float successRate, float delayToApply, float value,
		                         float channelingValue, float stateBindingValue,
		                         List<VfxConfig> vfxs, string icon,
		                         List<LifetimeConfig> lifetimeConfigs) {
			this.target = target;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.value = value;
			this.channelingValue = channelingValue;
			this.stateBindingValue = stateBindingValue;
			this.vfxs = vfxs;
			this.icon = icon;
			this.lifetimeConfigs = lifetimeConfigs;
		}

		public ModifierType ShowType() {
			return ModifierType.AdvancedFrame;
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

		public float Value {
			get { return value; }
		}

		public float ChannelingValue {
			get { return channelingValue; }
		}

		public float StateBindingValue {
			get { return stateBindingValue; }
		}
	}
}