
using System;
using Combat.Skills.ModifierConfigs.Modifiers;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Events;
using Ssar.Combat.Skills.Events.Actions;
using UnityEngine;
using Utils;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using Utils.Editor;
#endif

namespace Core.Skills.Vfxs.Weapons {
#if UNITY_EDITOR
	public class WeaponMaterialConfigEditor : EditorWindow {
		public WeaponMaterialConfig config;

		private EditorHelper.ScrollView.ScrollPosition scrollPosition = new EditorHelper.ScrollView.ScrollPosition();

		private void OnGUI() {
			if (config == null) return;

			config.editor = this;
			using (new EditorHelper.ScrollView(scrollPosition)) {
				config.OnGUI();
			}

			/*if (EditorHelper.MiniButton("Transfer")) {
				config.ids.Clear();
				config.groupCounter = 0;
				for (int i = 0; i < config.groupNames.Count; i++) {
					config.groupCounter++;
					config.ids.Add(config.groupCounter);
				}

				foreach (CharacterWeapons cw in config.characterWeapons) {
					foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
						int index = config.groupNames.IndexOf(mi.name);
						if (index < 0) continue;
						mi.id = config.ids[index];
					}
					foreach (WeaponAndMaterial wam in cw.weaponsAndMaterials) {
						foreach (MaterialInfo mi in wam.materialInfos) {
							int index = config.groupNames.IndexOf(mi.name);
							if (index < 0) continue;
							mi.id = config.ids[index];
						}
					}
				}

				EditorUtility.SetDirty(config);

				string[] allAssetsPath = AssetDatabaseExtensions.FindPathOfAllSsar3ScriptableObjects();
				bool shouldSave = false;
				foreach (string assetPath in allAssetsPath) {
					try {
						AssetFile af = new AssetFile(assetPath);
						string resourcePath = af.ShowResourcePath();

						UnityEngine.Object resource = Resources.Load(resourcePath);
						if (!(resource is ScriptableObject)) continue;
						ScriptableObject so = (ScriptableObject) resource;
						if (so is SkillFrameConfig) {
							SkillFrameConfig sfc = (SkillFrameConfig) so;
							sfc.DeserializeEventCollection();
							bool dirty = false;
							foreach (EventCollection ec in sfc.eventCollection) {
								foreach (BaseEvent be in ec.events) {
									BaseAction ba = be.ShowAction();
									if (ba.ShowActionType() == ActionType.Vfx) {
										VfxAction va = (VfxAction) ba;
										if (va.baseVfx.ShowVfxType() == VfxAction.VfxType.ChangeWeaponMaterial) {
											VfxAction.ChangeWeaponMaterialVfx cwmv = (VfxAction.ChangeWeaponMaterialVfx) va.baseVfx;
											cwmv.id = config.ids[config.groupNames.IndexOf(cwmv.name)];
											dirty = true;
											shouldSave = true;
										}
									}
								}
							}

							if (dirty) {
								sfc.SerializeEventCollection();
								EditorUtility.SetDirty(so);
							}
						}
					}
					catch (Exception e) {
						DLog.Log(assetPath);
						DLog.LogException(e);
					}
				}

				if (shouldSave) {
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
				}
			}*/
		}
	}
#endif
}