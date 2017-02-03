using System.Collections.Generic;
using Artemis;
using EntityComponentSystem;
using EntityComponentSystem.Components;
using EntityComponentSystem.Templates;
using EntrySystem;
using Equipment;
using Ssar.Combat.CustomizeVisual;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;
using Utils.DataStruct;
using ValueModifier = Core.Skills.Vfxs.Vfx.ChangeMaterial.ValueModifier;
using ColorModifier = Core.Skills.Vfxs.Vfx.ChangeMaterial.ColorModifier;

namespace Core.Skills.Vfxs {
	public class ChangeWeaponMaterialLogic : Vfx.Logic {
		private VfxAction.ChangeWeaponMaterialVfx config;
		private float timeToLive;
		private Character caster;
		private Environment environment;

		private Material material;
		private Renderer affectedRenderer;
		private SsarTuple<Material, Material[]> originalMaterials;
		private float elapsed;
		private bool isInterrupted;
		private List<ValueModifier> valueModifiers = new List<ValueModifier>();
		private List<ColorModifier> colorModifiers = new List<ColorModifier>();

		public ChangeWeaponMaterialLogic(VfxAction.ChangeWeaponMaterialVfx config, float timeToLive, Character caster, Environment environment) {
			this.config = config;
			this.timeToLive = timeToLive;
			this.caster = caster;
			this.environment = environment;
			Entity entity = caster.GameObject().GetComponent<EntityReference>().Entity;
			CustomizeVisualComponent cvc = entity.GetComponent<CustomizeVisualComponent>();
			EquipmentCollectData equipmentCollectData = cvc.EquipmentCollectData;
			if (equipmentCollectData == null) {
				material = environment.FindDefaultMaterialById(
					caster.CharacterId(), config.id
				);
			}
			else {
				material = environment.FindMaterialById(
					caster.CharacterId(), equipmentCollectData.EquipmentConfigId.VisualId , config.id
				);
			}

			if (material == null) {
				Interrupt();
				return;
			}

			EntityGameObjectComponent egoc = entity
				.GetComponent<EntityGameObjectComponent>();
			affectedRenderer = ((GameObjectComponent) egoc).WeaponRenderer;
			if (material != null) {
				originalMaterials = new SsarTuple<Material, Material[]>(affectedRenderer.material, affectedRenderer.materials);
				affectedRenderer.material = material;
				List<Material> materials = new List<Material>();
				materials.Add(material);

				for (int kIndex = 0; kIndex < config.valueModifiers.Count; kIndex++) {
					VfxAction.MaterialValueModifier mvm = config.valueModifiers[kIndex];
					valueModifiers.Add(new ValueModifier(mvm, materials));
				}

				for (int kIndex = 0; kIndex < config.colorModifiers.Count; kIndex++) {
					VfxAction.MaterialColorModifier mcm = config.colorModifiers[kIndex];
					colorModifiers.Add(new ColorModifier(mcm, materials));
				}
			}
		}

		public void Update(float dt) {
			if(IsFinish()) ReturnToOriginalMaterial();

			elapsed += dt;
			for (int kIndex = 0; kIndex < valueModifiers.Count; kIndex++) {
				valueModifiers[kIndex].Update(dt);
			}

			for (int kIndex = 0; kIndex < colorModifiers.Count; kIndex++) {
				colorModifiers[kIndex].Update(dt);
			}
		}

		public void DestroyVfx() {
			ReturnToOriginalMaterial();
		}

		public bool IsFinish() {
			return isInterrupted || elapsed >= timeToLive;
		}

		public void Interrupt() {
			isInterrupted = true;
		}

		public void LateUpdate(float dt) {
		}

		public void IncreaseTimeToLiveBy(float seconds) {
			timeToLive += seconds;
		}

		private void ReturnToOriginalMaterial() {
			if (affectedRenderer == null) return;
			affectedRenderer.material = originalMaterials.Element1;
			affectedRenderer.materials = originalMaterials.Element2;
			//DLog.Log("Vfx " + "restore origin value: Renderer " + renderer.GetInstanceID() + " material " + renderer.materials[0].GetInstanceID());
		}
	}
}