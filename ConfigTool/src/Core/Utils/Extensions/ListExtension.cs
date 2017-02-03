using System.Collections.Generic;

namespace Core.Utils.Extensions {
	public static class ListExtension {
		public static void Swap<T>(this List<T> list, int indexA, int indexB) {
			T tmp = list[indexA];
			list[indexA] = list[indexB];
			list[indexB] = tmp;
		}
	}
}