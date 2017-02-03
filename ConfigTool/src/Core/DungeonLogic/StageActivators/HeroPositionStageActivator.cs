using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;

namespace Core.DungeonLogic.StageActivators {
	public class HeroPositionStageActivator : StageActivator {
		private Environment.Environment environment;
		private float activationX;

		private bool actived = false;
		private NotNullReference notNullReference = new NotNullReference();

		public HeroPositionStageActivator(Environment.Environment environment) {
			notNullReference.Check(environment, "environment");

			this.environment = environment;
		}

		public void SetCookies(IEnumerable<string> cookies) {
			activationX = Convert.ToSingle(cookies.ElementAt(0));
		}

		public bool IsActive() {
			return actived;
		}

		public void Update(float dt) {
			if(actived) return;

			if (environment.Character().Position().x > activationX) {
				actived = true;
			}
		}
	}
}
