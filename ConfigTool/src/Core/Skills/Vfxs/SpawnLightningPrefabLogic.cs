using Core.Skills.Modifiers;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class SpawnLightningPrefabLogic : Vfx.Logic {
		private VfxAction.SpawnLightningPrefabVfx config;
		private Character caster;
		private float timeToLive;

		private GameObject lightningGo;
		private bool isFinish;
		private bool isInterrupted;
		private float elapsed;

		public SpawnLightningPrefabLogic(VfxAction.SpawnLightningPrefabVfx config, Character caster, float timeToLive) {
			this.config = config;
			this.caster = caster;
			this.timeToLive = timeToLive;

			Transform startJoint = caster.GameObject().transform.FindDeepChild(config.startJoint);
			Transform endJoint = caster.GameObject().transform.FindDeepChild(config.endJoint);
			lightningGo = GameObject.Instantiate(config.ShowVfxPrefab());
			lightningGo.transform.parent = startJoint;
			LightningTrail lightningTrail = lightningGo.GetComponent<LightningTrail>();
			lightningTrail.start = startJoint;
			lightningTrail.end = endJoint;
			lightningTrail.density = config.density;
			lightningTrail.interval = config.nodeProducingInterval;
			lightningTrail.ttl = config.nodeTtl;
			lightningTrail.nodeProducingDuration = config.nodeProducingDuration;
			lightningTrail.nodeStandStillDuration = config.nodeStandStillDuration;
			lightningTrail.lerpSpeed = config.lerpSpeed;
			lightningTrail.Init();
		}

		public void Update(float dt) {
			elapsed += dt;
			isFinish = elapsed >= timeToLive;
		}

		public void DestroyVfx() {
			GameObject.Destroy(lightningGo);
		}

		public bool IsFinish() {
			return isFinish || isInterrupted;
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public void LateUpdate(float dt) {
		}

		public void IncreaseTimeToLiveBy(float seconds) {
		}
	}
}