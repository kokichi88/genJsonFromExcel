namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public class TriggerActionByTime : IActionTrigger
    {
        private readonly float waitTime;
        
        public float elapsed;

        public TriggerActionByTime(float waitTime)
        {
            this.waitTime = waitTime;
        }

        public bool IsTriggered()
        {
            return elapsed >= waitTime;
        }

        public void Update(float dt)
        {
            elapsed += dt;
        }

        public void SetEnvironment(Environment.Environment env)
        {
            
        }
    }
}