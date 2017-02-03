using System;
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using Utils.DataStruct;
using AnimMix = Combat.Skills.ModifierConfigs.AnimationMix;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.MovementBehavior;
using FacingBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.FacingBehavior;
using Requirement = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.Requirement;

namespace Core.Skills.Modifiers.Info {
	public class BlastInfo : ModifierInfo {
		private Target target;
		private float successRate;
		private float delayToApply;
		private WeightLevel level;
		private float height;
		private float timeToPeak;
		private float timeToGround;
		private float flightDistance;
		private float flightMinSpeed;
		private float rollDistance;
		private float timeToRoll;
		private float timeToLie;
		private float lieToIdleDuration;
		private bool enableWallHit;
		private SsarTuple<AnimMix, PlayMethod, float>[] animationMixingTable;
		private BlastModifierConfig.AnimationProfile animationProfile;
		private readonly List<VfxConfig> vfxs;
		private List<JumpAction.Event> events = new List<JumpAction.Event>();
		private readonly MovementBehavior movementBehavior;
		private readonly FacingBehavior facingBehavior;
		private readonly Requirement requirement;
		private readonly List<LifetimeConfig> lifetimeConfigs;
		private readonly string icon;

		public BlastInfo(Target target, float successRate, float delayToApply, WeightLevel level, float height,
		                 float timeToPeak, float timeToGround, float flightDistance, float flightMinSpeed,
		                 float rollDistance, float timeToRoll, float timeToLie, float lieToIdleDuration,
		                 bool enableWallHit,
		                 SsarTuple<AnimMix, PlayMethod, float>[] animationMixingTable,
		                 List<JumpAction.Event> events,
		                 BlastModifierConfig.AnimationProfile animationProfile,
		                 List<VfxConfig> vfxs,
		                 MovementBehavior movementBehavior, FacingBehavior facingBehavior,
		                 Requirement requirement, List<LifetimeConfig> lifetimeConfigs,
		                 string icon = BaseModifierConfig.NO_ICON) {
			this.target = target;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.level = level;
			this.height = height;
			this.timeToPeak = timeToPeak;
			this.timeToGround = timeToGround;
			this.flightDistance = flightDistance;
			this.flightMinSpeed = flightMinSpeed;
			this.rollDistance = rollDistance;
			this.timeToRoll = timeToRoll;
			this.timeToLie = timeToLie;
			this.lieToIdleDuration = lieToIdleDuration;
			this.enableWallHit = enableWallHit;
			this.animationMixingTable = animationMixingTable;
			this.animationProfile = animationProfile;
			this.vfxs = vfxs;
			this.movementBehavior = movementBehavior;
			this.facingBehavior = facingBehavior;
			this.requirement = requirement;
			this.lifetimeConfigs = lifetimeConfigs;
			this.icon = icon;
			this.events.AddRange(events);
		}

		public ModifierType ShowType() {
			return ModifierType.Blast;
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

		public WeightLevel Level {
			get { return level; }
		}

		public float Height {
			get { return height; }
		}

		public float TimeToPeak {
			get { return timeToPeak; }
		}

		public float TimeToGround {
			get { return timeToGround; }
		}

		public float FlightDistance {
			get { return flightDistance; }
		}

		public float FlightMinSpeed {
			get { return flightMinSpeed; }
		}

		public float RollDistance {
			get { return rollDistance; }
		}

		public float TimeToRoll {
			get { return timeToRoll; }
		}

		public float TimeToLie {
			get { return timeToLie; }
		}

		public float LieToIdleDuration {
			get { return lieToIdleDuration; }
			set { lieToIdleDuration = value; }
		}

		public SsarTuple<AnimMix, PlayMethod, float>[] AnimationMixingTable {
			get { return animationMixingTable; }
		}

		public List<JumpAction.Event> Events {
			get { return events; }
		}

		public SsarTuple<AnimMix, PlayMethod, float> FindAnimMixing(AnimMix mix) {
			for (int i = 0; i < animationMixingTable.Length; i++) {
				SsarTuple<AnimMix, PlayMethod, float> m = animationMixingTable[i];
				if (m.Element1 == mix) return m;
			}

			throw new Exception("Missing AnimationMixing for " + mix);
		}

		public BlastModifierConfig.AnimationProfile AnimationProfile {
			get { return animationProfile; }
		}

		public bool EnableWallHit {
			get { return enableWallHit; }
		}

		public MovementBehavior MovementBehavior => movementBehavior;

		public FacingBehavior FacingBehavior => facingBehavior;

		public Requirement Requirement => requirement;
	}
}