using UnityEngine;

namespace Core.Skills.Modifiers {
	public class VfxTag : MonoBehaviour {
		public Tag tag = Tag.Startup;

		public enum Tag {
			Startup
		}
	}
}