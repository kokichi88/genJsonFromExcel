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
	public class WeaponAndMaterial {
		public int selectionMode = (int) SelectionMode.Override;
		public int weaponVisualId;
		public List<MaterialInfo> materialInfos = new List<MaterialInfo>();

#if UNITY_EDITOR
		private bool isRemoved;
		private bool isCopied;
		private bool isPasted;
#endif

		public SelectionMode ShowSelectionMode() {
			return (SelectionMode) selectionMode;
		}

#if UNITY_EDITOR
		public void OnGUI(int characterGroupId, List<int> poolOfWeaponVisualId, List<string> poolOfWeaponNames,
		                  List<string> groupNames, List<int> ids) {
			int selectedIndex = poolOfWeaponVisualId.IndexOf(weaponVisualId);

			bool foldOut = false;
			Dictionary<int,Dictionary<int,bool>> foldOutByVisualIdByGroupId = WeaponMaterialConfig.foldOutByVisualIdByGroupId;
			if (!foldOutByVisualIdByGroupId.ContainsKey(characterGroupId)) {
				foldOutByVisualIdByGroupId[characterGroupId] = new Dictionary<int, bool>();
			}

			if (!foldOutByVisualIdByGroupId[characterGroupId].ContainsKey(weaponVisualId)) {
				foldOutByVisualIdByGroupId[characterGroupId][weaponVisualId] = true;
			}
			using (new EditorHelper.Horizontal()) {
				foldOut = GUILayout.Toggle(
					foldOutByVisualIdByGroupId[characterGroupId][weaponVisualId],
					$"Weapon visual id {weaponVisualId} ({materialInfos.Count})", "Foldout",
					GUILayout.ExpandWidth(false), GUILayout.Width(192)
				);
				EditorGUILayout.LabelField(poolOfWeaponNames[selectedIndex], GUILayout.ExpandWidth(false), GUILayout.Width(192));

				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 50;
				SelectionMode currentMode = ShowSelectionMode();
				SelectionMode newMode = (SelectionMode) EditorGUILayout.EnumPopup("Mode", currentMode, GUILayout.ExpandWidth(false), GUILayout.Width(150));
				selectionMode = (int) newMode;
				EditorGUIUtility.labelWidth = labelWidth;

				if (EditorHelper.MiniButton("Add group", 96)) {
					materialInfos.Add(new MaterialInfo());
				}

				if (EditorHelper.MiniButton("Copy")) {
					isCopied = true;
				}

				if (EditorHelper.MiniButton("Paste")) {
					isPasted = true;
				}

				if (EditorHelper.MiniButton("-")) {
					isRemoved = true;
				}
			}

			foldOutByVisualIdByGroupId[characterGroupId][weaponVisualId] = foldOut;
			if (!foldOut) return;

			int indexToRemove = -1;
			for (int i = 0; i < materialInfos.Count; i++) {
				MaterialInfo mi = materialInfos[i];
				using (new EditorHelper.IndentPadding(20)) {
					mi.OnGUI(groupNames, ids, characterGroupId, weaponVisualId);
				}

				if (mi.IsRemoved) {
					indexToRemove = i;
				}
			}

			if (indexToRemove != -1) {
				materialInfos.RemoveAt(indexToRemove);
			}
		}

		[JsonIgnore]
		public bool IsRemoved => isRemoved;

		[JsonIgnore]
		public bool IsCopied {
			get => isCopied;
			set => isCopied = value;
		}

		[JsonIgnore]
		public bool IsPasted {
			get => isPasted;
			set => isPasted = value;
		}
#endif
	}
}