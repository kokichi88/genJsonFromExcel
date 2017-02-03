#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using Core.Commons;
using Equipment;
using JsonConfig.Model;
using LitJson;
using UnityEngine;

namespace Core.Skills.Vfxs.Weapons {
	[Serializable]
	public class WeaponMaterialConfig : ScriptableObject {
		public List<CharacterWeapons> characterWeapons = new List<CharacterWeapons>();
		public List<string> groupNames = new List<string>();
		public List<int> ids = new List<int>();
		public int groupCounter = 0;

#if UNITY_EDITOR
		public WeaponMaterialConfigEditor editor;
		public static Dictionary<int, bool> foldOutByGroupId = new Dictionary<int, bool>();
		public static Dictionary<int, Dictionary<int, bool>> foldOutByVisualIdByGroupId = new Dictionary<int, Dictionary<int, bool>>();
		public static Dictionary<int, Dictionary<int, Dictionary<int, bool>>> foldOutByProxyIdByVisualIdByGroupId = new Dictionary<int, Dictionary<int, Dictionary<int, bool>>>();
		private List<int> poolOfCharacterGroupId = new List<int>(new [] {
			1, 2, 3, 4, 5
		});
		private EquipmentVisualConfig equipmentVisualConfig;
		private Dictionary<int, List<int>> poolOfWeaponVisualIdByCharacterGroupId = new Dictionary<int, List<int>>();
		private Dictionary<int, List<string>> poolOfWeaponNameByCharacterGroupId = new Dictionary<int, List<string>>();
#endif

		public List<string> ListAllMaterialPaths() {
			List<string> r = new List<string>();
			foreach (CharacterWeapons cw in characterWeapons) {
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					foreach (MaterialInfo mi in wm.materialInfos) {
						if (!string.IsNullOrEmpty(mi.path)) {
							r.Add(mi.path);
						}
					}
				}
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					if (!string.IsNullOrEmpty(mi.path)) {
						r.Add(mi.path);
					}
				}
			}

			return r;
		}

		public void TemporaryStoreMaterial(string path, Material mat) {
			foreach (CharacterWeapons cw in characterWeapons) {
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					foreach (MaterialInfo mi in wm.materialInfos) {
						mi.TemporaryStoreMaterial(path, mat);
					}
				}
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					mi.TemporaryStoreMaterial(path, mat);
				}
			}
		}

		public Material FindMaterial(CharacterId characterId, int visualId, int id) {
			foreach (CharacterWeapons cw in characterWeapons) {
				if (cw.characterGroupId != characterId.GroupId) continue;
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					if (wm.weaponVisualId != visualId) continue;
					switch (wm.ShowSelectionMode()) {
						case SelectionMode.Override:
							foreach (MaterialInfo mi in wm.materialInfos) {
								if (mi.id == id) {
									return mi.ShowStoredMaterial();
								}
							}
							break;
						case SelectionMode.Inherit:
							Material mat = FindDefaultMaterial(characterId, id);
							foreach (MaterialInfo mi in wm.materialInfos) {
								if (mi.id == id) {
									Material overriddenMat = mi.ShowStoredMaterial();
									if (overriddenMat != null) {
										mat = overriddenMat;
									}
								}
							}

							return mat;
					}
				}
			}

			return null;
		}

		public Material FindDefaultMaterial(CharacterId characterId, int id) {
			foreach (CharacterWeapons cw in characterWeapons) {
				if (cw.characterGroupId != characterId.GroupId) continue;
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					if (mi.id == id) {
						return mi.ShowStoredMaterial();
					}
				}
			}

			return null;
		}

		public List<string> ListAllPrefabPaths() {
			List<string> r = new List<string>();
			foreach (CharacterWeapons cw in characterWeapons) {
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					foreach (MaterialInfo mi in wm.materialInfos) {
						if (!string.IsNullOrEmpty(mi.prefabPath)) {
							r.Add(mi.prefabPath);
						}
					}
				}
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					if (!string.IsNullOrEmpty(mi.prefabPath)) {
						r.Add(mi.prefabPath);
					}
				}
			}

			return r;
		}

		public void TemporaryStorePrefab(string path, GameObject prefab) {
			foreach (CharacterWeapons cw in characterWeapons) {
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					foreach (MaterialInfo mi in wm.materialInfos) {
						mi.TemporaryStorePrefab(path, prefab);
					}
				}
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					mi.TemporaryStorePrefab(path, prefab);
				}
			}
		}

		public GameObject FindPrefab(CharacterId characterId, int visualId, int id) {
			foreach (CharacterWeapons cw in characterWeapons) {
				if (cw.characterGroupId != characterId.GroupId) continue;
				foreach (WeaponAndMaterial wm in cw.weaponsAndMaterials) {
					if (wm.weaponVisualId != visualId) continue;
					switch (wm.ShowSelectionMode()) {
						case SelectionMode.Override:
							foreach (MaterialInfo mi in wm.materialInfos) {
								if (mi.id == id) {
									return mi.ShowStoredPrefab();
								}
							}
							break;
						case SelectionMode.Inherit:
							GameObject prefab = FindDefaultPrefab(characterId, id);
							foreach (MaterialInfo mi in wm.materialInfos) {
								if (mi.id == id) {
									GameObject overriddenPrefab = mi.ShowStoredPrefab();
									if (overriddenPrefab != null) {
										prefab = overriddenPrefab;
									}
								}
							}

							return prefab;
					}
				}
			}

			return null;
		}

		public GameObject FindDefaultPrefab(CharacterId characterId, int id) {
			foreach (CharacterWeapons cw in characterWeapons) {
				if (cw.characterGroupId != characterId.GroupId) continue;
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					if (mi.id == id) {
						return mi.ShowStoredPrefab();
					}
				}
			}

			return null;
		}

		public bool IsIdInUse(int id) {
			foreach (CharacterWeapons cw in characterWeapons) {
				foreach (MaterialInfo mi in cw.defaultWeapon.materialInfos) {
					if (mi.id == id) return true;
				}

				foreach (WeaponAndMaterial wam in cw.weaponsAndMaterials) {
					foreach (MaterialInfo mi in wam.materialInfos) {
						if (mi.id == id) return true;
					}
				}
			}

			return false;
		}

#if UNITY_EDITOR
		public void OnGUI() {
			if (equipmentVisualConfig == null)
			{
				string equipmentVisualConfigContent =
					((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/EquipmentVisualConfig.txt")).text;
				equipmentVisualConfig = JsonMapper.ToObject<EquipmentVisualConfig>(equipmentVisualConfigContent);
				foreach (int groupId in poolOfCharacterGroupId) {
					poolOfWeaponVisualIdByCharacterGroupId[groupId] = new List<int>();
					poolOfWeaponNameByCharacterGroupId[groupId] = new List<string>();
					IEnumerable<KeyValuePair<string,EquipmentVisualAvailableInfo>> allEquipmentVisualAvailableInfos = equipmentVisualConfig.GetAllEquipmentVisualAvailableInfos(EquipmentType.Weapon);
					List<KeyValuePair<string,EquipmentVisualAvailableInfo>> sorted = new List<KeyValuePair<string, EquipmentVisualAvailableInfo>>(allEquipmentVisualAvailableInfos);
					sorted.Sort((pair1, pair2) => { return pair1.Value.VisualId.CompareTo(pair2.Value.VisualId); });
					foreach (KeyValuePair<string,EquipmentVisualAvailableInfo> pair in sorted) {
						poolOfWeaponVisualIdByCharacterGroupId[groupId].Add(pair.Value.VisualId);
						poolOfWeaponNameByCharacterGroupId[groupId].Add(pair.Value.GetLocalizeName(new CharacterId(groupId, 1)));
					}
				}
			}
			EditorGUI.BeginChangeCheck();
			DrawMaterialNames();
			DrawWeaponMaterials();
			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(this);
			}
		}

		private void DrawWeaponMaterials() {
			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField("Groups by character", GUILayout.Width(256), GUILayout.ExpandWidth(false));
				if (EditorHelper.MiniButton("Add character", 96)) {
					CharacterWeapons cw = new CharacterWeapons();
					cw.characterGroupId = poolOfCharacterGroupId[0];
					characterWeapons.Add(cw);
				}
			}

			int indexToRemove = -1;
			using (new EditorHelper.IndentPadding(20)) {
				for (int i = 0; i < characterWeapons.Count; i++) {
					CharacterWeapons cw = characterWeapons[i];
					cw.OnGUI(
						poolOfCharacterGroupId,
						poolOfWeaponVisualIdByCharacterGroupId, poolOfWeaponNameByCharacterGroupId,
						groupNames,
						ids
					);

					if (cw.IsRemoved) {
						indexToRemove = i;
					}
				}
			}

			if (indexToRemove != -1) {
				characterWeapons.RemoveAt(indexToRemove);
			}
		}

		private void DrawMaterialNames() {
			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField("Group names", GUILayout.Width(128), GUILayout.ExpandWidth(false));
				if (EditorHelper.MiniButton("Add", 64)) {
					TextPrompt textPrompt = EditorWindow.GetWindow<TextPrompt>();
					textPrompt.textLabel = "Name";
					textPrompt.textValidator = s => {
						if (string.IsNullOrEmpty(s)) return false;
						foreach (string groupName in groupNames) {
							if (groupName.Equals(s)) return false;
						}
						return true;
					};
					textPrompt.onTextValueApplied = (s, o) => {
						groupCounter++;
						groupNames.Add(s);
						ids.Add(groupCounter);
						editor.Focus();
					};
					textPrompt.Show();
				}
			}

			int indexToRemove = -1;
			using (new EditorHelper.IndentPadding(20)) {
				for (int i = 0; i < groupNames.Count; i++) {
					string oldName = groupNames[i];
					using (new EditorHelper.Horizontal()) {
						EditorGUILayout.TextField("#" + ids[i], oldName, GUILayout.ExpandWidth(false), GUILayout.Width(256));
						if (EditorHelper.MiniButton("-")) {
							if (IsIdInUse(ids[i])) {
								DLog.Log($"Group name {oldName} is in use, cannot delete");
								return;
							}
							indexToRemove = i;
						}
						if (EditorHelper.MiniButton("Rename")) {
							TextPrompt textPrompt = EditorWindow.GetWindow<TextPrompt>();
							textPrompt.obj = new object[] {groupNames, i, oldName};
							textPrompt.textLabel = "New name";
							textPrompt.text = oldName;
							textPrompt.textValidator = s => {
								if (string.IsNullOrEmpty(s)) return false;
								foreach (string groupName in groupNames) {
									if (groupName.Equals(s)) return false;
								}
								return true;
							};
							textPrompt.onTextValueApplied = (s, o) => {
								List<string> _groupNames = (List<string>) o[0];
								int index = (int) o[1];
								_groupNames[index] = s;
								editor.Focus();
							};
							textPrompt.Show();
						}
					}
				}
			}

			if (indexToRemove != -1) {
				groupNames.RemoveAt(indexToRemove);
				ids.RemoveAt(indexToRemove);
			}
		}
#endif

		public class ConfigPath {
			private static string dir = "Config/Combat/Vfx";
			private static string fileName = "WeaponMaterial";
			private static string extension = ".asset";
			private static string resourcePath = dir + "/" + fileName;
			private static string resourcePathWithExtension = resourcePath + extension;

			public static string ResourcePath {
				get { return resourcePath; }
			}

			public static string ResourcePathWithExtension {
				get { return resourcePathWithExtension; }
			}
		}
	}
}