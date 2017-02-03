using System;
using System.Collections.Generic;
using System.Linq;
using Artemis;
using Assets.Scripts.Config;
using Checking;
using Core.Skills;
using Core.Utils.Extensions;
using Ssar.Combat.Skills;

namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public class CastSkillAction : IAction
    {
        private DungeonSpawnConfig.ActionLayer actionLayer;
        private DungeonSpawnConfig.ActionTriggerCondition triggerCondition;
        private float waitTime;
        private SkillId skillId;
        private int eventId;
        private int waveOrder;
        private readonly NotNullReference notNullReference = new NotNullReference();
        
        private IActionTrigger actionTrigger;
        private Entity target;
        private bool isFinished;
        private bool isTargetSet;
        private bool isTriggered;

        public DungeonSpawnConfig.ActionLayer GetLayer()
        {
            return actionLayer;
        }

        public bool IsFinished()
        {
            return isTargetSet && isFinished;
        }

        public void Update(float dt)
        {
            if (isFinished || !isTargetSet) return;
            
            actionTrigger.Update(dt);

            isTriggered = actionTrigger.IsTriggered();

            CheckCastSkill();
        }

        public void SetEnv(Environment.Environment env)
        {
            actionTrigger.SetEnvironment(env);
        }

        public void SetDungeonLogic(DungeonLogic dungeonLogic)
        {
            if (triggerCondition == DungeonSpawnConfig.ActionTriggerCondition.OnWaveFinish)
            {
                dungeonLogic.ListenToStageWaveCycle(OnWaveCycle);
            }
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            actionLayer = cookies.ElementAt(0).Parse<DungeonSpawnConfig.ActionLayer>();
            triggerCondition = cookies.ElementAt(1).Parse<DungeonSpawnConfig.ActionTriggerCondition>();
            waitTime = Convert.ToSingle(cookies.ElementAt(2));
            skillId = new SkillId(cookies.ElementAt(3));
            eventId = Convert.ToInt32(cookies.ElementAt(4));
            waveOrder = Convert.ToInt32(cookies.ElementAt(5));
            
            actionTrigger = GenerateActionTrigger();
        }

        public void AddEntity(Entity entity)
        {
            if (target == null && entity != null)
            {
                target = entity;
                isTargetSet = true;
                isTriggered = false;
                isFinished = false;
            }
        }

        public string UnfinishedReason()
        {
            return "Can't find target or target's skill ID";
        }

        private void CheckCastSkill()
        {
            if (isFinished || !isTriggered || !isTargetSet) return;
            
            isFinished = true;
            
            SkillComponent skillComponent = target.GetComponent<SkillComponent>();
            skillComponent.Character.CastSkill(skillId, SkillCastingSource.FromSystem());
        }
        
        private void OnWaveCycle(int stageorder, int waveorder, DefaultStage.WaveCycle cycle)
        {
            if (waveorder <= 1) return;

            int waveEnd = waveorder - 1;
            if (waveOrder != waveEnd) return;
            
            isTriggered = true;
            CheckCastSkill();
        }
        
        private IActionTrigger GenerateActionTrigger()
        {
            switch (triggerCondition)
            {
                case DungeonSpawnConfig.ActionTriggerCondition.Time:
                    return new TriggerActionByTime(waitTime);
                case DungeonSpawnConfig.ActionTriggerCondition.ByEvent:
                    return new TriggerActionByEvent(eventId);
                case DungeonSpawnConfig.ActionTriggerCondition.OnWaveFinish:
                    return new TriggerActionOnWaveFinish();
                default:
                    return new TriggerActionByDefault();
            }
        }
    }
}