using System;
using System.Collections.Generic;
using Artemis;
using Combat.Stats;
using Core.Skills.Modifiers;
using Core.Skills.Modifiers.Info;
using MovementSystem;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Projectiles.Entity.Components;
using UnityEngine;
using OnHitPhase = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.OnHitPhase;
using DeathBehavior = Ssar.Combat.Skills.Events.Actions.CastProjectileAction.DeathBehavior;

namespace Core.Skills {
	public class DamageFromAttack {
		private readonly SourceHistory sourceHistory;
		private float damageScale;
		private bool isHpPercent;
		private float weight;
		private readonly float skillPowerScale;
		private readonly int casterId;
		private List<ModifierInfo> modifierInfos = new List<ModifierInfo>();
		private Dictionary<OnHitPhase, List<ModifierInfo>> modifierInfoByOnHitPhase = new Dictionary<OnHitPhase, List<ModifierInfo>>();
		private bool forceAddModifier;
		private Vector2 projectilePosition;
		private Vector2 impactPosition;
		private BakedStatsContainer characterStats;
		private DeathBehavior deathBehavior;
		private readonly bool requireCasterToBeAlive;
		private readonly bool isHpPercentBaseOnMaxHp;
		private bool causeTargetToDie = true;
		private bool triggerHud = false;

		public DamageFromAttack(SourceHistory sourceHistory, float damageScale, bool isHpPercent, float weight, float skillPowerScale,
		                        int casterId, Vector2 projectilePosition,
		                        Vector2 impactPosition, BakedStatsContainer characterStats,
		                        bool forceAddModifier = false,
		                        DeathBehavior deathBehavior = CastProjectileAction.DeathBehavior.Neutral,
		                        bool requireCasterToBeAlive = true, bool isHpPercentBaseOnMaxHp = false) {
			if(characterStats == null) throw new Exception("null character stats");
			this.sourceHistory = sourceHistory;
			this.damageScale = damageScale;
			this.isHpPercent = isHpPercent;
			this.weight = weight;
			this.skillPowerScale = skillPowerScale;
			this.casterId = casterId;
			this.projectilePosition = projectilePosition;
			this.impactPosition = impactPosition;
			this.characterStats = characterStats;
			this.forceAddModifier = forceAddModifier;
			this.deathBehavior = deathBehavior;
			this.requireCasterToBeAlive = requireCasterToBeAlive;
			this.isHpPercentBaseOnMaxHp = isHpPercentBaseOnMaxHp;
		}

		public void AddModifierInfo(OnHitPhase phase, ModifierInfo mi) {
			if (mi == null) return;

			if (!modifierInfoByOnHitPhase.ContainsKey(phase)) {
				modifierInfoByOnHitPhase[phase] = new List<ModifierInfo>();
			}
			modifierInfoByOnHitPhase[phase].Add(mi);
		}

		public float DamageScale {
			get { return damageScale; }
		}

		public bool IsHpPercent {
			get { return isHpPercent; }
		}

		public bool IsHpPercentBaseOnMaxHp => isHpPercentBaseOnMaxHp;

		public float Weight {
			get { return weight; }
		}

		public float SkillPowerScale => skillPowerScale;

		public int CasterId {
			get { return casterId; }
		}

		public Dictionary<OnHitPhase, List<ModifierInfo>> ModifierInfoByOnHitPhase {
			get { return modifierInfoByOnHitPhase; }
		}

		public bool IsModifierForcedToAdd {
			get { return forceAddModifier; }
		}

		public Vector2 ProjectilePosition {
			get { return projectilePosition; }
		}

		public Vector2 ImpactPosition {
			get { return impactPosition; }
		}

		public DeathBehavior DeathBehavior {
			get { return deathBehavior; }
		}

		public BakedStatsContainer CharacterStats {
			get => characterStats;
		}

		public bool RequireCasterToBeAlive => requireCasterToBeAlive;

		public SourceHistory SourceHistory_ => sourceHistory;

		public DamageFromAttack CauseTargetToDie(bool value) {
			causeTargetToDie = value;
			return this;
		}

		public bool CauseTargetToDie_ => causeTargetToDie;

		public class SourceHistory {
			public List<Source> sources = new List<Source>();

			public SourceHistory(Source origin) {
				sources.Add(origin);
			}

			public SourceHistory Add(Source src) {
				sources.Add(src);
				return this;
			}

			public bool Contains(SourceType sourceType) {
				foreach (Source src in sources) {
					if (src.type == sourceType) return true;
				}

				return false;
			}

			public Source ShowOrigin() {
				return sources[0];
			}

			public Source Find(SourceType sourceType) {
				foreach (Source sauce in sources) {
					if (sauce.type == sourceType) return sauce;
				}

				return null;
			}
		}

		public DamageFromAttack SetTriggerHud(bool value) {
			triggerHud = value;
			return this;
		}

		public class Source {
			public SourceType type;
			public object value;

			public static Source FromSkill(Skill skill, SkillId skillId) {
				return new SourceFromSkill() {
					type = SourceType.Skill,
					value = skill,
					skillId = skillId
				};
			}

			public static Source FromSkill(Skill skill, SkillId skillId, ProjectileComponent projectile) {
				return new SourceFromSkill() {
					type = SourceType.Skill,
					value = skill,
					skillId = skillId,
					projectile = projectile
				};
			}

			public static Source FromModifier(Modifier modifier) {
				return new Source() {
					type = SourceType.Modifier,
					value = modifier
				};
			}

			public static Source FromDieSystem() {
				return new Source() {
					type = SourceType.DieSystem
				};
			}
		}

		public class SourceFromSkill : Source {
			public SkillId skillId;
			public ProjectileComponent projectile;

			public Skill ShowSkill() {
				return (Skill) value;
			}
		}

		public enum SourceType {
			Skill,
			Modifier,
			DieSystem
		}
	}
}