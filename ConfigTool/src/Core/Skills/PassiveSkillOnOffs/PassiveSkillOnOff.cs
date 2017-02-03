using System.Collections.Generic;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.PassiveSkillOnOffs {
	public class PassiveSkillOnOff : Loopable {
		private BaseEvent passiveEvent;
		private Character caster;

		public PassiveSkillOnOff(BaseEvent passiveEvent, Character caster) {
			this.passiveEvent = passiveEvent;
			this.caster = caster;

			PassiveSkillOnOffAction act = (PassiveSkillOnOffAction) passiveEvent.ShowAction();
			List<SkillId> ongoingSkills = this.caster.OngoingSkills();
			foreach (SkillId sid in ongoingSkills) {
				if (sid.StringValue.Equals(act.id)) {
					if (!act.state) {
						caster.FindOngoingSkill(sid).Deactivate();
					}
					else {
						caster.FindOngoingSkill(sid).Activate();
					}
				}
			}
		}

		public void Update(float dt) {
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return true;
		}
	}
}