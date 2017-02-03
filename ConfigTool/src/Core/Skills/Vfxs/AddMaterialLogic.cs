using System.Collections.Generic;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;
using ValueModifier = Core.Skills.Vfxs.Vfx.ChangeMaterial.ValueModifier;
using ColorModifier = Core.Skills.Vfxs.Vfx.ChangeMaterial.ColorModifier;

namespace Core.Skills.Vfxs {
	public class AddMaterialLogic : Vfx.Logic {
		public const string ALL = "_All";

		private VfxAction.AddMaterialVfx config;
		private float timeToLive;

		private float elapsed;
		private bool interrupted;
		private Material material;
		private List<Renderer> affectedRenderers = new List<Renderer>();
		private List<ValueModifier> valueModifiers;
		private List<ColorModifier> colorModifiers;

		public AddMaterialLogic(VfxAction.AddMaterialVfx config, float timeToLive, Character caster) {
			this.config = config;
			this.timeToLive = timeToLive;

			material = config.ShowMaterial();
			// DLog.Log("debug AddMaterialLogic material from config " + material.GetHashCode());

			EntityGameObjectComponent gameObjectComponent = caster.GameObject().GetComponent<EntityReference>().Entity.GetComponent<EntityGameObjectComponent>();
			IEnumerable<Renderer> allRenderers = ((GameObjectComponent)gameObjectComponent).RendererComponents;
			foreach (Renderer renderer in allRenderers) {
				if (!config.parent.Equals(ALL)) {
					if (!renderer.gameObject.name.Equals(config.parent)) continue;
				}

				affectedRenderers.Add(renderer);
				Material[] materials = new Material[renderer.sharedMaterials.Length + 1];
				materials[renderer.sharedMaterials.Length] = material;
				for (int i = 0; i < renderer.sharedMaterials.Length; i++) {
					materials[i] = renderer.sharedMaterials[i];
					// DLog.Log("debug AddMaterialLogic new sharedMaterial " + materials[i].GetHashCode());
				}

				renderer.sharedMaterials = materials;
			}

			valueModifiers = new List<ValueModifier>();
			for (int kIndex = 0; kIndex < config.valueModifiers.Count; kIndex++) {
				VfxAction.MaterialValueModifier mvm = config.valueModifiers[kIndex];
				valueModifiers.Add(new ValueModifier(mvm, new List<Material>(new []{material})));
			}

			colorModifiers = new List<ColorModifier>();
			for (int kIndex = 0; kIndex < config.colorModifiers.Count; kIndex++) {
				VfxAction.MaterialColorModifier mcm = config.colorModifiers[kIndex];
				colorModifiers.Add(new ColorModifier(mcm, new List<Material>(new []{material})));
			}

			// DLog.Log("debug AddMaterialLogic");
		}

		public void Update(float dt) {
			elapsed += dt;
			if (IsFinish()) {
				DestroyVfx();
			}

			for (int kIndex = 0; kIndex < valueModifiers.Count; kIndex++) {
				valueModifiers[kIndex].Update(dt);
			}

			for (int kIndex = 0; kIndex < colorModifiers.Count; kIndex++) {
				colorModifiers[kIndex].Update(dt);
			}
		}

		public void DestroyVfx() {
			// DLog.Log("debug AddMaterialVfx:DestroyVfx()");
			foreach (Renderer renderer in affectedRenderers) {
				List<int> indexesToIgnore = new List<int>();
				for (int index = 0; index < renderer.sharedMaterials.Length; index++) {
					// DLog.Log("debug AddMaterialLogic material from renderer " + renderer.sharedMaterials[index].GetHashCode());
					if (renderer.sharedMaterials[index] == material) {
						indexesToIgnore.Add(index);
					}
				}

				Material[] materials = new Material[renderer.sharedMaterials.Length - indexesToIgnore.Count];
				int counter = 0;
				for (int index = 0; index < materials.Length; index++) {
					if (indexesToIgnore.Contains(index)) continue;
					materials[counter] = renderer.sharedMaterials[index];
					// DLog.Log("debug AddMaterialLogic restore sharedMaterial " + materials[counter].GetHashCode());
					counter++;
				}

				renderer.sharedMaterials = materials;
			}
		}

		public bool IsFinish() {
			return elapsed >= timeToLive || interrupted;
		}

		public void Interrupt() {
			interrupted = true;
		}

		public void LateUpdate(float dt) {
		}

		public void IncreaseTimeToLiveBy(float seconds) {
			timeToLive += seconds;
		}
	}
}