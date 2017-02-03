using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class LauncherModifier : KnockdownModifier {
		private readonly LauncherInfo launcherInfo;

		public LauncherModifier(LauncherInfo launcherInfo, Vector3 collidedProjectilePosition,
		                        Vector3 collidedProjectileVelocity, Entity caster, Entity target,
		                        Camera camera, SkillId skillId, Environment environment,
		                        CollectionOfInteractions modifierInteractionCollection,
		                        WallHitConfig whc, float damageScale) : base(launcherInfo, collidedProjectilePosition, collidedProjectileVelocity, caster, target, camera, skillId, environment, modifierInteractionCollection, whc, damageScale) {
			this.launcherInfo = launcherInfo;
		}

		public override ModifierType Type() {
			return ModifierType.Launcher;
		}

		protected override KnockdownAnimationName AnimName() {
			return new KnockdownAnimationName(
				AnimationName.Knockdown.High.UPPER,
				AnimationName.Knockdown.High.FALL_LOOP,
				AnimationName.Knockdown.High.FALL_TO_LIE,
				AnimationName.Knockdown.High.LIE_LOOP,
				AnimationName.Knockdown.High.LIE_TO_IDLE
			);
		}

		protected override float GetSpeedOfUpperAnimation() {
			return launcherInfo.SpeedForUpperAnimation;
		}
	}
}