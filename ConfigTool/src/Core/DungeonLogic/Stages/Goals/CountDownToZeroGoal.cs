using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;

namespace Core.DungeonLogic.Stages.Goals {
	public class CountDownToZeroGoal : Goal {
		private float startNumberOfSecond;
		private Environment.Environment environment;

		private float lastTime;

		public CountDownToZeroGoal(float startNumberOfSecond, Environment.Environment environment) {
			new NotNullReference().Check(environment, "environment");

			this.startNumberOfSecond = startNumberOfSecond;
			this.environment = environment;
		}

		public bool IsAchieved() {
			return startNumberOfSecond >= 0;
		}

		public void Update(float dt) {
			if (startNumberOfSecond < 0) return;

			float currentTime = environment.ElapsedTime();
			startNumberOfSecond -= currentTime - lastTime;
			lastTime = currentTime;
		}

		public void OnAddedToStage(DefaultStage stage) {
		}

		public string Reason() {
			return "unknown";
		}
	}
}
