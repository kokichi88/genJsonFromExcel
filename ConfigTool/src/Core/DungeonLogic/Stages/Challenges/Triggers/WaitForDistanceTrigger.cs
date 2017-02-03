using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.DungeonLogic.Spawn;
using Checking;
using UnityEngine;
using Core.DungeonLogic.Environment.Character;

namespace Core.DungeonLogic.Stages.Challenges.Triggers {
	public class WaitForDistanceTrigger : Trigger {
		const float COOLDOWN = 0.1f;
		private float radius;
		private NotNullReference notNullReference = new NotNullReference();
		private Vector2 spawnerPos = Vector2.zero;
		private Character ch;
		private float cooldownCount = COOLDOWN;
		private bool isFinished = false;
		private Environment.Environment environment;

		public WaitForDistanceTrigger() {
			
		}

		public WaitForDistanceTrigger(float radius) {
			this.radius = radius;
		}

	
		public void SetEnv (Environment.Environment env) {
			environment = env;
			ch = env.Character ();
		}

		public void SetCookies(IEnumerable<string> cookies) {
			notNullReference.Check(cookies, "cookies");
			radius = Convert.ToSingle(cookies.ElementAt(0));
			float x = Convert.ToSingle(cookies.ElementAt(1));
			float y = Convert.ToSingle(cookies.ElementAt(2));

			spawnerPos = new Vector2 (x, y);
		}

		public string UnfinishedReason() {
			return "Hero is too far";
		}

		public bool IsFinished() {
			if (ch == null) return false;
			
			if (!isFinished) {
				isFinished = ch.Position().x >= (spawnerPos.x - radius);
			}
			return isFinished;
		}

		public void Update(float dt, int waveOrder) {
			if (ch == null) {
				ch = environment.Character();
				if(ch == null) return;
			}
			if (cooldownCount > 0) {
				cooldownCount -= dt;
				if (cooldownCount <= 0) {
					cooldownCount = COOLDOWN;
					IsFinished();
				}
					
			}
//			DLog.Log("wait time " + waitTimeInSeconds);
		}

		public override string ToString() {
			return string.Format("{0}: \n\t\t\t\t\tWaitForDistanceTrigger: {1}", GetType().Name, radius);
		}
	}
}
