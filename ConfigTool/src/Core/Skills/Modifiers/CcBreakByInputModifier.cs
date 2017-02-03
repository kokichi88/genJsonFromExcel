using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.MonsterStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class CcBreakByInputModifier : BaseModifier {
		public static HashSet<ModifierType> interested = new HashSet<ModifierType>(
			new[] {
				ModifierType.Stun, 
				ModifierType.Freeze, 
				ModifierType.Sleep, 
				ModifierType.Shackle, 
				ModifierType.Ragdoll,
				//ModifierType.Launcher,
				//ModifierType.Blast,
				//ModifierType.Knockdown
			}
		);

		private CcBreakByInputInfo info;
		private Modifier parentModifier;
		private readonly CcBreakByInputModifierEventConfig eventConfig;
		private readonly Environment environment;
		private readonly CollectionOfInteractions modifierInteractionCollection;

		private DefaultUserInput userInput;
		private int gauge;
		private UnpredictableDurationLifetime gaugeLifetime;
		private SkillId skillId;
		private Character character;
		private HeroStateMachineComponent smComponent;
		private bool triggered;
		private List<VibrateModifier> vibrateModifiers = new List<VibrateModifier>();

		public CcBreakByInputModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                              Modifier parentModifier, CcBreakByInputModifierEventConfig eventConfig,
		                              Environment environment,
		                              CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (CcBreakByInputInfo) info;
			this.parentModifier = parentModifier;
			this.eventConfig = eventConfig;
			this.environment = environment;
			this.modifierInteractionCollection = modifierInteractionCollection;

			mainLifetimes.Add(new ParentModifierLifetime(parentModifier));
			smComponent = targetEntity.GetComponent<HeroStateMachineComponent>();
			if (smComponent == null) {
				smComponent = targetEntity.GetComponent<MonsterStateMachineComponent>();
			}
			userInput = (DefaultUserInput) smComponent.UserInput;
			EquippedSkills equippedSkills = targetEntity.GetComponent<EquippedSkillsComponent>().EquippedSkills;
			skillId = equippedSkills.PassiveRecovery2.SkillId;

			character = targetEntity.GetComponent<SkillComponent>().Character;
		}

		public override ModifierType Type() {
			return ModifierType.CcBreakByInput;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			gaugeLifetime = new UnpredictableDurationLifetime();
			lifetimes.Add(gaugeLifetime);
			return lifetimes;
		}

		protected override void OnUpdate(float dt) {
			// DLog.Log("debug CcBreakByInputModifier " + GetHashCode() + " update()");
			if (userInput.IsInputCcBreak()) {
				gauge++;
				TriggerSuccessInputEvent();
				TriggerGaugeEvents();
				if (gauge >= info.Config.gaugeCapacity && !triggered) {
					triggered = true;
					gaugeLifetime.End();
					userInput.StartSkill(skillId);
					smComponent.HeroStateMachine.StateMachine.ReplaceCurrentStateBy(StateName.SKILL);
				}
			}
		}

		private void TriggerSuccessInputEvent() {
			TriggerVibrateActions(eventConfig.successInputEvent.vibrateActions);
		}

		private void TriggerGaugeEvents() {
			foreach (GaugeEvent gaugeEvent in eventConfig.gaugeEvents) {
				if (gaugeEvent.value != gauge) continue;

				TriggerVibrateActions(gaugeEvent.vibrateActions);
			}
		}

		private void TriggerVibrateActions(List<VibrateAction> vibrateActions) {
			foreach (VibrateAction vibrateAction in vibrateActions) {
				VibrateInfo vi = new VibrateInfo(
					Target.Target, 1, 0, vibrateAction.xAmplitude, vibrateAction.frequency,
					vibrateAction.shouldDecay, vibrateAction.decayConstant,
					new List<VfxConfig>(), BaseModifierConfig.NO_ICON,
					new List<LifetimeConfig>(
						new[] {
							new DurationInSecondsLifetimeConfig() {
								duration = vibrateAction.duration
							},
						}
					)
				);
				VibrateModifier vibrateModifier = new VibrateModifier(
					vi, casterEntity, targetEntity, environment, modifierInteractionCollection
				);
				character.AddModifier(vibrateModifier);
				vibrateModifiers.Add(vibrateModifier);
			}
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			foreach (VibrateModifier vibrateModifier in vibrateModifiers) {
				vibrateModifier.OnDetach(target);
			}
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			foreach (VibrateModifier vibrateModifier in vibrateModifiers) {
				vibrateModifier.OnBeReplaced(target, byModifier);
			}
		}

		private class ParentModifierLifetime : Lifetime {
			private Modifier parent;

			public ParentModifierLifetime(Modifier parent) {
				this.parent = parent;
			}

			public LifetimeType ShowType() {
				return LifetimeType.Unpredictable;
			}

			public void Update(float dt) {
			}

			public void Check() {
			}

			public bool IsEnd() {
				return parent.IsFinish();
			}

			public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			}
		}
	}
}