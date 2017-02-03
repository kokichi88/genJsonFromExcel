using System.Collections.Generic;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;
using SecretMission.Logic;

namespace Core.DungeonLogic.Stages.EndConditions
{
    public class SecretMissionEndCondition : Goal, LosingCondition
    {
        private readonly Environment.Environment environment;

        public SecretMissionEndCondition(Environment.Environment environment)
        {
            this.environment = environment;
        }
        
        public void SetCookies(IEnumerable<string> cookies)
        {
        }

        public bool IsAchieved()
        {
            SecretMissionTaskLogic taskLogic = Service.Get<SecretMissionTaskLogic>();
            return taskLogic != null && taskLogic.IsAllTaskCompleted();
        }

        public bool IsMet()
        {
            return IsAchieved();
        }

        public void Update(float dt)
        {
            
        }

        public void OnAddedToStage(DefaultStage stage)
        {
            
        }

        public string Reason()
        {
            return string.Empty;
        }
    }
}