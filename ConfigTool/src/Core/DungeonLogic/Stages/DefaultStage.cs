using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Core.Utils;
using Core.DungeonLogic.Stages.Challenges;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;
using Checking;
using Core.Commons;
using Core.DungeonLogic.Stages.Challenges.Actions;
using Core.DungeonLogic.Stages.EndConditions;
using Core.DungeonLogic.Stages.Waves;
using UnityEngine;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages {
	public class DefaultStage {
		public const int UPDATE_RATE = 3; //update logic is called once each 1 update frames
		public delegate void WaveCycleDelegate(int waveOrder, WaveCycle waveCycle);

		private Environment.Environment environment;

		private event WaveCycleDelegate waveCycleEvent;
		private List<Goal> goals = new List<Goal>();
		private List<LosingCondition> losingConditions = new List<LosingCondition>();
//		private List<Challenge> challenges = new List<Challenge>();
//		private List<Challenge> challengesUnaffectedByWave = new List<Challenge>();
		private NotNullReference notNullReference = new NotNullReference();
		private StageResult stageResult = StageResult.Undefined;
		private UpdateCounter updateCounter = new UpdateCounter(UPDATE_RATE);
		private List<WaveLogic> waves = new List<WaveLogic>();
		private List<WaveLogic> finishedWaves = new List<WaveLogic>();
		private List<IAction> actions = new List<IAction>();
		private WaveLogic currentWave;
		private bool firstWaveStart = false;
		private List<SsarTuple<CharacterId, int>> monsterIdsAndSpawnCount;

		public DefaultStage(Environment.Environment environment) {
			notNullReference.Check(environment, "environment");

			this.environment = environment;
			monsterIdsAndSpawnCount = new List<SsarTuple<CharacterId, int>>();
		}

		public StageResult EvaluationResult() {
			return stageResult;
		}
		
		public void OnStart()
		{
			environment.ClearTriggeredEvents();
		}

		public void AddGoal(Goal goal) {
			notNullReference.Check(goal, "goal");

			goals.Add(goal);
			goal.OnAddedToStage(this);
		}

		public void AddLosingCondition(LosingCondition losingCondition) {
			notNullReference.Check(losingCondition, "losingCondition");

			losingConditions.Add(losingCondition);
			losingCondition.OnAddedToStage(this);
		}

		public void AddWave(WaveLogic waveLogic) {
			notNullReference.Check(waveLogic, "wave logic");

			waves.Add(waveLogic);
			monsterIdsAndSpawnCount.AddRange(waveLogic.ShowMonsterIdAndSpawnCount());
		}

		public void AddAction(IAction action)
		{
			notNullReference.Check(action, "wave action");

			actions.Add(action);
		}

		public void ResetResult() {
			stageResult = StageResult.Undefined;
		}

		public void Update(float dt) {
//			DLog.Log("DefaultStage update");
			updateCounter.Update(dt);
			if (!updateCounter.IsAvailable()) {
				return;
			}
			if (stageResult != StageResult.Undefined) {
				if (stageResult == StageResult.Completed && IsHeroDeadLosingConditionMet()) {
					stageResult = StageResult.Failed;
					return;
				}
				return;
			}

//			UpdateChallenges(updateCounter.PreviousAccumulatedDt());
			UpdateWaves(updateCounter.PreviousAccumulatedDt());
			UpdateActions(updateCounter.PreviousAccumulatedDt());
			UpdateGoals(updateCounter.PreviousAccumulatedDt());
			UpdateLosingConditions(updateCounter.PreviousAccumulatedDt());

			bool anyLosingConditionMet = EvaluateAnyLosingConditionsMet();
			if (anyLosingConditionMet) {
				stageResult = StageResult.Failed;
				return;
			}

			if (!environment.IsIdle()) {
				return;
			}

			bool allGoalsAreAchieved = EvaluateAllGoalsIsAchieved();
			if (allGoalsAreAchieved) {
				stageResult = StageResult.Completed;
			}
		}

		public static bool IsWaveOrderUnaffectedByWaveLogic(int waveOrder) {
			return waveOrder < 0;
		}

//		private void UpdateChallenges(float previousAccumulatedDt) {
//			foreach (Challenge c in challengesUnaffectedByWave) {
//				c.Update(previousAccumulatedDt, -1);
//			}
//		}

		private void UpdateWaves(float dt) {
			if(waves.Count==0)return;
			int index = -1;
			if (currentWave == null) {
				currentWave = waves[0];
				if (!firstWaveStart) {
					firstWaveStart = true;
					OnWaveStart(1);
				}
			}
			currentWave.Update(dt);

			if (currentWave.IsFinished()) {
				finishedWaves.Add(currentWave);

				if (finishedWaves.Count < waves.Count) {
					int indexOfNextWave = waves.IndexOf(currentWave) + 1;
					currentWave = waves[indexOfNextWave];
					OnWaveStart(indexOfNextWave + 1);
				}
			}
		}

		private void OnWaveStart(int order)
		{
			environment.ClearTriggeredEvents();
			NotifyWaveStart(order);
		}

		public bool AreAllWavesFinished() {
			for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++) {
				if (!waves[waveIndex].IsFinished()) {
					//DLog.Log(challenge.UnfinishedReason());
					return false;
				}
			}

			return true;
		}

		public void ListenToWaveCycle(WaveCycleDelegate listener) {
			if (listener != null) {
				waveCycleEvent += listener;
			}
		}

		public void UnlistenToWaveCycle(WaveCycleDelegate listener) {
			if (listener != null) {
				waveCycleEvent -= listener;
			}
		}

		public List<SsarTuple<CharacterId, int>> ShowMonsterIdAndSpawnCount() {
			return monsterIdsAndSpawnCount;
		}

		private void NotifyWaveStart(int order) {
			try {
				if (waveCycleEvent != null) {
					waveCycleEvent(order, WaveCycle.Start);
				}
			}
			catch (Exception e) {
				DLog.LogError(e.Message + "\n" + e.StackTrace);
			}
		}

		private void UpdateLosingConditions(float dt) {
			foreach (LosingCondition losingCondition in losingConditions) {
				losingCondition.Update(dt);
			}
		}

		private void UpdateGoals(float dt) {
			foreach (Goal goal in goals) {
				goal.Update(dt);
			}
		}

		private bool EvaluateAllGoalsIsAchieved() {
			if (goals.Count <= 0) {
				return false;
			}

			foreach (Goal goal in goals) {
				if (!goal.IsAchieved()) {
					//DLog.Log(goal.Reason());
					return false;
				}
			}
			return true;
		}

		private bool EvaluateAnyLosingConditionsMet() {
			if (losingConditions.Count <= 0) {
				return false;
			}

			foreach (LosingCondition losingCondition in losingConditions) {
				if (losingCondition.IsMet()) {
					return true;
				}
			}
			return false;
		}

		private bool IsHeroDeadLosingConditionMet() {
			foreach (LosingCondition losingCondition in losingConditions) {
				if (losingCondition is HeroDeadEndCondition) {
					return losingCondition.IsMet();
				}
			}
			return false;
		}

		private void UpdateActions(float dt)
		{
			foreach (IAction action in actions)
			{
				if (!action.IsFinished())
					action.Update(dt);
			}
		}

		private class UpdateCounter {
			private int rate;
			private int updateCounter;
			private float accumulatedDt;
			private float previousAccumulatedDt;

			public UpdateCounter(int rate) {
				this.rate = rate;
			}

			public void Update(float dt) {
				updateCounter++;
				accumulatedDt += dt;
				updateCounter %= rate;
				if (IsAvailable()) {
					previousAccumulatedDt = accumulatedDt;
					accumulatedDt = 0;
				}
			}

			public bool IsAvailable() {
				return updateCounter == 0;
			}

			public float PreviousAccumulatedDt() {
				return previousAccumulatedDt;
			}
		}

		public override string ToString() {
			StringBuilder goalsSb = new StringBuilder();
			foreach (Goal goal in goals) {
				goalsSb.Append(string.Format("\t\t{0}, \n", goal));
			}

			StringBuilder losingConditionSb = new StringBuilder();
			foreach (LosingCondition losingCondition in losingConditions) {
				losingConditionSb.Append(string.Format("\t\t{0}, \n", losingCondition));
			}

			StringBuilder challengeSb = new StringBuilder();
//			foreach (Challenge challenge in challenges) {
//				challengeSb.Append(string.Format("\t\t{0}, \n", challenge));
//			}

			return string.Format("{0}\n{1}\n{2}\n{3}", GetType().Name, goalsSb, losingConditionSb, challengeSb);
		}

		public enum WaveCycle {
			Start
		}
	}
}
