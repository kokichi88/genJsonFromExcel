using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.DungeonLogic.StageActivators {
	public interface StageActivator {
		bool IsActive();

		void Update(float dt);
	}
}
