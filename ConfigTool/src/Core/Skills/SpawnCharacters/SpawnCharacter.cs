using System;
using System.Collections.Generic;
using Core.Commons;
using Core.DungeonLogic.Spawn;
using Core.Utils;
using Core.Utils.Extensions;
using EntityComponentSystem;
using Gameplay.DungeonLogic;
using JsonConfig.Model;
using MovementSystem.Components;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.SpawnCharacters {
	public class SpawnCharacter : Loopable {
		private float buffer = 3;

		private SpawnCharacterAction config;
		private EntitySpawner entitySpawner;
		private Character caster;
		private TemplateArgs args;
		private readonly Environment environment;
		private readonly SkillId skillId;
		private readonly HeroAndMonsterConfig hamc;
		private string entityGroupOfCaster;

		private float elapsed;
		private bool interrupted;
		private bool finished;
		private int spawnCountSoFar;
		private float spawnDelay;
		private float vfxDelay;
		private List<GameObject> vfxs = new List<GameObject>();
		private float bufferElapsed;
		private float spawnDelayElapsed;
		private Vector3 spawnPosition;
		private float vfxDelayElapsed;
		private List<Vector3> spawnPositions = new List<Vector3>();
		private int vfxCountSoFar;
		private float vfxElapsed;
		private string defaultEntityGroupOfMinion;

		public SpawnCharacter(SpawnCharacterAction config, EntitySpawner entitySpawner, Character caster,
		                      TemplateArgs args, Environment environment, SkillId skillId,
		                      HeroAndMonsterConfig hamc) {
			this.config = config;
			this.entitySpawner = entitySpawner;
			this.caster = caster;
			this.args = args;
			this.environment = environment;
			this.skillId = skillId;
			this.hamc = hamc;

			entityGroupOfCaster = caster.GameObject().GetComponent<EntityReference>().Entity.Group;
			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			spawnDelay = fasc.FramesToSeconds(config.delay);
			vfxDelay = fasc.FramesToSeconds(config.vfxDelay);
			defaultEntityGroupOfMinion = hamc.FindBasicStats(new CharacterId(config.groupId, config.subId)).team;

			if (config.spawnCount > 0) {
				Vector3 spawnPosition = CalculateSpawnPosition();
				spawnPositions.Add(spawnPosition);
				if (spawnDelay <= 0) {
					Spawn(spawnPosition);
				}

				if (vfxDelay <= 0) {
					PlayVfx(spawnPosition);
				}
			}
		}

		public void Update(float dt) {
			if (IsFinished()) {
				DestroyVfx();
				return;
			}

			spawnDelayElapsed += dt;
			if (spawnDelayElapsed >= spawnDelay) {
				elapsed += dt;
				if (elapsed >= config.spawnInterval && !IsSpawnCompleted()) {
					elapsed = 0;
					if (spawnPositions.Count <= spawnCountSoFar) {
						spawnPosition = CalculateSpawnPosition();
						spawnPositions.Add(spawnPosition);
					}
					else {
						spawnPosition = spawnPositions[spawnCountSoFar];
					}
					Spawn(spawnPosition);
				}
			}

			vfxDelayElapsed += dt;
			if (vfxDelayElapsed >= vfxDelay) {
				vfxElapsed += dt;
				if (vfxElapsed >= config.spawnInterval && !IsVfxCompleted()) {
					vfxElapsed = 0;
					if (spawnPositions.Count <= vfxCountSoFar) {
						spawnPosition = CalculateSpawnPosition();
						spawnPositions.Add(spawnPosition);
					}
					else {
						spawnPosition = spawnPositions[vfxCountSoFar];
					}
					PlayVfx(spawnPosition);
				}
			}

			if (IsSpawnCompleted()) {
				bufferElapsed += dt;
				if (bufferElapsed >= buffer) {
					finished = true;
					DestroyVfx();
				}
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			interrupted = true;
			DestroyVfx();
		}

		private void DestroyVfx() {
			foreach (GameObject vfx in vfxs) {
				GameObject.Destroy(vfx);
			}

			vfxs.Clear();
		}

		public bool IsFinished() {
			return finished || interrupted;
		}

		private Vector3 CalculateSpawnPosition() {
			Vector2 casterPos = caster.Position();
			Vector2 pivotPos = casterPos;
			if (config.targetPivot) {
				List<Character> nearbyEnemies = environment.FindNearbyCharacters(
					caster, casterPos, 99,
					new []{FindingFilter.ExcludeAllies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe}
				);
				if (nearbyEnemies.Count > 0) {
					pivotPos = nearbyEnemies[0].Position();
				}
			}
			Vector2 spawnPosition1 = pivotPos +
			                         config.relativeSpawnPosition.FlipFollowDirection(caster.FacingDirection());
			Vector2 spawnPosition2 = pivotPos +
			                         config.relativeSpawnPosition.FlipFollowDirection(caster.FacingDirection().Opposite());
			Vector2 diff1 = spawnPosition1 - casterPos;
			Vector2 diff2 = spawnPosition2 - casterPos;
			Vector2 spawnPosition = spawnPosition1;
			if (config.faraway && diff2.magnitude > diff1.magnitude) {
				spawnPosition = spawnPosition2;
			}
			if (args != null && args.Contains(TemplateArgsName.Position)) {
				spawnPosition = args.GetEntry<Vector2>(TemplateArgsName.Position);
			}

			Vector2 mostLeftAndMostRightOfCurrentStage = environment.MostLeftAndMostRightOfCurrentStage();
			spawnPosition.x = Mathf.Max(mostLeftAndMostRightOfCurrentStage.x, spawnPosition.x);
			spawnPosition.x = Mathf.Min(mostLeftAndMostRightOfCurrentStage.y, spawnPosition.x);

			if (config.ground) {
				Vector3 groundPosition = environment.MapColliders().ClampPositionToGround(spawnPosition);
				spawnPosition.y = groundPosition.y;
			}

			return spawnPosition;
		}

		private void Spawn(Vector3 spawnPosition) {
			spawnCountSoFar++;

			CharacterId cid = new CharacterId(config.groupId, config.subId);
			string[] splits = config.spawnSkillLevels.Split(',');
			int[] levels = new int[splits.Length];
			for (int kIndex = 0; kIndex < splits.Length; kIndex++) {
				levels[kIndex] = Convert.ToInt32(splits[kIndex]);
			}

			entitySpawner.SpawnEntity(
				cid, 1, spawnPosition.x, spawnPosition.y, spawnPosition.z,
				new SkillSpawnSourceInfo(skillId, caster.GameObject().GetComponent<EntityReference>().Entity.UniqueId),
				caster.FacingDirection(), levels, config.useParentGroup ? entityGroupOfCaster : defaultEntityGroupOfMinion
			);
		}

		private bool IsSpawnCompleted() {
			return spawnCountSoFar >= config.spawnCount;
		}

		private bool IsVfxCompleted() {
			return vfxCountSoFar >= config.spawnCount;
		}

		private void PlayVfx(Vector3 spawnPosition) {
			vfxCountSoFar++;

			GameObject vfxPrefab = config.ShowVfxPrefab();
			if (vfxPrefab == null) return;

			GameObject vfx = GameObject.Instantiate(vfxPrefab);
			vfxs.Add(vfx);
			vfx.transform.position = spawnPosition;
		}
	}
}