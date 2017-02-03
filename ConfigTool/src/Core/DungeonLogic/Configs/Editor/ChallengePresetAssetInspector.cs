using Assets.Scripts.Config;
using JsonConfig.Model;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEngine;
using Utils.Editor;

namespace Core.DungeonLogic.Configs.Editor
{
    [CustomEditor(typeof(ChallengePresetAsset))]
    public class ChallengePresetAssetInspector : UnityEditor.Editor
    {
        private MonsterConfig monsterConfig;

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            
            if (monsterConfig == null)
            {
                ReadMonsterConfig();
            }

            ChallengePresetAsset challengePresetAsset = (ChallengePresetAsset) target;
            if (challengePresetAsset.data == null || challengePresetAsset.data.Length < 1)
            {
                challengePresetAsset.data = new JsonSerializationOperation(new DungeonSpawnConfig.Challenge()).ActToStringArray();
                challengePresetAsset.Deserialize();
            }

            if (challengePresetAsset.challenge == null)
            {
                challengePresetAsset.Deserialize();
            }

            if (challengePresetAsset.monsterConfig == null)
            {
                challengePresetAsset.SetMonsterConfig(monsterConfig);
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Refresh Config"))
            {
                ReadMonsterConfig();
                challengePresetAsset.SetMonsterConfig(monsterConfig);
            }
            EditorGUILayout.Space();
            
            EditorGUI.BeginChangeCheck();
            challengePresetAsset.OnGUI();

            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                challengePresetAsset.Serialize();
                EditorUtility.SetDirty(challengePresetAsset);
            }
        }

        private void ReadMonsterConfig()
        {
            string monsterConfigContent = ((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/MonsterConfig.txt")).text;
            monsterConfig = JsonMapper.ToObject<MonsterConfig>(monsterConfigContent);
        }
    }
}