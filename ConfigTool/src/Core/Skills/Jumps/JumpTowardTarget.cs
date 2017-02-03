using System;
using System.Collections.Generic;
using MovementSystem.Requests;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Jumps {
	public class JumpTowardTarget : Loopable {
		private BaseEvent ef;
		private Character caster;
		private Environment environment;

		private bool isFinish;
		private bool isInterrupted;
		private float elapsed;
		private float duration;
		private JumpTowardTargetAction.TargetMode mode;
		private Transform joint;
		private JumpTowardTargetAction jtta;
		private Request jumpRequest;

		public JumpTowardTarget(BaseEvent ef, Character caster, Environment environment) {
			this.ef = ef;
			this.caster = caster;
			this.environment = environment;

			jtta = (JumpTowardTargetAction) ef.ShowAction();
			duration = jtta.timeToPeak + jtta.timeToFloat + jtta.timeToGround;
			List<Character> enemies = environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {
					FindingFilter.ExcludeAllies, FindingFilter.ExcludeMe
				}
			);
			if (enemies.Count > 0) {
				Character enemy = enemies[0];
				Vector2 translationFromCasterToEnemy = Vector2.zero;
				float distanceToJump = 0;
				mode = jtta.ShowTargetMode();
				switch (mode) {
					case JumpTowardTargetAction.TargetMode.Joint:
						joint = enemy.GameObject().transform.FindDeepChild(jtta.joint);
						Vector3 jumpTarget = joint.TransformPoint(jtta.offset);
						translationFromCasterToEnemy = jumpTarget - caster.Position();
						distanceToJump = Mathf.Abs(translationFromCasterToEnemy.x);
						break;
					case JumpTowardTargetAction.TargetMode.KeepDistance:
						translationFromCasterToEnemy = enemy.Position() - caster.Position();
						float distanceOnXAxis = Math.Abs(translationFromCasterToEnemy.x);
						bool isTargetFarAway = distanceOnXAxis > jtta.distanceToKeep;
						if (isTargetFarAway) {
							distanceToJump = distanceOnXAxis - jtta.distanceToKeep;
							if (jtta.maxDistance >= 0) {
								distanceToJump = Mathf.Min(distanceToJump, jtta.maxDistance);
							}
						}
						break;
				}
				if (translationFromCasterToEnemy.x > 0) {
					caster.SetFacingDirectionToRight();
					caster.SetMovingDirectionToRight();
				}
				else {
					caster.SetFacingDirectionToLeft();
					caster.SetMovingDirectionToLeft();
				}
				jumpRequest = caster.JumpOverDistance(
					jtta.height, jtta.timeToPeak,
					distanceToJump, jtta.timeToGround,
					false, jtta.timeToFloat, jtta.stopHorizontal
				);
			}
			else {
				isFinish = true;
			}
		}

		public void Update(float dt) {
			elapsed += dt;
			if (elapsed >= duration) {
				isFinish = true;
			}
		}

		public void LateUpdate(float dt) {
			if (mode == JumpTowardTargetAction.TargetMode.Joint) {
				float sqrMagnitude = Vector2.SqrMagnitude(caster.Position() - joint.TransformPoint(jtta.offset));
				if (sqrMagnitude < Mathf.Pow(0.1f, 2) || IsFinish()) {
					jumpRequest.Abort();
				}
			}
		}

		public bool IsFinished() {
			return IsFinish();
		}

		private bool IsFinish() {
			return isFinish || isInterrupted;
		}

		public void Interrupt() {
			isInterrupted = true;
			caster.InterruptJump();
		}
	}
}