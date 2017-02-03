namespace Core.DungeonLogic.Stages.Challenges.Actions
{
    public interface IActionTrigger
    {
        bool IsTriggered();

        void Update(float dt);

        void SetEnvironment(Environment.Environment env);
    }
}