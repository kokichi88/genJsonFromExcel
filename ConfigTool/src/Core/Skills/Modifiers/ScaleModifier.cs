using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Mode = Combat.Skills.ModifierConfigs.Modifiers.ScaleModifierConfig.Mode;

namespace Core.Skills.Modifiers {
	public class ScaleModifier : BaseModifier {
		private ScaleInfo info;

		private Vector3 originalScaleValue;
		private Mode scaleMode;
		private GameObjectComponent gameObjectComponent;

		public ScaleModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                     Environment environment,
		                     CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ScaleInfo) info;

			gameObjectComponent = (GameObjectComponent) targetEntity.GetComponent<EntityGameObjectComponent>();
			originalScaleValue = gameObjectComponent.GetScale();
		}

		public override ModifierType Type() {
			return ModifierType.Scale;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			scaleMode = info.Config.ShowMode();
			switch (scaleMode) {
				case Mode.Assignment:
					gameObjectComponent.SetScale(info.Config.scaleValue);
					break;
				case Mode.Increment:
					gameObjectComponent.IncreaseScaleBy(info.Config.scaleValue);
					break;
				default:
					throw new Exception("Missing logic to handle scale mode of " + scaleMode);
			}
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			ReturnToOriginalValue();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			ReturnToOriginalValue();
		}

		private void ReturnToOriginalValue() {
			switch (scaleMode) {
				case Mode.Assignment:
					gameObjectComponent.SetScale(originalScaleValue);
					break;
				case Mode.Increment:
					gameObjectComponent.IncreaseScaleBy(-info.Config.scaleValue);
					break;
				default:
					throw new Exception("Missing logic to handle scale mode of " + scaleMode);
			}
		}
	}
}