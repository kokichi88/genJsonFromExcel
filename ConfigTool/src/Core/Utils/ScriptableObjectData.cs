using RSG;
using SSAR.Combat.AISystem;
using Ssar.Combat.HackTool;
using Ssar.Combat.UI;
using Tower.Requests.Environment;
using Utils;

public class ScriptableObjectData {
	public static readonly string FOLDER = "ScriptableObject";
	public static readonly string AI_TOOL_PATH = FOLDER + "/AIConfig";
	public static readonly string GAME_AILOG_PATH = FOLDER + "/AILogConfig";
	public static readonly string HUD_CONFIG_PATH = FOLDER + "/HUDConfig";
	public static readonly string HACK_TOOL_PATH = FOLDER + "/HackTool";
	public static readonly string EFFECT_CONFIG_PATH = FOLDER + "/EffectConfig";
	public static readonly string SOUND_CONFIG_PATH = FOLDER + "/SoundConfig";
	public static readonly string ENVIRONMENT_CONFIG_PATH = FOLDER + "/EnvironmentConfig";
	public static HUDConfig HudConfig;
	public static HackTool HackTool;
	public static EffectConfig EffectConfig;
	public static SoundConfig SoundConfig;
	public static EnvironmentConfig EnvironmentConfig;

	public static IPromise<AIAsset> LoadAI(string aiPath) {
		IPromise<AIAsset> p = ResourceLoaderPromise.Load<AIAsset>(aiPath,Context.Dungeon,ResourceLoadMode.Instantly);
		return p;
	}
	public static IPromise<HUDConfig> InitHUD() {
		IPromise<HUDConfig> p = ResourceLoaderPromise.Load<HUDConfig>(HUD_CONFIG_PATH,Context.OutOfDungeon,ResourceLoadMode.Instantly);
		p.Then(tools => HudConfig = tools);
		return p;
	}
	public static IPromise<EffectConfig> InitEffectConfig() {
		IPromise<EffectConfig> p = ResourceLoaderPromise.Load<EffectConfig>(EFFECT_CONFIG_PATH,Context.OutOfDungeon,ResourceLoadMode.Instantly);
		p.Then(tools => EffectConfig = tools);
		return p;
	}
	public static IPromise<HackTool> InitHackTool() {
		IPromise<HackTool> p = ResourceLoaderPromise.Load<HackTool>(HACK_TOOL_PATH,Context.OutOfDungeon,ResourceLoadMode.Instantly);
		p.Then(tools => HackTool = tools);
		return p;
	}
	public static IPromise<SoundConfig> InitSoundConfig() {
		IPromise<SoundConfig> p = ResourceLoaderPromise.Load<SoundConfig>(SOUND_CONFIG_PATH,Context.OutOfDungeon,ResourceLoadMode.Instantly);
		p.Then(tools => SoundConfig = tools);
		return p;
	}

	public static IPromise<EnvironmentConfig> InitEnvironmentConfig()
	{
		IPromise<EnvironmentConfig> p = ResourceLoaderPromise.Load<EnvironmentConfig>(ENVIRONMENT_CONFIG_PATH, Context.OutOfDungeon, ResourceLoadMode.Instantly);
		p.Then(config =>
		{
			EnvironmentConfig = config;
			EnvironmentConfig.Deserialize();
		});
		return p;
	}
}
