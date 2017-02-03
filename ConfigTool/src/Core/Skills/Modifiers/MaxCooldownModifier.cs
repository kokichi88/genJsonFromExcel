using System.Collections.Generic;
using Artemis;
using Core.Skills.Cooldowns;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class MaxCooldownModifier : BaseModifier {
		private MaxCooldownInfo info;
		private Character targetCharacter;

		private List<TimeCooldownResource> timeCooldownResources = new List<TimeCooldownResource>();
		private List<RecoverableChargeResource> recoverableChargeResources = new List<RecoverableChargeResource>();
		private float powerScale = 1;

		public MaxCooldownModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment,
		                        CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (MaxCooldownInfo) info;
			targetCharacter = targetEntity.GetComponent<SkillComponent>().Character;
		}

		public override ModifierType Type() {
			return ModifierType.MaxCooldown;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			AdjustCooldownAndRecharge(target);
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			Reset();
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			Reset();
		}

		public void SetPowerScale(float value) {
			powerScale = value;
			Reset();
			AdjustCooldownAndRecharge(targetCharacter);
		}

		private void AdjustCooldownAndRecharge(Character target) {
			Dictionary<SkillId, SkillCastingRequirement> requirements = target.GetSkillCastingRequirements();
			float cooldownRatio = 1 - info.Cmc.cdReduction * powerScale;
			float rechargeRatio = 1 - info.Cmc.rcReduction * powerScale;
			foreach (KeyValuePair<SkillId, SkillCastingRequirement> pair in requirements) {
				foreach (Resource res in pair.Value.Resources) {
					if (res is TimeCooldownResource) {
						TimeCooldownResource tcr = (TimeCooldownResource) res;
						timeCooldownResources.Add(tcr);
						tcr.AdjustDurationWithRatio(cooldownRatio);
					}

					if (res is RecoverableChargeResource) {
						RecoverableChargeResource rcr = (RecoverableChargeResource) res;
						recoverableChargeResources.Add(rcr);
						rcr.AdjustDurationWithRatio(rechargeRatio);
					}
				}
			}
		}

		private void Reset() {
			foreach (TimeCooldownResource tcr in timeCooldownResources) {
				tcr.AdjustDurationWithRatio(1);
			}

			foreach (RecoverableChargeResource rcr in recoverableChargeResources) {
				rcr.AdjustDurationWithRatio(1);
			}
		}
	}
}