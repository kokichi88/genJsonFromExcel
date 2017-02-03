using System;
using System.Collections.Generic;
using Artemis;
using Artemis.Utils;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using Gameplay;
using MEC;
using Ssar.Combat.Animation;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class LockFrameModifier : BaseModifier {
		private LockFrameInfo lockFrameInfo;
		private Entity caster;
		private Entity target;
		private readonly PromiseWorld entityWorld;

		private bool[] lockFinished = {false, false};
		private List<bool> othersLockFinished = new List<bool>();
		private DurationBasedLifetime lifetime;

		public LockFrameModifier(LockFrameInfo lockFrameInfo, Entity caster, Entity target,
		                         PromiseWorld entityWorld, Environment environment,
		                         CollectionOfInteractions modifierInteractionCollection) : base(lockFrameInfo, caster, target, environment, modifierInteractionCollection) {
			this.lockFrameInfo = lockFrameInfo;
			this.caster = caster;
			this.target = target;
			this.entityWorld = entityWorld;
		}

		public override string Name() {
			return string.Format("{0}({1})", Type(), attachType);
		}

		public override ModifierType Type() {
			return ModifierType.LockFrame;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			Action onCasterLockFinish = () => {
				lockFinished[0] = true;
				//DLog.Log("onCasterLockFinish()");
				if (AreAllLocksFinish()) {
					lifetime.End();
				}
			};
			if (lockFrameInfo.DelayForCaster == 0) {
				PauseFor(caster, lockFrameInfo.DurationForCaster, onCasterLockFinish);
			}
			else {
				WaitThenPause(lockFrameInfo.DelayForCaster, caster, lockFrameInfo.DurationForCaster, onCasterLockFinish);
			}
			Action onTargetLockFinish = () => {
				lockFinished[1] = true;
				//DLog.Log("onTargetLockFinish()");
				if (AreAllLocksFinish()) {
					lifetime.End();
				}
			};
			if (lockFrameInfo.DelayForTarget == 0) {
				PauseFor(this.target, lockFrameInfo.DurationForTarget, onTargetLockFinish);
			}
			else {
				WaitThenPause(lockFrameInfo.DelayForTarget, this.target, lockFrameInfo.DurationForTarget, onTargetLockFinish);
			}

			if (lockFrameInfo.LockGlobally) {
				Bag<Entity> entities = entityWorld.GetEntities(Aspect.All(typeof(SkillComponent)));

				if (lockFrameInfo.DelayForGlobal == 0) {
					for (int kIndex = 0; kIndex < entities.Count; kIndex++) {
						Entity e = entities[kIndex];
						if (e == caster || e == this.target) continue;

						othersLockFinished.Add(false);
						int index = kIndex;
						PauseFor(e, lockFrameInfo.DurationForGlobal, () => {
							othersLockFinished[index] = true;
							if (AreAllLocksFinish()) {
								lifetime.End();
							}
						});
					}
				}
				else {
					for (int kIndex = 0; kIndex < entities.Count; kIndex++) {
						Entity e = entities[kIndex];
						if (e == caster || e == this.target) continue;

						othersLockFinished.Add(false);
						WaitThenPause(lockFrameInfo.DelayForGlobal, e, lockFrameInfo.DurationForGlobal, () => {
							othersLockFinished[kIndex] = true;
							if (AreAllLocksFinish()) {
								lifetime.End();
							}
						});
					}
				}
			}
		}

		private void WaitThenPause(float waitTime, Entity entityToPause, float duration, Action onCompleted) {
			Timing.RunCoroutine(_WaitThenPause(waitTime, entityToPause, duration, onCompleted));
		}

		private IEnumerator<float> _WaitThenPause(float waitTime, Entity entityToPause, float duration,
		                                          Action onCompleted) {
			yield return Timing.WaitForSeconds(waitTime);
			PauseFor(entityToPause, duration, onCompleted);
		}

		private void PauseFor(Entity entityToPause, float duration, Action onCompleted) {
			if (duration <= 0) {
				onCompleted();
				return;
			}
			Timing.RunCoroutine(PauseFor_(entityToPause, duration, onCompleted));
		}

		private IEnumerator<float> PauseFor_(Entity entityToPause, float duration, Action onCompleted) {
			TimeScaleComponent timeScaleComponent = entityToPause.GetComponent<TimeScaleComponent>();
			AnimationComponent animationComponent = entityToPause.GetComponent<AnimationComponent>();
			SkillComponent skillComponent = entityToPause.GetComponent<SkillComponent>();
			timeScaleComponent.Pause();
			animationComponent.Animation.PauseAnimation();
			if (timeScaleComponent.IsFirstPause()) {
				skillComponent.Character.PauseForLockFrame();
			}
			yield return Timing.WaitForSeconds(duration);
			Unpause(timeScaleComponent, animationComponent, skillComponent);
			onCompleted();
		}

		private static void Unpause(TimeScaleComponent timeScaleComponent, AnimationComponent animationComponent,
		                            SkillComponent skillComponent) {
			timeScaleComponent.Unpause();
			animationComponent.Animation.UnpauseAnimation();
			if (!timeScaleComponent.IsPaused) {
				skillComponent.Character.UnpauseForLockFrame();
			}
		}

		private void UnpauseEntity(Entity entity) {
			TimeScaleComponent timeScaleComponent = entity.GetComponent<TimeScaleComponent>();
			AnimationComponent animationComponent = entity.GetComponent<AnimationComponent>();
			SkillComponent skillComponent = entity.GetComponent<SkillComponent>();
			Unpause(timeScaleComponent, animationComponent, skillComponent);
		}

		private bool AreAllLocksFinish() {
			bool otherFinished = true;
			for (int kIndex = 0; kIndex < othersLockFinished.Count; kIndex++) {
				if (othersLockFinished[kIndex] == false) {
					otherFinished = false;
					break;
				}
			}
			return lockFinished[0] && lockFinished[1] && otherFinished;
		}

		public override object[] Cookies() {
			return new object[0];
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			LockFrameInfo lfi = (LockFrameInfo) modifierInfo;
			float durationForTarget = lfi.DelayForTarget + lfi.DurationForTarget;
			float durationForCaster = lfi.DelayForCaster + lfi.DurationForCaster;
			float durationForGlobal = lfi.DelayForGlobal + lfi.DurationForGlobal;
			float duration = Math.Max(Math.Max(durationForTarget, durationForCaster), durationForGlobal);
			lifetime = new DurationBasedLifetime(duration, true);
			return new List<Lifetime>(new[] {lifetime});
		}

		public override void OnCharacterDeath(Character deadCharacter) {
			base.OnCharacterDeath(deadCharacter);
			UnpauseEntity(targetEntity);
		}
	}
}