using Core.Commons;
using Core.DungeonLogic.Spawn;
using Gameplay.DungeonLogic;

namespace Core.DungeonLogic.Environment.Monster {
	public interface Monster {
		float DeadTime();

		MonsterType Type();

		CharacterId CharacterId();

		long UniqueId();

		SpawnSourceInfo SpawnSource();

		EntityRole EntityRole();
	}
}
