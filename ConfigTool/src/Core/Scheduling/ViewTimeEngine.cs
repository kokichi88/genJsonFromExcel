using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core.Utils;
using UnityEngine;

namespace Assets.Scripts.Core.Scheduling {
	public class ViewTimeEngine {
		private const int MAX_PHYS_FRAMES_PER_VIEW_FRAME = 5;
		private float timePerFrame;
		private float timeLast;
		private List<ViewPhysicsTimeObserver> physTimeObservers;
		private List<ViewFrameTimeObserver> frameTimeObservers;
		private List<ViewClockTimeObserver> clockTimeObservers;
		private MutableIterator physMiter;
		private MutableIterator frameMiter;
		private MutableIterator clockMiter;

		public ViewTimeEngine(float timePerFrame) {
			this.timePerFrame = timePerFrame;
			this.timeLast = this.Now();
			this.physTimeObservers = new List<ViewPhysicsTimeObserver>();
			this.frameTimeObservers = new List<ViewFrameTimeObserver>();
			this.clockTimeObservers = new List<ViewClockTimeObserver>();
			this.physMiter = new MutableIterator();
			this.frameMiter = new MutableIterator();
			this.clockMiter = new MutableIterator();
		}

		public void RegisterPhysicsTimeObserver(ViewPhysicsTimeObserver observer) {
			if (observer == null || this.physTimeObservers.IndexOf(observer) >= 0)
				return;
			this.physTimeObservers.Add(observer);
		}

		public void UnregisterPhysicsTimeObserver(ViewPhysicsTimeObserver observer) {
			int num = this.physTimeObservers.IndexOf(observer);
			if (num < 0)
				return;
			this.physTimeObservers.RemoveAt(num);
			this.physMiter.OnRemove(num);
		}

		public void RegisterFrameTimeObserver(ViewFrameTimeObserver observer) {
			if (observer == null || this.frameTimeObservers.IndexOf(observer) >= 0)
				return;
			this.frameTimeObservers.Add(observer);
		}

		public void UnregisterFrameTimeObserver(ViewFrameTimeObserver observer) {
			int num = this.frameTimeObservers.IndexOf(observer);
			if (num < 0)
				return;
			this.frameTimeObservers.RemoveAt(num);
			this.frameMiter.OnRemove(num);
		}

		public void RegisterClockTimeObserver(ClockTimeObserver observer, float tickSize) {
			if (observer == null)
				return;
			float accumulator = 0.0f;
			bool flag = false;
			int index = 0;
			for (int count = this.clockTimeObservers.Count; index < count; ++index) {
				ViewClockTimeObserver clockTimeObserver = this.clockTimeObservers[index];
				if (clockTimeObserver.Observer == observer)
					return;
				if (!flag && (double) clockTimeObserver.TickSize == (double) tickSize) {
					accumulator = clockTimeObserver.Accumulator;
					flag = true;
				}
			}
			if (!flag)
				accumulator = MathUtils.FloatMod(this.timeLast, tickSize);
			this.clockTimeObservers.Add(new ViewClockTimeObserver(observer, tickSize, accumulator));
		}

		public void UnregisterClockTimeObserver(ClockTimeObserver observer) {
			int index = 0;
			for (int count = this.clockTimeObservers.Count; index < count; ++index) {
				if (this.clockTimeObservers[index].Observer == observer) {
					this.clockTimeObservers.RemoveAt(index);
					this.clockMiter.OnRemove(index);
					break;
				}
			}
		}

		public void UnregisterAll() {
			this.physTimeObservers.Clear();
			this.frameTimeObservers.Clear();
			this.clockTimeObservers.Clear();
			this.physMiter.Reset();
			this.frameMiter.Reset();
			this.clockMiter.Reset();
		}

		public void OnUpdate() {
			float num1 = this.Now();
			float dt1 = num1 - this.timeLast;
			this.timeLast = num1;
			float num2 = dt1;
			int num3 = 0;
			while ((double) num2 > 0.0) {
				float dt2 = (double) num2 <= (double) this.timePerFrame ? num2 : this.timePerFrame;
				this.physMiter.Init((ICollection) this.physTimeObservers);
				while (this.physMiter.Active()) {
					this.physTimeObservers[this.physMiter.Index].OnViewPhysicsTime(dt2);
					this.physMiter.Next();
				}
				this.physMiter.Reset();
				if (++num3 != MAX_PHYS_FRAMES_PER_VIEW_FRAME)
					num2 -= dt2;
				else
					break;
			}
			this.frameMiter.Init((ICollection) this.frameTimeObservers);
			while (this.frameMiter.Active()) {
				this.frameTimeObservers[this.frameMiter.Index].OnViewFrameTime(dt1);
				this.frameMiter.Next();
			}
			this.frameMiter.Reset();
			this.clockMiter.Init((ICollection) this.clockTimeObservers);
			while (this.clockMiter.Active()) {
				ViewClockTimeObserver clockTimeObserver = this.clockTimeObservers[this.clockMiter.Index];
				float num4 = clockTimeObserver.Accumulator + dt1;
				float tickSize = clockTimeObserver.TickSize;
				while ((double) num4 >= (double) tickSize) {
					clockTimeObserver.Observer.OnViewClockTime(tickSize);
					num4 -= tickSize;
				}
				clockTimeObserver.Accumulator = num4;
				this.clockMiter.Next();
			}
			this.clockMiter.Reset();
		}

		private float Now() {
			return Time.time;
		}
	}
}
