using System;
using System.Collections.Generic;
using Artemis;
using Checking;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Skills.Vfxs;
using Core.Utils;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MEC;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Animation = Ssar.Combat.HeroStateMachines.Animation;
using MType = Combat.Skills.ModifierConfigs.ModifierType;
using SpawnPrefab = Core.Skills.Vfxs.Vfx.SpawnPrefab;
using ChangeMaterialColor = Core.Skills.Vfxs.Vfx.ChangeMaterialColor;
using ChangeMaterial = Core.Skills.Vfxs.Vfx.ChangeMaterial;
using SpawnPrefabVfx = Ssar.Combat.Skills.Events.Actions.VfxAction.SpawnPrefabVfx;
using VfxLogic = Core.Skills.Vfxs.Vfx.Logic;

namespace Core.Skills.Modifiers {
	public abstract class BaseModifier : Modifier {
		private ModifierInfo info;
		protected Entity casterEntity;
		protected Entity targetEntity;
		private List<VfxConfig> vfxs;
		private Environment environment;
		private CollectionOfInteractions modifierInteractionCollection;

		protected ModifierAttachType attachType;
		private GameObject targetGameObject;
		private Animation targetAnimation;
		private MovingJumpRequest movingJumpRequest;
		private DashRequest dashRequest;
		private float delayToApplyElapsed;
		private bool isAttachCalled;
		private Character target;
		protected List<Lifetime> mainLifetimes;
		private UnpredictableDurationLifetime subLifetime = new UnpredictableDurationLifetime();
		private IncreaseLyingDuration increaseLyingDuration;
		private List<VfxConfig> pendingVfxs = new List<VfxConfig>();
		private float elapsed;
		private List<VfxLogic> vfxLogics = new List<VfxLogic>();
		private List<VfxLogic> startupVfxLogics = new List<VfxLogic>();
		private bool isOverrideMode;
		private bool isOverridingModifierOfSameType;
		private bool isStartupVfxPlayed;
		private CoroutineHandle startupVfxCoroutine;
		private Dictionary<Lifetime, LifetimeConfig> configByLifetime = new Dictionary<Lifetime, LifetimeConfig>();
		private bool isReplaced;
		private bool isDetached;

		protected BaseModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                       Environment environment, CollectionOfInteractions modifierInteractionCollection) {
			NotNullReference nnr = new NotNullReference();
			nnr.Check(info, "modifier info");
			nnr.Check(casterEntity, "caster entity");
			nnr.Check(targetEntity, "target entity");
			this.info = info;
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;
			this.environment = environment;
			this.modifierInteractionCollection = modifierInteractionCollection;
			this.vfxs = info.ShowVfxConfig();

			pendingVfxs.AddRange(vfxs);
			targetGameObject = targetEntity.GetComponent<EntityGameObjectComponent>().GameObject;
			targetAnimation = targetEntity.GetComponent<AnimationComponent>().Animation;
			target = targetEntity.GetComponent<SkillComponent>().Character;
			mainLifetimes = CreateLifetimes(info);
			for (int kIndex = 0; kIndex < mainLifetimes.Count; kIndex++) {
				Lifetime l = mainLifetimes[kIndex];
				if (l.ShowType() == LifetimeType.Duration) {
					((DurationBasedLifetime) l).SetDelay(info.DelayToApply());
				}
			}
		}

		public virtual string Name() {
			return Type().ToString();
		}

		public abstract ModifierType Type();

		public virtual int SubType() {
			return 1;
		}

		public void Update(float dt) {
			elapsed += dt;

			if (movingJumpRequest != null) {
				if (movingJumpRequest.IsCompleted()) {
					subLifetime.End();
				}
			}

			if (dashRequest != null) {
				if (dashRequest.IsCompleted()) {
					subLifetime.End();
				}
			}

			delayToApplyElapsed += dt;
			if (delayToApplyElapsed < info.DelayToApply()) return;
			if (attachType == ModifierAttachType.Main && !isAttachCalled) {
				isAttachCalled = true;
				OnDelayedAttachAsMain(target);
			}

			if (increaseLyingDuration != null) {
				increaseLyingDuration.Update(dt);
			}

			ProcessVfx(dt);

			OnUpdate(dt);

			for (int kIndex = 0; kIndex < mainLifetimes.Count; kIndex++) {
				mainLifetimes[kIndex].Update(dt);
			}
		}

		public void LateUpdate(float dt) {
			OnLateUpdate(dt);
		}

		protected abstract void OnUpdate(float dt);

		protected virtual void OnLateUpdate(float dt) {
		}

		public abstract bool IsBuff();

		public virtual void OnReplaceOtherModifiers(Character target, List<Modifier> others) {
			isOverrideMode = true;

			for (int kIndex = 0; kIndex < others.Count; kIndex++) {
				Modifier overriddenModifier = others[kIndex];
				if (overriddenModifier.Type() == Type()) {
					isOverridingModifierOfSameType = true;
					//DLog.Log("override modifier of same type " + Type());
				}
			}
		}

		public virtual void OnBeReplaced(Character target, Modifier byModifier) {
			isReplaced = true;
			if (increaseLyingDuration != null) {
				increaseLyingDuration.Reset();
			}

			DestroyVfx();
		}

		public void OnAttachAsMain(Character target) {
			this.target = target;
			attachType = ModifierAttachType.Main;
			if (info.DelayToApply() == 0) {
				isAttachCalled = true;
				OnDelayedAttachAsMain(target);
			}
			ProcessVfx(0);
		}

		public bool OnAttachAsSub(Character target) {
			attachType = ModifierAttachType.Sub;
			bool attachSuccess = false;

			try {
				Modifier highest = target.FindModifierOfHighestRank();
				MType highestType = (MType) (int) highest.Type();
				MType thisType = (MType) (int) Type();
				List<Interaction> inters = modifierInteractionCollection.Find(highestType, thisType);
				if (inters.Count < 1) {
					return attachSuccess;
				}

				Interaction inter = inters[0];
				bool isStateQuerySuccess = false;
				MainModifierState mainState = MainModifierState.Ground;
				if (highest is BaseModifier) {
					bool justChanged;
					isStateQuerySuccess = ((BaseModifier) highest).TryQueryingState(out mainState, out justChanged);
				}

				if (!isStateQuerySuccess) {
					mainState = target.IsOnGround()
						? MainModifierState.Ground
						: MainModifierState.Air;
				}

				List<ActionDetails> actions = inter.FindActions(mainState);
				if (actions.Count < 1) {
					return attachSuccess;
				}

				for (int i = 0; i < actions.Count; i++) {
					ActionDetails ad = actions[i];
					switch (ad.ShowActionType()) {
						case InteractionActionType.DoNothing:
							attachSuccess = false;
							break;
						case InteractionActionType.Vibrate:
							VibrateActionDetails vad = (VibrateActionDetails) ad;
							VibrateTarget(
								vad.xAmplitude, vad.duration, vad.frequency, vad.shouldDecay,
								vad.decayConstant, targetGameObject, () => subLifetime.End()
							);
							attachSuccess = true;
							break;
						case InteractionActionType.PlayAnimation:
							PlayAnimationActionDetails paad = (PlayAnimationActionDetails) ad;
							if (targetAnimation.CurrentAnimationNames().Contains(paad.name)) {
								targetAnimation.Stop();
							}

							targetAnimation.PlayAnimation(paad.name);
							targetAnimation.JumpToFrame(paad.startFrame);
							if (highest is StunModifier) {
								((StunModifier) highest).OnAnimationPlayedBySubModifier(paad.name);
							}
							subLifetime.End();
							attachSuccess = true;
							break;
						case InteractionActionType.Jump:
							JumpActionDetails jad = (JumpActionDetails) ad;
							movingJumpRequest = new MovingJumpRequest(
								jad.height, jad.timeToPeak, jad.timeToGround, jad.timeToFloat, jad.distance
							);
							targetEntity.GetComponent<MovementComponent>().AddMovementRequest(
								movingJumpRequest
							);
							attachSuccess = true;
							break;
						case InteractionActionType.Dash:
							MovementComponent casterMc = casterEntity.GetComponent<MovementComponent>();
							Direction casterFacing = casterMc.FacingDirection;
							DashActionDetails dad = (DashActionDetails) ad;
							dashRequest = new DashRequest(
								casterFacing, dad.distance, dad.duration, 0, dad.constantSpeed
							);
							targetEntity.GetComponent<MovementComponent>().AddMovementRequest(
								dashRequest
							);
							attachSuccess = true;
							break;
						case InteractionActionType.OverrideMainModifier:
							attachType = ModifierAttachType.Main;
							target.RemoveModifier(highest);
							OnAttachAsMain(target);
							attachSuccess = true;
							break;
						case InteractionActionType.IncreaseLyingDuration:
							StunInfo si = (StunInfo) info;
							increaseLyingDuration = new IncreaseLyingDuration(
								subLifetime, si.ShowDuration(), targetEntity
							);
							attachSuccess = true;
							break;
						default:
							throw new Exception(
								"Missing logic to handle SubModifier of ActionType " + ad.ShowActionType()
							);
					}
				}

				return attachSuccess;
			}
			catch (Exception e) {
				DLog.LogError(e);
				return attachSuccess;
			}
		}

		public ModifierAttachType ShowAttachType() {
			return attachType;
		}

		public virtual void OnDetach(Character target) {
			isDetached = true;
			if (increaseLyingDuration != null) {
				increaseLyingDuration.Reset();
			}

			DestroyVfx();
		}

		public bool IsFinish() {
			if (isDetached || isReplaced) {
				return true;
			}
			if (attachType == ModifierAttachType.Main) {
				for (int kIndex = 0; kIndex < mainLifetimes.Count; kIndex++) {
					Lifetime mainLifetime = mainLifetimes[kIndex];
					if (mainLifetime.IsEnd()) {
						Skill parentSkill = info.ShowParentSkill();
						if(parentSkill != null && configByLifetime.ContainsKey(mainLifetime)){
							parentSkill.TriggerEventWithId(configByLifetime[mainLifetime].eId);
						}
						return true;
					}
				}

				return false;
			}
			else {
				return subLifetime.IsEnd();
			}
		}

		public virtual object[] Cookies() {
			return new[] {info};
		}

		public void CheckLifetimes() {
			if (attachType == ModifierAttachType.Main) {
				for (int kIndex = 0; kIndex < mainLifetimes.Count; kIndex++) {
					mainLifetimes[kIndex].Check();
				}
			}
			else {
				subLifetime.Check();
			}
		}

		public virtual void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			foreach (Lifetime lifetime in mainLifetimes) {
				lifetime.OnDamageDealt(caster, target, fromSkill, fromModifier, damage);
			}
		}

		public virtual void OnCreateAsBuffFromSkill(Skill parentSkill) {
		}

		public virtual void OnCharacterDeath(Character deadCharacter) {
		}

		public List<Lifetime> ShowLifetimes() {
			return mainLifetimes;
		}

		public float ShowAge() {
			return elapsed;
		}

		public virtual StackResult TryStackWithNewOne(Modifier newOne) {
			return StackResult.None;
		}

		public virtual int ShowStackCount() {
			return 0;
		}

		public string ShowIcon() {
			return info.ShowIcon();
		}

		public virtual bool IsInvalidated() {
			return false;
		}

		public virtual bool IsValidated() {
			return true;
		}

		protected virtual List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = new List<Lifetime>();
			foreach (LifetimeConfig lc in info.ShowLifetimeConfigs()) {
				switch (lc.ShowType()) {
					case LifetimeType.Duration:
						DurationInSecondsLifetimeConfig dis = (DurationInSecondsLifetimeConfig) lc;
						DurationBasedLifetime durationBasedLifetime = new DurationBasedLifetime(dis.duration);
						configByLifetime[durationBasedLifetime] = lc;
						lifetimes.Add(durationBasedLifetime);
						break;
					case LifetimeType.ParentSkill:
						ParentSkillBasedLifetime parentSkillBasedLifetime = new ParentSkillBasedLifetime(info.ShowParentSkill());
						configByLifetime[parentSkillBasedLifetime] = lc;
						lifetimes.Add(parentSkillBasedLifetime);
						break;
					case LifetimeType.DurationInFrames:
						DurationInFramesLifetimeConfig dif = (DurationInFramesLifetimeConfig) lc;
						durationBasedLifetime = new DurationBasedLifetime(FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(dif.duration));
						configByLifetime[durationBasedLifetime] = lc;
						lifetimes.Add(durationBasedLifetime);
						break;
					case LifetimeType.SuccessfulHit:
						SuccessfulHitLifetimeConfig shl = (SuccessfulHitLifetimeConfig) lc;
						SuccessfulHitLifetime successfulHitLifetime = new SuccessfulHitLifetime(shl.count, shl.ShowCategories(), targetEntity.GetComponent<SkillComponent>().Character);
						configByLifetime[successfulHitLifetime] = lc;
						lifetimes.Add(successfulHitLifetime);
						break;
					case LifetimeType.SpecificSkillStateExit:
						SpecificSkillStateExitLifetimeConfig ssselc = (SpecificSkillStateExitLifetimeConfig) lc;
						SpecificSkillStateExitLifetime specificSkillStateExitLifetime = new SpecificSkillStateExitLifetime(
							ssselc, targetEntity.GetComponent<SkillComponent>().Character
						);
						configByLifetime[specificSkillStateExitLifetime] = ssselc;
						lifetimes.Add(specificSkillStateExitLifetime);
						break;
					case LifetimeType.SpecificSkillFinish:
						SpecificSkillFinishLifetimeConfig ssflc = (SpecificSkillFinishLifetimeConfig) lc;
						SpecificSkillFinishLifetime specificSkillFinishLifetime = new SpecificSkillFinishLifetime(
							ssflc, targetEntity.GetComponent<SkillComponent>().Character
						);
						configByLifetime[specificSkillFinishLifetime] = ssflc;
						lifetimes.Add(specificSkillFinishLifetime);
						break;
					case LifetimeType.ParentSkillHitTarget:
						ParentSkillHitTargetLifetime pshtl = new ParentSkillHitTargetLifetime(
							targetEntity.GetComponent<SkillComponent>().Character, modifierInfo.ShowParentSkill()
						);
						configByLifetime[pshtl] = lc;
						lifetimes.Add(pshtl);
						break;
					case LifetimeType.ParentSkillStateExit:
						ParentSkillStateExitLifetime pssel = new ParentSkillStateExitLifetime(modifierInfo.ShowParentSkill());
						configByLifetime[pssel] = lc;
						lifetimes.Add(pssel);
						break;
					default:
						throw new Exception("Missing logic to create lifetime of type " + lc.ShowType());
				}
			}

			return lifetimes;
		}

		protected abstract void OnDelayedAttachAsMain(Character target);

		protected VibrationRequest.Vibration VibrateTarget(float xAmplitude, float duration, int frequency,
		                                                 bool shouldDecay, float decayConstant,
		                                                 GameObject go, Action onComplete) {
			VibrationRequest.Vibration v = new VibrationRequest.Vibration(
				new Pos(go.transform.Find("Renderer")),
				xAmplitude, duration, frequency, shouldDecay, decayConstant
			);
			Timing.RunCoroutine(_VibrateTarget(v, onComplete));
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

		private void ProcessVfx(float dt) {
			for (int i = pendingVfxs.Count - 1; i >= 0; i--) {
				try {
					VfxConfig vc = pendingVfxs[i];
					if (elapsed >= FrameAndSecondsConverter._30Fps.FramesToSeconds(vc.frame)) {
						pendingVfxs.RemoveAt(i);
						VfxLogic logic = null;
						VfxAction.VfxType vfxType = vc.vfx.ShowVfxType();
						switch (vfxType) {
							case VfxAction.VfxType.SpawnPrefab:
								SpawnPrefabVfx spv = (SpawnPrefabVfx) vc.vfx;
								VfxTag vfxTag = spv.ShowVfxPrefab().GetComponent<VfxTag>();
								if (vfxTag) {
									if (vfxTag.tag == VfxTag.Tag.Startup) {
										if (isOverrideMode && isOverridingModifierOfSameType) {
											//DLog.Log("Skip spawn startup vfx");
											continue;
										}

										if (!isStartupVfxPlayed) {
											startupVfxCoroutine = Timing.RunCoroutine(LoopStartupVfx_());
										}
										isStartupVfxPlayed = true;
										//DLog.Log("Spawn startup vfx");
										startupVfxLogics.Add(new SpawnPrefab(
											vc.ttl, (SpawnPrefabVfx) vc.vfx,
											new Vfx.DefaultVfxGameObjectFactory(environment), target,
											SkillCastingSource.FromUserInput(), environment.GetCamera(),
											environment, null
										));
										continue;
									}
								}
								logic = new SpawnPrefab(
									vc.ttl, (SpawnPrefabVfx) vc.vfx,
									new Vfx.DefaultVfxGameObjectFactory(environment), target,
									SkillCastingSource.FromUserInput(), environment.GetCamera(),
									environment, null
								);
								OnVfxPrefabSpawn((SpawnPrefab) logic);
								break;
							case VfxAction.VfxType.ChangeMaterial:
								logic = new ChangeMaterial(
									(VfxAction.ChangeMaterialVfx) vc.vfx, vc.ttl, environment, target
								);
								break;
							case VfxAction.VfxType.ChangeMaterialColor:
								logic = new ChangeMaterialColor(
									(VfxAction.ChangeMaterialColorVfx) vc.vfx, vc.ttl, environment, target
								);
								break;
							case VfxAction.VfxType.AddMaterial:
								logic = new AddMaterialLogic(
									(VfxAction.AddMaterialVfx) vc.vfx, vc.ttl, target
								);
								break;
							default:
								throw new Exception("Missing logic to create vfx logic of type " + vfxType);
						}
						vfxLogics.Add(logic);
					}
				}
				catch (Exception e) {
					DLog.LogException(e);
				}
			}

			for (int i = vfxLogics.Count - 1; i >= 0; i--) {
				vfxLogics[i].Update(dt);
				if (vfxLogics[i].IsFinish()) {
					vfxLogics.RemoveAt(i);
				}
			}
		}

		protected virtual void OnVfxPrefabSpawn(SpawnPrefab logic) {
		}

		private IEnumerator<float> LoopStartupVfx_() {
			float waitTime = 0.02f;
			while (true) {
				yield return Timing.WaitForSeconds(waitTime);
				for (int kIndex = startupVfxLogics.Count - 1; kIndex >= 0; kIndex--) {
					startupVfxLogics[kIndex].Update(waitTime);
					if (startupVfxLogics[kIndex].IsFinish()) {
						startupVfxLogics[kIndex].DestroyVfx();
						startupVfxLogics.RemoveAt(kIndex);
					}
				}

				if (startupVfxLogics.Count < 1) break;
			}
		}

		private void DestroyVfx() {
			for (int kIndex = 0; kIndex < vfxLogics.Count; kIndex++) {
				vfxLogics[kIndex].Interrupt();
				vfxLogics[kIndex].DestroyVfx();
			}
		}

		public virtual bool TryQueryingState(out MainModifierState value, out bool justChanged) {
			value = MainModifierState.Ground;
			justChanged = false;
			return false;
		}

		private class Pos : VibrationRequest.LocalPosition {
			private Transform t;

			public Pos(Transform t) {
				this.t = t;
			}

			public Vector2 Get() {
				return t.localPosition;
			}

			public void Set(Vector2 value) {
				t.localPosition = value;
			}
		}

		private class IncreaseLyingDuration {
			private UnpredictableDurationLifetime subLifetime;
			private float duration;

			private Stats extraLying;
			private ValueModifier vm;

			public IncreaseLyingDuration(UnpredictableDurationLifetime subLifetime, float duration, Entity targetEntity) {
				this.subLifetime = subLifetime;
				this.duration = duration;
				StatsComponent sc = targetEntity.GetComponent<StatsComponent>();
				bool found;
				extraLying = sc.CharacterStats.FindStats(StatsType.ExtraLyingDuration, out found);
				if (found) {
					vm = extraLying.AddModifier(StatsModifierOperator.Addition, duration);
				}
			}

			public void Update(float dt) {
				duration -= dt;
				if (duration <= 0) {
					subLifetime.End();
					Reset();
				}
			}

			public void Reset() {
				if (extraLying != null) {
					extraLying.RemoveModifier(vm);
				}
			}
		}
	}
}