using System;
using System.Collections.Generic;
using Combat.CombatContext.Dungeon;
using Combat.Utils;
using Core.Commons;
using Core.Skills;
using EntrySystem;
using Equipment;
using Gem;
using JsonConfig;
using JsonConfig.Model;
using MEC;
using strange.extensions.mediation.api;
using Ssar.Combat.UI;
using UI;
using UnityEngine;
using Utils;
using WorldMap;
using Object = System.Object;

namespace Core.Utils.Extensions
{
	public static class MiscExtension
	{
		public static Dictionary<EquipmentType, int> ToDict(this EquipmentTypeRate[] equipmentTypeRates)
		{
			Dictionary<EquipmentType, int> ret = new Dictionary<EquipmentType, int>();
			foreach (EquipmentTypeRate equipmentTypeRate in equipmentTypeRates)
			{
				if (!ret.ContainsKey(equipmentTypeRate.EquipmentType))
				{
					ret.Add(equipmentTypeRate.EquipmentType, 0);
				}

				ret[equipmentTypeRate.EquipmentType] += equipmentTypeRate.Rate;
			}

			return ret;
		}

		public static Dictionary<Rarity, int> ToDict(this RarityRate[] rarityRates)
		{
			Dictionary<Rarity, int> ret = new Dictionary<Rarity, int>();
			foreach (RarityRate rarityRate in rarityRates)
			{
				if (!ret.ContainsKey(rarityRate.Rarity))
				{
					ret.Add(rarityRate.Rarity, 0);
				}

				ret[rarityRate.Rarity] += rarityRate.Rate;
			}

			return ret;
		}

		public static Dictionary<Grade, int> ToDict(this GradeRate[] gradeRates)
		{
			Dictionary<Grade, int> ret = new Dictionary<Grade, int>();
			foreach (GradeRate gradeRate in gradeRates)
			{
				if (!ret.ContainsKey(gradeRate.Grade))
				{
					ret.Add(gradeRate.Grade, 0);
				}

				ret[gradeRate.Grade] += gradeRate.Rate;
			}

			return ret;
		}

		public static int TotalRate(this IRate[] value)
		{
			int ret = 0;
			foreach (IRate rate in value)
			{
				ret += rate.Rate;
			}

			return ret;
		}

		public static bool IsNullOrEmpty(this double[] value)
		{
			if (value == null)
			{
				return true;
			}

			if (value.Length == 0)
			{
				return true;
			}

			return false;
		}

		public static float Truncate(this float value, int digits)
		{
			double mult = Math.Pow(10.0, digits);
			double result = Math.Truncate(mult * value) / mult;
			return (float) result;
		}

		public static double Truncate(this double value, int digits)
		{
			double mult = Math.Pow(10.0, digits);
			double result = Math.Truncate(mult * value) / mult;
			return (float) result;
		}

		public static void WaitUntilFinshRegister(this IView view, Action onFinish)
		{
			Timing.RunCoroutine(WaitUntilFinshRegisterIE(view, onFinish));
		}

		private static IEnumerator<float> WaitUntilFinshRegisterIE(IView view, Action onFinsh)
		{
			while (!view.registeredWithContext)
			{
				yield return Timing.WaitForOneFrame;
			}

			onFinsh.Invoke();
		}

		public static CharacterId CharacterId(this MainCharacterData mainCharacterData)
		{
			return new CharacterId(mainCharacterData.GroupdId(), mainCharacterData.SubId());
		}

		public static HUDTextType ToHudTextType(this Rarity rarity)
		{
			switch (rarity)
			{
				case Rarity.Common:
					return HUDTextType.EquipmentCommon;
				case Rarity.Magic:
					return HUDTextType.EquipmentUncommon;
				case Rarity.Epic:
					return HUDTextType.EquipmentMagic;
				case Rarity.Legendary:
					return HUDTextType.EquipmentRare;
				case Rarity.Mythic:
					return HUDTextType.EquipmentLegendary;
				case Rarity.Ultimate:
					return HUDTextType.EquipmentMythic;
				case Rarity.Ultimate2:
					return HUDTextType.EquipmentUltimate;
				default:
#if UNITY_EDITOR
					throw new Exception("Not found hud type for " + rarity.ToString());
#endif
					return HUDTextType.EquipmentCommon;
			}
		}

		public static int GetTotalDiamond(this List<CharacterLevelUpRewardInfo> rewardInfos)
		{
			int ret = 0;
			foreach (CharacterLevelUpRewardInfo rewardInfo in rewardInfos)
			{
				ret += rewardInfo.diamond;
			}

			return ret;
		}

		public static int GetTotalGold(this List<CharacterLevelUpRewardInfo> rewardInfos)
		{
			int ret = 0;
			foreach (CharacterLevelUpRewardInfo rewardInfo in rewardInfos)
			{
				ret += rewardInfo.gold;
			}

			return ret;
		}
		public static int GetTotalAdventureKey(this List<CharacterLevelUpRewardInfo> rewardInfos)
		{
			int ret = 0;
			foreach (CharacterLevelUpRewardInfo rewardInfo in rewardInfos)
			{
				ret += rewardInfo.adventureKey;
			}

			return ret;
		}
		public static ReplaceEquipmentInfo GetReplaceEquipmentInfo(this MainCharacterData mainCharacterData,
			EquipmentType equipmentType)
		{
			EquipmentCollectData equipmentCollectData = mainCharacterData.InventoryData.GetEquipment(mainCharacterData
				.PresetData(Preset.Pve).GetEquippedEquipmentCollectId(equipmentType).ToEquipmentCollectId());
			if (equipmentCollectData != null)
			{
				EquipmentConfigId equipmentConfigId = equipmentCollectData.EquipmentConfigId;
				return new ReplaceEquipmentInfo(equipmentConfigId.VisualId, equipmentConfigId.Rarity);
			}

			return new ReplaceEquipmentInfo();
		}

		public static ReplaceEquipmentInfo GetReplaceEquipmentInfo(this EquipmentCollectData equipmentCollectData)
		{
			if (equipmentCollectData != null)
			{
				EquipmentConfigId equipmentConfigId = equipmentCollectData.EquipmentConfigId;
				return new ReplaceEquipmentInfo(equipmentConfigId.VisualId, equipmentConfigId.Rarity);
			}

			return new ReplaceEquipmentInfo();
		}

		public static ShowRewardPopupParameter ToShowRewardPopupParameter(this AbsRewardLogic itemLogic, Action onHide)
		{
			ShowRewardPopupParameter p = new ShowRewardPopupParameter(itemLogic, onHide);
			return p;
		}

		public static int ToIntValue(this DungeonId dungeonId)
		{
			int difficultyValue = (dungeonId.Difficulty.ToInt() + 1) * 1000000;
			int mapValue = (dungeonId.MapId + 1) * 1000;
			int nodeValue = (dungeonId.NodeId + 1);
			return difficultyValue + mapValue + nodeValue;
		}

		public static bool IsLastNodeOfDifficulty(this DungeonId dungeonId)
		{
			bool flag1 = dungeonId.NodeId == dungeonId.GetModeLogic().GetNumOfNodeInDifficulty();
			bool flag2 = dungeonId.MapId == dungeonId.GetModeLogic().GetHighestMapId();
			return flag1 && flag2;
		}

		public static bool IsSmallerThanOrEqual(this DungeonId a, DungeonId b)
		{
			return a.IsSmallerThan(b) || a.Equals(b);
		}
		public static bool IsSmallerThan(this DungeonId a, DungeonId b)
		{
			int nodeValue = a.ToIntValue();
			int nodeValueb = b.ToIntValue();
			return nodeValue < nodeValueb;
		}
		public static bool IsBiggerThanOrEqual(this DungeonId a, DungeonId b)
		{
			int nodeValue = a.ToIntValue();
			int nodeValueb = b.ToIntValue();
			return nodeValue >= nodeValueb;
		}
		public static bool IsBiggerThan(this DungeonId a, DungeonId b)
		{
			int nodeValue = a.ToIntValue();
			int nodeValueb = b.ToIntValue();
			return nodeValue >nodeValueb;
		}
		public static bool IsEqual(this DungeonId a, DungeonId b)
		{
			if (a == null) return false;
			return a.ToIntValue() == b.ToIntValue();
		}

		public static bool IsDifferent(this DungeonId a, DungeonId b)
		{
			int nodeValue = a.ToIntValue();
			int nodeValueb = b.ToIntValue();
			return nodeValue != nodeValueb;
		}

		public static bool IsContains(this EquipmentInfoContext[] contexts, EquipmentInfoContext context)
		{
			for (int i = 0; i < contexts.Length; i++)
			{
				if (contexts[i] == context)
				{
					return true;
				}
			}

			return false;
		}

		public static string ToHtml(this Color color)
		{
			return ColorUtility.ToHtmlStringRGBA(color);
		}

		public static void Resize(this UISprite sp, UIWidget.AspectRatioSource aspectRatioSource, int size)
		{
			sp.keepAspectRatio = UIWidget.AspectRatioSource.Free;
			sp.MakePixelPerfect();
			sp.keepAspectRatio = aspectRatioSource;
			if (aspectRatioSource == UIWidget.AspectRatioSource.BasedOnWidth)
			{
				sp.width = size;
			}

			if (aspectRatioSource == UIWidget.AspectRatioSource.BasedOnHeight)
			{
				sp.height = size;
			}
		}

		public static int GetChapter(this DungeonId dungeonId)
		{
			return 1;
			// return dungeonId.NodeId <= 10 ? 1 : 2;
		}

		public static bool IsPlayingFx(this GameObject value)
		{
			if (value != null)
			{
				ParticleSystem particleSystem = value.GetComponent<ParticleSystem>();
				if (particleSystem == null)
				{
					particleSystem = value.GetComponentInChildren<ParticleSystem>();
				}

				if (particleSystem != null)
				{
					return IsPlaying(particleSystem, 2);
				}
				
			}

			return false;
		}

		public static void PlayFx(this GameObject value, bool available = true,float percentToReset =0)
		{
			if (value != null)
			{
				if (available)
				{
					NGUITools.SetActiveSelf(value, true);
					ParticleSystem particleSystem = value.GetComponent<ParticleSystem>();
					if (particleSystem == null)
					{
						particleSystem = value.GetComponentInChildren<ParticleSystem>();
					}

					if (particleSystem != null)
					{
						if (percentToReset<=0)
						{
							particleSystem.Stop();
							particleSystem.Play(true);
						}
						else if(!IsPlaying(particleSystem,percentToReset))
						{
							particleSystem.Play();
						}
					}
				}
				else
				{
					value.StopFx();
				}
				
			}
		}

		private static bool IsPlaying(ParticleSystem particleSystem,float percentToReset)
		{
			ParticleSystem[] particleSystems = particleSystem.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem system in particleSystems)
			{
				if (system.isPlaying&&system.time/system.main.duration<=percentToReset)
				{
					return true;
				}
			}
			return false;
		}

		public static void StopFx(this GameObject value)
		{
			if (value != null)
			{
				NGUITools.SetActiveSelf(value, false);
			}
		}

		public static GameObject FindObject(this GameObject parent, string name)
		{
			Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
			foreach (Transform t in trs)
			{
				if (t.name == name)
				{
					return t.gameObject;
				}
			}

			return null;
		}

		public static void PlaySfx(this MonoBehaviour monoBehaviour, Sfx sfx)
		{
			if (SoundManager.instance != null)
			{
				SoundManager.instance.PlaySfx(sfx);
			}
		}

		public static void IgnoreAutoPlaySound(this GameObject value)
		{
			value.AddComponent<CustomSound>();
		}

		public static float GetFxDuration(this GameObject fx)
		{
			if (fx != null)
			{
				ParticleSystem[] particleSystems = fx.GetComponentsInChildren<ParticleSystem>(true);
				float ret = 0;
				foreach (ParticleSystem system in particleSystems)
				{
					if (!system.main.loop)
					{
						if (system.main.duration > ret)
						{
							ret = system.main.duration;
						}
					}
				}

				return ret;
			}

			return 0;
		}

		public static string ToSkillPath(this HeroConfig.SkillStats skillStats, CharacterId characterId)
		{
			string categoryFolderName = skillStats.ShowCategory().ShowParentSkillCategory().ToString();
			string pathToSkillFrameConfig = string.Format(
				ResourcePreload.PATH_TO_SKILL_CONFIG_FORMAT,
				characterId.GroupId,
				categoryFolderName,
				skillStats.groupId + "_" + skillStats.name + "_" + skillStats.level
			);
			return pathToSkillFrameConfig;
		}

		public static void Play(this Sfx sfx, bool checkInterrupt = false)
		{
			if (SoundManager.instance != null)
				SoundManager.instance.PlaySfx(sfx, false, checkInterrupt);
		}

		public static bool HasOneItem<T>(this T[] rate, ref T val) where T : IRate
		{
			List<T> ret = new List<T>();
			foreach (T rate1 in rate)
			{
				if (rate1.IsValid())
				{
					ret.Add(rate1);
				}
			}

			if (ret.Count == 1)
			{
				val = ret[0];
				return true;
			}

			return false;
		}

		public static bool IsSame(this GemId gemId, GemId b)
		{
			return gemId.Rarity == b.Rarity && gemId.StatType == b.StatType;
		}

		public static float ToFloat(this double val)
		{
			return (float) val;
		}

		public static DungeonMode GetDungeonMode(this DungeonId dungeonId)
		{
			return dungeonId.GetModeLogic().GetDungeonMode();
		}

		public static DungeonMode GetDungeonMode(this int mapId)
		{
			return mapId.GetModeLogic().GetDungeonMode();
		}

		public static List<Tuple<T1, T2>> ToList<T1, T2>(this Dictionary<T1, T2> dict)
		{
			List<Tuple<T1, T2>> ret = new List<Tuple<T1, T2>>();
			foreach (KeyValuePair<T1, T2> valuePair in dict)
			{
				ret.Add(new Tuple<T1, T2>(valuePair.Key, valuePair.Value));
			}

			return ret;
		}

		public static Dictionary<T1, T2> ToDict<T1, T2>(this List<Tuple<T1, T2>> tuples)
		{
			Dictionary<T1, T2> dict = new Dictionary<T1, T2>();
			for (int i = 0; i < tuples.Count; i++)
			{
				if (!dict.ContainsKey(tuples[i].Item1))
				{
					dict.Add(tuples[i].Item1, tuples[i].Item2);
				}
			}

			return dict;
		}

		public static Dictionary<T1, float> Merge<T1>(this Dictionary<T1, float> dict, Dictionary<T1, float> dict2)
		{
			foreach (KeyValuePair<T1, float> valuePair in dict2)
			{
				if (!dict.ContainsKey(valuePair.Key))
				{
					dict.Add(valuePair.Key, 0);
				}

				dict[valuePair.Key] += valuePair.Value;
			}

			return dict;
		}
	}
}