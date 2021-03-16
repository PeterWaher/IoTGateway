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
			return GetCaches(true);
		}

		/// <summary>
		/// Gets active caches.
		/// </summary>
		/// <param name="ExcludeStandalone">If standalone caches are to be excluded.</param>
		/// <returns>Array of cache objects.</returns>
		public static ICache[] GetCaches(bool ExcludeStandalone)
		{
			List<ICache> Result = new List<ICache>();

			lock (caches)
			{
				foreach (ICache Cache in caches.Values)
				{
					if (ExcludeStandalone && Cache.Standalone)
						continue;

					Result.Add(Cache);
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Clears all active caches.
		/// </summary>
		public static void ClearAll()
		{
			ClearAll(true);
		}

		/// <summary>
		/// Clears all active caches.
		/// </summary>
		/// <param name="ExcludeStandalone">If standalone caches are to be excluded.</param>
		public static void ClearAll(bool ExcludeStandalone)
		{
			foreach (ICache Cache in GetCaches(ExcludeStandalone))
				Cache.Clear();
		}
	}
}
