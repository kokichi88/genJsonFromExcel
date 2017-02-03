namespace Core.DungeonLogic.Stages.Goals {
	public interface Goal {
		bool IsAchieved();

		void Update(float dt);

		void OnAddedToStage(DefaultStage stage);

		string Reason();
	}
}
