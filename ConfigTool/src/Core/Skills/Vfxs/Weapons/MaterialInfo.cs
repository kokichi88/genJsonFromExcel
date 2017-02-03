#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace Core.Skills.Vfxs.Weapons {
	[Serializable]
	public class MaterialInfo {
		// public string name;
		public int id;
		public string path = string.Empty;
		public string prefabPath = string.Empty;

		private Material mat;
		private GameObject prefab;

#if UNITY_EDITOR
		private bool isRemoved;
		private static int copiedIndex = Int32.MinValue;
		private static string copiedMaterialPath = string.Empty;
		private static string copiedPrefabPath = string.Empty;
#endif

		public void TemporaryStoreMaterial(string path, Material mat) {
			if (this.path.Equals(path)) {
				this.mat = mat;
			}
		}

		public Material ShowStoredMaterial() {
			return mat;
		}

		public void TemporaryStorePrefab(string path, GameObject prefab) {
			if (prefabPath.Equals(path)) {
				this.prefab = prefab;
			}
		}

		public GameObject ShowStoredPrefab() {
			return prefab;
		}

#if UNITY_EDITOR
		public void OnGUI(List<string> materialNames, List<int> ids, int characterGroupId, int weaponVisualId) {
			if (id == 0) {
				id = ids[0];
			}
			Dictionary<int, Dictionary<int, Dictionary<int, bool>>> foldout =
				WeaponMaterialConfig.foldOutByProxyIdByVisualIdByGroupId;
			int index = ids.IndexOf(id);
			using (new EditorHelper.Horizontal()) {
				if (!foldout.ContainsKey(characterGroupId)) {
					foldout[characterGroupId] = new Dictionary<int, Dictionary<int, bool>>();
				}

				if (!foldout[characterGroupId].ContainsKey(weaponVisualId)) {
					foldout[characterGroupId][weaponVisualId] = new Dictionary<int, bool>();
				}

				if (!foldout[characterGroupId][weaponVisualId].ContainsKey(id)) {
					foldout[characterGroupId][weaponVisualId][id] = true;
				}
				foldout[characterGroupId][weaponVisualId][id] = GUILayout.Toggle(
					foldout[characterGroupId][weaponVisualId][id],
					"Group", "Foldout",
					GUILayout.ExpandWidth(false), GUILayout.Width(86)
				);

				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 0;
				index = EditorGUILayout.Popup("", index, materialNames.ToArray(), GUILayout.ExpandWidth(false), GUILayout.Width(256));
				if (EditorHelper.MiniButton("-")) {
					isRemoved = true;
				}
				EditorGUIUtility.labelWidth = labelWidth;

				if (EditorHelper.MiniButton("Copy")) {
					copiedIndex = index;
					copiedMaterialPath = path;
					copiedPrefabPath = prefabPath;
				}

				if (EditorHelper.MiniButton("Paste") && copiedIndex != Int32.MinValue) {
					index = copiedIndex;
					path = copiedMaterialPath;
					prefabPath = copiedPrefabPath;
				}
			}
			id = ids[index];

			if(!foldout[characterGroupId][weaponVisualId][id]) return;

			using (new EditorHelper.IndentPadding(20)) {
				path = new EditorHelper.MaterialDrawer().DrawFromPathString("Material", path);
				prefabPath = new EditorHelper.PrefabDrawer().DrawFromPathString("Prefab", prefabPath);
			}
		}

		[JsonIgnore]
		public bool IsRemoved => isRemoved;
#endif
	}
}