using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class LifeStealModifier : BaseModifier {
		private readonly LifeStealInfo info;

		private HealthComponent targetHealthComponent;
		private float powerScale = 1;
		private Character casterCharacter;

		public LifeStealModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                         Environment environment,
		                         CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (LifeStealInfo) info;
			targetHealthComponent = targetEntity.GetComponent<HealthComponent>();
			casterCharacter = casterEntity.GetComponent<SkillComponent>().Character;
		}

		public override ModifierType Type() {
			return ModifierType.LifeSteal;
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
			if (caster != casterCharacter) return;
			Heal(damage);
		}

		private void Heal(int damage) {
			int healAmount = (int) (damage * info.Lsmc.percent * powerScale);
			if (info.Lsmc.specific) {
				healAmount = (int) ((int) info.Lsmc.percent * powerScale);
			}
			targetHealthComponent.RecoverHealthBy(healAmount);
		}

		public void SetPowerScale(float value) {
			this.powerScale = value;
		}
	}
}