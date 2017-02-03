
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs.Modifiers;

namespace Core.Skills.Modifiers.Info {
	public class MoveSpeed2Info : MoveSpeedInfo {
		public MoveSpeed2Info(Target target, MoveSpeedModifierConfig msmc, List<VfxConfig> vfxs) : base(target, msmc, vfxs) {
		}

		public override ModifierType ShowType() {
			return ModifierType.MovementSpeed2;
		}
	}
}