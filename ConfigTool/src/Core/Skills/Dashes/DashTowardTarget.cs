using System;
using System.Collections.Generic;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using UnityEngine;
using Environment = Core.Skills.Environment;

namespace Core.Skills.Dashes {
	public class DashTowardTarget : Loopable {
		private BaseEvent ef;
		private Character caster;
		private Environment environment;

		private DashTowardTargetAction dtta;
		private float duration;

		private float elapsed;
		private bool isFinished;
		private DashRequest dashRequest;
		private Character enemy;

		public DashTowardTarget(BaseEvent ef, Character caster, Environment environment) {
			this.ef = ef;
			this.caster = caster;
			this.environment = environment;

			dtta = (DashTowardTargetAction) ef.ShowAction();
			float scaletime = 1;
			if (ef.ShowTrigger() is TimelineTrigger) {
				TimelineTrigger tt = (TimelineTrigger) ef.ShowTrigger();
				scaletime = tt.ShowScaleTime();
			}
			duration = dtta.ShowDurationInSeconds(scaletime);
			List<Character> enemies = environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {
					FindingFilter.ExcludeAllies, FindingFilter.ExcludeMe
				}
			);
			if (enemies.Count > 0) {
				enemy = enemies[0];
				Vector2 diff = enemy.Position() - caster.Position();

				if (!dtta.track) {
					float distance = Math.Abs(diff.x);
					bool isTargetFarAway = distance > dtta.distanceToKeep;
					if (isTargetFarAway) {
						float distanceToDash = distance - dtta.distanceToKeep;
						if (dtta.maxDistance >= 0) {
							distanceToDash = Mathf.Min(distanceToDash, dtta.maxDistance);
						}
						if (diff.x > 0) {
							caster.SetFacingDirectionToRight();
							caster.SetMovingDirectionToRight();
						}
						else {
							caster.SetFacingDirectionToLeft();
							caster.SetMovingDirectionToLeft();
						}
						dashRequest = (DashRequest) caster.Dash(distanceToDash, duration, 0, true, true, true, dtta.uniform);
					}
					else {
						isFinished = true;
					}
				}
				else {
					if (diff.x > 0) {
						caster.SetFacingDirectionToRight();
						caster.SetMovingDirectionToRight();
					}
					else {
						caster.SetFacingDirectionToLeft();
						caster.SetMovingDirectionToLeft();
					}
					dashRequest = (DashRequest) caster.Dash(dtta.distance, duration, 0, true, true, true, dtta.uniform);
				}
			}
			else {
				isFinished = true;
			}
		}

		public void Update(float dt) {
			if (IsFinish()) {
				return;
			}

			elapsed += dt;

			if (dtta.track && enemy != null && dashRequest != null) {
				Vector2 diff = enemy.Position() - caster.Position();
				if (diff.x > 0) {
					dashRequest.SetDirection(Direction.Right.ToNormalizedVector2());
				}
				else {
					dashRequest.SetDirection(Direction.Left.ToNormalizedVector2());
				}
			}
		}

		public void LateUpdate(float dt) {
		}

		public bool IsFinished() {
			return IsFinish();
		}

		public void Interrupt() {
			if (dashRequest != null) {
				dashRequest.Abort();
			}
		}

		private bool IsFinish() {
			return elapsed >= duration || isFinished;
		}
	}
}