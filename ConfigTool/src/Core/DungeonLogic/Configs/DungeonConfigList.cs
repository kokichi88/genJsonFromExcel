using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Config;
using Core.Commons;
using UnityEngine;

namespace Core.DungeonLogic.Configs {
	public interface DungeonConfigList {
		DungeonConfig GetDungeonById(int dungeonId);

		GateConfig GetGateById(int gateId);

		StageActivatorConfig GetStageActivatorById(int stageActivatorId);
	}

	public interface DungeonConfig {
		int Id();

		IEnumerable<StageConfig> StageList();

		List<GateConfig> ShowGateList();

		IEnumerable<StageActivatorConfig> StageActivatorList();
	}

	public interface StageConfig {
		int Id();

		List<GoalConfig> GoalList();

		List<LosingCondition> LosingConditionList();

		List<WaveConfig> WaveList();

		int WaveCount();
	}

	public interface GoalConfig {
		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface LosingCondition {
		string Name();

		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface ChallengeConfig
	{
		bool IsDisabled();
		TriggerConfig StartTrigger();
		SpawnConfig SpawnConfig();
	}

	public interface TriggerConfig {
		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface SpawnConfig {
		List<string> CookiesList();

		List<TrackerConfig> TrackerConfigs();

		List<ActionConfig> ActionConfigs();
	}
	
	public interface TrackerConfig
	{
		bool IsActive();
		
		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface ActionConfig
	{
		bool IsDisabled();
		
		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface GateConfig {
		int Id();

		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface StageActivatorConfig {
		int Id();

		string ClassName();

		IEnumerable<string> CookiesList();
	}

	public interface WaveConfig
	{
		bool IsDisabled();

		int Id();

		List<ChallengeConfig> ChallengeList();
	}
}
