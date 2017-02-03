using System;
using Core.Utils;
using UnityEngine;

namespace Core.Skills.Cooldowns {
	public class TimeCooldown : Cooldown {
		private float duration;
		private readonly float scale = 1;

		private float remaining;
		private float ratio = 1;
		private AcceptWindow recastWindow;
		private int maxRecast;
		private int currentRecast;
		private bool isCurrentRecastValueEverSet;
		private float elapsed;

		public TimeCooldown(float duration) {
			this.duration = duration;
		}

		public TimeCooldown(float duration, float scale = 1) {
			this.duration = duration;
			this.scale = scale;
		}

		public void Start() {
			if (IsRecastable()) {
				if (0 < remaining && remaining <= Duration()) {
					ConsumeRecast();
					return;
				}
			}

			currentRecast = maxRecast;
			remaining = Duration();
			isCurrentRecastValueEverSet = false;
		}

		public float RemainingPercentage() {
			return remaining / Duration();
		}

		public float Remaining() {
			return remaining;
		}

		public float Duration() {
			return duration * ratio;
		}

		public void Update(float dt) {
			if (IsComplete()) return;

			elapsed += dt;
			remaining -= dt / scale;
		}

		public bool IsComplete() {
			return remaining <= 0;
		}

		public void Reset() {
			remaining = 0;
		}

		public bool IsRecastable() {
			if (recastWindow == null) return false;
			if (remaining <= 0) return false;
			if (currentRecast < 1) return false;
			return recastWindow.IsAccept(elapsed);
		}

		public void AdjustRemainingWithRatio(float value) {
			value = Mathf.Clamp01(value);
			remaining *= value;
		}

		public void AdjustDurationWithRatio(float value) {
			ratio = Math.Max(0, value);
		}

		public void ReduceRemainingTimeBy(float value) {
			remaining -= value;
		}

		public void SetRecastWindow(AcceptWindow aw) {
			recastWindow = aw;
		}

		public void SetMaxRecast(int value) {
			maxRecast = value;
			if (!isCurrentRecastValueEverSet) {
				isCurrentRecastValueEverSet = true;
				currentRecast = maxRecast;
				// DLog.Log("debug set current recast " + currentRecast);
			}
		}

		public int CurrentRecast => currentRecast;

		public void ConsumeRecast() {
			currentRecast--;
			// DLog.Log("debug consume recast, current recast " + currentRecast);
		}
	}
}