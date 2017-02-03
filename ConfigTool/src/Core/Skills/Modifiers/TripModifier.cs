using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class TripModifier : KnockdownModifier {
		public TripModifier(TripInfo tripInfo, Vector3 collidedProjectilePosition,
		                    Vector3 collidedProjectileVelocity, Entity caster, Entity target,
		                    Camera camera, SkillId skillId, Environment environment,
		                    CollectionOfInteractions modifierInteractionCollection,
		                    WallHitConfig whc, float damageScale) : base(tripInfo, collidedProjectilePosition, collidedProjectileVelocity, caster, target, camera, skillId, environment, modifierInteractionCollection, whc, damageScale) {
		}

		public override ModifierType Type() {
			return ModifierType.Trip;
		}

		protected override KnockdownAnimationName AnimName() {
			return new KnockdownAnimationName(
				AnimationName.Trip.UPPER,
				AnimationName.Trip.FALL_LOOP,
				AnimationName.Trip.FALL_TO_LIE,
				AnimationName.Trip.LIE_LOOP,
				AnimationName.Trip.LIE_TO_IDLE
			);
		}

		protected override float GetSpeedOfUpperAnimation() {
			return base.GetSpeedOfUpperAnimation();
		}
	}
}