using System;
using System.Collections.Generic;
using Combat.Skills.ModifierConfigs;
using Combat.Skills.ModifierConfigs.Modifiers;
using Core.Skills.Modifiers.Info;
using Core.Utils;
using Ssar.Combat.Skills.Interactions;
using Func = System.Func<
	Core.Skills.ModifierInfoFactory.Params,
	Core.Skills.Modifiers.Info.ModifierInfo
>;
using ImpactVfxModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.ImpactVfxModifierConfig;
using PauseAnimationModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.PauseAnimationModifierConfig;
using PauseMovementModifierConfig = Combat.Skills.ModifierConfigs.Modifiers.PauseMovementModifierConfig;

namespace Core.Skills {
	public partial class ModifierInfoFactory {
		private static Dictionary<ModifierType, Func> subFactories;

		private CollectionOfInteractions collectionOfInteractions;
		private readonly WallHitConfig wallHitConfig;

		static ModifierInfoFactory() {
			subFactories = new Dictionary<ModifierType, Func>();
			subFactories[ModifierType.Stagger] = CreateStaggerInfo;
			subFactories[ModifierType.Knockdown] = CreateKnockdownInfo;
			subFactories[ModifierType.Launcher] = CreateLauncherInfo;
			subFactories[ModifierType.Trip] = CreateTripInfo;
			subFactories[ModifierType.Blast] = CreateBlastInfo;
			subFactories[ModifierType.LockFrame] = CreateLockFrameInfo;
			subFactories[ModifierType.Stun] = CreateStunInfo;
			subFactories[ModifierType.CameraFxShake] = CreateCameraFxShakeInfo;
			subFactories[ModifierType.AdvancedFrameToSelf] = CreateAdvancedFrameToSelfInfo;
			subFactories[ModifierType.Vibrate] = CreateVibrateInfo;
			subFactories[ModifierType.PlayImpactVfx] = CreatePlayImpactVfxInfo;
			subFactories[ModifierType.PauseMovement] = CreatePauseMovementInfo;
			subFactories[ModifierType.PauseAnimation] = CreatePauseAnimationInfo;
			subFactories[ModifierType.Shackle] = CreateShackleInfo;
			subFactories[ModifierType.Ragdoll] = CreateRagdollInfo;
			subFactories[ModifierType.PlayAnimation] = CreatePlayAnimationInfo;
			subFactories[ModifierType.Vanish] = CreateVanishInfo;
			subFactories[ModifierType.DamageOverTime] = CreateDamageOverTimeInfo;
			subFactories[ModifierType.HitboxTransform] = CreateHitboxTransformInfo;
			subFactories[ModifierType.KnockbackWeight] = CreateKnockbackWeightInfo;
			subFactories[ModifierType.KnockdownWeight] = CreateKnockdownWeightInfo;
			subFactories[ModifierType.MovementSpeed] = CreateMoveSpeedInfo;
			subFactories[ModifierType.MovementSpeed2] = CreateMoveSpeed2Info;
			subFactories[ModifierType.Healing] = CreateHealingInfo;
			subFactories[ModifierType.Stats] = CreateStatsInfo;
			subFactories[ModifierType.Invisible] = CreateInvisibleInfo;
			subFactories[ModifierType.Dash] = CreateDashInfo;
			subFactories[ModifierType.ColliderConfig] = CreateColliderConfigInfo;
			subFactories[ModifierType.SuperArmor] = CreateSuperArmorInfo;
			subFactories[ModifierType.WeakArmor] = CreateWeakArmorInfo;
			subFactories[ModifierType.Immune] = CreateImmuneInfo;
			subFactories[ModifierType.Scale] = CreateScaleInfo;
			subFactories[ModifierType.StunBreak] = CreateStunBreakInfo;
			subFactories[ModifierType.IkAnimation] = CreateIkInfo;
			subFactories[ModifierType.AetherOnDamaged] = CreateAetherOnDamagedInfo;
			subFactories[ModifierType.Sfx] = CreateSfxInfo;
			subFactories[ModifierType.Static] = CreateStaticInfo;
			subFactories[ModifierType.Bleed] = CreateBleedInfo;
			subFactories[ModifierType.HpShield] = CreateHpShieldInfo;
			subFactories[ModifierType.LifeSteal] = CreateLifeStealInfo;
			subFactories[ModifierType.MaxCooldown] = CreateMaxCooldownInfo;
			subFactories[ModifierType.MaxAether] = CreateMaxAetherInfo;
			subFactories[ModifierType.AetherRecovery] = CreateAetherRecoveryInfo;
			subFactories[ModifierType.ParentSkillEventDispatcher] = CreateParentSkillEventDispatcherInfo;
			subFactories[ModifierType.CooldownBySecond] = CreateCooldownBySecondInfo;
			subFactories[ModifierType.SuperAtk] = CreateSuperAtkInfo;
			subFactories[ModifierType.Sleep] = CreateSleepInfo;
			subFactories[ModifierType.Freeze] = CreateFreezeInfo;
			subFactories[ModifierType.AttachedVfx] = CreateAttachedVfxInfo;
			subFactories[ModifierType.ComboDamageType] = CreateComboDamageTypeInfo;
			subFactories[ModifierType.DamageTypeOverride] = CreateDamageTypeOverrideInfo;
			subFactories[ModifierType.Wind] = CreateWindInfo;
			subFactories[ModifierType.Recast] = CreateRecastInfo;
			subFactories[ModifierType.CcBreakByInput] = CreateCcBreakByInputInfo;
			subFactories[ModifierType.CooldownBySecondUponHitTarget] = CreateCooldownBySecondUponHitTargetInfo;
			subFactories[ModifierType.DarkEnergy] = CreateDarkEnergyInfo;
			subFactories[ModifierType.HealByRawAtkUponEnemyHit] = CreateHealByRawAtkUponEnemyHitInfo;
		}

		public ModifierInfoFactory(CollectionOfInteractions collectionOfInteractions, WallHitConfig whc) {
			this.collectionOfInteractions = collectionOfInteractions;
			this.wallHitConfig = whc;
		}

		public ModifierInfo CreateFrom(Skill parentSkill, BaseModifierConfig baseModifierConfig,
		                               Environment environment, float projectileAge = 0) {
			FrameAndSecondsConverter _30fps = FrameAndSecondsConverter._30Fps;
			float delayToApply = _30fps.FramesToSeconds(baseModifierConfig.delayToApplyInFrames);
			ModifierType modifierType = baseModifierConfig.ShowModifierType();
			if (!subFactories.ContainsKey(modifierType)) {
				throw new Exception("Missing logic to create modifier info of type " + modifierType);
			}

			Params p = new Params(
				baseModifierConfig, delayToApply, projectileAge, parentSkill
			);
			return subFactories[modifierType](p);
		}

		private static ModifierInfo CreateStaggerInfo(Params p) {
			List<StaggerInfo.Behavior> behaviors = new List<StaggerInfo.Behavior>();
			StaggerModifierConfig smc = (StaggerModifierConfig) p.baseModifierConfig;
			if (smc.interruptTargetSkill) {
				behaviors.Add(StaggerInfo.Behavior.InterruptTargetSkill);
			}

			return new StaggerInfo(
				Target.Target, p.parentSkill, smc.distance, smc.movementDuration, smc.successRate, p.delayToApply, smc.ShowLevel(),
				behaviors.ToArray(), smc.overrideAnimation,
				p.baseModifierConfig.ListEnabledVfx(), smc.ShowMovementBehavior(), smc.ShowFacingBehavior(),
				smc.ShowRequirement(), smc.lifetimes,
				smc.loopAnimation, smc.crossfade, smc.animFrame
			);
		}

		private static ModifierInfo CreateKnockdownInfo(Params p) {
			KnockdownModifierConfig kmc = (KnockdownModifierConfig) p.baseModifierConfig;

			ModifierInfo info = null;
			return new KnockdownInfo(
				Target.Target, kmc.successRate, p.delayToApply, kmc.ShowLevel(), kmc.height, kmc.timeToPeak,
				kmc.timeToGround, kmc.distance, kmc.timeToLie, kmc.lieToIdleDuration, kmc.floatingDuration,
				kmc.enableWallHit, kmc.ShowAnimationMixingTable(), kmc.ListAllEnabledEvents(),
				p.baseModifierConfig.ListEnabledVfx(), kmc.ShowMovementBehavior(), kmc.ShowFacingBehavior(),
				kmc.ShowRequirement(), kmc.lifetimes, kmc.floatingMove, kmc.stopHorizontal
			);
		}

		private static ModifierInfo CreateLauncherInfo(Params p) {
			LauncherModifierConfig lmc = (LauncherModifierConfig) p.baseModifierConfig;

			return new LauncherInfo(
				Target.Target, lmc.successRate, p.delayToApply, lmc.ShowLevel(), lmc.height, lmc.timeToPeak,
				lmc.timeToGround, lmc.lieToIdleDuration, lmc.distance, lmc.timeToLie, 1, lmc.floatingDuration,
				lmc.enableWallHit, lmc.ShowAnimationMixingTable(), lmc.ListAllEnabledEvents(),
				p.baseModifierConfig.ListEnabledVfx(), lmc.ShowMovementBehavior(), lmc.ShowFacingBehavior(),
				lmc.ShowRequirement(), lmc.lifetimes, lmc.floatingMove, lmc.stopHorizontal
			);
		}

		private static ModifierInfo CreateTripInfo(Params p) {
			TripModifierConfig tmc = (TripModifierConfig) p.baseModifierConfig;

			return new TripInfo(
				Target.Target, tmc.successRate, p.delayToApply, tmc.ShowLevel(), tmc.height, tmc.timeToPeak,
				tmc.timeToGround, tmc.distance, tmc.timeToLie, tmc.lieToIdleDuration, tmc.floatingDuration,
				tmc.enableWallHit, tmc.ShowAnimationMixingTable(), tmc.ListAllEnabledEvents(),
				p.baseModifierConfig.ListEnabledVfx(), tmc.ShowMovementBehavior(), tmc.ShowFacingBehavior(),
				tmc.ShowRequirement(), tmc.lifetimes, tmc.floatingMove, tmc.stopHorizontal
			);
		}

		private static ModifierInfo CreateBlastInfo(Params p) {
			BlastModifierConfig bmc = (BlastModifierConfig) p.baseModifierConfig;
			return new BlastInfo(
				Target.Target, bmc.successRate, p.delayToApply, bmc.ShowLevel(), bmc.height, bmc.timeToPeak, bmc.timeToGround,
				bmc.flightDistance, bmc.flightMinSpeed, bmc.rollDistance, bmc.rollDuration, bmc.timeToLie, bmc.lieToIdleDuration,
				bmc.enableWallHit, bmc.ShowAnimationMixingTable(),
				bmc.ListAllEnabledEvents(), bmc.ShowAnimationProfile(), p.baseModifierConfig.ListEnabledVfx(),
				bmc.ShowMovementBehavior(), bmc.ShowFacingBehavior(), bmc.ShowRequirement(), bmc.lifetimes
			);
		}

		private static ModifierInfo CreateLockFrameInfo(Params p) {
			LockFrameModifierConfig lfmc = (LockFrameModifierConfig) p.baseModifierConfig;
			FrameAndSecondsConverter _30fps = FrameAndSecondsConverter._30Fps;
			return new LockFrameInfo(
				Target.Target, lfmc.successRate, p.delayToApply,
				_30fps.FramesToSeconds(lfmc.delayInFramesForCaster),
				_30fps.FramesToSeconds(lfmc.delayInFramesForTarget),
				_30fps.FloatFramesToSeconds(lfmc.durationInFramesForCaster),
				_30fps.FloatFramesToSeconds(lfmc.durationInFramesForTarget), lfmc.lockGlobally,
				_30fps.FramesToSeconds(lfmc.delayInFramesForGlobal),
				_30fps.FramesToSeconds(lfmc.durationInFramesForGlobal), p.baseModifierConfig.ListEnabledVfx(),
				p.baseModifierConfig.lifetimes
			);
		}

		private static ModifierInfo CreateStunInfo(Params p) {
			StunModifierConfig stunMc = (StunModifierConfig) p.baseModifierConfig;
			return new StunInfo(Target.Target, stunMc, p.parentSkill);
		}

		private static ModifierInfo CreateCameraFxShakeInfo(Params p) {
			CameraShakeFxConfig csfc = (CameraShakeFxConfig) p.baseModifierConfig;
			return new CameraFxShakeInfo(
				Target.Target, csfc, p.baseModifierConfig.ListEnabledVfx());
		}

		private static ModifierInfo CreateAdvancedFrameToSelfInfo(Params p) {
			AdvancedFrameModifierConfig afmc = (AdvancedFrameModifierConfig) p.baseModifierConfig;
			return new AdvancedFrameInfo(
				Target.Self, afmc.successRate, p.delayToApply,
				FrameAndSecondsConverter._30Fps.FramesToSeconds(afmc.valueInFrame),
				FrameAndSecondsConverter._30Fps.FramesToSeconds(afmc.channelingValueInFrame),
				FrameAndSecondsConverter._30Fps.FramesToSeconds(afmc.stateBinding),
				p.baseModifierConfig.ListEnabledVfx(), afmc.ShowIcon(),
				afmc.lifetimes
			);
		}

		private static ModifierInfo CreateVibrateInfo(Params p) {
			VibrateModifierConfig vmc = (VibrateModifierConfig) p.baseModifierConfig;
			return new VibrateInfo(
				Target.Target, vmc.successRate, p.delayToApply, vmc.xAmplitude, vmc.frequency,
				vmc.shouldDecay, vmc.decayConstant, p.baseModifierConfig.ListEnabledVfx(),
				vmc.ShowIcon(), vmc.lifetimes
			);
		}

		private static ModifierInfo CreatePlayImpactVfxInfo(Params p) {
			ImpactVfxModifierConfig ivmc = (ImpactVfxModifierConfig) p.baseModifierConfig;
			return new PlayImpactVfxInfo(
				Target.Target, ivmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreatePauseMovementInfo(Params p) {
			PauseMovementModifierConfig pmmc = (PauseMovementModifierConfig) p.baseModifierConfig;
			return new PauseMovementInfo(
				Target.Target, pmmc, p.baseModifierConfig.ListEnabledVfx());
		}

		private static ModifierInfo CreatePauseAnimationInfo(Params p) {
			PauseAnimationModifierConfig pamc = (PauseAnimationModifierConfig) p.baseModifierConfig;
			return new PauseAnimationInfo(
				Target.Target, pamc, p.baseModifierConfig.ListEnabledVfx());
		}

		private static ModifierInfo CreateShackleInfo(Params p) {
			ShackleModifierConfig smc = (ShackleModifierConfig) p.baseModifierConfig;
			return new ShackleInfo(Target.Target, smc, p.baseModifierConfig.ListEnabledVfx());
		}

		private static ModifierInfo CreateRagdollInfo(Params p) {
			RagdollModifierConfig rmc = (RagdollModifierConfig) p.baseModifierConfig;
			return new RagdollInfo(
				rmc, p.baseModifierConfig.ListEnabledVfx(), p.projectileAge);
		}

		private static ModifierInfo CreatePlayAnimationInfo(Params p) {
			PlayAnimationModifierConfig pamc = (PlayAnimationModifierConfig) p.baseModifierConfig;
			return new PlayAnimationInfo(
				pamc, p.baseModifierConfig.ListEnabledVfx(), p.baseModifierConfig.lifetimes
			);
		}

		private static ModifierInfo CreateVanishInfo(Params p) {
			VanishModifierConfig vmc = (VanishModifierConfig) p.baseModifierConfig;
			return new VanishInfo(
				Target.Target, vmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateDamageOverTimeInfo(Params p) {
			DamageOverTimeModifierConfig dotmc = (DamageOverTimeModifierConfig) p.baseModifierConfig;
			return new DamageOverTimeInfo(
				Target.Target, dotmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateHitboxTransformInfo(Params p) {
			HitboxTransformModifierConfig htmc = (HitboxTransformModifierConfig) p.baseModifierConfig;
			return new HitboxTransformInfo(
				p.parentSkill, Target.Self, htmc,
				p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateKnockbackWeightInfo(Params p) {
			KnockbackWeightModifierConfig kwmc = (KnockbackWeightModifierConfig) p.baseModifierConfig;
			return new KnockbackWeightInfo(
				Target.Self, kwmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateKnockdownWeightInfo(Params p) {
			KnockdownWeightModifierConfig kwmc = (KnockdownWeightModifierConfig) p.baseModifierConfig;
			return new KnockdownWeightInfo(
				Target.Self, kwmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateMoveSpeedInfo(Params p) {
			MoveSpeedModifierConfig msmc = (MoveSpeedModifierConfig) p.baseModifierConfig;
			return new MoveSpeedInfo(
				Target.Target, msmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateMoveSpeed2Info(Params p) {
			MoveSpeedModifierConfig msmc = (MoveSpeedModifierConfig) p.baseModifierConfig;
			return new MoveSpeed2Info(
				Target.Target, msmc, p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateHealingInfo(Params p) {
			HealingModifierConfig hmc = (HealingModifierConfig) p.baseModifierConfig;
			return new HealingInfo(
				p.parentSkill, Target.Target, hmc,
				p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateStatsInfo(Params p) {
			StatsModifierConfig smc = (StatsModifierConfig) p.baseModifierConfig;
			return new StatsInfo(
				p.parentSkill, Target.Target, smc,
				p.baseModifierConfig.ListEnabledVfx()
			);
		}

		private static ModifierInfo CreateInvisibleInfo(Params p) {
			InvisibleModifierConfig imc = (InvisibleModifierConfig) p.baseModifierConfig;
			return new InvisibleInfo(
				Target.Target, imc.ListEnabledVfx(), imc, p.parentSkill
			);
		}

		private static ModifierInfo CreateDashInfo(Params p) {
			List<StaggerInfo.Behavior> behaviors = new List<StaggerInfo.Behavior>();
			DashModifierConfig dmc = (DashModifierConfig) p.baseModifierConfig;
			if (dmc.interruptTargetSkill) {
				behaviors.Add(StaggerInfo.Behavior.InterruptTargetSkill);
			}

			return new DashInfo(
				Target.Target, p.parentSkill, dmc.distance, dmc.movementDuration, dmc.successRate, p.delayToApply, dmc.ShowLevel(),
				behaviors.ToArray(), dmc.overrideAnimation,
				p.baseModifierConfig.ListEnabledVfx(), dmc.ShowMovementBehavior(), dmc.ShowFacingBehavior(),
				dmc.enableAnim, dmc.lifetimes
			);
		}

		private static ModifierInfo CreateColliderConfigInfo(Params p) {
			ColliderConfigModifierConfig ccmc = (ColliderConfigModifierConfig) p.baseModifierConfig;
			return new ColliderConfigInfo(
				Target.Target, ccmc.ListEnabledVfx(), ccmc
			);
		}

		private static ModifierInfo CreateSuperArmorInfo(Params p) {
			SuperArmorModifierConfig samc = (SuperArmorModifierConfig) p.baseModifierConfig;
			return new SuperArmorInfo(Target.Target, samc, p.parentSkill);
		}

		private static ModifierInfo CreateWeakArmorInfo(Params p) {
			WeakArmorModifierConfig wamc = (WeakArmorModifierConfig) p.baseModifierConfig;
			return new WeakArmorInfo(Target.Target, wamc);
		}

		private static ModifierInfo CreateImmuneInfo(Params p) {
			return new ImmuneInfo(
				Target.Target, (ImmuneModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateScaleInfo(Params p) {
			return new ScaleInfo(Target.Target, (ScaleModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateStunBreakInfo(Params p) {
			return new StunBreakInfo(Target.Target, (StunBreakModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateIkInfo(Params p) {
			return new IkInfo(Target.Target, (IkModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateAetherOnDamagedInfo(Params p) {
			return new AetherOnDamagedInfo(Target.Target, (AetherOnDamagedModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateSfxInfo(Params p) {
			return new SfxInfo(Target.Target, (SfxModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateStaticInfo(Params p) {
			return new StaticInfo(Target.Target, (StaticModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateBleedInfo(Params p) {
			return new BleedInfo(Target.Target, (BleedModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateHpShieldInfo(Params p) {
			return new HpShieldInfo(Target.Target, (HpShieldModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateLifeStealInfo(Params p) {
			return new LifeStealInfo(Target.Target, (LifeStealModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateMaxCooldownInfo(Params p) {
			return new MaxCooldownInfo(Target.Target, (MaxCooldownModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateMaxAetherInfo(Params p) {
			return new MaxAetherInfo(Target.Target, (MaxAetherModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateAetherRecoveryInfo(Params p) {
			return new AetherRecoveryInfo(Target.Target, (AetherRecoveryModifierConfig) p.baseModifierConfig);
		}

		private static ModifierInfo CreateParentSkillEventDispatcherInfo(Params p) {
			return new ParentSkillEventDispatcherInfo(
				(ParentSkillEventDispatcherModifierConfig) p.baseModifierConfig,
				p.parentSkill, Target.Target
			);
		}

		private static ModifierInfo CreateCooldownBySecondInfo(Params p) {
			return new CooldownBySecondInfo(
				Target.Target, (CooldownBySecondModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateSuperAtkInfo(Params p) {
			return new SuperAtkInfo(
				Target.Target, (SuperAtkModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateSleepInfo(Params p) {
			return new SleepInfo(
				Target.Target, (SleepModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateFreezeInfo(Params p) {
			return new FreezeInfo(
				Target.Target, (FreezeModifierConfig) p.baseModifierConfig, p.parentSkill
			);
		}

		private static ModifierInfo CreateAttachedVfxInfo(Params p) {
			return new AttachedVfxInfo(
				Target.Target, (AttachedVfxModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateComboDamageTypeInfo(Params p) {
			return new ComboDamageTypeModifierInfo(
				Target.Target, (ComboDamageTypeModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateDamageTypeOverrideInfo(Params p) {
			return new DamageTypeOverrideModifierInfo(
				Target.Target, (DamageTypeOverrideModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateWindInfo(Params p) {
			return new WindInfo(
				Target.Target, (WindModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateRecastInfo(Params p) {
			return new RecastInfo(
				Target.Target, (RecastModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateCcBreakByInputInfo(Params p) {
			return new CcBreakByInputInfo(
				Target.Target, (CcBreakByInputModifierConfig) p.baseModifierConfig, p.parentSkill
			);
		}

		private static ModifierInfo CreateCooldownBySecondUponHitTargetInfo(Params p) {
			return new CooldownBySecondUponHitTargetInfo(
				Target.Target, (CooldownBySecondUponHitTargetModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateDarkEnergyInfo(Params p) {
			return new DarkEnergyInfo(
				Target.Target, (DarkEnergyModifierConfig) p.baseModifierConfig
			);
		}

		private static ModifierInfo CreateHealByRawAtkUponEnemyHitInfo(Params p) {
			return new HealByRawAtkUponEnemyHitInfo(
				Target.Target, (HealByRawAtkUponEnemyHitModifierConfig) p.baseModifierConfig
			);
		}
	}

	public partial class ModifierInfoFactory {
		public struct Params {
			public BaseModifierConfig baseModifierConfig;
			public float delayToApply;
			public float projectileAge;
			public Skill parentSkill;

			public Params(BaseModifierConfig baseModifierConfig, float delayToApply,
			              float projectileAge, Skill parentSkill) {
				this.baseModifierConfig = baseModifierConfig;
				this.delayToApply = delayToApply;
				this.projectileAge = projectileAge;
				this.parentSkill = parentSkill;
			}
		}
	}
}