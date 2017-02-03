using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Checking;

namespace Core.DungeonLogic.Gates {
	public class GateComponent : Component {
		private GateController gateController;

		public GateComponent(GateController gateController) {
			new NotNullReference().Check(gateController, "gate");

			this.gateController = gateController;
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
			gateController.Update(dt);
		}
	}
}
