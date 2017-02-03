using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.DungeonLogic {
	public interface Component {
		void StartUp();

		void ShutDown();

		void Start();

		void Stop();

		void Update(float dt);
	}
}
