using System.Collections.Generic;
using Artemis;
using Artemis.System;
using EntityComponentSystem.Components;
using EntityComponentSystem.Systems;
using State = Core.Skills.Animations.AnimationSequenceComponent.State;

namespace Core.Skills.Animations {
	public class AnimationSequenceResetSystem : EntityProcessingSystemWithLocalTimeScale {
		public AnimationSequenceResetSystem() : base(Aspect.All(typeof(AnimationSequenceComponent), typeof(TimeScaleComponent)), true) {
		}

		protected override void OnProcess(Entity entity, float deltaTime) {
			AnimationSequenceComponent animationSequence = entity.GetComponent<AnimationSequenceComponent>();
			foreach (KeyValuePair<SkillId, State> pair in animationSequence.stateBySkillId) {
				pair.Value.elapsed += deltaTime;
				if (pair.Value.elapsed >= pair.Value.duration) {
					pair.Value.elapsed = 0;
					pair.Value.index = 0;
				}
			}
		}
	}
}