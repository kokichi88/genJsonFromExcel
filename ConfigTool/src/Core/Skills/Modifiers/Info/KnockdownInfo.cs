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
	public class KnockdownInfo : ModifierInfo {
		private Target target;
		private float successRate;
		private float delayToApply;
		private WeightLevel level;
		private float height;
		private float timeToPeak;
		private float timeToGround;
		private float distance;
		private float lieDuration;
		private float lieToIdleDuration;
		private float floatingDur;
		private bool enableWallHit;
		private SsarTuple<AnimMix, PlayMethod, float>[] animationMixingTable;
		private readonly List<VfxConfig> vfxs;
		private readonly MovementBehavior movementBehavior;
		private readonly FacingBehavior facingBehavior;
		private readonly Requirement requirement;
		private readonly List<LifetimeConfig> lifetimeConfigs;
		private readonly bool moveHorizontallyWhenFloat;
		private readonly bool stopHorizontalMovementWhenMeet;
		private readonly string icon;
		private List<JumpAction.Event> events = new List<JumpAction.Event>();

		public KnockdownInfo(Target target, float successRate, float delayToApply, WeightLevel level,
		                     float height, float timeToPeak, float timeToGround, float distance,
		                     float lieDuration, float lieToIdleDuration, float floatingDur,
		                     bool enableWallHit,
		                     SsarTuple<AnimMix, PlayMethod, float>[] animationMixingTable,
		                     List<JumpAction.Event> events, List<VfxConfig> vfxs,
		                     MovementBehavior movementBehavior, FacingBehavior facingBehavior,
		                     Requirement requirement, List<LifetimeConfig> lifetimeConfigs,
		                     bool moveHorizontallyWhenFloat,
		                     bool stopHorizontalMovementWhenMeet,
		                     string icon = BaseModifierConfig.NO_ICON) {
			this.target = target;
			this.successRate = successRate;
			this.delayToApply = delayToApply;
			this.level = level;
			this.height = height;
			this.timeToPeak = timeToPeak;
			this.timeToGround = timeToGround;
			this.distance = distance;
			this.lieDuration = lieDuration;
			this.lieToIdleDuration = lieToIdleDuration;
			this.floatingDur = floatingDur;
			this.enableWallHit = enableWallHit;
			this.animationMixingTable = animationMixingTable;
			this.vfxs = vfxs;
			this.movementBehavior = movementBehavior;
			this.facingBehavior = facingBehavior;
			this.requirement = requirement;
			this.lifetimeConfigs = lifetimeConfigs;
			this.moveHorizontallyWhenFloat = moveHorizontallyWhenFloat;
			this.stopHorizontalMovementWhenMeet = stopHorizontalMovementWhenMeet;
			this.icon = icon;
			this.events.AddRange(events);
		}

		public virtual ModifierType ShowType() {
			return ModifierType.Knockdown;
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

		public float SuccessRate {
			get { return successRate; }
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

		public float Distance {
			get { return distance; }
		}

		public float LieDuration {
			get { return lieDuration; }
		}

		public float LieToIdleDuration {
			get { return lieToIdleDuration; }
		}

		public float FloatingDur {
			get {
				return floatingDur;
			}
		}

		public SsarTuple<AnimMix, PlayMethod, float>[] AnimationMixingTable {
			get { return animationMixingTable; }
		}

		public SsarTuple<AnimMix, PlayMethod, float> FindAnimMixing(AnimMix mix) {
			for (int i = 0; i < animationMixingTable.Length; i++) {
				SsarTuple<AnimMix, PlayMethod, float> m = animationMixingTable[i];
				if (m.Element1 == mix) return m;
			}

			throw new Exception("Missing AnimationMixing for " + mix);
		}

		public List<JumpAction.Event> Events {
			get { return events; }
		}

		public bool EnableWallHit {
			get { return enableWallHit; }
		}

		public MovementBehavior MovementBehavior => movementBehavior;

		public FacingBehavior FacingBehavior => facingBehavior;

		public Requirement Requirement => requirement;

		public bool MoveHorizontallyWhenFloat => moveHorizontallyWhenFloat;

		public bool StopHorizontalMovementWhenMeet => stopHorizontalMovementWhenMeet;

		public enum Behavior {
			KnockdownFollowProjectileVelocity,
			KnockdownAwayFromCasterPosition,
			KnockdownFollowCasterFacingDirection,
			TargetFaceOppositeOfCasterFacingDirection
		}
	}
}