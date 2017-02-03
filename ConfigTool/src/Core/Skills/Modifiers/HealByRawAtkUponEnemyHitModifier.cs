using Artemis;
using Combat.DamageSystem;
using Combat.Stats;
using Core.Skills.Modifiers.Info;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class HealByRawAtkUponEnemyHitModifier : BaseModifier {
		private HealByRawAtkUponEnemyHitInfo info;

		private Character modifierTarget;
		private EquippedSkillsComponent equippedSkillsComponent;
		private HealthComponent healthComponent;
		private Stats rawAtk;

		public HealByRawAtkUponEnemyHitModifier(ModifierInfo info, Entity casterEntity,
		                                        Entity targetEntity, Environment environment,
		                                        CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (HealByRawAtkUponEnemyHitInfo) info;

			modifierTarget = targetEntity.GetComponent<SkillComponent>().Character;
			equippedSkillsComponent = targetEntity.GetComponent<EquippedSkillsComponent>();
			healthComponent = targetEntity.GetComponent<HealthComponent>();
			rawAtk = targetEntity.GetComponent<StatsComponent>().CharacterStats.FindStats(StatsType.RawAtk);
		}

		public override ModifierType Type() {
			return ModifierType.HealByRawAtkUponEnemyHit;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
		}

		public override void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
			base.OnDamageDealt(caster, target, fromSkill, fromModifier, damage);

			SkillId skillId = null;
			if (caster == modifierTarget && fromSkill != null
			                             && caster.SkillId(fromSkill, ref skillId)
			                             && fromModifier == null
			                             && skillId.Category.ShowParentSkillCategory() == ParentSkillCategory.Combo) {
				float powerScale = equippedSkillsComponent.GetFinalValueOfPowerScale(skillId);
				healthComponent.RecoverHealthBy((int) (info.Config.percentage * rawAtk.BakedFloatValue * powerScale));
			}
		}
	}
}