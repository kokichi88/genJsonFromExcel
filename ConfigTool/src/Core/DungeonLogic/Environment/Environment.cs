using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.DungeonLogic.Spawn;
using UnityEngine;

namespace Core.DungeonLogic.Environment {
	public interface Environment {
		bool IsIdle();

		Character.Character Character();

		IEnumerable<Monster.Monster> DeadMonstersSoFar();

		IEnumerable<Monster.Monster> DeadMonstersSoFarIncludingOnesSpawnedByOthers();

		float ElapsedTime();

		Spawner Spawner();

		GameObject GetGateById(int gateId);

		bool AreAllBeatableMonstersFromCurrentStageDead();

		bool IsEventTriggered(int eventId);

		void ClearTriggeredEvents();
	}
}
