using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using DefaultHero = Ssar.Combat.HeroStateMachines.HeroStateMachineComponent.DefaultHero;

namespace Core.Skills.Modifiers {
	public class StatsModifier : BaseModifier {
		private readonly SkillCastingSource src;
		private StatsInfo info;

		private StatsComponent targetStatsComponent;
		private Stats stats;
		private HealthComponent targetHealthComponent;
		private StatsType statsType;
		private List<ValueModifier> valueModifiers = new List<ValueModifier>();
		private List<List<ValueModifier>> extraValueModifiers = new List<List<ValueModifier>>();
		private float powerScale = 1;
		private DefaultHero targetHero;
		private DurationBasedLifetime lifetime;
		private int stackCount = 1;

		public StatsModifier(ModifierInfo info, Entity casterEntity,
		                     Entity targetEntity, Environment environment,
		                     CollectionOfInteractions modifierInteractionCollection,
		                     SkillCastingSource src) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.src = src;
			this.info = (StatsInfo) info;
			targetStatsComponent = targetEntity.GetComponent<StatsComponent>();
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			statsType = this.info.Smc.ShowStatsType();
			targetHero = (DefaultHero) targetEntity.GetComponent<HeroStateMachineComponent>().StateMachineHero;
		}

		public override ModifierType Type() {
			return ModifierType.Stats;
		}

		public override int SubType() {
			return (int) statsType;
		}

		public override string Name() {
			return base.Name() + ": " + statsType + ", Stack: " + stackCount;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return info.Smc.statsModifierValue >= 0;
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
			bool found = false;
			RemoveValueModifier();
			ModifyStats();

			if (statsType == StatsType.HpScale) {
				targetHealthComponent.Update();
				if (info.Smc.recoverHp && src.Src == SkillCastingSource.Source.EntityCreation) {
					targetHealthComponent.RecoverHealthBy(targetHealthComponent.MaxHealth, false);
					targetHealthComponent.Update();
				}
			}
		}

		private void ModifyStats() {
			bool found;
			stats = targetStatsComponent.CharacterStats.FindStats(statsType, out found);
			if (found) {
				float value = CalculateValue();
				ValueModifier valueModifier = stats.AddModifier(info.Smc.ShowStatsModifierOperator(), value);
				valueModifiers.Add(valueModifier);
			}

			for (int kIndex = 0; kIndex < info.Smc.extras.Count; kIndex++) {
				StatsModifierValue smv = info.Smc.extras[kIndex];
				bool extraFound;
				Stats extraStats = targetStatsComponent.CharacterStats.FindStats(smv.ShowStatsType(), out extraFound);
				if (extraFound) {
					float extraValue = smv.statsModifierValue * powerScale;
					ValueModifier extraValueModifier = extraStats.AddModifier(smv.ShowOperator(), extraValue);
					if (extraValueModifiers.Count - 1 < kIndex) {
						extraValueModifiers.Add(new List<ValueModifier>());
					}
					extraValueModifiers[kIndex].Add(extraValueModifier);
				}
			}
		}

		private float CalculateValue() {
			return info.Smc.statsModifierValue * powerScale;
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			RemoveValueModifier();
			if (statsType == StatsType.HpScale) {
				targetHealthComponent.Update();
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			RemoveValueModifier();
			if (statsType == StatsType.HpScale) {
				targetHealthComponent.Update();
			}
		}

		public override StackResult TryStackWithNewOne(Modifier newOne) {
			if (info.Smc.stack) {
				ResetLifetime();
				StackValue(newOne);
				IncreaseStackCount();
				// DLog.Log("Stack count: " + ShowStackCount());
				return StackResult.Manual;
			}

			if (info.Smc.retain) {
				return StackResult.Stack;
			}
			return StackResult.None;
		}

		public void IncreaseStackCount() {
			stackCount++;
		}

		public void DecreaseStackCount() {
			stackCount--;
		}

		public void StackValue(Modifier newOne) {
			StatsInfo statsInfoFromNewOne = (StatsInfo) newOne.Cookies()[0];
			ValueModifier valueModifier = stats.AddModifier(
				statsInfoFromNewOne.Smc.ShowStatsModifierOperator(),
				((StatsModifier)newOne).CalculateValue()
			);

			valueModifiers.Add(valueModifier);

			for (int kIndex = 0; kIndex < statsInfoFromNewOne.Smc.extras.Count; kIndex++) {
				StatsModifierValue extra = statsInfoFromNewOne.Smc.extras[kIndex];
				ValueModifier extraModifier = stats.AddModifier(
					extra.ShowOperator(), extra.statsModifierValue * powerScale
				);
				if (extraValueModifiers.Count - 1 < kIndex) {
					extraValueModifiers.Add(new List<ValueModifier>());
				}
				extraValueModifiers[kIndex].Add(extraModifier);
			}
		}

		public void Unstack() {
			ValueModifier valueModifier = valueModifiers[valueModifiers.Count - 1];
			valueModifiers.Remove(valueModifier);
			stats.RemoveModifier(valueModifier);

			foreach (List<ValueModifier> childExtras in extraValueModifiers) {
				ValueModifier valueModifierOfExtra = childExtras[childExtras.Count - 1];
				childExtras.Remove(valueModifierOfExtra);
				stats.RemoveModifier(valueModifierOfExtra);
			}
			DecreaseStackCount();
		}

		public void ResetLifetime() {
			lifetime.ResetElapsedTime();
		}

		public override int ShowStackCount() {
			return stackCount;
		}

		public override bool IsInvalidated() {
			if (SubType() == (int) StatsType.ResAllScale) {
				return valueModifiers[valueModifiers.Count - 1].Value == 0;
			}
			return base.IsInvalidated();
		}

		public override bool IsValidated() {
			if (SubType() == (int) StatsType.ResAllScale) {
				return valueModifiers[valueModifiers.Count - 1].Value > 0;
			}
			return base.IsValidated();
		}

		public string ShowIconName() {
			return info.Smc.ShowIcon();
		}

		private void RemoveValueModifier() {
			foreach (ValueModifier valueModifier in valueModifiers) {
				if (valueModifier != null) {
					if (stats != null) {
						stats.RemoveModifier(valueModifier);
					}
				}
			}
			valueModifiers.Clear();

			foreach (List<ValueModifier> childExtra in extraValueModifiers) {
				foreach (ValueModifier valueModifier in childExtra) {
					if (valueModifier != null) {
						if (stats != null) {
							stats.RemoveModifier(valueModifier);
						}
					}
				}
				childExtra.Clear();
			}
			extraValueModifiers.Clear();
		}

		public StatsType StatsType
		{
			get { return statsType; }
		}

		public void SetPowerScale(float value, bool reapplyStats = true) {
			powerScale = value;
			if (reapplyStats) {
				RemoveValueModifier();
				ModifyStats();
				if (statsType == StatsType.HpScale) {
					targetHealthComponent.Update();
				}
			}
		}
	}
}