using Core.Skills;

namespace Core.DungeonLogic.Spawn {
	public abstract class SpawnSourceInfo {
		private SpawnSource src;

		public SpawnSourceInfo(SpawnSource src) {
			this.src = src;
		}

		public SpawnSource Source => src;
	}

	public class HeroSpawnSourceInfo : SpawnSourceInfo {
		public HeroSpawnSourceInfo() : base(SpawnSource.Hero) {
		}
	}

	public class DungeonSystemSpawnSourceInfo : SpawnSourceInfo {
		public DungeonSystemSpawnSourceInfo() : base(SpawnSource.Dungeon_System) {
		}
	}

	public class SkillSpawnSourceInfo : SpawnSourceInfo {
		private readonly SkillId skillId;
		private readonly long parentEntityUniqueId;

		public SkillSpawnSourceInfo(SkillId skillId, long parentEntityUniqueId) : base(SpawnSource.Skill) {
			this.skillId = skillId;
			this.parentEntityUniqueId = parentEntityUniqueId;
		}

		public SkillId SkillId => skillId;

		public long ParentEntityUniqueId => parentEntityUniqueId;
	}

	public class GateSpawnSourceInfo : SpawnSourceInfo {
		public GateSpawnSourceInfo() : base(SpawnSource.Gate) {
		}
	}
}