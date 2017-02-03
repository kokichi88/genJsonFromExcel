using System.Collections.Generic;

namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public class DungeonActionComponent : Component
    {
        private List<IAction> actions;
        private bool isRunning = false;

        public DungeonActionComponent(List<IAction> actions)
        {
            this.actions = actions;
        }

        public void StartUp()
        {
            
        }

        public void ShutDown()
        {
            
        }

        public void Start()
        {
            isRunning = true;
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void Update(float dt)
        {
            if (!isRunning)
            {
                return;
            }

            foreach (IAction action in actions)
            {
                if (action.IsFinished()) continue;
                
                action.Update(dt);
            }
        }
    }
}