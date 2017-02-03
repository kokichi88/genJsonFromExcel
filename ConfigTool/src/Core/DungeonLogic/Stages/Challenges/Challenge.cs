using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Commons;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Challenges {
	public interface Challenge {
		bool IsFinished();

		void Update(float dt, int waveOrder);

		string UnfinishedReason();

		SsarTuple<CharacterId, int> ShowMonsterIdAndSpawnCount();
	}
}
