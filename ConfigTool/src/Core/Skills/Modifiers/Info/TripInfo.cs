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
	public class TripInfo : KnockdownInfo {
		public TripInfo(Target target, float successRate, float delayToApply, WeightLevel level, float height,
		                float timeToPeak, float timeToGround, float distance, float lieDuration, float lieToIdleDuration,
		                float floatingDur, bool enableWallHit,
		                SsarTuple<AnimMix, PlayMethod, float>[] animationMixingTable,
		                List<JumpAction.Event> events, List<VfxConfig> vfxs,
		                MovementBehavior movementBehavior, FacingBehavior facingBehavior,
		                Requirement requirement, List<LifetimeConfig> lifetimeConfigs,
		                bool moveHorizontallyWhenFloat,
		                bool stopHorizontalMovementWhenMeet)
			: base(target, successRate, delayToApply, level, height, timeToPeak, timeToGround, distance, lieDuration, lieToIdleDuration,
				floatingDur, enableWallHit, animationMixingTable, events, vfxs, movementBehavior, facingBehavior, requirement, lifetimeConfigs, moveHorizontallyWhenFloat, stopHorizontalMovementWhenMeet) {
		}

		public override ModifierType ShowType() {
			return ModifierType.Trip;
		}
	}
}