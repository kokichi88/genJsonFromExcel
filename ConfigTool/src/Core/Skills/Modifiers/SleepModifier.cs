using System;
using System.Collections;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using MEC;
using MovementSystem.Components;
using MovementSystem.Requests;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class SleepModifier : BaseModifier {
		private SleepInfo info;
		private readonly Entity casterEntity;
		private readonly Entity targetEntity;
		private Skill parentSkill;

		private DurationBasedLifetime lifetime;
		private AnimationComponent targetAnimationComponent;
		private MovementComponent targetMovementComponent;
		private StatsComponent targetStatsComponent;
		private Character targetCharacter;
		private StatsComponent casterStatsComponent;
		private float playLoopAt;
		private float crossfadeDuration;
		private float elapsed;
		private bool isLoopAnimationPlayed;
		private Stats targetStatusModifierStats;
		private ValueModifier statusModifierValueModifier;
		/*private float playIdleAnimationAt;
		private bool isIdleAnimationPlayed;*/

		public SleepModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                     Environment environment,
		                     CollectionOfInteractions modifierInteractionCollection,
		                     Skill parentSkill) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (SleepInfo) info;
			this.casterEntity = casterEntity;
			this.targetEntity = targetEntity;
			this.parentSkill = parentSkill;

			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			playLoopAt = fasc.FramesToSeconds(this.info.Smc.loopFrame);
			crossfadeDuration = fasc.FramesToSeconds(this.info.Smc.xfadeDur);
			targetAnimationComponent = targetEntity.GetComponent<AnimationComponent>();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			targetCharacter = targetEntity.GetComponent<SkillComponent>().Character;
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			/*float duration = FrameAndSecondsConverter._30Fps.FramesToSeconds(this.info.Smc.duration);
			playIdleAnimationAt = Mathf.Max(0, duration - crossfadeDuration);*/
		}

		public override ModifierType Type() {
			return ModifierType.Sleep;
		}

		protected override void OnUpdate(float dt) {
			if (IsFinish()) return;

			elapsed += dt;

			if(elapsed >= playLoopAt && !isLoopAnimationPlayed) {
				isLoopAnimationPlayed = true;
				if (!string.IsNullOrEmpty(info.Smc.loopAnim)) {
					targetAnimationComponent.Animation.PlayAnimation(
						info.Smc.loopAnim, 1, PlayMethod.Crossfade, crossfadeDuration
					);
				}
			}
		}

		public override bool IsBuff() {
			return false;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = base.CreateLifetimes(modifierInfo);
			foreach (Lifetime l in lifetimes) {
				if (l is DurationBasedLifetime) {
					lifetime = (DurationBasedLifetime) l;
				}
			}
			return lifetimes;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			target.InterruptChannelingSkill();

			targetAnimationComponent.Animation.PlayAnimation(info.Smc.startupAnim, 1, PlayMethod.Play, 0);
			targetAnimationComponent.Animation.JumpToFrame(info.Smc.jumpFrame);

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

			targetStatusModifierStats = targetStatsComponent.CharacterStats.FindStats(StatsType.StatusModifier);
			Stats targetWakeDmgDownScaleStats = targetStatsComponent.CharacterStats.FindStats(StatsType.WakeDmgDownScale);
			Stats casterWakeDmgUpScaleStats = casterStatsComponent.CharacterStats.FindStats(StatsType.WakeDmgUpScale);
			statusModifierValueModifier = targetStatusModifierStats.AddModifier(
				StatsModifierOperator.Addition,
				info.Smc.bonusDmg
				* (1 + targetWakeDmgDownScaleStats.BakedFloatValue)
				* (1 + casterWakeDmgUpScaleStats.BakedFloatValue)
			);
		}

		public override void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			base.OnDamageDealt(caster, target, fromSkill, fromModifier, damage);

			bool isDmgFromDOTModifier = fromModifier != null && fromModifier.Type() == ModifierType.DamageOverTime;
			if (target == targetCharacter && fromSkill != parentSkill && !isDmgFromDOTModifier) {
				lifetime.End();
				targetStatusModifierStats.RemoveModifier(statusModifierValueModifier);

				Action addSubModifiers = () => {
					foreach (BaseModifierConfig subModifier in info.Smc.subModifiers) {
						DefaultSkill.Dependencies dependencies = ((DefaultSkill) parentSkill).Dependencies_;
						ModifierInfoFactory modifierInfoFactory = dependencies.ModifierInfoFactory;
						ModifierInfo mi =
							modifierInfoFactory.CreateFrom(parentSkill, subModifier, dependencies.Environment);
						Modifier modifier = DamageSystem.Instance.CreateModifier(
							mi, casterEntity, targetEntity, target.Position(),
							target.Position(), parentSkill, dependencies.Config.ShowSkillId(), 0
						);
						if (modifier != null) {
							target.AddModifier(modifier);
						}
					}
				};
				target.AddLoopable(new WaitLoopable(1, addSubModifiers));
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetStatusModifierStats.RemoveModifier(statusModifierValueModifier);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetStatusModifierStats.RemoveModifier(statusModifierValueModifier);
		}

		private class WaitLoopable : Loopable {
			private int waitCount;
			private Action action;

			private int count;
			private bool finish;

			public WaitLoopable(int waitCount, Action action) {
				this.waitCount = waitCount;
				this.action = action;
			}

			public void Update(float dt) {
				if (finish) return;
				count++;
				if (count >= waitCount) {
					finish = true;
					action();
				}
			}

			public void LateUpdate(float dt) {
			}

			public void Interrupt() {
				finish = true;
			}

			public bool IsFinished() {
				return finish;
			}
		}
	}
}