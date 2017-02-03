using System.Collections.Generic;
using Core.Utils.Extensions;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
	public partial class DungeonSpawnConfig
	{
		public class Wave
		{
			public bool enabled = true;
			public Vector2 relativePosition;
			public List<Challenge> challenges = new List<Challenge>();

			private int uid = -1;
			private bool isRemoved;
			private int waveOrder;
			private bool isPasted;
			private Vector2 stagePosition;
			
			public bool IsDisabled()
			{
				return !enabled;
			}

			public Vector2 ShowWorldPosition()
			{
				return stagePosition + relativePosition;
			}

			public List<string> ListAllMonsterId()
			{
				List<string> l = new List<string>();
				for (int kIndex = 0; kIndex < challenges.Count; kIndex++)
				{
					l.Add(challenges[kIndex].spawn.monsterId);
				}

				return l;
			}

			public int CountMonster()
			{
				int count = 0;
				for (int kIndex = 0; kIndex < challenges.Count; kIndex++)
				{
					count += challenges[kIndex].spawn.spawnCount;
				}

				return count;
			}

			#region EDITOR

#if UNITY_EDITOR
			public void OnGUI()
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

				EditorGUILayout.Space();
				using (new EditorHelper.Box(true, 10))
				{
					using (new EditorHelper.Indent(-2))
					using (new EditorHelper.Horizontal())
					{
						enabled = EditorGUILayout.Toggle(enabled, GUILayout.ExpandWidth(false), GUILayout.Width(15));

						bool existed = Config.waveFold.ContainsKey(this);
						if (!existed)
						{
							Config.waveFold[this] = true;
						}

						bool foldout = Config.waveFold[this];
						GUIStyle gs = new GUIStyle(EditorStyles.foldout);
						gs.fontStyle = FontStyle.Bold;
						gs.fontSize = 13;
						bool isChildSelected = false;
						foreach (Challenge challenge in challenges)
						{
							if (challenge == selectedObject)
							{
								isChildSelected = true;
							}
						}

						if (isChildSelected)
						{
							gs.normal.textColor = Color.green;
							gs.onNormal.textColor = Color.green;
							gs.onActive.textColor = Color.green;
						}

						string postfixCount = challenges.Count > 0 ? $"({challenges.Count})" : string.Empty;
						foldout = GUILayout.Toggle(
							foldout, $"WAVE #{waveOrder} {postfixCount}", gs,
							GUILayout.ExpandWidth(false), GUILayout.Width(140)
						);
						
						if (Config.waveFold[this] != foldout)
						{
							Config.ResetCacheRectData();
						}
						Config.waveFold[this] = foldout;
						if (!foldout) return;

						isRemoved = GUILayout.Button("Remove", GUILayout.Width(80));
						if (GUILayout.Button("Add Challenge", GUILayout.Width(120)))
						{
							challenges.Add(new Challenge());
						}

						if (GUILayout.Button("Copy", GUILayout.Width(80)))
						{
							copiedWave = this;
						}

						isPasted = GUILayout.Button("Paste", GUILayout.Width(80));
					}

					using (new EditorHelper.Indent(-1))
					{
						relativePosition = EditorGUILayout.Vector2Field("Position", relativePosition);
						foreach (Challenge challenge in challenges)
						{
							challenge.spawn.SetWavePosition(ShowWorldPosition());
							challenge.spawn.SetChallenge(challenge);
							if (challenge.trigger is DistanceTrigger)
							{
								((DistanceTrigger) challenge.trigger).SetChallenge(challenge);
							}
						}

						Challenge removedChallenge = null;
						Challenge duplicatedChallenge = null;
						int upIndex = -1;
						int downIndex = -1;

						int count = challenges.Count;
						for (int kIndex = 0; kIndex < challenges.Count; kIndex++)
						{
							Challenge c = challenges[kIndex];

							c.SetOrder(kIndex + 1);
							c.SetLastOrder(count);
							if (c.OnPreGUI())
								c.OnGUI();

							if (c.IsRemoved)
							{
								removedChallenge = c;
								break;
							}

							if (c.IsPasted)
							{
								challenges[kIndex] = new JsonDeserializationOperation(
									new JsonSerializationOperation(copiedChallenge).Act()
								).Act<Challenge>();
								GUI.changed = true;
							}

							if (c.IsDuplicated)
							{
								duplicatedChallenge = c;
							}

							if (c.IsMoveUp)
							{
								upIndex = kIndex;
							}

							if (c.IsMoveDown)
							{
								downIndex = kIndex;
							}
						}

						if (removedChallenge != null)
						{
							challenges.Remove(removedChallenge);
							Config.ResetCacheRectData();
						}

						if (duplicatedChallenge != null)
						{
							Challenge newChallenge = new JsonDeserializationOperation(
								new JsonSerializationOperation(duplicatedChallenge).Act()
							).Act<Challenge>();
							challenges.Add(newChallenge);
							GUI.changed = true;
							selectedObject = newChallenge;
						}

						if (upIndex > 0)
						{
							challenges.Swap(upIndex, upIndex - 1);
							Config.ResetCacheRectData();
						}

						if (downIndex >= 0)
						{
							challenges.Swap(downIndex, downIndex + 1);
							Config.ResetCacheRectData();
						}
					}
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
				//Handles.Label(ShowWorldPosition(), "Wave #" + waveOrder);

				bool existed = Config.waveFold.ContainsKey(this);
				if (!existed)
				{
					Config.waveFold[this] = true;
				}

				bool foldout = Config.waveFold[this];
				Config.waveFold[this] = foldout;
				if (!foldout) return;

				for (int kIndex = 0; kIndex < challenges.Count; kIndex++)
				{
					challenges[kIndex].OnSceneGUI();
				}

				if (selectedObject == this)
				{
					Vector2 newWorldPosition = Handles.DoPositionHandle(ShowWorldPosition(), Quaternion.identity);
					relativePosition = newWorldPosition - stagePosition;
					foreach (Challenge challenge in challenges)
					{
						challenge.spawn.SetWavePosition(ShowWorldPosition());
					}
				}
			}

			public void OnDrawGizmos(GameObject o)
			{
				Gizmos.color = new Color(255f / 255f, 140f / 255f, 80f / 255f);
				Gizmos.DrawSphere(ShowWorldPosition(), .65f);

				bool existed = Config.waveFold.ContainsKey(this);
				if (!existed)
				{
					Config.waveFold[this] = true;
				}

				bool foldout = Config.waveFold[this];
				Config.waveFold[this] = foldout;
				if (!foldout) return;

				foreach (Challenge challenge in challenges)
				{
					challenge.OnDrawGizmos(o);
				}
			}

			[JsonIgnore]
			public bool IsRemoved
			{
				get { return isRemoved; }
			}

			public void SetOrder(int order)
			{
				waveOrder = order;
			}

			[JsonIgnore]
			public int WaveOrder
			{
				get { return waveOrder; }
			}

			[JsonIgnore]
			public bool IsPasted
			{
				get { return isPasted; }
			}
#endif

			#endregion

			public void SetStagePosition(Vector2 pos)
			{
				stagePosition = pos;
				foreach (Challenge challenge in challenges)
				{
					challenge.spawn.SetWavePosition(ShowWorldPosition());
				}
			}
		}
	}
}