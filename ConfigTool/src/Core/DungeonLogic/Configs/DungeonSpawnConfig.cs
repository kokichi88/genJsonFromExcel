#if UNITY_EDITOR
using UnityEditor;
using Utils.Editor;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Commons;
using JsonConfig.Model;
using LitJson;
using Ssar.Combat.Skills;
using UnityEngine;
using Utils;
using static Assets.Scripts.Config.DungeonSpawnConfig.Config;

namespace Assets.Scripts.Config {
	public partial class DungeonSpawnConfig : MonoBehaviour {
#if UNITY_EDITOR
		[NonSerialized]
		public Config configObjectForEditor;

		[NonSerialized]
		public static object selectedObject;

		[NonSerialized]
		public static List<Challenge> selectedChallenges = new List<Challenge>();

		[NonSerialized]
		public static Vector2 selectedChallengesPosition;
		
		private static Wave copiedWave;
		private static Challenge copiedChallenge;
		private static Tracker copiedTracker;
		private static EndCondition copiedCondition;
		private static StagePreset copiedStagePreset;
		private static Action copiedAction;
		private static ChallengePreset copiedChallengePreset;
#endif

		public string[] config = new string[0];

		[NonSerialized]
		public Config configObject;

		public void Deserialize()
		{
			configObject = DeserializeToObject();
		}

		public Config DeserializeToObject()
		{
			string json = string.Join("", config);
			return new JsonDeserializationOperation(json).Act<Config>();
		}

		#region EDITOR
#if UNITY_EDITOR
		public void Serialize(Config configObj)
		{
			config = new JsonSerializationOperation(configObj).ActToStringArray();
		}
		
		void OnDrawGizmos() {
			if(configObjectForEditor == null) return;

			configObjectForEditor.OnDrawGizmos(gameObject);
			if (selectedChallenges.Count > 0) {
				Selection.activeGameObject = gameObject;
			}
		}

		public bool OnSceneGUI() {
			SceneView scene = SceneView.currentDrawingSceneView;
			HandleHotKey();
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
			Event e = Event.current;

			int leftMouseButton = 0;
			bool leftMouseClick = e.type == EventType.MouseDown && e.button == leftMouseButton;
			bool shiftKeyDown = e.modifiers == EventModifiers.Shift;
			bool isObjectSelected = false;
			if (leftMouseClick) {
				Vector3 mousePos = e.mousePosition;
				float ppp = EditorGUIUtility.pixelsPerPoint;
				mousePos.y = scene.camera.pixelHeight - mousePos.y * ppp;
				mousePos.x *= ppp;

				Vector2 worldPosOfClick = scene.camera.ScreenToWorldPoint(mousePos);
				//Debug.Log("Left click at world pos: " + worldPosOfClick);
				if (configObjectForEditor != null) {
					float closestDistance = float.MaxValue;
					foreach (Stage stage in configObjectForEditor.stages) {
						float distance = Vector3.Distance(worldPosOfClick, stage.position);
						if (distance <= 1) {
							if (distance < closestDistance) {
								closestDistance = distance;
								selectedObject = stage;
								selectedChallenges.Clear();
								isObjectSelected = true;
							}
						}

						foreach (Wave wave in stage.waves) {
							distance = Vector3.Distance(worldPosOfClick, wave.ShowWorldPosition());
							if (distance <= 1) {
								if (distance < closestDistance) {
									closestDistance = distance;
									selectedObject = wave;
									selectedChallenges.Clear();
									isObjectSelected = true;
								}
							}

							foreach (Challenge challenge in wave.challenges) {
								Spawn spawn = challenge.spawn;
								distance = Vector3.Distance(worldPosOfClick, spawn.ShowWorldPosition());
								float left = spawn.ShowWorldPosition().x - 1;
								float right = spawn.ShowWorldPosition().x + 1;
								float top = spawn.ShowWorldPosition().y + 4;
								float bottom = spawn.ShowWorldPosition().y - 1;
//								Vector2 topLeft = new Vector2(top, left);
//								Vector2 topRight = new Vector2(top, right);
//								Vector2 bottomRight = new Vector2(bottom, right);
//								Vector2 bottomLeft = new Vector2(bottom, left);
//								Handles.DrawLine(topLeft, topRight);
//								Handles.DrawLine(topRight, bottomRight);
//								Handles.DrawLine(bottomRight, bottomLeft);
//								Handles.DrawLine(bottomLeft, topLeft);
								if (left < worldPosOfClick.x && worldPosOfClick.x < right && bottom < worldPosOfClick.y && worldPosOfClick.y < top) {
									if (distance < closestDistance) {
										closestDistance = distance;
										if (shiftKeyDown) {
//											Selection.SetActiveObjectWithContext(Selection.activeObject, Selection.activeObject);
											if (selectedObject != null
											    && selectedObject is Challenge
											    && !selectedChallenges.Contains(selectedObject)
											    && selectedChallenges.Count < 1) {
												selectedChallenges.Add((Challenge) selectedObject);
//												DLog.Log("Add " + selectedObject.GetHashCode() + " size: " + selectedChallenges.Count);
											}

											if (selectedChallenges.Contains(challenge)) {
												selectedChallenges.Remove(challenge);
//												DLog.Log("Remove " + challenge.GetHashCode() + " size: " + selectedChallenges.Count);
											}
											else {
												selectedChallenges.Add(challenge);
//												DLog.Log("Add " + challenge.GetHashCode() + " size: " + selectedChallenges.Count);
											}
										}
										else {
											selectedChallenges.Clear();
											selectedObject = challenge;
										}
										isObjectSelected = true;

										List<Challenge> unfoldChallenges = new List<Challenge>();
										foreach (KeyValuePair<Challenge,bool> pair in challengeFold) {
											unfoldChallenges.Add(pair.Key);
										}

										foreach (Challenge unfoldChallenge in unfoldChallenges) {
											challengeFold[unfoldChallenge] = false;
										}

										challengeFold[challenge] = true;
									}
								}
							}
						}

						foreach (EndCondition condition in stage.goals)
						{
							distance = Vector3.Distance(worldPosOfClick, condition.ShowWorldPosition());
							if (distance <= 1) {
								if (distance < closestDistance) {
									closestDistance = distance;
									selectedObject = condition;
									selectedChallenges.Clear();
									isObjectSelected = true;
								}
							}
						}
						
						foreach (EndCondition condition in stage.losingConditions)
						{
							distance = Vector3.Distance(worldPosOfClick, condition.ShowWorldPosition());
							if (distance <= 1) {
								if (distance < closestDistance) {
									closestDistance = distance;
									selectedObject = condition;
									selectedChallenges.Clear();
									isObjectSelected = true;
								}
							}
						}
					}

					foreach (Gate gate in configObjectForEditor.gates) {
						float distance = Vector3.Distance(worldPosOfClick, gate.location);
						if (distance <= 4) {
							if (distance < closestDistance) {
								closestDistance = distance;
								selectedObject = gate;
								selectedChallenges.Clear();
								isObjectSelected = true;
							}
						}
					}

					foreach (StageActivator activator in configObjectForEditor.stageActivators) {
						float distance = Vector3.Distance(worldPosOfClick, activator.location);
						if (distance <= 4) {
							if (distance < closestDistance) {
								closestDistance = distance;
								selectedObject = activator;
								selectedChallenges.Clear();
								isObjectSelected = true;
							}
						}
					}

					if (isObjectSelected) {
						e.Use();
					}
					else {
						selectedObject = null;
						selectedChallenges.Clear();
					}
				}
			}

			return isObjectSelected;
		}

		private void HandleHotKey() {
			Event e = Event.current;
//			DLog.Log("e.shift " + e.shift + " e.isKey " + e.isKey + " e.e.type == EventType.KeyDown " + (e.type == EventType.KeyDown) + " e.keyCode == KeyCode.D " + (e.keyCode == KeyCode.D));
			if (e.shift && e.isKey && e.type == EventType.KeyDown) {
				if (e.keyCode == KeyCode.D) {
					DLog.Log("Shift + D -> Duplicate");
					if (selectedObject != null && selectedObject is Challenge) {
						foreach (Stage stage in configObjectForEditor.stages) {
							foreach (Wave wave in stage.waves) {
								if (wave.challenges.Contains(selectedObject)) {
									Challenge newChallenge = new JsonDeserializationOperation(
										new JsonSerializationOperation(selectedObject).Act()
									).Act<Challenge>();
									selectedObject = newChallenge;
									wave.challenges.Add(newChallenge);
									GUI.changed = true;
									e.Use();
								}
							}
						}
					}
				}
				if (e.keyCode == KeyCode.Delete) {
					DLog.Log("Shift + DELETE -> Delete");
					if (selectedObject != null && selectedObject is Challenge) {
						foreach (Stage stage in configObjectForEditor.stages) {
							foreach (Wave wave in stage.waves) {
								if (wave.challenges.Contains(selectedObject)) {
									wave.challenges.Remove((Challenge) selectedObject);
									selectedObject = null;
									GUI.changed = true;
									e.Use();
								}
							}
						}
					}
				}
			}
		}
#endif
		#endregion
	}

	public partial class DungeonSpawnConfig {
		public class Config {
#if UNITY_EDITOR
			[JsonIgnore] public static IdBag idBag = new IdBag();
			[JsonIgnore]
			public static List<string> characterIds;
			[JsonIgnore]
			public static List<string> characterIdsLabels;
			[JsonIgnore]
			public static List<string> filterCharacterIds;
			[JsonIgnore]
			public static List<string> filterCharacterIdsLabels;
			[JsonIgnore]
			public static Dictionary<Stage, bool> stageFold = new Dictionary<Stage, bool>();
			[JsonIgnore]
			public static Dictionary<Wave, bool> waveFold = new Dictionary<Wave, bool>();
			[JsonIgnore]
			public static Dictionary<Challenge, bool> challengeFold = new Dictionary<Challenge, bool>();
			[JsonIgnore]
			public static Dictionary<Tracker, bool> trackerFold = new Dictionary<Tracker, bool>();
			[JsonIgnore]
			public static Dictionary<EndCondition, bool> conditionFold = new Dictionary<EndCondition, bool>();
			[JsonIgnore]
			public static Dictionary<Action, bool> actionFold = new Dictionary<Action, bool>();
			[JsonIgnore]
			public static Dictionary<ChallengePreset, bool> challengePresetFold = new Dictionary<ChallengePreset, bool>();
			[JsonIgnore]
			public static string characterIconPathFormat = "Assets/Gizmos/CharacterIcon/{0}_{1}_icon.png";
			[JsonIgnore] 
			public static List<string> stagePresetNames;
			[JsonIgnore] public static bool showSpawnerMode = true;
			[JsonIgnore] public static Dictionary<int, Rect> entryRects = new Dictionary<int, Rect>();
			[JsonIgnore] public static float scrollPosition = 0;
			[JsonIgnore] public static float windowHeight = 0;
			[JsonIgnore] public static string filterCharacterId = string.Empty;
#endif

			public Vector2 heroPosition;
			public List<Stage> stages = new List<Stage>();
			public List<Gate> gates = new List<Gate>();
			public List<StageActivator> stageActivators = new List<StageActivator>();

#if UNITY_EDITOR
			private EditorHelper.Ruler ruler = new EditorHelper.Ruler(new []{0f, 200f}, 5);
			private TimeStatistic timeStatistic;
			private MonsterConfig monsterConfig;
			private StageType stageType = StageType.Default;
			private GameObject gameObject;
			private bool isFilterCharacterIdJustSet = false;
#endif

			public List<string> ListAllMonsterId() {
				List<string> l = new List<string>();
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					l.AddRange(stages[kIndex].ListAllMonsterId());
				}

				return l;
			}

			public int CountMonster() {
				int count = 0;
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					count += stages[kIndex].CountMonster();
				}

				return count;
			}

			public Dictionary<string, int> ShowHighestSpawnCountForEachMonster() {
				Dictionary<string, int> highest = new Dictionary<string, int>();
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					Stage s = stages[kIndex];
					if (kIndex == 0) {
						highest = s.CountSpawnCount();
					}
					else {
						Dictionary<string, int> toCheck = s.CountSpawnCount();
						foreach (KeyValuePair<string,int> pairToCheck in toCheck) {
							if (!highest.ContainsKey(pairToCheck.Key)) {
								highest[pairToCheck.Key] = pairToCheck.Value;
							}
							else {
								int highestCount = highest[pairToCheck.Key];
								if (highestCount < pairToCheck.Value) {
									highest[pairToCheck.Key] = pairToCheck.Value;
								}
							}
						}
					}
				}

				return highest;
			}

			public Dictionary<string, Dictionary<int, int>> CountSpawnCountByMonsterIdAndLevel() {
				Dictionary<string, Dictionary<int, int>> l = new Dictionary<string, Dictionary<int, int>>();
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					Stage s = stages[kIndex];
					var spawnCountByMonsterIdAndLevel = s.CountSpawnCountByMonsterIdAndLevel();
					foreach (var outer in spawnCountByMonsterIdAndLevel) {
						if (!l.ContainsKey(outer.Key)) {
							l[outer.Key] = new Dictionary<int, int>();
						}

						var countByLevel = l[outer.Key];
						foreach (var inner in outer.Value) {
							if (!countByLevel.ContainsKey(inner.Key)) {
								countByLevel[inner.Key] = 0;
							}

							if (inner.Value > countByLevel[inner.Key]) {
								countByLevel[inner.Key] = inner.Value;
							}
						}
					}
				}

				return l;
			}

			public Dictionary<string, HashSet<int>> ListLevelsByMonsterId() {
				Dictionary<string, HashSet<int>> l = new Dictionary<string, HashSet<int>>();
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					Stage s = stages[kIndex];
					foreach (KeyValuePair<string,HashSet<int>> pair in s.ListLevelsByMonsterId()) {
						if (!l.ContainsKey(pair.Key)) {
							l[pair.Key] = new HashSet<int>();
						}

						foreach (int level in pair.Value) {
							l[pair.Key].Add(level);
						}
					}
				}

				return l;
			}

			#region EDITOR
#if UNITY_EDITOR

			public void OnGUI()
			{
				/*if (GUILayout.Button("Refresh"))
				{
					ReadCharacterIdsFromFolderStructure(monsterConfig);
					ReadStagePresetNamesFromCurrentFolder();
					ResetCacheRectData();
				}*/

				if (characterIds == null) // || characterIds.Count < 1)
				{
					ReadCharacterIdsFromFolderStructure(monsterConfig);
				}

				if (stagePresetNames == null) // || stagePresetNames.Count < 1)
				{
					ReadStagePresetNamesFromCurrentFolder();
				}

				// showSpawnerMode = EditorGUILayout.Toggle("Spawner Display Mode", showSpawnerMode);
				EditorGUILayout.Space();
				
				DrawTimeStatistic();
				
				EditorGUILayout.Space();
				
				DrawCharacterFilter();

				new Statistic(this).OnGUI();

				heroPosition = EditorGUILayout.Vector2Field("Hero position", heroPosition);
				
				DrawStages();

				DrawGates();

				DrawActivators();
			}

			private void DrawTimeStatistic()
			{
				if (timeStatistic == null)
				{
					timeStatistic = new TimeStatistic(this);
				}
				
				timeStatistic.OnGUI();
			}

			private void DrawCharacterFilter()
			{
				int index = filterCharacterIds.IndexOf(filterCharacterId);
				if (index == -1) index = 0;
				string charIdLabel = filterCharacterIdsLabels[index];

				using (new EditorHelper.Horizontal())
				{
					EditorGUIUtility.fieldWidth = 0;
					EditorGUILayout.LabelField("Filter Monster ID", GUILayout.ExpandWidth(false));
					GUIStyle gs = new GUIStyle(EditorStyles.popup);
					if (GUILayout.Button(charIdLabel.Substring(charIdLabel.LastIndexOf('/') + 1,
							charIdLabel.Length - charIdLabel.LastIndexOf('/') - 1), gs))
					{
						GenericMenu menu = new GenericMenu();
						
						for (int i = 0; i < filterCharacterIds.Count; i++)
						{
							string mId = filterCharacterIds[i];
							string label = filterCharacterIdsLabels[i];
							menu.AddItem(
								new GUIContent(label),
								mId.Equals(filterCharacterId),
								data =>
								{
									filterCharacterId = (string) data;
									isFilterCharacterIdJustSet = true;
								},
								mId
							);
						}

						menu.ShowAsContext();
					}
				}
				
				/*index = EditorGUILayout.Popup($"Filter Monster ID", index, filterCharacterIdsLabels.ToArray());
				string newCharacterId = filterCharacterIds[index];

				if (newCharacterId != filterCharacterId)
				{
					isMonsterIdJustSet = true;
					filterCharacterId = newCharacterId;
				}*/

				if (isFilterCharacterIdJustSet)
				{
					isFilterCharacterIdJustSet = false;
					ResetCacheRectData();
				}
			}

			private void DrawStages()
			{
				stageType = (StageType) EditorGUILayout.EnumPopup("Stage Type", stageType);
				if (GUILayout.Button("Add stage"))
				{
					Stage stage;
					switch (stageType)
					{
						case StageType.Random:
							stage = new RandomStage();
							break;
						default:
							stage = new Stage();
							break;
					}

					stages.Add(stage);
				}

				Stage removedStage = null;
				Stage duplicatedStage = null;
				for (int kIndex = 0; kIndex < stages.Count; kIndex++)
				{
					Stage stage = stages[kIndex];
					
					stage.SetOrder(kIndex + 1);
					stage.OnGUI();

					if (stage.IsRemoved)
					{
						removedStage = stage;
						break;
					}

					if (stage.IsDuplicated)
					{
						duplicatedStage = stage;
					}
				}

				if (removedStage != null)
				{
					stages.Remove(removedStage);
					ResetCacheRectData();
				}

				if (duplicatedStage != null)
				{
					stages.Add(duplicatedStage.Clone());
				}
			}
			
			private void DrawGates()
			{
				if (GUILayout.Button("Add Gate"))
				{
					if (gates.Count >= stages.Count - 1)
					{
						DLog.LogError(string.Format(
							"Cannot add gate because gate count is {0} and stage count is {1}",
							gates.Count, stages.Count
						));
						return;
					}

					Gate g = new Gate();
					gates.Add(g);
				}

				Gate removedGate = null;
				for (int kIndex = 0; kIndex < gates.Count; kIndex++)
				{
					Gate g = gates[kIndex];
					g.SetOrder(kIndex + 1);
					g.OnGUI();
					if (g.IsRemoved)
					{
						removedGate = g;
					}
				}

				if (removedGate != null)
				{
					gates.Remove(removedGate);
				}
			}

			private void DrawActivators()
			{
				if (GUILayout.Button("Add Stage activator"))
				{
					if (stageActivators.Count >= stages.Count - 1)
					{
						DLog.LogError(string.Format(
							"Cannot add stage activator because stage activator count is {0} and stage count is {1}",
							stageActivators.Count, stages.Count
						));
						return;
					}

					StageActivator sa = new StageActivator();
					stageActivators.Add(sa);
				}

				StageActivator removedSa = null;
				for (int kIndex = 0; kIndex < stageActivators.Count; kIndex++)
				{
					StageActivator sa = stageActivators[kIndex];
					sa.SetOrder(kIndex + 1);
					sa.OnGUI();
					if (sa.IsRemoved)
					{
						removedSa = sa;
					}
				}

				if (removedSa != null)
				{
					stageActivators.Remove(removedSa);
				}
			}

			public void OnSceneGUI() {
				ruler.OnSceneGUI();
				//heroPosition = Handles.DoPositionHandle(heroPosition, Quaternion.identity);
				//Handles.Label(heroPosition, "Hero");

				// return;
				for (int kIndex = 0; kIndex < stages.Count; kIndex++) {
					stages[kIndex].OnSceneGUI();
				}

				for (int kIndex = 0; kIndex < gates.Count; kIndex++) {
					gates[kIndex].OnSceneGUI();
				}

				for (int kIndex = 0; kIndex < stageActivators.Count; kIndex++) {
					stageActivators[kIndex].OnSceneGUI();
				}

//				DLog.Log("selectedChallenges.Count: " + selectedChallenges.Count);
				if (selectedChallenges.Count > 0) {
					Vector3 positionSum = Vector2.zero;
					foreach (Challenge challenge in selectedChallenges) {
						positionSum += challenge.spawn.ShowWorldPosition();
					}

					Vector2 handlePosition = positionSum / selectedChallenges.Count;
					handlePosition += Vector2.down * 4;
//					DLog.Log("handlePosition: " + handlePosition);
					selectedChallengesPosition = Handles.DoPositionHandle(handlePosition, Quaternion.identity);
					Vector3 translation = selectedChallengesPosition - handlePosition;
					foreach (Challenge challenge in selectedChallenges) {
						challenge.spawn.relativePosition += translation;
					}
				}
			}

			public void OnDrawGizmos(GameObject o) {
//				Gizmos.DrawIcon(heroPosition + Vector2.up * 2, string.Format(characterIconPathFormat, 1, 1), true);
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(heroPosition + Vector2.up, 1);

				foreach (Stage stage in stages) {
					stage.OnDrawGizmos(o);
				}

				foreach (Gate gate in gates) {
					gate.OnDrawGizmos(o);
				}

				foreach (StageActivator activator in stageActivators) {
					activator.OnDrawGizmos(o);
				}
			}

			public static void ReadCharacterIdsFromFolderStructure(MonsterConfig monsterConfig)
			{
				characterIds = new List<string>();
				characterIdsLabels = new List<string>();
				filterCharacterIds = new List<string>();
				filterCharacterIdsLabels = new List<string>();
				
				string path = Application.dataPath + "/" + ResourcesFile.RESOURCES + "/" + "Characters";
				List<CharacterId> characterIdsFromFolder = new List<CharacterId>();
				foreach (string groupPath in Directory.GetDirectories(path))
				{
					string groupDirName = Path.GetFileName(groupPath);
					string[] split = groupDirName.Split('_');
					if (split.Length < 2)
					{
						continue;
					}

					string groupId = split[1];
					foreach (string subPath in Directory.GetDirectories(groupPath))
					{
						string subDirName = Path.GetFileName(subPath);
						try
						{
							int v = Convert.ToInt32(subDirName);
						}
						catch (Exception e)
						{
							continue;
						}

						string subId = subDirName;
						string cid = string.Format("{0}_{1}", groupId, subId);
						characterIdsFromFolder.Add(new CharacterId(cid));
					}
				}

				characterIdsFromFolder.Sort((id1, id2) => { return id1.GroupId.CompareTo(id2.GroupId); });
				Dictionary<int, string> groupNameByGroupId = new Dictionary<int, string>();
				foreach (CharacterId cid in characterIdsFromFolder) {
					if (monsterConfig != null)
					{
						HeroConfig.BasicStats bs = null;
						bool found = monsterConfig.FindBasicStats(cid, out bs);
						if (found)
						{
							if (!groupNameByGroupId.ContainsKey(cid.GroupId)) {
								groupNameByGroupId[cid.GroupId] = bs.name;
							}
							characterIdsLabels.Add(cid.GroupId + " - " + groupNameByGroupId[cid.GroupId] + "/" + cid + " - " + bs.name);
						}
						else
						{
							characterIdsLabels.Add(cid.StringValue);
						}
					}

					characterIds.Add(cid.StringValue);
				}
				
				filterCharacterIds.Add(string.Empty);
				filterCharacterIds.AddRange(characterIds);
				filterCharacterIdsLabels.Add("None");
				filterCharacterIdsLabels.AddRange(characterIdsLabels);
			}

			public void ReadStagePresetNamesFromCurrentFolder()
			{
				stagePresetNames = new List<string>();
				GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
				string parentPath = AssetDatabase.GetAssetPath(prefab != null ? prefab : gameObject);
				if (string.IsNullOrEmpty(parentPath)) return;
				int prefabNameLength = gameObject.name.Length + ".prefab".Length;
				int prefixLength = "Assets".Length;
				string processedParentPath = parentPath.Substring(prefixLength, parentPath.Length - prefabNameLength - prefixLength);
				string presetFolderPath = $"{Application.dataPath}{processedParentPath}Preset";
				if (!Directory.Exists(presetFolderPath)) return;
				string[] filePaths = Directory.GetFiles(presetFolderPath);
				foreach (string filePath in filePaths)
				{
					string fileName = Path.GetFileNameWithoutExtension(filePath).Replace(".prefab", string.Empty);
					if (!stagePresetNames.Contains(fileName))
						stagePresetNames.Add(fileName);
				}
			}

			public void SetMonsterConfig(MonsterConfig mc) {
				monsterConfig = mc;
			}

			public void SetGameObject(GameObject gameObject)
			{
				this.gameObject = gameObject;
			}
			
			public static bool IsOutOfScreen(int uid, out float height)
			{
				if (entryRects.TryGetValue(uid, out Rect rect))
				{
					if (rect.yMax < scrollPosition || rect.yMin > (scrollPosition + windowHeight))
					{
						height = rect.height;
						return true;
					}
				}

				height = 0;
				return false;
			}

			public static void ResetCacheRectData()
			{
				entryRects.Clear();
			}

			public static void DebugRects()
			{
				DLog.Log($"Scroll Position: {scrollPosition} - Window Height: {windowHeight}");
				foreach (KeyValuePair<int,Rect> pair in entryRects)
				{
					DLog.Log($"UID: {pair.Key} - yMin: {pair.Value.yMin} | yMax: {pair.Value.yMax} | height: {pair.Value.height}");
				}
			}
#endif
			#endregion
		}

		public class StageActivator {
			public Vector2 location;

			private int order;
			private bool isRemoved;

			#region EDITOR
#if UNITY_EDITOR
			public void OnGUI() {
				using (new EditorHelper.Horizontal()) {
					EditorGUILayout.LabelField("StageActivator #" + order, GUILayout.ExpandWidth(false), GUILayout.Width(120));
					isRemoved = GUILayout.Button("Remove", GUILayout.ExpandWidth(false), GUILayout.Width(80));
				}

				location = EditorGUILayout.Vector2Field("Location", location);
			}

			public void OnSceneGUI() {
				//Handles.Label(location, "StageActivator #" + order);

				if (selectedObject != this) return;

				location = Handles.DoPositionHandle(location, Quaternion.identity);
			}

			public void OnDrawGizmos(GameObject o) {
				Gizmos.color = Color.white;
				Gizmos.DrawCube(location + Vector2.up * 0.5f, new Vector3(1, 1, 1));
			}

			public void SetOrder(int order) {
				this.order = order;
			}

			[JsonIgnore]
			public bool IsRemoved {
				get { return isRemoved; }
			}
#endif
			#endregion
		}

		public class Gate {
			public Vector2 location;
			public string prefabPath = string.Empty;

			private int order;
			private bool isRemoved;

			#region EDITOR
#if UNITY_EDITOR
			public void OnGUI() {
				using (new EditorHelper.Horizontal()) {
					EditorGUILayout.LabelField("Gate #" + order, GUILayout.ExpandWidth(false), GUILayout.Width(80));
					isRemoved = GUILayout.Button("Remove", GUILayout.ExpandWidth(false), GUILayout.Width(80));
				}

				prefabPath = new EditorHelper.PrefabDrawer().DrawFromPathString("Prefab", prefabPath);

				location = EditorGUILayout.Vector2Field("Location", location);
			}

			public void OnSceneGUI() {
				if (selectedObject != this) return;

				location = Handles.DoPositionHandle(location, Quaternion.identity);
				//Handles.Label(location, "Gate #" + order);
			}

			public void OnDrawGizmos(GameObject o) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube(location + Vector2.up * 3, new Vector3(1, 6, 1));
			}

			public void SetOrder(int order) {
				this.order = order;
			}

			[JsonIgnore]
			public bool IsRemoved {
				get { return isRemoved; }
			}
#endif
			#endregion
		}

#if UNITY_EDITOR
		public class Statistic {
			private Config config;

			private int total;
			private List<int> countByStage = new List<int>();

			public Statistic(Config config) {
				this.config = config;
				Process();
			}

			public void OnGUI() {
				EditorGUILayout.LabelField("Total monster count: " + total);
				using (new EditorHelper.IndentPadding(10)) {
					for (int kIndex = 1; kIndex <= countByStage.Count; kIndex++) {
						EditorGUILayout.LabelField("Stage " + kIndex + ": " + countByStage[kIndex - 1]);
					}
				}
			}

			private void Process() {
				foreach (Stage s in config.stages) {
					int count = 0;
					foreach (Wave w in s.waves) {
						foreach (Challenge c in w.challenges) {
							total += c.spawn.spawnCount;
							count += c.spawn.spawnCount;
						}
					}
					countByStage.Add(count);
				}
			}
		}
		
		private class TimeStatistic
		{
			private Config config;

			private float totalTime;
			private DateTime lastUpdate;
			private List<float> waveMinTimes = new List<float>();
			private Dictionary<int, float> trackerMinWaitTimeByEventID = new Dictionary<int, float>();

			public TimeStatistic(Config config)
			{
				this.config = config;
				Process();
			}

			public void OnGUI()
			{
				using (new EditorHelper.Horizontal())
				{
					GUIStyle guiStyle = new GUIStyle(EditorStyles.label);
					guiStyle.fontStyle = FontStyle.Bold;
					EditorGUILayout.LabelField($"Min time: {totalTime}s", guiStyle, GUILayout.MaxWidth(150));
					EditorGUILayout.LabelField($"Last update: {lastUpdate}", GUILayout.MaxWidth(250));

					if (GUILayout.Button("Calculate Min Time"))
					{
						Process();
					}

					if (GUILayout.Button("Log", GUILayout.Width(50)))
					{
						for (int i = 0; i < waveMinTimes.Count; i++)
						{
							DLog.Log($"Wave {i+1}: {waveMinTimes[i]}s");
						}
						
						DLog.Log("-------------------");

						foreach (KeyValuePair<int,float> pair in trackerMinWaitTimeByEventID)
						{
							DLog.Log($"EventID: {pair.Key} | Time: {pair.Value}s");
						}

						DLog.Log("-------------------");
					}
				}
			}

			public void Process()
			{
				totalTime = 0;
				lastUpdate = DateTime.Now;
				waveMinTimes.Clear();
				trackerMinWaitTimeByEventID.Clear();
				
				foreach (Stage stage in config.stages)
				{
					foreach (Wave wave in stage.waves)
					{
						if (wave.IsDisabled()) continue;

						CalculateWaveMinTime(wave);
						float waveMinTime = CalculateWaveMinTime(wave);
						totalTime += waveMinTime;
						waveMinTimes.Add(waveMinTime);
					}
				}
			}

			private float CalculateWaveMinTime(Wave wave)
			{
				float waveMinTime = 0;
						
				foreach (Challenge challenge in wave.challenges)
				{
					if (challenge.IsDisabled()) continue;

					float challengeWaitTime = CalculateChallengeMinTime(challenge);

					waveMinTime = Mathf.Max(waveMinTime, challengeWaitTime);
				}

				return waveMinTime;
			}

			private float CalculateChallengeMinTime(Challenge challenge)
			{
				float challengeMinTime = 0;

				switch (challenge.trigger.ShowTriggerName())
				{
					case TriggerName.WaitForSeconds:
						challengeMinTime = (challenge.trigger as TimeTrigger).waitTime;
						break;
					case TriggerName.WaitForEvent:
						EventTrigger eventTrigger = challenge.trigger as EventTrigger;
						int eventId = eventTrigger.eventId;
						challengeMinTime = trackerMinWaitTimeByEventID.ContainsKey(eventId)
							? trackerMinWaitTimeByEventID[eventId]
							: 0;
						challengeMinTime += eventTrigger.waitTime;
						break;
				}
							
				if (challenge.IsSpawner())
				{
					CalculateTrackerMinWaitTime(challenge.spawn.trackers, challengeMinTime);

					float spawnerMinTime = 0;
					foreach (Action action in challenge.spawn.actions)
					{
						if (action.IsSpawnAction())
						{
							CastSkillAction spawnAction = action as CastSkillAction;
							float triggerTime = 0;
							float spawnTime = 0;

							switch (spawnAction.ShowTriggerCondition())
							{
								case ActionTriggerCondition.Time:
									triggerTime = spawnAction.waitTime;
									break;
								case ActionTriggerCondition.ByEvent:
									triggerTime = trackerMinWaitTimeByEventID.ContainsKey(spawnAction.eventId)
										? trackerMinWaitTimeByEventID[spawnAction.eventId]
										: 0;
									triggerTime += spawnAction.waitTime;
									break;
							}
							
							foreach (ChallengePreset challengePreset in spawnAction.challengePresets)
							{
								spawnTime = Mathf.Max(spawnTime,
									challengePreset.delay + (challengePreset.count - 1) * challengePreset.interval);
								
								// DLog.Log($"{challengeMinTime + action.waitTime + challengePreset.delay}s");
								CalculateTrackerMinWaitTime(challengePreset.trackers, challengeMinTime + action.waitTime + challengePreset.delay);
							}
							
							spawnerMinTime = Mathf.Max(triggerTime +spawnTime, spawnerMinTime);
						}
					}

					challengeMinTime += spawnerMinTime;
				}
				else
				{
					CalculateTrackerMinWaitTime(challenge.spawn.trackers, challengeMinTime);
				}

				return challengeMinTime;
			}

			private void CalculateTrackerMinWaitTime(List<Tracker> trackers, float parentMinTime)
			{
				foreach (Tracker tracker in trackers)
				{
					float trackerTriggerMinTime = parentMinTime;
					if (tracker is TimeTracker timeTracker)
					{
						trackerTriggerMinTime += timeTracker.timeThreshold;
					}

					if (!trackerMinWaitTimeByEventID.ContainsKey(tracker.eventId))
					{
						trackerMinWaitTimeByEventID.Add(tracker.eventId, Single.MaxValue);
					}

					trackerMinWaitTimeByEventID[tracker.eventId] =
						Mathf.Min(trackerTriggerMinTime, trackerMinWaitTimeByEventID[tracker.eventId]);
				}
			}
		}
		
		public class IdBag
		{
			private int currentID;

			public int GenerateId()
			{
				currentID++;
				// DLog.Log($"ID: {currentID}");
				return currentID;
			}
		}
#endif
	}
}
