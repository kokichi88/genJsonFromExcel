using System;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
    public partial class DungeonSpawnConfig
    {
        public enum TrackerName
        {
            TrackHp,
            TrackSkill,
            TrackTime,
        }
        
        public abstract class Tracker
        {
            public bool enabled = true;
            public string trackerName = TrackerName.TrackHp.ToString();
            public int eventId = -1;

            private int order;

            protected Tracker(string trackerName)
            {
                this.trackerName = trackerName;
            }

            public TrackerName ShowTrackerName()
            {
                return (TrackerName) Enum.Parse(typeof(TrackerName), trackerName);
            }
            
            public bool IsDisabled()
            {
                return !enabled;
            }

            #region EDITOR
#if UNITY_EDITOR

            public virtual void OnGUI()
            {
                using (new EditorHelper.Box(true, 0))
                {
                    using (new EditorHelper.Indent(-4))
                    using (new EditorHelper.Horizontal())
                    {
                        enabled = EditorGUILayout.Toggle(enabled, GUILayout.ExpandWidth(false), GUILayout.Width(30));

                        bool existed = Config.trackerFold.ContainsKey(this);
                        if (!existed)
                        {
                            Config.trackerFold[this] = true;
                        }

                        bool foldout = Config.trackerFold[this];
                        GUIStyle gs = new GUIStyle(EditorStyles.foldout);
                        gs.fontStyle = FontStyle.Bold;
                        bool isSelected = this == selectedObject;

                        if (isSelected)
                        {
                            gs.normal.textColor = Color.green;
                            gs.onNormal.textColor = Color.green;
                            gs.onActive.textColor = Color.green;
                        }

                        foldout = GUILayout.Toggle(
                            foldout, "Tracker #" + order, gs,
                            GUILayout.ExpandWidth(false), GUILayout.Width(140)
                        );
                        
                        if (Config.trackerFold[this] != foldout)
                        {
                            Config.ResetCacheRectData();
                        }
                        Config.trackerFold[this] = foldout;
                        if (!foldout) return;

                        IsRemoved = GUILayout.Button("Remove", GUILayout.Width(80));

                        if (GUILayout.Button("Copy", GUILayout.Width(80)))
                        {
                            copiedTracker = this;
                        }

                        IsPasted = GUILayout.Button("Paste", GUILayout.Width(80));
                    }

                    using (new EditorHelper.Indent(-3))
                    {
                        eventId = EditorGUILayout.IntField("Event ID", eventId);
                        
                        TrackerName oldTrackerName = ShowTrackerName();
                        TrackerName newTrackerName = (TrackerName) EditorGUILayout.EnumPopup(
                            "Tracker Name", oldTrackerName);
                        trackerName = newTrackerName.ToString();
                        IsTrackerChanged = !oldTrackerName.Equals(newTrackerName);

                        DrawGUI();
                    }
                }
            }
            
            protected virtual void DrawGUI() {}
            
            public virtual void OnDrawGizmos(GameObject o)
            {
            }
            
            [JsonIgnore]
            public bool IsTrackerChanged { get; private set; }
            
            [JsonIgnore]
            public bool IsRemoved { get; set; }

            [JsonIgnore]
            public bool IsPasted { get; private set; }

            public void SetOrder(int order)
            {
                this.order = order;
            }
            
            public abstract Tracker Clone();

            protected T Clone<T>()
            {
                return new JsonDeserializationOperation(new JsonSerializationOperation(this).Act()).Act<T>();
            }
#endif
            #endregion
        }
        
        public class HpTracker : Tracker
        {
            public float hpPercent;

            public HpTracker() : base(TrackerName.TrackHp.ToString())
            {
            }
            
            #region EDITOR
#if UNITY_EDITOR

            protected override void DrawGUI()
            {
                base.DrawGUI();
                
                float threshold = hpPercent * 100;
                threshold = EditorGUILayout.FloatField("HP Threshold (%)", threshold);
                threshold = Mathf.Clamp(threshold, 0, 100);
                hpPercent = threshold / 100f;
            }

            public override Tracker Clone()
            {
                return Clone<HpTracker>();
            }

#endif
            #endregion
        }
        
        public class SkillTracker : Tracker
        {
            public string skillName;
            
            public SkillTracker() : base(TrackerName.TrackSkill.ToString())
            {
            }
            
            #region EDITOR
#if UNITY_EDITOR

            protected override void DrawGUI()
            {
                base.DrawGUI();

                skillName = EditorGUILayout.TextField("Skill ID", skillName);
            }

            public override Tracker Clone()
            {
                return Clone<SkillTracker>();
            }

#endif
            #endregion
        }
        
        public class TimeTracker : Tracker
        {
            public float timeThreshold;

            public TimeTracker() : base(TrackerName.TrackTime.ToString())
            {
            }
            
            #region EDITOR
#if UNITY_EDITOR

            protected override void DrawGUI()
            {
                base.DrawGUI();
                
                timeThreshold = EditorGUILayout.FloatField("Time Threshold (s)", timeThreshold);
            }

            public override Tracker Clone()
            {
                return Clone<TimeTracker>();
            }

#endif
            #endregion
        }
    }
}