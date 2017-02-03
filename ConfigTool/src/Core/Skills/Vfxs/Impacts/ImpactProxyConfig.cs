#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System.Collections.Generic;
using Core.Commons;
using JsonConfig.Model;
using LitJson;
using UnityEngine;
using Equipment;

namespace Core.Skills.Vfxs.Impacts {
	public class ImpactProxyConfig : ScriptableObject {
		public int proxyCounter = 0;
		public List<Proxy> proxies = new List<Proxy>();
		public List<ImpactProxyEntry> entries = new List<ImpactProxyEntry>();

#if UNITY_EDITOR
		public ImpactProxyConfigEditor editor;
		private List<int> poolOfCharacterGroupId = new List<int>();
		private MonsterConfig monsterConfig;
		private Foldout foldout;
		private EquipmentVisualConfig equipmentVisualConfig;
		private List<int> poolOfWeaponVisualId = new List<int>();
		private List<string> poolOfWeaponName = new List<string>();

		public void OnGUI() {
			if (monsterConfig == null) {
				string monsterConfigContent =
					((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/MonsterConfig.txt")).text;
				monsterConfig = JsonMapper.ToObject<MonsterConfig>(monsterConfigContent);
				List<int> listAllGroupIds = monsterConfig.ListAllGroupIds();
				listAllGroupIds.Sort((i, i1) => { return i - i1; });
				poolOfCharacterGroupId = new List<int>(listAllGroupIds);
			}

			if (equipmentVisualConfig == null) {
				poolOfWeaponVisualId = new List<int>();
				poolOfWeaponName = new List<string>();
				CharacterId characterId = new CharacterId(1, 1);
				string equipmentVisualConfigContent =
					((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/EquipmentVisualConfig.txt"))
					.text;
				equipmentVisualConfig = JsonMapper.ToObject<EquipmentVisualConfig>(equipmentVisualConfigContent);
				IEnumerable<KeyValuePair<string, EquipmentVisualAvailableInfo>> allEquipmentVisualAvailableInfos =
					equipmentVisualConfig.GetAllEquipmentVisualAvailableInfos(EquipmentType.Weapon);
				List<KeyValuePair<string,EquipmentVisualAvailableInfo>> sorted = new List<KeyValuePair<string, EquipmentVisualAvailableInfo>>(allEquipmentVisualAvailableInfos);
				sorted.Sort((pair1, pair2) => { return pair1.Value.VisualId.CompareTo(pair2.Value.VisualId); });
				HashSet<int> uniqueVisualIds = new HashSet<int>();
				foreach (KeyValuePair<string, EquipmentVisualAvailableInfo> pair in sorted) {
					if (!uniqueVisualIds.Contains(pair.Value.VisualId)) {
						uniqueVisualIds.Add(pair.Value.VisualId);
						poolOfWeaponVisualId.Add(pair.Value.VisualId);
						poolOfWeaponName.Add(pair.Value.GetLocalizeName(characterId));
					}
				}
			}

			if (foldout == null) {
				Foldout f = LoadFoldout(this);
				if (f == null) {
					f = new Foldout();
				}

				foldout = f;
			}

			EditorGUI.BeginChangeCheck();
			DrawProxyNames();
			DrawProxyEntries(foldout);
			if (EditorGUI.EndChangeCheck()) {
				EditorUtility.SetDirty(this);
				SaveFoldout(this, foldout);
			}
		}

		private void DrawProxyEntries(Foldout foldout) {
			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField("Proxy entries", GUILayout.Width(128), GUILayout.ExpandWidth(false));
				if (EditorHelper.MiniButton("Add", 64)) {
					entries.Add(new ImpactProxyEntry());
				}
			}

			using (new EditorHelper.IndentPadding(20)) {
				int indexToRemove = -1;
				for (int index = 0; index < entries.Count; index++) {
					ImpactProxyEntry entry = entries[index];
					entry.OnGUI(
						foldout, "ProxyEntries." + index, proxies, poolOfCharacterGroupId,
						poolOfWeaponVisualId, poolOfWeaponName
					);
					if (entry.IsRemoved) {
						indexToRemove = index;
					}
				}

				if (indexToRemove != -1) {
					entries.RemoveAt(indexToRemove);
				}
			}
		}

		private void DrawProxyNames() {
			using (new EditorHelper.Horizontal()) {
				EditorGUILayout.LabelField("Proxy names", GUILayout.Width(128), GUILayout.ExpandWidth(false));
				if (EditorHelper.MiniButton("Add", 64)) {
					TextPrompt textPrompt = EditorWindow.GetWindow<TextPrompt>();
					textPrompt.textLabel = "Name";
					textPrompt.textValidator = s => {
						if (string.IsNullOrEmpty(s)) return false;
						foreach (Proxy proxy in proxies) {
							string proxyName = proxy.name;
							if (proxyName.Equals(s)) return false;
						}

						return true;
					};
					textPrompt.onTextValueApplied = (s, o) => {
						proxies.Add(new Proxy() {
							id = ++proxyCounter,
							name = s
						});
						editor.Focus();
					};
					textPrompt.Show();
				}
			}

			int indexToRemove = -1;
			using (new EditorHelper.IndentPadding(20)) {
				for (int i = 0; i < proxies.Count; i++) {
					string oldName = proxies[i].name;
					using (new EditorHelper.Horizontal()) {
						EditorGUILayout.TextField("Id " + proxies[i].id, oldName, GUILayout.ExpandWidth(false), GUILayout.Width(256));
						if (EditorHelper.MiniButton("-")) {
							indexToRemove = i;
						}

						if (EditorHelper.MiniButton("Rename")) {
							TextPrompt textPrompt = EditorWindow.GetWindow<TextPrompt>();
							textPrompt.obj = new object[] {proxies, i, oldName};
							textPrompt.textLabel = "New name";
							textPrompt.text = oldName;
							textPrompt.textValidator = s => {
								if (string.IsNullOrEmpty(s)) return false;
								foreach (Proxy proxy in proxies) {
									string groupName = proxy.name;
									if (groupName.Equals(s)) return false;
								}

								return true;
							};
							textPrompt.onTextValueApplied = (s, o) => {
								List<Proxy> _proxies = (List<Proxy>) o[0];
								int index = (int) o[1];
								_proxies[index].name = s;

								editor.Focus();
							};
							textPrompt.Show();
						}
					}
				}
			}

			if (indexToRemove != -1) {
				proxies.RemoveAt(indexToRemove);
			}
		}

		public static Foldout LoadFoldout(ScriptableObject scriptableObject) {
			Foldout foldout = null;
			string prefKey = FoldoutPrefKey(scriptableObject);
			if (EditorPrefs.HasKey(prefKey)) {
				string json = EditorPrefs.GetString(prefKey);
				//DLog.Log("foldout json" + json);
				foldout = JsonMapper.ToObject<Foldout>(json);
			}

			return foldout;
		}

		public static void SaveFoldout(ScriptableObject scriptableObject, Foldout foldout) {
			string prefKey = FoldoutPrefKey(scriptableObject);
			EditorPrefs.SetString(prefKey, JsonMapper.ToJson(foldout));
		}

		private static string FoldoutPrefKey(ScriptableObject scriptableObject) {
			string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(scriptableObject));
			string prefKey = "ImpactProxyConfig.Editor.Foldout." + guid;
			return prefKey;
		}
#endif

		public List<string> ListAllPrefabPaths() {
			List<string> r = new List<string>();
			foreach (ImpactProxyEntry entry in entries) {
				foreach (string prefabPath in entry.ListAllPrefabPaths()) {
					r.Add(prefabPath);
				}
			}

			return r;
		}

		public void TemporaryStorePrefab(string path, GameObject prefab) {
			foreach (ImpactProxyEntry entry in entries) {
				entry.TemporaryStorePrefab(path, prefab);
			}
		}

		public GameObject FindPrefab(int proxyId, CharacterId characterId, int[] typesOfOngoingModifiers, int[] weaponVisualIds) {
			GameObject result = null;
			foreach (ImpactProxyEntry entry in entries) {
				if (entry.proxyId != proxyId) continue;

				result = entry.FindPrefab(characterId, typesOfOngoingModifiers, weaponVisualIds);
			}

			return result;
		}

		public List<string> FindPrefabPath(int proxyId) {
			List<string> result = new List<string>();
			foreach (ImpactProxyEntry entry in entries) {
				if (entry.proxyId != proxyId) continue;

				result = entry.ListAllPrefabPaths();
			}

			return result;
		}

		public class ConfigPath {
			private static string dir = "Config/Combat/Vfx";
			private static string fileName = "ImpactProxy";
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

#if UNITY_EDITOR
		public class Foldout {
			public Dictionary<string, bool> foldoutByPath = new Dictionary<string, bool>();
		}
#endif
	}
}