using System.Collections.Generic;
using Core.Utils;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class DashShadowVfxLogic : Vfx.Logic {
		private VfxAction.DashShadowVfx dsv;
		private Environment environment;
		private Character caster;

		private DashShadow dashShadow;

		public DashShadowVfxLogic(VfxAction.DashShadowVfx dsv, Environment environment, Character caster) {
			this.dsv = dsv;
			this.environment = environment;
			this.caster = caster;

			GameObject go = caster.GameObject();
			DashShadow ds = go.GetComponent<DashShadow>();
			List<DashShadow.Illusion> pool = null;
			if (ds) {
				pool = ds.pool;
				GameObject.Destroy(ds);
			}
			ds = go.AddComponent<DashShadow>();
			if (pool != null) {
				ds.pool = pool;
			}

			dashShadow = ds;

			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			ds.prefab = dsv.ShowShadowPrefab();
			ds.MaxInstances = dsv.maxShadow;
			ds.Rate = dsv.spawnRate;
			ds.InstanceLiveTime = fasc.FramesToSeconds(dsv.shadowTtl);
			ds.ReservedTime = fasc.FramesToSeconds(dsv.reservedTime);
			ds.material = dsv.ShowShadowMaterial();
			ds.AlphaName = dsv.alphaName;
			ds.AlphaOverLifeTime = dsv.alphaOverLifetime;
			ds.frameDatas = new DashShadow.IllusionFrameData[dsv.frameDatas.Count];
			for (int i = 0; i < ds.frameDatas.Length; i++) {
				DashShadow.IllusionFrameData ifd = new DashShadow.IllusionFrameData();
				ifd.animationClip = new AnimationClip();
				VfxAction.ShadowFrameData sfd = dsv.frameDatas[i];
				ifd.animationClip.name = sfd.anim;
				ifd.startFrame = sfd.start;
				ifd.endFrame = sfd.end;
				ds.frameDatas[i] = ifd;
			}
			ds.changeBodyMaterial = dsv.bodyMat;
			ds.bodyRendererName = dsv.bodyRendererName;
			ds.changeHeadMaterial = dsv.headMat;
			ds.headRendererName = dsv.headRendererName;
			ds.changeWeaponMaterial = dsv.weaponMat;
			ds.weaponRendererName = dsv.weaponRendererName;
			ds.Start_(null, go.GetComponentInChildren<Animation>());
			ds.SetReady(true);
		}

		public void Update(float dt) {
		}

		public void DestroyVfx() {
			//dashShadow.SetReady(false);
		}

		public bool IsFinish() {
			return false;
		}

		public void Interrupt() {
			//dashShadow.SetReady(false);
		}

		public void LateUpdate(float dt) {
		}

		public void IncreaseTimeToLiveBy(float seconds) {
			throw new System.NotImplementedException();
		}
	}
}