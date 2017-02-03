using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.DungeonLogic.Environment.Monster;
using Checking;
using Core.Commons;
using UnityEngine;

namespace Core.DungeonLogic.Stages.Goals {
	public class KillEnemyOfSpecificIdGoal : Goal{
		private Environment.Environment environment;
		private CharacterId enemyId;
		private int enemyCount;

		private NotNullReference notNullReference = new NotNullReference();
		private List<Monster> recognizedDeadMonster = new List<Monster>();
		private float lastTimeCheck;
		private bool lastTimeCheckInited = false;

		public KillEnemyOfSpecificIdGoal(Environment.Environment environment) {
			this.environment = environment;
		}

		public bool IsAchieved() {
			return recognizedDeadMonster.Count >= enemyCount;
		}

		public void Update(float dt) {
			if(IsAchieved()) return;

			if (!lastTimeCheckInited) {
				lastTimeCheckInited = true;
				lastTimeCheck = environment.ElapsedTime();
			}

			foreach (Monster monster in environment.DeadMonstersSoFar()) {
				if (monster.DeadTime() < lastTimeCheck) continue;
				if (recognizedDeadMonster.Contains(monster)) continue;
				if (!monster.CharacterId().Equals(enemyId)) continue;

				recognizedDeadMonster.Add(monster);
				//DLog.Log("Recognize dead monster " + monster);
			}
		}

		public void OnAddedToStage(DefaultStage stage) {
		}

		public string Reason() {
			return string.Format(
				"Kill count of monster of id {0} is {1}/{2}",
				enemyId, recognizedDeadMonster.Count, enemyCount
			);
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");

			enemyId = new CharacterId(cookies.ElementAt(0));
			enemyCount = Convert.ToInt32(cookies.ElementAt(1));
		}
	}
}
