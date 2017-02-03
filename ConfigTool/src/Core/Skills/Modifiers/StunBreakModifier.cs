using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.MonsterStateMachines;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class StunBreakModifier : BaseModifier {
		private readonly Entity casterEntity;
		private readonly Entity targetEntity;
		private readonly Environment environment;
		private readonly CollectionOfInteractions modifierInteractionCollection;
		private StunBreakInfo info;

		private Stats extraStunDuration;
		private List<ValueModifier> valueModifiers = new List<ValueModifier>();
		private UserInput userInput;
		private VibrateModifier vibrateModifier;
		private Character targetCharacter;

		public StunBreakModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                         Environment environment,
		                         CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;
			this.environment = environment;
			this.modifierInteractionCollection = modifierInteractionCollection;
			this.info = (StunBreakInfo) info;
			bool found;
			extraStunDuration = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.ExtraStunDuration, out found);
			HeroStateMachineComponent smComponent = targetEntity.GetComponent<HeroStateMachineComponent>();
			if (smComponent == null) {
				smComponent = targetEntity.GetComponent<MonsterStateMachineComponent>();
			}
			userInput = smComponent.UserInput;
		}

		public override ModifierType Type() {
			return ModifierType.StunBreak;
		}

		protected override void OnUpdate(float dt) {
			if (userInput.IsJoyStickDirectionJustChanged()) {
				//DLog.Log("decrease stun duration by " + info.Config.reduction);
				valueModifiers.Add(extraStunDuration.AddModifier(StatsModifierOperator.Addition, -info.Config.reduction));
				VibrateInfo vi = new VibrateInfo(
					Target.Target, 1, 0, info.Config.vibrate.xAmplitude, info.Config.vibrate.frequency,
					info.Config.vibrate.shouldDecay,
					info.Config.vibrate.decayConstant, new List<VfxConfig>(), BaseModifierConfig.NO_ICON,
					info.Config.vibrate.lifetimes
				);
				vibrateModifier = new VibrateModifier(
					vi, casterEntity, targetEntity, environment, modifierInteractionCollection
				);
				targetCharacter.AddModifier(vibrateModifier);
			}
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			this.targetCharacter = target;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			foreach (ValueModifier valueModifier in valueModifiers) {
				extraStunDuration.RemoveModifier(valueModifier);
			}

			if (vibrateModifier != null) {
				vibrateModifier.OnBeReplaced(target, byModifier);
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			foreach (ValueModifier valueModifier in valueModifiers) {
				extraStunDuration.RemoveModifier(valueModifier);
			}

			if (vibrateModifier != null) {
				vibrateModifier.OnDetach(target);
			}
		}
	}
}