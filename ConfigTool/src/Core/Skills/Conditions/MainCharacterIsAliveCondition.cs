using System.Collections;
using System.Collections.Generic;
using Artemis;
//using SSAR.BattleSystem.Utils;
using UnityEngine;

namespace Core.Skills.Conditions {

	public class MainCharacterIsAliveCondition : Condition {
//		private HealthComponent mainHealthComponent;
//		public MainCharacterIsAliveCondition() {
//			Entity main = DungeonService.GetEntry<Entity>(DungeonBlackBoardName.MAIN_CHARACTER);
//			if (main != null) {
//				mainHealthComponent = main.GetComponent<HealthComponent>();
//			}
//		}

		public bool IsMeet() {
//			return mainHealthComponent != null && mainHealthComponent.IsAlive();
			return false;
		}

		public void Update(float dt) {
		}

		public string Reason() {
			return "Main char is not alive";
		}
	}
}
