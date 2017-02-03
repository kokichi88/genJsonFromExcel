using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Config;
using JsonConfig;
using JsonConfig.Model;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Editor;
using Object = UnityEngine.Object;

namespace Core.DungeonLogic.Configs.Editor {
	[CustomEditor(typeof(DungeonSpawnConfig))]
	public class DungeonSpawnConfigEditor : UnityEditor.Editor {
		private bool changed;
		private bool isChangedRecognized;
		private int serializationDelayInUpdateCount = 30;
		private int updateCounter;
		private MonsterConfig monsterConfig;

		private bool isEnabled;
		private bool isDisposed;
		// private EditorHelper.ScrollView.ScrollPosition scrollPosition = new EditorHelper.ScrollView.ScrollPosition();
		// private Rect contentRect;

		// private void OnEnable()
		// {
		// 	isDisposed = false;
		// 	
		// 	if (!isEnabled && !System.Environment.StackTrace.Contains("Importer"))
		// 	{
		// 		isEnabled = true;
		// 		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		// 		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		// 		DLog.Log($"Enabled {target.name}");
		// 		
		// 		DungeonSpawnConfig dci = (DungeonSpawnConfig) target;
		// 		dci.configObjectForEditor = null;
		// 	}
		// }
		
		private void OnDisable()
		{
			// Dispose();
		}

		private void OnDestroy()
		{
			// Dispose();
		}

		private void Dispose()
		{
			isEnabled = false;
			if(!isDisposed)
			{
				isDisposed = true;
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				DLog.Log($"Disposed {target.name}");
				
				ForceSave();
			}
		}

		private void OnPlayModeStateChanged(PlayModeStateChange obj)
		{
			if (EditorApplication.isPlayingOrWillChangePlaymode)
			{
				EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				ForceSave();
				// Selection.activeObject = null;
			}
		}

		public override void OnInspectorGUI() 
		{
			HandleHotkeys();

			if (monsterConfig == null) {
				ReadMonsterConfig();
			}

			DungeonSpawnConfig dungeonSpawnConfig = (DungeonSpawnConfig) target;
			//SerializedObject serializedObject = new SerializedObject(dci);

			using (new EditorHelper.Horizontal())
			{
				if (GUILayout.Button("Save"))
				{
					ForceSave();
				}
			
				if (GUILayout.Button("Refresh MonsterConfig"))
				{
					ReadMonsterConfig();
				}
			}

			// bool changed = Draw(dci);// serializedObject.FindProperty("config"));
			Draw(dungeonSpawnConfig);

			/*if (changed) {
				// DLog.Log(GetHashCode() + " OnInspectorGUI()");
				// serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(dci.gameObject);
				/*try {
					PrefabUtility.SavePrefabAsset(dci.gameObject);
				}
				catch (Exception e) {
					var prefab_root = PrefabUtility.FindPrefabRoot(dci.gameObject);
					var prefab_src = PrefabUtility.GetPrefabParent(prefab_root);
					if (prefab_src != null) {
						PrefabUtility.ReplacePrefab(prefab_root, prefab_src,  ReplacePrefabOptions.ConnectToPrefab);
					}
					string prefabPath_ = AssetDatabase.GetAssetPath(prefab_src);
					if (string.IsNullOrEmpty(prefabPath_)) {
						prefabPath_ = AssetDatabase.GetAssetPath(prefab_root);
					}

					if (string.IsNullOrEmpty(prefabPath_)) {
						prefabPath_ = AssetDatabase.GetAssetPath(dci.gameObject);
					}

					if (string.IsNullOrEmpty(prefabPath_)) {
						prefabPath_ = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(dci.gameObject).prefabAssetPath;
					}
					if (string.IsNullOrEmpty(prefabPath_)) {
						prefabPath_ = AssetDatabase.GetAssetPath(dci.gameObject.transform.root.gameObject);
					}

					PrefabUtility.SaveAsPrefabAsset(prefab_root, prefabPath_);
				}#1#
			}*/
		}

		private void ReadMonsterConfig() {
			string monsterConfigContent =
				((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/MonsterConfig.txt")).text;
			monsterConfig = JsonMapper.ToObject<MonsterConfig>(monsterConfigContent);
			
			string staticMonsterConfigContent =
				((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/StaticMonsterConfig.txt")).text;
			StaticMonsterConfig staticMonsterConfig =
				JsonMapper.ToObject<StaticMonsterConfig>(staticMonsterConfigContent);
			
			monsterConfig.Merge(staticMonsterConfig);
		}

		private void OnSceneGUI() {
			DungeonSpawnConfig dci = (DungeonSpawnConfig) target;
			SerializedObject serializedObject = new SerializedObject(dci);
			SerializedProperty configProp = serializedObject.FindProperty("config");

			if (configProp.arraySize < 1) {
				DungeonSpawnConfig.Config c = new DungeonSpawnConfig.Config();
				string[] s = new JsonSerializationOperation(c).ActToStringArray();
				configProp.arraySize = s.Length;
				for (int kIndex = 0; kIndex < s.Length; kIndex++) {
					configProp.GetArrayElementAtIndex(kIndex).stringValue = s[kIndex];
				}
			}

			if (dci.configObjectForEditor == null) {
				string[] s = new string[configProp.arraySize];
				for (int kIndex = 0; kIndex < s.Length; kIndex++) {
					s[kIndex] = configProp.GetArrayElementAtIndex(kIndex).stringValue;
				}

				string join = string.Join("", s);
				dci.configObjectForEditor = new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Config>();
				// DLog.Log("config object deserialized OnSceneGUI()");
			}

			EditorGUI.BeginChangeCheck();
			dci.configObjectForEditor.OnSceneGUI();
			if (EditorGUI.EndChangeCheck()) {
				UnityEditor.Editor[] ed = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
				for (int i = 0; i < ed.Length; i++)
				{
					ed[i].Repaint();
				}
			}

			serializedObject.ApplyModifiedProperties();

			bool isObjectSelected = dci.OnSceneGUI();
			if (isObjectSelected) {
				UnityEditor.Editor[] ed = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
				for (int i = 0; i < ed.Length; i++)
				{
					ed[i].Repaint();
				}
			}
		}

		private void Draw(DungeonSpawnConfig dungeonSpawnConfig)//, SerializedProperty configProp)
		{
			// if (configProp.arraySize < 1)
			// {
			// 	DungeonSpawnConfig.Config c = new DungeonSpawnConfig.Config();
			// 	string[] s = new JsonSerializationOperation(c).ActToStringArray();
			// 	configProp.arraySize = s.Length;
			// 	for (int kIndex = 0; kIndex < s.Length; kIndex++)
			// 	{
			// 		configProp.GetArrayElementAtIndex(kIndex).stringValue = s[kIndex];
			// 	}
			// }

			bool isUndoed = (Event.current.type == UnityEngine.EventType.ValidateCommand &&
			                 Event.current.commandName == "UndoRedoPerformed");
			if (dungeonSpawnConfig.configObjectForEditor == null || isUndoed)
			{
				/*string[] s = new string[configProp.arraySize];
				for (int kIndex = 0; kIndex < s.Length; kIndex++)
				{
					s[kIndex] = configProp.GetArrayElementAtIndex(kIndex).stringValue;
				}

				string join = string.Join("", s);
				dci.configObjectForEditor = new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Config>();*/
				DLog.Log($"config object deserialized OnInspectorGUI() - {target.name}");
				dungeonSpawnConfig.configObjectForEditor = dungeonSpawnConfig.DeserializeToObject();
			}
			
			dungeonSpawnConfig.configObjectForEditor.SetMonsterConfig(monsterConfig);
			dungeonSpawnConfig.configObjectForEditor.SetGameObject(dungeonSpawnConfig.gameObject);

			using (new EditorHelper.Horizontal())
			{
				if (GUILayout.Button("Refresh UI"))
				{
					DungeonSpawnConfig.Config.ReadCharacterIdsFromFolderStructure(monsterConfig);
					dungeonSpawnConfig.configObjectForEditor.ReadStagePresetNamesFromCurrentFolder();
					DungeonSpawnConfig.Config.ResetCacheRectData();
				}

				if (GUILayout.Button("Collapse All Challenges"))
				{
					List<DungeonSpawnConfig.Challenge> keys = new List<DungeonSpawnConfig.Challenge>(DungeonSpawnConfig.Config.challengeFold.Keys);
					foreach (DungeonSpawnConfig.Challenge key in keys)
					{
						DungeonSpawnConfig.Config.challengeFold[key] = false;
					}
					
					DungeonSpawnConfig.Config.ResetCacheRectData();
				}
				
				if (GUILayout.Button("Collapse All Waves"))
				{
					List<DungeonSpawnConfig.Wave> keys = new List<DungeonSpawnConfig.Wave>(DungeonSpawnConfig.Config.waveFold.Keys);
					foreach (DungeonSpawnConfig.Wave key in keys)
					{
						DungeonSpawnConfig.Config.waveFold[key] = false;
					}
					
					DungeonSpawnConfig.Config.ResetCacheRectData();
				}
			}
			
			CheckLayoutInfo();

			// scrollPosition.ScrollPos = GUI.BeginScrollView(new Rect(contentRect.x - 20, contentRect.y, contentRect.width + 20, 775), scrollPosition.ScrollPos, contentRect);
			// EditorGUILayout.BeginVertical();

			EditorGUI.BeginChangeCheck();
			dungeonSpawnConfig.configObjectForEditor.OnGUI();
			if (EditorGUI.EndChangeCheck())
			{
				EditorUtility.SetDirty(dungeonSpawnConfig);
			}
			
			// EditorGUILayout.EndVertical();
			// if( Event.current.type == EventType.Repaint )
			// {
			// 	contentRect = GUILayoutUtility.GetLastRect();
			// }
			//
			// GUI.EndScrollView();

			// return true;
				/*bool dataChanged = false;

				string[] dataFromEditorObj = new JsonSerializationOperation(dungeonSpawnConfig.configObjectForEditor).ActToStringArray();
				// DLog.Log(new JsonSerializationOperation(dci.configObjectForEditor).Act());

				string[] dataFromSerializedObj = new string[configProp.arraySize];
				for (int kIndex = 0; kIndex < dataFromSerializedObj.Length; kIndex++)
				{
					dataFromSerializedObj[kIndex] = configProp.GetArrayElementAtIndex(kIndex).stringValue;
				}

				string join = string.Join("", dataFromSerializedObj);
				DungeonSpawnConfig.Config c = new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Config>();
				dataFromSerializedObj = new JsonSerializationOperation(c).ActToStringArray();

				if (dataFromEditorObj.Length != dataFromSerializedObj.Length)
				{
					dataChanged = true;
				}

				for (int i = 0; i < dataFromEditorObj.Length; i++)
				{
					if (!dataFromEditorObj[i].Equals(dataFromSerializedObj[i]))
					{
						dataChanged = true;
						break;
					}
				}

				if (dataChanged)
				{
					configProp.ClearArray();
					configProp.arraySize = dataFromEditorObj.Length;
					for (int kIndex = 0; kIndex < dataFromEditorObj.Length; kIndex++)
					{
						configProp.GetArrayElementAtIndex(kIndex).stringValue = dataFromEditorObj[kIndex];
					}
				}

				// DLog.Log("debug: populate props");
				return dataChanged;*/
			// }
			// return false;
		}

		/*public void PopulateUsingDataReadFromDisk(string[] config, DungeonSpawnConfig.Config configObjectForEditor) {
			/*var prefabRoot = PrefabUtility.FindPrefabRoot(((DungeonSpawnConfig)target).gameObject);
			var prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
			string prefabPath = AssetDatabase.GetAssetPath(prefabParent);
			if (string.IsNullOrEmpty(prefabPath)) {
				prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
			}
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			DungeonSpawnConfig dsc = prefab.GetComponent<DungeonSpawnConfig>();
			SerializedObject serializedObject = new SerializedObject(dsc);
			SerializedProperty configProp = serializedObject.FindProperty("config");
			string[] s = new string[configProp.arraySize];
			for (int kIndex = 0; kIndex < s.Length; kIndex++) {
				s[kIndex] = configProp.GetArrayElementAtIndex(kIndex).stringValue;
			}

			string join = string.Join("", s);
			dsc.configObjectForEditor = new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Config>();
			dsc.Deserialize();#1#
			//DLog.Log(new JsonSerializationOperation(dsc.configObject).Act());

			//DLog.Log(new JsonSerializationOperation(((DungeonSpawnConfig) target).configObject).Act());
			/*SerializedObject serializedObjectFromThisEditorInstance = new SerializedObject((DungeonSpawnConfig) target);
			SerializedProperty configPropFromThisEditorInstance = serializedObjectFromThisEditorInstance.FindProperty("config");
			configPropFromThisEditorInstance.ClearArray();
			configPropFromThisEditorInstance.arraySize = config.Length;
			for (int i = 0; i < config.Length; i++) {
				configPropFromThisEditorInstance.GetArrayElementAtIndex(i).stringValue = config[i];
			}
			serializedObjectFromThisEditorInstance.ApplyModifiedProperties();
			((DungeonSpawnConfig) target).configObject = null;
			((DungeonSpawnConfig) target).configObjectForEditor = null;#1#
			// ((DungeonSpawnConfig)target).configObjectForEditor = configObjectForEditor;
			PrefabUtility.RevertPrefabInstance(((DungeonSpawnConfig) target).gameObject);
		}*/

		private void CheckLayoutInfo()
		{
			EditorWindow focus = EditorWindow.focusedWindow;
			EditorWindow mouseOver = EditorWindow.mouseOverWindow;
			if (focus != null && (focus.ToString().Contains("UnityEditor.InspectorWindow")
			                      || mouseOver != null && (focus = mouseOver).ToString().Contains("UnityEditor.InspectorWindow")))
			{
				Type T = Type.GetType("UnityEditor.InspectorWindow,UnityEditor");
				if (T != null)
				{
					ScrollView scrollView =
						(ScrollView) T.GetField("m_ScrollView", BindingFlags.NonPublic | BindingFlags.Instance)
						              .GetValue(focus);
					DungeonSpawnConfig.Config.scrollPosition = scrollView.scrollOffset.y - 333;
					DungeonSpawnConfig.Config.windowHeight = scrollView.contentViewport.layout.height;
				}
			}
		}
		
		private bool debugged = false;
		private void HandleHotkeys()
		{
			Event e = Event.current;
			if (e.type == EventType.KeyDown)
			{
				/*if (e.control && e.keyCode == KeyCode.M)
				{
					DungeonSpawnConfig.Config.DebugRects();SWSS
				}

				if (e.control && e.keyCode == KeyCode.N)
				{
					debugged = true;
				}*/
				
				if (e.control && e.keyCode == KeyCode.S)
				{
					ForceSave();
				}
			}

			if (debugged && e.type == EventType.Repaint)
			{
				debugged = false;
				Rect rect = GUILayoutUtility.GetLastRect();
				DLog.Log($"yMin: {rect.yMin} | yMax: {rect.yMax} | height: {rect.height}");
			}
		}

		private void ForceSave()
		{
			DLog.Log($"Saving {target.name}");
			
			DungeonSpawnConfig dungeonSpawnConfig = (DungeonSpawnConfig) target;

			if (target == null || dungeonSpawnConfig == null || dungeonSpawnConfig.configObjectForEditor == null)
			{
				DLog.Log($"Save failed {target?.name}");
				return;
			}
			
			dungeonSpawnConfig.Serialize(dungeonSpawnConfig.configObjectForEditor);
			EditorUtility.SetDirty(dungeonSpawnConfig);

			DungeonSpawnConfigEditor[] editors = Resources.FindObjectsOfTypeAll<DungeonSpawnConfigEditor>();
			GameObject prefabRoot = PrefabUtility.FindPrefabRoot(dungeonSpawnConfig.gameObject);
			Object prefabParent = PrefabUtility.GetPrefabParent(prefabRoot);
			string prefabPath = AssetDatabase.GetAssetPath(prefabParent);
			if (string.IsNullOrEmpty(prefabPath))
			{
				prefabPath = AssetDatabase.GetAssetPath(prefabRoot);
			}

			if (string.IsNullOrEmpty(prefabPath))
			{
				prefabPath = AssetDatabase.GetAssetPath(dungeonSpawnConfig.gameObject);
			}

			if (string.IsNullOrEmpty(prefabPath))
			{
				prefabPath = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(dungeonSpawnConfig.gameObject)
				                        .prefabAssetPath;
			}

			foreach (DungeonSpawnConfigEditor editor in editors)
			{
				// DLog.Log("editor " + editor.GetHashCode());
				// editor.ResetTarget();
				if (editor == this) continue;
				DungeonSpawnConfig otherDungeonSpawnConfig = editor.target as DungeonSpawnConfig;
				if (otherDungeonSpawnConfig == null) continue;
				GameObject otherPrefabRoot = PrefabUtility.FindPrefabRoot(otherDungeonSpawnConfig.gameObject);
				Object otherPrefabParent = PrefabUtility.GetPrefabParent(otherPrefabRoot);
				string otherPrefabPath = AssetDatabase.GetAssetPath(otherPrefabParent);
				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherPrefabRoot);
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherDungeonSpawnConfig.gameObject);
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = UnityEditor.Experimental.SceneManagement.PrefabStageUtility
					                             .GetPrefabStage(otherDungeonSpawnConfig.gameObject).prefabAssetPath;
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherDungeonSpawnConfig.gameObject.transform.root.gameObject);
				}

				if (!otherPrefabPath.Equals(prefabPath))
				{
					// DLog.Log("editor of other dungeon spawn config detected: this " + prefabPath + " other " + otherPrefabPath);
					PrefabUtility.RevertPrefabInstance(otherDungeonSpawnConfig.gameObject);
					continue;
				}

				otherDungeonSpawnConfig.configObjectForEditor = ((DungeonSpawnConfig) target).configObjectForEditor;
				otherDungeonSpawnConfig.config = ((DungeonSpawnConfig) target).config;
			}

			foreach (DungeonSpawnConfig otherDsc in FindObjectsOfType<DungeonSpawnConfig>())
			{
				GameObject otherPrefabRoot = PrefabUtility.FindPrefabRoot(otherDsc.gameObject);
				Object otherPrefabParent = PrefabUtility.GetPrefabParent(otherPrefabRoot);
				string otherPrefabPath = AssetDatabase.GetAssetPath(otherPrefabParent);
				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherPrefabRoot);
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherDsc.gameObject);
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(otherDsc.gameObject)
					                             .prefabAssetPath;
				}

				if (string.IsNullOrEmpty(otherPrefabPath))
				{
					otherPrefabPath = AssetDatabase.GetAssetPath(otherDsc.gameObject.transform.root.gameObject);
				}

				if (!otherPrefabPath.Equals(prefabPath))
				{
					// DLog.Log("hierarchy editor of other dungeon spawn config detected: this " + prefabPath + " other " + otherPrefabPath);
					PrefabUtility.RevertPrefabInstance(otherDsc.gameObject);
					continue;
				}

				otherDsc.configObjectForEditor = ((DungeonSpawnConfig) target).configObjectForEditor;
				otherDsc.config = ((DungeonSpawnConfig) target).config;
			}
			
			DLog.Log($"Saved {target.name}");
		}
	}
}