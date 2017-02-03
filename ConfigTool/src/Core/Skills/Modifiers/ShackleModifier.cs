using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils.Extensions;
using EntityComponentSystem;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class ShackleModifier : BaseModifier {
		private ShackleInfo info;

		private float elapsed;
		private Character casterCharacter;
		private Character targetCharacter;

		public ShackleModifier(ModifierInfo info, Entity casterEntity,
		                       Entity targetEntity, Environment environment,
		                       CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ShackleInfo) info;

			targetCharacter = targetEntity.GetComponent<SkillComponent>().Character;
			casterCharacter = casterEntity.GetComponent<SkillComponent>().Character;
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.Shackle;
		}

		protected override void OnUpdate(float dt) {
			if (IsFinish()) return;

			elapsed += dt;
			if (info.ShackleModifierConfig.followCasterPosition) {
				Vector2 casterPosition = casterCharacter.Position();
				Vector2 offsetPosition = info.ShackleModifierConfig.offsetPosition;
				Vector2 controlPosition =
					casterPosition + offsetPosition.FlipFollowDirection(casterCharacter.FacingDirection());
				targetCharacter.SetPosition(controlPosition);
			}
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			Entity entity = target.GameObject().GetComponent<EntityReference>().Entity;
			if (!entity.GetComponent<HealthComponent>().IsAlive()) return;
			target.PlayAnimation(AnimationName.IDLE);
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			target.InterruptChannelingSkill();
			target.PlayAnimation(AnimationName.SHACKLE);
		}
	}
}