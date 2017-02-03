using System;
using System.Collections.Generic;
using Artemis;
using Assets.Scripts.Ssar.Dungeon.Model;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Core.Utils.Extensions;
using MovementSystem.Components;
using RootMotion.FinalIK;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils.Gizmos;

namespace Core.Skills.Modifiers {
	public class IkModifier : BaseModifier {
		private IkInfo info;
		private Environment environment;
		private MapColliderBoundariesConfig mapCollider;

		private float elapsed;
		private float aimAt;
		private float startupAt;
		private float activeAt;
		private float recoveryAt;
		private float aimDuration;
		private float aimLogicDuration;
		private float aimInterpolationDuration;
		private float startupDuration;
		private float activeDuration;
		private float recoveryDuration;
		private bool isAim;
		private bool isStartup;
		private bool isActive;
		private bool isRecovery;
		private Character target;
		private Vector3 aimPosition;
		private Vector3 ikPosition;
		private FABRIK fabrik;
		private float aimElapsed;
		private float startupElapsed;
		private float activeElapsed;
		private float recoveryElapsed;
		private Transform ikJoint;
		private Transform ikJointParent;
		private Vector3 intersectionPosition;
		private Vector3 aimPivot;
		private Vector3 axisDirection;
		private Character caster;
		private float totalDuration;
#if UNITY_EDITOR
		private SphereShape gizmosShape;
		private SphereShape gizmosShapeForAnimationAim;
		private LineShape gizmosAimLine;
		private SphereShape gizmosAnimationIntersection;
		private SphereShape gizmosClampedAnimationIntersection;
		private SphereShape gizmosIkTarget;
		private TextShape textAnimationIntersection;
		private TextShape textClampedAnimationIntersection;
		private TextShape textIkTarget;
		private SphereShape gizmosAimPosition;
		private TextShape textAimPosition;
		private TextShape textPhase;
#endif

		public IkModifier(ModifierInfo mi, Entity casterEntity, Entity targetEntity,
		                  Environment environment,
		                  CollectionOfInteractions modifierInteractionCollection) : base(mi, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.environment = environment;
			this.info = (IkInfo) mi;
			mapCollider = environment.MapColliders();

			caster = casterEntity.GetComponent<SkillComponent>().Character;
			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			aimAt = 0;
			aimDuration = fasc.FramesToSeconds(info.Config.aimDuration);
			aimLogicDuration = fasc.FramesToSeconds(info.Config.aimLogicDuration);
			aimInterpolationDuration = fasc.FramesToSeconds(info.Config.aimInterpolationDuration);
			startupAt = aimAt + aimDuration;
			startupDuration = fasc.FramesToSeconds(info.Config.startupDuration);
			activeAt = startupAt + startupDuration;
			activeDuration = fasc.FramesToSeconds(info.Config.activeDuration);
			recoveryAt = activeAt + activeDuration;
			recoveryDuration = fasc.FramesToSeconds(info.Config.recoveryDuration);
			totalDuration = recoveryAt + recoveryDuration;
			target = environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {FindingFilter.ExcludeMe, FindingFilter.ExcludeDead, FindingFilter.ExcludeAllies}
			)[0];
			fabrik = caster.GameObject().GetComponent<FABRIK>();
			IKSolverFABRIK solverFabrik = (IKSolverFABRIK) fabrik.GetIKSolver();
			ikJoint = solverFabrik.bones[solverFabrik.bones.Length - 1].transform;
			ikJointParent = solverFabrik.bones[solverFabrik.bones.Length - 2].transform;
		}

		public override ModifierType Type() {
			return ModifierType.IkAnimation;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;

			bool isFirstUpdate = elapsed - dt == 0;
			if (elapsed < aimLogicDuration && !isFirstUpdate) {
				aimPosition = mapCollider.ClampPositionToGround(target.Position()) + info.Config.offset;
				float diff = aimPosition.x - aimPivot.x;
				if (Mathf.Abs(diff) > info.Config.startupMaxDistance) {
					aimPosition.x = aimPivot.x + info.Config.startupMaxDistance * Mathf.Sign(diff);
				}
#if UNITY_EDITOR
				if (gizmosAimPosition != null) {
					gizmosAimPosition.SetPos(aimPosition);
					textAimPosition.SetPos(aimPosition);
				}
#endif
			}

			if (info.Config.startupEnable) {
				ProcessStartupPhase(dt);
			}
			ProcessActivePhase(dt);
		}

		protected override void OnLateUpdate(float dt) {
			base.OnLateUpdate(dt);
			ProcessAimPhase(dt);
			ProcessRecoveryPhase(dt);
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			IkInfo imi = (IkInfo) modifierInfo;
			return new List<Lifetime>(new [] {
				new DurationBasedLifetime(imi.Config.ShowTotalDurationInSeconds()),
			});
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			DisableIk();
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			DisableIk();
		}

		private void DisableIk() {
			if (fabrik) {
				fabrik.enabled = false;
			}
		}

		private void ProcessAimPhase(float dt) {
			if (elapsed >= aimAt && !isAim) {
				isAim = true;
				fabrik.enabled = true;
				axisDirection = ikJoint.rotation * ikJoint.GetComponent<RotationLimit>().axis.normalized;
				intersectionPosition = CalculateIntersectionPosition(ikJoint.position, axisDirection);
				aimPivot = mapCollider.ClampPositionToGround(intersectionPosition);
				ikPosition = intersectionPosition;
//				fabrik.GetIKSolver().IKPosition = ikPosition;

#if UNITY_EDITOR
				gizmosIkTarget = new SphereShape(0.1f, ikPosition, Color.red, aimDuration);
				textIkTarget = new TextShape(ikPosition, Color.red, "IK target");
				gizmosAnimationIntersection = new SphereShape(0.1f, intersectionPosition, Color.green, aimDuration);
				textAnimationIntersection = new TextShape(intersectionPosition, Color.green, "Animation intersection");
				gizmosClampedAnimationIntersection = new SphereShape(0.1f, intersectionPosition, Color.red, aimDuration);
				textClampedAnimationIntersection = new TextShape(intersectionPosition, Color.red, "Clamped anim intersection");
				gizmosAimLine = new LineShape(ikJoint.position, Color.red, 1, Vector3.up);
				gizmosAimPosition = new SphereShape(0.1f, aimPosition, Color.red, aimDuration);
				textAimPosition = new TextShape(aimPosition, Color.red, "Target");
				textPhase = new TextShape(caster.Position(), Color.red, "Aiming");
				GizmosDrawer.Instance.AddRequest(new DrawRequest(gizmosIkTarget, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(textIkTarget, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(gizmosAnimationIntersection, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(textAnimationIntersection, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(gizmosClampedAnimationIntersection, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(textClampedAnimationIntersection, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(gizmosAimLine, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(gizmosAimPosition, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(textAimPosition, aimDuration));
				GizmosDrawer.Instance.AddRequest(new DrawRequest(textPhase, totalDuration));
#endif
			}

			if (isAim && aimElapsed < aimInterpolationDuration) {
				aimElapsed += dt;
				float progress = aimElapsed / aimInterpolationDuration;
				axisDirection = (ikJoint.position - ikJointParent.position).normalized;
//				DLog.Log("OnUpdate:axisDirection: " + axisDirection.ToPreciseString());
				intersectionPosition = CalculateIntersectionPosition(ikJointParent.position, axisDirection);
				aimPivot = mapCollider.ClampPositionToGround(intersectionPosition);
				Vector3 aimPos = mapCollider.ClampPositionToGround(target.Position()) + info.Config.offset;
				if (info.Config.clampAngle) {
					aimPos = intersectionPosition;
				}
//				Vector3 direction = (aimPos - ikJoint.position).normalized;
//				intersectionPosition = CalculateIntersectionPosition(ikJoint.position, direction);
				Vector3 clampedIntersection = intersectionPosition.CloneWithNewX(Mathf.Lerp(intersectionPosition.x, aimPos.x, progress));
				Vector3 diff = clampedIntersection - ikJointParent.position;
				ikPosition = ikJointParent.position + diff.normalized *
				             ((ikJoint.position - ikJointParent.position).magnitude + 1f);
				fabrik.GetIKSolver().IKPosition = ikPosition;

#if UNITY_EDITOR
				gizmosIkTarget.SetPos(ikPosition);
				textIkTarget.SetPos(ikPosition);
				gizmosAnimationIntersection.SetPos(intersectionPosition);
				textAnimationIntersection.SetPos(intersectionPosition);
				gizmosClampedAnimationIntersection.SetPos(clampedIntersection);
				textClampedAnimationIntersection.SetPos(clampedIntersection);
				gizmosAimLine.SetPos(ikJointParent.position);
				gizmosAimLine.Length = diff.magnitude;
				gizmosAimLine.Direction = diff;
#endif
			}
		}

		private void ProcessStartupPhase(float dt) {
			if (elapsed >= startupAt && !isStartup) {
				isStartup = true;
				Vector3 diff = aimPosition - ikJoint.position;
				Vector3 direction = diff.normalized;
				if (info.Config.clampAngle) {
					Vector2 directionOnXYPlane = (Vector2) direction;
					float halfOpen = (info.Config.to - info.Config.from) / 2f;
					Vector3 axis;
					float angle;
					float angleOfMiddleRay = info.Config.to - halfOpen;
					Vector2 directionOfMiddleRay = new Vector2(
						(float) Math.Cos(Mathf.Deg2Rad * angleOfMiddleRay),
						(float) Math.Sin(Mathf.Deg2Rad * angleOfMiddleRay)
					).normalized;
					if (caster.FacingDirection() == Direction.Left) {
						directionOfMiddleRay.x *= -1;
					}
					Quaternion fromHalfOpenToDirection = Quaternion.FromToRotation(directionOfMiddleRay, directionOnXYPlane);
					fromHalfOpenToDirection.ToAngleAxis(out angle, out axis);
					if (Mathf.Abs(angle) > Mathf.Abs(halfOpen)) {
						Vector3 clampAxis = Vector3.back;
						if (caster.FacingDirection() == Direction.Left) {
							clampAxis = Vector3.forward;
						}
						directionOnXYPlane = Quaternion.AngleAxis(Mathf.Abs(halfOpen), clampAxis) * directionOfMiddleRay;
						direction.x = directionOnXYPlane.x;
						direction.y = directionOnXYPlane.y;
						aimPosition = ikJoint.position + direction.normalized * diff.magnitude;
					}
				}
				ikPosition = ikJoint.position + direction * .5f;
				fabrik.GetIKSolver().IKPosition = ikPosition;

#if UNITY_EDITOR
				textPhase.Text = "Startup";
				gizmosShape = new SphereShape(0.1f, ikPosition, Color.red, startupDuration);
				DrawRequest dr = new DrawRequest(gizmosShape, startupDuration);
				GizmosDrawer.Instance.AddRequest(dr);
				GizmosDrawer.Instance.AddRequest(new DrawRequest(
					new LineShape(
						ikPosition, Color.red,
						(aimPosition - ikJoint.position).magnitude, direction
					),
					startupDuration
				));
#endif
			}

			if (isStartup && startupElapsed < startupDuration) {
				startupElapsed += dt;
				float progress = startupElapsed / startupDuration;
				Vector3 currentIkPos = Vector3.Lerp(ikPosition, aimPosition, progress);
				fabrik.GetIKSolver().IKPosition = currentIkPos;

#if UNITY_EDITOR
				gizmosShape.SetPos(currentIkPos);
#endif
			}
		}

		private void ProcessActivePhase(float dt) {
			if (elapsed >= activeAt && !isActive) {
				isActive = true;

#if UNITY_EDITOR
				textPhase.Text = "Active";
				gizmosShape = new SphereShape(0.1f, ikPosition, Color.red, activeDuration);
				DrawRequest dr = new DrawRequest(gizmosShape, activeDuration);
				GizmosDrawer.Instance.AddRequest(dr);
#endif
			}

			if (isActive && activeElapsed < activeDuration) {
				activeElapsed += dt;

#if UNITY_EDITOR
				gizmosShape.SetPos(ikPosition);
#endif
			}
		}

		private void ProcessRecoveryPhase(float dt) {
			if (elapsed >= recoveryAt && !isRecovery) {
				isRecovery = true;
				if (!info.Config.startupEnable) {
					aimPosition = ikPosition;
				}

#if UNITY_EDITOR
				textPhase.Text = "Recovery";
				gizmosShape = new SphereShape(0.1f, aimPosition, Color.red, recoveryDuration);
				DrawRequest dr = new DrawRequest(gizmosShape, recoveryDuration);
				GizmosDrawer.Instance.AddRequest(dr);
#endif
			}

			if (isRecovery && recoveryElapsed < recoveryDuration) {
				recoveryElapsed += dt;
				float progress = recoveryElapsed / recoveryDuration;
				if (!info.Config.startupEnable) {
					aimPosition = ikPosition;
				}
				Vector3 currentIkPos = Vector3.Lerp(aimPosition, ikJoint.position, progress);
				fabrik.GetIKSolver().IKPosition = currentIkPos;
				if (progress >= 1) {
					fabrik.enabled = false;
				}

#if UNITY_EDITOR
				gizmosShape.SetPos(currentIkPos);
#endif
			}
		}

		private Vector3 CalculateIntersectionPosition(Vector3 startPos, Vector3 direction) {
			//XoY plane
			Vector3 p = Vector3.up;
			Vector3 n = Vector3.forward;
			float d = Vector3.Dot(p, n);
			//Ray
			Vector3 p0 = startPos;
			Vector3 D = direction;
			float t = (d - Vector3.Dot(p0, n)) / Vector3.Dot(D, n);

			return p0 + t * D;
		}
	}
}