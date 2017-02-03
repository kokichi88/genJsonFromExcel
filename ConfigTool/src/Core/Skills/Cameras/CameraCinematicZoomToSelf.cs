using Com.LuisPedroFonseca.ProCamera2D;
using Core.Utils.Extensions;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Cameras {
	public class CameraCinematicZoomToSelf : Loopable {
		private Environment environment;
		private BaseEvent eventFrame;
		private Character caster;

		private CinematicTarget zoomTarget;
		private Vector2 offset;
		private bool isFinished;

		public CameraCinematicZoomToSelf(Environment environment, BaseEvent eventFrame, Character caster) {
			this.environment = environment;
			this.eventFrame = eventFrame;
			this.caster = caster;

			CameraAction ca = (CameraAction) eventFrame.ShowAction();
			CameraAction.CinematicZoomToSelfFx cztsf = (CameraAction.CinematicZoomToSelfFx) ca.fx;
			offset = cztsf.offset.FlipFollowDirection(caster.FacingDirection());
			Vector2 zoomPosition = (Vector2) caster.Position() + offset;
			zoomTarget = environment.PerformCameraCinematicZoom(
				zoomPosition, cztsf.easeDuration, cztsf.ShowEaseType(), cztsf.holdDuration, cztsf.zoomLevel
			);
			zoomTarget.onLeftTarget += () => { isFinished = true; };
		}

		public void Update(float dt) {
			zoomTarget.TargetTransform.position = (Vector2) caster.Position() + offset;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return isFinished;
		}
	}
}