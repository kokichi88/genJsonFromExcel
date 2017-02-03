using Artemis;
using Artemis.System;

namespace Core.Skills {
	public class CharacterStateTransitionEventCleanupSystem : EntityProcessingSystem {
		private static Aspect aspect = Aspect.All(typeof(CharacterStateTransitionComponent));

		public CharacterStateTransitionEventCleanupSystem() : base(aspect) {
		}

		public override void Process(Entity entity) {
			entity.GetComponent<CharacterStateTransitionComponent>().justChanged = false;
		}
	}
}