using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class HpShieldModifier : BaseModifier {
		private HpShieldInfo info;

		private int shieldHp;
		private int accumulatedDamage;
		private HealthComponent hc;
		private bool isHpRecovered;
		private float powerScale = 1;

		public HpShieldModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity, Environment environment,
		                        CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity,
			targetEntity, environment, modifierInteractionCollection) {
			this.info = (HpShieldInfo) info;
		}

		public override ModifierType Type() {
			return ModifierType.HpShield;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			DamageSystem.Instance.EntityBeHitSingleEventHandler += OnEntityBeHitSingle;
			TurnShieldOn();
		}

		protected override void OnLateUpdate(float dt) {
			base.OnLateUpdate(dt);
			if (!isHpRecovered) {
				isHpRecovered = true;
				hc.RecoverHealthBy(shieldHp, false);
			}
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			DamageSystem.Instance.EntityBeHitSingleEventHandler -= OnEntityBeHitSingle;
			TurnShieldOff();
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			DamageSystem.Instance.EntityBeHitSingleEventHandler -= OnEntityBeHitSingle;
			TurnShieldOff();
		}

		public int ShowMaxHpOfShield() {
			return shieldHp;
		}

		public int ShowCurrentHpOfShield() {
			int value = shieldHp - accumulatedDamage;
			//DLog.Log("Current hp of shield " + value);
			return Mathf.Max(0, value);
		}

		public void SetPowerScale(float value) {
			powerScale = value;
			TurnShieldOff();
			TurnShieldOn();
		}

		private void TurnShieldOff() {
			hc.Health -= ShowCurrentHpOfShield();
			hc.DecreaseMaxHealthBy(shieldHp);
		}

		private void TurnShieldOn() {
			hc = targetEntity.GetComponent<HealthComponent>();
			shieldHp = (int) (hc.MaxHealth * info.Hsmc.percent * powerScale);
			hc.IncreaseMaxHealthBy(shieldHp);
		}

		private void OnEntityBeHitSingle(object sender, EntityBeHitEventArgs e) {
			if (!isHpRecovered) return;
			if (e.Target == targetEntity) {
				accumulatedDamage += (int) e.HealthDrop;
				if (accumulatedDamage >= shieldHp) {
					targetEntity.GetComponent<SkillComponent>().Character.RemoveModifier(this);
				}
			}
		}
	}
}