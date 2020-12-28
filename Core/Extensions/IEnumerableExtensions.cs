using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Core.Extensions {

	public static class IEnumerableExtensions {

		///<summary>Returns true if the IEnumerable is null or the count is equal to 0.</summary>
		///<param name="enumerable">The enumerable.</param>
		public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> enumerable) {
			return enumerable == null || enumerable.Count() == 0;
		}

		///<summary>Returns an observable collection.</summary>
		public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> enumerable) {
			return new ObservableCollection<TSource>(enumerable);
		}

	}

}
