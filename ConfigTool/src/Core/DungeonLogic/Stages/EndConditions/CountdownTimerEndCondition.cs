using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;
using Core.DungeonLogic.Stages.Goals;

namespace Core.DungeonLogic.Stages.LosingConditions
{
	public class CountdownTimerEndCondition : LosingCondition, Goal
	{
		private float seconds;
		private Environment.Environment environment;

		private float lastTime;
		private bool lastTimeInited;
		private NotNullReference notNullReference = new NotNullReference();

		public CountdownTimerEndCondition(float seconds, Environment.Environment environment)
		{
			notNullReference.Check(environment, "environment");

			this.seconds = seconds;
			this.environment = environment;
		}

		public CountdownTimerEndCondition(Environment.Environment environment) : this(0, environment)
		{
		}

		public void SetCookies(IEnumerable<string> cookies)
		{
			notNullReference.Check(cookies, "cookies");

			seconds = Convert.ToSingle(cookies.ElementAt(0));
		}

		public bool IsMet()
		{
			return seconds < 0;
		}

		public bool IsAchieved()
		{
			return IsMet();
		}

		public void Update(float dt)
		{
			if (seconds < 0) return;

			if (!lastTimeInited)
			{
				lastTime = environment.ElapsedTime();
				lastTimeInited = true;
			}

			float currentTime = environment.ElapsedTime();
			seconds -= currentTime - lastTime;
			lastTime = currentTime;
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
			return $"{GetType().Name}\n\t\t\tSeconds: {seconds}";
		}
	}
}
