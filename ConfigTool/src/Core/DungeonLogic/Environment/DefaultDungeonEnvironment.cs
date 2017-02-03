using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Assets.Scripts.Core.Utils;
using Core.DungeonLogic.Spawn;
using Checking;
using EntityComponentSystem;
using EntityComponentSystem.Templates;
using Gameplay.DungeonLogic;
using JsonConfig.Model;
using UnityEngine;
using Utils;
using Utils.DataStruct;

namespace Core.DungeonLogic.Environment {
	public class DefaultDungeonEnvironment : Environment {
		private Character.Character character;
		private Spawner spawner;
		private List<Monster.Monster> deadMonstersSoFar = new List<Monster.Monster>();
		private List<Monster.Monster> deadMonstersSoFarIncludingOnesSpawnedByOthers = new List<Monster.Monster>();
		private float elapsedTime;
		private NotNullReference notNullReference = new NotNullReference();
		private List<SsarTuple<int, GameObject>> gateAndId = new List<SsarTuple<int, GameObject>>();
//		private InitDungeonSystemCmd.SpawnedMonsterList spawnedMonsterList;
		private int currentStageOrder;
		private bool areAllBeatableMonstersFromCurrentStageDead = true;
		private List<long> spawnedMonstersThatAreNotEnvironmentRole = new List<long>();
		private List<long> naturalBornMonstersThatAreNotEnvironment = new List<long>();
		private List<int> triggeredEventIDs = new List<int>();

		public DefaultDungeonEnvironment(Spawner spawner, List<SsarTuple<int, GameObject>> gateAndId) {
			notNullReference.Check(spawner, "spawner");
			notNullReference.Check(gateAndId, "gateAndId");

			this.spawner = spawner;
			this.gateAndId = gateAndId;
			for (int i = 0; i < this.gateAndId.Count; i++) {
				GameObject gate = gateAndId[i].Element2;
				gate.layer = EntityLayerName.Gate.ToLayerIndex();
				foreach (Transform t in gate.GetComponentsInChildren<Transform>()) {
					t.gameObject.layer = gate.layer;
				}
			}
		}

		public void SetHero(Character.Character character) {
			notNullReference.Check(character, "character");

			this.character = character;
		}

		public void DestroyGates() {
			for (int kIndex = 0; kIndex < gateAndId.Count; kIndex++) {
				GameObject.Destroy(gateAndId[kIndex].Element2);
			}
		}

		public int GateCount() {
			return gateAndId.Count;
		}

		public bool IsIdle() {
			return true;
		}

		public Character.Character Character() {
			return character;
		}

		public IEnumerable<Monster.Monster> DeadMonstersSoFar() {
			return deadMonstersSoFar.AsReadOnly();
		}

		public IEnumerable<Monster.Monster> DeadMonstersSoFarIncludingOnesSpawnedByOthers() {
			return deadMonstersSoFarIncludingOnesSpawnedByOthers;
		}

		public Spawner Spawner() {
			return spawner;
		}

		public float ElapsedTime() {
			return elapsedTime;
		}

		public GameObject GetGateById(int gateId) {
			for (int kIndex = 0; kIndex < gateAndId.Count; kIndex++) {
				SsarTuple<int,GameObject> tuple = gateAndId[kIndex];
				if (tuple.Element1 == gateId) return tuple.Element2;
			}

			throw new Exception(string.Format(
				"Cannot find gate of id '{0}'", gateId
			));
		}

		public bool IsGateExisted(int gateId) {
			for (int kIndex = 0; kIndex < gateAndId.Count; kIndex++) {
				SsarTuple<int,GameObject> tuple = gateAndId[kIndex];
				if (tuple.Element1 == gateId) return true;
			}

			return false;
		}

		public bool AreAllBeatableMonstersFromCurrentStageDead() {
			int numberOfSpawnMonsterThatIsDead = 0;
			for (int spawnMonsterIndex = 0; spawnMonsterIndex < spawnedMonstersThatAreNotEnvironmentRole.Count; spawnMonsterIndex++) {
				long spawnMonsterUniqueId = spawnedMonstersThatAreNotEnvironmentRole[spawnMonsterIndex];

				for (int deadMonsterIndex = 0; deadMonsterIndex < deadMonstersSoFarIncludingOnesSpawnedByOthers.Count; deadMonsterIndex++) {
					Monster.Monster deadMonster = deadMonstersSoFarIncludingOnesSpawnedByOthers[deadMonsterIndex];
					if (spawnMonsterUniqueId == deadMonster.UniqueId()) {
						numberOfSpawnMonsterThatIsDead += 1;
					}
				}
			}

			return numberOfSpawnMonsterThatIsDead >= spawnedMonstersThatAreNotEnvironmentRole.Count;
		}

		public bool IsEventTriggered(int eventId)
		{
			return triggeredEventIDs.Contains(eventId);
		}

		public void ClearTriggeredEvents()
		{
			triggeredEventIDs.Clear();
		}

		public void DisableDefaultLogicCheckMonsterDeath()
		{
			areAllBeatableMonstersFromCurrentStageDead = false;
		}

		public void ManualSetAllMonstersDead() {
			areAllBeatableMonstersFromCurrentStageDead = true;
		}

		public void Elapse(float dt) {
			elapsedTime += dt;
		}

		public void AddDeadMonster(Monster.Monster monster) {
			notNullReference.Check(monster, "monster");
			
			if (monster.EntityRole() == EntityRole.Environment) return;

			deadMonstersSoFarIncludingOnesSpawnedByOthers.Add(monster);
			if (monster.SpawnSource().Source == SpawnSource.Dungeon_System)
			{
				deadMonstersSoFar.Add(monster);
			}
		}

		public void AddSpawnedMonster(long uniqueId, HeroConfig.BasicStats basicStats, Entity entity)
		{
			if (spawnedMonstersThatAreNotEnvironmentRole.Contains(uniqueId)) return;
			if (basicStats.ShowRole() == EntityRole.Environment) return;

			spawnedMonstersThatAreNotEnvironmentRole.Add(uniqueId);

			CacheTemplateArgsComponent argsComponent = entity.GetComponent<CacheTemplateArgsComponent>();
			if (argsComponent == null) return;
			
			SpawnSourceInfo source = new DungeonSystemSpawnSourceInfo();
			source = argsComponent.TemplateArgs.GetEntry<SpawnSourceInfo>(TemplateArgsName.SpawnSource);
			if (source.Source != SpawnSource.Dungeon_System) return;
			
			naturalBornMonstersThatAreNotEnvironment.Add(uniqueId);
		}

		public void OnStageStart(int stageOrder) {
			this.currentStageOrder = stageOrder;
		}

		public void TriggerEvent(int eventId)
		{
			triggeredEventIDs.Add(eventId);
		}
	}
}
