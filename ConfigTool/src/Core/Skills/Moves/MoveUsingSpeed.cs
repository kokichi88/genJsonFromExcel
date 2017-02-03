using System;
using MovementSystem.Components;
using UnityEngine;

namespace Core.Skills.Moves {
    public class MoveUsingSpeed {
        private Character character;
        private float acceleration;
        private float startSpeed;
        private float maxSpeed;
        private float distance;
        private float moveDelay;
        private bool changeFacingDirection;
        private string animationName;

        private float duration;
        private float previousSpeed;
        private float traveledDistance;
        private float elapsed;
        private bool isFacingDirectionChanged;

        public MoveUsingSpeed(Character character, float acceleration, float startSpeed, float maxSpeed,
                              float distance, float moveDelay, bool changeFacingDirection, string animationName) {
            this.character = character;
            this.acceleration = acceleration;
            this.startSpeed = startSpeed;
            this.maxSpeed = maxSpeed;
            this.distance = distance;
            this.moveDelay = moveDelay;
            this.changeFacingDirection = changeFacingDirection;
            this.animationName = animationName;

            duration = distance / this.maxSpeed;
            previousSpeed = startSpeed;
        }

        public void Update(float dt) {
            if (IsFinish()) return;

            elapsed += dt;

            if (changeFacingDirection && !isFacingDirectionChanged) {
                isFacingDirectionChanged = true;
                ChangeFacingDirectionToTheOpposite();
                character.PlayAnimation(animationName);
            }

            if(elapsed < moveDelay) return;

            float currentSpeed = previousSpeed + acceleration * dt;
            currentSpeed = Math.Min(maxSpeed, currentSpeed);
            previousSpeed = currentSpeed;
            float displacement = currentSpeed * dt;
            throw new NotImplementedException();
//            character.DisplaceBy(new Vector3(displacement * character.FacingDirection(), 0, 0));
            traveledDistance += displacement;
        }

        public bool IsFinish() {
            return traveledDistance >= distance;
        }

        private void ChangeFacingDirectionToTheOpposite() {
            if ((int)character.FacingDirection() == (int) Direction.Left) {
                character.SetFacingDirectionToRight();
            }
            else {
                character.SetFacingDirectionToLeft();
            }
        }
    }
}