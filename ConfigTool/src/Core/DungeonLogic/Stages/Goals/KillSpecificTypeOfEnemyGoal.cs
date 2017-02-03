using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Core.Utils;
using Core.DungeonLogic.Environment.Monster;
using Checking;

namespace Core.DungeonLogic.Stages.Goals {
	public class KillSpecificTypeOfEnemyGoal : Goal {
		private MonsterType monsterType;
		private int killCountRequirement;
		private Environment.Environment environment;

		private int killCountSoFar;
		private float lastTimeCheck;
		private NotNullReference notNullReference = new NotNullReference();

		public KillSpecificTypeOfEnemyGoal(MonsterType monsterType, int killCountRequirement, Environment.Environment environment) {
			notNullReference.Check(monsterType, "monsterType");
			notNullReference.Check(environment, "environment");

			this.monsterType = monsterType;
			this.killCountRequirement = killCountRequirement;
			this.environment = environment;
		}

		public KillSpecificTypeOfEnemyGoal(Environment.Environment environment) : this(MonsterType.Undefined, 0, environment) {
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");

			monsterType = (MonsterType) Enum.Parse(typeof(MonsterType), cookies.ElementAt(0));
			killCountRequirement = Convert.ToInt32(cookies.ElementAt(1));
		}

		public bool IsAchieved() {
			return killCountSoFar >= killCountRequirement;
		}

		public void Update(float dt) {
			foreach (Monster monster in environment.DeadMonstersSoFar()) {
				if (monster.Type() != monsterType) continue;
				if (monster.DeadTime() <= lastTimeCheck) continue;

				killCountSoFar++;
				lastTimeCheck = environment.ElapsedTime();
			}
		}

		public void OnAddedToStage(DefaultStage stage) {
		}

		public string Reason() {
			return string.Format(
				"Killed count of monster of type {0} is {1}/{2}",
				monsterType, killCountSoFar, killCountRequirement
			);
		}

		public override string ToString() {
			return string.Format("{0}\n\t\t\tMonsterType: {2}, KillCountRequirement: {3}", GetType().Name, monsterType, killCountRequirement);
		}
	}
}
