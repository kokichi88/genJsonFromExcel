using System.Collections.Generic;
using Core.Utils;
using UnityEngine;

namespace Core.Skills.Cooldowns {
	public class RecoverableCharge : Cooldown {
		private int max_charge = 3;

		private float duration;

		private int currentCharge;
		private TimeCooldown timeCooldown;
		private Queue<TimeCooldown> timeCooldowns = new Queue<TimeCooldown>();

		private bool adjust;
		private float ratio;
		private float durationRatio = 1;
		private int extraMaxCharge = 0;

		public RecoverableCharge(float duration, int max_charge) {
			this.duration = duration;
			this.max_charge = max_charge;
			currentCharge = max_charge;
		}

		public void Start() {
			if (IsRecastable()) {
				if (timeCooldown != null) {
					timeCooldown.ConsumeRecast();
				}
				return;
			}

			currentCharge--;

			//DLog.Log("Current charge: " + currentCharge);
			TimeCooldown cooldown = new TimeCooldown(Duration());
			timeCooldowns.Enqueue(cooldown);
			cooldown.Start();
		}

		public float RemainingPercentage() {
			if (timeCooldown != null) {
				return timeCooldown.RemainingPercentage();
			}

			return 0f;
		}

		public float Remaining() {
			if (timeCooldown != null) {
				return timeCooldown.Remaining();
			}

			return 0f;
		}

		public float Duration() {
			return duration * durationRatio;
		}

		public bool IsComplete() {
			return currentCharge > 0;
		}

		public void Update(float dt) {
			if (timeCooldown == null) {
				if (timeCooldowns.Count > 0) {
					timeCooldown = timeCooldowns.Dequeue();
				}
			}

			if (timeCooldown != null) {
				if (adjust) {
					adjust = false;
					timeCooldown.AdjustRemainingWithRatio(ratio);
				}

				timeCooldown.Update(dt);
				if (timeCooldown.IsComplete()) {
					currentCharge++;
					//DLog.Log("Recover charge, current: " + currentCharge);
					timeCooldown = null;
				}
			}
		}

		public void Reset() {
			currentCharge = ShowMaxCharge();
			timeCooldowns.Clear();
		}

		private int ShowMaxCharge() {
			return max_charge + extraMaxCharge;
		}

		public bool IsRecastable() {
			foreach (TimeCooldown timeCooldown in timeCooldowns) {
				if (timeCooldown.IsRecastable()) return true;
			}

			if (timeCooldown != null) {
				if (timeCooldown.IsRecastable()) return true;
			}

			return false;
		}

		public int CurrentCharge {
			get { return currentCharge; }
		}

		public void ConsumeAllCharges() {
			int chargeLeft = currentCharge;
			for (int i = 0; i < chargeLeft; i++) {
				Start();
			}
		}

		public void AdjustRemainingWithRatio(float value) {
			adjust = true;
			ratio = value;
		}

		public void AdjustDurationWithRatio(float newValue) {
			durationRatio = Mathf.Max(0, newValue);
			foreach (TimeCooldown tc in timeCooldowns) {
				tc.AdjustDurationWithRatio(newValue);
				tc.AdjustRemainingWithRatio(newValue);
			}
		}

		public int GetCurrentCharge()
		{
			return currentCharge;
		}

		public void ReduceRemainingTimeBy(float value) {
			if (timeCooldown != null) {
				timeCooldown.ReduceRemainingTimeBy(value);
			}
		}

		public void SetRecastWindow(AcceptWindow aw) {
			if (timeCooldown != null) {
				timeCooldown.SetRecastWindow(aw);
			}
		}

		public void SetMaxRecast(int value) {
			if (timeCooldown != null) {
				timeCooldown.SetMaxRecast(value);
			}
		}

		public int CurrentRecast {
			get {
				if (timeCooldown != null) return timeCooldown.CurrentRecast;

				return 0;
			}
		}

		public void IncreaseMaxChargeBy(int count) {
			extraMaxCharge += count;
		}
	}
}