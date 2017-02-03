namespace Core.Skills.Modifiers.Lifetimes {
	public interface Lifetime {
		LifetimeType ShowType();
		void Update(float dt);
		void Check();
		bool IsEnd();
		void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage);
	}
}