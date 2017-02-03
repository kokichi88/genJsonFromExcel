using System;
using System.Collections.Generic;
using Artemis;
using Assets.Scripts.Config;
using Core.DungeonLogic.Configs;
using Core.DungeonLogic.Environment;
using Core.DungeonLogic.Stages;
using Core.DungeonLogic.Stages.Challenges;
using Checking;
using Combat.DamageSystem;
using Combat.Stats;
using Core.Commons;
using Core.DungeonLogic.Spawn;
using Core.DungeonLogic.Stages.Challenges.Actions;
using Core.DungeonLogic.Stages.Challenges.Trackers;
using Core.DungeonLogic.Stages.Challenges.Triggers;
using Core.DungeonLogic.Stages.Waves;
using EntityComponentSystem;
using EntityComponentSystem.Templates;
using Gameplay.DungeonLogic;
using JsonConfig;
using JsonConfig.Model;
using Ssar.Combat.Skills;
using UnityEngine;
using Utils.DataStruct;
using DungeonConfig = Core.DungeonLogic.Configs.DungeonConfig;

namespace Core.DungeonLogic {
	public class DungeonFactory {
		private DungeonConfig dungeonConfig;
		private DefaultDungeonEnvironment defaultEnvironment;
		private HeroAndMonsterConfig heroAndMonsterConfig;

		private NotNullReference notNullReference = new NotNullReference();

		public DungeonFactory(DungeonConfig dungeonConfig, EntitySpawner entitySpawner,
		                      CharacterId heroId, List<SsarTuple<int, GameObject>> gateAndId,
		                      DamageSystem damageSystem, PromiseWorld world, ConfigManager configManager)
		{
			notNullReference.Check(dungeonConfig, "dungeonConfig");
			notNullReference.Check(entitySpawner, "entitySpawner");
			notNullReference.Check(heroId, "heroId");
			notNullReference.Check(gateAndId, "gateAndId");
			notNullReference.Check(damageSystem, "damageSystem");
			notNullReference.Check(configManager, "configManager");

			heroAndMonsterConfig = configManager.GetConfig<HeroAndMonsterConfig>();
			defaultEnvironment = new DefaultDungeonEnvironment(entitySpawner, gateAndId);
			this.dungeonConfig = dungeonConfig;
			
			world.EntityCreationEventHandler += (sender, arg) =>
			{
				Entity entity = arg.Entity;
				if (arg.EntityType != EntityType.Creature) return;

				if (entity.GetComponent<SkillComponent>().CharacterId.Equals(heroId))
				{
					defaultEnvironment.SetHero(new DefaultDungeonCharacter(entity, damageSystem));
				}
			};
			
			damageSystem.EntityDeathEventHandler_Late += (sender, args) =>
			{
				CacheTemplateArgsComponent argsComponent = args.Entity.GetComponent<CacheTemplateArgsComponent>();
				SpawnSourceInfo source = argsComponent?.TemplateArgs.GetEntry<SpawnSourceInfo>(TemplateArgsName.SpawnSource) ?? new DungeonSystemSpawnSourceInfo();
				EntityRole role = args.Entity.GetComponent<StatsComponent>().BasicStatsFromConfig.ShowRole();
				defaultEnvironment.AddDeadMonster(new EntityMonster(Time.timeSinceLevelLoad,
					args.Entity.GetComponent<SkillComponent>().CharacterId, args.Entity.UniqueId, source, role));
			};
			
			entitySpawner.EntitySpawnEventHandler += (sender, args) =>
			{
				if (args.Entity.Group == EntityGroup.GROUP_A) return;
				defaultEnvironment.AddSpawnedMonster(args.UniqueId, args.BasicStats, args.Entity);
			};
		}

		public DefaultDungeonEnvironment DefaultEnvironment {
			get { return defaultEnvironment; }
		}

		public DungeonLogic CreateDungeon(int dungeonId) {
			DungeonLogic dungeonLogic = new DungeonLogic();

			dungeonLogic.AddComponent(new EnvironmentComponent(defaultEnvironment));
			CreateStages(dungeonConfig, dungeonLogic);

			CreateGates(dungeonConfig, dungeonLogic);

			CreateStageActivators(dungeonConfig, dungeonLogic);

			return dungeonLogic;
		}

		private void CreateStageActivators(Configs.DungeonConfig dungeonConfigCfg, DungeonLogic dungeonLogic) {
			foreach (StageActivatorConfig stageActivatorConfig in dungeonConfigCfg.StageActivatorList()) {
				Type stageActivatorClass = Type.GetType(stageActivatorConfig.ClassName());
				StageActivators.StageActivator stageActivator =
					(StageActivators.StageActivator) stageActivatorClass
						.GetConstructor(new[] {typeof(Environment.Environment)})
						.Invoke(new object[] {defaultEnvironment});
				stageActivatorClass.GetMethod("SetCookies")
					.Invoke(stageActivator, new[] {stageActivatorConfig.CookiesList()});

				dungeonLogic.AddStageActivator(stageActivator);
			}
		}

		private void CreateGates(Configs.DungeonConfig dungeonCfg, DungeonLogic dungeonLogic) {
			foreach (GateConfig gateConfig in dungeonCfg.ShowGateList()) {
				Type gateClass = Type.GetType(gateConfig.ClassName());
				Gates.GateController gateController = (Gates.GateController) gateClass
					.GetConstructor(new[] {typeof(int), typeof(Environment.Environment)})
					.Invoke(new object[] { gateConfig.Id(), defaultEnvironment});
				gateClass.GetMethod("SetCookies").Invoke(gateController, new[] {gateConfig.CookiesList()});

				dungeonLogic.AddGate(gateController);
			}
		}

		private void CreateStages(DungeonConfig dungeonCfg, DungeonLogic dungeonLogic) {
			foreach (StageConfig stageConfig in dungeonCfg.StageList()) {
				DefaultStage stage = new DefaultStage(defaultEnvironment);
				dungeonLogic.AddStage(stage);

				CreateGoals(stageConfig, stage);

				CreateLosingConditions(stageConfig, stage);

				CreateWaves(stageConfig, stage, dungeonLogic);
			}
		}

		private void CreateWaves(StageConfig stageConfig, DefaultStage stage, DungeonLogic dungeonLogic)
		{
			List<WaveConfig> waves = stageConfig.WaveList();
			for (int waveIndex = 0; waveIndex < waves.Count; waveIndex++)
			{
				WaveConfig w = waves[waveIndex];
				if (w.IsDisabled()) continue;

				DefaultWaveLogic waveLogic = new DefaultWaveLogic(waveIndex + 1);
				List<ChallengeConfig> challengeConfigs = w.ChallengeList();
				for (int challengeIndex = 0; challengeIndex < challengeConfigs.Count; challengeIndex++)
				{
					ChallengeConfig challengeConfig = challengeConfigs[challengeIndex];
					if (challengeConfig.IsDisabled()) continue;

					ActionsByLayer actionsByLayer = CreateActionsByLayer(challengeConfig.SpawnConfig().ActionConfigs(), dungeonLogic);
					Challenge challenge = CreateChallenge(challengeConfig, actionsByLayer.allActions, dungeonLogic);
//					challenge.Name = (waveIndex + 1) + "-" + (challengeIndex + 1);
					waveLogic.AddChallenge(challenge);

					foreach (IAction stageAction in actionsByLayer.stageActions)
					{
						stage.AddAction(stageAction);
					}

					foreach (IAction dungeonAction in actionsByLayer.dungeonActions)
					{
						dungeonLogic.AddAction(dungeonAction);
					}
				}

				stage.AddWave(waveLogic);
			}
		}

		private Challenge CreateChallenge(ChallengeConfig challengeConfig, List<IAction> actions, DungeonLogic dungeonLogic)
		{
			Trigger startTrigger = CreateTrigger(challengeConfig.StartTrigger());
			SpawnOverTime sot = CreateSpawnOverTime(challengeConfig.SpawnConfig());

			List<TrackerConfig> trackerConfigs = challengeConfig.SpawnConfig().TrackerConfigs();
			foreach (TrackerConfig trackerConfig in trackerConfigs)
			{
				if (!trackerConfig.IsActive()) continue;
						
				Tracker tracker = CreateTracker(trackerConfig);
				sot.AddTracker(tracker);
			}

			// List<ActionConfig> actionConfigs = challengeConfig.SpawnConfig().ActionConfigs();
			// foreach (ActionConfig actionConfig in actionConfigs)
			// {
			// 	if (actionConfig.IsDisabled()) continue;
			// 	
			// 	IAction action = CreateAction(actionConfig, dungeonLogic);
			// 	sot.AddAction(action);
			// }

			foreach (IAction action in actions)
			{
				sot.AddAction(action);
			}

			DefaultChallenge challenge = new DefaultChallenge(startTrigger, sot);

			return challenge;
		}

		private Trigger CreateTrigger(TriggerConfig triggerConfigConfig) {
			Type triggerClass = Type.GetType(triggerConfigConfig.ClassName());
			Trigger trigger = (Trigger)triggerClass.GetConstructor (Type.EmptyTypes).Invoke (new object[]{});
			trigger.SetEnv (defaultEnvironment);
			trigger.SetCookies (triggerConfigConfig.CookiesList ());
			return trigger;
		}

		private SpawnOverTime CreateSpawnOverTime(SpawnConfig spawnConfig) {
			SpawnOverTime sot = new SpawnOverTime();
			sot.SetEnv(defaultEnvironment);
			sot.SetHeroAndMonsterConfig(heroAndMonsterConfig);
			sot.SetCookies(spawnConfig.CookiesList());
			return sot;
		}

		private Tracker CreateTracker(TrackerConfig trackerConfig)
		{
			Type trackerClass = Type.GetType(trackerConfig.ClassName());
			Tracker tracker = (Tracker) trackerClass.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
			tracker.SetEnv(defaultEnvironment);
			tracker.SetCookies(trackerConfig.CookiesList());
			return tracker;
		}

		private ActionsByLayer CreateActionsByLayer(List<ActionConfig> actionConfigs, DungeonLogic dungeonLogic)
		{
			ActionsByLayer actionsByLayer = new ActionsByLayer();
			foreach (ActionConfig actionConfig in actionConfigs)
			{
				if (actionConfig.IsDisabled()) continue;
				
				IAction action = CreateAction(actionConfig, dungeonLogic);
				actionsByLayer.allActions.Add(action);

				switch (action.GetLayer())
				{
					case DungeonSpawnConfig.ActionLayer.Stage:
						actionsByLayer.stageActions.Add(action);
						break;
					case DungeonSpawnConfig.ActionLayer.Dungeon:
						actionsByLayer.dungeonActions.Add(action);
						break;
				}
			}

			return actionsByLayer;
		}

		private IAction CreateAction(ActionConfig actionConfig, DungeonLogic dungeonLogic)
		{
			Type actionClass = Type.GetType(actionConfig.ClassName());
			IAction action = (IAction) actionClass.GetConstructor(Type.EmptyTypes).Invoke(new object[] { });
			action.SetCookies(actionConfig.CookiesList());
			action.SetEnv(defaultEnvironment);
			action.SetDungeonLogic(dungeonLogic);
			return action;
		}

		private void CreateLosingConditions(StageConfig stageConfigConfig, DefaultStage stage) {
			foreach (LosingCondition losingConditionConfig in stageConfigConfig.LosingConditionList()) {
				Type losingConditionClass = Type.GetType(losingConditionConfig.ClassName());
				Stages.LosingConditions.LosingCondition losingCondition =
					(Stages.LosingConditions.LosingCondition) losingConditionClass
						.GetConstructor(new[] {typeof(Environment.Environment)})
						.Invoke(new object[] {defaultEnvironment});
				losingConditionClass.GetMethod("SetCookies")
					.Invoke(losingCondition, new[] {losingConditionConfig.CookiesList()});
				stage.AddLosingCondition(losingCondition);
			}
		}

		private void CreateGoals(StageConfig stageConfigConfig, DefaultStage stage) {
			foreach (GoalConfig goalConfig in stageConfigConfig.GoalList()) {
				Type goalClass = Type.GetType(goalConfig.ClassName());
				Stages.Goals.Goal goal = (Stages.Goals.Goal) goalClass
					.GetConstructor(new[] {typeof(Environment.Environment)})
					.Invoke(new object[] {defaultEnvironment});
				goalClass.GetMethod("SetCookies").Invoke(goal, new[] {goalConfig.CookiesList()});
				stage.AddGoal(goal);
			}
		}

		#region Subclass

		private class ActionsByLayer
		{
			public List<IAction> allActions = new List<IAction>();
			public List<IAction> stageActions = new List<IAction>();
			public List<IAction> dungeonActions = new List<IAction>();
		}

		#endregion
	}
}
