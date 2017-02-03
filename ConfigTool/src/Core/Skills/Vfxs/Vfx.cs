using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Assets.Scripts.Utils;
using Combat.DamageSystem;
using Core.Skills.Modifiers;
using Core.Utils;
using Core.Utils.Extensions;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using Equipment;
using MEC;
using MovementSystem.Components;
using Ssar.Combat.CustomizeVisual;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;
using Utils.DataStruct;
using Xft;

namespace Core.Skills.Vfxs {
	public class Vfx : Loopable {
		private BaseEvent ef;
		private Environment environment;
		private Character caster;
		private SkillCastingSource scs;
		private readonly Skill parentSkill;

		private VfxAction vfxAction;
		private bool isInterrupted;
		private Logic logic;
		private bool isConditionMet;
		private CoroutineHandle independentUpdateCoroutineHandle;
		private CoroutineHandle independentLateUpdateCoroutineHandle;

		public Vfx(BaseEvent ef, Environment environment, Character caster,
		           SkillCastingSource scs, Skill parentSkill, TemplateArgs args) {
			this.ef = ef;
			this.environment = environment;
			this.caster = caster;
			this.scs = scs;
			this.parentSkill = parentSkill;

			isConditionMet = CheckConditions();
			if (!isConditionMet) return;

			vfxAction = (VfxAction) ef.action;
			switch (vfxAction.baseVfx.ShowVfxType()) {
				case VfxAction.VfxType.SpawnPrefab:
					logic = new SpawnPrefab(
						vfxAction.ShowTimeToLiveInSeconds(), (VfxAction.SpawnPrefabVfx) vfxAction.baseVfx,
						new DefaultVfxGameObjectFactory(environment), caster, scs, environment.GetCamera(),
						environment, parentSkill, args
					);
					break;
				case VfxAction.VfxType.ChangeMaterialColor:
					logic = new ChangeMaterialColor(
						(VfxAction.ChangeMaterialColorVfx) vfxAction.baseVfx,
						vfxAction.timeToLiveInSeconds, environment, caster
					);
					break;
				case VfxAction.VfxType.ChangeMaterial:
					logic = new ChangeMaterial(
						(VfxAction.ChangeMaterialVfx) vfxAction.baseVfx,
						vfxAction.timeToLiveInSeconds, environment, caster
					);
					break;
				case VfxAction.VfxType.DashShadow:
					logic = new DashShadowVfxLogic((VfxAction.DashShadowVfx) vfxAction.baseVfx, environment, caster);
					break;
				case VfxAction.VfxType.ChangeWeaponMaterial:
					logic = new ChangeWeaponMaterialLogic(
						(VfxAction.ChangeWeaponMaterialVfx) vfxAction.baseVfx, vfxAction.timeToLiveInSeconds, caster, environment
					);
					break;
				case VfxAction.VfxType.AddMaterial:
					logic = new AddMaterialLogic((VfxAction.AddMaterialVfx) vfxAction.baseVfx, vfxAction.timeToLiveInSeconds, caster);
					break;
				case VfxAction.VfxType.SpawnLightningPrefab:
					logic = new SpawnLightningPrefabLogic((VfxAction.SpawnLightningPrefabVfx) vfxAction.baseVfx, caster, vfxAction.ShowTimeToLiveInSeconds());
					break;
				default:
					throw new Exception("Missing logic to create vfx of type " + vfxAction.baseVfx.ShowVfxType());
			}

			if (!vfxAction.lifecycleDependOnParentSkill) {
				independentUpdateCoroutineHandle = Timing.RunCoroutine(IndependentUpdate(), Segment.Update);
				independentLateUpdateCoroutineHandle = Timing.RunCoroutine(IndependentLateUpdate(), Segment.LateUpdate);
			}
		}

		private bool CheckConditions() {
			bool met = true;
			TriggerConditions conditions = ((VfxAction) ef.ShowAction()).triggerConditions;
			foreach (Condition condition in conditions.conditions) {
				switch (condition.ShowName()) {
					case ConditionName.CharacterState:
						CharacterStateCondition csd = (CharacterStateCondition) condition;
						if (csd.ShowOperator() == Operator.Is) {
							met &= caster.State() == csd.ShowState();
						}

						if (csd.ShowOperator() == Operator.Not) {
							met &= caster.State() != csd.ShowState();
						}
						break;
					default:
						throw new Exception("Missing logic to handle condition of name " + condition.ShowName());
				}
			}

			return met;
		}

		public void SetVfxPosition(Vector2 pos, Quaternion orientation) {
			if (logic is SpawnPrefab) {
				((SpawnPrefab) logic).SetVfxPosition(pos, orientation);
			}
		}

		public Logic ShowLogic() {
			return logic;
		}

		public void Update(float dt) {
			if (!vfxAction.lifecycleDependOnParentSkill) return;
			Update_(dt);
		}

		private void Update_(float dt) {
			if (IsFinish()) {
				return;
			}

			logic.Update(dt);
		}

		private IEnumerator<float> IndependentUpdate() {
			while (true) {
				yield return Timing.WaitForOneFrame;
				Update_(Time.deltaTime);
			}
		}

		public void LateUpdate(float dt) {
			if (vfxAction == null) return;
			if (!vfxAction.lifecycleDependOnParentSkill) return;
			LateUpdate_(dt);
		}

		private void LateUpdate_(float dt) {
			logic.LateUpdate(dt);

			if (IsFinish()) {
				Interrupt();
				logic.DestroyVfx();
				if (independentUpdateCoroutineHandle != null) {
					Timing.KillCoroutines(independentUpdateCoroutineHandle);
				}

				if (independentLateUpdateCoroutineHandle != null) {
					Timing.KillCoroutines(independentLateUpdateCoroutineHandle);
				}
			}
		}

		private IEnumerator<float> IndependentLateUpdate() {
			while (true) {
				yield return Timing.WaitForOneFrame;
				LateUpdate_(Time.deltaTime);
			}
		}

		public bool IsFinished() {
			return IsFinish();
		}

		public bool IsFinish() {
			if (isConditionMet) {
				return logic.IsFinish();
			}
			else {
				return true;
			}
		}

		public void Interrupt() {
			if (vfxAction.lifecycleDependOnParentSkill) {
				logic.Interrupt();
				logic.DestroyVfx();
			}
		}

		public void PauseForLockFrame() {
			if (logic is SpawnPrefab) {
				((SpawnPrefab) logic).PauseForLockFrame();
			}
		}

		public void UnpauseForLockFrame() {
			if (logic is SpawnPrefab) {
				((SpawnPrefab) logic).UnpauseForLockFrame();
			}
		}

		public interface Logic {
			void Update(float dt);
			void DestroyVfx();
			bool IsFinish();
			void Interrupt();
			void LateUpdate(float dt);
			void IncreaseTimeToLiveBy(float seconds);
		}

		public interface VfxGameObjectFactory {
			GameObject Instantiate(GameObject prefab);
		}

		public class DefaultVfxGameObjectFactory : VfxGameObjectFactory {
			private Environment environment;

			public DefaultVfxGameObjectFactory(Environment environment) {
				this.environment = environment;
			}

			public GameObject Instantiate(GameObject prefab) {
				return environment.InstantiateGameObject(prefab);
			}
		}

		public class SpawnPrefab : Logic {
			private float timeToLive;
			private readonly VfxGameObjectFactory vfxGameObjectFactory;
			private Character caster;
			private readonly SkillCastingSource skillCastingSource;
			private readonly Camera camera;
			private readonly Environment environment;
			private readonly Skill parentSkill;

			private VfxAction.AttachType attachType;
			private bool isInterrupted;
			private Transform joint;
			private VfxAction.JointAttachment jointAttachment;
			private Transform vfx;
			private bool isVfxDestroyed;
			private VfxAction.RelativePositionAttachment relativePosAttachment;
			private float elapsed;
			private Direction desiredFlipDirection = Direction.Right;
			private XWeaponTrail trail;
			private float trailFadeoutAt;
			private Animator animator;
			private AnimatorStateInfo animatorStateInfo;
			private bool isFadingOut;
			private Transform cameraTransform;
			private VfxAction.CameraAttachment cameraAttachment;
			private VfxAction.UpdateType updateType;
			private VfxAction.RelativeXClampYAttachment relativeXClampYAttachment;
			private ParticleSystem[] particleSystems;
			private float animatorSpeed;
			private SkillId skillId;
			private float jointAttachmentElapsed;
			private TimeScaleComponent timeScaleComponent;
			private XWeaponTrail[] trails;
			private List<Transform> children = new List<Transform>();
			private List<Transform> targets = new List<Transform>();
			private HealthComponent healthComponent;

			public SpawnPrefab(float timeToLive, VfxAction.SpawnPrefabVfx vfxConfig,
			                   VfxGameObjectFactory vfxGameObjectFactory, Character caster,
			                   SkillCastingSource skillCastingSource, Camera camera,
			                   Environment environment, Skill parentSkill,
			                   TemplateArgs args = null) {
				this.timeToLive = timeToLive;
				this.vfxGameObjectFactory = vfxGameObjectFactory;
				this.caster = caster;
				this.skillCastingSource = skillCastingSource;
				this.camera = camera;
				this.environment = environment;
				this.parentSkill = parentSkill;
				caster.SkillId(parentSkill, ref skillId);

				bool shouldPlayVfx = false;
				switch (vfxConfig.ShowCharacterState()) {
					case VfxAction.SpawnPrefabVfx.State.Air:
						if (!caster.IsOnGround()) {
							shouldPlayVfx = true;
						}
						break;
					case VfxAction.SpawnPrefabVfx.State.Ground:
						if (caster.IsOnGround()) {
							shouldPlayVfx = true;
						}
						break;
					case VfxAction.SpawnPrefabVfx.State.Any:
						shouldPlayVfx = true;
						break;
				}

				if (!shouldPlayVfx) return;

				VfxAction.SpawnPrefabVfx spawnPrefabVfx = vfxConfig;
				Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;

				GameObject prefab = null;
				if(spawnPrefabVfx.proxyMode){
					switch (spawnPrefabVfx.ShowProxySource()) {
						case VfxAction.ProxySource.WeaponMaterial:
							CustomizeVisualComponent cvc = entity.GetComponent<CustomizeVisualComponent>();
							if (cvc == null) {
								prefab = environment.FindDefaultPrefabById(
									caster.CharacterId(), spawnPrefabVfx.proxyId
								);
							}
							else {
								EquipmentCollectData equipmentCollectData = cvc.EquipmentCollectData;
								if (equipmentCollectData == null) {
									prefab = environment.FindDefaultPrefabById(
										caster.CharacterId(), spawnPrefabVfx.proxyId
									);
								}
								else {
									prefab = environment.FindPrefabById(
										caster.CharacterId(), equipmentCollectData.EquipmentConfigId.VisualId,
										spawnPrefabVfx.proxyId
									);
								}
							}
							break;
						case VfxAction.ProxySource.ImpactProxy:
							List<Modifier> ongoingModifiers = caster.GetListModifiers();
							int[] typesOfOngoingModifiers = new int[ongoingModifiers.Count];
							for (int kIndex = 0; kIndex < ongoingModifiers.Count; kIndex++) {
								typesOfOngoingModifiers[kIndex] = (int) ongoingModifiers[kIndex].Type();
							}
							cvc = entity.GetComponent<CustomizeVisualComponent>();
							int[] weaponVisualIds = null;
							if (cvc == null) {
								weaponVisualIds = new int[0];
							}
							else {
								EquipmentCollectData equipmentCollectData = cvc.EquipmentCollectData;
								weaponVisualIds	= equipmentCollectData == null
									? new int[0]
									: new int[] {equipmentCollectData.EquipmentConfigId.VisualId};
							}
							prefab = environment.FindVfxProxyPrefabById(
								caster.CharacterId(), spawnPrefabVfx.proxyId, typesOfOngoingModifiers, weaponVisualIds
							);
							break;
					}
				}
				else {
					prefab = spawnPrefabVfx.ShowVfxPrefab();
				}

				vfx = vfxGameObjectFactory.Instantiate(prefab).transform;
				animator = vfx.GetComponent<Animator>();
				trail = vfx.GetComponentInChildren<XWeaponTrail>();
				particleSystems = vfx.GetComponentsInChildren<ParticleSystem>();
				if (trail) {
					trailFadeoutAt = timeToLive - trail.fadeoutDuration;
				}
				attachType = spawnPrefabVfx.attachment.ShowAttachType();
				switch (skillCastingSource.Src) {
					case SkillCastingSource.Source.UserInput:
					case SkillCastingSource.Source.EnemyDeath:
					case SkillCastingSource.Source.EntityCreation:
						desiredFlipDirection = caster.FacingDirection();
						break;
					default:
						desiredFlipDirection = caster.FacingDirection();
						break;
				}
				switch (attachType) {
					case VfxAction.AttachType.Joint:
						jointAttachment = (VfxAction.JointAttachment) spawnPrefabVfx.attachment;
						Transform casterTransform = caster.GameObject().transform;
						joint = casterTransform.FindDeepChild(jointAttachment.jointName);
						if (joint == null) {
							DLog.LogError(string.Format("Cannot find joint named '{0}'", jointAttachment.jointName));
						}
						else {
							vfx.position = joint.TransformPoint(jointAttachment.offset);
						}

						if (jointAttachment.flip) {
							Flip();
						}

						foreach (VfxAction.ChildAttachment ca in jointAttachment.children) {
							Transform child = vfx.FindDeepChild(ca.child);
							children.Add(child);
							Transform target = casterTransform.FindDeepChild(ca.target);
							targets.Add(target);

							if (child && target) {
								child.position = target.position;
							}
						}

						updateType = jointAttachment.ShowUpdateType();
						break;
					case VfxAction.AttachType.RelativePosition:
						Flip();
						relativePosAttachment = (VfxAction.RelativePositionAttachment) spawnPrefabVfx.attachment;
						Vector3 relativePos = relativePosAttachment.p;
						Vector3 pivot = caster.Position();
						if (args != null && args.Contains(TemplateArgsName.Position)) {
							pivot = args.GetEntry<Vector2>(TemplateArgsName.Position);
						}
						vfx.position = pivot + relativePos.FlipFollowDirection(desiredFlipDirection);
						if (relativePosAttachment.sEnable) {
							vfx.localScale = relativePosAttachment.s;
						}
						break;
					case VfxAction.AttachType.ImpactPosition:
						Flip();
						if (skillCastingSource.Src == SkillCastingSource.Source.KilledByEnemy) {
							Vector2 projectilePosition = ((List<Vector2>) skillCastingSource.Param[1])[0];
							Vector2 impactPosition = ((List<Vector2>) skillCastingSource.Param[2])[0];
							vfx.position = impactPosition;
						}
						break;
					case VfxAction.AttachType.PresetPosition:
						Flip();
						vfx.position = ((VfxAction.PresetPositionAttachment) spawnPrefabVfx.attachment).position;
						break;
					case VfxAction.AttachType.Camera:
						cameraAttachment = (VfxAction.CameraAttachment) spawnPrefabVfx.attachment;
						cameraTransform = camera.transform;
						vfx.position = cameraTransform.position + cameraAttachment.relativePosition.FlipFollowDirection(desiredFlipDirection);
						break;
					case VfxAction.AttachType.RelativeXClampY:
						Flip();
						relativeXClampYAttachment = (VfxAction.RelativeXClampYAttachment) spawnPrefabVfx.attachment;
						relativePos = new Vector2(relativeXClampYAttachment.offset.x, 0)
							.FlipFollowDirection(caster.FacingDirection());
						Vector3 pos = caster.Position() + (Vector3) relativePos;
						pos = environment.MapColliders().ClampPositionToGround(pos)
						      + new Vector3(0, relativeXClampYAttachment.offset.y, 0);
						vfx.position = pos;
						break;
					case VfxAction.AttachType.RelativePosAtSkillStart:
						Flip();
						VfxAction.RelativePositionAtSkillStartAttachment rel = (VfxAction.RelativePositionAtSkillStartAttachment) spawnPrefabVfx.attachment;
						if (rel.facing) {
							desiredFlipDirection = parentSkill.CharacterFacingDirectionAtSkillStart
								.ToLeftOrRightDirectionEnum();
						}
						relativePos = rel.relativePosition.FlipFollowDirection(desiredFlipDirection);
						vfx.position = parentSkill.CharacterPositionAtSkillStart + (Vector3) relativePos;
						break;
				}

				healthComponent = entity.GetComponent<HealthComponent>();
				timeScaleComponent = entity.GetComponent<TimeScaleComponent>();
				trails = vfx.GetComponentsInChildren<XWeaponTrail>();
				if (trails != null && trails.Length > 0) {
					Timing.RunCoroutine(UpdateTrailWhenTimeScaleIsPaused(), Segment.LateUpdate);
				}
			}

			private IEnumerator<float> UpdateTrailWhenTimeScaleIsPaused() {
				float elapsed = 0;
				while (true) {
					yield return Timing.WaitForOneFrame;
					if (trails[0] == null || trails[0].transform == null) {
						// DLog.Log("debug break timeScale checking due to component deletion");
						break;
					}

					if (timeScaleComponent.IsPaused) {
						LateUpdate(0);
						foreach (XWeaponTrail weaponTrail in trails) {
							// DLog.Log("debug pause trail");
							if (!weaponTrail.paused) {
								weaponTrail.UpdateForLockFrame();
							}
							weaponTrail.paused = true;
						}
					}
					else {
						elapsed += Time.deltaTime;
						LateUpdate(0);
						foreach (XWeaponTrail weaponTrail in trails) {
							// DLog.Log("debug un-pause trail");
							if (weaponTrail.paused) {
								weaponTrail.UpdateForLockFrame();
							}
							weaponTrail.paused = false;
						}
					}
					// DLog.Log("debug late update");

					if (elapsed >= timeToLive) {
						// DLog.Log("debug break timeScale checking due to expiration");
						break;
					}
				}
			}

			public void SetVfxPosition(Vector2 pos, Quaternion orientation) {
				vfx.position = pos;
				vfx.rotation = orientation;
			}

			private void Flip() {
				ParticleRotation particleRotation = vfx.GetComponent<ParticleRotation>();
				if (particleRotation) {
					if (desiredFlipDirection == Direction.Left) {
						particleRotation.Flip();
					}
				}
			}

			public void Update(float dt) {
				elapsed += dt;
				if (vfx == null) return;
				switch (attachType) {
					case VfxAction.AttachType.Joint:
						if (healthComponent.IsDead()) return;
						jointAttachmentElapsed += dt;
						if (jointAttachment.duration >= 0 && jointAttachmentElapsed >= jointAttachment.duration) break;
						if (updateType != VfxAction.UpdateType.Update) break;
						if (joint == null) break;

						vfx.position = joint.TransformPoint(jointAttachment.offset);

						if (!jointAttachment.rotation) break;
						if (jointAttachment._2d) {
							Vector3 direction = joint.localToWorldMatrix * jointAttachment.jointAxis.normalized;
							vfx.right = (Vector2) direction.normalized;
						}
						else {
							vfx.localScale = joint.localToWorldMatrix.lossyScale;
							vfx.rotation = joint.rotation;
						}

						for (int i = 0; i < children.Count; i++) {
							Transform child = children[i];
							Transform target = targets[i];
							if (child && target) {
								child.position = target.position;
							}
						}
						break;
					case VfxAction.AttachType.RelativePosition:
						if (healthComponent.IsDead()) return;
						if (relativePosAttachment.followCaster) {
							Vector3 relativePos = relativePosAttachment.p;
							Vector3 casterPos = caster.Position();
							vfx.position = casterPos + relativePos.FlipFollowDirection(desiredFlipDirection);
							if (relativePosAttachment.followCasterFacing) {
								Direction direction = caster.FacingDirection();
								vfx.position = casterPos + relativePos.FlipFollowDirection(direction);
								vfx.right = direction.ToNormalizedVector2();
							}
						}
						break;
					case VfxAction.AttachType.Camera:
						if (healthComponent.IsDead()) return;
						Vector3 newPos = cameraTransform.position + cameraAttachment.relativePosition.FlipFollowDirection(desiredFlipDirection);
						if (cameraAttachment.xOnly) {
							newPos = new Vector3(newPos.x, vfx.position.y, vfx.position.z);
						}
						vfx.position = newPos;
						break;
				}

				if (trail && elapsed >= trailFadeoutAt) {
					trail.StopSmoothly();
					trail = null;
				}
				if (animator != null) {
					if (animatorStateInfo.normalizedTime >= 1) {
						GameObject.Destroy(vfx.gameObject);
					}

					if (elapsed >= timeToLive || isInterrupted) {
						if (!isFadingOut) {
							Fadeout();
							WaitForFadeoutFinishThenDestroy();
						}
					}
				}
				else {
					if (IsFinish()) {
						DestroyVfx();
					}
				}
			}

			private void WaitForFadeoutFinishThenDestroy() {
				//DLog.Log("SpawnPrefab:WaitForFadeoutFinishThenDestroy() " + skillId);
				Timing.RunCoroutine(WaitThenDestroy());
			}

			private IEnumerator<float> WaitThenDestroy() {
				float dt = 0.1f;
				while (true) {
					yield return Timing.WaitForSeconds(dt);
					//DLog.Log("SpawnPrefab:tick wait");
					if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1) {
						DestroyGameObject();
						break;
					}
				}
			}

			public void DestroyVfx() {
				if (animator) {
					if (animator.HasState(0, Animator.StringToHash("Fadeout"))) {
						if (!isFadingOut) {
							Fadeout();
							WaitForFadeoutFinishThenDestroy();
						}
					}
					else {
						DestroyGameObject();
					}
				}
				else {
					DestroyGameObject();
				}
			}

			private void DestroyGameObject() {
				//DLog.Log("SpawnPrefab:DestroyGameObject() " + skillId);
				if (vfx == null) return;
				if (!isVfxDestroyed) {
					//DLog.Log("SpawnPrefab:DestroyGameObject " + skillId);
					isVfxDestroyed = true;
					GameObject.Destroy(vfx.gameObject);
				}
			}

			private void Fadeout() {
				animator.Play("Fadeout");
				isFadingOut = true;
			}

			public bool IsFinish() {
				if (animator) {
					if (isFadingOut) {
						return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
					}
					else {
						return false;
					}
				}
				else {
					return elapsed >= timeToLive || isInterrupted || isVfxDestroyed;
				}
			}

			public void Interrupt() {
				//DLog.Log("SpawnPrefab:Interrupt() " + skillId);
				isInterrupted = true;
				if (animator) {
					if (animator.speed == 0) {
						DestroyGameObject();
					}
					else {
						if (animator.HasState(0, Animator.StringToHash("Fadeout"))) {
							if (!isFadingOut) {
								Fadeout();
								WaitForFadeoutFinishThenDestroy();
							}
						}
						else {
							DestroyGameObject();
						}
					}
				}

				if (trail) {
					if (trail.timeScale == 0) {
						DestroyGameObject();
					}
				}

				if (particleSystems != null) {
					if (particleSystems.Length > 0) {
						if (particleSystems[0] != null && particleSystems[0].isPaused) {
							DestroyGameObject();
						}
					}
				}
			}

			public void LateUpdate(float dt) {
				if (vfx == null) return;
				switch (attachType) {
					case VfxAction.AttachType.Joint:
						if (healthComponent.IsDead()) return;
						jointAttachmentElapsed += dt;
						if (jointAttachment.duration >= 0 && jointAttachmentElapsed >= jointAttachment.duration) break;
						if (updateType != VfxAction.UpdateType.LateUpdate) break;
						if (joint == null) break;

						vfx.position = joint.TransformPoint(jointAttachment.offset);

						if (!jointAttachment.rotation) break;
						if (jointAttachment._2d) {
							Vector3 direction = joint.rotation * jointAttachment.jointAxis;
							vfx.rotation = Quaternion.FromToRotation(jointAttachment.jointAxis, (Vector2) direction);
						}
						else {
							vfx.localScale = joint.localToWorldMatrix.lossyScale;
							vfx.rotation = joint.rotation;
						}

						for (int i = 0; i < children.Count; i++) {
							Transform child = children[i];
							Transform target = targets[i];
							if (child && target) {
                            	child.position = target.position;
                            }
						}
						break;
				}
			}

			public Transform Vfx {
				get { return vfx; }
			}

			public Transform Joint => joint;

			public void PauseForLockFrame() {
				if (trail) {
					trail.timeScale = 0;
				}

				if (particleSystems != null) {
					foreach (ParticleSystem particleSystem in particleSystems) {
						if (particleSystem == null) continue;

						particleSystem.Pause(true);
					}
				}

				if (animator) {
					animatorSpeed = animator.speed;
					animator.speed = 0;
				}
			}

			public void UnpauseForLockFrame() {
				if (trail) {
					trail.timeScale = 1;
				}

				if (particleSystems != null) {
					foreach (ParticleSystem particleSystem in particleSystems) {
						if (particleSystem == null) continue;

						particleSystem.Play(true);
					}
				}

				if (animator) {
					animator.speed = animatorSpeed;
				}
			}

			public void IncreaseTimeToLiveBy(float seconds) {
				timeToLive += seconds;
				if (trail) {
					trailFadeoutAt = timeToLive - trail.fadeoutDuration;
				}
			}
		}

		public class ChangeMaterialColor : Logic {
			private Environment environment;
			private Character caster;

			private VfxAction.ChangeMaterialColorVfx config;
			private float timeToLive;
			private float elapsed;
			private bool isInterrupted;
			private List<Material> materials = new List<Material>();
			private FlashingColorComponent flashingColorComponent;

			public ChangeMaterialColor(VfxAction.ChangeMaterialColorVfx config, float timeToLive, Environment environment, Character caster) {
				this.config = config;
				this.timeToLive = timeToLive;
				this.environment = environment;
				this.caster = caster;

				Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
				EntityGameObjectComponent gameObjectComponent = entity.GetComponent<EntityGameObjectComponent>();
				List<Renderer> allRenderers = new List<Renderer>(((GameObjectComponent)gameObjectComponent).RendererComponents);
				flashingColorComponent = entity.GetComponent<FlashingColorComponent>();

				for (int kIndex = 0; kIndex < allRenderers.Count; kIndex++) {
					Renderer r = allRenderers[kIndex];
					materials.AddRange(new List<Material>(r.materials));
				}

				if (config.flashAlphaStartFrame == 0) {
					float alpha = config.flashAlphaCurve.Evaluate(0);
					SetFlashAlpha(materials, config.flashAlphaKey, alpha);
				}

				if (config.materialAlphaStartFrame == 0) {
					float alpha = config.materialAlphaCurve.Evaluate(0);
					SetMaterialAlpha(materials, config.materialColorKey, alpha);
				}
			}

			public void Update(float dt) {
				elapsed += dt;

				FrameAndSecondsConverter _30fps = FrameAndSecondsConverter._30Fps;
				float flashAlphaStartTime = _30fps.FramesToSeconds(config.flashAlphaStartFrame);
				if (elapsed >= flashAlphaStartTime) {
					flashingColorComponent.StopFlashing();

					SetFlashColor(
						materials, config.flashColorKey, config.flashColor
					);

					float progress = (elapsed - flashAlphaStartTime) / config.flashAlphaDuration;
					float value = config.flashAlphaCurve.Evaluate(progress);
					SetFlashAlpha(materials, config.flashAlphaKey, value);
				}

				float materialAlphaStartTime = _30fps.FloatFramesToSeconds(config.materialAlphaStartFrame);
				if (elapsed >= materialAlphaStartTime) {
					float progress = (elapsed - materialAlphaStartTime) / config.materialAlphaDuration;
					float value = config.materialAlphaCurve.Evaluate(progress);
					SetMaterialAlpha(materials, config.materialColorKey, value);
				}
			}

			public void DestroyVfx() {
			}

			public bool IsFinish() {
				return elapsed >= timeToLive || isInterrupted;
			}

			public void Interrupt() {
				isInterrupted = true;
			}

			public void LateUpdate(float dt) {
			}

			public void IncreaseTimeToLiveBy(float seconds) {
				throw new NotImplementedException();
			}

			private void ForEachMaterial(List<Material> materials, Action<Material> action) {
				for (int kIndex = 0; kIndex < materials.Count; kIndex++) {
					action(materials[kIndex]);
				}
			}

			private void SetFlashColor(List<Material> materials, string flashColorKey, Color color) {
				ForEachMaterial(
					materials,
					material => {
						material.SetColor(flashColorKey, color);
					}
				);
			}

			private void SetFlashAlpha(List<Material> materials, string flashAlphaKey, float alpha) {
				ForEachMaterial(
					materials,
					material => {
						material.SetFloat(flashAlphaKey, alpha);
					}
				);
			}

			private void SetMaterialAlpha(List<Material> materials, string colorKey, float alpha) {
				ForEachMaterial(
					materials,
					material => {
						Color c = material.GetColor(colorKey);
						c.a = alpha;
						material.SetColor(colorKey, c);
					}
				);
			}
		}

		public class ChangeMaterial : Logic {
			public const string ALL = "_All";

			private float timeToLive;
			private Environment environment;
			private Character caster;

			private float elapsed;
			private VfxAction.ChangeMaterialVfx config;
			private List<ValueModifier> valueModifiers;
			private List<SsarTuple<Renderer, SsarTuple<Material, Material[]>>> originalMaterials = new List<SsarTuple<Renderer, SsarTuple<Material, Material[]>>>();
			private List<Renderer> allRenderers = new List<Renderer>();
			private ChangeMaterialTracker tracker;
			private bool interrupted;
			private List<Renderer> affectedRenderers = new List<Renderer>();
			private List<ColorModifier> colorModifiers;
			private FlashingColorComponent fcc;
			private bool isOriginalMaterialsRestored;

			public ChangeMaterial(VfxAction.ChangeMaterialVfx config, float timeToLive, Environment environment, Character caster) {
				this.timeToLive = timeToLive;
				this.environment = environment;
				this.caster = caster;

				this.config = config;
				GameObject go = caster.GameObject();
				Entity entity = go.GetComponent<EntityReference>().Entity;
				fcc = entity.GetComponent<FlashingColorComponent>();
				tracker = go.GetComponent<ChangeMaterialTracker>();
				if (tracker == null) {
					tracker = go.AddComponent<ChangeMaterialTracker>();
				}
				Logic runningLogic;
				if (tracker.ongoingByParentName.TryGetValue(config.parent, out runningLogic)) {
					if (runningLogic != null) {
						runningLogic.Interrupt();
					}
				}

				tracker.ongoingByParentName[config.parent] = this;
				EntityGameObjectComponent gameObjectComponent = entity.GetComponent<EntityGameObjectComponent>();
				IEnumerable<Renderer> renderers = ((GameObjectComponent)gameObjectComponent).RendererComponents;
				/*foreach (Renderer r in renderers) {
					DLog.Log("Vfx " + r.gameObject.name + " " + r.gameObject.GetInstanceID() + " renderer " + r.GetInstanceID() + " materials " + string.Join(", ", r.materials.Select(material => material.GetInstanceID().ToString()).ToArray()));
				}*/
				allRenderers.AddRange(new List<Renderer>(renderers));
				List<Material> materials = new List<Material>();
				Material newMaterial = config.ShowMaterial();
				// DLog.Log("debug ChangeMaterial new material " + newMaterial.GetHashCode());
				if (newMaterial == null) {
					throw new Exception("There isnt any material stored for material path: '" + config.materialPath + "'");
				}

				for (int kIndex = 0; kIndex < allRenderers.Count; kIndex++) {
					Renderer r = allRenderers[kIndex];
					if (!config.parent.Equals(ALL)) {
						if(!r.gameObject.name.Equals(config.parent)) continue;
					}
					affectedRenderers.Add(r);

					Material[] newMats = new Material[r.sharedMaterials.Length];
					for (int mIndex = 0; mIndex < newMats.Length; mIndex++) {
						if (mIndex == 0) {
							newMats[mIndex] = newMaterial;
						}
						else {
							newMats[mIndex] = r.sharedMaterials[mIndex];
						}
					}

					// DLog.Log("debug ChangeMaterial " + go.name + " " + go.GetInstanceID() + " cache origin value: Renderer " + r.GetInstanceID() + " material " + r.material.GetHashCode());
					originalMaterials.Add(new SsarTuple<Renderer, SsarTuple<Material, Material[]>>(
						r, new SsarTuple<Material, Material[]>(r.material, r.sharedMaterials)
					));
					r.sharedMaterials = newMats;
					materials.AddRange(r.sharedMaterials);
				}
				fcc.SwitchToMaterials(materials);

				valueModifiers = new List<ValueModifier>();
				for (int kIndex = 0; kIndex < config.valueModifiers.Count; kIndex++) {
					VfxAction.MaterialValueModifier mvm = config.valueModifiers[kIndex];
					valueModifiers.Add(new ValueModifier(mvm, materials));
				}

				colorModifiers = new List<ColorModifier>();
				for (int kIndex = 0; kIndex < config.colorModifiers.Count; kIndex++) {
					VfxAction.MaterialColorModifier mcm = config.colorModifiers[kIndex];
					colorModifiers.Add(new ColorModifier(mcm, materials));
				}
			}

			public void Update(float dt) {
				elapsed += dt;
				for (int kIndex = 0; kIndex < valueModifiers.Count; kIndex++) {
					valueModifiers[kIndex].Update(dt);
				}

				for (int kIndex = 0; kIndex < colorModifiers.Count; kIndex++) {
					colorModifiers[kIndex].Update(dt);
				}

				if (IsFinish()) {
					ReturnToOriginalMaterial();
					tracker.ongoingByParentName[config.parent] = null;
				}
			}

			private void ReturnToOriginalMaterial() {
				if (isOriginalMaterialsRestored) return;
				isOriginalMaterialsRestored = true;
				for (int kIndex = 0; kIndex < affectedRenderers.Count; kIndex++) {
					Renderer renderer = affectedRenderers[kIndex];
					for (int mIndex = 0; mIndex < originalMaterials.Count; mIndex++) {
						Renderer cachedRenderer = originalMaterials[mIndex].Element1;
						if (renderer != cachedRenderer) continue;

						Material[] mats = new Material[renderer.sharedMaterials.Length];
						for (int nIndex = 0; nIndex < mats.Length; nIndex++) {
							if (nIndex == 0) {
								mats[nIndex] = originalMaterials[mIndex].Element2.Element2[nIndex];
							}
							else {
								mats[nIndex] = renderer.sharedMaterials[nIndex];
							}
							// DLog.Log("debug ChangeMaterial " + "restore origin value: Renderer " + renderer.GetInstanceID() + " material " + mats[nIndex].GetHashCode());
						}
						renderer.sharedMaterials = mats;
					}
				}
				fcc.SwitchToOriginalMaterials();
			}

			public void DestroyVfx() {
				ReturnToOriginalMaterial();
			}

			public bool IsFinish() {
				return elapsed >= timeToLive || interrupted;
			}

			public void Interrupt() {
				interrupted = true;
				ReturnToOriginalMaterial();
				tracker.ongoingByParentName[config.parent] = null;
			}

			public void LateUpdate(float dt) {
			}

			public void IncreaseTimeToLiveBy(float seconds) {
				throw new NotImplementedException();
			}

			public class ValueModifier {
				private VfxAction.MaterialValueModifier config;
				private List<Material> materials;

				private float elapsed;

				public ValueModifier(VfxAction.MaterialValueModifier config, List<Material> materials) {
					this.config = config;
					this.materials = materials;

					for (int kIndex = 0; kIndex < materials.Count; kIndex++) {
						Material material = materials[kIndex];
						if (!material.HasProperty(config.name)) continue;

						material.SetFloat(config.name, config.curve.Evaluate(0));
					}
				}

				public void Update(float dt) {
					elapsed += dt;
					float progress = elapsed / config.duration;
					for (int kIndex = 0; kIndex < materials.Count; kIndex++) {
						Material material = materials[kIndex];
						if (!material.HasProperty(config.name)) continue;

						material.SetFloat(config.name, config.curve.Evaluate(progress));
					}
				}
			}

			public class ColorModifier {
				private VfxAction.MaterialColorModifier config;
				private List<Material> materials;

				private float elapsed;

				public ColorModifier(VfxAction.MaterialColorModifier config, List<Material> materials) {
					this.config = config;
					this.materials = materials;
				}

				public void Update(float dt) {
					elapsed += dt;
					float progress = elapsed / config.durationR;
					float r = config.curveR.Evaluate(progress) / 255f;
					progress = elapsed / config.durationR;
					float g = config.curveG.Evaluate(progress) / 255f;
					progress = elapsed / config.durationR;
					float b = config.curveB.Evaluate(progress) / 255f;
					progress = elapsed / config.durationR;
					float a = config.curveA.Evaluate(progress) / 255f;
					for (int kIndex = 0; kIndex < materials.Count; kIndex++) {
						Material m = materials[kIndex];
						if (!m.HasProperty(config.name)) continue;

						m.SetColor(config.name, new Color(r, g, b, a));
					}
				}
			}
		}
	}
}