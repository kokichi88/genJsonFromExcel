using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using MovementSystem.Components;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class VanishModifier : BaseModifier {
		private VanishInfo info;
		private Entity targetEntity;

		private float elapsed;
		private StatsComponent targetStatsComponent;
		private GameObjectComponent targetGameObjectComponent;
		private Stats targetVanishStats;
		private MovementComponent targetMovementComponent;
		private MovementComponent casterMovementComponent;

		public VanishModifier(ModifierInfo info, Entity casterEntity,
		                      Entity targetEntity, Environment environment,
		                      CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (VanishInfo) info;
			this.targetEntity = targetEntity;
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.Vanish;
		}

		protected override void OnUpdate(float dt) {
			if (IsFinish()) return;

			elapsed += dt;

			float progress = elapsed / info.VanishModifierConfig.translationDuration;
			Vector2 targetPos = targetMovementComponent.Position;
			Vector2 casterPos = casterMovementComponent.Position;
			Vector2 newPos = Vector2.Lerp(targetPos, casterPos, progress);
			targetMovementComponent.Move(newPos - targetPos);
		}

		public override bool IsBuff() {
			return false;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			if (targetVanishStats != null) {
				targetVanishStats.SetBaseBoolValue(false);
				targetGameObjectComponent.EnableRendererObject();
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			if (targetVanishStats != null) {
				targetVanishStats.SetBaseBoolValue(false);
				targetGameObjectComponent.EnableRendererObject();
			}
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			target.InterruptChannelingSkill();
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			targetGameObjectComponent = (GameObjectComponent) targetEntity.GetComponent<EntityGameObjectComponent>();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();

			bool found;
			targetVanishStats = targetStatsComponent.CharacterStats.FindStats(StatsType.Vanish, out found);
			targetVanishStats.SetBaseBoolValue(true);
			targetGameObjectComponent.DisableRendererObject();

			casterMovementComponent = casterEntity.GetComponent<MovementComponent>();
		}
	}
}