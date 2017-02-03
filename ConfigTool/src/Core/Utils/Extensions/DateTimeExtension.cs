using System;

namespace Core.Utils.Extensions {
	public static class DateTimeExtension {
		public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek) {
			int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
			return dt.AddDays(-1 * diff).Date;
		}
	}
}