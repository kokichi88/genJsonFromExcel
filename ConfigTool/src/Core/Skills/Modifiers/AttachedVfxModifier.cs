using System.Collections.Generic;
using Artemis;
using Combat.Skills.Projectiles.Entity.Components;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Skills.Vfxs;
using Core.Utils.Extensions;
using MovementSystem.Components;
using Ssar.Combat.Skills.Interactions;
using Ssar.Combat.Skills.Projectiles.Entity.Components;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class AttachedVfxModifier : BaseModifier {
		private readonly ProjectileComponent projectile;

		private Direction projectileDirectionAtAttachment;
		private Direction targetFacingAtAttachment;
		private Vfx.SpawnPrefab spawnPrefabLogic;
		private Transform vfxTransform;
		private Character target;
		private bool flip;
		private DurationBasedLifetime lifetime;

		public AttachedVfxModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                           Environment environment,
		                           CollectionOfInteractions modifierInteractionCollection,
		                           ProjectileComponent projectile) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.projectile = projectile;
			projectileDirectionAtAttachment = projectile.Entity.GetComponent<ProjectileTrajectoryComponent>()
				.ShowTrajectoryDirection().ToLeftOrRightDirectionEnum();
		}

		public override ModifierType Type() {
			return ModifierType.AttachedVfx;
		}

		protected override void OnUpdate(float dt) {
			if (vfxTransform != null && target != null) {
				if (!flip) {
					flip = true;
					if (projectileDirectionAtAttachment == Direction.Left) {
						vfxTransform.localScale = new Vector3(-1, 1, 1);
					}
				}
			}
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			targetFacingAtAttachment = target.FacingDirection();
			this.target = target;
		}

		protected override void OnVfxPrefabSpawn(Vfx.SpawnPrefab logic) {
			base.OnVfxPrefabSpawn(logic);
			spawnPrefabLogic = logic;
			vfxTransform = logic.Vfx;
			vfxTransform.parent = logic.Joint;
		}

		public override StackResult TryStackWithNewOne(Modifier newOne) {
			if (newOne.Type() == ModifierType.AttachedVfx) {
				return StackResult.Stack;
			}
			return StackResult.None;
		}
	}
}