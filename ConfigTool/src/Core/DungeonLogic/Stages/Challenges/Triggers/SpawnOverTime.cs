using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Config;
using Core.DungeonLogic.Environment.Monster;
using Core.DungeonLogic.Spawn;
using Checking;
using Combat.Stats;
using Core.Commons;
using Core.DungeonLogic.Environment;
using Core.DungeonLogic.Stages.Challenges.Actions;
using Core.DungeonLogic.Stages.Challenges.Trackers;
using EntityComponentSystem;
using Gameplay.DungeonLogic;
using JsonConfig.Model;
using MovementSystem.Components;
using UnityEngine;
using Utils;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Challenges.Triggers {
	public class SpawnOverTime {
		private CharacterId monsterId;
		private int monsterCount;
		private float spawnInterval;
		private int spawnCount;
		private float spawnX;
		private float spawnY;
		private float spawnZ;
		private Spawner gameObjectSpawner;
		private float xAxisAmplitude;
		private int xAxisDensity;
		private Direction facingDirection;
		private int[] spawnSkillLevelPool;
		private int monsterLevel;
		private string group;

		private int spawnCountSoFar;
		private float elapsedTimeSinceLastSpawn;
		private NotNullReference notNullReference = new NotNullReference();
		private int monsterCountSoFar;
		private Environment.Environment environment;
		private List<Monster> deadMonsters = new List<Monster>();
		private List<long> monsterUniqueIds = new List<long>();
		private EntityRole entityRole = EntityRole.Undefined;
		private SsarTuple<CharacterId, int> monsterIdAndSpawnCount;
		private HeroAndMonsterConfig hamc;
		private List<Tracker> trackers = new List<Tracker>();
		private List<IAction> actions = new List<IAction>();

		public void SetEnv(Environment.Environment env) {
			this.environment = env;
			this.gameObjectSpawner = env.Spawner();
		}

		public void SetHeroAndMonsterConfig(HeroAndMonsterConfig hamc) {
			this.hamc = hamc;
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");

			monsterId = new CharacterId(cookies.ElementAt(0));
			monsterCount = Convert.ToInt32(cookies.ElementAt(1));
			spawnInterval = Convert.ToSingle(cookies.ElementAt(2));
			spawnCount = Convert.ToInt32(cookies.ElementAt(3));
			spawnX = Convert.ToSingle(cookies.ElementAt(4));
			spawnY = Convert.ToSingle(cookies.ElementAt(5));
			spawnZ = Convert.ToSingle(cookies.ElementAt(6));
			xAxisAmplitude = Convert.ToSingle(cookies.ElementAt(7));
			xAxisDensity = Convert.ToInt32(cookies.ElementAt(8));
			facingDirection = (Direction) Enum.Parse(typeof(Direction), cookies.ElementAt(9));
			spawnSkillLevelPool = DungeonSpawnConfig.Spawn.ParseSkillLevelPool(cookies.ElementAt(10));
			monsterLevel = Convert.ToInt32(cookies.ElementAt(11));

			elapsedTimeSinceLastSpawn = spawnInterval;

			monsterCountSoFar = monsterCount * spawnCount;
			monsterIdAndSpawnCount = new SsarTuple<CharacterId, int>(monsterId, monsterCountSoFar);

			group = hamc.FindBasicStats(monsterId).team;
		}

		public void AddTracker(Tracker tracker)
		{
			trackers.Add(tracker);
		}

		public void AddAction(IAction action)
		{
			actions.Add(action);
		}

		public string UnfinishedReason() {
			if (!IsSpawnCompleted()) {
				return "Spawn not completed";
			}

			if (monsterCountSoFar != monsterUniqueIds.Count) {
				return string.Format(
					"monsterCountSoFar {0} != monsterSpawnIds.Count {1}, monster id {2}",
					monsterCountSoFar, monsterUniqueIds.Count, monsterId
				);
			}
			if (monsterCountSoFar != deadMonsters.Count) {
				return "monsterCountSoFar != deadMonsters.Count";
			}
			
			foreach (Tracker tracker in trackers)
			{
				if (!tracker.IsFinished())
					return tracker.UnfinishedReason();
			}

			foreach (IAction action in actions)
			{
				if (action.GetLayer() == DungeonSpawnConfig.ActionLayer.Wave && !action.IsFinished())
				{
					return action.UnfinishedReason();
				}
			}
			
			return "Unknown";
		}

		public bool IsFinished()
		{
			if (entityRole == EntityRole.Environment)
			{
				return IsSpawnCompleted();
				// return IsSpawnCompleted() && IsAllTrackersFinished() && IsAllActionFinished();
			}

			if (!IsAllTrackersFinished()) return false;

			if (!IsAllActionFinished()) return false;

			return IsSpawnCompleted()
			       && monsterCountSoFar == monsterUniqueIds.Count
			       && monsterCountSoFar == deadMonsters.Count;
		}

		public void Update(float dt, int waveOrder)
		{
			UpdateSpawn(dt, waveOrder);
			UpdateTracker(dt, waveOrder);
			UpdateAction(dt, waveOrder);
		}

		private void UpdateSpawn(float dt, int waveOrder)
		{
			if (IsFinished()) return;

			elapsedTimeSinceLastSpawn += dt;
			if (!IsSpawnCompleted() && elapsedTimeSinceLastSpawn >= spawnInterval)
			{
				elapsedTimeSinceLastSpawn = 0;
				spawnCountSoFar++;
				for (int i = 0; i < monsterCount; i++)
				{
					float x = Random();
					gameObjectSpawner.SpawnEntity(
						monsterId,
						monsterLevel,
						x,
						spawnY,
						spawnZ,
						new DungeonSystemSpawnSourceInfo(),
						facingDirection,
						spawnSkillLevelPool,
						group
					).Then(entity =>
					{
						entityRole = entity.GetComponent<StatsComponent>().BasicStatsFromConfig.ShowRole();
						monsterUniqueIds.Add(entity.UniqueId);

						foreach (Tracker tracker in trackers)
						{
							tracker.AddEntity(entity);
						}

						foreach (IAction action in actions)
						{
							action.AddEntity(entity);
						}
					});
				}
			}

			foreach (Monster m in environment.DeadMonstersSoFar())
			{
				if (!monsterUniqueIds.Contains(m.UniqueId())) continue;
				if (deadMonsters.Contains(m)) continue;

				deadMonsters.Add(m);
			}
		}

		private void UpdateTracker(float dt, int waveOrder)
		{
			foreach (Tracker tracker in trackers)
			{
				tracker.Update(dt, waveOrder);
			}
		}

		private void UpdateAction(float dt, int waveOrder)
		{
			foreach (IAction action in actions)
			{
				if (action.GetLayer() == DungeonSpawnConfig.ActionLayer.Wave)
					action.Update(dt);
			}
		}

		public override string ToString() {
			return string.Format(
				"{0}: \n\t\t\t\t\tmonsterId: {1}, monsterCount: {2}, spawnInterval: {3}, spawnCount: {4}, spawnX: {5}, spawnY: {6}",
				GetType().Name, monsterId, monsterCount, spawnInterval, spawnCount, spawnX, spawnY
			);
		}

		public SsarTuple<CharacterId, int> ShowMonsterIdAndSpawnCount() {
			return monsterIdAndSpawnCount;
		}

		private bool IsSpawnCompleted() {
			return spawnCountSoFar >= spawnCount;
		}

		private bool IsAllTrackersFinished()
		{
			foreach (Tracker tracker in trackers)
			{
				if (!tracker.IsFinished())
					return false;
			}

			return true;
		}

		private bool IsAllActionFinished()
		{
			foreach (IAction action in actions)
				if (!action.IsFinished())
					return false;

			return true;
		}

		private float Random() {
			float left = spawnX;
			float[] pos = new float[xAxisDensity];
			for (int i = 0; i < pos.Length; i++) {
				pos[i] = left + i * xAxisAmplitude / xAxisDensity;
			}
			int index = BattleUtils.RandomRangeInt(0, pos.Length);
			return pos[index];
		}
	}
}
