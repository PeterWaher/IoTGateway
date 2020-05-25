using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Runtime.Cache
{
	/// <summary>
	/// Interface for caches.
	/// </summary>
	public interface ICache : IDisposable
	{
		/// <summary>
		/// Clears the cache.
		/// </summary>
		void Clear();
	}
}
