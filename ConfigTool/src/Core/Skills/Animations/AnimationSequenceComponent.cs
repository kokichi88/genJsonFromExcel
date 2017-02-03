using System.Collections.Generic;
using Artemis.Interface;

namespace Core.Skills.Animations {
	public class AnimationSequenceComponent : IComponent {
		public Dictionary<SkillId, State> stateBySkillId = new Dictionary<SkillId, State>();

		public State GetStateBySkillId(SkillId sid) {
			State state;
			if (!stateBySkillId.TryGetValue(sid, out state)) {
				state = new State();
				stateBySkillId[sid] = state;
			}

			return state;
		}

		public class State {
			public int index = 0;
			public float elapsed = 0;
			public float duration = 5;
		}
	}
}