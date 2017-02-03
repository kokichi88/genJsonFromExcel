using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Core.Utils;
using Core.DungeonLogic.Environment.Monster;
using Checking;
using UnityEngine;

namespace Core.DungeonLogic.Stages.Goals {
	public class KillAllEnemyGoal : Goal {
		private int enemyCount;
		private Environment.Environment environment;

		private List<Monster> recognizedDeadMonster = new List<Monster>();
		private float lastTimeCheck;
		private bool lastTimeCheckInited = false;
		private NotNullReference notNullReference = new NotNullReference();

		public KillAllEnemyGoal(int enemyCount, Environment.Environment environment) {
			notNullReference.Check(environment, "environment");

			this.enemyCount = enemyCount;
			this.environment = environment;
		}

		public KillAllEnemyGoal(Environment.Environment environment) : this(0, environment) {
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");

			enemyCount = Convert.ToInt32(cookies.ElementAt(0));
			//DLog.Log("Kill " + enemyCount + " enemies");
		}

		public bool IsAchieved() {
			return recognizedDeadMonster.Count >= enemyCount;
		}

		public void Update(float dt) {
			if (!lastTimeCheckInited) {
				lastTimeCheckInited = true;
				lastTimeCheck = environment.ElapsedTime();
			}
			if(recognizedDeadMonster.Count >= environment.DeadMonstersSoFar().Count()) return;

			foreach (Monster monster in environment.DeadMonstersSoFar()) {
				if(monster.DeadTime() < lastTimeCheck) continue;
				if (recognizedDeadMonster.Contains(monster)) continue;

				recognizedDeadMonster.Add(monster);
				//DLog.Log("Recognize dead monster " + monster);
			}
		}

		public void OnAddedToStage(DefaultStage stage) {
		}

		public string Reason() {
			return string.Format(
				"Dead monster count is {0}/{1}", recognizedDeadMonster.Count, enemyCount
			);
		}

		public override string ToString() {
			return string.Format("{0}\n\t\t\tEnemyCount: {2}", GetType().Name, enemyCount);
		}
	}
}
