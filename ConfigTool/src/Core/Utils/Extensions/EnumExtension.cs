using System;
using System.Collections.Generic;
using CharacterSelection;
using Combat.Stats;
using Core.Commons;
using Equipment;
using JsonConfig;
using JsonConfig.Model;
using Login;
using Mastery;
using Misc;
using Ssar.Combat.UI;
using UnityEngine;
using Utils;
using WorldMap;

namespace Core.Utils.Extensions
{
    public static class EnumExtension
    {
        private static Enum[] enumArray;
        private static string[] stringArray;
        private static bool init;

        static void Init(Enum enum_)
        {
            Array enumValues = Enum.GetValues(enum_.GetType());

            enumArray = new Enum[enumValues.Length];
            stringArray = new string[enumValues.Length];
            int i = 0;
            foreach (object value in enumValues)
            {
                Enum e_ = (Enum) value;
                string string_ = e_.ToString();
                enumArray[i] = e_;
                stringArray[i] = string_;

                i++;
            }
        }

        public static string ToStringValueGeneric(this Enum enum_)
        {
            return ToStringValueArrGeneric(enum_);
        }

        public static string ToStringValueArrGeneric(this Enum enum_)
        {
            if (!init)
            {
                Init(enum_);
                init = true;
            }

            return stringArray[enumArray.IndexOf(enum_)];
        }

        public static Enum FromStringValueGeneric(this Enum enum_, string stringValue)
        {
            return FromStringValueArrGeneric(enum_, stringValue);
        }

        public static Enum FromStringValueArrGeneric(this Enum enum_, string stringValue)
        {
            if (!init)
            {
                Init(enum_);
                init = true;
            }

            return enumArray[stringArray.IndexOf(stringValue)];
        }

        public static bool Has<T>(this System.Enum type, T value)
        {
            try
            {
                return (((int) (object) type & (int) (object) value) == (int) (object) value);
            }
            catch
            {
                return false;
            }
        }

        public static bool Is<T>(this System.Enum type, T value)
        {
            try
            {
                return (int) (object) type == (int) (object) value;
            }
            catch
            {
                return false;
            }
        }

        public static T Add<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) (((int) (object) type | (int) (object) value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Could not append value from enumerated type '{0}'.", typeof(T).Name), ex);
            }
        }

        public static T Remove<T>(this System.Enum type, T value)
        {
            try
            {
                return (T) (object) (((int) (object) type & ~(int) (object) value));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(
                    string.Format("Could not remove value from enumerated type '{0}'.", typeof(T).Name), ex);
            }
        }

        public static int ToInt(this Grade grade)
        {
            return (int) (grade) + 1;
        }
        public static EquipmentCategory ToEquipmentCategory(this EquipmentType equipmentType)
        {
            switch (equipmentType)
            {
                case EquipmentType.Weapon:
                    return EquipmentCategory.Weapon;
                case EquipmentType.Helm:
                case EquipmentType.Chest:
                case EquipmentType.Pant:
                    return EquipmentCategory.Armor;
                case EquipmentType.Amulet:
                case EquipmentType.Ring:
                    return EquipmentCategory.Accessory;
                default:
                    throw new Exception("Unfinifed " + equipmentType);
            }
        }

        public static int ToInt(this Difficulty difficulty)
        {
            return (int) difficulty;
        }
        
        public static int ToInt(this MasterySlotType slotType)
        {
            return (int) slotType;
        }

        public static List<EquipmentType> SeperateToEquipmentTypes(this EquipmentCategory equipmentCategory)
        {
            List<EquipmentType> ret = new List<EquipmentType>();
            switch (equipmentCategory)
            {
                case EquipmentCategory.Weapon:
                    ret.Add(EquipmentType.Weapon);
                    break;
                case EquipmentCategory.Armor:
                    ret.Add(EquipmentType.Helm);
                    ret.Add(EquipmentType.Chest);
                    ret.Add(EquipmentType.Pant);
                    break;
                case EquipmentCategory.Accessory:
                    ret.Add(EquipmentType.Amulet);
                    ret.Add(EquipmentType.Ring);
                    break;
            }

            return ret;
        }

        public static Rarity Increase(this Rarity rarity)
        {
            return (Rarity) ((int) rarity + 1);
        }

        public static bool IsValid(this CharacterEquipmentSubTabType subTabType, EquipmentType equipmentType,
            int visualId, CharacterId characterId, ConfigManager configManager)
        {
            return subTabType.IsValid(equipmentType, visualId, characterId.GetEquipmentGenerationConfig(configManager));
        }

        public static bool IsValid(this CharacterEquipmentSubTabType subTabType, EquipmentType equipmentType,
            int visualId, EquipmentGenerationConfig equipmentGenerationConfig)
        {
            switch (subTabType)
            {
                case CharacterEquipmentSubTabType.All:
                    return true;
                case CharacterEquipmentSubTabType.Weapon:
                    return equipmentType == EquipmentType.Weapon;
                case CharacterEquipmentSubTabType.Pant:
                    return equipmentType == EquipmentType.Helm;
                case CharacterEquipmentSubTabType.Helm:
                    return equipmentType == EquipmentType.Chest;
                case CharacterEquipmentSubTabType.Chest:
                    return equipmentType == EquipmentType.Pant;
                case CharacterEquipmentSubTabType.Ring:
                    return equipmentType == EquipmentType.Ring;
                case CharacterEquipmentSubTabType.Amulet:
                    return equipmentType == EquipmentType.Amulet;
            }

            return false;
        }

        public static Rarity ToRarity(this EquipmentRarityFilter rarityFilter)
        {
            switch (rarityFilter)
            {
                case EquipmentRarityFilter.Ultimate:
                    return Rarity.Ultimate2;
                case EquipmentRarityFilter.Mythic:
                    return Rarity.Ultimate;
                case EquipmentRarityFilter.Legendary:
                    return Rarity.Mythic;
                case EquipmentRarityFilter.Rare:
                    return Rarity.Legendary;
                case EquipmentRarityFilter.Magic:
                    return Rarity.Epic;
                case EquipmentRarityFilter.Uncommon:
                    return Rarity.Magic;
                case EquipmentRarityFilter.Common:
                    return Rarity.Common;
                default:
                    throw new Exception("Undefined");
            }
        }
        

        public static GameObject Prefab(this EffectType pathIndex)
        {
            GameObject fx = null;
            if (pathIndex != EffectType.None)
            {
                fx = PrefabUtils.LoadPrefab(ScriptableObjectData.EffectConfig.GetPath(pathIndex));
                return fx;
            }

            return fx;
        }

        public static bool IsHighestDifficulty(this Difficulty difficulty,DungeonMode dungeonMode,ConfigManager configManager)
        {
            return difficulty == dungeonMode.GetModeLogic().GetHighestDifficulty(configManager);
        }

        public static HUDTextInfo HudTextInfo(this HUDTextType hudTextType)
        {
            return global::Utils.Utils.GetHudConfig().GetHUDTextInfo(hudTextType);
        }

        public static HUDTextInfo GetHudTextInfo(this HUDTextType hudTextType)
        {
            return global::Utils.Utils.GetHudConfig().GetHUDTextInfo(hudTextType);
        }

        public static Difficulty Next(this Difficulty difficulty)
        {
            return (Difficulty) ((int) (difficulty) + 1);
        }

        public static Difficulty Previous(this Difficulty difficulty)
        {
            return (Difficulty) ((int) (difficulty) - 1);
        }

        public static string ToLocalize(this ErrorCode loginErrorCode)
        {
            return LocalizationText.GetString(loginErrorCode.ToString().ToUpper()) + "(" + (int) (loginErrorCode) + ")";
        }
        public static CraftingPopupTabType ToCraftingPopupTabType(this EquipmentType equipmentType)
        {
            if (equipmentType == EquipmentType.Weapon) return CraftingPopupTabType.Weapon;
            return CraftingPopupTabType.Armor_Acessories;
        }

        public static string GetLocalize(this EquipmentType equipmentType)
        {
            return LocalizationText.GetString(equipmentType.ToString().ToUpper());
        }

        public static string GetLocalize(this Rarity rarity)
        {
            return LocalizationText.GetString(rarity.ToString().ToUpper());
        }

        public static SetType ToSetType(this SetTypeFilter setTypeFilter)
        {
            switch (setTypeFilter)
            {
                case SetTypeFilter.Fire:
                    return SetType.Fire;
                case SetTypeFilter.Ice:
                    return SetType.Ice;
                case SetTypeFilter.Poison:
                    return SetType.Poison;
                case SetTypeFilter.Physic:
                    return SetType.Physic;
                case SetTypeFilter.Lightning:
                    return SetType.Lightning;
            }

            return SetType.Invalid;
        }

        public static string ToGradeText(this Grade grade)
        {
            int v = 0;
            for (int i = 0; i < (int) grade; i++)
            {
                v += 1;
            }

            return v.ToString();
        }
        public static string CardFullDesc(this CardType cardType)
        {
            return LocalizationText.GetString("CARD_" + cardType.ToString().ToUpper() + "_FULL_DESC");
        }

        public static string ToLocalize(this System.Enum type)
        {
            return LocalizationText.GetString(type.ToString().ToUpper());
        }
    }
}