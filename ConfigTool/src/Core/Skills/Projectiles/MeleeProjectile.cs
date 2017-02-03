using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Utils.Extensions;
//using Assets.Scripts.Ssar.Combat.Effects.Pool;
using MovementSystem.Components;
//using SSAR.BattleSystem.Utils;
using UnityEngine;
using Utils.DataStruct;

namespace Core.Skills.Projectiles {
	public class MeleeProjectile : Projectile {
		private Character character;
		private readonly Skill skill;
		private SsarCollider collider;
		private readonly int maxEnemyHitCount;
		private float timeToLive;
		private int numberOfHit;
		private float intervalBetweenHit;
		private readonly GameObject impactVfxPrefab;
		private readonly bool ignoreHeroPosOnYAxis;
		private readonly Vector3 characterPositionAtTheTimeOfFirstProjectileCreation;
		private readonly Direction characterFacingDirectionAtTheTimeOfFirstProjectileCreation;

		private float elapsed;
		private Trajectory trajectory;
		private MaxEnemyHitCountFilter maxEnemyHitCountFilter;
		private HitIntervalAndHitCountFilter hitIntervalAndHitCountFilter;
		private MeleeDamageFilter meleeDamageFilter;
		private Vector3 velocity = Vector3.zero;

		public MeleeProjectile(Character character, Skill skill, Collision collision, SsarCollider collider,
		                       int maxEnemyHitCount, float timeToLive, int noh, float ibh,
		                       float delayHandleObstacleCollision, float delayHandleObjectCollision, 
		                       GameObject impactVfxPrefab, bool ignoreHeroPosOnYAxis,
		                       Vector3 characterPositionAtTheTimeOfFirstProjectileCreation,
		                       Direction characterFacingDirectionAtTheTimeOfFirstProjectileCreation) : base(skill, collision, collider, maxEnemyHitCount, delayHandleObstacleCollision, delayHandleObjectCollision) {
			this.character = character;
			this.skill = skill;
			this.collider = collider;
			this.maxEnemyHitCount = maxEnemyHitCount;
			this.timeToLive = timeToLive;
			numberOfHit = noh;
			intervalBetweenHit = ibh;
			this.impactVfxPrefab = impactVfxPrefab;
			this.ignoreHeroPosOnYAxis = ignoreHeroPosOnYAxis;
			this.characterPositionAtTheTimeOfFirstProjectileCreation = characterPositionAtTheTimeOfFirstProjectileCreation;
			this.characterFacingDirectionAtTheTimeOfFirstProjectileCreation = characterFacingDirectionAtTheTimeOfFirstProjectileCreation;
			maxEnemyHitCountFilter = new MaxEnemyHitCountFilter(
				maxEnemyHitCount, numberOfHit, intervalBetweenHit
			);
			hitIntervalAndHitCountFilter = new HitIntervalAndHitCountFilter(numberOfHit, intervalBetweenHit);
			meleeDamageFilter = new MeleeDamageFilter();
		}

		protected internal override SsarCollider GetCollider() {
			return collider;
		}

		protected override bool ShouldFilterMaxEnemyHitCount() {
			return false;
		}

		protected internal override List<Character> PickInterestedOnesFrom(List<Character> collidedCharacters) {
			List<Character> filteredCollidedCharacters = new List<Character>();

			filteredCollidedCharacters = meleeDamageFilter.FilterForThisHit(elapsed, collidedCharacters);

			return filteredCollidedCharacters;
		}

		protected internal override void UpdateTrajectory(float dt) {
			elapsed += dt;

			Vector2 relativePos = collider.RelativePositionToCharacter();
			Direction facingDirection = character.FacingDirection();
			relativePos = relativePos.FlipFollowDirection(facingDirection);
			Vector2 charPos = character.Position();
			if (trajectory != null) {
				charPos = trajectory.AdjustCharacterPositionParam(
					charPos, characterPositionAtTheTimeOfFirstProjectileCreation
				);
			}
			if (ignoreHeroPosOnYAxis) {
//				charPos = BattleUtils.ClampPositionToGround(charPos);
			}
			collider.SetWorldPosition(charPos + relativePos);
			//DLog.Log("Melee projectile update trajectory");
			if (trajectory != null) {
				trajectory.Update(dt, this, Position());
			}
		}

		protected internal override bool IsFinish() {
			//DLog.Log("Melee projectile is finish " + elapsed + " " + timeToLive);
			return elapsed >= timeToLive;
		}

		protected override void OnDestroy() {
		}

		public override Vector3 Position() {
			return collider.WorldPosition();
		}

		public override Vector3 Velocity() {
			return velocity;
		}

		public override void SetVelocity(Vector3 velocity) {
			this.velocity = velocity;
		}

		public override void SetPosition(Vector3 newPosition) {
			collider.SetWorldPosition(newPosition);
		}

		public override void SetTrajectory(Trajectory newTrajectory) {
			this.trajectory = newTrajectory;
		}

		public void AdjustTimeToLive(float newValue) {
			timeToLive = newValue;
		}

		public void PlayVfx(Vector3 pos) {
			if (impactVfxPrefab == null) {
				#if UNITY_EDITOR
				DLog.LogWarning(string.Format(
					"Destroy Vfx of projectile of skill {0} is not configured", skill.GetType().Name
				));
				#endif
				return;
			}
			try {
//				effectPool.Obtain(impactVfxPrefab).transform.position = pos;
			}
			catch (Exception e) {
				DLog.LogError(e.Message + "\n" + e.StackTrace);
			}
		}

		public class MaxEnemyHitCountFilter {
			private readonly int maxEnemyHitCountPerHit;
			private readonly int numberOfHit;
			private readonly float intervalBetweenHit;

			private Dictionary<float, List<Character>> collidedCharactersesByElapsedTime = new Dictionary<float, List<Character>>();
			private float lastRecognizedElapsedTime = 0;
			private bool isFirstHit = true;

			public MaxEnemyHitCountFilter(int maxEnemyHitCountPerHit, int numberOfHit,
			                              float intervalBetweenHit) {
				this.maxEnemyHitCountPerHit = maxEnemyHitCountPerHit;
				this.numberOfHit = numberOfHit;
				this.intervalBetweenHit = intervalBetweenHit;
			}

			public List<Character> FilterForThisHit(float elapsedTime,
			                                        List<Character> collidedCharacters) {

				List<Character> filteredCollidedCharacters = new List<Character>();

				bool isThisHitValidInTermOfHitTime = false;
				if (isFirstHit) {
					isFirstHit = false;
					isThisHitValidInTermOfHitTime = true;
					lastRecognizedElapsedTime = elapsedTime;
				}
				else {
					float elapsedTimeSinceLastHit = elapsedTime - lastRecognizedElapsedTime;
					if (elapsedTimeSinceLastHit >= intervalBetweenHit) {
						isThisHitValidInTermOfHitTime = true;
						lastRecognizedElapsedTime = elapsedTime;
					}
				}

				bool isMaxNumberOfHitExceeded = collidedCharactersesByElapsedTime.Count >= numberOfHit;

				if (isThisHitValidInTermOfHitTime && !isMaxNumberOfHitExceeded) {
					List<Character> collidedCharactersOfThisHis = new List<Character>();
					collidedCharactersesByElapsedTime[elapsedTime] = collidedCharactersOfThisHis;

					for (int i = 0; i < collidedCharacters.Count; i++) {
						if(collidedCharactersOfThisHis.Count >= maxEnemyHitCountPerHit) break;

						collidedCharactersOfThisHis.Add(collidedCharacters[i]);
					}
					filteredCollidedCharacters.AddRange(collidedCharactersOfThisHis);
				}

				return filteredCollidedCharacters;
			}

			public bool IsMaxNumberOfHitExceeded() {
				return collidedCharactersesByElapsedTime.Count >= numberOfHit;
			}

			public int NumberOfHit() {
				return collidedCharactersesByElapsedTime.Count;
			}

			private List<Character> Act(bool isFirstHit, float elapsedTime,
			                            List<Character> collidedCharacters) {

				List<Character> result = new List<Character>();



				return result;
			}
		}

		public class HitIntervalAndHitCountFilter {
			private int numberOfHit;
			private float intervalBetweenHit;

			private Dictionary<Character, int> charactersAndHitCount = new Dictionary<Character, int>();
			private Dictionary<Character, float> charactersAndHitTime = new Dictionary<Character, float>();

			public HitIntervalAndHitCountFilter(int numberOfHit, float intervalBetweenHit) {
				this.numberOfHit = numberOfHit;
				this.intervalBetweenHit = intervalBetweenHit;
			}

			public List<Character> TakeAction(float elapsed, List<Character> collidedCharacters) {
				List<Character> filteredCollidedCharacters = new List<Character>();
				foreach (Character collidedCharacter in collidedCharacters) {
					if (!charactersAndHitCount.ContainsKey(collidedCharacter)) {
						charactersAndHitCount[collidedCharacter] = 0;
					}
					bool isFirstHit = false;
					if (!charactersAndHitTime.ContainsKey(collidedCharacter)) {
						charactersAndHitTime[collidedCharacter] = elapsed;
						isFirstHit = true;
					}

					int hitCount = charactersAndHitCount[collidedCharacter];
					if(hitCount >= numberOfHit) continue;

					if (!isFirstHit) {
						float previousHitTime = charactersAndHitTime[collidedCharacter];
						//DLog.Log("previous hit time " + previousHitTime);
						float hitTime = elapsed;
						float elapsedBetweenCurrentHitAndPreviousHit = hitTime - previousHitTime;
						if(elapsedBetweenCurrentHitAndPreviousHit < intervalBetweenHit) continue;

						charactersAndHitTime[collidedCharacter] = hitTime;
						hitCount++;
					}
					else {
						hitCount++;
					}

					charactersAndHitCount[collidedCharacter] = hitCount;

					filteredCollidedCharacters.Add(collidedCharacter);
					//DLog.Log("time " + elapsed + " " + charactersAndHitTime[collidedCharacter]);
				}
				return filteredCollidedCharacters;
			}
		}

		public class MeleeDamageFilter {
			private List<SsarTuple<float, List<Character>>> hitCharacterAndHitTime =
				new List<SsarTuple<float, List<Character>>>();

			public List<Character> FilterForThisHit(float elapsedTime, List<Character> collidedCharacters) {
				List<Character> hitCharacters = new List<Character>();

				for (int i = 0; i < collidedCharacters.Count; i++) {
					Character hitChar = collidedCharacters[i];
					bool isHitCharRecognized = false;
					for (int m = 0; m < hitCharacterAndHitTime.Count; m++) {
						List<Character> recognizedHitChars = hitCharacterAndHitTime[m].Element2;
						for (int p = 0; p < recognizedHitChars.Count; p++) {
							Character recognized = recognizedHitChars[p];
							if (recognized == hitChar) {
								isHitCharRecognized = true;
								break;
							}
						}

						if (isHitCharRecognized) break;
					}

					if (!isHitCharRecognized) {
						hitCharacters.Add(hitChar);
						//DLog.Log("Recognized hit char: " + hitChar.Id());
					}
				}
				hitCharacterAndHitTime.Add(new SsarTuple<float, List<Character>>(
					elapsedTime, hitCharacters
				));

				return hitCharacters;
			}
		}
	}
}
