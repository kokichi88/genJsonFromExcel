using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using MEC;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.Modifiers {
	public class StaticModifier : BaseModifier {
		private StaticInfo info;
		private readonly Environment environment;

		private Character caster;
		private Character target;
		private HealthComponent targetHealthComponent;
		private StatsComponent casterStatsComponent;
		private StatsComponent targetStatsComponent;
		private Stats targetStatusModifierStats;
		private ValueModifier statusModifierValueModifier;
		private float duration = 0;

		public StaticModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment, CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (StaticInfo) info;
			this.environment = environment;
			caster = casterEntity.GetComponent<SkillComponent>().Character;
			target = targetEntity.GetComponent<SkillComponent>().Character;
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			casterStatsComponent = casterEntity.GetComponent<StatsComponent>();
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();

			duration = this.info.Smc.ShowDurationInSeconds();
		}

		public override ModifierType Type() {
			return ModifierType.Static;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return false;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			targetStatusModifierStats = targetStatsComponent.CharacterStats.FindStats(StatsType.StatusModifier);
			Stats targetShockDmgDownScaleStats = targetStatsComponent.CharacterStats.FindStats(StatsType.ShockDmgDownScale);
			Stats casterShockDmgUpScaleStats = casterStatsComponent.CharacterStats.FindStats(StatsType.ShockDmgUpScale);
			statusModifierValueModifier = targetStatusModifierStats.AddModifier(
				StatsModifierOperator.Addition,
				info.Smc.bonusDmg
				* (1 + targetShockDmgDownScaleStats.BakedFloatValue)
				* (1 + casterShockDmgUpScaleStats.BakedFloatValue)
			);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			targetStatusModifierStats.RemoveModifier(statusModifierValueModifier);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			targetStatusModifierStats.RemoveModifier(statusModifierValueModifier);
		}

		public override void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			base.OnDamageDealt(caster, target, fromSkill, fromModifier, damage);

			GameObject vfxPrefab = info.Smc.ShowStoredPrefab();
			if (vfxPrefab != null) {
				GameObject vfx = environment.InstantiateGameObject(vfxPrefab);
				vfx.transform.position = target.Position();
				Timing.RunCoroutine(_WaitThenInvoke(duration, () => { GameObject.Destroy(vfx); }));
			}
		}

		private IEnumerator<float> _WaitThenInvoke(float waitTime, Action action) {
			yield return Timing.WaitForSeconds(waitTime);
			action();
		}
	}
}