using System;
using System.Collections.Generic;
using System.Linq;
using Checking;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;

namespace Core.DungeonLogic.Stages.EndConditions
{
    public class TotalBattleTimeEndCondition : LosingCondition, Goal
    {
        private float targetTime;
        private readonly Environment.Environment environment;

        private float currentTime;
        private readonly NotNullReference notNullReference = new NotNullReference();

        public TotalBattleTimeEndCondition(float targetTime, Environment.Environment environment)
        {
            notNullReference.Check(environment, "environment");

            this.targetTime = targetTime;
            this.environment = environment;
        }

        public TotalBattleTimeEndCondition(Environment.Environment environment) : this(0, environment)
        {
        }
        
        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");

            targetTime = Convert.ToSingle(cookies.ElementAt(0));
        }

        public bool IsMet()
        {
            return currentTime >= targetTime;
        }

        public bool IsAchieved()
        {
            return IsMet();
        }

        public void Update(float dt)
        {
            currentTime = environment.ElapsedTime();
        }

        public void OnAddedToStage(DefaultStage stage)
        {
            
        }

        public string Reason()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return $"{GetType().Name}\n\t\t\tSeconds: {targetTime}";
        }
    }
}