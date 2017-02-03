using System.Collections.Generic;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class ChangeMaterialTracker : MonoBehaviour {
		public Dictionary<string, Vfx.Logic> ongoingByParentName = new Dictionary<string, Vfx.Logic>();
	}
}