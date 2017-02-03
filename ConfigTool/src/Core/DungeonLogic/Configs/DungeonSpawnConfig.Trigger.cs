using System;
using LitJson;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace Assets.Scripts.Config
{
    public partial class DungeonSpawnConfig
    {
	    public enum TriggerName {
		    WaitForSeconds,
		    WaitForDistance,
		    WaitForEvent,
	    }
	    
        public abstract class Trigger
        {
            public string triggerName = TriggerName.WaitForSeconds.ToString();
            
            private bool isTriggerChanged;
            protected Spawn spawn;

            protected Trigger(TriggerName tn)
            {
                this.triggerName = tn.ToString();
            }

            public TriggerName ShowTriggerName()
            {
                return (TriggerName) Enum.Parse(typeof(TriggerName), triggerName);
            }

            #region EDITOR
            #if UNITY_EDITOR

            public virtual void OnGUI()
            {
                TriggerName oldTriggerName = ShowTriggerName();
                TriggerName newTriggerName = (TriggerName) EditorGUILayout.EnumPopup(
                    "Trigger name", oldTriggerName, GUILayout.ExpandWidth(false)
                );
                triggerName = newTriggerName.ToString();
                isTriggerChanged = oldTriggerName != newTriggerName;
            }

            public virtual void OnDrawGizmos(GameObject o)
            {
            }

            [JsonIgnore]
            public bool IsTriggerChanged
            {
                get { return isTriggerChanged; }
            }

            public void SetSpawn(Spawn s)
            {
                spawn = s;
            }

            #endif
            #endregion
        }
        
        public class DistanceTrigger : Trigger {
			public float distance;

			private Challenge challenge;

			public DistanceTrigger() : base(TriggerName.WaitForDistance) {
			}

			#region EDITOR
#if UNITY_EDITOR
			public override void OnGUI() {
				using (new EditorHelper.Horizontal()) {
					float labelWidth = EditorGUIUtility.labelWidth;
					float fieldWidth = EditorGUIUtility.fieldWidth;

					EditorGUIUtility.fieldWidth = 150;
					base.OnGUI();

					GUI.SetNextControlName(challenge.GetHashCode().ToString());
					EditorGUIUtility.labelWidth = 60;
					EditorGUIUtility.fieldWidth = 60;
					EditorGUI.indentLevel -= 2;
					distance = EditorGUILayout.FloatField("Distance", distance, GUILayout.ExpandWidth(false));

					EditorGUIUtility.labelWidth = labelWidth;
					EditorGUIUtility.fieldWidth = fieldWidth;
					EditorGUI.indentLevel += 2;
				}

				EditorWindow inspectorWindow = null;
				foreach (EditorWindow editorWindow in Resources.FindObjectsOfTypeAll<EditorWindow>()) {
					if (editorWindow.title.Contains("Inspector")) {
						inspectorWindow = editorWindow;
					}
				}
				if (GUI.GetNameOfFocusedControl().Contains(challenge.GetHashCode().ToString())
				    && EditorWindow.mouseOverWindow == inspectorWindow) {
					if (selectedObject != challenge) {
						SceneView.lastActiveSceneView.Focus();
						inspectorWindow.Focus();
					}

					selectedObject = challenge;
				}
			}

			public override void OnDrawGizmos(GameObject o) {
				base.OnDrawGizmos(o);

				if (Selection.activeGameObject != o) return;

				Gizmos.color = Color.yellow;
				Vector2 rectDimension = new Vector2(distance, 1/2f);
				RectPivotPosition rpp = new RectPivotPosition(
					RectPivotPosition.PivotType.Center, Vector2.zero, rectDimension
				);
				Gizmos.DrawCube(
					rpp.RelativePositionOfPivotAt(RectPivotPosition.PivotType.BottomLeft) + (Vector2) spawn.ShowWorldPosition() + Vector2.up * rectDimension.y,
					new Vector3(rectDimension.x, rectDimension.y, 0)
				);
			}

			public void SetChallenge(Challenge c) {
				challenge = c;
			}
#endif
			#endregion
		}

		public class TimeTrigger : Trigger {
			public float waitTime;

			public TimeTrigger() : base(TriggerName.WaitForSeconds) {
			}

			#region EDITOR
#if UNITY_EDITOR
			public override void OnGUI() {
				EditorGUIUtility.fieldWidth = 150;
				base.OnGUI();

				waitTime = EditorGUILayout.FloatField("Trigger at(s)", waitTime);
			}
#endif
			#endregion
		}
		
		public class EventTrigger : Trigger
		{
			public int eventId;
			public float waitTime;

			public EventTrigger() : base(TriggerName.WaitForEvent)
			{
			}
			
			#region EDITOR
#if UNITY_EDITOR
			public override void OnGUI() {
				EditorGUIUtility.fieldWidth = 150;
				base.OnGUI();
				
				eventId = EditorGUILayout.IntField("Event ID", eventId);
				waitTime = EditorGUILayout.FloatField("Wait Time", waitTime);
			}
#endif
			#endregion
		}
    }
}