using System.Collections.Generic;
using Core.Commons;
using Core.Skills;
using Core.Utils.Extensions;
using JsonConfig.Model;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
    public partial class DungeonSpawnConfig
    {
        public enum ActionName
        {
            CastSkill,
        }

        public enum ActionTriggerCondition
        {
            Time,
            OnWaveFinish,
            ByEvent,
        }
        
        public enum ActionLayer
        {
            Wave,
            Stage,
            Dungeon,
        }
        
        public abstract class Action
        {
            public bool enabled = true;
            public string actionLayer = ActionLayer.Wave.ToString();
            public string actionName = ActionName.CastSkill.ToString();
            public string triggerCondition = ActionTriggerCondition.Time.ToString();
            public float waitTime;
            public int waveOrder = 1;
            public int eventId = -1;

            protected Action(string actionName)
            {
                this.actionName = actionName;
            }

            public ActionLayer ShowActionLayer()
            {
                return actionLayer.Parse<ActionLayer>();
            }
            
            public ActionName ShowActionName()
            {
                return actionName.Parse<ActionName>();
            }

            public ActionTriggerCondition ShowTriggerCondition()
            {
                return triggerCondition.Parse<ActionTriggerCondition>();
            }

            public bool IsDisabled()
            {
                return !enabled;
            }

            public virtual bool IsSpawnAction()
            {
                return false;
            }

            #region EDITOR
#if UNITY_EDITOR
            protected string monsterId;
            protected Challenge challenge;
            protected int order;

            public virtual void OnGUI()
            {
                EditorGUILayout.Space();
                using (new EditorHelper.Box(true, 0))
                {
                    using (new EditorHelper.Indent(-4))
                    using (new EditorHelper.Horizontal())
                    {
                        enabled = EditorGUILayout.Toggle(enabled, GUILayout.ExpandWidth(false), GUILayout.Width(15));

                        bool existed = Config.actionFold.ContainsKey(this);
                        if (!existed)
                        {
                            Config.actionFold[this] = true;
                        }

                        bool foldout = Config.actionFold[this];
                        GUIStyle gs = new GUIStyle(EditorStyles.foldout);
                        GUIStyleState normal = new GUIStyleState();
                        normal.textColor = new Color(226 / 255f, 100 / 255f, 226 / 255f, 1);
                        gs.fontStyle = FontStyle.Bold;
                        gs.normal = normal;
                        gs.focused = normal;
                        gs.hover = normal;
                        gs.active = normal;
                        gs.onActive = normal;
                        gs.onFocused = normal;
                        gs.onHover = normal;
                        gs.onNormal = normal;
                        
                        foldout = GUILayout.Toggle(
                            foldout, $"{GetTitleText()}", gs,
                            GUILayout.ExpandWidth(false), GUILayout.Width(150)
                        );
                        
                        if (Config.actionFold[this] != foldout)
                        {
                            Config.ResetCacheRectData();
                        }
                        Config.actionFold[this] = foldout;
                        if (!foldout) return;

                        IsRemoved = GUILayout.Button("Remove", GUILayout.Width(80));

                        if (GUILayout.Button("Copy", GUILayout.Width(80)))
                        {
                            copiedAction = this;
                        }

                        IsPasted = GUILayout.Button("Paste", GUILayout.Width(80));
                    }
                    
                    using (new EditorHelper.Indent(-3))
                    {
                        ActionLayer layer = ShowActionLayer();
                        layer = (ActionLayer) EditorGUILayout.EnumPopup("Active Layer", layer);
                        actionLayer = layer.ToString();
                        
                        if (!Config.showSpawnerMode)
                        {
                            ActionName oldActionName = ShowActionName();
                            ActionName newActionName = (ActionName) EditorGUILayout.EnumPopup(
                                "Action Name", oldActionName);
                            actionName = newActionName.ToString();
                            IsActionChanged = !oldActionName.Equals(newActionName);
                        }

                        ActionTriggerCondition oldCondition = ShowTriggerCondition();
                        ActionTriggerCondition newCondition = (ActionTriggerCondition) EditorGUILayout.EnumPopup("Trigger Condition", oldCondition);
                        triggerCondition = newCondition.ToString();
                        
                        if (newCondition == ActionTriggerCondition.Time)
                        {
                            waitTime = EditorGUILayout.FloatField("Activate at(s)", waitTime);
                            waitTime = Mathf.Max(0, waitTime);
                        }
                        else if (newCondition == ActionTriggerCondition.ByEvent)
                        {
                            eventId = EditorGUILayout.IntField("Event ID", eventId);
                        }
                        else if (newCondition == ActionTriggerCondition.OnWaveFinish)
                        {
                            waveOrder = EditorGUILayout.IntField("Wave Order", waveOrder);
                            waveOrder = Mathf.Max(1, waveOrder);
                        }
                        
                        DrawGUI();
                    }
                }
            }
            
            protected virtual void DrawGUI() {}
            
            [JsonIgnore] public bool IsActionChanged { get; private set; }
            
            [JsonIgnore] public bool IsRemoved { get; set; }

            [JsonIgnore] public bool IsPasted { get; private set; }

            public void SetOrder(int order)
            {
                this.order = order;
            }

            public int GetOrder()
            {
                return order;
            }

            public void SetMonsterId(string monsterId)
            {
                bool isChanged = this.monsterId != monsterId;
                this.monsterId = monsterId;

                if (isChanged)
                {
                    OnChangeMonster();
                }
            }
            
            public void SetChallenge(Challenge c)
            {
                challenge = c;
            }

            public abstract Action Clone();
            
            protected T Clone<T>()
            {
                return new JsonDeserializationOperation(new JsonSerializationOperation(this).Act()).Act<T>();
            }

            protected virtual void OnChangeMonster() { }
            
            protected virtual string GetTitleText()
            {
                return $"Action #{order}";
            }
#endif

            #endregion
        }
        
        public class CastSkillAction : Action
        {
            public string skillId;
            public List<ChallengePreset> challengePresets = new List<ChallengePreset>();
            
            public CastSkillAction() : base(ActionName.CastSkill.ToString())
            {
            }

            public override bool IsSpawnAction()
            {
                return challengePresets.Count > 0;
            }

#if UNITY_EDITOR
            private List<string> skillIds = new List<string>();
            private List<string> skillNames = new List<string>();
            
            protected override void DrawGUI()
            {
                base.DrawGUI();

                if (skillIds.Count < 1)
                {
                    ReadSkillConfig(monsterId);
                }

                string oldSkillId = skillId;
                int index = skillIds.IndexOf(oldSkillId);
                if (index == -1) index = 0;
                EditorGUIUtility.fieldWidth = 170;
                index = EditorGUILayout.Popup("Skill ID", index, skillNames.ToArray());
                string newSkillId = skillIds[index];
                skillId = newSkillId;
                bool isSkillIdChanged = oldSkillId != newSkillId;

                if (!Config.showSpawnerMode) return;

                /*if (challenge.IsSpawner())
                {
                    challenge.UpdateSpawner(order, oldSkillId, newSkillId);
                }*/
                
                DrawSpawnPresets();
            }

            private void DrawSpawnPresets()
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                // using (new EditorHelper.Box(true, 10)).
                // using (new EditorHelper.Indent(1))
                {
                    // using (new EditorHelper.Indent(-2))
                    using (new EditorHelper.Horizontal())
                    {
                        GUIStyle gs = new GUIStyle(EditorStyles.label);
                        gs.fontStyle = FontStyle.Bold;
                        gs.fontSize = 11;
                        EditorGUILayout.LabelField("OnCast phase", gs, GUILayout.ExpandWidth(false), GUILayout.Width(120));

                        if (GUILayout.Button("Add Spawn Preset", GUILayout.Width(120)))
                        {
                            challengePresets.Add(new ChallengePreset());
                            // challenge.AddSpawnPresetForSkill(order, skillId);
                        }
                    }

                    // using (new EditorHelper.Indent(-2))
                    using (new EditorHelper.Indent(1))
                    {
                        if (!IsSpawnAction()) return;

                        ChallengePreset removedPreset = null;
                        for (int i = 0; i < challengePresets.Count; i++)
                        {
                            ChallengePreset challengePreset = challengePresets[i];
                            // if (string.IsNullOrEmpty(challengePreset.skillId) || !challengePreset.skillId.Equals(skillId)) continue;

                            EditorGUILayout.Space();
                            // EditorGUILayout.Space();

                            challengePreset.SetOrder(i + 1);
                            challengePreset.OnGUI();

                            if (challengePreset.IsRemoved)
                            {
                                removedPreset = challengePreset;
                            }
                        }

                        if (removedPreset != null)
                        {
                            challengePresets.Remove(removedPreset);
                            // challenge.RemovePreset(removedPreset);
                        }
                    }
                }
            }

            public override Action Clone()
            {
                return Clone<CastSkillAction>();
            }

            protected override void OnChangeMonster()
            {
                base.OnChangeMonster();

                ReadSkillConfig(monsterId);
            }

            protected override string GetTitleText()
            {
                string countAsPostfix = IsSpawnAction() ? $" ({challengePresets.Count})" : string.Empty;
                return $"Cast Skill #{order}{countAsPostfix}";
            }

            private void ReadSkillConfig(string monsterId)
            {
                CharacterId characterId = new CharacterId(monsterId);
                string monsterConfigContent = ((TextAsset) EditorGUIUtility.Load($"Assets/Resources/Config/General/{characterId.GroupId}_SkillConfig.txt")).text;
                SkillStatsConfig skillStatsConfig = JsonMapper.ToObject<SkillStatsConfig>(monsterConfigContent);
                
                skillIds.Clear();
                foreach (HeroConfig.SkillStats stats in skillStatsConfig.skills.Values)
                {
                    stats.Prepare();
                    if (stats.ShowCategory() != SkillCategory.Skill && stats.ShowCategory() != SkillCategory.Spawn) continue;
                    skillIds.Add(stats.SkillId.StringValue);
                    skillNames.Add(string.IsNullOrEmpty(stats.note) ? $"{stats.SkillId.StringValue}" : $"{stats.SkillId.StringValue} - {stats.note}");
                }
                
                skillIds.Sort();
                skillNames.Sort();
            }
#endif
        }
    }
}