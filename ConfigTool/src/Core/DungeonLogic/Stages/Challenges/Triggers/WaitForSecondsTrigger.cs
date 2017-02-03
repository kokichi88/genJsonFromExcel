using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.DungeonLogic.Spawn;
using Checking;
using UnityEngine;
using Utils;

namespace Core.DungeonLogic.Stages.Challenges.Triggers {
	public class WaitForSecondsTrigger : Trigger {
		private float waitTimeInSeconds;
		private float waitTimeAmplitudeInSeconds;
		private int waitTimeDensity;

		private NotNullReference notNullReference = new NotNullReference();

		public WaitForSecondsTrigger() {
			
		}

		public WaitForSecondsTrigger(float waitTimeInSeconds) {
			this.waitTimeInSeconds = waitTimeInSeconds;
		}

		public WaitForSecondsTrigger(Spawner gameObjectSpawner) : this(0) {
		}

		public void SetEnv (Environment.Environment env) {
			
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");

			waitTimeInSeconds = Convert.ToSingle(cookies.ElementAt(0));
//			waitTimeAmplitudeInSeconds = Convert.ToSingle(cookies.ElementAt(1));
//			waitTimeDensity = Convert.ToInt32(cookies.ElementAt(2));
//
//			float original = waitTimeInSeconds;
//			waitTimeInSeconds = Random(waitTimeInSeconds, waitTimeAmplitudeInSeconds, waitTimeDensity);
//			DLog.Log("original " + original + " new " + waitTimeInSeconds);
		}

		public string UnfinishedReason() {
			return string.Format(
				"Remaining wait time of {0} is >= 0", waitTimeInSeconds
			);
		}

		public bool IsFinished() {
			return waitTimeInSeconds < 0;
		}

		public void Update(float dt, int waveOrder) {
			if(IsFinished()) return;

			waitTimeInSeconds -= dt;
//			DLog.Log("wait time " + waitTimeInSeconds);
		}

		public override string ToString() {
			return string.Format("{0}: \n\t\t\t\t\tWaitTimeInSeconds: {1}", GetType().Name, waitTimeInSeconds);
		}

		private float Random(float baseValue, float amplitude, int density) {
			float left = baseValue - amplitude / 2f;
			float[] pos = new float[density];
			for (int i = 0; i < pos.Length; i++) {
				float delta = 0;
				if (amplitude == 1) {
					delta = amplitude / 2;
				}
				else {
					delta = amplitude / (density - 1);
				}
				if (density == 1) {
					pos[i] = left + delta;
				}
				else {
					pos[i] = left + i * delta;
				}
			}
			int index = BattleUtils.RandomRangeInt(0, pos.Length);
			return pos[index];
		}
	}
}
