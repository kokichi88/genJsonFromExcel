using System;
using System.Collections.Generic;
using Core.Skills.Vfxs;
using MEC;
using MovementSystem.Requests;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Events.Triggers;
using UnityEngine;
using Utils;
using Event = Ssar.Combat.Skills.Events.Actions.JumpAction.Event;

namespace Core.Skills.Jumps {
    public class Jump : Loopable {
        private readonly int creatureMask = LayerMask.GetMask(
            EntityLayerName.CreatureEntity.ToString()
        );

        private Character character;
        private Skill parentSkill;
        private Environment environment;
        private bool jumpOverDistance;

        private float elapsed;
        private int jumpCount;
        private float timeOfNextJump;
        private bool isThisJumpDisabled;
        private bool isInterrupted;
        private JumpAction jumpAction;
        private Request jumpRequest;
        private bool isFinish;
        private Vfxs.Vfx.Logic vfxLogic;
        private bool isLandingEventTriggered;
        private BoxCollider characterCollider;

        public Jump(BaseEvent eventFrame, Character character, Skill parentSkill,
                    Environment environment, bool jumpOverDistance = true) {
            this.character = character;
            this.parentSkill = parentSkill;
            this.environment = environment;
            this.jumpOverDistance = jumpOverDistance;

            characterCollider = character.GameObject().GetComponent<BoxCollider>();

            BaseEvent be = eventFrame;
            jumpAction = (JumpAction) be.action;
            if (be.trigger.ShowTriggerType() == TriggerType.Frame) {
                TimelineTrigger tt = (TimelineTrigger) be.trigger;
                if (tt.ShowScaledFrameInSeconds() == 0) {
                    DoJump();
                }
            }
            else {
                DoJump();
            }
        }

        public bool JumpOverDistance {
            set { jumpOverDistance = value; }
        }

        public Request JumpRequest => jumpRequest;

        public void Update(float dt) {
            if(isInterrupted) return;
            if(IsFinish()) return;

            elapsed += dt;
            elapsed = (float) ((int)(elapsed * 1000)) / 1000f;
//            DLog.Log(elapsed);
            if (jumpCount == 0 && elapsed >= timeOfNextJump) {
                DoJump();
            }

            if (jumpRequest != null) {
                if (jumpRequest.IsCompleted() && jumpRequest.CompleteReason() == Reason.EndOfLifeCycle) {
                    if (!isLandingEventTriggered) {
                        isLandingEventTriggered = true;
                        TriggerOnGroundLandingEvents();
                    }
                }
            }

            if (jumpAction.collision) {
                if (jumpRequest != null) {
                    Collider[] collidedCreatures = Physics.OverlapBox(
                        character.Position() + characterCollider.center, characterCollider.size / 2f, Quaternion.identity, creatureMask
                    );
                    if (collidedCreatures.Length > 1) {
                        if (jumpRequest is MovingJumpRequest) {
                            ((MovingJumpRequest)jumpRequest).OverrideHorizontalSpeed(0);
                        }
                    }
                }
            }
        }

        public void LateUpdate(float dt) {
        }

        public bool IsFinished() {
            return IsFinish();
        }

        public void OnCharacterGround() {
        }

        private void TriggerOnGroundLandingEvents() {
            List<JumpAction.Event> events = jumpAction.FindEvents(
                JumpAction.Event.TriggerType.OnGroundLanding
            );
            for (int kIndex = 0; kIndex < events.Count; kIndex++) {
                JumpAction.Event e = events[kIndex];
                JumpAction.Event.ActionType at = e.ShowActionType();
                switch (at) {
                    case JumpAction.Event.ActionType.CameraFx:
                        JumpAction.CameraFxEvent cfe = (JumpAction.CameraFxEvent) e;
                        CameraAction.BaseFx bf = cfe.fx;
                        CameraAction.FxType fxType = bf.ShowFxType();
                        switch (fxType) {
                            case CameraAction.FxType.Shake:
                                CameraAction.ShakeFx sf = (CameraAction.ShakeFx) bf;
                                environment.ShakeCamera(
                                    sf.strength, sf.duration, sf.vibrato, sf.smoothness, sf.randomness,
                                    sf.useRandomInitialAngel, sf.rotation
                                );
                                break;
                            default:
                                throw new Exception("Missing logic to handle camera fx of type " + fxType);
                        }

                        break;
                    case Event.ActionType.Vfx:
                        JumpAction.VfxEvent ve = (JumpAction.VfxEvent) e;
                        VfxAction.VfxType vt = ve.fx.ShowVfxType();
                        switch (vt) {
                            case VfxAction.VfxType.SpawnPrefab:
                                vfxLogic = new Vfxs.Vfx.SpawnPrefab(
                                    5, (VfxAction.SpawnPrefabVfx) ve.fx,
                                    new DefaultVfxGameObjectFactory(),
                                    character,
                                    SkillCastingSource.FromUserInput(),
                                    environment.GetCamera(), environment, parentSkill
                                );
                                Timing.RunCoroutine(UpdateVfxLogic(vfxLogic));
                                break;
                        }
                        break;
                    case Event.ActionType.Id:
                        parentSkill.TriggerEventWithId(((JumpAction.IdEvent)e).id);
                        break;
                    default:
                        DLog.LogError("Missing logic to handle event of action type of " + at);
                        break;
                }
            }
        }

        private IEnumerator<float> UpdateVfxLogic(Vfx.Logic vfxLogic) {
            float maximumWaitTime = 15;
            float waitTime = 0;
            float deltaTime = 0.1f;
            while (true) {
                yield return Timing.WaitForSeconds(deltaTime);
                waitTime += deltaTime;
                vfxLogic.Update(deltaTime);
                if (vfxLogic.IsFinish()) {
                    break;
                }
                if (waitTime >= maximumWaitTime) {
                    break;
                }
            }
        }

        private void DoJump() {
            switch (jumpAction.ShowRequirement()) {
               case JumpAction.Requirement.Air:
                   if (character.IsOnGround()) isThisJumpDisabled = true;
                   break;
               case JumpAction.Requirement.Ground:
                   if (!character.IsOnGround()) isThisJumpDisabled = true;
                   break;
               case JumpAction.Requirement.ElevationLte:
                   Vector3 pos = character.Position();
                   Vector3 groundPos = environment.MapColliders().ClampPositionToGround(pos);
                   float elevation = pos.y - groundPos.y;
                   // DLog.Log("debug elevation " + elevation);
                   if (elevation > jumpAction.reqValue) {
                       isThisJumpDisabled = true;
                   }
                   break;
               case JumpAction.Requirement.ElevationGte:
                   pos = character.Position();
                   groundPos = environment.MapColliders().ClampPositionToGround(pos);
                   elevation = pos.y - groundPos.y;
                   // DLog.Log("debug elevation " + elevation);
                   if (elevation < jumpAction.reqValue) {
                       isThisJumpDisabled = true;
                   }
                   break;
            }
            if (isThisJumpDisabled) {
                return;
            }

//            DLog.Log(GetHashCode() + "Jump at " + elapsed);
            character.ConsumeJumpCharge(jumpAction.jumpChargeToConsume);
            jumpCount++;
            float timeToPeak = jumpAction.ShowTimeToPeak();
            float timeToGround = jumpAction.ShowTimeToGround();
            float height = jumpAction.ShowHeight();
            float distance = jumpAction.ShowDistance();
            float timeToFloat = jumpAction.ShowTimeToFloat();
            bool floatMove = jumpAction.floatingMove;
            bool preciseHeight = jumpAction.preciseHeight;
            if (jumpOverDistance) {
                jumpRequest = character.JumpOverDistance(
                    height, timeToPeak, distance, timeToGround, false, timeToFloat, false, preciseHeight, true, floatMove
                );
            }
            else {
                jumpRequest = character.Jump(
                    height, timeToPeak, distance, timeToGround, false, timeToFloat, preciseHeight
                );
            }
            float jumpDuration = timeToPeak +
                                 timeToGround;
            timeOfNextJump = elapsed + jumpDuration;
           // DLog.Log("time of next jump " + timeOfNextJump);
        }

        private bool IsFinish() {
            if (isThisJumpDisabled) return true;
            
            return isFinish || isInterrupted;
        }

        public void Interrupt() {
            isInterrupted = true;
            character.InterruptJump();
        }

        private class DefaultVfxGameObjectFactory : Vfxs.Vfx.VfxGameObjectFactory {
            public GameObject Instantiate(GameObject prefab) {
                return GameObject.Instantiate(prefab);
            }
        }
    }
}