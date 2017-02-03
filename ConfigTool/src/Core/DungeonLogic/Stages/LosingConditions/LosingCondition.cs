namespace Core.DungeonLogic.Stages.LosingConditions {
	public interface LosingCondition {
		bool IsMet();

		void Update(float dt);

		void OnAddedToStage(DefaultStage stage);
	}
}
