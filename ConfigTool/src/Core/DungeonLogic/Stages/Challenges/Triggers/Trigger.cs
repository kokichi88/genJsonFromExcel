using System;
using System.Collections.Generic;
using System.Text;

namespace Core.DungeonLogic.Stages.Challenges.Triggers {
	public interface Trigger {
		bool IsFinished();

		void Update(float dt, int waveOrder);

		void SetEnv (Environment.Environment env);

		void SetCookies (IEnumerable<string> cookies);

		string UnfinishedReason();
	}
}
