using System;
using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Skills.Vfxs;
using Core.Utils;
using MovementSystem.Components;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.MonsterStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class StunModifier : StaggerModifier {
		private StunInfo info;

		private Stats extraStunDuration;
		private Transform circleProgressBar;
		private Material circleProgressBarMaterial;
		private float duration = 0;
		private AnimationComponent animComponent;
		private string animNameToTrack;
		private bool isAnimToTrackPlaying;
		private bool isResumedToStunAnimation;
		private float animDuration;
		private float animElapsed;

		public StunModifier(StunInfo info, Entity casterEntity, Entity targetEntity,
		                    Vector3 collidedProjectilePosition, Environment environment,
		                    CollectionOfInteractions modifierInteractionCollection)
			: this(
				info, info.Config.ShowLevel(),
				casterEntity, targetEntity, collidedProjectilePosition, environment, modifierInteractionCollection
			) {
			this.info = info;
			bool found;
			extraStunDuration = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.ExtraStunDuration, out found);
			duration = this.info.Config.ShowDurationInSeconds();
		}

		public StunModifier(StunInfo info, WeightLevel forceLevel, Entity casterEntity, Entity targetEntity,
		                    Vector3 collidedProjectilePosition, Environment environment,
		                    CollectionOfInteractions modifierInteractionCollection)
			: base(
				new StaggerInfo(
					info.Target(), info.ShowParentSkill(),
					info.Config.distance, info.Config.movementDuration,
					info.ShowSuccessRate(), info.DelayToApply(),
					forceLevel, info.ShowBehaviors(), info.Config.overrideAnimation,
					info.ShowVfxConfig(), info.Config.ShowMovementBehavior(), info.Config.ShowFacingBehavior(),
					StaggerModifierConfig.Requirement.Any, info.ShowLifetimeConfigs(),
					info.Config.loopAnimation,
					info.Config.crossfade, info.Config.animFrame
				),
				casterEntity, targetEntity, collidedProjectilePosition, environment, modifierInteractionCollection
			) {
			this.info = info;
			bool found;
			extraStunDuration = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.ExtraStunDuration, out found);
			animComponent = targetEntity.GetComponent<AnimationComponent>();
		}

		public override string Name() {
			return string.Format("{0}({1})", Type(), attachType);
		}

		public override ModifierType Type() {
			return ModifierType.Stun;
		}

		protected override void OnUpdate(float dt) {
			base.OnUpdate(dt);

			lifetime.DynamicExtraDuration = extraStunDuration.BakedFloatValue;
			if (circleProgressBarMaterial) {
				circleProgressBarMaterial.SetFloat("_Cutoff", 1 - lifetime.ShowRemainingDuration() / duration);
			}

			if (!string.IsNullOrEmpty(animNameToTrack)) {
				animElapsed += dt;
				bool lastPlayingStatus = isAnimToTrackPlaying;
				isAnimToTrackPlaying = animComponent.Animation.IsPlaying(animNameToTrack);
				bool resumeAnim = false;
				if (lastPlayingStatus && isAnimToTrackPlaying == false) {
					if (!isResumedToStunAnimation) {
						isResumedToStunAnimation = true;
						animNameToTrack = string.Empty;
						resumeAnim = true;
					}
				}

				if (animElapsed + 0.4f > animDuration) {
					if (!isResumedToStunAnimation) {
						isResumedToStunAnimation = true;
						animNameToTrack = string.Empty;
						resumeAnim = true;
					}
				}

				if (resumeAnim) {
					animComponent.Animation.PlayAnimation(
						info.Config.loopAnimation, 1, PlayMethod.Crossfade, 0.2f
					);
				}
			}
		}

		protected override void OnVfxPrefabSpawn(Vfx.SpawnPrefab logic) {
			base.OnVfxPrefabSpawn(logic);
			Renderer renderer = logic.Vfx.GetComponent<Renderer>();
			if (renderer) {
				if (renderer.material.HasProperty("_Cutoff")) {
					circleProgressBarMaterial = renderer.material;
				}
			}
		}

		public void OnAnimationPlayedBySubModifier(string animationName) {
			animNameToTrack = animationName;
			isResumedToStunAnimation = false;
			animDuration = animComponent.Animation.Duration(animationName);
		}
	}
}