using System;
using System.Collections.Generic;
using Core.Skills.Modifiers;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class AdditionalAttachedVfxMarker : MonoBehaviour {
		public List<GameObject> vfxs = new List<GameObject>();
		public List<ModifierType> interestedModifierTypes = new List<ModifierType>();
	}
}