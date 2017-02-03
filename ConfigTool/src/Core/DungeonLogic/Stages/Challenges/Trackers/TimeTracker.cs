using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Checking;
using Core.DungeonLogic.Environment;

namespace Core.DungeonLogic.Stages.Challenges.Trackers
{
    public class TimeTracker : Tracker
    {
        private int eventId;
        private float timeThreshold;
        private readonly NotNullReference notNullReference = new NotNullReference();

        private float elapsed;
        private bool isFinished;
        private Environment.Environment environment;
        private Entity target;
        private bool isTargetSet;
        
        public bool IsFinished()
        {
            if (!isTargetSet) return false;

            return isFinished;
        }

        public void Update(float dt, int waveOrder)
        {
            if (!isTargetSet || isFinished) return;

            elapsed += dt;

            if (elapsed >= timeThreshold)
            {
                DispatchEvent();
            }
        }

        public void SetEnv(Environment.Environment env)
        {
            environment = env;
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            eventId = Convert.ToInt32(cookies.ElementAt(0));
            timeThreshold = Convert.ToSingle(cookies.ElementAt(1));
        }

        public void AddEntity(Entity entity)
        {
            if (target != null || entity == null) return;
            target = entity;
            isTargetSet = true;
        }

        public string UnfinishedReason()
        {
            return "Can't find target or target's time is lower than threshold";
        }
        
        private void DispatchEvent()
        {
            if (isFinished) return;
            
            isFinished = true;

            if (environment is DefaultDungeonEnvironment dungeonEnvironment)
            {
                dungeonEnvironment.TriggerEvent(eventId);
            }
        }
    }
}