using System;
using System.Collections.Generic;
using System.Linq;
using Checking;
using Core.DungeonLogic.Environment.Character;
using Core.DungeonLogic.Stages.Goals;
using Core.DungeonLogic.Stages.LosingConditions;
using UnityEngine;

namespace Core.DungeonLogic.Stages.EndConditions
{
    public class ReachCheckpointEndCondition : Goal, LosingCondition
    {
        private Vector2 checkpointPosition;
        private readonly NotNullReference notNullReference = new NotNullReference();

        private readonly Environment.Environment environment;
        private Character character;

        public ReachCheckpointEndCondition(Environment.Environment environment)
        {
            this.environment = environment;
        }

        public void SetCookies(IEnumerable<string> cookies)
        {
            notNullReference.Check(cookies, "cookies");
            float x = Convert.ToSingle(cookies.ElementAt(0));
            float y = Convert.ToSingle(cookies.ElementAt(1));
            
            checkpointPosition = new Vector2(x, y);
        }

        public bool IsAchieved()
        {
            if (environment.Character() == null) return false;

            Character character = environment.Character();
            return character.Position().x >= checkpointPosition.x;
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