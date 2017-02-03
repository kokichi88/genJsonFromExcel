using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis;
using Core.Commons;
using EntityComponentSystem;
using MovementSystem.Components;
using RSG;
using UnityEngine;

namespace Core.DungeonLogic.Spawn {
	public abstract class Spawner {
		public abstract IPromise<Entity> SpawnEntity(CharacterId id, int level, float spawnX, float spawnY, float spawnZ,
		                                             SpawnSourceInfo spawnSource, Direction facingDirection,
		                                             int[] spawnSkillLevelPool, string entityGroup);
	}
}