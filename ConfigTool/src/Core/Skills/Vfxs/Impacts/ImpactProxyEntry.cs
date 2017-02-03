#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Core.Commons;
using UnityEngine;
using JsonConfig.Model;

namespace Core.Skills.Vfxs.Impacts {
	[Serializable]
	public class ImpactProxyEntry {
		public int proxyId = -1;
		public string prefab = string.Empty;
		public List<CharacterIdAndPrefab> cidAndPrefabs = new List<CharacterIdAndPrefab>();

		private GameObject prefab_;

#if UNITY_EDITOR
		private bool isRemoved;

		public void OnGUI(ImpactProxyConfig.Foldout foldout, string foldoutName, List<Proxy> proxies,
		                  List<int> poolOfCharacterGroupId,
		                  List<int> poolOfWeaponVisualId, List<string> poolOfWeaponName) {
			bool isFoldout = DrawProxyName(foldout, foldoutName, proxies);
			if (!isFoldout) return;
			DrawDefaultPrefab();
			DrawCharacterIdAndPrefabs(
				poolOfCharacterGroupId, poolOfWeaponVisualId, poolOfWeaponName
			);
		}

		private void DrawDefaultPrefab() {
			using (new EditorHelper.IndentPadding(20)) {
				prefab = new EditorHelper.PrefabDrawer().DrawFromPathString("Default prefab", prefab);
			}
		}

		private void DrawCharacterIdAndPrefabs(List<int> poolOfCharacterGroupId,
		                                       List<int> poolOfWeaponVisualId,
		                                       List<string> poolOfWeaponName) {
			using (new EditorHelper.IndentPadding(20)) {
				using (new EditorHelper.Horizontal()) {
					EditorGUILayout.LabelField("Overrides", GUILayout.Width(64), GUILayout.ExpandWidth(false));
					if (EditorHelper.MiniButton("Add", 64)) {
						cidAndPrefabs.Add(new CharacterIdAndPrefab());
					}
				}

				using (new EditorHelper.IndentMargin(20)) {
					int indexToRemove = -1;
					for (int index = 0; index < cidAndPrefabs.Count; index++) {
						CharacterIdAndPrefab ciap = cidAndPrefabs[index];
						ciap.OnGUI(
							poolOfCharacterGroupId, poolOfWeaponVisualId, poolOfWeaponName
						);
						if (ciap.IsRemoved) {
							indexToRemove = index;
						}
					}

					if (indexToRemove != -1) {
						cidAndPrefabs.RemoveAt(indexToRemove);
					}
				}
			}
		}

		private bool DrawProxyName(ImpactProxyConfig.Foldout foldout, string foldoutName, List<Proxy> proxies) {
			if (proxyId == -1) {
				proxyId = proxies[0].id;
			}

			int index = -1;
			for (int i = 0; i < proxies.Count; i++) {
				if (proxies[i].id == proxyId) {
					index = i;
				}
			}
			if (!foldout.foldoutByPath.ContainsKey(foldoutName)) {
				foldout.foldoutByPath[foldoutName] = true;
			}

			bool isFoldout = foldout.foldoutByPath[foldoutName];
			using (new EditorHelper.Horizontal()) {
				GUIStyle gs = new GUIStyle(EditorStyles.foldout);
				gs.stretchWidth = false;
				gs.fixedWidth = 75;
				isFoldout = EditorGUILayout.Foldout(isFoldout, "Proxy", true, gs);
				foldout.foldoutByPath[foldoutName] = isFoldout;

				index = EditorGUILayout.Popup(
					"", index, proxies.Select(proxy => proxy.name).ToArray(), GUILayout.ExpandWidth(false), GUILayout.Width(256)
				);
				if (EditorHelper.MiniButton("-")) {
					isRemoved = true;
				}
			}

			proxyId = proxies[index].id;
			return isFoldout;
		}

		public bool IsRemoved => isRemoved;
#endif

		public List<string> ListAllPrefabPaths() {
			List<string> r = new List<string>();
			if (!string.IsNullOrEmpty(prefab)) {
				r.Add(prefab);
			}

			foreach (CharacterIdAndPrefab ciap in cidAndPrefabs) {
				foreach (string prefabPath in ciap.ListAllPrefabPaths()) {
					r.Add(prefabPath);
				}
			}

			return r;
		}

		public void TemporaryStorePrefab(string path, GameObject prefab) {
			if (this.prefab.Equals(path)) {
				prefab_ = prefab;
			}

			foreach (CharacterIdAndPrefab ciap in cidAndPrefabs) {
				ciap.TemporaryStorePrefab(path, prefab);
			}
		}

		public GameObject FindPrefab(CharacterId characterId, int[] typesOfOngoingModifiers, int[] weaponVisualIds) {
			string subIdString = characterId.SubId.ToString();
			GameObject result = prefab_;
			List<CharacterIdAndPrefab> met = new List<CharacterIdAndPrefab>();
			foreach (CharacterIdAndPrefab ciap in cidAndPrefabs) {
				bool isGroupIdMet = false;
				if (ciap.groupIds.Count < 1) {
					isGroupIdMet = true;
				}
				else {
					isGroupIdMet = ciap.groupIds.Contains(characterId.GroupId);
				}

				bool isSubIdMet = false;
				if (string.IsNullOrEmpty(ciap.subIds)) {
					isSubIdMet = true;
				}
				else {
					if (ciap.IsSubIdInteresting(subIdString)) {
						isSubIdMet = true;
					}
				}

				bool isBuffModifierMet = false;
				if (ciap.modifierTypes.Length < 1) {
					isBuffModifierMet = true;
				}
				else {
					foreach (int typesOfOngoingModifier in typesOfOngoingModifiers) {
						foreach (int modifierType in ciap.modifierTypes) {
							if (typesOfOngoingModifier == modifierType) {
								isBuffModifierMet = true;
								break;
							}
						}
						if (isBuffModifierMet) break;
					}
				}

				bool isWeaponVisualIdMet = false;
				if (ciap.weaponVisualIds.Count < 1) {
					isWeaponVisualIdMet = true;
				}
				else {
					foreach (int weaponVisualId in weaponVisualIds) {
						foreach (int ciapWeaponVisualId in ciap.weaponVisualIds) {
							if (weaponVisualId == ciapWeaponVisualId) {
								isWeaponVisualIdMet = true;
								break;
							}
						}

						if (isWeaponVisualIdMet) break;
					}
				}

				if (isGroupIdMet && isSubIdMet && isBuffModifierMet && isWeaponVisualIdMet) {
					met.Add(ciap);
				}
			}
			met.Sort((ciap1, ciap2) => { return ciap2.priority.CompareTo(ciap1.priority);});
			if (met.Count > 0) {
				return met[0].ShowStoredPrefab();
			}

			return result;
		}
	}
}