using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils {
	public class Handles : MonoBehaviour {
		private static Handles instance;
		private static GameObject gameObject;
		private static List<PositionHandle> positionHandles = new List<PositionHandle>();
		private static List<Label> labels = new List<Label>();

		public static Handles Instance {
			get {
				if (gameObject == null) {
					gameObject =  new GameObject("Handles");
					instance = gameObject.AddComponent<Handles>();
				}

				return instance;
			}
		}

		public PositionHandle AddPositionHandle(Vector2 position) {
			PositionHandle ph = new PositionHandle();
			ph.position = position;
			positionHandles.Add(ph);
			return ph;
		}

		public Label AddLabel(string text, Vector2 position) {
			Label l = new Label();
			l.text = text;
			l.position = position;
			labels.Add(l);
			return l;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos() {
			for (int kIndex = 0; kIndex < positionHandles.Count; kIndex++) {
				PositionHandle ph = positionHandles[kIndex];
				UnityEditor.Handles.DoPositionHandle(ph.position, Quaternion.identity);
			}

			for (int kIndex = 0; kIndex < labels.Count; kIndex++) {
				Label l = labels[kIndex];
				UnityEditor.Handles.Label(l.position, l.text);
			}
		}
#endif

		public class PositionHandle {
			public Vector2 position;
		}

		public class Label {
			public string text;
			public Vector2 position;
		}
	}
}