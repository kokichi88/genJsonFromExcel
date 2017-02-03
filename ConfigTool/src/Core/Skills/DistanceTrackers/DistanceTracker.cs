using Core.Utils;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.DistanceTrackers {
	public class DistanceTracker : Loopable {
		private BaseEvent be;
		private readonly Skill skill;
		private Character caster;

		private DistanceTrackerAction distanceTrackerAction;
		private Vector3 lastPosition;
		private float traveledDistance;
		private float distanceInterval;
		private bool isInterrupted;
		private bool isFinished;
		private float elapsed;
		private float duration;

		public DistanceTracker(BaseEvent be, Skill skill, Character caster) {
			this.be = be;
			this.skill = skill;
			this.caster = caster;

			distanceTrackerAction = (DistanceTrackerAction) be.ShowAction();
			duration = FrameAndSecondsConverter._30Fps.FramesToSeconds(distanceTrackerAction.endFrame) - skill.Elapsed;
			lastPosition = caster.Position();
			distanceInterval = distanceTrackerAction.distance;
		}

		public void Update(float dt) {
			elapsed += dt;
			Vector3 casterPos = caster.Position();
			traveledDistance += (casterPos - lastPosition).magnitude;
			if (traveledDistance >= distanceInterval) {
				skill.TriggerEventWithId(distanceTrackerAction.eventId);
				traveledDistance = 0;
			}

			lastPosition = casterPos;

			if (elapsed >= duration) {
				isFinished = true;
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return isFinished || isInterrupted;
		}
	}
}