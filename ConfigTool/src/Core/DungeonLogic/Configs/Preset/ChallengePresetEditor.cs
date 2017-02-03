#if UNITY_EDITOR
using JsonConfig.Model;
using LitJson;
using Ssar.Combat.Skills;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Utils;
using Utils.Editor;

namespace Assets.Scripts.Config
{
    public class ChallengePresetEditor : EditorWindow
    {
        private ChallengePresetAsset challengePresetAsset;
        private string pathToFile;
        private EditorHelper.ScrollView.ScrollPosition scrollPosition = new EditorHelper.ScrollView.ScrollPosition();
        private MonsterConfig monsterConfig;

        [OnOpenAsset(1)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            ChallengePresetAsset challengePresetAsset = Selection.activeObject as ChallengePresetAsset;
            if (!mouseOverWindow.titleContent.text.Contains("Project")) return false;
            if (challengePresetAsset != null)
            {
                ChallengePresetEditor editor = CreateInstance<ChallengePresetEditor>();
                editor.pathToFile = new AssetFile(AssetDatabase.GetAssetPath(instanceID)).ShowResourcePath();
                editor.Show();
                editor.position = new Rect(100, 400, 640, 480);
                GUIContent editorTitleContent = new GUIContent(editor.titleContent);
                editorTitleContent.text = challengePresetAsset.name;
                editor.titleContent = editorTitleContent;
            }

            return false;
        }

        public static void Open(ChallengePresetAsset asset, string fileName)
        {
            if (asset != null)
            {
                ChallengePresetEditor editor = CreateInstance<ChallengePresetEditor>();
                editor.pathToFile = new AssetFile(AssetDatabase.GetAssetPath(asset)).ShowResourcePath();
                if (string.IsNullOrEmpty(editor.pathToFile)) return;
                editor.Show();
                editor.position = new Rect(100, 200, 640, 480);
                GUIContent editorTitleContent = new GUIContent(editor.titleContent);
                editorTitleContent.text = asset.name;
                editor.titleContent = editorTitleContent;
            }
        }

        private void OnGUI()
        {
            if (monsterConfig == null)
            {
                ReadMonsterConfig();
            }

            // EditorGUIUtility.labelWidth = 300;
            if (challengePresetAsset == null)
            {
                challengePresetAsset = Resources.Load<ChallengePresetAsset>(pathToFile);
                challengePresetAsset.Deserialize();
            }

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
            }
            EditorGUILayout.Space();

            DrawHeader();
            
            EditorGUI.BeginChangeCheck();
            using (new EditorHelper.ScrollView(scrollPosition))
            {
                challengePresetAsset.OnGUI();
            }

            bool changed = EditorGUI.EndChangeCheck();
            if (changed)
            {
                challengePresetAsset.Serialize();
                EditorUtility.SetDirty(challengePresetAsset);
            }
        }

        private void DrawHeader()
        {
            GUIStyle gs = new GUIStyle(EditorStyles.label);
            gs.normal.textColor = Color.white;
            gs.fontStyle = FontStyle.Bold;
            gs.fontSize = 14;
            EditorGUILayout.LabelField(challengePresetAsset.name, gs, GUILayout.Height(20));
            EditorGUILayout.Space();
        }
        
        private void ReadMonsterConfig() {
            string monsterConfigContent =
                ((TextAsset) EditorGUIUtility.Load("Assets/Resources/Config/General/MonsterConfig.txt")).text;
            monsterConfig = JsonMapper.ToObject<MonsterConfig>(monsterConfigContent);
            challengePresetAsset?.SetMonsterConfig(monsterConfig);
        }
    }
}
#endif