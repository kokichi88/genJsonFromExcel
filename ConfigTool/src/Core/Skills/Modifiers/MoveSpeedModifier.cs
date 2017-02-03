using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using MovementSystem.Components;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class MoveSpeedModifier : BaseModifier {
		private MoveSpeedInfo info;
		private readonly Entity targetEntity;

		private MovementComponent targetMovementComponent;
		private ValueModifier valueModifier;
		private float powerScale = 1;

		public MoveSpeedModifier(ModifierInfo info, Entity casterEntity,
		                         Entity targetEntity, Environment environment,
		                         CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (MoveSpeedInfo) info;
			this.targetEntity = targetEntity;

			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.MovementSpeed;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return info.Msmc.percentage >= 0;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			RemoveValueModifier();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			RemoveValueModifier();
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			RemoveValueModifier();
			ModifyMovementSpeed();
		}

		public void SetPowerScale(float value) {
			powerScale = value;
			RemoveValueModifier();
			ModifyMovementSpeed();
		}

		private void ModifyMovementSpeed() {
			float finalPercentage = info.Msmc.percentage * powerScale;
			valueModifier = targetMovementComponent.ConfigData.speedStats.AddModifier(
				StatsModifierOperator.Percentage, finalPercentage
			);
		}

		private void RemoveValueModifier() {
			if (valueModifier != null) {
				targetMovementComponent.ConfigData.speedStats.RemoveModifier(valueModifier);
			}
		}
	}
}