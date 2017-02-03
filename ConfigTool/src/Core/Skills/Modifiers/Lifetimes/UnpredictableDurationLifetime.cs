namespace Core.Skills.Modifiers.Lifetimes {
	public class UnpredictableDurationLifetime : Lifetime {
		private bool end;

		public LifetimeType ShowType() {
			return LifetimeType.Unpredictable;
		}

		public void Update(float dt) {
		}

		public void Check() {
		}

		public bool IsEnd() {
			return end;
		}

		public void OnDamageDealt(Character caster, Character target, Skill fromSkill, Modifier fromModifier, int damage) {
		}

		public void End() {
			end = true;
		}
	}
}