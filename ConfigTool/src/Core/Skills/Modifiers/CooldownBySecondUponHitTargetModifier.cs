using System.Collections.Generic;
using Artemis;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Cooldowns;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class CooldownBySecondUponHitTargetModifier : BaseModifier {
		private CooldownBySecondUponHitTargetInfo info;

		private Character modifierTarget;

		public CooldownBySecondUponHitTargetModifier(ModifierInfo info, Entity casterEntity,
		                                             Entity targetEntity, Environment environment,
		                                             CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (CooldownBySecondUponHitTargetInfo) info;
			modifierTarget = targetEntity.GetComponent<SkillComponent>().Character;
		}

		public override ModifierType Type() {
			return ModifierType.CooldownBySecondUponHitTarget;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}

		public override void OnDamageDealt(Character caster, Character target, Skill fromSkill,
		                                   Modifier fromModifier, int damage) {
			base.OnDamageDealt(caster, target, fromSkill, fromModifier, damage);
			SkillId skillId = null;
			if (caster == modifierTarget && fromSkill != null
			                              && caster.SkillId(fromSkill, ref skillId)
			                              && info.Config.IsProducerInterested(skillId.Category)) {
				Dictionary<SkillId, SkillCastingRequirement> requirements = modifierTarget.GetSkillCastingRequirements();
				foreach (KeyValuePair<SkillId, SkillCastingRequirement> pair in requirements) {
					if (!info.Config.IsConsumerInterested(pair.Key.Category)) continue;
					foreach (Resource res in pair.Value.Resources) {
						if (res is TimeCooldownResource) {
							TimeCooldownResource tcr = (TimeCooldownResource) res;
							tcr.ReduceRemainingTimeBy(info.Config.cdReduction);
						}

						if (res is RecoverableChargeResource) {
							RecoverableChargeResource rcr = (RecoverableChargeResource) res;
							rcr.ReduceRemainingTimeBy(info.Config.rcReduction);
						}
					}
				}
			}
		}
	}
}