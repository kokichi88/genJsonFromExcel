using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Core.Utils;
using UnityEngine;

namespace Assets.Scripts.Core.Scheduling {
	public class SimTimeEngine {
		private float timePerFrame;
		private float timeLast;
		private List<SimTimeObserver> observers;
		private MutableIterator miter;
		private float scale;

		public SimTimeEngine(float timePerFrame) {
			this.timePerFrame = timePerFrame;
			this.ScaleTime(1f);
			this.timeLast = this.Now();
			this.observers = new List<SimTimeObserver>();
			this.miter = new MutableIterator();
		}

		public void RegisterSimTimeObserver(SimTimeObserver observer) {
			if (observer == null || this.observers.IndexOf(observer) >= 0)
				return;
			this.observers.Add(observer);
		}

		public void UnregisterSimTimeObserver(SimTimeObserver observer) {
			int num = this.observers.IndexOf(observer);
			if (num < 0)
				return;
			this.observers.RemoveAt(num);
			this.miter.OnRemove(num);
		}

		public void UnregisterAll() {
			this.observers.Clear();
			this.miter.Reset();
		}

		public void ScaleTime(float scale) {
			if (IsPaused() && scale > 0) {
				timeLast = Now();
			}
			this.scale = scale;
		}

		public bool IsPaused() {
			return (int) this.scale == 0;
		}

		public void OnUpdate() {
			float curTime = this.Now();
			float scaledTimePerFrame = (timePerFrame * this.scale);
			while (timeLast < curTime) {
				this.miter.Init((ICollection) this.observers);
				while (this.miter.Active()) {
					this.observers[this.miter.Index].OnSimTime(scaledTimePerFrame);
					this.miter.Next();
				}
				this.miter.Reset();
				timeLast += scaledTimePerFrame;
			}
		}

		private float Now() {
			float num = Time.time ;
			return num;
		}
	}
}
