using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Achievement;
using EntrySystem;
using Equipment;
using JsonConfig.Model;
using LitJson;
using Shop;
using UI;
using UnityEngine;

namespace Core.Utils.Extensions
{
	public static class StringExtension
	{
		static Dictionary<string, Ret> cache = new Dictionary<string, Ret>();

		public static int GetNthIndex(this string s, char t, int n)
		{
			int count = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == t)
				{
					count++;
					if (count == n)
					{
						return i;
					}
				}
			}

			return -1;
		}

		public static string[] SplitOnCapitalLetters(this string inputString)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var ch in inputString)
			{
				if (char.IsUpper(ch) && sb.Length > 0)
				{
					sb.Append(' ');
				}

				sb.Append(ch);
			}

			return sb.ToString().Split(' ');
		}

		public static string[] SplitOnDigitOrLetter(this string inputString)
		{
			if (inputString.Length < 1) return new string[0];

			StringBuilder sb = new StringBuilder();

			char[] charArray = inputString.ToCharArray();
			sb.Append(charArray[0]);
			for (int i = 1; i < charArray.Length; i++)
			{
				char previousChar = charArray[i - 1];
				char currentChar = charArray[i];

				bool addSpace = false;
				if (char.IsDigit(previousChar))
				{
					if (char.IsLetter(currentChar))
					{
						addSpace = true;
					}
				}
				else
				{
					if (char.IsDigit(currentChar))
					{
						addSpace = true;
					}
				}

				if (addSpace)
				{
					sb.Append(' ');
				}

				sb.Append(currentChar);
			}

			return sb.ToString().Split(' ');
		}

		public static string ToFirstLetterUpcase(this string value)
		{
			value = value.ToLower();
			char[] array = value.ToCharArray();
			List<int> index = new List<int>();
			// Handle the first letter in the string.
			if (array.Length >= 1)
			{
				if (char.IsLower(array[0]))
				{
					array[0] = char.ToUpper(array[0]);
					index.Add(0);
				}
			}

			// Scan through the letters, checking for spaces.
			// ... Uppercase the lowercase letters following spaces.
			for (int i = 1; i < array.Length; i++)
			{
				if (array[i - 1] == ' ')
				{
					if (char.IsLower(array[i]))
					{
						array[i] = char.ToUpper(array[i]);
						index.Add(i);
					}
				}

				if (!index.Contains(i))
				{
					array[i] = char.ToLower(array[i]);
				}
			}

			return new string(array);
		}

		public static int[] SplitToIntArray(this string value, char separator)
		{
			string[] s = value.Replace(" ", "").Split(separator);
			int[] r = new int[s.Length];
			for (int i = 0; i < r.Length; i++)
			{
				r[i] = Convert.ToInt32(s[i]);
			}

			return r;
		}

		public static string FirstCharacterToLower(this string str)
		{
			if (String.IsNullOrEmpty(str) || Char.IsLower(str, 0))
			{
				return str;
			}

			return Char.ToLowerInvariant(str[0]) + str.Substring(1);
		}

		public static bool IsNullOrEmpty(this string[] value)
		{
			return value == null || value.Length == 0;
		}

		public static StatType ToStatType(this string value)
		{
			return Parse<StatType>(value);
		}

		public static EquipmentType ToEquipmentType(this string value)
		{
			return Parse<EquipmentType>(value);
		}

		public static Rarity ToRarity(this string value)
		{
			return Parse<Rarity>(value);
		}

		public static Grade ToGrade(this string value)
		{
			return Parse<Grade>(value);
		}

		public static EquipmentType[] ToEquipmentTypes(this string[] value)
		{
			EquipmentType[] ret = new EquipmentType[value.Length];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = value[i].ToEquipmentType();
			}

			return ret;
		}

		public static Rarity[] ToRarities(this string[] value)
		{
			List<Rarity> ret = new List<Rarity>();
			for (int i = 0; i < value.Length; i++)
			{
				if (value[i].ToRarity() != Rarity.Empty)
					ret.Add(value[i].ToRarity());
			}

			return ret.ToArray();
		}

		public static Grade[] ToGrades(this string[] value)
		{
			Grade[] ret = new Grade[value.Length];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = value[i].ToGrade();
			}

			return ret;
		}

		public static StatType[] ToStatTypes(this string[] value)
		{
			StatType[] ret = new StatType[value.Length];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = value[i].ToStatType();
			}

			return ret;
		}

		public static EquipmentTypeRate[] ToEquipmentTypeRates(this string[] value)
		{
			EquipmentTypeRate[] rates = new EquipmentTypeRate[value.Length];
			for (int i = 0; i < value.Length; i++)
			{
				rates[i] = value[i].ToEquipmentTypeRate();
			}

			return rates;
		}

		public static RarityRate[] ToRarityRates(this string[] value)
		{
			RarityRate[] rates = new RarityRate[value.Length];
			for (int i = 0; i < value.Length; i++)
			{
				rates[i] = value[i].ToRarityRate();
			}

			return rates;
		}

		public static GradeRate[] ToGradeRates(this string[] value)
		{
			GradeRate[] rates = new GradeRate[value.Length];
			for (int i = 0; i < rates.Length; i++)
			{
				rates[i] = value[i].ToGradeRate();
			}

			return rates;
		}
		public static EquipmentTypeRate ToEquipmentTypeRate(this string value)
		{
			string[] split = value.Split(Convert.ToChar("="));
			return new EquipmentTypeRate(split[0].ToEquipmentType(), int.Parse(split[1]));
		}

		public static RarityRate ToRarityRate(this string value)
		{
			string[] split = value.Split(Convert.ToChar("="));
			return new RarityRate(split[0].ToRarity(), int.Parse(split[1]));
		}

		public static GradeRate ToGradeRate(this string value)
		{
			string[] split = value.Split(Convert.ToChar("="));
			return new GradeRate(split[0].ToGrade(), int.Parse(split[1]));
		}

		public static ItemType ToItemType(this string value)
		{
			return Parse<ItemType>(value);
		}

		public static MaterialType ToCraftMaterialType(this string value)
		{
			return Parse<MaterialType>(value);
		}

		public static int ToInt(this string value)
		{
			return int.Parse(value);
		}

		public static ResourcesType ToResourceType(this string value)
		{
			return Parse<ResourcesType>(value);
		}

		public static bool Parse<T>(this string text, out T ret)
			where T : struct, IComparable, IFormattable, IConvertible
		{
			string key = typeof(T) + "_" + text;
			if (!cache.ContainsKey(key))
			{
				T def = default(T);
				bool success = false;
				try
				{
					def = (T) Enum.Parse(typeof(T), text);
					success = true;
				}
				catch (Exception e)
				{
					DLog.LogError(e);
					ret = default(T);
					return false;
				}

				cache.Add(key, new Ret(success, def));
			}

			Ret r = cache[key];
			ret = (T) r.value;
			return r.success;
		}

		public static T Parse<T>(this string text) where T : struct, IComparable, IFormattable, IConvertible
		{
			T ret;
			Parse(text, out ret);
			return ret;
		}

		private class Ret
		{
			public bool success;
			public object value;

			public Ret(bool success, object value)
			{
				this.success = success;
				this.value = value;
			}
		}

		public static ShopCardPackType ToPackType(this string value)
		{
			return value.Parse<ShopCardPackType>();
		}
		public static List<ShopCardPackType> ToPackTypes(this string[] value)
		{
			List<ShopCardPackType> ret = new List<ShopCardPackType>();
			for (int i = 0; i < value.Length; i++)
			{
				ret.Add(value[i].ToPackType());
			}
			return ret;
		}
		public static Dictionary<string, List<T>> MapSheetToDictionary<T>(this string content, List<string> ignores,
			IItemValidator itemValidator = null)
		{
			Dictionary<string, List<T>> cache = new Dictionary<string, List<T>>();
			if (itemValidator == null)
			{
				itemValidator = new DefaultItemValidator();
			}

			JsonData obj = JsonMapper.ToObject(content);
			foreach (var objKey in obj.Keys)
			{
				JsonData rateData = obj[objKey];
				if (!ignores.Contains(objKey))
				{
					Dictionary<string, T> d = new Dictionary<string, T>();
					foreach (string key in rateData.Keys)
					{
						T v = MapData<T>(rateData[key]);
						d.Add(key, v);
					}
					cache.Add(objKey, SortDictionary(d, itemValidator));
				}
			}

			return cache;
		}

		private static T MapData<T>(JsonData data)
		{
			T o = (T) Activator.CreateInstance(typeof(T));

			foreach (string s in data.Keys)
			{
				PropertyInfo p = o.GetType().GetProperty(s, BindingFlags.NonPublic | BindingFlags.Instance|BindingFlags.Public);
				FieldInfo f = o.GetType().GetField(s, BindingFlags.NonPublic | BindingFlags.Instance|BindingFlags.Public);
				if (p != null)
				{
					if (p.PropertyType == typeof(string))
					{
						string val = (string) data[s];
						p.SetValue(o, val, null);
					}
					else if(p.PropertyType==typeof(ProtectedInt))
					{
						int val = (int) data[s];
						p.SetValue(o,new ProtectedInt(val),null);
					}
					else
					{
						int val = (int) data[s];
						p.SetValue(o, val, null);
					}
				}
				else if(f!=null)
				{
					if (f.FieldType == typeof(string))
					{
						string val = (string) data[s];
						f.SetValue(o, val);
					}
					else if(f.FieldType==typeof(ProtectedInt))
					{
						int val = (int) data[s];
						f.SetValue(o,new ProtectedInt(val));
					}
					else
					{
						int val = (int) data[s];
						f.SetValue(o, val);
					}
				}
			}

			return o;
		}

		public static List<T> SortDictionary<T>(Dictionary<string, T> dict, IItemValidator validator = null)
		{
			List<T> ret = new List<T>();
			foreach (KeyValuePair<string, T> valuePair in dict)
			{
				if (validator==null|| validator.IsValid(valuePair.Value))
					ret.Add(valuePair.Value);
			}

			return ret;
		}

		public interface IItemValidator
		{
			bool IsValid<T>(T obj);
		}

		public class DefaultItemValidator : IItemValidator
		{
			public bool IsValid<T>(T obj)
			{
				return true;
			}
		}
		public static string IOPathToAssetDatabasePath(this string path)
		{
			return path.Replace(Application.dataPath, "Assets");
		}

		public static string AssetDatabasePathGetName(this string path)
		{
			string[] split = path.Split(Convert.ToChar("/"));
			string[] split2 = split[split.Length - 1].Split(Convert.ToChar("."));
			return split2[0];
		}

		public static string AssetDatabasePathToIOPath(this string path)
		{
			return Application.dataPath + "" + path.Replace("Assets", "");
		}

		public static SetType[] ToSetTypes(this string[] val)
		{
			SetType[] ret = new SetType[val.Length];
			for (int i = 0; i < val.Length; i++)
			{
				ret[i] = val[i].Parse<SetType>();
			}
			return ret;
		}
		public static T[] ParseToRates<T>(this string[] text,T[] cache = null) where T : IRate
		{
			if (cache == null)
			{
				List<T> ret = new List<T>();
				for (int i = 0; i < text.Length; i++)
				{
					T v = (T) Activator.CreateInstance(typeof(T), text[i]);
					if (v.IsValid())
					{
						ret.Add(v);
					}
				}
				cache = ret.ToArray();
			}
			return cache;
		}

		public static string RemoveExtension(this string original)
		{
			if (string.IsNullOrEmpty(original)|| !original.Contains("."))
			{
				return original;
			}
			string p = original.Substring(0, original.LastIndexOf(Convert.ToChar(".")));
			return p;
		}

		public static string GetTextAfterSplit(this string text,FullDescIndex index)
		{
			string[] splits = text.Split(Convert.ToChar("\n"));
			try
			{
				if (index == FullDescIndex.Desc)
				{
					string t = "";
					for (int i = (int)index; i < splits.Length; i++)
					{
						t += splits[i];
						if (i < splits.Length - 1)
						{
							t += "\n";
						}
					}
					return t;
				}
				else
				{
					
				}
				return splits[(int)index];
			}
			catch (Exception e)
			{
				return "";
			}
		}
	}
}