using System.Collections.Generic;
using Core.Skills.LoopableAdapters;
using Utils.DataStruct;

namespace Core.Skills.Modifiers {
	public class ModifierAndTargetLoopable : ImmediatelyFinishedLoopable {
		private List<SsarTuple<Character, Modifier>> characterAndModifier = new List<SsarTuple<Character, Modifier>>();

		public ModifierAndTargetLoopable(List<SsarTuple<Character, Modifier>> characterAndModifier) {
			this.characterAndModifier = characterAndModifier;
		}

		public List<SsarTuple<Character, Modifier>> CharacterAndModifier => characterAndModifier;
	}
}