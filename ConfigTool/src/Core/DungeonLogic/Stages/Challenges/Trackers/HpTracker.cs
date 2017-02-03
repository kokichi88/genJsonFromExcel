using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Checking;
using Combat.DamageSystem;
using Core.DungeonLogic.Environment;

namespace Core.DungeonLogic.Stages.Challenges.Trackers
{
    public class HpTracker : Tracker
    {
        private const float COOLDOWN = 0.1f;
        private int eventId;
        private float hpThreshold;
        private readonly NotNullReference notNullReference = new NotNullReference();
        
        private float cooldownCount = COOLDOWN;
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
            if (!(cooldownCount > 0)) return;
            cooldownCount -= dt;
            if (!(cooldownCount <= 0)) return;
            cooldownCount = COOLDOWN;
            
            CheckHp();
        }

        public void SetEnv(Environment.Environment env)
        {
            environment = env;
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            eventId = Convert.ToInt32(cookies.ElementAt(0));
            hpThreshold = Convert.ToSingle(cookies.ElementAt(1));
        }

        public void AddEntity(Entity entity)
        {
            if (target == null && entity != null)
            {
                target = entity;
                isTargetSet = true;
            }
        }

        public string UnfinishedReason()
        {
            return "Can't find target or target's hp is higher than threshold";
        }

        private void CheckHp()
        {
            if (!isTargetSet) return;
            
            if (target == null)
            {
                DispatchEvent();
                return;
            }
            
            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            bool matched = healthComponent == null ||
                           healthComponent.IsDead() ||
                         (float) healthComponent.Health / healthComponent.MaxHealth <= hpThreshold;
            if (matched)
            {
                DispatchEvent();
            }
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