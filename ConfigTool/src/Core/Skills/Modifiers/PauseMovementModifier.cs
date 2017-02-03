using System.Collections.Generic;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using MovementSystem.Components;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class PauseMovementModifier : BaseModifier {
		private readonly PauseMovementInfo info;
		private readonly Entity casterEntity;
		private readonly Entity targetEntity;

		private float elapsed;
		private MovementComponent targetMovementComponent;

		public PauseMovementModifier(PauseMovementInfo info, Entity casterEntity,
		                             Entity targetEntity, Environment environment,
		                             CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = info;
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;

			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.PauseMovement;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetMovementComponent.Unpause();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetMovementComponent.Unpause();
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		public override void OnCharacterDeath(Character deadCharacter) {
			targetMovementComponent.Unpause();
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			targetMovementComponent.Pause();
		}
	}
}