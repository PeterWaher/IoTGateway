using System;

namespace Waher.Runtime.Cache
{
	/// <summary>
	/// Interface for caches.
	/// </summary>
	public interface ICache : IDisposable
	{
		/// <summary>
		/// If cache is a standalone cache, or if it can be managed collectively
		/// with other caches.
		/// </summary>
		bool Standalone
		{ 
			get; 
		}

		/// <summary>
		/// Clears the cache.
		/// </summary>
		void Clear();
	}
}
