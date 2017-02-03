using System.Collections.Generic;
using Checking;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;

namespace Core.DungeonLogic.Stages.EndConditions
{
	public class KillAllEnemyEndCondition : Goal, LosingCondition
	{
		private readonly Environment.Environment environment;
		private DefaultStage stage;

		private float interval = 0.5f;
		private float elapsed = 0;
		private bool achieved = false;

		public KillAllEnemyEndCondition(Environment.Environment environment)
		{
			new NotNullReference().Check(environment, "environment is null");

			this.environment = environment;
		}

		public bool IsAchieved()
		{
			return achieved;
		}

		public bool IsMet()
		{
			return IsAchieved();
		}

		public void Update(float dt)
		{
			elapsed += dt;
			if (elapsed >= interval)
			{
				elapsed -= interval;
				if (!achieved
				    && stage.AreAllWavesFinished()
				    && environment.AreAllBeatableMonstersFromCurrentStageDead())
				{
					achieved = true;
				}
			}
		}

		public void OnAddedToStage(DefaultStage stage)
		{
			new NotNullReference().Check(stage, "stage is null");

			this.stage = stage;
		}

		public string Reason()
		{
			if (!stage.AreAllWavesFinished())
			{
				return "All waves of stage is not finished";
			}

			if (!environment.AreAllBeatableMonstersFromCurrentStageDead())
			{
				return "There are beatable monsters from current stage alive";
			}

			return "Unknown";
		}

		public void SetCookies(IEnumerable<string> cookies)
		{
		}
	}
}