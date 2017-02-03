namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public class TriggerActionByEvent : IActionTrigger
    {
        private readonly int eventId;
        private Environment.Environment environment;

        public TriggerActionByEvent(int eventId)
        {
            this.eventId = eventId;
        }

        public bool IsTriggered()
        {
            return environment.IsEventTriggered(eventId);
        }

        public void Update(float dt)
        {
        }

        public void SetEnvironment(Environment.Environment env)
        {
            this.environment = env;
        }
    }
}