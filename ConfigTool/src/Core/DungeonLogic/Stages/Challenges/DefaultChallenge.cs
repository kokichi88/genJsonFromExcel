using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.DungeonLogic.Stages.Challenges.Triggers;
using Checking;
using Core.Commons;
using UnityEngine;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Challenges {
	public class DefaultChallenge : Challenge {
		private static string unknown = "Unknown";

		private Trigger startTrigger;
		private SpawnOverTime spawnOverTime;

		private NotNullReference notNullReference = new NotNullReference();
		private string name;

		public DefaultChallenge(Trigger startTrigger, SpawnOverTime spawnOverTime) {
			notNullReference.Check(startTrigger, "startTrigger");
			notNullReference.Check(spawnOverTime, "spawnOverTime");

			this.startTrigger = startTrigger;
			this.spawnOverTime = spawnOverTime;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public bool IsFinished() {
			return startTrigger.IsFinished() && spawnOverTime.IsFinished();
		}

		public void Update(float dt, int waveOrder) {
			/*if (name != null) {
				DLog.Log("Challenge " + name + " update");
			}*/
			if (!startTrigger.IsFinished()) {
				startTrigger.Update(dt, waveOrder);
			}
			else {
				spawnOverTime.Update(dt, waveOrder);
			}
		}

		public string UnfinishedReason() {
			if (!startTrigger.IsFinished()) {
				return startTrigger.UnfinishedReason();
			}
			if (!spawnOverTime.IsFinished()) {
				return spawnOverTime.UnfinishedReason();
			}
			return unknown;
		}

		public override string ToString() {
			return string.Format("{0}\n\t\t\tStartTrigger: \n\t\t\t\t{1}, \n\t\t\tMainTrigger: \n\t\t\t\t{2}", GetType().Name, startTrigger, spawnOverTime);
		}

		public SsarTuple<CharacterId, int> ShowMonsterIdAndSpawnCount() {
			return spawnOverTime.ShowMonsterIdAndSpawnCount();
		}
	}
}
