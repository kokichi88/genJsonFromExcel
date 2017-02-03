namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public class TriggerActionByDefault : IActionTrigger
    {
        public bool IsTriggered()
        {
            return true;
        }

        public void Update(float dt)
        {
        }

        public void SetEnvironment(Environment.Environment env)
        {
        }
    }
}