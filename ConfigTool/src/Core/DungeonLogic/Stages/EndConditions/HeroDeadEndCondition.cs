using System.Collections.Generic;
using Core.DungeonLogic.Environment.Character;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;

namespace Core.DungeonLogic.Stages.EndConditions
{
	public class HeroDeadEndCondition : LosingCondition, Goal
	{
		private readonly Environment.Environment environment;
		private Character character;

		public HeroDeadEndCondition(Environment.Environment environment)
		{
			this.environment = environment;
		}

		public void SetCookies(IEnumerable<string> cookies)
		{
		}

		public bool IsMet()
		{
			if (environment.Character() == null) return false;

			return environment.Character().IsDead();
		}

		public bool IsAchieved()
		{
			return IsMet();
		}

		public void Update(float dt)
		{
		}

		public void OnAddedToStage(DefaultStage stage)
		{
			
		}

		public string Reason()
		{
			return string.Empty;
		}

		public override string ToString()
		{
			return string.Format("{0}", GetType().Name);
		}
	}
}
