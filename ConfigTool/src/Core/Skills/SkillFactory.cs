using Core.Skills;

namespace Core.Skills {
	public interface SkillFactory {
		Skill Create(Character caster, SkillId skillId, SkillCastingSource skillCastingSource);
	}
}
