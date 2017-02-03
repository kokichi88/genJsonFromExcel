using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Core.Utils;
using Checking;
using UnityEngine;

namespace Core.DungeonLogic.Environment.Character {
	public class GameObjectCharacter : Character {
		private GameObject gameObject;

		public GameObjectCharacter(GameObject gameObject) {
			new NotNullReference().Check(gameObject, "gameObject");

			this.gameObject = gameObject;
		}

		public Vector2 Position() {
			return gameObject.transform.position;
		}

		public bool IsDead() {
			return !gameObject.activeInHierarchy;
		}
	}
}
