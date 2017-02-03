using Core.Utils;

namespace Core.Skills.Cooldowns {
	public class NonRecoverableCharge : Cooldown {
		private readonly int maxCharge;

		private int currentCharge;
		private AcceptWindow recastWindow;
		private float elapsed;
		private int maxRecast;
		private int currentRecast;
		private bool isCurrentRecastValueEverSet;

		public NonRecoverableCharge(int maxCharge) {
			this.maxCharge = maxCharge;
			currentCharge = maxCharge;
		}

		public void Start() {
			if (IsRecastable()) {
				ConsumeRecast();
				return;
			}

			currentRecast = maxRecast;
			currentCharge--;
		}

		public bool IsComplete() {
			return currentCharge > 0;
		}

		public void Update(float dt) {
			if (!IsComplete()) {
				elapsed += dt;
			}
		}

		public void Reset() {
			currentCharge = maxCharge;
		}

		public bool IsRecastable() {
			if (recastWindow == null) return false;
			if (currentRecast < 1) return false;
			return recastWindow.IsAccept(elapsed);
		}

		public int GetCurrentCharge()
		{
			return currentCharge;
		}

		public void SetRecastWindow(AcceptWindow aw) {
			recastWindow = aw;
		}

		public void SetMaxRecast(int value) {
			maxRecast = value;
			if (!isCurrentRecastValueEverSet) {
				isCurrentRecastValueEverSet = true;
				currentRecast = maxRecast;
			}
		}

		public int CurrentRecast => currentRecast;

		public void ConsumeRecast() {
			currentRecast--;
		}
	}
}