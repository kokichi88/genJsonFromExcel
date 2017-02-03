namespace Core.Skills.Cooldowns {
	public interface Cooldown {
		void Start();

		bool IsComplete();

		void Update(float dt);

		void Reset();

		bool IsRecastable();
	}
}
