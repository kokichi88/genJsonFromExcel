using System.Collections.Generic;
using Artemis;
using Core.Skills.Cooldowns;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Core.Utils;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class RecastModifier : BaseModifier {
		private RecastInfo info;
		private SkillId skillId;

		private SkillCastingRequirement skillCastingRequirements;
		private Character target;
		private float cooldownRemaining;
		private RunOutOfRecastLifetime recastLifetime;

		public RecastModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                      Environment environment,
		                      CollectionOfInteractions modifierInteractionCollection,
		                      Skill skill, SkillId skillId) : base(info, casterEntity, targetEntity, environment,
			modifierInteractionCollection) {
			this.skillId = skillId;
			this.info = (RecastInfo) info;

			target = targetEntity.GetComponent<SkillComponent>().Character;
			skillCastingRequirements = target.GetSkillCastingRequirements(skillId);
			cooldownRemaining = this.info.Rmc.cooldown;
			recastLifetime.SetResources(skillCastingRequirements.Resources);
		}

		public override ModifierType Type() {
			return ModifierType.Recast;
		}

		protected override void OnUpdate(float dt) {
			cooldownRemaining -= dt;
		}

		public override bool IsBuff() {
			return true;
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			// DLog.Log("debug RecastModifier:OnDelayedAttachAsMain()");
			FrameAndSecondsConverter fasc = FrameAndSecondsConverter._30Fps;
			AcceptWindow recastWindow = new AcceptWindow(
				fasc.FloatFramesToSeconds(info.Rmc.@from), fasc.FloatFramesToSeconds(info.Rmc.to)
			);

			recastLifetime.EnableChecking();
			SetRecastWindow(recastWindow);
			SetMaxRecastCountWindow(info.Rmc.max);

			if (recastLifetime.IsEnd()) {
				target.RemoveModifier(this);
			}
		}

		public override void OnDetach(Character target) {
			base.OnDetach(target);
			SetRecastWindow(null);
		}

		public override void OnBeReplaced(Character target, Modifier byModifier) {
			base.OnBeReplaced(target, byModifier);
			SetRecastWindow(null);
		}

		public override StackResult TryStackWithNewOne(Modifier newOne) {
			bool existed = false;
			bool cooleddown = false;
			RecastModifier newRecastModifier = (RecastModifier) newOne;
			foreach (Modifier modifier in target.GetListModifiers()) {
				if (modifier.Type() == ModifierType.Recast) {
					RecastModifier other = (RecastModifier) modifier;
					if (other.skillId.Equals(newRecastModifier.skillId)) {
						existed = true;
						if (other.cooldownRemaining <= 0) {
							cooleddown = true;
						}
					}
				}
			}

			if (!existed || cooleddown && !newRecastModifier.recastLifetime.IsEnd()) {
				target.StackBuff(newOne);
			}
			return StackResult.Manual;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			List<Lifetime> lifetimes = new List<Lifetime>();
			lifetimes.AddRange(base.CreateLifetimes(modifierInfo));
			recastLifetime = new RunOutOfRecastLifetime();
			lifetimes.Add(recastLifetime);
			return lifetimes;
		}

		private void SetRecastWindow(AcceptWindow recastWindow) {
			foreach (Resource resource in skillCastingRequirements.Resources) {
				if (resource is TimeCooldownResource) {
					((TimeCooldownResource) resource).SetRecastWindow(recastWindow);
				}

				if (resource is RecoverableChargeResource) {
					((RecoverableChargeResource) resource).SetRecastWindow(recastWindow);
				}

				if (resource is NonRecoverableChargeResource) {
					((NonRecoverableChargeResource) resource).SetRecastWindow(recastWindow);
				}
			}
		}

		private void SetMaxRecastCountWindow(int max) {
			foreach (Resource resource in skillCastingRequirements.Resources) {
				if (resource is TimeCooldownResource) {
					((TimeCooldownResource) resource).SetMaxRecast(max);
				}

				if (resource is RecoverableChargeResource) {
					((RecoverableChargeResource) resource).SetMaxRecast(max);
				}

				if (resource is NonRecoverableChargeResource) {
					((NonRecoverableChargeResource) resource).SetMaxRecast(max);
				}
			}
		}

		public SkillId SKillId()
		{
			return skillId;
		}

		private class RunOutOfRecastLifetime : Lifetime {
			private Resource[] resources;

			private bool enable;

			public LifetimeType ShowType() {
				return LifetimeType.Unpredictable;
			}

			public void Update(float dt) {
			}

			public void Check() {
			}

			public bool IsEnd() {
				// DLog.Log("debug RecastModifier:IsEnd()");

				if (!enable) return false;
				foreach (Resource resource in resources) {
					if (resource is TimeCooldownResource) {
						if (((TimeCooldownResource) resource).GetCurrentRecast() < 1) return true;
					}

					if (resource is RecoverableChargeResource) {
						if (((RecoverableChargeResource) resource).GetCurrentRecast() < 1) return true;
					}

					if (resource is NonRecoverableChargeResource) {
						if (((NonRecoverableChargeResource) resource).GetCurrentRecast() < 1) return true;
					}
				}

				return false;
			}

			public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier,
			                          int damage) {
			}

			public void SetResources(Resource[] resources) {
				this.resources = resources;
			}

			public void EnableChecking() {
				// DLog.Log("debug RecastModifier:EnableChecking()");
				enable = true;
			}
		}
	}
}