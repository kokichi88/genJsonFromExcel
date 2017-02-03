using System.Collections.Generic;
using Core.Commons;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Waves {
	public interface WaveLogic {
		bool IsFinished();

		void Update(float dt);

		string UnfinishedReason();

		List<SsarTuple<CharacterId, int>> ShowMonsterIdAndSpawnCount();
	}
}