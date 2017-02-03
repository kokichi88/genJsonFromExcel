using System;
using System.Collections.Generic;
using Core.Commons;
using Core.Utils;
using EntityComponentSystem;
using MovementSystem.Components;
using UnityEngine;
using Utils;
using KeepDistanceMode = Ssar.Combat.Skills.Events.Actions.TeleportAction.KeepDistanceMode;

namespace Core.Skills.Teleports {
	public class TeleportKeepDistanceLogic : Loopable {
		private KeepDistanceMode info;
		private Skill skill;
		private Environment environment;
		private Character caster;

		private float lockTargetAt;
		private float teleportAt;
		private float notifyAt;
		private float elapsed;
		private bool isInterrupted;
		private bool isTargetLocked;
		private bool isTeleported;
		private bool isNotified;
		private Character target;
		private Vector2 teleportPosition;

		public TeleportKeepDistanceLogic(KeepDistanceMode info, Skill skill, Environment environment,
		                                 Character caster) {
			this.info = info;
			this.skill = skill;
			this.environment = environment;
			this.caster = caster;

			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			lockTargetAt = fasc.FramesToSeconds(info.targetLockOffset);
			teleportAt = fasc.FramesToSeconds(info.teleportOffset);
			notifyAt = fasc.FramesToSeconds(info.notiOffset);
		}

		public void Update(float dt) {
			elapsed += dt;

			LockTarget();
			Teleport();
			Notify();
		}

		private void Notify() {
			if (elapsed >= notifyAt && !isNotified) {
				isNotified = true;
				TemplateArgs args = new TemplateArgs();
				args.SetEntry(TemplateArgsName.Position, teleportPosition);
				skill.TriggerEventWithId(info.notiEventId, args);
			}
		}

		private void Teleport() {
			if (elapsed >= teleportAt && !isTeleported) {
				isTeleported = true;
				caster.SetPosition(teleportPosition);
			}
		}

		private void LockTarget() {
			if (elapsed >= lockTargetAt && !isTargetLocked) {
				isTargetLocked = true;
				List<Character> enemies = environment.FindNearbyCharacters(
					caster, Vector3.zero, 999,
					new[] {
						FindingFilter.ExcludeAllies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe
					}
				);
				if (enemies.Count > 0) {
					Character enemy = enemies[0];
					CharacterId enemyCharId = enemy.CharacterId();
					CharacterId charToControl = environment.CharacterToControl();
					CharacterId casterCharId = caster.CharacterId();

					if (casterCharId != charToControl) {
						float from = info.distanceRange[0];
						float to = info.distanceRange[1];
						float distanceBetween = Math.Abs(enemy.Position().x - caster.Position().x);
						float distance = distanceBetween;
						if (distanceBetween <= @from) {
							distance = @from;
						}
						else if (distanceBetween >= to) {
							distance = to;
						}

						target = enemy;
						teleportPosition = (Vector2) enemy.Position() +
						                 enemy.FacingDirection().ToNormalizedVector2() * distance;
						if (info.ground) {
							teleportPosition =
								BattleUtils.ClampPositionToGround(environment.MapColliders(), teleportPosition);
						}
						//DLog.Log("teleport position " + teleportPosition);
					}
				}
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return isInterrupted || (isTargetLocked && isTeleported && isNotified);
		}
	}
}