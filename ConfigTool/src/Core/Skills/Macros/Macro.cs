#if UNITY_EDITOR
using System.Collections.Generic;
using Artemis;
using EntityComponentSystem;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Macros {
	public class Macro : Loopable {
		private readonly BaseEvent eventFrame;
		private readonly Character character;

		private DefaultUserInput userInput;
		private List<SkillId> remainingSkills = new List<SkillId>();

		public Macro(BaseEvent eventFrame, Character character) {
			this.eventFrame = eventFrame;
			this.character = character;

			foreach (string s in ((MacroAction)eventFrame.ShowAction()).skills) {
				remainingSkills.Add(new SkillId(s));
			}
			Entity entity = character.GameObject().GetComponent<EntityReference>().Entity;
			HeroStateMachineComponent hsmc = entity.GetComponent<HeroStateMachineComponent>();
			userInput = (DefaultUserInput) hsmc.UserInput;
			character.PostSkillCastEventHandler += CharacterOnPostSkillCastEventHandler;
		}

		private void CharacterOnPostSkillCastEventHandler(object sender, Character.SkillCastEventArgs e) {
			if (remainingSkills.Count > 0) {
				if (remainingSkills[0].Equals(e.skillId)) {
					remainingSkills.RemoveAt(0);
				}
			}
		}

		public void Update(float dt) {
			if (remainingSkills.Count > 0) {
				userInput.StartSkill(remainingSkills[0]);
			}
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			Unlisten();
		}

		private void Unlisten() {
			character.PostSkillCastEventHandler -= CharacterOnPostSkillCastEventHandler;
		}

		public bool IsFinished() {
			bool r = remainingSkills.Count == 0;
			if (r) {
				Unlisten();
			}

			return r;
		}
	}
}
#endif