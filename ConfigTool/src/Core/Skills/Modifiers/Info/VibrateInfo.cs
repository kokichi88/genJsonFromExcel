using System;
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers.Info {
	public class VibrateInfo : ModifierInfo {
		private Target target;
		private float successRate;
		private float delayToApply;
		private float xAmplitude;
		private int frequency;
		private bool shouldDecay;
		private float decayConstant;
		private readonly List<VfxConfig> vfxs;
		private readonly string icon;
		private readonly List<LifetimeConfig> lifetimeConfigs;

		public VibrateInfo(Target target, float successRate, float delayToApply, float xAmplitude,
		                   int frequency, bool shouldDecay, float decayConstant,
		                   List<VfxConfig> vfxs, string icon, List<LifetimeConfig> lifetimeConfigs) {
			this.target = target;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.xAmplitude = xAmplitude;
			this.frequency = frequency;
			this.shouldDecay = shouldDecay;
			this.decayConstant = decayConstant;
			this.vfxs = vfxs;
			this.icon = icon;
			this.lifetimeConfigs = lifetimeConfigs;
		}

		public ModifierType ShowType() {
			return ModifierType.Vibrate;
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
			return  icon;
		}

		public List<LifetimeConfig> ShowLifetimeConfigs() {
			return lifetimeConfigs;
		}

		public float XAmplitude {
			get { return xAmplitude; }
		}

		public int Frequency {
			get { return frequency; }
		}

		public bool ShouldDecay {
			get { return shouldDecay; }
		}

		public float DecayConstant {
			get { return decayConstant; }
		}

		public float ShowDuration(){
			foreach (LifetimeConfig lc in lifetimeConfigs) {
				switch (lc.ShowType()) {
					case LifetimeType.Duration:
						return ((DurationInSecondsLifetimeConfig) lc).duration;
						break;
					case LifetimeType.DurationInFrames:
						return  FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(
							((DurationInFramesLifetimeConfig) lc).duration);
						break;
				}
			}

			throw new Exception("Missing duration lifetime config");
		}
	}
}