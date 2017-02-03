using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artemis.Interface;

namespace Core.DungeonLogic.Stages.Goals {
	public class MonsterIdEntityComponent : IComponent {
		private string configId;

		public MonsterIdEntityComponent(string configId) {
			this.configId = configId;
		}

		public string ConfigId {
			get { return configId; }
		}
	}
}
