using System.Collections.Generic;
using Core.Skills;
using UnityEngine;

namespace Core.Skills {
	public interface Collision {
		List<Character> FindCharactersCollideWith(SsarCollider collider);
		List<Obstacle> FindObstaclesCollideWith(SsarCollider collider);
	}

	public interface SsarCollider {
		Vector3 RelativePositionToCharacter();

		void SetWorldPosition(Vector3 worldPos);

		Vector3 WorldPosition();

		Vector2 Dimension();

		float Radius();

		ColliderShape Shape();
	}

	public enum ColliderShape {
		Undefined,
		Rect,
		Circle
	}
}
