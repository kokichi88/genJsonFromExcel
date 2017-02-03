using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Core.Scheduling;
using Checking;
using Core.Commons;
using Core.DungeonLogic.Gates;
using Core.DungeonLogic.StageActivators;
using Core.DungeonLogic.Stages;
using Core.DungeonLogic.Stages.Challenges.Actions;
using Gameplay.DungeonLogic;
using JsonConfig;
using JsonConfig.Model;
using UnityEngine;
using Utils;
using Utils.DataStruct;

namespace Core.DungeonLogic {
	public class DungeonLogic : SimTimeObserver {
		public delegate void StageCycleDelegate(int stageOrder, StageCycle cycle);
		public delegate void GateCycleDelegate(int gateOrder, GateCycle cycle);

		public delegate void DungeonEventDelegate(DungeonEvent dungeonEvent);

		public delegate void StageWaveCycleDelegate(int stageOrder, int waveOrder, DefaultStage.WaveCycle cycle);

		public event EventHandler<DungeonResult> DungeonResultEventHandler;

		private event StageCycleDelegate stageCycleEvent;
		private event GateCycleDelegate gateCycleEvent;
		private event DungeonEventDelegate dungeonEventDelegateEvent;
		private event StageWaveCycleDelegate stageWaveCycleEvent;
		private bool isStart = false;
		private List<Component> components = new List<Component>();
		private List<DefaultStage> stages = new List<DefaultStage>();
		private DefaultStage activeStage;
		private StageComponent activeStageComponent;
		private DungeonResult dungeonResult = DungeonResult.Undefined;
		private DungeonResult previousDungeonResult = DungeonResult.Undefined;
		private List<GateController> gates = new List<GateController>();
		private GateController activeGateController;
		private GateComponent activeGateComponent;
		private List<StageActivator> stageActivators = new List<StageActivator>();
		private StageActivator activeStageActivator;
		private StageActivatorComponent activeStageActivatorComponent;
		private List<IAction> actions = new List<IAction>();
		private NotNullReference notNullReference = new NotNullReference();
		private int completedStagesCount;
		private bool isGateOpenedLastTimeCheck = false;
		private int currentStageOrder;

		public DungeonResult Result() {
			return dungeonResult;
		}

		public void StartUp() {
			//DLog.Log("Dungeon start up");
			foreach (Component component in components) {
				component.StartUp();
			}

			CheckStagesIsConfigProperly();
			activeStage = stages[0];
			activeStageComponent = new StageComponent(activeStage);
			AddComponent(activeStageComponent);

			CheckStageCountMatchGateCount();
			if (gates.Count > 0) {
				activeGateController = gates[0];
			}

			CheckStageCountMatchStageActivatorCount();
			if (stageActivators.Count > 0) {
				activeStageActivator = stageActivators[0];
			}

			activeStage.OnStart();
			activeStage.ListenToWaveCycle(OnWaveCycle);
			
			DungeonActionComponent actionComponent = new DungeonActionComponent(actions);
			AddComponent(actionComponent);
		}

		public void ShutDown() {
			//DLog.Log("Dungeon shut down");
			foreach (Component component in components) {
				component.ShutDown();
			}
		}

		public void OnSimTime(float dt) {
			Update(dt);
		}

		public void Update(float frameTimeInSeconds) {
//			DLog.Log("Dungeon update");
			if (!isStart) return;

			UpdateComponents(frameTimeInSeconds);

			if (dungeonResult != DungeonResult.Undefined) return;

			EvaluateDungeonResult();

			if (IsStageTransitionTakingPlace()) {
				OpenGate();
				WaitForGateFullyOpenThenActiveStageActivator();
				WaitForStageActivatorToActiveThenMoveToNextStage();
			}
		}

		public void Start() {
			isStart = true;
			foreach (Component component in components) {
				component.Start();
			}
			currentStageOrder = 1;
			NotifyStageCycle(currentStageOrder, StageCycle.Start);
		}

		public void Restart()
		{
			isStart = true;
			foreach (Component component in components) {
				component.Start();
			}
			NotifyDungeonEvent(DungeonEvent.Restart);
		}

		public void Stop() {
			isStart = false;
			foreach (Component component in components) {
				component.Stop();
			}
		}

		public void AddComponent(Component component) {
			notNullReference.Check(component, "component");

			components.Add(component);
		}

		public void AddStage(DefaultStage stage) {
			notNullReference.Check(stage, "stage");

			stages.Add(stage);
		}

		public void ResetResult() {
			dungeonResult = DungeonResult.Undefined;
			activeStage.ResetResult();
		}

		public void AddGate(GateController gateController) {
			notNullReference.Check(gateController, "gate");
			
			gates.Add(gateController);
		}

		public void AddStageActivator(StageActivator stageActivator) {
			notNullReference.Check(stageActivator, "stageActivator");

			stageActivators.Add(stageActivator);
		}

		public void AddAction(IAction action)
		{
			notNullReference.Check(action, "stage action");

			actions.Add(action);
		}

		public int CompletedStagesCount() {
			return completedStagesCount;
		}

		public int ShowMonsterCountOfCurrentStage() {
			if (activeStage != null) {
				int count = 0;
				var monsterIdAndSpawnCount = activeStage.ShowMonsterIdAndSpawnCount();
				for (int kIndex = 0; kIndex < monsterIdAndSpawnCount.Count; kIndex++) {
					if(!BattleUtils.IgnoreMonsterInObjective(monsterIdAndSpawnCount[kIndex].Element1))
						count += monsterIdAndSpawnCount[kIndex].Element2;
				}

				return count;
			}

			return 0;
		}

		public int ShowMonsterCountThatAreNotEnvironmentOfCurrentStage(ConfigManager configManager)
		{
			if (activeStage == null) return 0;

			int count = 0;
			List<SsarTuple<CharacterId, int>> monsterIdAndSpawnCount = activeStage.ShowMonsterIdAndSpawnCount();
			HeroAndMonsterConfig heroAndMonsterConfig = configManager.GetConfig<HeroAndMonsterConfig>();
			for (int i = 0; i < monsterIdAndSpawnCount.Count; i++)
			{
				CharacterId characterId = monsterIdAndSpawnCount[i].Element1;
				HeroConfig.BasicStats basicStats = heroAndMonsterConfig.FindBasicStats(characterId);
				if (basicStats.ShowRole() == EntityRole.Environment) continue;

				count += monsterIdAndSpawnCount[i].Element2;
			}

			return count;
		}

		public void ListenToStageCycle(StageCycleDelegate listener) {
			stageCycleEvent += listener;
		}

		public void UnlistenToStageCycle(StageCycleDelegate listener) {
			stageCycleEvent -= listener;
		}

		public void ListenToGateCycle(GateCycleDelegate listener) {
			gateCycleEvent += listener;
		}

		public void UnlistenToGateCycle(GateCycleDelegate listener) {
			gateCycleEvent -= listener;
		}

		public void ListenToDungeonEvent(DungeonEventDelegate listener) {
			dungeonEventDelegateEvent += listener;
		}

		public void UnlistenToDungeonEvent(DungeonEventDelegate listener) {
			dungeonEventDelegateEvent -= listener;
		}

		public void ListenToStageWaveCycle(StageWaveCycleDelegate listener) {
			if (listener != null) {
				stageWaveCycleEvent += listener;
			}
		}

		public void UnlistenToStageWaveCycle(StageWaveCycleDelegate listener) {
			if (listener != null) {
				stageWaveCycleEvent -= listener;
			}
		}

		private void NotifyStageWaveCycle(int stageOrder, int waveOrder, DefaultStage.WaveCycle cycle) {
			try {
				if (stageWaveCycleEvent != null) {
					stageWaveCycleEvent(stageOrder, waveOrder, cycle);
				}
			}
			catch (Exception e) {
				DLog.LogError(e.Message + "\n" + e.StackTrace);
			}
		}

		private void OnWaveCycle(int waveorder, DefaultStage.WaveCycle wavecycle) {
			NotifyStageWaveCycle(currentStageOrder, waveorder, wavecycle);
		}

		private void NotifyDungeonEvent(DungeonEvent dungeonEvent) {
			if (dungeonEventDelegateEvent != null) {
				dungeonEventDelegateEvent(dungeonEvent);
			}
		}

		private void NotifyGateCycle(int gateOrder, GateCycle cycle) {
			if (gateCycleEvent != null) {
				gateCycleEvent(gateOrder, cycle);
			}
		}

		private void NotifyStageCycle(int stageOrder, StageCycle cycle) {
			if (stageCycleEvent != null) {
				stageCycleEvent(stageOrder, cycle);
			}
		}

		private void UpdateComponents(float frameTimeInSeconds) {
//			DLog.Log("Dungeon is start, update its components");
			foreach (Component component in components) {
				component.Update(frameTimeInSeconds);
			}
		}

		private void CheckStagesIsConfigProperly() {
			if (stages.Count < 1) {
				throw new Exception("Stages list is empty. Let's config somes beforehand");
			}
		}

		private void CheckStageCountMatchStageActivatorCount() {
			if (stages.Count - 1 != stageActivators.Count) {
				throw new Exception(string.Format(
					"Stage count '{0}' mismatches stage activator count '{1}'",
					stages.Count, stageActivators.Count
				));
			}
		}

		private void CheckStageCountMatchGateCount() {
			if (stages.Count - 1 != gates.Count) {
				throw new Exception(string.Format(
					"Stage count '{0}' mismatches gate count '{1}'", stages.Count, gates.Count
				));
			}
		}

		private void EvaluateDungeonResult()
		{
			StageResult stageResult = activeStage.EvaluationResult();
//				DLog.Log("stage result " + stageResult);
			if (stageResult == StageResult.Failed)
			{
				dungeonResult = DungeonResult.Failed;
				NotifyDungeonResult();
			}
			else if (stageResult == StageResult.Completed)
			{
				completedStagesCount++;
				if (IsLastStage())
				{
					dungeonResult = DungeonResult.Completed;
					int indexOfActiveStage = stages.IndexOf(activeStage);
					NotifyStageCycle(indexOfActiveStage + 1, StageCycle.Clear);
					//DLog.Log("Dungeon: result: " + dungeonResult);
					NotifyDungeonResult();
				}
			}
		}

		private void NotifyDungeonResult()
		{
			DungeonResultEventHandler?.Invoke(this, dungeonResult);
		}

		private void WaitForStageActivatorToActiveThenMoveToNextStage() {
			if (!activeStageActivator.IsActive()) return;

			isGateOpenedLastTimeCheck = false;
			//close and seal the gate so that it would not be opened again
			//DLog.Log("Close gate");
			activeGateController.Close();
			activeGateController.Seal();

			int indexOfActiveStageActivator = stageActivators.IndexOf(activeStageActivator);
			if (indexOfActiveStageActivator < stageActivators.Count - 1) {
				activeStageActivator = stageActivators[indexOfActiveStageActivator + 1];
				components.Remove(activeStageActivatorComponent);
			}

			int indexOfActiveGate = gates.IndexOf(activeGateController);
			if (indexOfActiveGate < gates.Count - 1) {
				activeGateController = gates[indexOfActiveGate + 1];
//				components.Remove(activeGateComponent);
			}

			MoveToNextStage();
		}

		private void WaitForGateFullyOpenThenActiveStageActivator() {
			if (!isGateOpenedLastTimeCheck && activeGateController.IsOpened()) {
				isGateOpenedLastTimeCheck = true;
				int indexOfActiveGate = gates.IndexOf(activeGateController);
				NotifyGateCycle(indexOfActiveGate + 1, GateCycle.Opened);
				activeStageActivatorComponent = new StageActivatorComponent(activeStageActivator);
				AddComponent(activeStageActivatorComponent);
			}
		}

		private void OpenGate() {
			if (activeGateController.IsClosed() && !activeGateController.IsSealed()) {
				//DLog.Log("Open gate");
				int indexOfActiveStage = stages.IndexOf(activeStage);
				NotifyStageCycle(indexOfActiveStage + 1, StageCycle.Clear);
				activeGateController.Open();
				activeGateComponent = new GateComponent(activeGateController);
				AddComponent(activeGateComponent);
			}
		}

		private bool IsStageTransitionTakingPlace() {
			StageResult stageResult = activeStage.EvaluationResult();
			return dungeonResult == DungeonResult.Undefined
			       && stageResult == StageResult.Completed;
		}

		private void MoveToNextStage() {
			activeStage.UnlistenToWaveCycle(OnWaveCycle);
			int indexOfActiveStage = stages.IndexOf(activeStage);
			NotifyStageCycle(indexOfActiveStage + 1, StageCycle.End);
			activeStage = stages[indexOfActiveStage + 1];
			activeStage.OnStart();
			activeStage.ListenToWaveCycle(OnWaveCycle);
			components.Remove(activeStageComponent);
			activeStageComponent = new StageComponent(activeStage);
			activeStageComponent.StartUp();
			activeStageComponent.Start();
			AddComponent(activeStageComponent);
			currentStageOrder = indexOfActiveStage + 2;
			NotifyStageCycle(currentStageOrder, StageCycle.Start);
			//DLog.Log("NEXT STAGE");
		}

		private bool IsLastStage() {
			return activeStage == stages[stages.Count - 1];
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			foreach (DefaultStage stage in stages) {
				sb.Append(string.Format("\t{0}, \n", stage));
			}
			return string.Format("{0}\n{1}\n{2}\n{3}", GetType().Name, sb.ToString(), gates, stageActivators);
		}

		public enum StageCycle {
			Start,
			Clear,
			End
		}

		public enum GateCycle {
			Opened,
			Closed
		}

		public enum DungeonEvent {
			Restart
		}
	}
}
