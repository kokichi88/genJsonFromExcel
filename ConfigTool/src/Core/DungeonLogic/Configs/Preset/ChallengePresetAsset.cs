using System;
using JsonConfig.Model;
using Ssar.Combat.Skills;
using UnityEngine;
using Utils.Editor;

namespace Assets.Scripts.Config
{
    [CreateAssetMenu(menuName = "Dungeon Spawn/Challenge Preset")]
    public class ChallengePresetAsset : ScriptableObject
    {
        public string[] data = new string[0];
        
        [NonSerialized] public DungeonSpawnConfig.Challenge challenge;

        public void Deserialize()
        {
            challenge = DeserializeToObject();
        }

        public DungeonSpawnConfig.Challenge DeserializeToObject()
        {
            string join = string.Join("", data);
            return new JsonDeserializationOperation(join).Act<DungeonSpawnConfig.Challenge>();
        }

        #region EDITOR
#if UNITY_EDITOR
        [NonSerialized] public MonsterConfig monsterConfig;
        
        public void Serialize()
        {
            data = new JsonSerializationOperation(challenge).ActToStringArray();
        }

        public void OnGUI()
        {
            challenge.SetOrder(1);
            challenge.spawn.SetWavePosition(Vector2.zero);
            challenge.spawn.SetChallenge(challenge);
            if (challenge.trigger is DungeonSpawnConfig.DistanceTrigger trigger)
            {
                trigger.SetChallenge(challenge);
            }

            using (new EditorHelper.Indent(1))
            {
                challenge.OnGUI();
            }
        }

        public void SetMonsterConfig(MonsterConfig monsterConfig)
        {
            this.monsterConfig = monsterConfig;
            DungeonSpawnConfig.Config.ReadCharacterIdsFromFolderStructure(monsterConfig);
        }
#endif
        #endregion
    }
}