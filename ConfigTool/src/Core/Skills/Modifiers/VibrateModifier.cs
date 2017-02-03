using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MEC;
using MovementSystem.Requests;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class VibrateModifier : BaseModifier {
		private VibrateInfo info;
		private Entity casterEntity;
		private Entity targetEntity;

		private float elapsed;
		private VibrationRequest.Vibration vibration;
		private ModifierAttachType attachType;
		private CoroutineHandle coroutineHandle;
		private DurationBasedLifetime lifetime;
		private float duration;

		public VibrateModifier(VibrateInfo info, Entity casterEntity, Entity targetEntity, Environment environment,
		                       CollectionOfInteractions modifierInteractionCollection)
			: base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = info;
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;
			duration = this.info.ShowDuration();
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.Vibrate;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;

			if (vibration.Completed) {
				lifetime.End();
			}
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			if (vibration == null) return;
			vibration.Stop();
			Timing.KillCoroutines(coroutineHandle);
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			GameObject targetGo = targetEntity.GetComponent<EntityGameObjectComponent>().GameObject;
			vibration = VibrateTarget(
				info.XAmplitude, duration, info.Frequency, info.ShouldDecay, info.DecayConstant,
				targetGo, () => { }
			);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			if (vibration == null) return;
			vibration.Stop();
			Timing.KillCoroutines(coroutineHandle);
		}

		public override object[] Cookies() {
			return new object[0];
		}

		public override void OnCharacterDeath(Character deadCharacter) {
			if (vibration == null) return;
			vibration.Stop();
			Timing.KillCoroutines(coroutineHandle);
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			foreach (Lifetime l in lifetimes) {
				if (l is DurationBasedLifetime) {
					lifetime = (DurationBasedLifetime) l;
				}
			}
			return lifetimes;
		}

		private VibrationRequest.Vibration VibrateTarget(float xAmplitude, float duration, int frequency,
		                                                 bool shouldDecay, float decayConstant,
		                                                 GameObject go, Action onComplete) {
			VibrationRequest.Vibration v = new VibrationRequest.Vibration(
				new Pos(go.transform.Find("Renderer")),
				xAmplitude, duration, frequency, shouldDecay, decayConstant
			);
			coroutineHandle = Timing.RunCoroutine(_VibrateTarget(v, onComplete));
			return v;
		}

		private IEnumerator<float> _VibrateTarget(VibrationRequest.Vibration v, Action onComplete) {
			while (true) {
				float waitTime = 0.02f;
				yield return Timing.WaitForSeconds(waitTime);
				v.Update(waitTime);
				if (v.Completed) {
					onComplete();
					break;
				}
			}
		}

		private class Pos : VibrationRequest.LocalPosition {
			private Transform t;

			public Pos(Transform t) {
				this.t = t;
			}

			public Vector2 Get() {
				//DLog.Log("local pos " + t.localPosition);
				return t.localPosition;
			}

			public void Set(Vector2 value) {
				t.localPosition = new Vector3(value.x, value.y, t.localPosition.z);
			}
		}
	}
}