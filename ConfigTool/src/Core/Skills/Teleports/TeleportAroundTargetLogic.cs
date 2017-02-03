using System;
using System.Diagnostics;
using Artemis;
using Combat.Stats;
using Core.Utils;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MovementSystem.Components;
using Ssar.Combat.Skills.Character50020;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;
using AroundTargetMode = Ssar.Combat.Skills.Events.Actions.TeleportAction.AroundTargetMode;
using Grid2 = Ssar.Combat.Skills.Character50020.Character50020Skill1.Grid2;

namespace Core.Skills.Teleports {
	public partial class TeleportAroundTargetLogic : Loopable {
		private AroundTargetMode info;
		private Character caster;
		private Environment environment;
		private Skill skill;

		private int countSoFar;
		private float elapsed;
		private bool isTeleportEndTriggered;
		private Grid2 grid2;
		private MovementComponent movementComponent;
		private Character target;
		private Stats invisibleStats;
		private GameObjectComponent gameObjectComponent;
		private float delayElapsed;
		private float delay;
		private float prepareDelayElapsed;
		private float prepareDelay;
		private bool isPrepareEventDispatched;
		private float prepareEndDelayElapsed;
		private float prepareEndDelay;
		private bool isPrepareEndEventDispatched;
		private int prepareDispatchCount;

		public TeleportAroundTargetLogic(AroundTargetMode info, Character caster,
		                                 Environment environment, Skill skill) {
			this.info = info;
			this.caster = caster;
			this.environment = environment;
			this.skill = skill;

			Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
			movementComponent = entity.GetComponent<MovementComponent>();
			StatsComponent statsComponent = entity.GetComponent<StatsComponent>();
			invisibleStats = statsComponent.CharacterStats.FindStats(StatsType.Invisible);
			gameObjectComponent = (GameObjectComponent) entity.GetComponent<EntityGameObjectComponent>();
			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			delay = fasc.FramesToSeconds(info.delay);
			prepareDelay = fasc.FramesToSeconds(info.prepareDelay);
			prepareEndDelay = delay + info.duration - fasc.FramesToSeconds(info.endPreceding);
			if (delay == 0) {
				skill.TriggerEventWithId(info.prepareEventId);
				Perform();
				isPrepareEventDispatched = true;
				prepareDispatchCount++;
			}

			if (prepareDelay == 0 && prepareDispatchCount < info.count) {
				skill.TriggerEventWithId(info.prepareEventId);
				isPrepareEventDispatched = true;
				prepareDispatchCount++;
			}
		}

		protected virtual Character FindTarget(Character caster, Environment environment) {
			return environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {FindingFilter.ExcludeAllies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe}
			)[0];
		}

		public void Update(float dt) {
			if (IsFinished()) return;

			if (delayElapsed < delay) {
				delayElapsed += dt;
				if (delayElapsed >= delay) {
					Perform();
					isPrepareEventDispatched = false;
					// DLog.Log("debug teleport");
					return;
				}

				if (prepareDispatchCount < info.count) {
					prepareDelayElapsed += dt;
					// DLog.Log("debug delay prepareDelayElapsed: " + prepareDelayElapsed);
				}
				if (prepareDelayElapsed >= prepareDelay && !isPrepareEventDispatched) {
					isPrepareEventDispatched = true;
					skill.TriggerEventWithId(info.prepareEventId);
					prepareDelayElapsed = 0;
					prepareDispatchCount++;
					// DLog.Log("debug trigger preparation");
				}

				prepareEndDelayElapsed += dt;
				// DLog.Log("debug prepare END DelayElapsed: " + prepareEndDelayElapsed);
				if (prepareEndDelayElapsed >= prepareEndDelay && !isPrepareEndEventDispatched) {
					isPrepareEndEventDispatched = true;
					skill.TriggerEventWithId(info.endPrecedingEid);
					prepareEndDelayElapsed = 0;
					// DLog.Log("debug trigger preparation END ");
				}
			}
			else {
				elapsed += dt;
				if (elapsed >= info.duration && !isTeleportEndTriggered) {
					isTeleportEndTriggered = true;
					skill.TriggerEventWithId(info.endEventId);
					GameObject disappearPrefab = info.ShowAppearPrefab();
					if(disappearPrefab != null)
						environment.InstantiateGameObject(disappearPrefab).transform.position = caster.Position();
					if (info.invisible && invisibleStats != null) {
						invisibleStats.SetBaseBoolValue(false);
						gameObjectComponent.EnableRendererComponent();
					}
					isPrepareEndEventDispatched = false;
				}

				if (elapsed >= info.interval && countSoFar < info.count) {
					elapsed -= info.interval;
					Perform();
					isPrepareEventDispatched = false;
					// DLog.Log("debug teleport");
				}

				if (prepareDispatchCount < info.count) {
					prepareDelayElapsed += dt;
					// DLog.Log("debug prepareDelayElapsed: " + prepareDelayElapsed);
				}
				if (prepareDelayElapsed >= info.interval && !isPrepareEventDispatched) {
					isPrepareEventDispatched = true;
					prepareDelayElapsed -= info.interval;
					skill.TriggerEventWithId(info.prepareEventId);
					prepareDispatchCount++;
					// DLog.Log("debug trigger preparation");
				}

				prepareEndDelayElapsed += dt;
				// DLog.Log("debug prepare END DelayElapsed: " + prepareEndDelayElapsed);
				if (prepareEndDelayElapsed >= prepareEndDelay && !isPrepareEndEventDispatched) {
					isPrepareEndEventDispatched = true;
					skill.TriggerEventWithId(info.endPrecedingEid);
					prepareEndDelayElapsed -= info.interval;
					// DLog.Log("debug trigger preparation END ");
				}
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return IsTeleportActionFinished() && elapsed >= info.duration;
		}

		public bool IsTeleportActionFinished() {
			return countSoFar >= info.count;
		}

		private void Perform() {
			var mostLeftAndMostRight = environment.MostLeftAndMostRightOfCurrentStage();
			target = FindTarget(caster, environment);
			grid2 = new Grid2(
				info.row, info.column, info.size.x, info.size.y, info.relativePos,
				mostLeftAndMostRight.x, mostLeftAndMostRight.y, info.blacklistRadius,
				target, info.ShowPreferedSide(), info.targetRadius, info.invert
			);
			countSoFar++;
			isTeleportEndTriggered = false;
			Vector2 teleportTargetPosition;
			if (countSoFar == 1) {
				teleportTargetPosition = grid2.ObtainFirstTime(target.Position(), caster.Position());
			}
			else {
				teleportTargetPosition = grid2.Obtain(target.Position());
			}

			if (info.ground) {
				teleportTargetPosition = environment.MapColliders().ClampPositionToGround(teleportTargetPosition);
			}

			//DLog.Log("Teleport to " + teleportTargetPosition);
			Vector2 casterPosition = caster.Position();
			Vector2 originalStartPositionOfLightning = casterPosition;
			Vector2 displacement = teleportTargetPosition - casterPosition;
			skill.TriggerEventWithId(info.beginEventId);
			GameObject appearPrefab = info.ShowDisappearPrefab();
			if(appearPrefab != null)
				environment.InstantiateGameObject(appearPrefab).transform.position = caster.Position();

			if (info.ignoreGate) {
				Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
				entity.GetComponent<MovementComponent>().ForceSetPosition(teleportTargetPosition);
			}
			else {
				caster.DisplaceBy(displacement);
			}

			if (info.resetRoration) {
				movementComponent.SetOrientation(Quaternion.identity);
			}

			if (info.duration == 0) {
				isTeleportEndTriggered = true;
				skill.TriggerEventWithId(info.endEventId);
				GameObject disappearPrefab = info.ShowAppearPrefab();
				if(disappearPrefab != null)
					environment.InstantiateGameObject(disappearPrefab).transform.position = caster.Position();
			}

			casterPosition = caster.Position();
			Vector2 originalEndPositionOfLightning = casterPosition;
			if (info.ShowChainingPrefab() != null) {
				skill.AddLoopable(
					new Chaining(
						info, caster, environment, skill,
						originalStartPositionOfLightning, originalEndPositionOfLightning
					)
				);
			}

			if (info.invisible && invisibleStats != null) {
				invisibleStats.SetBaseBoolValue(true);
				gameObjectComponent.DisableRendererComponent();
			}
		}
	}

	public partial class TeleportAroundTargetLogic {
		private class Chaining : Loopable {
			private AroundTargetMode info;
			private Character caster;
			private Environment environment;
			private Skill skill;
			private Vector2 originalStartPositionOfLightning;
			private Vector2 originalEndPositionOfLightning;

			private GameObject lightningVfx;
			private Transform lightningStart;
			private Transform lightningEnd;
			private float elapsed;
			private float colapseElapsed;
			private bool isLightningColapse;
			private bool isLightningExpand;
			private float expandElapsed;

			public Chaining(AroundTargetMode info, Character caster,
			                 Environment environment, Skill skill,
			                 Vector2 originalStartPositionOfLightning,
			                 Vector2 originalEndPositionOfLightning) {
				this.info = info;
				this.caster = caster;
				this.environment = environment;
				this.skill = skill;
				this.originalStartPositionOfLightning = originalStartPositionOfLightning;
				this.originalEndPositionOfLightning = originalEndPositionOfLightning;

				GameObject chainingPrefab = info.ShowChainingPrefab();
				if (chainingPrefab != null) {
					lightningVfx = environment.InstantiateGameObject(chainingPrefab);
					lightningStart = lightningVfx.transform.Find("LightningStart");
					lightningStart.position = originalStartPositionOfLightning;
					lightningEnd = lightningVfx.transform.Find("LightningEnd");
					lightningEnd.position = originalEndPositionOfLightning;
				}

				isLightningExpand = info.cExpandDuration > 0;
				if (isLightningExpand && lightningEnd != null) {
					lightningEnd.position = originalStartPositionOfLightning;
				}
			}

			public void Update(float dt) {
				if (IsFinished()) return;
				elapsed += dt;

				if (isLightningExpand && lightningEnd != null) {
					expandElapsed += dt;
					float progress = expandElapsed / info.cColapseDuration;
					progress = Math.Min(progress, 1f);
					lightningEnd.position = Vector2.Lerp(originalStartPositionOfLightning, originalEndPositionOfLightning, progress);
					if (expandElapsed >= info.cExpandDuration) {
						isLightningExpand = false;
					}
				}

				if (elapsed >= info.cDuration - info.cColapseDuration && !isLightningColapse) {
					isLightningColapse = true;
					colapseElapsed = 0;
				}

				if (isLightningColapse && colapseElapsed <= info.cColapseDuration && lightningStart != null) {
					colapseElapsed += dt;
					float progress = colapseElapsed / info.cColapseDuration;
					progress = Math.Min(progress, 1f);
//					DLog.Log("progress " + progress);
					lightningStart.position = Vector2.Lerp(originalStartPositionOfLightning, originalEndPositionOfLightning, progress);
					if (colapseElapsed >= info.cColapseDuration) {
						isLightningColapse = false;
					}
				}

				if (IsFinished()) {
					GameObject.Destroy(lightningVfx);
				}
			}

			public void LateUpdate(float dt) {
			}

			public void Interrupt() {
			}

			public bool IsFinished() {
				return elapsed >= info.cDuration;
			}
		}
	}
}