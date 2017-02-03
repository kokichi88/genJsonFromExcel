using System;
using System.Collections.Generic;
using Core.Commons;
using LitJson;
using MovementSystem.Components;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace Assets.Scripts.Config
{
	public partial class DungeonSpawnConfig
	{
		public class Spawn
		{
			public string monsterId;
			public int monsterLevel = 1;
			public Vector3 relativePosition;
			public Vector2 spawnLocation;
			public float xAxisAmplitude = 0;
			public int xAxisDensity = 1;
			public float spawnInterval;
			public int spawnCount = 1;
			public string facingDirection = Direction.Left.ToString();
			public string skillPool = "1";
			public List<Tracker> trackers = new List<Tracker>();
			public List<Action> actions = new List<Action>();
			private Vector2 wavePosition;

#if UNITY_EDITOR
			private static Dictionary<string, float> frustumHeightByMonsterId;
			private static Dictionary<string, Texture> textureByMonsterId;
			private GameObject go;
			private Challenge challenge;
			private bool isMonsterIdJustSet = false;
			private Material materialForIcon;
#endif

			public static int[] ParseSkillLevelPool(string pool)
			{
				string[] splits = pool.Split(',');
				int[] levels = new int[splits.Length];
				for (int kIndex = 0; kIndex < levels.Length; kIndex++)
				{
					levels[kIndex] = Convert.ToInt32(splits[kIndex]);
				}

				return levels;
			}

			public Direction ShowFacingDirection()
			{
				return (Direction) Enum.Parse(typeof(Direction), facingDirection);
			}

			public bool HasTrackerWithEventId(int eventId)
			{
				for (int i = 0; i < trackers.Count; i++)
				{
					if (trackers[i].eventId == eventId)
						return true;
				}

				return false;
			}

			public Vector3 ShowWorldPosition()
			{
				return (Vector3) wavePosition + relativePosition;
			}

			#region EDITOR

#if UNITY_EDITOR
			private bool foldTracker = false;
			private bool foldAction;

			public static Dictionary<string, Texture> ShowCacheOfTextureByMonsterId() {
				return textureByMonsterId;
			}
			
			public void OnGUI()
			{
				using (new EditorHelper.Horizontal()) {
					using (new EditorHelper.Vertical()) {
						using (new EditorHelper.Horizontal())
						{
							float labelWidth = EditorGUIUtility.labelWidth;
							float fieldWidth = EditorGUIUtility.fieldWidth;

							int oldIndex = Config.characterIds.IndexOf(monsterId);
							if (oldIndex == -1) oldIndex = 0;
							GUI.SetNextControlName(challenge.GetHashCode().ToString());
							EditorGUIUtility.fieldWidth = 0;
							EditorGUILayout.LabelField("Monster id", GUILayout.ExpandWidth(false), GUILayout.Width(labelWidth));
							GUIStyle gs = new GUIStyle(EditorStyles.popup);
							string charIdLabel = Config.characterIdsLabels[oldIndex];
							if (GUILayout.Button(charIdLabel.Substring(charIdLabel.LastIndexOf('/') + 1, charIdLabel.Length - charIdLabel.LastIndexOf('/') - 1), gs)) {
								GenericMenu menu = new GenericMenu();

								for (int i = 0; i < Config.characterIds.Count; i++) {
									string mId = Config.characterIds[i];
									string label = Config.characterIdsLabels[i];
									menu.AddItem(
										new GUIContent(label),
										mId.Equals(monsterId),
										data => {
											monsterId = (string) data;
											// DLog.Log(this.GetHashCode() + " monster id " + monsterId);
											isMonsterIdJustSet = true;
											// DLog.Log(monsterId);
										},
										mId
									);
								}

								menu.ShowAsContext();
								GUI.changed = false;
							}

							if (isMonsterIdJustSet) {
								isMonsterIdJustSet = false;
								GUI.changed = true;
							}

							EditorGUIUtility.labelWidth = 40;
							EditorGUIUtility.fieldWidth = 20;
							EditorGUI.indentLevel -= 2;
							GUI.SetNextControlName(challenge.GetHashCode().ToString() + 8);
							monsterLevel = EditorGUILayout.IntField("Level", monsterLevel, GUILayout.ExpandWidth(false));

							EditorGUIUtility.labelWidth = labelWidth;
							EditorGUIUtility.fieldWidth = fieldWidth;
							EditorGUI.indentLevel += 2;
						}

						Direction oldFacingDirection = ShowFacingDirection();
						Direction newFacingDirection = (Direction) EditorGUILayout.EnumPopup("Facing direction", oldFacingDirection);
						facingDirection = newFacingDirection.ToString();

						GUI.SetNextControlName(challenge.GetHashCode().ToString() + 9);
						string newSkillPool = EditorGUILayout.TextField("Spawn skill levels", skillPool);
						try
						{
							ParseSkillLevelPool(newSkillPool);
							skillPool = newSkillPool;
						}
						catch (Exception e)
						{
							DLog.LogException(e);
						}

						GUI.SetNextControlName(challenge.GetHashCode().ToString() + 1);
						spawnCount = EditorGUILayout.IntField("Spawn count", spawnCount);

						using (new EditorHelper.Horizontal())
						{
							GUI.SetNextControlName(challenge.GetHashCode().ToString() + 2);
							EditorGUILayout.LabelField("Spawn location", GUILayout.Width(EditorGUIUtility.labelWidth),
								GUILayout.ExpandWidth(false));

							float originalFieldWidth = EditorGUIUtility.fieldWidth;
							float originalLabelWidth = EditorGUIUtility.labelWidth;
							EditorGUIUtility.fieldWidth = 60;
							EditorGUIUtility.labelWidth = 20;
							EditorGUI.indentLevel -= 2;

							// GUILayout.Space(10);
							GUI.SetNextControlName(challenge.GetHashCode().ToString() + 3);
							float newX = EditorGUILayout.FloatField("X", relativePosition.x, GUILayout.ExpandWidth(false));
							GUILayout.Space(10);
							GUI.SetNextControlName(challenge.GetHashCode().ToString() + 4);
							float newY = EditorGUILayout.FloatField("Y", relativePosition.y, GUILayout.ExpandWidth(false));
							GUILayout.Space(10);
							GUI.SetNextControlName(challenge.GetHashCode().ToString() + 9);
							float newZ = EditorGUILayout.FloatField("Z", relativePosition.z, GUILayout.ExpandWidth(false));
							relativePosition = new Vector3(newX, newY, newZ);

							EditorGUIUtility.fieldWidth = originalFieldWidth;
							EditorGUIUtility.labelWidth = originalLabelWidth;
							EditorGUI.indentLevel += 2;
						}

						GUI.SetNextControlName(challenge.GetHashCode().ToString() + 5);
						xAxisAmplitude = EditorGUILayout.FloatField("X axis amplitude", xAxisAmplitude);

						GUI.SetNextControlName(challenge.GetHashCode().ToString() + 6);
						xAxisDensity = EditorGUILayout.IntField("X axis density", xAxisDensity);

						GUI.SetNextControlName(challenge.GetHashCode().ToString() + 7);
						spawnInterval = EditorGUILayout.FloatField("Spawn interval", spawnInterval);

						/*if (!Config.showSpawnerMode || !challenge.IsSpawner())
						{
						}*/
						DrawTrackers(trackers, ref foldTracker);

						DrawActions();

						EditorWindow inspectorWindow = null;
						foreach (EditorWindow editorWindow in Resources.FindObjectsOfTypeAll<EditorWindow>())
						{
							if (editorWindow.title.Contains("Inspector"))
							{
								inspectorWindow = editorWindow;
							}
						}

						if (GUI.GetNameOfFocusedControl().Contains(challenge.GetHashCode().ToString())
						    && EditorWindow.mouseOverWindow == inspectorWindow)
						{
							if (selectedObject != challenge)
							{
								SceneView.lastActiveSceneView.Focus();
								inspectorWindow.Focus();
							}

							selectedObject = challenge;
						}
					}

					DrawIconOfCharacterOfSpawner();
				}
			}

			private void DrawIconOfCharacterOfSpawner() {
				if (textureByMonsterId == null) {
					textureByMonsterId = new Dictionary<string, Texture>();
				}

				CharacterId characterId = new CharacterId(monsterId);
				Texture icon = null;
				bool found = textureByMonsterId.TryGetValue(characterId.ToString(), out icon);
				if (!found) {
					string pathToIcon = string.Format(Config.characterIconPathFormat, characterId.GroupId,
						characterId.SubId);
					icon = EditorGUIUtility.Load(pathToIcon) as Texture;
					textureByMonsterId[characterId.StringValue] = icon;
				}

				if (icon != null) {
					EditorGUILayout.LabelField("", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false),
						GUILayout.Height(89), GUILayout.Width(89));
					Rect lastRect = GUILayoutUtility.GetLastRect();
					GUIStyle gs = new GUIStyle(EditorStyles.label);
					gs.normal.background = Texture2D.grayTexture;
					Rect positionOfBackground = new Rect(lastRect.position - new Vector2(34, 0), lastRect.size);
					EditorGUI.LabelField(positionOfBackground, "", gs);
					EditorGUI.LabelField(positionOfBackground, "", gs);
					EditorGUI.LabelField(positionOfBackground, "", gs);

					if (materialForIcon == null) {
						materialForIcon = new Material(Shader.Find("Sprites/Default"));
					}
					EditorGUI.DrawPreviewTexture(lastRect, icon, materialForIcon);
				}
			}

			public void OnSceneGUI()
			{
				Vector3 newWorldPosition = Handles.DoPositionHandle(ShowWorldPosition(), Quaternion.identity);
				relativePosition = newWorldPosition - (Vector3) wavePosition;
				//Handles.Label(ShowWorldPosition(), "Spawn " + monsterId);

				/*float left = ShowWorldPosition().x - 1;
				float right = ShowWorldPosition().x + 1;
				float top = ShowWorldPosition().y + 4;
				float bottom = ShowWorldPosition().y - 1;
				Vector2 bottomLeft = new Vector2(left, bottom);
				Vector2 topLeft = new Vector2(left, top);
				Vector2 topRight = new Vector2(right, top);
				Vector2 bottomRight = new Vector2(right, bottom);
				Handles.DrawLine(bottomLeft, topLeft);
				Handles.DrawLine(topLeft, topRight);
				Handles.DrawLine(topRight, bottomRight);
				Handles.DrawLine(bottomRight, bottomLeft);*/
			}

			public void OnDrawGizmos(bool isChallengeSelected)
			{
				if (frustumHeightByMonsterId == null)
				{
					frustumHeightByMonsterId = new Dictionary<string, float>();
					frustumHeightByMonsterId["1100"] = 3;
					frustumHeightByMonsterId["4003"] = 3;
					frustumHeightByMonsterId["4004"] = 2;
					frustumHeightByMonsterId["4005"] = 2;
					frustumHeightByMonsterId["4006"] = 2;
					frustumHeightByMonsterId["4007"] = 1;
					frustumHeightByMonsterId["40010"] = 4;
				}

				if (textureByMonsterId == null)
				{
					textureByMonsterId = new Dictionary<string, Texture>();
				}


				CharacterId characterId;
				try
				{
					characterId = new CharacterId(monsterId);
					
				}
				catch (Exception e)
				{
					DLog.LogError(e);
					characterId = new CharacterId(1, 1);
				}
				
				Texture icon;
				bool found = textureByMonsterId.TryGetValue(characterId.ToString(), out icon);
				if (!found)
				{
					string pathToIcon = string.Format(Config.characterIconPathFormat, characterId.GroupId, characterId.SubId);
					icon = EditorGUIUtility.Load(pathToIcon) as Texture;
					textureByMonsterId[characterId.StringValue] = icon;
				}

//				Gizmos.DrawIcon((Vector3) ShowWorldPosition() + Vector3.up * 3.25f, pathToIcon, true);
				if (icon)
				{
					Gizmos.DrawGUITexture(
						new Rect(ShowWorldPosition() + new Vector3(-3.25f, 6), new Vector2(6, -6)),
						icon
					);
				}

				float height = 4;
				float newHeight = height;
				if (frustumHeightByMonsterId.TryGetValue(characterId.GroupId.ToString(), out newHeight))
				{
					height = newHeight;
				}

				Gizmos.color = Color.red;
				Matrix4x4 originalMatrix = Gizmos.matrix;
				Matrix4x4 rotationMatrix = Matrix4x4.Rotate(Quaternion.LookRotation(Vector3.up, Vector3.up));
				Matrix4x4 translationMatrix = Matrix4x4.Translate(ShowWorldPosition() + Vector3.up * height);
				Gizmos.matrix = Matrix4x4.identity * translationMatrix * rotationMatrix;
				Gizmos.DrawFrustum(Vector3.zero, 45, 1, 0, 1);
				Gizmos.matrix = originalMatrix;

				if (!isChallengeSelected) return;

				Gizmos.color = Color.green;
				Vector2 rectDimension = new Vector2(xAxisAmplitude, .5f);
				RectPivotPosition rpp = new RectPivotPosition(RectPivotPosition.PivotType.Center, Vector2.zero, rectDimension);
				Vector2 offset = rpp.RelativePositionOfPivotAt(RectPivotPosition.PivotType.BottomLeft);
				Gizmos.DrawCube(
					ShowWorldPosition() + new Vector3(offset.x * -1, offset.y),
					rectDimension
				);
			}

			public void SetChallenge(Challenge c)
			{
				challenge = c;
			}

			public static void DrawTrackers(List<Tracker> trackers, ref bool foldTracker)
			{
				EditorGUILayout.Space();
				using (new EditorHelper.Indent(-1))
				using (new EditorHelper.Box(false))
				using (new EditorHelper.Indent(2))
				{
					using (new EditorHelper.Horizontal())
					{
						GUIStyle gs = new GUIStyle(EditorStyles.foldout);
						string countAsPostfix = string.Empty;
						if (trackers.Count > 0)
						{
							countAsPostfix = " (" + trackers.Count + ")";
							GUIStyleState normal = new GUIStyleState();
							normal.textColor = new Color(226 / 255f, 131 / 255f, 34 / 255f, 1);
							gs.fontStyle = FontStyle.Bold;
							gs.fontSize = 13;
							gs.normal = normal;
							gs.focused = normal;
							gs.hover = normal;
							gs.active = normal;
							gs.onActive = normal;
							gs.onFocused = normal;
							gs.onHover = normal;
							gs.onNormal = normal;
						}

						bool foldout = GUILayout.Toggle(
							foldTracker,
							$"Tracker {countAsPostfix}", gs,
							GUILayout.ExpandWidth(false), GUILayout.Width(200)
						);
						
						if (GUILayout.Button("Add Tracker", GUILayout.Width(120)))
						{
							trackers.Add(GenerateTracker(TrackerName.TrackHp));
						}
						
						if (foldTracker != foldout)
						{
							Config.ResetCacheRectData();
						}
						foldTracker = foldout;
					}

					if (!foldTracker) return;
					
					Tracker removedTracker = null;
					for (int tIndex = 0; tIndex < trackers.Count; tIndex++)
					{
						Tracker tracker = trackers[tIndex];
						tracker.SetOrder(tIndex + 1);
						tracker.OnGUI();

						if (tracker.IsTrackerChanged)
						{
							tracker = GenerateTracker(tracker.ShowTrackerName());
							trackers[tIndex] = tracker;
						}

						if (tracker.IsRemoved)
						{
							removedTracker = tracker;
						}

						if (tracker.IsPasted)
						{
							trackers[tIndex] = copiedTracker.Clone();
							GUI.changed = true;
						}
					}

					if (removedTracker != null)
					{
						trackers.Remove(removedTracker);
						Config.ResetCacheRectData();
					}
				}
			}

			public static Tracker GenerateTracker(TrackerName trackerName)
			{
				Tracker tracker;
				switch (trackerName)
				{
					case TrackerName.TrackHp:
						tracker = new HpTracker();
						break;
					case TrackerName.TrackSkill:
						tracker = new SkillTracker();
						break;
					case TrackerName.TrackTime:
						tracker = new TimeTracker();
						break;
					default:
						throw new Exception(string.Format($"Not recognized tracker name of '{trackerName}'"));
				}

				return tracker;
			}

			private void DrawActions()
			{
				EditorGUILayout.Space();

				using (new EditorHelper.Indent(-1))
				using (new EditorHelper.Box(false))
				using (new EditorHelper.Indent(2))
				{
					using (new EditorHelper.Horizontal())
					{
						GUIStyle gs = new GUIStyle(EditorStyles.foldout);
						string countAsPostfix = string.Empty;
						if (actions.Count > 0)
						{
							countAsPostfix = " (" + actions.Count + ")";
							GUIStyleState normal = new GUIStyleState();
							normal.textColor = new Color(226 / 255f, 131 / 255f, 34 / 255f, 1);
							gs.fontStyle = FontStyle.Bold;
							gs.fontSize = 13;
							gs.normal = normal;
							gs.focused = normal;
							gs.hover = normal;
							gs.active = normal;
							gs.onActive = normal;
							gs.onFocused = normal;
							gs.onHover = normal;
							gs.onNormal = normal;
						}

						bool foldout = GUILayout.Toggle(
							foldAction,
							$"Cast Skill Action {countAsPostfix}", gs,
							GUILayout.ExpandWidth(false), GUILayout.Width(200)
						);

						if (GUILayout.Button("Add Cast Skill Action", GUILayout.Width(170)))
						{
							actions.Add(GenerateAction(ActionName.CastSkill));
						}

						if (foldAction != foldout)
						{
							Config.ResetCacheRectData();
						}
						foldAction = foldout;
					}

					if (!foldAction) return;

					Action removedAction = null;
					for (int i = 0; i < actions.Count; i++)
					{
						Action action = actions[i];
						action.SetOrder(i + 1);
						action.SetMonsterId(monsterId);
						action.SetChallenge(challenge);
						action.OnGUI();

						if (action.IsActionChanged)
						{
							action = GenerateAction(action.ShowActionName());
							actions[i] = action;
						}

						if (action.IsRemoved)
						{
							removedAction = action;
						}

						if (action.IsPasted)
						{
							actions[i] = copiedAction.Clone();
							GUI.changed = true;
						}
					}

					if (removedAction != null)
					{
						/*if (removedAction is CastSkillAction castSkillAction)
							challenge.OnRemoveCastSkill(castSkillAction);*/

						actions.Remove(removedAction);
						Config.ResetCacheRectData();
					}
				}
			}

			public static Action GenerateAction(ActionName actionName)
			{
				Action action;
				switch (actionName)
				{
					case ActionName.CastSkill:
						action = new CastSkillAction();
						break;
					default:
						throw new Exception(string.Format($"Not recognized action name of '{actionName}'"));
				}

				return action;
			}

			public void UpdateTrackerEventId(int oldEventId, int newEventId, string newSkillId)
			{
				for (int i = 0; i < trackers.Count; i++)
				{
					if (trackers[i].eventId == oldEventId)
					{
						trackers[i].eventId = newEventId;
						if (trackers[i] is SkillTracker skillTracker)
						{
							skillTracker.skillName = newSkillId;
						}
					}
				}
			}

			public void RemoveTrackerByEventId(int eventId)
			{
				int removedTrackerIndex = -1;
				for (int i = 0; i < trackers.Count; i++)
				{
					if (trackers[i].eventId == eventId)
					{
						removedTrackerIndex = i;
						break;
					}
				}

				if (removedTrackerIndex >= 0)
				{
					trackers.RemoveAt(removedTrackerIndex);
				}
			}

			public void ClearTrackers()
			{
				trackers.Clear();
			}
#endif

			#endregion

			public void SetWavePosition(Vector2 pos)
			{
				wavePosition = pos;
			}
		}
	}
}