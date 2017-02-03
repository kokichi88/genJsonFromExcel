using Core.Utils;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Timers {
	public class Timer : Loopable {
		private readonly BaseEvent be;
		private readonly Skill skill;

		private float elapsed;
		private TimerAction ta;
		private float waitTime;
		private bool isInterrupted;
		private int count;
		private float intervalElapsed;
		private bool triggerFirstTime;
		private float interval;

		public Timer(BaseEvent be, Skill skill) {
			this.be = be;
			this.skill = skill;
			ta = (TimerAction) be.ShowAction();
			waitTime = FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(ta.frame);
			interval = FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(ta.interval);
		}

		public void Update(float dt) {
			elapsed += dt;
			if (elapsed >= waitTime) {
				intervalElapsed += dt;
			}
		}

		public void LateUpdate(float dt) {
			if (elapsed >= waitTime) {
				if (!triggerFirstTime) {
					triggerFirstTime = true;
					skill.TriggerEventWithId(ta.eventId);
				}

				if (ta.repeat && intervalElapsed >= interval) {
					count++;
					intervalElapsed -= interval;
					skill.TriggerEventWithId(ta.eventId);
				}
			}
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			if (ta.repeat) {
				return count >= ta.count || isInterrupted;
			}
			else {
				return elapsed >= waitTime || isInterrupted;
			}
		}
	}
}