using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;

namespace Core.DungeonLogic.StageActivators {
	public class StageActivatorComponent : Component {
		private StageActivator stageActivator;

		public StageActivatorComponent(StageActivator stageActivator) {
			new NotNullReference().Check(stageActivator, "stageActivator");

			this.stageActivator = stageActivator;
		}

		public void StartUp() {
		}

		public void ShutDown() {
		}

		public void Start() {
		}

		public void Stop() {
		}

		public void Update(float dt) {
			stageActivator.Update(dt);
		}
	}
}
