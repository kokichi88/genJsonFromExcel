using UnityEngine;

namespace Core.Utils {
	public class AttackStateWindow {
		private AcceptWindow aw;

		public AttackStateWindow(float start, float end) {
			aw = new AcceptWindow(start, end);
		}

		public virtual bool IsTransitionAvailable(float time) {
			return aw.IsAccept(time);
		}

		public virtual void StartSoonerBy(float value) {
			//DLog.Log("AttackStateWindow:StartSoonerBy " + value);
			aw.StartSoonerBy(value);
		}

		public virtual void ReturnToOriginalValue() {
			//DLog.Log("AttackStateWindow:ReturnToOriginalValue");
			aw.ReturnToOriginalValue();
		}

		public virtual float Start() {
			return aw.Start();
		}

		public float End() {
			return aw.End();
		}
	}
}