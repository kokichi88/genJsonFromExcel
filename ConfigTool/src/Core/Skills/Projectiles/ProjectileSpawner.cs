using System;
using System.Collections.Generic;
using Artemis;
using Assets.Scripts.Ssar.Dungeon.Model;
using Combat.Skills.Projectiles.Entity.Components;
using Combat.Stats;
using Core.Utils.Extensions;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using RSG;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Projectiles.Entity.Components;
using UnityEngine;
using Utils.DataStruct;
using Utils.Gizmos;
using BaseProjectile = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.BaseProjectile;
using ProjectileType = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.ProjectileType;
using MeleeProjectileConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.MeleeProjectile;
using RangerProjectileConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.RangerProjectile;
using StationaryTrajectoryConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.StationaryTrajectoryConfig;
using HitBoxShape = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.HitBoxShape;
using TrajectoryConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.TrajectoryConfig;
using DamageTickerConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.DamageTickerConfig;
using DestroyVfx = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.DestroyVfx;
using FollowerMode = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.FollowerMode;
using JointFollowerTrajectoryConfig = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.JointFollowerTrajectoryConfig;
using JointAnchorFollower = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.JointAnchorFollower;
using BlockDirection = Ssar.Combat.Skills.Projectiles.Entity.Components.ProjectileComponent.BlockDirection;

namespace Core.Skills.Projectiles {
	public class ProjectileSpawner {
		private const string meleeProjectilePrefabPath = "Effect/Common/MeleeProjectile";

		private PromiseWorld promiseWorld;
		private readonly MapColliderBoundariesConfig mapColliders;
		private readonly Environment environment;

		public ProjectileSpawner(PromiseWorld promiseWorld, MapColliderBoundariesConfig mapColliders,
		                         Environment environment) {
			this.promiseWorld = promiseWorld;
			this.mapColliders = mapColliders;
			this.environment = environment;
		}

		public SsarTuple<Entity, IPromise<Entity>> Spawn(BaseEvent instruction,
		                                                 Character caster, Skill parentSkill,
		                                                 TemplateArgs preferedArgs = null) {
			CastProjectileAction cpa = (CastProjectileAction) instruction.ShowAction();
			BaseProjectile projectileConfig = cpa.ShowProjectile();
			ProjectileType projectileType = projectileConfig.ShowProjectileType();

			TemplateArgs args = new TemplateArgs();
			Entity projectileEntity = null;
			IPromise<Entity> creationPromise = null;
			Vector2 relativePos = Vector2.zero;
			string prefabPath = String.Empty;
			Entity casterEntity = promiseWorld.GetEntityById(caster.Id());
			bool found;
			List<CastProjectileAction.Lifetime> lifetimes = null;
			TrajectoryConfig trajectoryConfig = null;
			DamageTickerConfig damageTickerConfig = projectileConfig.damageTickerConfig;
			List<HitBoxShape> hitBoxes = new List<HitBoxShape>();
			Vector2 pivotPosition = caster.Position();
			UsageGoal usageGoal = cpa is ImpactAction ? UsageGoal.CollisionDetection : UsageGoal.DamageDealing;
			int targetLockingDuration = 0;
			int targetLockingEventId = -1;
			bool targetLockingClampY = false;
			switch (projectileType) {
				case ProjectileType.Melee:
					MeleeProjectileConfig meleeProjectileConfig = (MeleeProjectileConfig) projectileConfig;
					HitBoxShape firstHitBox = meleeProjectileConfig.hitBoxes[0];
					hitBoxes.AddRange(meleeProjectileConfig.hitBoxes);
					relativePos = firstHitBox.ShowRelativePositionOfCenterPivot()
						.FlipFollowDirection(caster.FacingDirection());
					prefabPath = meleeProjectilePrefabPath;
					lifetimes = meleeProjectileConfig.lifetimes;
					FollowerMode followerMode = meleeProjectileConfig.anchor.ShowFollowerMode();
					switch (followerMode) {
						case FollowerMode.None:
							StationaryTrajectoryConfig stationaryTrajectoryConfig = new StationaryTrajectoryConfig();
							CastProjectileAction.NoneAnchorFollower anchorFollower = (CastProjectileAction.NoneAnchorFollower) meleeProjectileConfig.anchor;
							stationaryTrajectoryConfig.directionReverse = anchorFollower.directionReverse;
							trajectoryConfig = stationaryTrajectoryConfig;
							break;
						case FollowerMode.Caster:
							stationaryTrajectoryConfig = new StationaryTrajectoryConfig();
							stationaryTrajectoryConfig.followCaster = true;
							trajectoryConfig = stationaryTrajectoryConfig;
							break;
						case FollowerMode.Joint:
							JointFollowerTrajectoryConfig jftc = new JointFollowerTrajectoryConfig();
							JointAnchorFollower jaf = (JointAnchorFollower) meleeProjectileConfig.anchor;
							jftc.joint = jaf.joint;
							jftc.rotation = jaf.rotation;
							jftc.rotationDirection = jaf.rotationDirection;
							jftc.axis = jaf.axis;
							trajectoryConfig = jftc;
							break;
						default:
							throw new Exception(string.Format(
								"Missing logic to create config of FollowerMode '{0}'", followerMode
							));
					}
					break;
				case ProjectileType.Ranger:
					RangerProjectileConfig rangerProjectileConfig = (RangerProjectileConfig) projectileConfig;
					if (rangerProjectileConfig.useOwnPivotPosition) {
						pivotPosition = rangerProjectileConfig.pivotPosition;
					}
					relativePos = rangerProjectileConfig.hitBox.ShowRelativePositionOfCenterPivot();
					if (rangerProjectileConfig.flipRelativePositionTowardCasterFacingDirection) {
						relativePos = relativePos.FlipFollowDirection(caster.FacingDirection());
					}
					prefabPath = rangerProjectileConfig.prefabPath;
					lifetimes = rangerProjectileConfig.lifetimes;
					trajectoryConfig = rangerProjectileConfig.trajectoryConfig;
					hitBoxes.Add(rangerProjectileConfig.hitBox);
					targetLockingDuration = rangerProjectileConfig.lockDuration;
					targetLockingEventId = rangerProjectileConfig.lockEid;
					targetLockingClampY = rangerProjectileConfig.clampY;
					break;
				default:
					DLog.LogError(string.Format(
						"Cannot create projectile of type '{0}'", projectileType
					));
					break;
			}

			Vector2 worldPos = pivotPosition + relativePos;

			args.SetEntry(TemplateArgsName.Position, worldPos);
			args.SetEntry(TemplateArgsName.EntityType, EntityType.Projectile);
			args.SetEntry(TemplateArgsName.Group, caster.Group());
			args.SetEntry(TemplateArgsName.Projectile_PrefabPath, prefabPath);
			args.SetEntry(TemplateArgsName.Projectile_ParentCharacter, caster);
			args.SetEntry(TemplateArgsName.Projectile_ParentSkill, parentSkill);
			args.SetEntry(TemplateArgsName.Projectile_HitBoxes, hitBoxes);
			args.SetEntry(
				TemplateArgsName.Projectile_ParentEntity_TimeScaleComponent,
				casterEntity.GetComponent<TimeScaleComponent>()
			);
			args.SetEntry(TemplateArgsName.Projectile_Lifetime, lifetimes);
			args.SetEntry(TemplateArgsName.Projectile_Trajectory, trajectoryConfig);
			args.SetEntry(TemplateArgsName.Projectile_DamageTicker, damageTickerConfig);
			args.SetEntry(TemplateArgsName.Projectile_Blockable, cpa.projectile.blockable);
			args.SetEntry(TemplateArgsName.Projectile_BlockDirection, BlockDirection.Front);
			args.SetEntry(TemplateArgsName.Projectile_BlockSource, Source.PlainValue);
			args.SetEntry(TemplateArgsName.Projectile_BlockCount, 0);
			args.SetEntry(TemplateArgsName.Projectile_BlockDirection, BlockDirection.Front);
			args.SetEntry(TemplateArgsName.Projectile_UsageGoal, usageGoal);
			args.SetEntry(TemplateArgsName.MapColliderBoundaries, mapColliders);
			args.SetEntry(TemplateArgsName.Projectile_CollisionEvaluator, projectileConfig.ShowCollisionGroupEvaluator());
			args.SetEntry(TemplateArgsName.Projectile_SleepTime, cpa.projectile.sleep);
			args.SetEntry(TemplateArgsName.Projectile_TargetLockingDuration, targetLockingDuration);
			args.SetEntry(TemplateArgsName.Projectile_TargetLockingEventId, targetLockingEventId);
			args.SetEntry(TemplateArgsName.Projectile_TargetLockingClampY, targetLockingClampY);
			args.SetEntry(TemplateArgsName.Projectile_PivotCharacter, caster);
			args.SetEntry(TemplateArgsName.Projectile_Environment, environment);
			if (preferedArgs != null) {
				if (preferedArgs.Contains(TemplateArgsName.Position)) {
					pivotPosition = preferedArgs.GetEntry<Vector2>(TemplateArgsName.Position);
					worldPos = pivotPosition + relativePos;
					args.SetEntry(TemplateArgsName.Position, worldPos);
				}

				if (preferedArgs.Contains(TemplateArgsName.Projectile_BlockDirection)) {
					args.SetEntry(
						TemplateArgsName.Projectile_BlockDirection,
						preferedArgs.GetEntry<BlockDirection>(TemplateArgsName.Projectile_BlockDirection)
					);
				}

				if (preferedArgs.Contains(TemplateArgsName.Projectile_BlockSource)) {
					args.SetEntry(
						TemplateArgsName.Projectile_BlockSource,
						preferedArgs.GetEntry<Source>(TemplateArgsName.Projectile_BlockSource)
					);
				}

				if (preferedArgs.Contains(TemplateArgsName.Projectile_BlockCount)) {
					args.SetEntry(
						TemplateArgsName.Projectile_BlockCount,
						preferedArgs.GetEntry<int>(TemplateArgsName.Projectile_BlockCount)
					);
				}

				if (preferedArgs.Contains(TemplateArgsName.Projectile_PivotCharacter)) {
					args.SetEntry(
						TemplateArgsName.Projectile_PivotCharacter,
						preferedArgs.GetEntry<Character>(TemplateArgsName.Projectile_PivotCharacter)
					);
				}
			}
			projectileEntity = promiseWorld.CreateEntity(null, TemplateName.Projectile, args);
			creationPromise = projectileEntity.GetComponent<EntityCreationPromiseComponent>().Promise;
			creationPromise.Then(entity => { CheckIfProjectileGameObjectIsInstantiated(caster, parentSkill, entity); });

			return new SsarTuple<Entity, IPromise<Entity>>(projectileEntity, creationPromise);
		}

		private void CheckIfProjectileGameObjectIsInstantiated(Character caster, Skill parentSkill, Entity entity) {
			if (entity.GetComponent<EntityGameObjectComponent>().GameObject != null) return;

			try {
				SkillId skillId = null;
				caster.SkillId(parentSkill, ref skillId);
				DLog.LogError(string.Format(
					"Cannot create GameObject for projectile of skill '{0}'. Maybe prefab path hasn't been configured properly",
					skillId
				));
			}
			catch (Exception e) {
				DLog.LogException(e);
			}
		}
	}
}