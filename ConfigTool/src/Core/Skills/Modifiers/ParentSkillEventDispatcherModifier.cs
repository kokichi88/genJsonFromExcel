using System.Collections.Generic;
using Artemis;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills.Interactions;

namespace Core.Skills.Modifiers {
	public class ParentSkillEventDispatcherModifier : BaseModifier {
		private readonly ParentSkillEventDispatcherInfo info;

		public ParentSkillEventDispatcherModifier(ModifierInfo info, Entity casterEntity,
		                                          Entity targetEntity, Environment environment,
		                                          CollectionOfInteractions modifierInteractionCollection) : base(info, casterEntity, targetEntity, environment, modifierInteractionCollection) {
			this.info = (ParentSkillEventDispatcherInfo) info;
		}

		public override ModifierType Type() {
			return ModifierType.ParentSkillEventDispatcher;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			return new List<Lifetime>(new []{new DurationBasedLifetime(.1f), });
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			info.ShowParentSkill().TriggerEventWithId(info.Psedmc.eventId);
		}
	}
}