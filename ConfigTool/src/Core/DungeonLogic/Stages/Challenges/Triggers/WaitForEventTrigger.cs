using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Checking;
using Combat.DamageSystem;
using Core.DungeonLogic.Environment;
using Utils.DataStruct;

namespace Core.DungeonLogic.Stages.Challenges.Triggers
{
    public class WaitForEventTrigger : Trigger
    {
        private const float COOLDOWN = 0.1f;
        private int eventId;
        private float waitTimeInSeconds;
        private readonly NotNullReference notNullReference = new NotNullReference();

        private float cooldownCount = COOLDOWN;
        private bool isFinished = false;
        private bool isTriggered = false;
        private Environment.Environment environment;

        public bool IsFinished()
        {
            if (!isTriggered)
            {
                isTriggered = environment.IsEventTriggered(eventId);
            }

            if (!isFinished)
            {
                isFinished = isTriggered && waitTimeInSeconds <= 0;
            }
            
            return isFinished;
        }

        public void Update(float dt, int waveOrder)
        {
            if (isTriggered)
            {
                waitTimeInSeconds -= dt;
            }
            
            if (!(cooldownCount > 0)) return;
            cooldownCount -= dt;
            if (!(cooldownCount <= 0)) return;
            cooldownCount = COOLDOWN;
            IsFinished();
        }

        public void SetEnv(Environment.Environment env)
        {
            environment = env;
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            eventId = Convert.ToInt32(cookies.ElementAt(0));
            waitTimeInSeconds = Convert.ToSingle(cookies.ElementAt(1));
        }

        public string UnfinishedReason()
        {
            return "Can't find event with ID: " + eventId;
        }
    }
}