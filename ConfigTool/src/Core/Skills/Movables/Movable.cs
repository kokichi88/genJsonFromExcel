using Core.Utils;
using MovementSystem.Components;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills.Events.Actions;

namespace Core.Skills.Movables {
	public class Movable : Loopable {
		private MovableAction movableAction;
		private Skill skill;
		private readonly Direction facingDirectionAtStart;
		private readonly UserInput userInput;
		private readonly Character caster;
		private float durationInSeconds;

		private float elapsed;
		private bool isInterrupted;
		private bool isFinished;
		private int count;

		public Movable(MovableAction movableAction, Skill skill, Direction facingDirectionAtStart,
		               UserInput userInput, Character caster) {
			this.movableAction = movableAction;
			this.skill = skill;
			this.facingDirectionAtStart = facingDirectionAtStart;
			this.userInput = userInput;
			this.caster = caster;
			durationInSeconds = FrameAndSecondsConverter._30Fps.FloatFramesToSeconds(movableAction.duration);
			if (movableAction.ShowMode() == MovableAction.Mode.Move) {
				skill.SetMovable(true);
				if (movableAction.backward) {
					bool isBackward = facingDirectionAtStart != userInput.RunDirection();
					skill.SetMoveBackward(isBackward);
				}
			}
		}

		public void Update(float dt) {
			elapsed += dt;
			if (isInterrupted || isFinished) return;

			if (movableAction.ShowMode() == MovableAction.Mode.Move) {
				if (movableAction.backward) {
					bool isBackward = facingDirectionAtStart != userInput.RunDirection();
					skill.SetMoveBackward(isBackward);
				}

				if (!isFinished && elapsed >= durationInSeconds) {
					skill.SetMovable(false);
				}
			}

			if (movableAction.ShowMode() == MovableAction.Mode.Face) {
				if (userInput.IsInputRun()) {
					if (caster.FacingDirection() != userInput.RunDirection()) {
						if (count < movableAction.count) {
							count++;
							switch (userInput.RunDirection()) {
								case Direction.Left:
									caster.SetFacingDirectionToLeft();
									break;
								case Direction.Right:
									caster.SetFacingDirectionToRight();
									break;
							}
						}
					}
				}
			}

			if (elapsed >= durationInSeconds) isFinished = true;
		}

		public void LateUpdate(float dt) {
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public bool IsFinished() {
			return isFinished || isInterrupted;
		}
	}
}