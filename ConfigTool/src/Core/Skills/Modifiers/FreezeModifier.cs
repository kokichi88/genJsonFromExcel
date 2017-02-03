using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using EntityComponentSystem;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class FreezeModifier : BaseModifier {
		private FreezeInfo info;

		private AnimationComponent targetAnimationComponent;
		private MovementComponent targetMovementComponent;

		public FreezeModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (FreezeInfo) info;

			targetAnimationComponent = targetEntity.GetComponent<AnimationComponent>();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
		}

		public override ModifierType Type() {
			return ModifierType.Freeze;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			FreezeInfo fi = (FreezeInfo) modifierInfo;
			float duration = fi.Fmc.ShowDurationInSeconds();
			StatsComponent casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			Stats casterFreezeDurationUpScaleStats =
				casterStatsComponent.CharacterStats.FindStats(StatsType.FreezeDurationUpScale);
			StatsComponent targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			Stats targetFreezeDurationDownScaleStats =
				targetStatsComponent.CharacterStats.FindStats(StatsType.FreezeDurationDownScale);
			return new List<Lifetime>(new []{
				new DurationBasedLifetime(
					duration
					* (1 + targetFreezeDurationDownScaleStats.BakedFloatValue)
					* (1 + casterFreezeDurationUpScaleStats.BakedFloatValue)
				),
			});
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			target.InterruptChannelingSkill();

			targetAnimationComponent.Animation.PauseAnimation();

			if (targetMovementComponent.ConfigData.isFallable) {
				float gravity = targetMovementComponent.Gravity;
				for (int kIndex = 0; kIndex < targetMovementComponent.MovementRequests.Count; kIndex++) {
					Request r = targetMovementComponent.MovementRequests[kIndex];
					RequestType requestType = r.ShowRequestType();
					if (requestType == RequestType.StationaryJump
					    || requestType == RequestType.MovingJump) {
						gravity = ((StationaryJumpRequest) r).GroundingGravity;
					}
				}
				FallRequest fallRequest = new FallRequest(
					gravity, targetMovementComponent.Velocity.magnitude
				);
				targetMovementComponent.AddMovementRequest(fallRequest);
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			ResumeAnimation();
			TemplateArgs args = new TemplateArgs();
			args.SetEntry(TemplateArgsName.Position, (Vector2) target.Position());
			info.ShowParentSkill().TriggerEventWithId(info.Fmc.eid, args);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			if (byModifier.Type() == ModifierType.Freeze) {
				targetAnimationComponent.Animation.UnpauseAnimation();
				return;
			}

			targetAnimationComponent.Animation.PlayAnimation(AnimationName.IDLE, 1, PlayMethod.Crossfade, 0.2f);
			TemplateArgs args = new TemplateArgs();
			args.SetEntry(TemplateArgsName.Position, (Vector2) target.Position());
			info.ShowParentSkill().TriggerEventWithId(info.Fmc.eid, args);
		}

		private void ResumeAnimation() {
			targetAnimationComponent.Animation.UnpauseAnimation();
			targetAnimationComponent.Animation.PlayAnimation(AnimationName.IDLE, 1, PlayMethod.Crossfade, 0.2f);
		}
	}
}