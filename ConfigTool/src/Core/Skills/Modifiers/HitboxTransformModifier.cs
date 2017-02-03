using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils;
using Hitbox = Combat.Skills.ModifierConfigs.Modifiers.HitboxTransformModifierConfig.Hitbox;

namespace Core.Skills.Modifiers {
	public class HitboxTransformModifier : BaseModifier {
		private HitboxTransformInfo info;

		private float elapsed;
		private BoxCollider collider;
		private List<Hitbox> sortedHitboxes;
		private Vector3 originalCenter;
		private Vector3 originalSize;

		public HitboxTransformModifier(ModifierInfo info, Entity casterEntity,
		                               Entity targetEntity, Environment environment,
		                               CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (HitboxTransformInfo) info;
			GameObjectComponent goc = (GameObjectComponent) targetEntity.GetComponent<EntityGameObjectComponent>();
			collider = (BoxCollider) goc.Collider;
			sortedHitboxes = new List<Hitbox>(this.info.HtmcConfig.hitboxes);
			sortedHitboxes.Sort((a, b) => {
				float diff = a.frame - b.frame;
				if (diff > 0) {
					return 1;
				}

				if (diff < 0) {
					return -1;
				}

				return 0;
			});
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.HitboxTransform;
		}

		protected override void OnUpdate(float dt) {
			elapsed += dt;
			Hitbox h = FindHitbox(elapsed, sortedHitboxes);
			RectPivotPosition rectPivotPosition = new RectPivotPosition(
				h.ShowPivotType(), h.pivotRelativePosition, h.size
			);
			collider.center = rectPivotPosition.RelativePositionOfPivotAt(
				RectPivotPosition.PivotType.Center
			);
			collider.size = h.size;
			if (collider.size == Vector3.zero) {
				collider.enabled = false;
			}
			else {
				collider.enabled = true;
			}
		}

		public override bool IsBuff() {
			return true;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			SetOriginalValues();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			SetOriginalValues();
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			originalCenter = collider.center;
			originalSize = collider.size;
		}

		private Hitbox FindHitbox(float elapsed, List<Hitbox> hitboxes) {
			for (int kIndex = hitboxes.Count - 1; kIndex >= 0; kIndex--) {
				Hitbox h = hitboxes[kIndex];
				float time = FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(h.frame);
				if (elapsed >= time) {
					return h;
				}
			}

			throw new Exception("Cannot find hitbox");
		}

		private void SetOriginalValues() {
			collider.center = originalCenter;
			collider.size = originalSize;
			collider.enabled = true;
		}
	}
}