using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Skills.Cooldowns;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class CooldownBySecondModifier : BaseModifier {
		private SkillId parentSkillId;
		private CooldownBySecondInfo info;

		private float powerScale = 1;
		private Stats cooldownAndRechargeReductionScaleStats;
		private float multiplier = 1;

		public CooldownBySecondModifier(ModifierInfo info, Entity casterEntity,
		                                Entity targetEntity, SkillId parentSkillId,
		                                Environment environment,
		                                CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.parentSkillId = parentSkillId;
			this.info = (CooldownBySecondInfo) info;
			cooldownAndRechargeReductionScaleStats = targetEntity.GetComponent<StatsComponent>().CharacterStats
				.FindStats(StatsType.CooldownAndRechargeReductionScale);
			if (this.info.Cbsmc.statsAffection) {
				multiplier = cooldownAndRechargeReductionScaleStats.BakedFloatValue;
			}
		}

		public override ModifierType Type() {
			return ModifierType.CooldownBySecond;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			return new List<Lifetime>(new []{new DurationBasedLifetime(0.1f), });
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			Dictionary<SkillId, SkillCastingRequirement> requirements = target.GetSkillCastingRequirements();
			foreach (KeyValuePair<SkillId, SkillCastingRequirement> pair in requirements) {
				if (pair.Key.Category.ShowParentSkillCategory() == ParentSkillCategory.Passive) continue;
				if (info.Cbsmc.ShowMode() == CooldownBySecondModifierConfig.Mode.Local) {
					if (!pair.Key.Equals(parentSkillId)) continue;
				}
				foreach (Resource res in pair.Value.Resources) {
					if (res is TimeCooldownResource) {
						TimeCooldownResource tcr = (TimeCooldownResource) res;
						tcr.ReduceRemainingTimeBy(info.Cbsmc.cdReduction * powerScale * multiplier);
					}

					if (res is RecoverableChargeResource) {
						RecoverableChargeResource rcr = (RecoverableChargeResource) res;
						rcr.ReduceRemainingTimeBy(info.Cbsmc.rcReduction * powerScale * multiplier);
					}
				}
			}
		}

		public void SetPowerScale(float value) {
			powerScale = value;
		}
	}
}