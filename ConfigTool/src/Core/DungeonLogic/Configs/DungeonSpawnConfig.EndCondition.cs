using System;
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
        public enum EndConditionName
        {
            KillAllEnemy,
            HeroDeath,
            ReachCheckpoint,
            CountdownTime,
            TotalBattleTime,
            SecretMission,
        }
        
        public abstract class EndCondition
        {
            public string conditionName = EndConditionName.KillAllEnemy.ToString();
            
            private int order;

            protected EndCondition(string conditionName)
            {
                this.conditionName = conditionName;
            }

            public EndConditionName ShowConditionName()
            {
                return conditionName.Parse<EndConditionName>();
            }
            
            #region EDITOR
#if UNITY_EDITOR
            
            public virtual void OnGUI()
            {
                using (new EditorHelper.Box(true, 10))
                {
                    using (new EditorHelper.Horizontal())
                    {
                        bool existed = Config.conditionFold.ContainsKey(this);
                        if (!existed)
                        {
                            Config.conditionFold[this] = true;
                        }

                        bool foldout = Config.conditionFold[this];
                        GUIStyle gs = new GUIStyle(EditorStyles.foldout);
                        bool isSelected = this == selectedObject;

                        if (isSelected)
                        {
                            gs.normal.textColor = Color.green;
                            gs.onNormal.textColor = Color.green;
                            gs.onActive.textColor = Color.green;
                        }

                        foldout = GUILayout.Toggle(
                            foldout, "Condition #" + order, gs,
                            GUILayout.ExpandWidth(false), GUILayout.Width(140)
                        );
                        
                        if (Config.conditionFold[this] != foldout)
                        {
                            Config.ResetCacheRectData();
                        }
                        Config.conditionFold[this] = foldout;
                        if (!foldout) return;

                        IsRemoved = GUILayout.Button("Remove", GUILayout.Width(80));

                        if (GUILayout.Button("Copy", GUILayout.Width(80)))
                        {
                            copiedCondition = this;
                        }

                        IsPasted = GUILayout.Button("Paste", GUILayout.Width(80));
                    }

                    using (new EditorHelper.Indent(-2))
                    {
                        EndConditionName oldConditionName = ShowConditionName();
                        EndConditionName newConditionName = (EndConditionName) EditorGUILayout.EnumPopup(
                            "Condition Name", oldConditionName);
                        conditionName = newConditionName.ToString();
                        IsConditionChanged = !oldConditionName.Equals(newConditionName);

                        DrawGUI();
                    }
                }
            }
            
            protected virtual void DrawGUI() {}
            
            public virtual void OnSceneGUI() {}
            
            public virtual void OnDrawGizmos(GameObject o)
            {
            }
            
            [JsonIgnore]
            public bool IsConditionChanged { get; private set; }
            
            [JsonIgnore]
            public bool IsRemoved { get; set; }

            [JsonIgnore]
            public bool IsPasted { get; private set; }

            public void SetOrder(int order)
            {
                this.order = order;
            }

            public virtual Vector2 ShowWorldPosition()
            {
                return Vector2.zero;
            }

            public abstract EndCondition Clone();

            protected T Clone<T>()
            {
                return new JsonDeserializationOperation(new JsonSerializationOperation(this).Act()).Act<T>();
            }
#endif

            #endregion
        }
        
        public class KillAllEnemyCondition : EndCondition
        {
            public KillAllEnemyCondition() : base(EndConditionName.KillAllEnemy.ToString())
            {
            }

#if UNITY_EDITOR
            public override EndCondition Clone()
            {
                return Clone<KillAllEnemyCondition>();
            }
#endif
        }
        
        public class HeroDeathCondition : EndCondition
        {
            public HeroDeathCondition() : base(EndConditionName.HeroDeath.ToString())
            {
            }

#if UNITY_EDITOR
            public override EndCondition Clone()
            {
                return Clone<HeroDeathCondition>();
            }
#endif
        }
        
        public class ReachCheckpointCondition : EndCondition
        {
            public Vector2 checkpointPosition;
            
            public ReachCheckpointCondition() : base(EndConditionName.ReachCheckpoint.ToString())
            {
            }
            
            #region EDITOR
#if UNITY_EDITOR

            protected override void DrawGUI()
            {
                base.DrawGUI();

                checkpointPosition = EditorGUILayout.Vector2Field("Checkpoint Position", checkpointPosition);
            }

            public override void OnSceneGUI()
            {
                base.OnSceneGUI();

                if (selectedObject == this)
                {
                    checkpointPosition = Handles.DoPositionHandle(checkpointPosition, Quaternion.identity);
                }
            }

            public override void OnDrawGizmos(GameObject o)
            {
                base.OnDrawGizmos(o);
                
                Gizmos.color = new Color(1f, 0.43f, 1f);
                Gizmos.DrawSphere(checkpointPosition, .75f);
            }

            public override EndCondition Clone()
            {
                return Clone<ReachCheckpointCondition>();
            }

            public override Vector2 ShowWorldPosition()
            {
                return checkpointPosition;
            }
#endif
            #endregion
        }
        
        public class CountdownTimeCondition : EndCondition
        {
            public int time;
            
            public CountdownTimeCondition() : base(EndConditionName.CountdownTime.ToString())
            {
            }

            #region EDITOR
#if UNITY_EDITOR

            public override void OnGUI()
            {
                base.OnGUI();

                time = EditorGUILayout.IntField("Time", time);
            }

            public override EndCondition Clone()
            {
                return Clone<CountdownTimeCondition>();
            }
            
            #endif
            #endregion
        }
        
        public class TotalBattleTimeCondition : EndCondition
        {
            public int time;
            
            public TotalBattleTimeCondition() : base(EndConditionName.TotalBattleTime.ToString())
            {
            }

            #region EDITOR
#if UNITY_EDITOR

            public override void OnGUI()
            {
                base.OnGUI();

                time = EditorGUILayout.IntField("Time", time);
            }

            public override EndCondition Clone()
            {
                return Clone<TotalBattleTimeCondition>();
            }
            
#endif
            #endregion
        }
        
        public class SecretMissionCondition : EndCondition
        {
            public SecretMissionCondition() : base(EndConditionName.SecretMission.ToString())
            {
            }
            
            #region EDITOR
#if UNITY_EDITOR

            public override EndCondition Clone()
            {
                return Clone<SecretMissionCondition>();
            }
            
#endif
            #endregion
        }
    }
}