namespace Core.Skills {
	public interface Loopable {
		void Update(float dt);
		void LateUpdate(float dt);
		void Interrupt();
		bool IsFinished();
	}
}