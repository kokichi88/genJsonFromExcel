using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using MovementSystem.Components;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class DamageOverTimeModifier : BaseModifier {
		private DamageOverTimeInfo info;
		private Environment environment;
		private Skill parentSkill;
		private readonly SkillId skillId;

		private float elapsed;
		private float timeUntilNextDamage;
		private HealthComponent targetHealthComponent;
		private MovementComponent targetMovementComponent;
		private int casterAtk;
		private BakedStatsContainer characterStats;
		private CleanupComponent casterCleanupComponent;
		private int cleanupTicketId;
		private UnpredictableDurationLifetime _1HpLifetime;

		public DamageOverTimeModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                              Environment environment, Skill parentSkill, SkillId skillId,
		                              CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (DamageOverTimeInfo) info;
			this.environment = environment;
			this.parentSkill = parentSkill;
			this.skillId = skillId;
			bool found;
			StatsComponent casterStats = casterEntity.GetComponent<StatsComponent>();
			casterAtk = (int) casterStats.CharacterStats.FindStats(StatsType.RawAtk, out found).BakedFloatValue;
			timeUntilNextDamage = this.info.DotModifierConfig.interval;
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			targetMovementComponent = targetEntity.GetComponent<MovementComponent>();
			characterStats = casterStats.CharacterStats;
			casterCleanupComponent = casterEntity.GetComponent<CleanupComponent>();
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.DamageOverTime;
		}

		protected override void OnUpdate(float dt) {
			if (IsFinish()) return;

			elapsed += dt;
			timeUntilNextDamage -= dt;
			if (timeUntilNextDamage < 0) {
				timeUntilNextDamage = this.info.DotModifierConfig.interval;
				SourceHistory sourceHistory = new SourceHistory(Source.FromSkill(parentSkill, skillId))
					.Add(Source.FromModifier(this));
				DamageFromAttack damage = new DamageFromAttack(
					sourceHistory,
					this.info.DotModifierConfig.damageScale, false, 1f, 1,
					casterEntity.Id, targetMovementComponent.Position,
					targetMovementComponent.Position,
					characterStats,
					false, info.DotModifierConfig.ShowDeathBehavior(),
					false
				);
				damage.CauseTargetToDie(false);
				targetHealthComponent.ReceiveDamage(damage);
			}

			if (targetHealthComponent.Health <= 1) {
				_1HpLifetime.End();
			}
		}

		public override bool IsBuff() {
			return false;
		}

		public override object[] Cookies() {
			return new[] {info};
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			cleanupTicketId = casterCleanupComponent.Obtain();
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			DamageOverTimeInfo doti = (DamageOverTimeInfo) modifierInfo;
			StatsComponent casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			Stats casterBurnDurationUpScaleStats =
				casterStatsComponent.CharacterStats.FindStats(StatsType.BurnDurationUpScale);
			StatsComponent targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			Stats targetBurnDurationDownScaleStats =
				targetStatsComponent.CharacterStats.FindStats(StatsType.BurnDurationDownScale);

			float duration = doti.DotModifierConfig.ShowDurationInSeconds();

			_1HpLifetime = new UnpredictableDurationLifetime();
			return new List<Lifetime>(new Lifetime[] {
				new DurationBasedLifetime(
					duration
					* (1 + targetBurnDurationDownScaleStats.BakedFloatValue)
					* (1 + casterBurnDurationUpScaleStats.BakedFloatValue)
				),
				_1HpLifetime
			});
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			casterCleanupComponent.Consume(cleanupTicketId);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			casterCleanupComponent.Consume(cleanupTicketId);
		}
	}
}