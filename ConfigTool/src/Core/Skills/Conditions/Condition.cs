namespace Core.Skills.Conditions {
	public interface Condition {
		bool IsMeet();

		void Update(float dt);
		string Reason();
	}
}
