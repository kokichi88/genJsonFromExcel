using System;
using System.Globalization;
using CodeStage.AntiCheat.ObscuredTypes;
using Equipment;
using UnityEngine;

namespace Core.Utils.Extensions
{
	public static class IntegerExtension
	{
		public static Grade ToGrade(this int value)
		{
			return (Grade) (value);
		}

		public static string ToNumberQuantityFormat(this int number)
		{
			return (number < 1000) ? number.ToString() : string.Format("{0:#,#}", number);
		}
		public static string ToNumberQuantityFormat(this ObscuredInt number)
		{
			int v = number;
			return v.ToNumberQuantityFormat();
		}
		public static int ToInt(this float value)
		{
			return (int) (value);
		}

		public static float DecimalPart(this float number)
		{
			string input_decimal_number = number.ToString();
			string[] split = input_decimal_number.Split(Convert.ToChar("."));
			return split.Length > 1 ? float.Parse("0." + split[1]) : 0;
		}

		public static string ToPercent(this float number)
		{
			float delta = number * 100 - (int) (number * 100);
			string p = delta >= 0.1f &&(number*100).DecimalPart()>0? "P1" : "P0";
			return number.ToString(p, CultureInfo.InvariantCulture);
		}

		public static string ToPercent(this double number)
		{
			return ((float) number).ToPercent();
		}

		public static EquipmentCollectId ToEquipmentCollectId(this int number)
		{
			return new EquipmentCollectId(number);
		}

		public static int[] ToInts(this ObscuredInt[] value)
		{
			int[] ret = new int[value.Length];
			for (int i = 0; i < ret.Length; i++)
			{
				ret[i] = value[i];
			}
			return ret;
		}

		public static Tuple<int, int> MinMax(this int[] value)
		{
			int min = value[0];
			int max = value.Length == 0 ? min : value[value.Length - 1];
			return new Tuple<int, int>(min,max);
		}

		public static int CountDigit(this int n) {
			if (n >= 0) {
				if (n < 10) return 1;
				if (n < 100) return 2;
				if (n < 1000) return 3;
				if (n < 10000) return 4;
				if (n < 100000) return 5;
				if (n < 1000000) return 6;
				if (n < 10000000) return 7;
				if (n < 100000000) return 8;
				if (n < 1000000000) return 9;
				return 10;
			}
			else {
				if (n > -10) return 2;
				if (n > -100) return 3;
				if (n > -1000) return 4;
				if (n > -10000) return 5;
				if (n > -100000) return 6;
				if (n > -1000000) return 7;
				if (n > -10000000) return 8;
				if (n > -100000000) return 9;
				if (n > -1000000000) return 10;
				return 11;
			}
		}
	}
}