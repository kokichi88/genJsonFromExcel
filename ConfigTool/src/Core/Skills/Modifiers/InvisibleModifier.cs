using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class InvisibleModifier : BaseModifier {
		private InvisibleInfo info;
		private readonly Entity targetEntity;

		private Stats invisibleStats;
		private GameObjectComponent gameObjectComponent;

		public InvisibleModifier(ModifierInfo info, Entity casterEntity,
		                         Entity targetEntity, Environment environment,
		                         CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.targetEntity = targetEntity;
			this.info = (InvisibleInfo) info;

			StatsComponent targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			bool found;
			invisibleStats = targetStatsComponent.CharacterStats.FindStats(StatsType.Invisible, out found);
			gameObjectComponent = (GameObjectComponent) targetEntity.GetComponent<EntityGameObjectComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.Invisible;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			if (invisibleStats != null && info.InvisibleModifierConfig.invisibleStats) {
				invisibleStats.SetBaseBoolValue(true);
			}

			DisableRenderers();
			DisableGameObjects();
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			if (invisibleStats != null && info.InvisibleModifierConfig.invisibleStats) {
				invisibleStats.SetBaseBoolValue(false);
			}

			EnableRenderers();
			EnableGameObjects();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			if (invisibleStats != null && info.InvisibleModifierConfig.invisibleStats) {
				invisibleStats.SetBaseBoolValue(false);
			}

			EnableRenderers();
			EnableGameObjects();
		}

		private void DisableGameObjects() {
			if (info.InvisibleModifierConfig.AreAnyGameObjectShouldBeDisabled()) {
				string[] names = info.InvisibleModifierConfig.ListAffectedGameObjectsName();
				for (int kIndex = 0; kIndex < names.Length; kIndex++) {
					try {
						gameObjectComponent.GameObject.transform.FindDeepChild(names[kIndex])
							.gameObject.SetActive(false);
					}
					catch (Exception e) {
						DLog.LogException(e);
					}
				}
			}
		}

		private void DisableRenderers() {
			if (info.InvisibleModifierConfig.IsAllRendererMode()) {
				gameObjectComponent.DisableRendererComponent();
			}
			else {
				string[] names = info.InvisibleModifierConfig.ListAffectedRenderersName();
				for (int kIndex = 0; kIndex < names.Length; kIndex++) {
					gameObjectComponent.DisableRendererComponent(names[kIndex]);
				}
			}
		}

		private void EnableGameObjects() {
			if (info.InvisibleModifierConfig.AreAnyGameObjectShouldBeDisabled()) {
				string[] names = info.InvisibleModifierConfig.ListAffectedGameObjectsName();
				for (int kIndex = 0; kIndex < names.Length; kIndex++) {
					try {
						gameObjectComponent.GameObject.transform.FindDeepChild(names[kIndex])
							.gameObject.SetActive(true);
					}
					catch (Exception e) {
						DLog.LogException(e);
					}
				}
			}
		}

		private void EnableRenderers() {
			if (info.InvisibleModifierConfig.IsAllRendererMode()) {
				gameObjectComponent.EnableRendererComponent();
			}
			else {
				string[] names = info.InvisibleModifierConfig.ListAffectedRenderersName();
				for (int kIndex = 0; kIndex < names.Length; kIndex++) {
					gameObjectComponent.EnableRendererComponent(names[kIndex]);
				}
			}
		}
	}
}