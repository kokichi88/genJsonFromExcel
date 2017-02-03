using Artemis;
using Core.Utils;
using EntityComponentSystem;
using Ssar.Combat.Animation;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Animations {
	public class AnimationPlayback : Loopable {
		private BaseEvent ef;
		private Character caster;

		private float animClipLength;
		private float elapsed;
		private Animation anim;

		public AnimationPlayback(BaseEvent ef, Character caster) {
			this.ef = ef;
			this.caster = caster;

			AnimationAction aa = (AnimationAction) ef.action;
			bool isEligible = false;
			switch (aa.ShowRequirement()) {
				case JumpAction.Requirement.Air:
					isEligible = !caster.IsOnGround();
					break;
				case JumpAction.Requirement.Ground:
					isEligible = caster.IsOnGround();
					break;
				default:
					isEligible = true;
					break;
			}

			if (isEligible) {
				Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
				anim = entity.GetComponent<AnimationComponent>().Animation;
				anim.PlayAnimation(aa.name, aa.speed, aa.ShowPlayMethod(), aa.crossfadeLength, aa.startFrame, true);
				if (aa.speed > 0) {
					if (aa.ShowPlayMethod() == PlayMethod.Play || aa.ShowPlayMethod() == PlayMethod.ForcePlay) {
						anim.JumpToFrame(aa.startFrame);
					}
				}

				animClipLength = anim.Duration(aa.name);
			}
		}

		public void Update(float dt) {
			elapsed += dt;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
		}

		public bool IsFinished() {
			return elapsed >= animClipLength;
		}

		public void JumpToTime(float time) {
			int frame = FrameAndSecondsConverter._30Fps.SecondsToFrames(time);
			anim.JumpToFrame(frame);
		}
	}
}