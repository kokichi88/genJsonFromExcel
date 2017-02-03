using MovementSystem.Components;
using UnityEngine;

namespace Core.Utils.Extensions {
	public static class VectorExtension {
		public static Vector2 Clone(this Vector2 vector) {
			return new Vector2(vector.x, vector.y);
		}

		public static Vector2 CloneWithNewY(this Vector2 vector, float y) {
			return new Vector2(vector.x, y);
		}

		public static Vector2 CloneWithNewX(this Vector2 vector, float x) {
			return new Vector2(x, vector.y);
		}

		public static Vector3 CloneWithNewY(this Vector3 vector, float y) {
			return new Vector3(vector.x, y, vector.z);
		}

		public static Vector3 CloneWithNewX(this Vector3 vector, float x) {
			return new Vector3(x, vector.y, vector.z);
		}

		public static Vector2 ToLeftOrRightDirection(this Vector2 vector2) {
			if (vector2.x < 0) {
				return Vector2.left;
			}

			return Vector2.right;
		}

		public static Direction ToLeftOrRightDirectionEnum(this Vector2 vector2) {
			if (vector2.x < 0) {
				return Direction.Left;
			}

			return Direction.Right;
		}

		public static Vector2 FlipFollowDirection(this Vector2 vector, Direction newDirection,
		                                          Direction currentDirection = Direction.Right) {
			if (currentDirection == newDirection) {
				return vector;
			}

			return new Vector2(vector.x * -1, vector.y);
		}

		public static Vector3 FlipFollowDirection(this Vector3 vector, Direction newDirection,
		                                          Direction currentDirection = Direction.Right) {
			if (currentDirection == newDirection) {
				return vector;
			}

			return new Vector3(vector.x * -1, vector.y, vector.z);
		}

		public static string ToPreciseString(this Vector3 v) {
			return string.Format("x: {0:0.###} y: {1:0.###} z: {2:0.###}", v.x, v.y, v.z);
		}

		public static string ToPreciseString(this Vector2 v) {
			return string.Format("x: {0:0.###} y: {1:0.###}", v.x, v.y);
		}

		public static Vector3 Round(this Vector3 vector3, int decimalPlaces = 2) {
			float multiplier = 1;
			for (int i = 0; i < decimalPlaces; i++) {
				multiplier *= 10f;
			}

			return new Vector3(
				Mathf.Round(vector3.x * multiplier) / multiplier,
				Mathf.Round(vector3.y * multiplier) / multiplier,
				Mathf.Round(vector3.z * multiplier) / multiplier
			);
		}
	}
}