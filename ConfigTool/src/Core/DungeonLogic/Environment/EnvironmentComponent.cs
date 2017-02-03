using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.DungeonLogic.Environment {
	public class EnvironmentComponent : Component {
		private DefaultDungeonEnvironment defaultEnvironment;
		private bool isRunning = false;

		public EnvironmentComponent(DefaultDungeonEnvironment defaultEnvironment) {
			this.defaultEnvironment = defaultEnvironment;
		}

		public void StartUp() {
		}

		public void ShutDown() {
		}

		public void Start() {
			isRunning = true;
		}

		public void Stop() {
			isRunning = false;
		}

		public void Update(float dt) {
			if (!isRunning) {
				return;
			}

			defaultEnvironment.Elapse(dt);
		}
	}
}
