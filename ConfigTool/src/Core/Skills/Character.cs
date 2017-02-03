using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Artemis;
using Checking;
using Core.Commons;
using Core.Skills.Cooldowns;
using Core.Skills.Exceptions;
using Core.Skills.Modifiers;
using Core.Utils.Extensions;
using MovementSystem.Components;
using MovementSystem.Requests;
using UnityEngine;

namespace Core.Skills {
	public abstract class Character {
		private static ModifierType[] DEBUFF_WITHOUT_STATE_CHANGE = new[] {
			ModifierType.Attack, ModifierType.MovementSpeed, ModifierType.Def, ModifierType.Burn,
			ModifierType.Healing, ModifierType.ReduceDamageByDistance, ModifierType.CritDamageOverride,
			ModifierType.AttackOverride, ModifierType.CritRateOverride, ModifierType.MaxHpOverride,
			ModifierType.DefOverride, ModifierType.MagicResistOverride, ModifierType.KnockbackWeight,
			ModifierType.KnockdownWeight, ModifierType.Cursed, ModifierType.Scream, ModifierType.ColdBurn,
			ModifierType.Static, ModifierType.SkillCritRate, ModifierType.SkillCritDamage,
			ModifierType.ArenaRuneArcane, ModifierType.ArenaRuneAttack, ModifierType.ArenaRuneHaste,
			ModifierType.ArenaRuneHealth, ModifierType.ActiveSkillCooldown,
			ModifierType.Warcry, ModifierType.Bless, ModifierType.Bleed, ModifierType.WildHowl,
			ModifierType.Immune, ModifierType.LockFrame, ModifierType.AdvancedFrame, ModifierType.Vibrate,
			ModifierType.PauseMovement, ModifierType.PauseAnimation, ModifierType.PlayAnimation,
			ModifierType.DamageOverTime, ModifierType.HitboxTransform, ModifierType.KnockbackWeight,
			ModifierType.Stats, ModifierType.Invisible, ModifierType.Dash, ModifierType.ColliderConfig,
			ModifierType.SuperArmor, ModifierType.WeakArmor, ModifierType.Scale, ModifierType.MovementSpeed2,
			ModifierType.StunBreak, ModifierType.AetherOnDamaged, ModifierType.Sfx, ModifierType.SuperAtk,
			ModifierType.Wind, ModifierType.Recast
		};

		private static ModifierType[] MODIFIER_WITHOUT_STATE_CHANGE = new ModifierType[0];

		private SkillFactory skillFactory;

		private List<Skill> ongoingSkills = new List<Skill>();
		private List<Modifier> ongoingModifiers = new List<Modifier>();
		private Dictionary<SkillId, SkillCastingRequirement> skillCastingRequirements = new Dictionary<SkillId, SkillCastingRequirement>();
		private Skill channelingSkill;
		private bool isUnderChanneling;
		private Dictionary<Skill, SkillId> ongoingSkillIdsByOnGoingSkills = new Dictionary<Skill, SkillId>();
		private Vector3 spawnPosition = new Vector3(-1, -1, -1);
		private Action<Modifier> OnAttachModifier = delegate (Modifier modifier) { };
		private Action<Modifier> OnDetachModifier = delegate (Modifier modifier) { };
		private List<Modifier> jumpPreventionModifiers = new List<Modifier>();
		public event EventHandler<SkillCastEventArgs> PostSkillCastEventHandler = delegate(object sender, SkillCastEventArgs args) {  };
		public event EventHandler<SkillConsumeResourceArgs> PostSkillConsumeResourceEventHandler;
		public event EventHandler<SkillConsumeResourceArgs> PostAllResourcesConsumptionEventHandler;
		private List<Loopable> loopables = new List<Loopable>();
		private List<SkillId> queuedInterruptSkills = new List<SkillId>(); 

		protected Character(SkillFactory skillFactory) {
			this.skillFactory = skillFactory;
		}

		public void OnMyDeath(Character deadCharacter) {
			for (int kIndex = 0; kIndex < ongoingSkills.Count; kIndex++) {
				ongoingSkills[kIndex].OnCasterDeath(deadCharacter);
			}
			for (int i = 0; i < ongoingModifiers.Count; i++) {
				ongoingModifiers[i].OnCharacterDeath(deadCharacter);
			}
		}

		public void OnKillEnemy(Character deadCharacter, List<DamageFromAttack.SourceFromSkill> sourceFromSkills) {
			int count = ongoingSkills.Count;
			for (int kIndex = 0; kIndex < count; kIndex++) {
				ongoingSkills[kIndex].OnKillEnemy(deadCharacter, sourceFromSkills);
			}
		}

		public bool OnReceiveFatalDamage(Character dealer, int damage,
		                                 int healthBeforeFatalDamage,
		                                 int healthAfterFatalDamage,
		                                 out int outDamage, out int outHealthAfterFatalDamage) {
			int count = ongoingSkills.Count;
			outDamage = damage;
			outHealthAfterFatalDamage = healthAfterFatalDamage;
			for (int kIndex = 0; kIndex < count; kIndex++) {
				int d;
				int h;
				bool isProcessed = ongoingSkills[kIndex].OnReceiveFatalDamage(
					dealer, damage, healthBeforeFatalDamage, healthAfterFatalDamage,
					out d, out h
				);
				if (isProcessed) {
					outDamage = d;
					outHealthAfterFatalDamage = h;
					//DLog.Log("Receive fatal damage event is processed, outHealthAfterFatalDamage: " + outHealthAfterFatalDamage);
					return true;
				}
			}

			return false;
		}

		private void NotifyModifierAttachment(Modifier m) {
			try {
				if (OnAttachModifier != null) {
					OnAttachModifier(m);
				}
			}
			catch (Exception e) {
				DLog.LogException(e);
			}
		}

		public void ListenOnAttachModifier(Action<Modifier> action) {
			this.OnAttachModifier += action;
		}

		public void UnlistenOnAttachModifier(Action<Modifier> action) {
			this.OnAttachModifier -= action;
		}

		private void NotifyModifierDetachment(Modifier m) {
			try {
				if (OnDetachModifier != null) {
					OnDetachModifier(m);
				}
			}
			catch (Exception e) {
				DLog.LogException(e);
			}
		}

		public void ListenOnDetachModifier(Action<Modifier> action) {
			this.OnDetachModifier += action;
		}

		public void UnlistenOnDetachModifier(Action<Modifier> action) {
			this.OnDetachModifier -= action;
		}

		public void Update(float dt) {
			if (spawnPosition.Equals(new Vector3(-1, -1, -1))) {
				spawnPosition = Position();
			}
			foreach (SkillCastingRequirement skillCastingRequirement in skillCastingRequirements.Values) {
				skillCastingRequirement.Update(dt);
			}
//			DLog.LogError(this.GetType()+" "+BattleUtils.frame+" "+BattleUtils.time);
			CleanupSkillsThatAreFinished();
			for (int i = ongoingSkills.Count - 1; i >= 0; i--) {
				Skill skill = ongoingSkills[i];
				skill.Update(dt);
			}

			foreach (SkillId skillId in queuedInterruptSkills)
			{
				InterruptSkill(skillId);
			}
			queuedInterruptSkills.Clear();
			
			if (isUnderChanneling && channelingSkill.IsChannelingFinish()) {
				isUnderChanneling = false;
			}
			
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				modifier.Update(dt);
				if (modifier.IsFinish()) {
					ongoingModifiers.RemoveAt(i);
					modifier.OnDetach(this);
					NotifyModifierDetachment(modifier);
				}
			}
			List<Modifier> ongoingModifiersCauseStateChange = new List<Modifier>();
			foreach (Modifier modifier in ongoingModifiers) {
				if (modifier.IsBuff()) continue;
				if (IsDebuffWithoutStateChangeContains(modifier.Type())) continue;

				ongoingModifiersCauseStateChange.Add(modifier);
			}
			if (ongoingModifiersCauseStateChange.Count < 1 && State() != CharacterState.Default) {
				ChangeToState(CharacterState.Default);
			}

			for (int i = loopables.Count - 1; i >= 0; i--) {
				Loopable loopable = loopables[i];
				loopable.Update(dt);
				if (loopable.IsFinished()) {
					loopables.RemoveAt(i);
				}
			}
		}

		private void CleanupSkillsThatAreFinished() {
			for (int i = ongoingSkills.Count - 1; i >= 0; i--) {
				Skill skill = ongoingSkills[i];
				if (skill.IsFinish()) {
					ongoingSkills.RemoveAt(i);
					skill.OnPreFinish(this);
					skill.OnFinish(this);
					ongoingSkillIdsByOnGoingSkills.Remove(skill);
				}
			}
		}

		public void LateUpdate(float dt) {
			for (int kIndex = ongoingSkills.Count - 1; kIndex >= 0; kIndex--) {
				Skill skill = ongoingSkills[kIndex];
				skill.LateUpdate(dt);
			}
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				modifier.LateUpdate(dt);
			}
		}

		public void PauseIndependentUpdate(float dt) {
			for (int kIndex = ongoingSkills.Count - 1; kIndex >= 0; kIndex--) {
				Skill skill = ongoingSkills[kIndex];
				skill.PauseIndependentUpdate(dt);
			}
		}

		public void AddSkillCastingRequirement(SkillId skillId, SkillCastingRequirement requirement) {
			skillCastingRequirements[skillId] = requirement;
		}

		public bool AddModifier(Modifier modifier) {
			bool addSuccess = false;
			if (!modifier.IsBuff()) {
				if (IsDebuffWithoutStateChangeContains(modifier.Type())) {
					addSuccess = true;
					AddBuff(modifier);
				}
				else {
					int modifierRank = modifier.Type().Rank();
					int characterStateRank = State().Rank();
					if (modifierRank >= characterStateRank) {
						addSuccess = true;
						List<Modifier> debuffs = new List<Modifier>();
						foreach (Modifier ongoingModifier in ongoingModifiers) {
							if (IsDebuffWithoutStateChangeContains(ongoingModifier.Type())) continue;
							if (!ongoingModifier.IsBuff()) {
								debuffs.Add(ongoingModifier);
								ongoingModifier.OnBeReplaced(this, modifier);
								//DLog.Log(string.Format("modifier {0} is replaced by {1}", ongoingModifier.Type(), modifier.Type()));
							}
						}
						foreach (Modifier debuff in debuffs) {
							ongoingModifiers.Remove(debuff);
						}

						modifier.OnReplaceOtherModifiers(this, debuffs);
						ongoingModifiers.Add(modifier);
						modifier.OnAttachAsMain(this);
						NotifyModifierAttachment(modifier);
						if (!MODIFIER_WITHOUT_STATE_CHANGE.Contains(modifier.Type())) {
							ChangeToState(CharacterState._.From(modifierRank));
						}
					}
					else {
						for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
							Modifier m = ongoingModifiers[i];
							if (m.Type() != modifier.Type()) continue;

							m.OnBeReplaced(this, modifier);
							ongoingModifiers.Remove(m);
						}

						bool attachSuccessAsSub = modifier.OnAttachAsSub(this);
						if (attachSuccessAsSub) {
							ongoingModifiers.Add(modifier);
							ChangeStateBaseOnModifiers();
						}
					}
				}
			}
			else {
				addSuccess = true;
				AddBuff(modifier);
			}

			return addSuccess;
		}

		public void RemoveModifierAndChangeState(Modifier m) {
			RemoveModifier(m);
			ChangeStateBaseOnModifiers();
		}

		public void ChangeStateBaseOnModifiers() {
			List<Modifier> ongoingModifiersCauseStateChange = new List<Modifier>();
			foreach (Modifier modifier in ongoingModifiers) {
				if (modifier.ShowAttachType() == ModifierAttachType.Sub) continue;
				if (IsDebuffWithoutStateChangeContains(modifier.Type())) continue;

				ongoingModifiersCauseStateChange.Add(modifier);
			}

			if (ongoingModifiersCauseStateChange.Count > 0) {
				ModifierType highestType = ongoingModifiersCauseStateChange[0].Type();

				for (int i = 0; i < ongoingModifiersCauseStateChange.Count; i++) {
					ModifierType mt = ongoingModifiersCauseStateChange[i].Type();
					if (mt > highestType) highestType = mt;
				}

				ChangeToState(CharacterState._.From(highestType.Rank()));
			}

			if (ongoingModifiersCauseStateChange.Count < 1 && State() != CharacterState.Default) {
				ChangeToState(CharacterState.Default);
			}
		}

		public void RemoveModifier(Modifier m) {
			bool found = false;
			for (int i = 0; i < ongoingModifiers.Count; i++) {
				if (ongoingModifiers[i] == m) {
					found = true;
					break;
				}
			}

			if (found) {
				ongoingModifiers.Remove(m);
				m.OnDetach(this);
				NotifyModifierDetachment(m);
			}
		}

		public bool SkillId(Skill s, ref SkillId skillId) {
			foreach (KeyValuePair<Skill, SkillId> p in ongoingSkillIdsByOnGoingSkills) {
				if (p.Key == s) {
					skillId = p.Value;
					return true;
				}
			}
			return false;
		}

		public void StackBuff(Modifier modifier) {
			if (modifier == null) return;

			ongoingModifiers.Add(modifier);
			modifier.OnAttachAsMain(this);
			NotifyModifierAttachment(modifier);
		}

		private void AddBuff(Modifier modifier) {
			List<Modifier> ongoingModifiersOfSameType = new List<Modifier>();
			foreach (Modifier ongoingModifier in ongoingModifiers) {
				if (ongoingModifier.Type() == modifier.Type()) {
					if (ongoingModifier.SubType() == modifier.SubType()) {
						StackResult stackResult = ongoingModifier.TryStackWithNewOne(modifier);
						if (stackResult == StackResult.Manual) return;
						if (stackResult == StackResult.Stack) {
							StackBuff(modifier);
							return;
						}

						ongoingModifiersOfSameType.Add(ongoingModifier);
					}
				}
			}
			foreach (Modifier m in ongoingModifiersOfSameType) {
				m.OnBeReplaced(this, modifier);
				ongoingModifiers.Remove(m);
			}

			modifier.OnReplaceOtherModifiers(this, ongoingModifiersOfSameType);
			ongoingModifiers.Add(modifier);
			modifier.OnAttachAsMain(this);
			NotifyModifierAttachment(modifier);
		}

		private void ReplaceOngoingByNewModifier(Modifier newModifier) {

		}

		public Modifier FindOngoingModifierOfType(ModifierType newModifierType) {
			foreach (Modifier ongoingModifier in ongoingModifiers) {
				if (ongoingModifier.Type() == newModifierType) {
					return ongoingModifier;
				}
			}
			throw new Exception(string.Format("Cannot find modifier of type '{0}' to remove", newModifierType));
		}

		public List<Modifier> FindOngoingModifiersOfType(ModifierType newModifierType) {
			List<Modifier> list = new List<Modifier>();
			foreach (Modifier ongoingModifier in ongoingModifiers) {
				if (ongoingModifier.Type() == newModifierType) {
					list.Add(ongoingModifier);
				}
			}
			return list;
		}

		public Skill CastSkill(SkillId skillId, SkillCastingSource skillCastingSource) {
#if GENERAL_LOG
			DLog.Log("Cast skill " + skillId);
#endif
			CheckSkillCastingRequirementExisted(skillId);
			CheckSkillIsCastable(skillId);

			Skill skill = skillFactory.Create(this, skillId, skillCastingSource);
			ongoingSkills.Add(skill);
			ongoingSkillIdsByOnGoingSkills[skill] = skillId;
			skill.OnCast(this);
			channelingSkill = skill;
			if (!channelingSkill.IsChannelingFinish()) {
				isUnderChanneling = true;
			}
			Notify(skill, skillId);
			return skill;
		}

		public void OnAllCastingResourcesConsumed(Skill skill) {
			if (PostAllResourcesConsumptionEventHandler != null) {
				SkillId skillId = null;
				SkillId(skill, ref skillId);
				PostAllResourcesConsumptionEventHandler(
					this,
					new SkillConsumeResourceArgs() {
						SkillId = skillId
					}
				);
			}
		}

		public void ConsumeSkillCastingResources(SkillId skillId, params Resource.Name[] names) {
			skillCastingRequirements[skillId].Consume(names);
			NotifyConsume(skillId, names);
		}

		public void ConsumeSkillCastingResourcesUp(SkillId skillId, params Resource.Name[] names) {
			SkillCastingRequirement requirement = skillCastingRequirements[skillId];
			for (int i = 0; i < 20; i++) {
				if(!requirement.IsCastable()) break;

				requirement.Consume(names);
			}
		}

		public bool IsChannelingSkillMoveable() {
			if (isUnderChanneling) {
				return channelingSkill.IsMoveable();
			}
			return false;
		}

		public bool IsChannelingSkillMoveBackward() {
			if (isUnderChanneling) {
				return channelingSkill.IsMoveBackward();
			}
			return false;
		}

		public bool IsChannelingSkillJumpable() {
			if (isUnderChanneling) {
				return channelingSkill.IsJumpable();
			}

			return false;
		}

		public bool IsChannelingSkillChainableToCombo() {
			if (isUnderChanneling) {
				return channelingSkill.IsChainableToCombo();
			}

			return false;
		}

		public bool IsChannelingSkillChainableToAirCombo() {
			if (isUnderChanneling) {
				return channelingSkill.IsChainableToAirCombo();
			}

			return false;
		}

		public string PrepareChannelingSkillForChainingToCombo() {
			if (isUnderChanneling) {
				return channelingSkill.PrepareForChainingToCombo();
			}

			return string.Empty;
		}

		public bool IsJumpable() {
			if (isUnderChanneling) {
				Debug.Log("Character is under channeling of skill id: " + ongoingSkillIdsByOnGoingSkills[channelingSkill]);
				Debug.Log("Character is under channeling, is jumpable " + channelingSkill.IsJumpable());
				return channelingSkill.IsJumpable();
			}

			Debug.Log("Character jumpPreventionModifiers.Count < 1: " + (jumpPreventionModifiers.Count < 1));
			return jumpPreventionModifiers.Count < 1;
		}

		public void AddJumpPreventionModifier(Modifier m) {
			new NotNullReference().Check(m, "modifier");

			jumpPreventionModifiers.Add(m);
		}

		public void RemoveJumpPreventionModifier(Modifier m) {
			if (m == null) return;

			jumpPreventionModifiers.Remove(m);
		}

		public bool IsChannelingSkillInterruptible() {
			if (isUnderChanneling) {
				return channelingSkill.IsInterruptibleWhileChanneling();
			}

			return false;
		}

		public void NotifyJumpBegin() {
			if (isUnderChanneling) {
				channelingSkill.OnJumpBegin();
			}
		}

		public void NotifyJumpEnd() {
			if (isUnderChanneling) {
				channelingSkill.OnJumpEnd();
			}
		}

		public void InterruptChannelingSkill() {
			if (isUnderChanneling) {
				SkillId skillId = ongoingSkillIdsByOnGoingSkills[channelingSkill];
				channelingSkill.Interrupt();
				isUnderChanneling = false;

				for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
					Modifier modifier = ongoingModifiers[i];
					modifier.CheckLifetimes();
					if (modifier.IsFinish()) {
						ongoingModifiers.RemoveAt(i);
						modifier.OnDetach(this);
						NotifyModifierDetachment(modifier);
					}
				}
			}
		}

		public bool ShowChannelingSkillId(ref SkillId skillId) {
			if (isUnderChanneling) {
				skillId = ongoingSkillIdsByOnGoingSkills[channelingSkill];
				return true;
			}
			return false;
		}

		public void InterruptSkill(SkillId skillId) {
			if (!ongoingSkillIdsByOnGoingSkills.Values.Contains(skillId)) {
				throw new Exception("Skill id " + skillId + " is not ongoing");
			}
			Skill s = null;
			foreach (KeyValuePair<Skill, SkillId> pair in ongoingSkillIdsByOnGoingSkills) {
				if (pair.Value.Equals(skillId)) {
					s = pair.Key;
					break;
				}
			}

			InterruptSkill(s);
		}

		public void InterruptSkill(Skill s) {
			if (isUnderChanneling && s == channelingSkill) {
				//DLog.Log("InterruptSkill() that is under channeling " + skillId);
				isUnderChanneling = false;
			}
			ongoingSkills.Remove(s);
			s.Interrupt();
			s.OnPreFinish(this);
			s.OnFinish(this);
			ongoingSkillIdsByOnGoingSkills.Remove(s);
		}

		public void QueueInterruptSkill(SkillId skillId)
		{
			queuedInterruptSkills.Add(skillId);
		}

		public abstract bool IsCanPlayAnimationRunInTheAir();
		
		public abstract bool IsOnGround();

		public bool IsChanneling() {
			return isUnderChanneling;
		}

		public bool IsSkillCastable(SkillId skillId) {
			CheckSkillCastingRequirementExisted(skillId);
			return skillCastingRequirements[skillId].IsCastable();
		}

		public void InterruptOngoingSkills() {
			isUnderChanneling = false;
			foreach (Skill ongoingSkill in ongoingSkills) {
				ongoingSkill.Interrupt();
			}
			CleanupSkillsThatAreFinished();
		}

		public void InterruptInterruptibleOngoingSkills() {
			isUnderChanneling = false;
			foreach (Skill ongoingSkill in ongoingSkills) {
				if (!ongoingSkill.IsInterruptible()) continue;
				ongoingSkill.Interrupt();
			}
			CleanupSkillsThatAreFinished();
		}

		public void InterruptOngoingModifiers() {
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				ongoingModifiers.RemoveAt(i);
				modifier.OnDetach(this);
				NotifyModifierDetachment(modifier);
			}
		}

		public void InterruptOngoingDebuffModifiers() {
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				if(modifier.IsBuff()) continue;
				
				ongoingModifiers.RemoveAt(i);
				modifier.OnDetach(this);
				NotifyModifierDetachment(modifier);
			}
		}

		public void InterruptOngoingModifiersOfType(ModifierType type) {
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				if(modifier.Type() != type)continue;
				
				ongoingModifiers.RemoveAt(i);
				modifier.OnDetach(this);
				NotifyModifierDetachment(modifier);
			}
		}

		public void InterruptModifier(Modifier m) {
			for (int i = ongoingModifiers.Count - 1; i >= 0; i--) {
				Modifier modifier = ongoingModifiers[i];
				if (modifier != m) continue;

				ongoingModifiers.RemoveAt(i);
				modifier.OnDetach(this);
				NotifyModifierDetachment(modifier);
			}
		}

		public List<SkillId> OngoingSkills() {
			return ongoingSkillIdsByOnGoingSkills.Values.ToList();
		}

		public Skill FindOngoingSkill(SkillId skillId) {
			foreach (KeyValuePair<Skill,SkillId> pair in ongoingSkillIdsByOnGoingSkills) {
				if (pair.Value.Equals(skillId)) {
					return pair.Key;
				}
			}

			return null;
		}

		public List<Skill> FindOngoingSkills(SkillId skillId) {
			List<Skill> l = new List<Skill>();
			foreach (KeyValuePair<Skill,SkillId> pair in ongoingSkillIdsByOnGoingSkills) {
				if (pair.Value.Equals(skillId)) {
					l.Add(pair.Key);
				}
			}

			return l;
		}

		public Modifier FindModifierOfHighestRank() {
			if (ongoingModifiers.Count < 1) throw new Exception("There is no ongoing modifier");

			Modifier highest = ongoingModifiers[0];
			foreach (Modifier ongoingModifier in ongoingModifiers) {
				if (ongoingModifier.Type().Rank() > highest.Type().Rank()) {
					highest = ongoingModifier;
				}
			}
			return highest;
		}

		public List<Modifier> GetListModifiers() {
			return ongoingModifiers;
		}

		public Vector3 SpawnPosition() {
			return spawnPosition;
		}

		public Dictionary<SkillId, SkillCastingRequirement> GetSkillCastingRequirements() {
			return skillCastingRequirements;
		}

		public SkillCastingRequirement GetSkillCastingRequirements(SkillId skillId)
		{
			if (GetSkillCastingRequirements().ContainsKey(skillId))
			{
				return GetSkillCastingRequirements()[skillId];
			}
			return null;
		}

		public void PauseForLockFrame() {
			foreach (Skill skill in ongoingSkills) {
				skill.PauseForLockFrame();
			}
		}

		public void UnpauseForLockFrame() {
			foreach (Skill skill in ongoingSkills) {
				skill.UnpauseForLockFrame();
			}
		}

		protected abstract void LeaveSkillState();
		public abstract CharacterState State();
		protected abstract void ChangeToState(CharacterState newState);
		public abstract Vector3 Position();
		public abstract Vector3 TorsoPosition();
		public abstract Direction FacingDirection();
		public abstract void PlayAnimation(string name, float speed = 1,
		                                   bool stopIfBeingPlayed = false, bool stopCurrent = false);
		public abstract void StopAnimation();
		public abstract void QueueAnimation(string name);
		public abstract void FreezeAnimation(int frame);
		public abstract string Group();
		public abstract int Id();
		public abstract CharacterId CharacterId();
		public abstract Request Dash(float distance, float duration, float blendTime, bool isInvokedFromEventFrame, bool isFromUserInput, bool ignoreMinSpeedOnAir = false, bool constantSpeed = false);
		public abstract void StopDash();
		public abstract Request Jump(float height, float durationReachMaxHeight, float distance,
		                             float durationLandGround, bool isFromSignatureSkill, float floatingDuration = 0, bool preciseHeight = false);
		public abstract Request JumpOverDistance(float height, float durationReachMaxHeight, float distance,
		                                      float durationLandGround, bool isFromSignatureSkill,
		                                      float floatingDuration = 0,
		                                      bool stopHorizontalMovementWhenMeet = false,
		                                      bool preciseHeight = false,
		                                      bool followFacingDirection = false,
		                                      bool moveHorizontallyWhenFloat = false);
		public abstract void InterruptJump();
		public abstract void SetMovingDirectionToLeft();
		public abstract void SetMovingDirectionToRight();
		public abstract void SetMovingDirection(Vector2 direction);
		public abstract void DisplaceBy(Vector3 displacement);
		public abstract void TurnCollider(bool on);
		public abstract void AddAnimationSpeed(string animationName, float bonus);
		public abstract void AdjustCurrentAnimationSpeed(float speed);
		public abstract float AnimationDuration(string animationName);
		public abstract void SetAnimationSpeed(float speed);
		public abstract float GetAnimationSpeed();
		public abstract void SetFacingDirectionToLeft();
		public abstract void SetFacingDirectionToRight();
		public abstract void PauseAnimation();
		public abstract void SetPosition(Vector3 pos);
		public abstract List<string> CurrentAnimationNames();
		public abstract GameObject GameObject();
		public abstract void TurnVisibility(bool on);
		public abstract void SkipFramesOfCurrentPlayingAnimation(int frames);
		public abstract void JumpToFrame(int frame);
		public abstract void ReceiveDamage(DamageFromAttack damage);
		public abstract Request Blast(float height, float timeToPeak, float timeToGround,
		                           float flightDistance, float flightMinSpeed, float rollDistance,
		                           float timeToRoll);
		public abstract void ConsumeJumpCharge(int value = 1);
		public abstract void ConsumeJumpAttackCharge(int value = 1);

		public void OnGroundEvent() {
			for (int kIndex = 0; kIndex < ongoingSkills.Count; kIndex++) {
				ongoingSkills[kIndex].OnGroundEvent();
			}
		}

		public void AddLoopable(Loopable l) {
			if (l != null) {
				loopables.Add(l);
			}
		}

		private void CheckSkillIsCastable(SkillId skillId) {
			SkillCastingRequirement req = skillCastingRequirements[skillId];
			if (!req.IsCastable()) {
				throw new SkillCastingRequirementException(string.Format(
					"Skill of id '{0}' is not castable, reason: {1}", skillId, req.Reasons()
				));
			}
		}

		private void CheckSkillCastingRequirementExisted(SkillId skillId) {
			if (!skillCastingRequirements.ContainsKey(skillId)) {
				throw new Exception(string.Format(
					"No skill casting requirement found for skill of id '{0}'", skillId
				));
			}
		}

		private void Notify(Skill skill, SkillId skillId) {
			try {
				if (PostSkillCastEventHandler != null) {
					PostSkillCastEventHandler(this, new SkillCastEventArgs() {
						skillId = skillId,
						skill = skill
					});
				}
			}
			catch (Exception e) {
				DLog.LogException(e);
			}
		}

		private void NotifyConsume(SkillId skillId, params Resource.Name[] names)
		{
			try
			{
				PostSkillConsumeResourceEventHandler?.Invoke(this, new SkillConsumeResourceArgs()
				{
					SkillId = skillId,
					Name = names
				});
			}
			catch (Exception e)
			{
				DLog.LogException(e);
			}
		}

		public enum CharacterState {
			_ = int.MinValue,
			Default = 0,
			Stun = ModifierType.Stun,
			Stagger = ModifierType.Stagger,
			Trip = ModifierType.Trip,
			Launcher = ModifierType.Launcher,
			Blast = ModifierType.Blast,
			Knockdown = ModifierType.Knockdown,
			Shackle = ModifierType.Shackle,
			Ragdoll = ModifierType.Ragdoll,
			Sleep = ModifierType.Sleep,
			Freeze = ModifierType.Freeze,
			Vanish = ModifierType.Vanish,
			Immune = ModifierType.Immune,
			Death = int.MaxValue
		}

		public void OnDamageDealt(Character caster, Character target,
		                          Skill fromSkill, Modifier fromModifier, int damage, bool critical) {
			for (int i = 0; i < ongoingSkills.Count; i++) {
				ongoingSkills[i].OnDamageDealt(caster, target, fromSkill, fromModifier, damage, critical);
			}

			for (int i = 0; i < ongoingModifiers.Count; i++) {
				ongoingModifiers[i].OnDamageDealt(caster, target, fromSkill, fromModifier, damage);
			}
		}

		public void OnComboCount(int combo) {
			int count = ongoingSkills.Count;
			for (int i = 0; i < count; i++) {
				ongoingSkills[i].OnComboCount(combo);
			}
		}

		public void OnPreDamageCalculation(Character caster, Character target,
		                                   DamageFromAttack.SourceHistory dmgSourceHistory) {
			foreach (Skill ongoingSkill in ongoingSkills) {
				ongoingSkill.OnPreDamageCalculation(caster, target, dmgSourceHistory);
			}
		}

		private bool IsDebuffWithoutStateChangeContains(ModifierType modifierType) {
			for (int i = 0; i < DEBUFF_WITHOUT_STATE_CHANGE.Length; i++) {
				ModifierType mt = DEBUFF_WITHOUT_STATE_CHANGE[i];
				if (mt == modifierType) return true;
			}

			return false;
		}

		public class SkillCastEventArgs : EventArgs {
			public SkillId skillId;
			public Skill skill;
		}
		
		public class SkillConsumeResourceArgs : EventArgs
		{
			public SkillId SkillId;
			public Resource.Name[] Name;
		}
	}

	public class SkillCastingSource {
		private Source src;
		private object[] param;

		public SkillCastingSource(Source src, object[] param) {
			this.src = src;
			this.param = param;
		}

		public SkillCastingSource(Source src) {
			this.src = src;
		}

		public static SkillCastingSource FromUserInput() {
			return new SkillCastingSource(Source.UserInput);
		}
		public static SkillCastingSource FromAI() {
			return new SkillCastingSource(Source.AI);
		}

		public static SkillCastingSource FromKilledByEnemy(Direction enemyFacingDirection,
		                                                   List<Vector2> projectilesPosition,
		                                                   List<Vector2> impactPositions) {
			return new SkillCastingSource(
				Source.KilledByEnemy, new object[]{enemyFacingDirection, projectilesPosition, impactPositions}
			);
		}

		public static SkillCastingSource FromEnemyDeath() {
			return new SkillCastingSource(Source.EnemyDeath);
		}

		public static SkillCastingSource FromEntityCreation() {
			return new SkillCastingSource(Source.EntityCreation);
		}

		public static SkillCastingSource FromDiePrepare() {
			return new SkillCastingSource(Source.DiePrepare);
		}

		public static SkillCastingSource FromFinish() {
			return new SkillCastingSource(Source.Finish);
		}

		public static SkillCastingSource FromResurrection() {
			return new SkillCastingSource(Source.Resurrection);
		}

		public static SkillCastingSource FromSuicide() {
			return new SkillCastingSource(Source.Suicide);
		}

		public static SkillCastingSource FromSystem()
		{
			return new SkillCastingSource(Source.System);
		}

		public Source Src {
			get { return src; }
		}

		public object[] Param {
			get { return param; }
		}

		public enum Source {
			UserInput,
			KilledByEnemy,
			EnemyDeath,
			AI,
			EntityCreation,
			DiePrepare,
			Finish,
			Resurrection,
			Suicide,
			System,
		}
	}

	static class CharacterStateMethods {
		private static Character.CharacterState[] INPUT_BLOCKED = new[] {
			Character.CharacterState.Stun,
			Character.CharacterState.Stagger,
			Character.CharacterState.Trip,
			Character.CharacterState.Launcher,
			Character.CharacterState.Blast,
			Character.CharacterState.Knockdown,
			Character.CharacterState.Shackle,
			Character.CharacterState.Ragdoll,
			Character.CharacterState.Sleep,
			Character.CharacterState.Freeze,
			Character.CharacterState.Vanish,
			Character.CharacterState.Death
		};

		private static Character.CharacterState[] INPUT_ALLOWED = new[] {
			Character.CharacterState.Default,
			Character.CharacterState.Immune,
		};

		static CharacterStateMethods() {
			foreach (Character.CharacterState state in Enum.GetValues(typeof(Character.CharacterState))) {
				if (state == Character.CharacterState._) continue;

				if (!INPUT_ALLOWED.Contains(state) && !INPUT_BLOCKED.Contains(state)) {
					throw new Exception("State '" + state + "' must be classified as INPUT_BLOCKED or INPUT_ALLOWED");
				}
			}
		}

		public static int Rank(this Character.CharacterState characterState) {
			return (int) characterState;
		}

		public static Character.CharacterState From(this Character.CharacterState ctor, int rank) {
			return (Character.CharacterState) rank;
		}

		public static bool IsInputBlocked(this Character.CharacterState characterState) {
			for (int i = 0; i < INPUT_BLOCKED.Length; ++i)
				if (INPUT_BLOCKED[i] == characterState) {
					return true;
				}
			return false;
		}
	}
}
