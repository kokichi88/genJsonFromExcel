using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using CameraShakeFxConfig = Combat.Skills.ModifierConfigs.Modifiers.CameraShakeFxConfig;

namespace Core.Skills.Modifiers.Info {
	public class CameraFxShakeInfo : ModifierInfo {
		private Target target;
		private CameraShakeFxConfig csfc;
		private readonly List<VfxConfig> vfxs;

		public CameraFxShakeInfo(Target target, CameraShakeFxConfig csfc, List<VfxConfig> vfxs) {
			this.target = target;
			this.csfc = csfc;
			this.vfxs = vfxs;
		}

		public ModifierType ShowType() {
			return ModifierType.CameraFxShake;
		}

		public float ShowSuccessRate() {
			return csfc.successRate;
		}

		public float DelayToApply() {
			return FrameAndSecondsConverter._30Fps.FramesToSeconds(csfc.delayToApplyInFrames);
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
			return csfc.ShowIcon();
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return csfc.lifetimes;
		}

		public CameraShakeFxConfig CameraShakeFxConfig {
			get { return csfc; }
		}
	}
}