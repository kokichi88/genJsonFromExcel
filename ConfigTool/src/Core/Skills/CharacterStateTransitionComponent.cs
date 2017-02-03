using Artemis.Interface;

namespace Core.Skills {
	public class CharacterStateTransitionComponent : IComponent {
		public Character.CharacterState from = Character.CharacterState.Default;
		public Character.CharacterState to = Character.CharacterState._;
		public bool justChanged = false;
	}
}