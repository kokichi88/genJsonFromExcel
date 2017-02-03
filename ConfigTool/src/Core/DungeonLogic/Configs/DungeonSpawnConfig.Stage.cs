using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
	public partial class DungeonSpawnConfig
	{
		public enum StageType
		{
			Default,
			Random
		}
		
		public class Stage
		{
			public Vector2 position = Vector2.zero;
			public List<Wave> waves = new List<Wave>();
			public List<EndCondition> goals = new List<EndCondition>();
			public List<EndCondition> losingConditions = new List<EndCondition>();

			private int uid = -1;
			private bool isRemoved;
			private int stageOrder;
			private bool isDuplicated;

			public void UpdateWorldPositionOfChildren()
			{
				foreach (Wave wave in waves)
				{
					wave.SetStagePosition(position);
				}
			}

			public List<string> ListAllMonsterId()
			{
				List<string> l = new List<string>();
				for (int k = 0; k < waves.Count; k++)
				{
					l.AddRange(waves[k].ListAllMonsterId());
				}

				return l;
			}

			public int CountMonster()
			{
				int count = 0;
				for (int k = 0; k < waves.Count; k++)
				{
					count += waves[k].CountMonster();
				}

				return count;
			}

			public Dictionary<string, int> CountSpawnCount()
			{
				Dictionary<string, int> l = new Dictionary<string, int>();
				for (int kIndex = 0; kIndex < waves.Count; kIndex++)
				{
					Wave w = waves[kIndex];
					for (int mIndex = 0; mIndex < w.challenges.Count; mIndex++)
					{
						Challenge c = w.challenges[mIndex];
						if (!l.ContainsKey(c.spawn.monsterId))
						{
							l[c.spawn.monsterId] = 0;
						}

						l[c.spawn.monsterId] = l[c.spawn.monsterId] + 1;
					}
				}

				return l;
			}

			public Dictionary<string, Dictionary<int, int>> CountSpawnCountByMonsterIdAndLevel()
			{
				Dictionary<string, Dictionary<int, int>>
					l = new Dictionary<string, Dictionary<int, int>>();
				for (int kIndex = 0; kIndex < waves.Count; kIndex++)
				{
					Wave w = waves[kIndex];
					for (int mIndex = 0; mIndex < w.challenges.Count; mIndex++)
					{
						Challenge c = w.challenges[mIndex];
						if (!l.ContainsKey(c.spawn.monsterId))
						{
							l[c.spawn.monsterId] = new Dictionary<int, int>();
							l[c.spawn.monsterId][c.spawn.monsterLevel] = 0;
						}

						if (!l[c.spawn.monsterId].ContainsKey(c.spawn.monsterLevel))
						{
							l[c.spawn.monsterId].Add(c.spawn.monsterLevel, 0);
						}

						l[c.spawn.monsterId][c.spawn.monsterLevel] =
							l[c.spawn.monsterId][c.spawn.monsterLevel] + 1;
					}
				}

				return l;
			}

			public Dictionary<string, HashSet<int>> ListLevelsByMonsterId()
			{
				Dictionary<string, HashSet<int>> l = new Dictionary<string, HashSet<int>>();
				for (int kIndex = 0; kIndex < waves.Count; kIndex++)
				{
					Wave w = waves[kIndex];
					for (int mIndex = 0; mIndex < w.challenges.Count; mIndex++)
					{
						Challenge c = w.challenges[mIndex];
						if (!l.ContainsKey(c.spawn.monsterId))
						{
							l[c.spawn.monsterId] = new HashSet<int>();
						}

						l[c.spawn.monsterId].Add(c.spawn.monsterLevel);
					}
				}

				return l;
			}

			#region EDITOR

#if UNITY_EDITOR
			public virtual void OnGUI()
			{
				if (uid < 1)
				{
					uid = Config.idBag.GenerateId();
				}

				if (Config.IsOutOfScreen(uid, out float height))
				{
					GUILayout.Space(height);
					return;
				}
				
				using (new EditorHelper.Vertical())
				{
					using (new EditorHelper.Horizontal())
					{
						bool existed = Config.stageFold.ContainsKey(this);
						if (!existed)
						{
							Config.stageFold[this] = true;
						}

						bool foldout = Config.stageFold[this];
						GUIStyle gs = new GUIStyle(EditorStyles.foldout);
						gs.fontStyle = FontStyle.Bold;
						gs.fontSize = 13;
						bool isChildSelected = false;
						foreach (Wave wave in waves)
						{
							foreach (Challenge challenge in wave.challenges)
							{
								if (challenge == selectedObject)
								{
									isChildSelected = true;
								}
							}
						}

						if (isChildSelected)
						{
							gs.normal.textColor = Color.green;
							gs.onNormal.textColor = Color.green;
							gs.onActive.textColor = Color.green;
						}

						string postfixCount = waves.Count > 0 ? $"({waves.Count})" : string.Empty;
						foldout = GUILayout.Toggle(
							foldout, $"{GetStageName()} #{stageOrder} {postfixCount}", gs,
							GUILayout.ExpandWidth(false), GUILayout.Width(140)
						);
						
						if (Config.stageFold[this] != foldout)
						{
							Config.ResetCacheRectData();
						}
						Config.stageFold[this] = foldout;
						if (!foldout) return;

						isRemoved = GUILayout.Button("Remove", GUILayout.Width(80));
						isDuplicated = GUILayout.Button("Duplicate", GUILayout.Width(100));
						DrawAddFunctions();
					}

					using (new EditorHelper.Indent(1))
					{
						position = EditorGUILayout.Vector2Field("Position", position);
					}

					DrawWaves();

					DrawWinConditions();

					DrawLoseConditions();
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
				//Handles.Label(position, "Stage #" + stageOrder);

				bool existed = Config.stageFold.ContainsKey(this);
				if (!existed)
				{
					Config.stageFold[this] = true;
				}

				bool foldout = Config.stageFold[this];
				if (!foldout) return;

				for (int kIndex = 0; kIndex < waves.Count; kIndex++)
				{
					waves[kIndex].OnSceneGUI();
				}

				if (selectedObject == this)
				{
					position = Handles.DoPositionHandle(position, Quaternion.identity);

					foreach (Wave wave in waves)
					{
						wave.SetStagePosition(position);
					}
				}

				foreach (EndCondition condition in goals)
				{
					condition.OnSceneGUI();
				}

				foreach (EndCondition condition in losingConditions)
				{
					condition.OnSceneGUI();
				}
			}

			public void OnDrawGizmos(GameObject o)
			{
				Gizmos.color = new Color(255f / 255f, 140f / 255f, 80f / 255f);
				Gizmos.DrawSphere(position, 1);

				bool existed = Config.stageFold.ContainsKey(this);
				if (!existed)
				{
					Config.stageFold[this] = true;
				}

				bool foldout = Config.stageFold[this];
				if (!foldout) return;

				foreach (Wave wave in waves)
				{
					wave.OnDrawGizmos(o);
				}

				foreach (EndCondition condition in goals)
				{
					condition.OnDrawGizmos(o);
				}

				foreach (EndCondition condition in losingConditions)
				{
					condition.OnDrawGizmos(o);
				}
			}

			private EndCondition GenerateEndCondition(EndConditionName conditionName)
			{
				EndCondition condition;
				switch (conditionName)
				{
					case EndConditionName.HeroDeath:
						condition = new HeroDeathCondition();
						break;
					case EndConditionName.KillAllEnemy:
						condition = new KillAllEnemyCondition();
						break;
					case EndConditionName.ReachCheckpoint:
						condition = new ReachCheckpointCondition();
						break;
					case EndConditionName.CountdownTime:
						condition = new CountdownTimeCondition();
						break;
					case EndConditionName.TotalBattleTime:
						condition = new TotalBattleTimeCondition();
						break;
					default:
						throw new Exception(string.Format(
							"Not recognized end condition name of '{0}'", conditionName
						));
				}

				return condition;
			}

			protected virtual void DrawAddFunctions()
			{
				DrawAddWave();
				DrawAddGoal();
				DrawAddLose();
			}

			private void DrawAddWave()
			{
				if (GUILayout.Button("Add Wave", GUILayout.Width(80)))
				{
					Wave wave = new Wave();
					waves.Add(wave);
				}
			}

			protected void DrawAddGoal()
			{
				if (GUILayout.Button("Add Goal", GUILayout.Width(80)))
				{
					EndCondition condition = new KillAllEnemyCondition();
					goals.Add(condition);
				}
			}

			protected void DrawAddLose()
			{
				if (GUILayout.Button("Add Lose", GUILayout.Width(80)))
				{
					EndCondition condition = new HeroDeathCondition();
					losingConditions.Add(condition);
				}
			}

			private void DrawWaves()
			{
				foreach (Wave wave in waves)
				{
					wave.SetStagePosition(position);
				}

				using (new EditorHelper.Indent(1))
				{
					Wave removedWave = null;
					for (int kIndex = 0; kIndex < waves.Count; kIndex++)
					{
						Wave wave = waves[kIndex];
						wave.SetOrder(kIndex + 1);
						wave.OnGUI();

						if (wave.IsRemoved)
						{
							removedWave = wave;
						}

						if (wave.IsPasted && copiedWave != null)
						{
							waves[kIndex] = new JsonDeserializationOperation(
								new JsonSerializationOperation(copiedWave).Act()
							).Act<Wave>();
							GUI.changed = true;
						}
					}

					if (removedWave != null)
					{
						waves.Remove(removedWave);
						Config.ResetCacheRectData();
					}
				}
			}

			private void DrawWinConditions()
			{
				using (new EditorHelper.IndentPadding(10))
				{
					if (goals.Count > 0)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Win Stage Conditions");
					}
				}

				using (new EditorHelper.Indent(1))
				{
					EndCondition removedCondition = null;
					for (int gIndex = 0; gIndex < goals.Count; gIndex++)
					{
						EndCondition condition = goals[gIndex];
						condition.SetOrder(gIndex + 1);
						condition.OnGUI();

						if (condition.IsConditionChanged)
						{
							condition = GenerateEndCondition(condition.ShowConditionName());

							goals[gIndex] = condition;
						}

						if (condition.IsRemoved)
						{
							removedCondition = condition;
						}

						if (condition.IsPasted && copiedCondition != null)
						{
							goals[gIndex] = copiedCondition.Clone();
						}
					}

					if (removedCondition != null)
					{
						goals.Remove(removedCondition);
						Config.ResetCacheRectData();
					}
				}
			}

			private void DrawLoseConditions()
			{
				using (new EditorHelper.IndentPadding(10))
				{
					if (losingConditions.Count > 0)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Lose Stage Conditions");
					}
				}

				using (new EditorHelper.Indent(1))
				{
					EndCondition removedCondition = null;
					for (int gIndex = 0; gIndex < losingConditions.Count; gIndex++)
					{
						EndCondition condition = losingConditions[gIndex];
						condition.SetOrder(gIndex + 1);
						condition.OnGUI();

						if (condition.IsConditionChanged)
						{
							condition = GenerateEndCondition(condition.ShowConditionName());

							losingConditions[gIndex] = condition;
						}

						if (condition.IsRemoved)
						{
							removedCondition = condition;
						}

						if (condition.IsPasted && copiedCondition != null)
						{
							losingConditions[gIndex] = copiedCondition.Clone();
						}
					}

					if (removedCondition != null)
					{
						losingConditions.Remove(removedCondition);
						Config.ResetCacheRectData();
					}
				}
			}

			protected virtual string GetStageName()
			{
				return "Stage";
			}

			[JsonIgnore]
			public bool IsRemoved
			{
				get { return isRemoved; }
			}

			public void SetOrder(int order)
			{
				stageOrder = order;
			}

			[JsonIgnore]
			public int StageOrder
			{
				get { return stageOrder; }
			}

			[JsonIgnore]
			public bool IsDuplicated
			{
				get { return isDuplicated; }
			}
			
			protected T Clone<T>()
			{
				return new JsonDeserializationOperation(new JsonSerializationOperation(this).Act()).Act<T>();
			}

			public virtual Stage Clone()
			{
				return Clone<Stage>();
			}
#endif

			#endregion
		}
		
		public class RandomStage : Stage
		{
			public List<StagePreset> stagePresets = new List<StagePreset>();

			#region EDITOR

#if UNITY_EDITOR
			public override void OnGUI()
			{
				base.OnGUI();
				
				DrawPresets();
			}
			
			protected override void DrawAddFunctions()
			{
				DrawAddPreset();
				DrawAddGoal();
				DrawAddLose();
			}

			private void DrawAddPreset()
			{
				if (GUILayout.Button("Add Preset", GUILayout.Width(80)))
				{
					stagePresets.Add(new StagePreset());
				}
			}

			private void DrawPresets()
			{
				using (new EditorHelper.IndentPadding(10))
				{
					if (stagePresets.Count > 0)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("Stage Presets Pool");
					}
				}

				using (new EditorHelper.Indent(1))
				{
					StagePreset removedPreset = null;
					for (int i = 0; i < stagePresets.Count; i++)
					{
						StagePreset preset = stagePresets[i];
						preset.SetOrder(i + 1);
						preset.OnGUI();
						
						if (preset.IsRemoved)
						{
							removedPreset = preset;
						}

						if (preset.IsPasted && copiedStagePreset != null)
						{
							stagePresets[i] = copiedStagePreset.Clone();
						}
					}

					if (removedPreset != null)
					{
						stagePresets.Remove(removedPreset);
					}
				}
			}

			protected override string GetStageName()
			{
				return "Random Stage";
			}

			public override Stage Clone()
			{
				return Clone<RandomStage>();
			}
#endif

			#endregion
		}
		
		public class StagePreset
		{
			public string name;

			#region EDITOR

#if UNITY_EDITOR
			private bool isRemoved;
			private int order;
			private bool isDuplicated;
			
			public void OnGUI()
			{
				using (new EditorHelper.Box(true, 10))
				{
					using (new EditorHelper.Horizontal())
					{
						int index = Config.stagePresetNames.IndexOf(name);
						if (index == -1) index = 0;
						EditorGUIUtility.fieldWidth = 170;
						index = EditorGUILayout.Popup($"Preset #{order}", index,
							Config.stagePresetNames.ToArray());
						name = Config.stagePresetNames[index];

						if (GUILayout.Button("Copy", GUILayout.Width(50)))
						{
							copiedStagePreset = this;
						}
						IsPasted = GUILayout.Button("Paste", GUILayout.Width(50));
						IsRemoved = GUILayout.Button("X", GUILayout.Width(24));
					}
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

			public StagePreset Clone()
			{
				return Clone<StagePreset>();
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