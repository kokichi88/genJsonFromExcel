using System.Collections.Generic;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Animation;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class PauseAnimationModifier : BaseModifier {
		private readonly PauseAnimationInfo info;
		private readonly Entity casterEntity;
		private readonly Entity targetEntity;

		private float elapsed;
		private AnimationComponent targetAnimationComponent;

		public PauseAnimationModifier(PauseAnimationInfo info, Entity casterEntity,
		                              Entity targetEntity, Environment environment,
		                              CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = info;
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;

			targetAnimationComponent = targetEntity.GetComponent<AnimationComponent>();
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.PauseAnimation;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			//DLog.Log("PauseAnimationModifierConfig:OnBeReplaced");
			base.OnBeReplaced(target, byModifier);
			targetAnimationComponent.Animation.UnpauseAnimation();
		}

		public override void OnDetach(Character target) {
			//DLog.Log("PauseAnimationModifier:Detach");
			base.OnDetach(target);
			targetAnimationComponent.Animation.UnpauseAnimation();
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		public override void OnCharacterDeath(Character deadCharacter) {
			//DLog.Log("PauseAnimationModifierConfig:OnCharacterDeath");
			targetAnimationComponent.Animation.UnpauseAnimation();
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			//DLog.Log("PauseAnimationModifierConfig:OnDelayedAttachAsMain");
			targetAnimationComponent.Animation.PauseAnimation();
		}
	}
}