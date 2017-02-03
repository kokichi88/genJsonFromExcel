using System;
using System.Collections.Generic;
using Artemis;
using Core.Utils.Extensions;
using EntityComponentSystem;
using MEC;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using UnityEngine;
using Utils;
using Utils.DataStruct;
using Utils.Gizmos;

namespace Core.Skills.Dashes {
	public class Dash : Loopable {
		private readonly int obstacleMask = LayerMask.GetMask(
			EntityLayerName.Gate.ToString(),
			EntityLayerName.HardBlock.ToString(),
			EntityLayerName.SoftBlock.ToString()
		);
		private readonly int creatureMask = LayerMask.GetMask(
			EntityLayerName.CreatureEntity.ToString()
		);
		private readonly BaseEvent ef;
		private readonly Character caster;
//		private CharacterMediatorComponent characterMediatorComponent;

		private float duration;
		private float elapsed;
		private BoxCollider casterBoxCollider;
		private bool ignoreMinSpeedOnAir = false;
		private readonly Skill skill;
		private bool isDashPerformed;
		private Request dashRequest;
		private MovementComponent movementComponent;
		private DashAction da;
		private Transform casterTransform;
		private CubeShape cubeShape;
		private bool dispatched;
		private GameObject mapGround;
		private GameObject mapCeil;
		private CoroutineHandle collisionDetectionCoroutineHandle;
		private Vector2 centerOfHitBoxAtPreviousFrame;
		private Vector2 positionAtPreviousFrame;
		private Entity casterEntity;
		private bool isThisJumpDisabled;
		private int pauseCount;

		public Dash(BaseEvent ef, Character caster, bool ignoreMinSpeedOnAir, Skill skill, Environment environment) {
			this.ef = ef;
			this.caster = caster;
			this.ignoreMinSpeedOnAir = ignoreMinSpeedOnAir;
			this.skill = skill;
			mapGround = environment.MapColliders().bottom;
			mapCeil = environment.MapColliders().top;
			GameObject gameObject = caster.GameObject();
			casterTransform = gameObject.transform;
			casterBoxCollider = gameObject.GetComponent<BoxCollider>();
			casterEntity = gameObject.GetComponent<EntityReference>().Entity;
			movementComponent = casterEntity.GetComponent<MovementComponent>();
			da = (DashAction) ef.ShowAction();
			if (ef.ShowTrigger().ShowTriggerType() == TriggerType.Frame) {
				TimelineTrigger tt = (TimelineTrigger) ef.ShowTrigger();
				duration = da.ShowEndFrameInSeconds(tt.ShowScaleTime());
			}
			else {
				duration = da.ShowEndFrameInSeconds(1);
			}
			PerformDash(da, caster);
			isDashPerformed = true;
			CacheOriginalColliderData(ef, caster);
			ResizeCollider(ef);
			if (da.collision) {
				cubeShape = new CubeShape(GetCenterOfHitbox(), casterBoxCollider.size, new List<SsarTuple<Color, float>>(new []{new SsarTuple<Color, float>(Color.cyan, duration), }));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(cubeShape, duration));
			}

			if (da.collision) {
				collisionDetectionCoroutineHandle = Timing.RunCoroutine(DetectCollision(), Segment.FixedUpdate);
				centerOfHitBoxAtPreviousFrame = GetCenterOfHitbox();
			}

			positionAtPreviousFrame = movementComponent.Position;
		}

		private IEnumerator<float> DetectCollision() {
			while (true) {
				yield return Timing.WaitForOneFrame;
				Update_(Time.fixedDeltaTime);
				if(IsFinish()) break;
			}
		}

		public void Update(float dt) {
			if(da.collision) return;

			Update_(dt);
		}

		public void PauseForLockFrame() {
			pauseCount++;
		}

		public void UnpauseForLockFrame() {
			pauseCount--;
		}

		private void Update_(float dt) {
			if (pauseCount > 0) return;
			if(IsFinish()) return;

			elapsed += dt;

			if (da.ignoreObstacles || da.collision) {
				dashRequest.Update(dt);
				Vector3 displacement = ((FixedUpdateDashRequest) dashRequest).Displacement_(movementComponent, dt);
				if (da.ignoreObstacles) {
					movementComponent.ForceSetPosition(movementComponent.PositionV3 + displacement);
				}
				else {
					movementComponent.Move(displacement);
				}
			}

			if (da.collision) {
				Vector2 centerOfHitbox = GetCenterOfHitbox();
				Vector2 bridgeHitBoxSize = new Vector2(
					Mathf.Abs(centerOfHitbox.x - centerOfHitBoxAtPreviousFrame.x),
					casterBoxCollider.size.y
				);
				Vector2 centerOfBridgeHitBox = (centerOfHitbox + centerOfHitBoxAtPreviousFrame) / 2;
				/*CubeShape cc = new CubeShape(centerOfBridgeHitBox, bridgeHitBoxSize, new List<SsarTuple<Color, float>>(new []{new SsarTuple<Color, float>(Color.yellow, .1f), }));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(cc, .1f));*/
				cubeShape.SetPos(centerOfHitbox);
				cubeShape.size = casterBoxCollider.size;

				bool collision = false;
				Transform collidedTransform = null;
				Collider targetCollider = null;
				Collider[] c = Physics.OverlapBox(centerOfBridgeHitBox, bridgeHitBoxSize / 2, Quaternion.identity, obstacleMask);
				foreach (Collider collider in c) {
					//DLog.Log("Collide with obstacle " + collider.name);
					if(collider.gameObject == mapGround) continue;
					if(collider.gameObject == mapCeil) continue;

					collision = true;
					collidedTransform = collider.transform;
					targetCollider = collider;
					break;
				}
				if (!collision) {
					c = Physics.OverlapBox(centerOfBridgeHitBox, bridgeHitBoxSize / 2, Quaternion.identity, creatureMask);
					foreach (Collider collider in c) {
						//DLog.Log("Collide with creature " + collider.GetComponentInParent<EntityReference>().gameObject.name);
						EntityReference collidedEntityRef = collider.GetComponentInParent<EntityReference>();
						if (collidedEntityRef.transform == casterTransform) continue;
						if (collidedEntityRef.Entity.Group.Equals(casterEntity.Group)) continue;
						collision = true;
						if (collidedTransform == null) {
							collidedTransform = collider.transform;
							targetCollider = collider;
						}
						else {
							float distanceBetweenCasterAndTransform = Vector2.Distance(positionAtPreviousFrame, collider.transform.position);
							float distanceBetweenCasterAndPreviousTransform = Vector2.Distance(positionAtPreviousFrame, collidedTransform.position);
							if (distanceBetweenCasterAndTransform < distanceBetweenCasterAndPreviousTransform) {
								collidedTransform = collider.transform;
								targetCollider = collider;
							}
						}
					}
				}

				if (collision && !dispatched) {
					Vector2 pos;
					if (elapsed > dt) {//second loop or above
						//DLog.Log("Second and above loop");
						Vector2 offset = da.offset.FlipFollowDirection(movementComponent.FacingDirection);
						pos = (Vector2) targetCollider.bounds.center - offset;
						pos.y = movementComponent.Position.y;
						float minX = centerOfHitBoxAtPreviousFrame.x - offset.x;
						float maxX = centerOfHitbox.x - offset.x;
						if (movementComponent.FacingDirection == Direction.Left) {
							minX = centerOfHitbox.x - offset.x;
							maxX = centerOfHitBoxAtPreviousFrame.x - offset.x;
						}
						/*if (pos.x < minX || pos.x > maxX) {
							DLog.Log("x " + pos.x + " minX " + minX + " maxX " + maxX);
						}*/
						pos.x = Mathf.Clamp(pos.x, minX, maxX);
					}
					else {//first loop
						//DLog.Log("First loop");
						bool alreadyCollided = false;
						Collider[] collidedCreatures = Physics.OverlapBox(centerOfHitBoxAtPreviousFrame, casterBoxCollider.size / 2, Quaternion.identity, creatureMask);
						if (collidedCreatures.Contains(targetCollider)) {
							alreadyCollided = true;
						}

						if (alreadyCollided) {
							pos = positionAtPreviousFrame;
						}
						else {
							Vector2 offset = da.offset.FlipFollowDirection(movementComponent.FacingDirection);
							pos = (Vector2) targetCollider.bounds.center - offset;
							if (movementComponent.FacingDirection == Direction.Right) {
								pos -= new Vector2(targetCollider.bounds.size.x / 2, 0);
							}
							else {
								pos += new Vector2(targetCollider.bounds.size.x / 2, 0);
							}
							pos.y = movementComponent.Position.y;
						}
					}

					movementComponent.ForceSetPosition(pos);
					cubeShape.SetPos(GetCenterOfHitbox());
					dispatched = false;
					skill.TriggerEventWithId(da.eventId);
					dashRequest.Abort();
				}

				centerOfHitBoxAtPreviousFrame = centerOfHitbox;
			}

			positionAtPreviousFrame = movementComponent.Position;
			if (IsFinish()) {
				ResetColliderToOriginalValues();
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
			ResetColliderToOriginalValues();
		}

		public Request DashRequest => dashRequest;

		public bool IgnoreMinSpeedOnAir {
			get { return ignoreMinSpeedOnAir; }
			set { ignoreMinSpeedOnAir = value; }
		}

		private Vector2 GetCenterOfHitbox() {
			return (Vector2) movementComponent.Position + (Vector2) casterBoxCollider.center
			                                  + da.offset.FlipFollowDirection(movementComponent.FacingDirection);
		}

		private void ResetColliderToOriginalValues() {
			if(casterBoxCollider == null) return;
			
//			casterBoxCollider.center = characterMediatorComponent.ColliderOriginalCenter;
//			casterBoxCollider.size = characterMediatorComponent.ColliderOriginalSize;
		}

		private void ResizeCollider(BaseEvent ef) {
//			if (ef.Move.AdjustCollider) {
//				casterBoxCollider.center = new Vector3(
//					ef.Move.MoveColliderCenter.x, ef.Move.MoveColliderCenter.y, characterMediatorComponent.ColliderOriginalCenter.z
//				);
//				casterBoxCollider.size = new Vector3(
//					ef.Move.MoveColliderSize.x, ef.Move.MoveColliderSize.y, characterMediatorComponent.ColliderOriginalSize.z
//				);
//			}
		}

		private void CacheOriginalColliderData(BaseEvent ef, Character caster) {
//			EntityWorld ew = (EntityWorld) ef.Extras[0];
//			Entity casterEntity = ew.EntityManager.GetEntity(caster.Id());
//			characterMediatorComponent = casterEntity.GetComponent<CharacterMediatorComponent>();
//			GameObject casterGameObject = characterMediatorComponent.GetGameObject();
//			casterBoxCollider = casterGameObject.GetComponent<BoxCollider>();
		}

		private void PerformDash(DashAction da, Character caster) {
			switch (da.ShowRequirement()) {
				case JumpAction.Requirement.Air:
					if (caster.IsOnGround()) isThisJumpDisabled = true;
					break;
				case JumpAction.Requirement.Ground:
					if (!caster.IsOnGround()) isThisJumpDisabled = true;
					break;
			}
			if (isThisJumpDisabled) {
				return;
			}

			float distance = Math.Abs(da.ShowDistance());
			Vector2 direction = da.ShowDirectionAsNormalizedVector();
			if (caster.FacingDirection() == Direction.Left) {
				direction = new Vector2(direction.x * -1, direction.y);
			}
			if (da.ShowDistance() < 0) {
				direction *= -1;
			}
			caster.SetMovingDirection(direction);

			if (da.ignoreObstacles || da.collision) {
				dashRequest = new FixedUpdateDashRequest(distance, duration, da.ShowBlendTime(), da.IsUniform());
				movementComponent.AddMovementRequest(dashRequest);
			}
			else {
				dashRequest = caster.Dash(
					distance, duration, da.ShowBlendTime(), true, true, ignoreMinSpeedOnAir, da.IsUniform()
				);
			}
		}

		private bool IsFinish() {
			if (isThisJumpDisabled) return true;
			
			return elapsed >= duration || dashRequest.IsCompleted();
		}
	}
}