using Core.Skills.LoopableAdapters;

namespace Core.Skills.Modifiers {
	public class ModifierLoopable : ImmediatelyFinishedLoopable {
		private Modifier modifier;

		public ModifierLoopable(Modifier modifier) {
			this.modifier = modifier;
		}

		public Modifier Modifier => modifier;
	}
}