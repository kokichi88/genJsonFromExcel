using System.Collections.Generic;
using Artemis;
using Assets.Scripts.Config;

namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public interface IAction
    {
        DungeonSpawnConfig.ActionLayer GetLayer();

        bool IsFinished();

        void Update(float dt);
        
        void SetEnv (Environment.Environment env);

        void SetDungeonLogic(DungeonLogic dungeonLogic);

        void SetCookies (IEnumerable<string> cookies);

        void AddEntity(Entity entity);
        
        string UnfinishedReason();
    }
}