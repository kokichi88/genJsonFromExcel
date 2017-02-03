using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Checking;
using Core.DungeonLogic.Environment;
using Core.Skills;
using Ssar.Combat.Skills;

namespace Core.DungeonLogic.Stages.Challenges.Trackers
{
    public class SkillTracker : Tracker
    {
        private const float COOLDOWN = 0.1f;
        private int eventId;
        private SkillId skillId;
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
            
            CheckSkill();
        }

        public void SetEnv(Environment.Environment env)
        {
            environment = env;
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            eventId = Convert.ToInt32(cookies.ElementAt(0));
            skillId = new SkillId(cookies.ElementAt(1));
        }

        public void AddEntity(Entity entity)
        {
            if (target == null && entity != null)
            {
                target = entity;
                isTargetSet = true;

                SkillComponent skillComponent = target.GetComponent<SkillComponent>();
                skillComponent.Character.PostSkillCastEventHandler += OnCastSkill;
            }
        }

        public string UnfinishedReason()
        {
            return "Can't find target or target's skill ID";
        }
        
        private void CheckSkill()
        {
            if (!isTargetSet) return;
            
            if (target == null)
            {
                DispatchEvent();
                return;
            }
        }
        
        private void OnCastSkill(object sender, Character.SkillCastEventArgs e)
        {
            if (e.skillId.Equals(skillId))
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