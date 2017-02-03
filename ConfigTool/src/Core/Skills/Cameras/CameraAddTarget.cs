using Com.LuisPedroFonseca.ProCamera2D;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Cameras {
	public class CameraAddTarget : Loopable {
		private Environment environment;
		private BaseEvent eventFrame;

		private float elapsed;
		private CameraTarget cameraTarget;
		private ProCamera2D pc2d;
		private Transform joint;
		private CameraAction.AddTargetFx atf;
		private float originalX;
		private float originalY;
		private CameraTarget casterCameraTarget;

		public CameraAddTarget(Environment environment, BaseEvent eventFrame, Character caster) {
			this.environment = environment;
			this.eventFrame = eventFrame;

			CameraAction ca = (CameraAction) eventFrame.ShowAction();
			atf = (CameraAction.AddTargetFx) ca.fx;
			Character target = environment.FindNearbyCharacters(
				caster, Vector3.zero, 999,
				new[] {FindingFilter.ExcludeMe, FindingFilter.ExcludeDead, FindingFilter.ExcludeAllies}
			)[0];
			joint = target.GameObject().transform.FindDeepChild(atf.joint);
			pc2d = environment.GetCamera().GetComponent<ProCamera2D>();
			casterCameraTarget = pc2d.CameraTargets[0];
			originalX = casterCameraTarget.TargetInfluenceH;
			originalY = casterCameraTarget.TargetInfluenceV;
			casterCameraTarget.TargetInfluenceH = atf.curInfX;
			casterCameraTarget.TargetInfluenceV = atf.curInfY;
			cameraTarget = pc2d.AddCameraTarget(
				joint, atf.influenceY, atf.influenceX, atf.translationDuration
			);
		}

		public void Update(float dt) {
			elapsed += dt;
		}

		public void LateUpdate(float dt) {
			if (IsFinished()) {
				Cleanup();
			}
		}

		public void Interrupt() {
			Cleanup();
		}

		public bool IsFinished() {
			return elapsed >= atf.duration;
		}

		private void Cleanup() {
			pc2d.RemoveCameraTarget(joint);
			casterCameraTarget.TargetInfluenceH = originalX;
			casterCameraTarget.TargetInfluenceV = originalY;
		}
	}
}