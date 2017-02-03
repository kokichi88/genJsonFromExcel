using System;
using System.Collections.Generic;
using Combat.Skills.Projectiles.Entity.Components;
using Core.Skills.Animations;
using Core.Skills.EventTriggers;
using Core.Skills.Jumps;
using Core.Skills.Modifiers;
using Core.Skills.Projectiles;
using Core.Skills.Vfxs;
using Core.Utils;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using MovementSystem;
using MovementSystem.Components;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using Ssar.Combat.Skills.Interactions;
using Ssar.Combat.Skills.Projectiles.Entity.Components;
using UnityEngine;
using Utils.DataStruct;
using ActionType = Ssar.Combat.Skills.Events.Actions.ActionType;

namespace Core.Skills {
	public abstract class Skill {
		private readonly uint instanceId;
		private BuffFactory buffFactory;
		private SkillLoopableElementFactory loopableElementFactory;
		private Environment environment;
		private Config config;

		protected List<ProjectileComponent> projectiles = new List<ProjectileComponent>();
		protected List<BaseEvent> pendingEventFrames = new List<BaseEvent>();
		private List<BaseEvent> processedEventFrames = new List<BaseEvent>();
		private float elapsed;
		private float phaseElapsed;
		private float duration;
		private float channelingDuration;
		private float stateBindingDuration;
		private Character caster;
		private List<Jump> jumps = new List<Jump>();
		private bool isInterrupted;
		private List<SsarTuple<Modifier, Character>> characterAndBuff = new List<SsarTuple<Modifier, Character>>();
		private List<Vfxs.Vfx> vfxs = new List<Vfxs.Vfx>();
		private List<Loopable> loopableElements = new List<Loopable>();
		protected Queue<List<BaseEvent>> pendingEventFramesByPhases = new Queue<List<BaseEvent>>();
		private float originalChannelingDuration;
		private float pauseDuration;
		private Dictionary<BaseEvent, float> offsetByEvent = new Dictionary<BaseEvent, float>();
		private Vector3 characterPosAtSkillStart;
		private Vector2 facingDirectionAtSkillStart;
		private bool isActive;
		private float originalStateBindingDuration;
		private bool isMovable;
		private bool isMoveBackward;
		private int orderOfCurrentPhase;

		protected Skill(uint instanceId, Character caster, BuffFactory buffFactory, SkillLoopableElementFactory loopableElementFactory,
		                Environment environment, Config config) {
			this.instanceId = instanceId;
			this.caster = caster;
			this.buffFactory = buffFactory;
			this.loopableElementFactory = loopableElementFactory;
			this.environment = environment;
			this.config = config;

			duration = config.Duration();
			channelingDuration = config.ChannelingDuration();
			stateBindingDuration = config.StateBindingDuration();
			// DLog.Log("debug skill " + config.ShowSkillId() + " instance " + instanceId);
		}

		public uint InstanceId => instanceId;

		public void EndChannelingSoonerBy(float reduction) {
			originalChannelingDuration = channelingDuration;
			channelingDuration -= reduction;
		}

		public void ReturnChannelingToOriginalValue() {
			channelingDuration = originalChannelingDuration;
		}

		public void EndStateBindingSoonerBy(float reduction) {
			originalStateBindingDuration = stateBindingDuration;
			stateBindingDuration -= reduction;
		}

		public void ReturnStateBindingToOriginalValue() {
			stateBindingDuration = originalStateBindingDuration;
		}

		public Character Caster() {
			return caster;
		}

		public virtual void AddEventFrame(int phaseIndex, BaseEvent eventFrame) {
			if(eventFrame == null) throw new Exception("Event frame is null");
			if (phaseIndex >= pendingEventFramesByPhases.Count) {
				pendingEventFramesByPhases.Enqueue(new List<BaseEvent>());
			}

			IEnumerator<List<BaseEvent>> enumer = pendingEventFramesByPhases.GetEnumerator();
			int index = -1;
			while (enumer.MoveNext()) {
				index++;
				if (index != phaseIndex) continue;

				List<BaseEvent> eventFrames = enumer.Current;
				eventFrames.Add(eventFrame);
			}
		}

		public void AddPendingEventFrame(BaseEvent be) {
			pendingEventFrames.Add(be);
		}

		public virtual void OnCast(Character character) {
			caster = character;
			if (pendingEventFramesByPhases.Count > 0) {
				pendingEventFrames.AddRange(pendingEventFramesByPhases.Dequeue());
			}
			TriggerEventFrame(0);
			orderOfCurrentPhase++;

			characterPosAtSkillStart = caster.Position();
			facingDirectionAtSkillStart = caster.FacingDirection().ToNormalizedVector2();
		}

		public void SetMovable(bool value) {
			this.isMovable = value;
		}

		public virtual bool IsMoveable() {
			return isMovable;
		}

		public void SetMoveBackward(bool value) {
			isMoveBackward = value;
		}

		public virtual bool IsMoveBackward() {
			return isMoveBackward;
		}

		public virtual bool IsJumpable() {
			return false;
		}

		public virtual bool IsChainableToCombo() {
			return false;
		}

		public virtual bool IsChainableToAirCombo() {
			return false;
		}

		public virtual string PrepareForChainingToCombo() {
			return string.Empty;
		}

		public virtual bool IsInterruptibleWhileChanneling() {
			return false;
		}

		public virtual void OnJumpBegin() {
		}

		public virtual void OnJumpEnd() {
		}

		public virtual void OnPreFinish(Character character) {
			for (int i = 0; i < characterAndBuff.Count; i++) {
				Character character_ = characterAndBuff[i].Element2;
				Modifier buff = characterAndBuff[i].Element1;
				character_.RemoveModifierAndChangeState(buff);
			}

			for (int kIndex = 0; kIndex < vfxs.Count; kIndex++) {
				vfxs[kIndex].Interrupt();
			}

			int count = loopableElements.Count;
			for (int i = 0; i < count; i++) {
				Loopable l = loopableElements[i];
				if (l is Vfx) {
					((Vfx)l).Interrupt();
				}
			}
//
//			for (int kIndex = 0; kIndex < loopableElements.Count; kIndex++) {
//				loopableElements[kIndex].Interrupt();
//			}

			DestroyProjectiles();
		}
		public abstract void OnFinish(Character character);

		public virtual void OnProjectileHitTargets(ProjectileComponent projectile,
		                                           List<Character> hitTargets,
		                                           List<float> weights,
		                                           List<Vector2> impactPositions) {
			if (IsProjectileTriggerNextPhase(projectile)) {
				SwitchToNextPhase();
			}
		}

		protected virtual bool IsProjectileTriggerNextPhase(ProjectileComponent projectile) {
			return true;
		}

		public void SwitchToNextPhase() {
			if (pendingEventFramesByPhases.Count > 0) {
				phaseElapsed = 0;
				pendingEventFrames.Clear();
				pendingEventFrames.AddRange(pendingEventFramesByPhases.Dequeue());
				orderOfCurrentPhase++;
				OnPhaseSwitched();
			}
		}

		protected void OffsetEvent(BaseEvent be, float offset) {
			offsetByEvent[be] = offset;
		}

		protected virtual void OnPhaseSwitched() {
		}

		public virtual ProjectileImpactResult OnMyProjectileBeHitByEnemyProjectile(ProjectileComponent myProjectile,
		                                                                           ProjectileComponent enemyProjectile) {
			return ProjectileImpactResult.Unchanged;
		}

		public virtual void OnProjectileContactTargets(ProjectileComponent projectile,
		                                               List<Character> hitTargets) {
		}

		public abstract void OnProjectileHitObstacles(ProjectileComponent projectile, List<GameObject> hitObstacles);

		public virtual void OnSuccessulShieldBlock(ProjectileComponent shieldProjectile) {
			//DLog.Log("Skill blocks projectile successfully");
		}

		public virtual void Update(float dt) {
			if(isInterrupted) return;
			if (pauseDuration > 0) {
				pauseDuration -= dt;
				pauseDuration = Mathf.Max(0, pauseDuration);
				if (pauseDuration > 0) return;
			}

			elapsed += dt;
			phaseElapsed += dt;
			TriggerEventFrame(dt);
//			DLog.LogError(this.GetType()+" "+BattleUtils.frame+" "+BattleUtils.time);

			for (int kIndex = loopableElements.Count - 1; kIndex >=0 ; kIndex--) {
				Loopable loopableElement = loopableElements[kIndex];
				if (loopableElement.IsFinished()) {
					loopableElements.RemoveAt(kIndex);
					if (loopableElement is Jump) {
						jumps.Remove((Jump) loopableElement);
					}
					else if (loopableElement is Vfxs.Vfx) {
						vfxs.Remove((Vfxs.Vfx) loopableElement);
					}
					continue;
				}

				loopableElement.Update(dt);
			}

			OnUpdate(dt);
		}

		public void LateUpdate(float dt) {
			for (int kIndex = loopableElements.Count - 1; kIndex >= 0; kIndex--) {
				Loopable loopableElement = loopableElements[kIndex];
				loopableElement.LateUpdate(dt);
			}
			OnLateUpdate(dt);
		}

		public void PauseIndependentUpdate(float dt) {
			OnPauseIndependentUpdate(dt);
		}

		protected void TriggerEventFrame(float dt) {
			for (int i = pendingEventFrames.Count - 1; i >= 0; i--) {
				BaseEvent ef = pendingEventFrames[i];
				if(ef.ShowTrigger().ShowTriggerType() != TriggerType.Frame) continue;
				TimelineTrigger timelineTrigger = (TimelineTrigger) ef.ShowTrigger();
				float activeAt = timelineTrigger.ShowScaledFrameInSeconds();
				activeAt = activeAt / config.AttackSpeed();
				if (offsetByEvent.ContainsKey(ef)) {
					activeAt += offsetByEvent[ef];
				}

				float timelineElapsed = elapsed;
				if (timelineTrigger.relative) {
					timelineElapsed = phaseElapsed;
				}
				if (timelineElapsed < activeAt) continue;

				pendingEventFrames.RemoveAt(i);
				processedEventFrames.Add(ef);
				Trigger(ef, GetEventFrameArgsFor(ef));
			}
		}

		protected virtual TemplateArgs GetEventFrameArgsFor(BaseEvent baseEvent) {
			return null;
		}

		public virtual bool IsFinish() {
			return elapsed >= duration || isInterrupted;
		}

		public virtual bool IsChannelingFinish() {
			return IsFinish() || elapsed >= channelingDuration || isInterrupted;
		}

		public virtual bool IsStateBindingFinish() {
			return IsFinish() || elapsed >= stateBindingDuration || isInterrupted;
		}

		protected internal virtual bool IgnoreMinSpeedOnAirForDashes() {
			return false;
		}

		public virtual void Interrupt() {
			isInterrupted = true;

			for (int kIndex = 0; kIndex < loopableElements.Count; kIndex++) {
				loopableElements[kIndex].Interrupt();
			}

			DestroyProjectiles();
		}

		private void DestroyProjectiles() {
			for (int kIndex = 0; kIndex < projectiles.Count; kIndex++) {
				ProjectileComponent p = projectiles[kIndex];
				try {
					if (p.IsDestroyed) continue;
					((ProjectileGameObjectComponent) p.Entity.GetComponent<EntityGameObjectComponent>()).Destroy();
					p.Destroy();
				}
				catch (Exception e) {
					DLog.LogException(e);
				}
			}
		}

		internal void InterruptLoopable<T>()
		{
			List<Loopable> removedLoopables = new List<Loopable>();
			for (int kIndex = 0; kIndex < loopableElements.Count; kIndex++)
			{
				Loopable loopable = loopableElements[kIndex];
				if (loopable is T)
				{
					removedLoopables.Add(loopable);
					loopable.Interrupt();
				}
			}

			for (int i = 0; i < removedLoopables.Count; i++)
			{
				RemoveLoopable(removedLoopables[i]);
			}
		}

		public abstract List<ProjectileComponent> LaunchProjectiles(BaseEvent ef);

		protected virtual void OnUpdate(float dt) {
		}

		protected virtual void OnLateUpdate(float dt) {
		}

		protected virtual void OnPauseIndependentUpdate(float dt) {
		}

		public virtual void OnCasterDeath(Character character) {
			/*DLog.Log(string.Format(
				"Skill:{0}:OnCaster:{1}", config.ShowSkillId(), character.GameObject().name
			));*/
		}

		protected void PauseFor(float seconds) {
			pauseDuration += seconds;
		}

		protected void Unpause() {
			pauseDuration = 0;
		}

		protected bool IsInterrupted() {
			return isInterrupted;
		}

		public float Elapsed {
			get { return elapsed; }
		}

		public void AddLoopable(Loopable l) {
			if (l != null) {
				loopableElements.Add(l);
			}
		}

		protected bool RemoveLoopable(Loopable l) {
			return loopableElements.Remove(l);
		}

		public virtual AcceptWindow InputAcceptWindow() {
			return config.InputAcceptWindow();
		}

		public virtual AttackStateWindow AttackStateWindow() {
			return config.AttackStateWindow();
		}

		public SkillCastingSource ShowCastingSource() {
			return config.ShowSkillCastingSource();
		}

		public Vector3 CharacterPositionAtSkillStart => characterPosAtSkillStart;

		public Vector2 CharacterFacingDirectionAtSkillStart => facingDirectionAtSkillStart;

		public virtual void PauseForLockFrame() {
		}

		public virtual void UnpauseForLockFrame() {
		}

		public interface Config {
			float Duration();
			float ChannelingDuration();
			float StateBindingDuration();
			bool IsActiveSkill();
			AcceptWindow InputAcceptWindow();
			AttackStateWindow AttackStateWindow();
			CollectionOfInteractions ModifierInteractionCollection();
			SkillCastingSource ShowSkillCastingSource();
			SkillId ShowSkillId();
			float AttackSpeed();
		}

		public virtual void OnDamageDealt(Character caster, Character target,
		                                  Skill fromSkill, Modifier fromModifier, int damage, bool critical) {
		}

		public virtual void OnComboCount(int combo) {
			//DLog.Log("Skill:" + config.ShowSkillId() + ":OnComboCount: " + combo);
		}

		public virtual void OnPreDamageCalculation(Character caster, Character target,
		                                           DamageFromAttack.SourceHistory dmgSourceHistory) {
			//DLog.Log("Skill:OnPreDamageCalculation " + caster.Id() + " " + target.Id() + " " + dmgSourceHistory.ShowOrigin().type);
		}

		protected void ReduceDurationBy(float value) {
			value = Math.Max(0, value);
			duration -= value;
			duration = Math.Max(0, duration);
		}

		public void IncreaseDurationBy(float value) {
			value = Math.Max(0, value);
			duration += value;
			duration = OnDurationIncreasedBy(value, duration);
		}

		protected virtual float OnDurationIncreasedBy(float value, float newDuration) {
			return newDuration;
		}

		protected float Duration {
			get { return duration; }
		}

		public void TriggerEventWithId(int id, TemplateArgs args = null) {
			for (int i = pendingEventFrames.Count - 1; i >= 0; i--) {
				BaseEvent ef = pendingEventFrames[i];
				if(ef.ShowTrigger().ShowTriggerType() != TriggerType.Event) continue;

				EventBasedTrigger trigger = (EventBasedTrigger) ef.ShowTrigger();
				if (trigger.id != id) continue;

				Trigger(ef, args);
			}
		}

		protected void Trigger(BaseEvent be, TemplateArgs args = null) {
			try {
				if (!IsEventTriggerable(be)) return;

				BaseAction ba = be.ShowAction();
				ActionType actionType = ba.ShowActionType();
				switch (actionType) {
					case ActionType.CastProjectile:
					case ActionType.Impact:
						List<ProjectileComponent> justLaunchedProjectiles = LaunchProjectiles(be);
						projectiles.AddRange(justLaunchedProjectiles);
						break;
					default:
						Loopable loopable = loopableElementFactory.Produce(
							caster, this, config.ShowSkillId(), be, config.ShowSkillCastingSource(), args
						);
						loopableElements.Insert(0, loopable);
						if (actionType == ActionType.Jump) {
							jumps.Add((Jump) loopable);
						}
						OnLoopableElementProduction(be, loopable);
						break;
				}
			}
			catch (Exception e) {
				Exception ex = new Exception("Error trigger event of skill id " + config.ShowSkillId(), e);
				DLog.LogException(ex);
			}
		}

		protected virtual void OnLoopableElementProduction(BaseEvent be, Loopable loopable) {
		}

		protected virtual bool IsEventTriggerable(BaseEvent be) {
			return true;
		}

		public void OnGroundEvent() {
			for (int kIndex = 0; kIndex < jumps.Count; kIndex++) {
				jumps[kIndex].OnCharacterGround();
			}
		}

		public virtual void OnProjectileDestroy(ProjectileComponent projectile) {
			projectiles.Remove(projectile);
		}

		public virtual void OnProjectileLockTarget(ProjectileComponent projectile) {
		}

		public virtual bool ShouldConsumeCastingRequest() {
			return true;
		}

		public virtual void OnKillEnemy(Character deadCharacter, List<DamageFromAttack.SourceFromSkill> sourceFromSkills) {
			/*DLog.Log(string.Format(
				"Skill:{0}:OnKillEnemy:{1}", config.ShowSkillId(), deadCharacter.GameObject().name
			));*/
		}

		public virtual bool OnReceiveFatalDamage(Character dealer, int damage,
		                                 int healthBeforeFatalDamage,
		                                 int healthAfterFatalDamage,
		                                 out int outDamage, out int outHealthAfterFatalDamage) {
			outDamage = damage;
			outHealthAfterFatalDamage = 1;
			return false;
		}

		public virtual void Deactivate() {
			isActive = false;
		}

		public virtual void Activate() {
			isActive = true;
		}

		protected bool IsActive() {
			return isActive;
		}

		public virtual bool IsInterruptible() {
			return true;
		}

		public virtual bool IsInterruptibleByDashInput(out AcceptWindow dashWindow) {
			dashWindow = null;
			return false;
		}

		public virtual bool IsInterruptibleByJumpInput(out AcceptWindow jumpWindow) {
			jumpWindow = null;
			return false;
		}

		public void SkipToTime(float time) {
			float deltaElapsed = time - elapsed;
			if (deltaElapsed > 0) {
				elapsed += deltaElapsed;
			}

			float deltaPhaseElapsed = time - phaseElapsed;
			if (deltaPhaseElapsed > 0) {
				phaseElapsed += deltaPhaseElapsed;
			}

			for (int i = pendingEventFrames.Count - 1; i >= 0; i--) {
				BaseEvent ef = pendingEventFrames[i];
				if(ef.ShowTrigger().ShowTriggerType() != TriggerType.Frame) continue;
				TimelineTrigger timelineTrigger = (TimelineTrigger) ef.ShowTrigger();
				float activeAt = timelineTrigger.ShowScaledFrameInSeconds();
				activeAt = activeAt / config.AttackSpeed();
				if (offsetByEvent.ContainsKey(ef)) {
					activeAt += offsetByEvent[ef];
				}

				float timelineElapsed = elapsed;
				if (timelineTrigger.relative) {
					timelineElapsed = phaseElapsed;
				}
				if (timelineElapsed < activeAt) continue;

				pendingEventFrames.RemoveAt(i);
				processedEventFrames.Add(ef);
			}

			foreach (Loopable loopable in loopableElements) {
				if (loopable is AnimationPlayback) {
					((AnimationPlayback)loopable).JumpToTime(time);
				}
			}
		}

		public int OrderOfCurrentPhase => orderOfCurrentPhase;

		public float ChannelingDuration => channelingDuration;

		public List<ProjectileComponent> Projectiles => projectiles;
	}
}
