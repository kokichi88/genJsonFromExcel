#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;

namespace Core.Skills.Vfxs.Weapons {
	[Serializable]
	public class CharacterWeapons {
		public int characterGroupId;
		public List<WeaponAndMaterial> weaponsAndMaterials = new List<WeaponAndMaterial>();
		public WeaponAndMaterial defaultWeapon = new WeaponAndMaterial();

#if UNITY_EDITOR
		private bool isRemoved;
		private static WeaponAndMaterial copiedWeaponAndMaterial = null;
#endif

#if UNITY_EDITOR
		public void OnGUI(List<int> poolOfCharacterGroupId,
		                  Dictionary<int, List<int>> poolOfWeaponVisualIdByCharacterGroupId,
		                  Dictionary<int, List<string>> poolOfWeaponNameByCharacterGroupId,
		                  List<string> groupNames, List<int> ids) {
			List<int> poolOfWeaponVisualId = poolOfWeaponVisualIdByCharacterGroupId[characterGroupId];
			List<string> poolOfWeaponNames = poolOfWeaponNameByCharacterGroupId[characterGroupId];

			int selectedIndex = poolOfCharacterGroupId.IndexOf(characterGroupId);
			if (selectedIndex == -1) {
				selectedIndex = 0;
			}
			using (new EditorHelper.Horizontal()) {
				Dictionary<int,bool> foldOutByGroupId = WeaponMaterialConfig.foldOutByGroupId;
				if (!foldOutByGroupId.ContainsKey(characterGroupId)) {
					foldOutByGroupId[characterGroupId] = false;
				}
				foldOutByGroupId[characterGroupId] = GUILayout.Toggle(
					foldOutByGroupId[characterGroupId],
					"Character group id", "Foldout",
					GUILayout.ExpandWidth(false), GUILayout.Width(128)
				);
				selectedIndex = EditorGUILayout.Popup(
					"", selectedIndex, poolOfCharacterGroupId.Select(i => i.ToString()).ToArray(),
					GUILayout.ExpandWidth(false), GUILayout.Width(128)
				);
				characterGroupId = poolOfCharacterGroupId[selectedIndex];

				if (EditorHelper.MiniButton("Add weapon", 96)) {
					int availableIndex = -1;

					for (int i = 0; i < poolOfWeaponVisualId.Count; i++) {
						int visualId = poolOfWeaponVisualId[i];
						bool isUsed = false;
						foreach (WeaponAndMaterial wm in weaponsAndMaterials) {
							if (visualId == wm.weaponVisualId) {
								isUsed = true;
								break;
							}
						}

						if (!isUsed) {
							availableIndex = i;
							break;
						}
					}

					if (availableIndex != -1) {
						WeaponAndMaterial newWm = new WeaponAndMaterial();
						newWm.weaponVisualId = poolOfWeaponVisualId[availableIndex];
						weaponsAndMaterials.Add(newWm);
					}
				}

				if (EditorHelper.MiniButton("Sort")) {
					weaponsAndMaterials.Sort((wam1, wam2) => {
						return wam1.weaponVisualId.CompareTo(wam2.weaponVisualId);
					});
				}

				if (EditorHelper.MiniButton("Collapse all")) {
					Dictionary<int,Dictionary<int,bool>> foldOutData = WeaponMaterialConfig.foldOutByVisualIdByGroupId;
					if (foldOutData.ContainsKey(characterGroupId)) {
						List<int> keys = new List<int>(foldOutData[characterGroupId].Keys);
						foreach (int key in keys) {
							foldOutData[characterGroupId][key] = false;
						}
					}
				}

				if (EditorHelper.MiniButton("Expand all")) {
					Dictionary<int,Dictionary<int,bool>> foldOutData = WeaponMaterialConfig.foldOutByVisualIdByGroupId;
					if (foldOutData.ContainsKey(characterGroupId)) {
						List<int> keys = new List<int>(foldOutData[characterGroupId].Keys);
						foreach (int key in keys) {
							foldOutData[characterGroupId][key] = true;
						}
					}
				}

				if (EditorHelper.MiniButton("-")) {
					isRemoved = true;
				}

				if (!foldOutByGroupId[characterGroupId]) return;
			}

			bool foldOut = false;
			int defaultWeaponVisualId = int.MinValue;
			Dictionary<int,Dictionary<int,bool>> foldOutByVisualIdByGroupId = WeaponMaterialConfig.foldOutByVisualIdByGroupId;
			if (!foldOutByVisualIdByGroupId.ContainsKey(characterGroupId)) {
				foldOutByVisualIdByGroupId[characterGroupId] = new Dictionary<int, bool>();
			}
			if (!foldOutByVisualIdByGroupId[characterGroupId].ContainsKey(defaultWeaponVisualId)) {
				foldOutByVisualIdByGroupId[characterGroupId][defaultWeaponVisualId] = true;
			}
			using (new EditorHelper.IndentPadding(20)) {
				using (new EditorHelper.Horizontal()) {
					foldOut = GUILayout.Toggle(
						foldOutByVisualIdByGroupId[characterGroupId][defaultWeaponVisualId],
						$"Weapon default ({defaultWeapon.materialInfos.Count})", "Foldout",
						GUILayout.ExpandWidth(false), GUILayout.Width(192)
					);
					if (EditorHelper.MiniButton("Add group", 96)) {
						defaultWeapon.materialInfos.Add(new MaterialInfo());
					}

					if (EditorHelper.MiniButton("Copy")) {
						copiedWeaponAndMaterial = defaultWeapon;
					}

					if (EditorHelper.MiniButton("Paste")) {
						if (copiedWeaponAndMaterial != null) {
							defaultWeapon.materialInfos = JsonMapper.ToObject<List<MaterialInfo>>(
								JsonMapper.ToJson(copiedWeaponAndMaterial.materialInfos)
							);
							GUI.changed = true;
						}
					}
				}
				foldOutByVisualIdByGroupId[characterGroupId][defaultWeaponVisualId] = foldOut;
				if (foldOut) {
					int indexToRemove_ = -1;
					for (int i = 0; i < defaultWeapon.materialInfos.Count; i++) {
						MaterialInfo mi = defaultWeapon.materialInfos[i];
						using (new EditorHelper.IndentPadding(20)) {
							mi.OnGUI(groupNames, ids, characterGroupId, Int32.MinValue);
						}

						if (mi.IsRemoved) {
							indexToRemove_ = i;
						}
					}

					if (indexToRemove_ != -1) {
						defaultWeapon.materialInfos.RemoveAt(indexToRemove_);
					}
				}
			}

			int indexToRemove = -1;
			using (new EditorHelper.IndentPadding(20)) {
				for (int i = 0; i < weaponsAndMaterials.Count; i++) {
					WeaponAndMaterial wm = weaponsAndMaterials[i];
					wm.OnGUI(characterGroupId, poolOfWeaponVisualId, poolOfWeaponNames, groupNames, ids);
					if (wm.IsRemoved) {
						indexToRemove = i;
					}

					if (wm.IsCopied) {
						copiedWeaponAndMaterial = wm;
						wm.IsCopied = false;
					}

					if (wm.IsPasted && copiedWeaponAndMaterial != null) {
						weaponsAndMaterials[i].materialInfos = JsonMapper.ToObject<List<MaterialInfo>>(
							JsonMapper.ToJson(copiedWeaponAndMaterial.materialInfos)
						);
					}

					if (wm.IsPasted) {
						wm.IsPasted = false;
					}
				}
			}

			if (indexToRemove != -1) {
				weaponsAndMaterials.RemoveAt(indexToRemove);
			}
		}

		[JsonIgnore]
		public bool IsRemoved => isRemoved;
#endif
	}
}