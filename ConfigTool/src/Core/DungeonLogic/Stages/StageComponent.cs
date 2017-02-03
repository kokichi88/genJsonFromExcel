using Assets.Scripts.Core.Utils;
using Checking;

namespace Core.DungeonLogic.Stages {
	public class StageComponent : Component {
		private DefaultStage stage;
		private bool isRunning;

		public StageComponent(DefaultStage stage) {
			new NotNullReference().Check(stage, "stage");

			this.stage = stage;
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

			stage.Update(dt);
		}
	}
}
