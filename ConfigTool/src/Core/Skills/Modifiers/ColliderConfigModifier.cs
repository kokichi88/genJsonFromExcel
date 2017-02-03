using System.Collections.Generic;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using MovementSystem;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class ColliderConfigModifier : BaseModifier {
		private ColliderConfigInfo info;

		private ColliderConfigData colliderConfigData;
		private Vector2 originalHeadOffset;
		private Vector2 originalBodyOffset;
		private Vector2 originalFeetOffset;

		public ColliderConfigModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                              Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ColliderConfigInfo) info;
		}

		public override ModifierType Type() {
			return ModifierType.ColliderConfig;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			colliderConfigData = target.GameObject().GetComponent<ColliderConfigData>();

			originalHeadOffset = colliderConfigData.GetHeadSphere().offset;
			originalBodyOffset = colliderConfigData.GetBodySphere().offset;
			originalFeetOffset = colliderConfigData.GetFeetSphere().offset;

			if (info.ColliderConfigModifierConfig.modifier) {
				colliderConfigData.GetHeadSphere().offset += info.ColliderConfigModifierConfig.headOffset;
				colliderConfigData.GetBodySphere().offset += info.ColliderConfigModifierConfig.bodyOffset;
				colliderConfigData.GetFeetSphere().offset += info.ColliderConfigModifierConfig.feetOffset;
			}
			else {
				colliderConfigData.GetHeadSphere().offset = info.ColliderConfigModifierConfig.headOffset;
				colliderConfigData.GetBodySphere().offset = info.ColliderConfigModifierConfig.bodyOffset;
				colliderConfigData.GetFeetSphere().offset = info.ColliderConfigModifierConfig.feetOffset;
			}
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			ReturnToOriginalValues();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			ReturnToOriginalValues();
		}

		private void ReturnToOriginalValues() {
			colliderConfigData.GetHeadSphere().offset = originalHeadOffset;
			colliderConfigData.GetBodySphere().offset = originalBodyOffset;
			colliderConfigData.GetFeetSphere().offset = originalFeetOffset;
		}
	}
}