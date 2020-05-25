using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Cache
{
	/// <summary>
	/// Repository of all active caches.
	/// </summary>
	public static class Caches
	{
		private static readonly Dictionary<Guid, ICache> caches = new Dictionary<Guid, ICache>();

		internal static void Register(Guid Guid, ICache Cache)
		{
			lock (caches)
			{
				caches[Guid] = Cache;
			}
		}

		internal static bool Unregister(Guid Guid)
		{
			lock (caches)
			{
				return caches.Remove(Guid);
			}
		}

		/// <summary>
		/// Gets active caches.
		/// </summary>
		/// <returns>Array of cache objects.</returns>
		public static ICache[] GetCaches()
		{
			ICache[] Result;

			lock (caches)
			{
				Result = new ICache[caches.Count];
				caches.Values.CopyTo(Result, 0);
			}

			return Result;
		}

		/// <summary>
		/// Clears all active caches.
		/// </summary>
		public static void ClearAll()
		{
			foreach (ICache Cache in GetCaches())
				Cache.Clear();
		}
	}
}
