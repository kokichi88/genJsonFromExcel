using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;

namespace Core.Skills.Modifiers.Info {
	public interface ModifierInfo {
		ModifierType ShowType();
		float ShowSuccessRate();
		float DelayToApply();
		Target Target();
		bool IsDependentOnSkill();
		Skill ShowParentSkill();
		List<VfxConfig> ShowVfxConfig();
		string ShowIcon();
		List<LifetimeConfig> ShowLifetimeConfigs();
	}

	public enum Target {
		Self,
		Target
	}
}