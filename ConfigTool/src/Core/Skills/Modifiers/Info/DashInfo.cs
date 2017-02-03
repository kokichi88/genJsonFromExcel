using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;
using Combat.Stats;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using MovementBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.MovementBehavior;
using FacingBehavior = Combat.Skills.ModifierConfigs.Modifiers.StaggerModifierConfig.FacingBehavior;

namespace Core.Skills.Modifiers.Info {
	public class DashInfo : StaggerInfo{
		private readonly bool enableAnim;

		public DashInfo(Target target, Skill parentSkill, float distance, float movementDuration,
		                float successRate, float delayToApply, WeightLevel level, Behavior[] behaviors,
		                string overrideAnimation, List<VfxConfig> vfxs,
		                MovementBehavior movementBehavior, FacingBehavior facingBehavior,
		                bool enableAnim, List<LifetimeConfig> lifetimeConfigs) : base(target, parentSkill, distance, movementDuration, successRate, delayToApply, level, behaviors, overrideAnimation, vfxs, movementBehavior, facingBehavior, StaggerModifierConfig.Requirement.Any, lifetimeConfigs) {
			this.enableAnim = enableAnim;
		}

		public override ModifierType ShowType() {
			return ModifierType.Dash;
		}

		public bool EnableAnim => enableAnim;
	}
}