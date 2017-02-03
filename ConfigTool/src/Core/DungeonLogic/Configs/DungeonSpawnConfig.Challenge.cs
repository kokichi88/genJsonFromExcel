using System;
using System.Collections.Generic;
using Core.Commons;
using Core.Skills;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
	public partial class DungeonSpawnConfig
	{
		public class Challenge
		{
			public bool enabled = true;
			public Trigger trigger = new DistanceTrigger();
			public Spawn spawn = new Spawn();
			// public List<ChallengePreset> relativePresets = new List<ChallengePreset>();

#if UNITY_EDITOR
			private int uid = -1;
			private bool isRemoved;
			private int order;
			private int lastOrder;
			private bool isPasted;
			private bool isDuplicated;
			private static Dictionary<string, Texture> textureByMonsterId;
#endif

			public bool IsSpawner()
			{
				for (int i = 0; i < spawn.actions.Count; i++)
				{
					if (spawn.actions[i].IsSpawnAction())
					{
						return true;
					}
				}

				return false;
				//return relativePresets != null && spawn.trackers.Count > 0 && relativePresets.Count > 0;
			}

			public bool IsDisabled()
			{
				return !enabled;
			}
			
			#region EDITOR
#if UNITY_EDITOR
			private Wave wave;

			public bool OnPreGUI()
			{
				if (uid < 1)//(!Config.challengeUID.TryGetValue(this, out uid))
				{
					uid = Config.idBag.GenerateId();
					// Config.challengeUID[this] = uid;
				}

				if (!string.IsNullOrEmpty(Config.filterCharacterId) && !spawn.monsterId.Equals(Config.filterCharacterId))
				{
					if (!IsSpawner() || !IsSpawnerContainMonsterId(Config.filterCharacterId))
					{
						return false;
					}
				}
				
				if (Config.IsOutOfScreen(uid, out float height))
				{
					GUILayout.Space(height);
					return false;
				}

				return true;
			}
			
			public void OnGUI()
			{
				bool isEventTrigger = trigger.ShowTriggerName() == TriggerName.WaitForEvent;
				GUIStyle guiStyle = isEventTrigger ? EditorHelper.BlackBox : EditorStyles.helpBox;

				using (new EditorHelper.Box(true, 10, guiStyle))
				{
					using (new EditorHelper.Indent(-2))
					{
						using (new EditorHelper.Horizontal())
						{
							enabled = EditorGUILayout.Toggle(enabled, GUILayout.ExpandWidth(false),
								GUILayout.Width(15));

							bool existed = Config.challengeFold.ContainsKey(this);
							if (!existed)
							{
								Config.challengeFold[this] = true;
							}

							bool foldout = Config.challengeFold[this];
							GUIStyle gs = new GUIStyle(EditorStyles.foldout);
							gs.fontStyle = FontStyle.Bold;
							gs.fontSize = 13;
							bool isSelected = this == selectedObject;

							if (isSelected)
							{
								gs.normal.textColor = Color.green;
								gs.onNormal.textColor = Color.green;
								gs.onActive.textColor = Color.green;
							}
							else
							{
								GUIStyleState normal = new GUIStyleState();
								normal.textColor = new Color(226 / 255f, 131 / 255f, 34 / 255f, 1);
								gs.normal = normal;
								gs.focused = normal;
								gs.hover = normal;
								gs.active = normal;
								gs.onActive = normal;
								gs.onFocused = normal;
								gs.onHover = normal;
								gs.onNormal = normal;
							}
							
							string prefix = IsSpawner() ? "Spawner #" : "Challenge #";
							string challengeName = isEventTrigger
								? $"{prefix}{order} - Event({(trigger as EventTrigger).eventId} | {(trigger as EventTrigger).waitTime}s)"
								: $"{prefix}{order}";
							foldout = GUILayout.Toggle(
								foldout, challengeName, gs,
								GUILayout.ExpandWidth(false), GUILayout.Width(140)
							);

							if (Config.challengeFold[this] != foldout)
							{
								Config.ResetCacheRectData();
							}
							Config.challengeFold[this] = foldout;
							if (!foldout) return;

							isRemoved = GUILayout.Button("Remove", GUILayout.Width(80));

							if (GUILayout.Button("Copy", GUILayout.Width(80)))
							{
								copiedChallenge = this;
								copiedChallenge.uid = -1;
							}

							isPasted = GUILayout.Button("Paste", GUILayout.Width(80));

							isDuplicated = GUILayout.Button("Duplicate", GUILayout.Width(100));

							using (new EditorHelper.DisabledGroup(order == 1))
							{
								IsMoveUp = GUILayout.Button("Up", GUILayout.Width(60));
							}
							
							using (new EditorHelper.DisabledGroup(order == lastOrder))
							{
								IsMoveDown = GUILayout.Button("Down", GUILayout.Width(60));
							}
						}
					}

					using (new EditorHelper.Indent(-1))
					{
						DrawTrigger();
						
						DrawSpawn();
					}
					
					// DrawSpawner();
				}
				
				if (Event.current.type == EventType.Repaint)
				{
					Config.entryRects[uid] = GUILayoutUtility.GetLastRect();
					/*if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
					{
						DLog.Log("Mouse over!");
					}*/
				}
			}

			public void OnSceneGUI()
			{
				bool existed = Config.challengeFold.ContainsKey(this);
				if (!existed)
				{
					Config.challengeFold[this] = true;
				}

				bool foldout = Config.challengeFold[this];
				Config.challengeFold[this] = foldout;

				bool isSelected = selectedObject == this || selectedChallenges.Contains(this);
				if (!isSelected) return;

				spawn.OnSceneGUI();
			}

			public void OnDrawGizmos(GameObject o)
			{
				bool existed = Config.challengeFold.ContainsKey(this);
				if (!existed)
				{
					Config.challengeFold[this] = true;
				}

				bool foldout = Config.challengeFold[this];
				Config.challengeFold[this] = foldout;

				bool isChallengeSelected = selectedObject == this;

				spawn.OnDrawGizmos(isChallengeSelected);

				if (!isChallengeSelected) return;

				trigger.SetSpawn(spawn);
				trigger.OnDrawGizmos(o);
			}

			[JsonIgnore]
			public bool IsRemoved
			{
				get { return isRemoved; }
				set { isRemoved = value; }
			}

			public void SetOrder(int order)
			{
				this.order = order;
			}

			public void SetLastOrder(int lastOrder)
			{
				this.lastOrder = lastOrder;
			}

			[JsonIgnore]
			public bool IsPasted
			{
				get { return isPasted; }
			}

			[JsonIgnore]
			public bool IsDuplicated
			{
				get { return isDuplicated; }
				set { isDuplicated = value; }
			}
			
			[JsonIgnore] public bool IsMoveUp { get; private set; }
			[JsonIgnore] public bool IsMoveDown { get; private set; }

			private void DrawTrigger()
			{
				GUIStyle gs = new GUIStyle(EditorStyles.largeLabel);
				gs.fontStyle = FontStyle.Bold;
				EditorGUILayout.LabelField("Trigger", gs);
				using (new EditorHelper.Indent(1))
				{
					trigger.OnGUI();

					if (trigger.IsTriggerChanged)
					{
						switch (trigger.ShowTriggerName())
						{
							case TriggerName.WaitForDistance:
								trigger = new DistanceTrigger();
								break;
							case TriggerName.WaitForSeconds:
								trigger = new TimeTrigger();
								break;
							case TriggerName.WaitForEvent:
								trigger = new EventTrigger();
								break;
							default:
								throw new Exception("Missing logic");
						}
					}
				}
			}

			private void DrawSpawn()
			{
				GUIStyle gs = new GUIStyle(EditorStyles.largeLabel);
				gs.fontStyle = FontStyle.Bold;
				// gs.fontSize = 12;
				EditorGUILayout.LabelField("Spawn", gs);
				using (new EditorHelper.Indent(1))
				{
					spawn.OnGUI();
				}
			}

			private bool IsSpawnerContainMonsterId(string monsterId)
			{
				for (int i = 0; i < spawn.actions.Count; i++)
				{
					if (spawn.actions[i].IsSpawnAction())
					{
						CastSkillAction castSkillAction = spawn.actions[i] as CastSkillAction;
						foreach (ChallengePreset challengePreset in castSkillAction.challengePresets)
						{
							if (!string.IsNullOrEmpty(challengePreset.path) && challengePreset.path.Contains(monsterId))
								return true;
						}
					}
				}

				return false;
			}
#endif

			#endregion
		}
		
		public class ChallengePreset
		{
			public bool enabled = true;
			public string path = string.Empty;
			public float delay;
			public float interval;
			public int count = 1;
			public List<Tracker> trackers = new List<Tracker>();
			
			// public int eventId;
			// public string skillId;

			public bool IsDisabled()
			{
				return !enabled;
			}

			#region EDITOR
#if UNITY_EDITOR
			private bool isRemoved;
			private int order;
			private bool isDuplicated;
			private bool foldTracker = false;
			private Material materialForIcon;
			private uint onGuiCount = 0;
			private ResourceRequest resourceRequest;
			private Texture icon = null;

			public void OnGUI()
			{
				onGuiCount++;
				using (new EditorHelper.Horizontal()) {
					using (new EditorHelper.Vertical()) {
						// using (new EditorHelper.Box(Config.showSpawnerMode, 0))
						{
							using (new EditorHelper.Indent(Config.showSpawnerMode ? -1 : -2))
							using (new EditorHelper.Horizontal())
							{
								enabled = EditorGUILayout.Toggle(enabled, GUILayout.ExpandWidth(false), GUILayout.Width(30));

								bool existed = Config.challengePresetFold.ContainsKey(this);
								if (!existed)
								{
									Config.challengePresetFold[this] = true;
								}

								bool foldout = Config.challengePresetFold[this];
								GUIStyle gs = new GUIStyle(EditorStyles.foldout);
								gs.fontStyle = FontStyle.Bold;
								GUIStyleState normal = new GUIStyleState();
								normal.textColor = new Color(20 / 255f, 225 / 255f, 226 / 255f, 1);
								gs.normal = normal;
								gs.focused = normal;
								gs.hover = normal;
								gs.active = normal;
								gs.onActive = normal;
								gs.onFocused = normal;
								gs.onHover = normal;
								gs.onNormal = normal;

								foldout = GUILayout.Toggle(
									foldout, $"Preset #{order}", gs,
									GUILayout.ExpandWidth(false), GUILayout.Width(140)
								);

								if (Config.challengePresetFold[this] != foldout)
								{
									Config.ResetCacheRectData();
								}
								Config.challengePresetFold[this] = foldout;
								if (!foldout) return;

								// EditorGUILayout.LabelField($"Preset #{order}", GUILayout.ExpandWidth(false), GUILayout.Width(120));
								IsRemoved = GUILayout.Button("Remove", GUILayout.ExpandWidth(false), GUILayout.Width(70));
							}

							path = new EditorHelper.ScriptableObjectDrawer<ChallengePresetAsset, ChallengePresetEditor>().DrawFromPathString("Asset", path);
							delay = EditorGUILayout.FloatField("Spawn at(s)", delay);
							count = EditorGUILayout.IntField("Spawn Count", count);
							interval = EditorGUILayout.FloatField("Spawn Interval", interval);

							delay = Mathf.Max(0, delay);
							count = Mathf.Max(1, count);
							interval = Mathf.Max(0, interval);

							Spawn.DrawTrackers(trackers, ref foldTracker);
						}
					}

					DrawIconOfCharacterOfSpawner();
				}
			}

			private void DrawIconOfCharacterOfSpawner() {
				if (Spawn.ShowCacheOfTextureByMonsterId() == null) {
					return;
				}

				if (onGuiCount % 13 == 0) {
					onGuiCount = 0;

					if (!string.IsNullOrEmpty(path)) {
						resourceRequest = Resources.LoadAsync<ChallengePresetAsset>(path);
					}
				}

				if (resourceRequest != null) {
					if (resourceRequest.isDone) {
						if (resourceRequest.asset == null) {
							resourceRequest = null;
							return;
						}
						ChallengePresetAsset challengePresetAsset = (ChallengePresetAsset) resourceRequest.asset;
						Challenge challenge = challengePresetAsset.DeserializeToObject();
						CharacterId characterId = new CharacterId(challenge.spawn.monsterId);
						bool found = Spawn.ShowCacheOfTextureByMonsterId().TryGetValue(characterId.ToString(), out icon);
						if (!found) {
							string pathToIcon = string.Format(Config.characterIconPathFormat, characterId.GroupId,
								characterId.SubId);
							icon = EditorGUIUtility.Load(pathToIcon) as Texture;
							Spawn.ShowCacheOfTextureByMonsterId()[characterId.StringValue] = icon;
						}
						resourceRequest = null;
					}
				}

				if (icon != null) {
					/*EditorGUILayout.LabelField("", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false),
						GUILayout.Height(89), GUILayout.Width(89));*/
					Rect lastRect = GUILayoutUtility.GetLastRect();
					Rect rectForIcon = new Rect(lastRect.position + new Vector2(lastRect.width, 0), new Vector2(89, 89));
					GUIStyle gs = new GUIStyle(EditorStyles.label);
					gs.normal.background = Texture2D.grayTexture;
					Rect positionOfBackground = new Rect(rectForIcon.position - new Vector2(34, 0), rectForIcon.size);
					EditorGUI.LabelField(positionOfBackground, "", gs);
					EditorGUI.LabelField(positionOfBackground, "", gs);
					EditorGUI.LabelField(positionOfBackground, "", gs);

					if (materialForIcon == null) {
						materialForIcon = new Material(Shader.Find("Sprites/Default"));
					}
					EditorGUI.DrawPreviewTexture(rectForIcon, icon, materialForIcon);
				}
			}

			public void SetOrder(int order)
			{
				this.order = order;
			}
			
			[JsonIgnore]
			public bool IsRemoved { get; set; }

			[JsonIgnore]
			public bool IsPasted { get; private set; }

			public ChallengePreset Clone()
			{
				return Clone<ChallengePreset>();
			}
			
			protected T Clone<T>()
			{
				return new JsonDeserializationOperation(new JsonSerializationOperation(this).Act()).Act<T>();
			}
#endif
			#endregion
		}
	}
}