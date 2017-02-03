using System;
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.MovementBehavior;
using FacingBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.FacingBehavior;

namespace Core.Skills.Modifiers.Info {
	public class StaggerInfo : ModifierInfo {
		private Target target;
		private Skill parentSkill;
		private float distance;
		private float movementDuration;
		private float successRate;
		private float delayToApply;
		private WeightLevel level;
		private Behavior[] behaviors = new Behavior[0];
		private string overrideAnimation;
		private string loopAnimation;
		private readonly float crossfadeLength;
		private readonly int animFrame;
		private readonly string icon;
		private readonly List<VfxConfig> vfxs;
		private readonly MovementBehavior movementBehavior;
		private readonly FacingBehavior facingBehavior;
		private readonly StaggerModifierConfig.Requirement requirement;
		private readonly List<LifetimeConfig> lifetimeConfigs;

		public StaggerInfo(Target target, Skill parentSkill,
		                   float distance, float movementDuration, float successRate,
		                   float delayToApply, WeightLevel level, Behavior[] behaviors,
		                   string overrideAnimation, List<VfxConfig> vfxs,
		                   MovementBehavior movementBehavior, FacingBehavior facingBehavior,
		                   StaggerModifierConfig.Requirement requirement,
		                   List<LifetimeConfig> lifetimeConfigs,
		                   string loopAnimation = null, float crossfadeLength = 0.1f, int animFrame = 0,
		                   string icon = BaseModifierConfig.NO_ICON) {
			this.target = target;
			this.parentSkill = parentSkill;
			this.distance = distance;
			this.movementDuration = movementDuration;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.level = level;
			this.overrideAnimation = overrideAnimation;
			this.vfxs = vfxs;
			this.movementBehavior = movementBehavior;
			this.facingBehavior = facingBehavior;
			this.requirement = requirement;
			this.lifetimeConfigs = lifetimeConfigs;
			this.loopAnimation = loopAnimation;
			this.crossfadeLength = crossfadeLength;
			this.animFrame = animFrame;
			this.icon = icon;
			if (behaviors != null) {
				this.behaviors = behaviors;
			}
		}

		public virtual ModifierType ShowType() {
			return ModifierType.Stagger;
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
			foreach (LifetimeConfig lifetimeConfig in lifetimeConfigs) {
				if (lifetimeConfig.ShowType() == LifetimeType.ParentSkill) return true;
			}
			return false;
		}

		public Skill ShowParentSkill() {
			return parentSkill;
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

		public float Distance {
			get { return distance; }
		}

		public float MovementDuration {
			get { return movementDuration; }
		}

		public WeightLevel Level {
			get { return level; }
		}

		public Behavior[] Behaviors {
			get { return behaviors; }
		}

		public string OverrideAnimation {
			get { return overrideAnimation; }
		}

		public string LoopAnimation {
			get => loopAnimation;
		}

		public float CrossfadeLength {
			get => crossfadeLength;
		}

		public int AnimFrame {
			get => animFrame;
		}

		public MovementBehavior MovementBehavior => movementBehavior;

		public FacingBehavior FacingBehavior => facingBehavior;

		public StaggerModifierConfig.Requirement Requirement => requirement;

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

		public enum Behavior {
			InterruptTargetSkill,
			StaggerTowardProjectile,
			StaggerAwayFromProjectilePosition,
			StaggerAwayFromCasterPosition,
			StaggerFollowCasterFacingDirection,
			TargetFaceOppositeOfCasterFacingDirection
		}

		public class VibrationInfo {
			private float xAmplitude;
			private int frequency;
			private float duration;
			private readonly bool shouldDecay;
			private readonly float decayConstant;

			public VibrationInfo(float xAmplitude, int frequency, float duration, bool shouldDecay,
			                     float decayConstant) {
				this.xAmplitude = xAmplitude;
				this.frequency = frequency;
				this.duration = duration;
				this.shouldDecay = shouldDecay;
				this.decayConstant = decayConstant;
			}

			public float XAmplitude {
				get { return xAmplitude; }
			}

			public int Frequency {
				get { return frequency; }
			}

			public float Duration {
				get { return duration; }
			}

			public bool ShouldDecay {
				get { return shouldDecay; }
			}

			public float DecayConstant {
				get { return decayConstant; }
			}
		}
	}
}