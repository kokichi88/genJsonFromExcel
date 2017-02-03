using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using EntityComponentSystem;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class AetherOnDamagedModifier : BaseModifier {
		private AetherOnDamagedInfo info;

		private AetherComponent aetherComponent;
		private float powerScale = 1;
		private Character targetCharacter;

		public AetherOnDamagedModifier(ModifierInfo info,
		                               Entity casterEntity, Entity targetEntity, 
		                               Environment environment, 
		                               CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (AetherOnDamagedInfo) info;
			aetherComponent = targetEntity.GetComponent<AetherComponent>();
			targetCharacter = targetEntity.GetComponent<SkillComponent>().Character;
		}

		public override ModifierType Type() {
			return ModifierType.AetherOnDamaged;
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

			if (target == targetCharacter) {
				aetherComponent.Add((int) (info.AetherOnDamagedModifierConfig.aether * powerScale));
			}
		}

		public override StackResult TryStackWithNewOne(Modifier newOne) {
			return info.AetherOnDamagedModifierConfig.stack ? StackResult.Stack : StackResult.None;
		}

		public void SetPowerScale(float value) {
			powerScale = value;
		}
	}
}