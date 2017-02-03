using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using MEC;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class BleedModifier : BaseModifier {
		private readonly Entity casterEntity;
		private BleedInfo info;
		private readonly Environment environment;

		private Character target;
		private Vector2 lastPos;
		private float accumulatedDistance;
		private HealthComponent targetHealthComponent;
		private float accumulatedTriggerDistance;
		private BakedStatsContainer characterStats;
        private List<Transform> vfxTransforms = new List<Transform>();
		public BleedModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.casterEntity = casterEntity;
			this.info = (BleedInfo) info;
			this.environment = environment;
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			StatsComponent casterStats = casterEntity.GetComponent<StatsComponent>();
			characterStats = casterStats.CharacterStats;
		}

		public override ModifierType Type() {
			return ModifierType.Bleed;
		}

		protected override void OnUpdate(float dt) {
			if (target == null) return;

			Vector2 delta = (Vector2) target.Position() - lastPos;
			accumulatedDistance += Mathf.Abs(delta.x);
			if (accumulatedDistance >= info.Bmc.maxDistance) {
				accumulatedTriggerDistance += Mathf.Abs(delta.x);
				if (accumulatedTriggerDistance >= info.Bmc.triggerDistance) {
					accumulatedTriggerDistance -= info.Bmc.triggerDistance;
					float percent = info.Bmc.percent / info.Bmc.distance * info.Bmc.triggerDistance;
					DamageFromAttack damage = new DamageFromAttack(
						new SourceHistory(Source.FromModifier(this)), percent, true, 1, 1,
						casterEntity.Id, target.Position(), target.Position(),
						characterStats, false, CastProjectileAction.DeathBehavior.Neutral,
						true, true
					).CauseTargetToDie(false);
					targetHealthComponent.ReceiveDamage(damage);
					GameObject vfxPrefab = info.Bmc.ShowStoredPrefab();
					if (vfxPrefab != null) {
						GameObject vfx = environment.InstantiateGameObject(vfxPrefab);
						vfx.transform.position = target.Position();
                        vfxTransforms.Add(vfx.transform);

						ParticleSystem[] ps = vfx.GetComponentsInChildren<ParticleSystem>();
						if (ps != null && ps.Length > 0) {
							Timing.RunCoroutine(
								_WaitForParticleSystemFinish(ps, () => GameObject.Destroy(vfx))
							);
						}
						else {
							float waitTime = 1;
							Timing.RunCoroutine(_WaitThenInvoke(waitTime, () => { GameObject.Destroy(vfx); }));
						}
					}
				}
			}
			lastPos = target.Position();

            foreach(Transform vfxTransform in vfxTransforms)
            {
                if(vfxTransform != null)
                    vfxTransform.position = target.Position();
            }
		}

		private IEnumerator<float> _WaitThenInvoke(float waitTime, Action action) {
			yield return Timing.WaitForSeconds(waitTime);
			action();
		}

		private IEnumerator<float> _WaitForParticleSystemFinish(ParticleSystem[] particleSystems, Action callback) {
			float timeout = 5;
			float elapsed = 0;
			while (true) {
				yield return Timing.WaitForSeconds(0.1f);
				elapsed += 0.1f;
				if (particleSystems.Length < 1) break;
				bool finish = true;
				foreach (ParticleSystem ps in particleSystems) {
					finish &= !ps.IsAlive(true);
				}

				if (finish) break;
				if (elapsed >= timeout) break;
			}

			callback();
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			this.target = target;
			lastPos = target.Position();
		}
	}
}