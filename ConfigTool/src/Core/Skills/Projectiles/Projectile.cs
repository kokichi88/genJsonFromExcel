using System;
using System.Collections.Generic;
using System.Linq;
using Core.Skills.Modifiers;
using MovementSystem.Components;
//using SSAR.BattleSystem.Utils;
using UnityEngine;

namespace Core.Skills.Projectiles {
	public abstract class Projectile {
		private Skill skill;
		private Collision collision;
		private SsarCollider collider;
		private int maxEnemyHitCount;
		private readonly float delayHandleObstacleCollision;
		private readonly float delayHandleObjectCollision;
		private List<Character> hitEnemiesSoFar = new List<Character>();
		private Dictionary<ExtraName, object> extras = new Dictionary<ExtraName, object>();

		private bool isDestroyed;
		private float elapsed;

		protected Projectile(Skill skill, Collision collision, SsarCollider collider, int maxEnemyHitCount,
		                     float delayHandleObstacleCollision = 0, float delayHandleObjectCollision = 0)
		{
			this.skill = skill;
			this.collision = collision;
			this.collider = collider;
			this.maxEnemyHitCount = maxEnemyHitCount;
			this.delayHandleObstacleCollision = delayHandleObstacleCollision;
			this.delayHandleObjectCollision = delayHandleObjectCollision;
//			DLog.LogError("add new projectile: "+this.GetHashCode());
		}

		public virtual void Update(float dt) {
			if(isDestroyed) return;
			if (IsFinish()) return;

			elapsed += dt;

			UpdateTrajectory(dt);
			//DLog.LogError("update "+dt+" "+elapsed+" "+this.GetHashCode());
			if (collider == null) collider = GetCollider();
			List<Character> collidedCharacters = new List<Character>();
			//DLog.LogError("elapes: "+elapsed + " " + delayHandleObjectCollision+" "+collision.FindCharactersCollideWith(collider).Count+" "+this.GetHashCode());
			if (elapsed >= delayHandleObjectCollision) {
				collidedCharacters.AddRange(collision.FindCharactersCollideWith(collider));
			}
			//DLog.LogError("collided character: "+collidedCharacters.Count);
			
			List<Character> collidedEnemies = FilterCollidedEnemies(collidedCharacters);
			List<Character> additionalCollidedEnemies = new List<Character>();
			if (collidedEnemies.Count > 0) {
				List<Character> additionalCollidedCharacters = FindAdditionalCollidedCharacters();
				List<Character> uniqueAdditionalCollidedCharacters = new List<Character>();
				for (int i = 0; i < additionalCollidedCharacters.Count; i++) {
					Character additional = additionalCollidedCharacters[i];
					if (!collidedCharacters.Contains(additional)) {
						uniqueAdditionalCollidedCharacters.Add(additional);
					}
				}
				additionalCollidedEnemies = FilterCollidedEnemies(uniqueAdditionalCollidedCharacters);
			}
			collidedEnemies.AddRange(additionalCollidedEnemies);
			List<Character> ascSortedCollidedEnemies = SortByDistanceAsc(collidedEnemies);
			List<Character> hitEnemiesSoFarBeforeThisCollision = new List<Character>();
			hitEnemiesSoFarBeforeThisCollision.AddRange(hitEnemiesSoFar);
			FilterHitEnemiesSoFar(ascSortedCollidedEnemies);

			List<Character> filteredEnemies = new List<Character>();
			//DLog.LogError("collided character: "+ascSortedCollidedEnemies.Count+" "+collidedEnemies.Count+" "+additionalCollidedEnemies.Count);
			foreach (Character collidedCharacter in ascSortedCollidedEnemies) {
				if (ascSortedCollidedEnemies.Contains(collidedCharacter)
				    && hitEnemiesSoFar.Contains(collidedCharacter)
				    //this check should be written outside of this class (from design perspective)
				    && !IsCharacterVanish(collidedCharacter)) {
					filteredEnemies.Add(collidedCharacter);
				}
			}
			//DLog.LogError("projectile update "+BattleUtils.frame+" "+BattleUtils.time+" "+filteredEnemies.Count);
			if (filteredEnemies.Any()) {
				List<Character> enemiesGotHitForTheFirstTime = new List<Character>();
				foreach (Character c in hitEnemiesSoFar) {
					if(hitEnemiesSoFarBeforeThisCollision.Contains(c)) continue;

					enemiesGotHitForTheFirstTime.Add(c);
//					skill.OnProjectileHitTargetsForFirstTime(this, new List<Character>(new []{c}));
				}
				filteredEnemies = PickInterestedOnesFrom(filteredEnemies);
//				skill.OnProjectileHitTargets(this, filteredEnemies);
			}

			if (elapsed >= delayHandleObstacleCollision) {
				List<Obstacle> collidedObstacles = collision.FindObstaclesCollideWith(collider);
				if (collidedObstacles.Any()) {
//					skill.OnProjectileHitObstacles(this, collidedObstacles);
				}
			}
		}

		public float AgeInSeconds() {
			return elapsed;
		}

		protected virtual List<Character> FindAdditionalCollidedCharacters() {
			return new List<Character>();
		}

		public void PutExtras(ExtraName key, object extras) {
			this.extras[key] = extras;
		}

		public object GetExtras(ExtraName key) {
			if (!extras.ContainsKey(key)) {
				throw new Exception(string.Format("Key '{0}' is not existed", key));
			}

			return extras[key];
		}

		private bool IsCharacterVanish(Character character) {
			try {
				character.FindOngoingModifierOfType(ModifierType.Vanish);
				return true;
			}
			catch (Exception e) {
				return false;
			}
		}

		private List<Character> SortByDistanceAsc(List<Character> collidedEnemies) {
			List<EnemyWithDistanceToCaster> enemyWithDistanceToCasters = new List<EnemyWithDistanceToCaster>();
			foreach (Character collidedEnemy in collidedEnemies) {
				enemyWithDistanceToCasters.Add(new EnemyWithDistanceToCaster(
					Vector3.Distance(skill.Caster().Position(), collidedEnemy.Position()), collidedEnemy
				));
			}
			enemyWithDistanceToCasters.Sort((e1, e2) => (int)((e1.distance - e2.distance) * 1000));

			List<Character> result = new List<Character>();
			foreach (EnemyWithDistanceToCaster enemyWithDistanceToCaster in enemyWithDistanceToCasters) {
				result.Add(enemyWithDistanceToCaster.enemy);
			}
			return result;
		}

		protected void FilterHitEnemiesSoFar(List<Character> collidedEnemies) {
			foreach (Character collidedCharacter in collidedEnemies) {
				if(ShouldFilterMaxEnemyHitCount() && hitEnemiesSoFar.Count >= maxEnemyHitCount) continue;
				if(hitEnemiesSoFar.Contains(collidedCharacter)) continue;

				hitEnemiesSoFar.Add(collidedCharacter);
			}
		}

		private List<Character> FilterCollidedEnemies(List<Character> collidedCharacters) {
			List<Character> collidedEnemies = new List<Character>();

			foreach (Character collidedCharacter in collidedCharacters) {
				if (collidedCharacter.Group() == skill.Caster().Group()) continue;

				collidedEnemies.Add(collidedCharacter);
			}
			return collidedEnemies;
		}

		protected virtual bool ShouldFilterMaxEnemyHitCount() {
			return true;
		}

		protected internal abstract SsarCollider GetCollider();
		protected internal abstract List<Character> PickInterestedOnesFrom(List<Character> collidedCharacters);
		protected internal abstract void UpdateTrajectory(float dt);
		protected internal abstract bool IsFinish();
		protected abstract void OnDestroy();
		public abstract Vector3 Position();
		public abstract Vector3 Velocity();
		public abstract void SetVelocity(Vector3 velocity);
		public abstract void SetPosition(Vector3 newPosition);
		public abstract void SetTrajectory(Trajectory newTrajectory);

		public void Destroy() {
			if (!isDestroyed) {
				isDestroyed = true;
				OnDestroy();
			}
			else {
				//DLog.LogError("Destroy more than one time");
			}
		}

		private class EnemyWithDistanceToCaster {
			public float distance;
			public Character enemy;

			public EnemyWithDistanceToCaster(float distance, Character enemy) {
				this.distance = distance;
				this.enemy = enemy;
			}

			public override string ToString() {
				return string.Format("char {0} dist {1}", enemy, distance);
			}
		}

		public interface Trajectory {
			Direction AdjustCharacterFacingDirectionParam(Direction currentDirection,
			                                              Direction directionAtTheTimeOfFirstProjectileCreation);

			Vector3 AdjustCharacterPositionParam(Vector3 currentCharacterPosition,
			                                     Vector3 characterPositionAtTheTimeOfFirstProjectileCreation);
			void Update(float dt, Projectile projectile, Vector3 currentPosition);
		}
	}
}
