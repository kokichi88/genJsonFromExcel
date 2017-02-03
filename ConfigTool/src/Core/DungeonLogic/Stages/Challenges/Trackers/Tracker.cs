using System.Collections.Generic;
using Artemis;

namespace Core.DungeonLogic.Stages.Challenges.Trackers
{
    public interface Tracker
    {
        bool IsFinished();

        void Update(float dt, int waveOrder);
        
        void SetEnv (Environment.Environment env);

        void SetCookies (IEnumerable<string> cookies);

        void AddEntity(Entity entity);
        
        string UnfinishedReason();
    }
}