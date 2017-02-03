using System.Collections.Generic;
using System.Linq;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class AdvancedFrameModifier : BaseModifier {
		private AdvancedFrameInfo info;
		private Skill skill;

		private EquippedSkillsComponent esc;
		private HeroStateMachineComponent hsmc;
		private string stateAtAttachMoment;
		private UnpredictableDurationLifetime lifetime = new UnpredictableDurationLifetime();

		public AdvancedFrameModifier(AdvancedFrameInfo info, Entity casterEntity,
		                             Entity targetEntity, Skill skill, Environment environment,
		                             CollectionOfInteractions modifierInteractionCollection)
			: base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = info;
			this.skill = skill;
			this.targetEntity = targetEntity;
		}

		public override string Name() {
			return Type().ToString();
		}

		public override ModifierType Type() {
			return ModifierType.AdvancedFrame;
		}

		protected override void OnUpdate(float dt) {
			if (hsmc == null) return;
			if (!hsmc.HeroStateMachine.GetCurrentStateName().Equals(stateAtAttachMoment)) {
				lifetime.End();
				for (int i = 0; i < esc.EquippedSkills.ComboCount(); i++) {
					esc.EquippedSkills.GetCombo(i+1).AttackStateWindow.ReturnToOriginalValue();
				}

				for (int i = 0; i < esc.EquippedSkills.JumpAtkCount(); i++) {
					esc.EquippedSkills.GetJumpAtk(i + 1).AttackStateWindow.ReturnToOriginalValue();
				}

				skill.ReturnChannelingToOriginalValue();
				skill.ReturnStateBindingToOriginalValue();
			}
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			return new List<Lifetime>(new[] {lifetime});
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			esc = targetEntity.GetComponent<EquippedSkillsComponent>();
			hsmc = targetEntity.GetComponent<HeroStateMachineComponent>();
			int comboCount = esc.EquippedSkills.ComboCount();
			string[] interested = new string[comboCount];
			for (int i = 1; i <= comboCount; i++) {
				interested[i - 1] = StateName.COMBO_ATTACK + i;
			}
			string targetStateName = hsmc.HeroStateMachine.GetCurrentStateName();
			stateAtAttachMoment = targetStateName;
			if (interested.Contains(targetStateName)) {
				for (int i = 0; i < comboCount; i++) {
					esc.EquippedSkills.GetCombo(i+1).AttackStateWindow.StartSoonerBy(info.Value);
				}
			}

			int airComboCount = esc.EquippedSkills.JumpAtkCount();
			interested = new string[airComboCount];
			for (int i = 1; i < airComboCount; i++) {
				interested[i - 1] = StateName.JUMP_ATTACK + i;
			}

			if (interested.Contains(targetStateName)) {
				for (int i = 0; i < airComboCount; i++) {
					esc.EquippedSkills.GetJumpAtk(i + 1).AttackStateWindow.StartSoonerBy(info.Value);
				}
			}

			skill.EndChannelingSoonerBy(info.ChannelingValue);
			skill.EndStateBindingSoonerBy(info.StateBindingValue);
		}

		public override object[] Cookies() {
			return new object[0];
		}
	}
}