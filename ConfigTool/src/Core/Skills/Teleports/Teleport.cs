using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commons;
using MovementSystem.Components;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
//using SSAR.BattleSystem.Utils;
using UnityEngine;

namespace Core.Skills.Teleports {
    public class Teleport {
        private BaseEvent ef;
        private Character caster;
        private Environment environment;

        private float elapsed;
        private bool notificationPlayed;
        private bool targetTracked;
        private Vector3 trackedPosition;

        public Teleport(BaseEvent ef, Character caster, Environment environment) {
            this.ef = ef;
            this.caster = caster;
            this.environment = environment;

            TeleportAction ta = (TeleportAction) ef.action;
            TeleportAction.ModeName mode = ta.mode.ShowModeName();
            switch (mode) {
                case TeleportAction.ModeName.PredefinedPositionOnMap:
                    TeleportAction.PredefinedPositionOnMapMode ppomm = (TeleportAction.PredefinedPositionOnMapMode) ta.mode;
                    Vector2 pos = environment.GetPositionOnMap(ppomm.positionName);
                    Vector2 displacement = pos - (Vector2) caster.Position();
                    caster.DisplaceBy(displacement);
                    break;
                case TeleportAction.ModeName.KeepDistance:
                    TeleportAction.KeepDistanceMode kdm = (TeleportAction.KeepDistanceMode) ta.mode;
                    List<Character> enemies = environment.FindNearbyCharacters(
                        caster, Vector3.zero, 999,
                        new[] {
                            FindingFilter.ExcludeAllies, FindingFilter.ExcludeDead, FindingFilter.ExcludeMe
                        }
                    );
                    if (enemies.Count > 0) {
                        Character enemy = enemies[0];
                        CharacterId enemyCharId = enemy.CharacterId();
                        CharacterId charToControl = environment.CharacterToControl();
                        CharacterId casterCharId = caster.CharacterId();

                        if (casterCharId != charToControl) {
                            float from = kdm.distanceRange[0];
                            float to = kdm.distanceRange[1];
                            float distanceBetween = Math.Abs(enemy.Position().x - caster.Position().x);
                            float distance = distanceBetween;
                            if (distanceBetween <= from) {
                                distance = from;
                            }else if (distanceBetween >= to) {
                                distance = to;
                            }

                            Vector2 targetPos = (Vector2) enemy.Position() +
                                                enemy.FacingDirection().ToNormalizedVector2() * distance;
                            caster.DisplaceBy(targetPos - (Vector2) caster.Position());
                        }
                    }
                    break;
            }
        }

        public void Update(float dt) {
            elapsed += dt;
        }
    }
}