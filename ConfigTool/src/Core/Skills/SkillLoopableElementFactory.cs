using System;
using System.Collections.Generic;
using Artemis;
using Combat.DamageSystem;
using Core.Commons;
using Core.DungeonLogic.Spawn;
using Core.Skills.Animations;
using Core.Skills.Cameras;
using Core.Skills.Cooldowns;
using Core.Skills.Dashes;
using Core.Skills.DistanceTrackers;
using Core.Skills.FacingDirections;
using Core.Skills.Input;
using Core.Skills.Jumps;
using Core.Skills.LoopableAdapters;
using Core.Skills.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Skills.Movables;
using Core.Skills.PassiveSkillOnOffs;
using Core.Skills.Projectiles;
using Core.Skills.Rotations;
using Core.Skills.SelfDamageDealings;
using Core.Skills.Sounds;
using Core.Skills.SpawnCharacters;
using Core.Skills.SwitchPhases;
using Core.Skills.Teleports;
using Core.Skills.Timers;
using Core.Utils.Extensions;
using EntityComponentSystem;
using Gameplay.DungeonLogic;
using JsonConfig.Model;
using Ssar.Combat.HeroStateMachines;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using Ssar.Combat.Skills.Interactions;
using UnityEngine;
using Utils.DataStruct;
using BaseModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.BaseModifierConfig;
#if UNITY_EDITOR
using Core.Skills.Macros;
#endif

namespace Core.Skills {
	public class SkillLoopableElementFactory {
		private Environment environment;
		private ModifierInfoFactory modifierInfoFactory;
		private EntitySpawner entitySpawner;
		private readonly HeroAndMonsterConfig hamc;

		public SkillLoopableElementFactory(Environment environment,
		                                   ModifierInfoFactory modifierInfoFactory,
		                                   EntitySpawner entitySpawner, HeroAndMonsterConfig hamc) {
			this.environment = environment;
			this.modifierInfoFactory = modifierInfoFactory;
			this.entitySpawner = entitySpawner;
			this.hamc = hamc;
		}

		public Loopable Produce(Character caster, Skill skill, SkillId skillId, BaseEvent baseEvent,
		                        SkillCastingSource skillCastingSource, TemplateArgs args) {
			BaseAction ba = baseEvent.ShowAction();
			ActionType actionType = ba.ShowActionType();
			Loopable loopable = null;
			switch (actionType) {
				case ActionType.Camera:
					CameraAction ca = (CameraAction) ba;
					CameraAction.BaseFx bf = ca.fx;
					CameraAction.FxType fxType = bf.ShowFxType();
					switch (fxType) {
						case CameraAction.FxType.Shake:
							CameraAction.ShakeFx sf = (CameraAction.ShakeFx) bf;
							loopable = new CameraShake(environment, sf);
							break;
						case CameraAction.FxType.Fade:
							loopable = new CameraFade(environment, baseEvent);
							break;
						case CameraAction.FxType.CinematicZoomToSelf:
							loopable = new CameraCinematicZoomToSelf(environment, baseEvent, caster);
							break;
						case CameraAction.FxType.SlowMotion:
							loopable = new CameraSlowMotion(environment, baseEvent);
							break;
						case CameraAction.FxType.AddTarget:
							loopable = new CameraAddTarget(environment, baseEvent, caster);
							break;
						default:
							throw new Exception("Missing logic to handle camera fx of type " + fxType);
					}
					break;
				case ActionType.Dash:
					Dash dash = new Dash(baseEvent, caster, skill.IgnoreMinSpeedOnAirForDashes(), skill, environment);
					loopable = dash;
					break;
				case ActionType.Jump:
					bool jumpOverDistance = true;
					if (args != null) {
						bool found;
						jumpOverDistance = args.TryGetEntry<bool>(TemplateArgsName.JumpSkill_JumpOverDistance, out found);
						if (!found) {
							jumpOverDistance = true;
						}
					}
					Jump jump = new Jump(baseEvent, caster, skill, environment, jumpOverDistance);
					loopable = jump;
					break;
				case ActionType.Vfx:
					Vfxs.Vfx vfx = new Vfxs.Vfx(
						baseEvent, environment, caster, skillCastingSource, skill, args
					);
					loopable = vfx;
					break;
				case ActionType.Modifier:
					ModifierAction ma = (ModifierAction) ba;
					BaseModifierConfig bmc = ma.modifierConfig;
					ModifierInfo mi = modifierInfoFactory.CreateFrom(skill, bmc, environment);
					EntityReference er = caster.GameObject().GetComponent<EntityReference>();
					Modifier modifier = DamageSystem.Instance.CreateModifier(
						mi, er.Entity, er.Entity, caster.Position(),
						caster.Position(), skill, skillId, 0
					);
					if (modifier != null) {
						caster.AddModifier(modifier);
					}
					loopable = new ModifierLoopable(modifier);
					break;
				case ActionType.Animation:
					loopable = new AnimationPlayback(baseEvent, caster);
					break;
				case ActionType.Teleport:
					TeleportAction ta = (TeleportAction) baseEvent.action;
					TeleportAction.ModeName mode = ta.mode.ShowModeName();
					switch (mode) {
						case TeleportAction.ModeName.PredefinedPositionOnMap:
							new Teleport(baseEvent, caster, environment);
							loopable = new ImmediatelyFinishedLoopable();
							break;
						case TeleportAction.ModeName.KeepDistance:
							TeleportAction.KeepDistanceMode kdm = (TeleportAction.KeepDistanceMode) ta.mode;
							loopable = new TeleportKeepDistanceLogic(kdm, skill, environment, caster);
							break;
						case TeleportAction.ModeName.AroundTarget:
							TeleportAction.AroundTargetMode atm = (TeleportAction.AroundTargetMode) ta.mode;
							loopable = new TeleportAroundTargetLogic(atm, caster, environment, skill);
							break;
						case TeleportAction.ModeName.AroundTeamMate:
							TeleportAction.AroundTeamMateMode atmm = (TeleportAction.AroundTeamMateMode) ta.mode;
							loopable = new TeleportAroundTeamMate(atmm, caster, environment, skill);
							break;
						default:
							throw new Exception("Cannot create teleport of type " + mode);
					}
					break;
				case ActionType.FacingDirection:
					loopable = new FacingDirection(baseEvent, caster, environment);
					break;
				case ActionType.DashTowardTarget:
					DashTowardTarget dtt = new DashTowardTarget(baseEvent, caster, environment);
					loopable = dtt;
					break;
				case ActionType.JumpTowardTarget:
					loopable = new JumpTowardTarget(baseEvent, caster, environment);
					break;
				case ActionType.SpawnCharacter:
					SpawnCharacterAction sca = (SpawnCharacterAction) ba;
					loopable = new SpawnCharacter(sca, entitySpawner, caster, args, environment, skillId, hamc);
					break;
				case ActionType.Rotation:
					loopable = new Rotation(baseEvent, caster, environment);
					break;
				case ActionType.Timer:
					loopable = new Timer(baseEvent, skill);
					break;
				case ActionType.Sound:
					loopable = new AudioClipPlayback(baseEvent, environment);
					break;
				case ActionType.PassiveSkillOnOff:
					loopable = new PassiveSkillOnOff(baseEvent, caster);
					break;
				case ActionType.DistanceTracker:
					loopable = new DistanceTracker(baseEvent, skill, caster);
					break;
				case ActionType.SelfDamageDealing:
					loopable = new SelfDamageDealing(baseEvent, caster, skill, skillId);
					break;
				case ActionType.SwitchPhase:
					loopable = new SwitchPhase(skill);
					break;
				case ActionType.Movable:
					Entity casterEntity = caster.GameObject().GetComponent<EntityReference>().Entity;
					UserInput userInput = casterEntity.GetComponent<HeroStateMachineComponent>().UserInput;
					MovableAction movableAction = (MovableAction) ba;
					loopable = new Movable(movableAction, skill, caster.FacingDirection(), userInput, caster);
					break;
				case ActionType.Input:
					casterEntity = caster.GameObject().GetComponent<EntityReference>().Entity;
					userInput = casterEntity.GetComponent<HeroStateMachineComponent>().UserInput;
					InputAction ia = (InputAction) ba;
					loopable = new InputSimulation(ia, (DefaultUserInput) userInput);
					break;
#if UNITY_EDITOR
				case ActionType.Macro:
					loopable = new Macro(baseEvent, caster);
					break;
#endif
				default:
					DLog.Log("Missing logic to handle action of type " + actionType);
					break;
			}

			return loopable;
		}
	}
}