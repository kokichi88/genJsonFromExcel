using System.Collections.Generic;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;

namespace Core.Skills.Modifiers {
	public class DashModifier : StaggerModifier {
		private DashInfo info;

		public DashModifier(DashInfo info, Entity casterEntity, Entity targetEntity,
		                    Vector3 collidedProjectilePosition, Environment environment,
		                    CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, collidedProjectilePosition, environment, modifierInteractionCollection) {
			this.info = info;
		}

		public override ModifierType Type() {
			return ModifierType.Dash;
		}

		protected override bool ShouldInterruptTargetSkill() {
			return false;
		}

		protected override bool ShouldPlayAnimation() {
			return info.EnableAnim;
		}
	}
}