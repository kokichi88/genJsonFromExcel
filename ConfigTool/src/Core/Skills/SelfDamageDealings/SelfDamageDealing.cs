using Artemis;
using Combat.DamageSystem;
using Combat.Stats;
using EntityComponentSystem;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using SourceHistory = Core.Skills.DamageFromAttack.SourceHistory;
using Source = Core.Skills.DamageFromAttack.Source;

namespace Core.Skills.SelfDamageDealings {
	public class SelfDamageDealing : Loopable {
		private SelfDamageDealingAction config;
		private Character caster;
		private Skill skill;
		private SkillId skillId;

		private bool isFinished;
		private bool isInterrupted;

		public SelfDamageDealing(BaseEvent baseEvent, Character caster, Skill skill, SkillId skillId) {
			config = (SelfDamageDealingAction) baseEvent.action;
			this.caster = caster;
			this.skill = skill;
			this.skillId = skillId;
			isFinished = true;
			DealDamageToSelf();
		}

		public void Update(float dt) {
		}

		private void DealDamageToSelf() {
			Entity casterEntity = caster.GameObject().GetComponent<EntityReference>().Entity;
			HealthComponent hc = casterEntity.GetComponent<HealthComponent>();
			StatsComponent sc = casterEntity.GetComponent<StatsComponent>();
			DamageFromAttack damage = new DamageFromAttack(
				new SourceHistory(Source.FromSkill(skill, skillId)), config.damageScale, config.isHpPercent,
				1, 1, caster.Id(), caster.Position(), caster.Position(), sc.CharacterStats, false,
				config.ShowDeathBehavior()
			);
			damage.SetTriggerHud(config.hud);
			hc.ReceiveDamage(damage);
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return isFinished || isInterrupted;
		}
	}
}