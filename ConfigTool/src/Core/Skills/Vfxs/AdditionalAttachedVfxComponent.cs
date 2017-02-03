using System.Collections.Generic;
using Artemis.Interface;
using Core.Skills.Modifiers;
using UnityEngine;

namespace Core.Skills.Vfxs {
	public class AdditionalAttachedVfxComponent : IComponent {
		public HashSet<ModifierType> interestedModifierTypes = new HashSet<ModifierType>();
		public List<GameObject> vfxs = new List<GameObject>();
	}
}