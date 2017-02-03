#if UNITY_EDITOR
using Combat.Skills.ModifierConfigs;
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Utils.Extensions;
using UnityEngine;

namespace Core.Skills.Vfxs.Impacts {
	[Serializable]
	public class CharacterIdAndPrefab {
		public int priority = 0;
		public List<int> groupIds = new List<int>();
		public string subIds = string.Empty;
		public int[] modifierTypes = new int[0];
		public List<int> weaponVisualIds = new List<int>();
		public string prefab = string.Empty;

		private GameObject prefab_;

#if UNITY_EDITOR
		private bool isRemoved;
		private EditorHelper.MultipleEnumSelectionDrawer<ModifierType> modifierTypesMenu;

		public void OnGUI(List<int> poolOfCharacterGroupId,
		                  List<int> poolOfWeaponVisualId,
		                  List<string> poolOfWeaponName) {
			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField(
					"Character group id", GUILayout.ExpandWidth(false), GUILayout.Width(128)
				);
				string label = groupIds.Count < 1 ? "ANY" : string.Join(", ", groupIds);
				if (GUILayout.Button(label)) {
					GenericMenu menu = new GenericMenu();

					foreach (int groupId in poolOfCharacterGroupId) {
						menu.AddItem(
							new GUIContent(groupId.ToString()),
							groupIds.Contains(groupId),
							data => {
								int gId = (int) data;
								if (groupIds.Contains(gId)) {
									groupIds.Remove(gId);
								}
								else {
									groupIds.Add(gId);
								}

								groupIds.Sort((i, i1) => { return i - i1; });
								GUI.changed = true;
							},
							groupId
						);
					}

					menu.ShowAsContext();
				}

				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 48;
				priority = EditorGUILayout.IntField("Priority", priority, GUILayout.ExpandWidth(false), GUILayout.Width(128));
				EditorGUIUtility.labelWidth = labelWidth;

				if (EditorHelper.MiniButton("-")) {
					isRemoved = true;
				}
			}

			GUIContent gc = new GUIContent("Character sub id(s)", "Separate sub id(s) by space");
			subIds = EditorGUILayout.TextField(gc, subIds);

			if (modifierTypesMenu == null) {
				modifierTypesMenu = new EditorHelper.MultipleEnumSelectionDrawer<ModifierType>(
					"ANY", 999
				);
			}

			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField("Buff/Modifier", GUILayout.ExpandWidth(false), GUILayout.Width(EditorGUIUtility.labelWidth));
				modifierTypesMenu.Draw(modifierTypes, selections => { this.modifierTypes = selections; });
			}

			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField(
					"Weapon visual id(s)", GUILayout.ExpandWidth(false), GUILayout.Width(128)
				);
				string label = weaponVisualIds.Count < 1 ? "ANY" : string.Join(", ", weaponVisualIds);
				if (GUILayout.Button(label)) {
					GenericMenu menu = new GenericMenu();

					int i = 0;
					foreach (int weaponVisualId in poolOfWeaponVisualId) {
						menu.AddItem(
							new GUIContent(weaponVisualId + " - " + poolOfWeaponName[i]),
							weaponVisualIds.Contains(weaponVisualId),
							data => {
								int gId = (int) data;
								if (weaponVisualIds.Contains(gId)) {
									weaponVisualIds.Remove(gId);
								}
								else {
									weaponVisualIds.Add(gId);
								}
								GUI.changed = true;
							},
							weaponVisualId
						);
						i++;
					}

					menu.ShowAsContext();
				}
			}

			prefab = new EditorHelper.PrefabDrawer().DrawFromPathString("Prefab", prefab);
		}

		public bool IsRemoved => isRemoved;
#endif

		public List<string> ListAllPrefabPaths() {
			List<string> r = new List<string>();
			if (!string.IsNullOrEmpty(prefab)) {
				r.Add(prefab);
			}

			return r;
		}

		public void TemporaryStorePrefab(string path, GameObject prefab) {
			if (this.prefab.Equals(path)) {
				prefab_ = prefab;
			}
		}

		public GameObject ShowStoredPrefab() {
			return prefab_;
		}

		public bool IsSubIdInteresting(string subId) {
			string[] split = subIds.Split(' ');

			return split.Contains(subId);
		}
	}
}