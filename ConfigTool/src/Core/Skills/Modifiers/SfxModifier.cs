using System;
using System.Collections.Generic;
using Artemis;
using Combat.Sfx;
using Combat.Sfx.Players;
using Core.Commons;
using Core.Skills.Modifiers.Info;
using Core.Skills.Modifiers.Lifetimes;
using Ssar.Combat.Skills;
using Ssar.Combat.Skills.Interactions;
using Utils;

namespace Core.Skills.Modifiers {
	public class SfxModifier : BaseModifier {
		private readonly SfxInfo info;
		private readonly Entity targetEntity;
		private readonly SkillId skillId;

		public SfxModifier(ModifierInfo info, Entity casterEntity, Entity targetEntity,
		                   Environment environment, CollectionOfInteractions modifierInteractionCollection,
		                   SkillId skillId) : base(info, casterEntity, targetEntity, environment,
			modifierInteractionCollection) {
			this.info = (SfxInfo) info;
			this.targetEntity = targetEntity;
			this.skillId = skillId;
		}

		public override ModifierType Type() {
			return ModifierType.Sfx;
		}

		protected override void OnUpdate(float dt) {
		}

		public override bool IsBuff() {
			return true;
		}

		protected override List<Lifetime> CreateLifetimes(ModifierInfo modifierInfo) {
			SfxInfo si = (SfxInfo) modifierInfo;
			return new List<Lifetime>(new[] {new DurationBasedLifetime(0.1f),});
		}

		protected override void OnDelayedAttachAsMain(Character target) {
			SfxPlayer.instance.AddIntervalConfig(new IntervalConfigEntry() {
				id = skillId.StringValue,
				interval = info.Smc.interval
			});
			CharacterId targetCharacterId = targetEntity.GetComponent<SkillComponent>().CharacterId;
			AudioClipConfig acc = null;
			foreach (AudioAndCharacterId aaci in info.Smc.overrides) {
				if (aaci.charId.Equals(targetCharacterId.StringValue)) {
					if (aaci.audioClips.Count > 0) {
						acc = aaci.audioClips[BattleUtils.RandomRangeInt(0, aaci.audioClips.Count)];
					}
				}
			}

			if (acc == null) {
				if (info.Smc.audioClips.Count > 0) {
					acc = info.Smc.audioClips[BattleUtils.RandomRangeInt(0, info.Smc.audioClips.Count)];
				}
			}

			if (acc != null) {
				SfxPlayer.instance.AddPlaybackRequest(new PlaybackRequest() {
					intervalId = skillId.StringValue,
					audioPath = acc.audioPath,
					pitch = acc.pitch,
					volume = acc.volume
				});
			}
		}
	}
}