using System;
using System.Threading.Tasks;
using Waher.Runtime.Cache;

namespace Waher.Runtime.Threading
{
	/// <summary>
	/// Static class of application-wide semaphores that can be used to order access
	/// to editable objects.
	/// 
	/// Semaphores are created dynamically, and identified by a string Key, that must
	/// be unique for each semaphore. A semaphore that has not been used for an hour
	/// is automatically disposed (but can be recreated later).
	/// </summary>
	public static class Semaphores
	{
		private static readonly Cache<string, MultiReadSingleWriteObject> semaphores;

		static Semaphores()
		{
			semaphores = new Cache<string, MultiReadSingleWriteObject>(int.MaxValue, TimeSpan.MaxValue, TimeSpan.FromHours(1), true);
			semaphores.Removed += Semaphores_Removed;
		}

		private static void Semaphores_Removed(object Sender, CacheItemEventArgs<string, MultiReadSingleWriteObject> e)
		{
			e.Value.Dispose();
		}

		private static MultiReadSingleWriteObject GetSemaphore(string Key)
		{
			lock (semaphores)
			{
				if (semaphores.TryGetValue(Key, out MultiReadSingleWriteObject Result))
					return Result;

				Result = new MultiReadSingleWriteObject();
				semaphores[Key] = Result;

				return Result;
			}
		}

		/// <summary>
		/// Waits until the semaphore identified by <paramref name="Key"/> is ready for reading.
		/// Each call to <see cref="BeginRead"/> must be followed by exactly one call to <see cref="EndRead"/>
		/// with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Key">Semaphore key.</param>
		/// <returns>Semaphore object that can be used for managing the semaphore.</returns>
		public static async Task<Semaphore> BeginRead(string Key)
		{
			await GetSemaphore(Key).BeginRead();
			return new Semaphore(Key, 1);
		}

		/// <summary>
		/// Ends a reading session of the semaphore identified by <paramref name="Key"/>.
		/// Must be called once for each call to <see cref="BeginRead"/> or successful call to <see cref="TryBeginRead(int)"/>
		/// with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Key">Semaphore key.</param>
		public static Task EndRead(string Key)
		{
			return GetSemaphore(Key).EndRead();
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until 
		/// the semaphore identified by <paramref name="Key"/> is ready for reading.
		/// Each successful call to <see cref="TryBeginRead"/> must be followed by 
		/// exactly one call to <see cref="EndRead"/> with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Key">Semaphore key.</param>
		public static Task<bool> TryBeginRead(string Key, int Timeout)
		{
			return GetSemaphore(Key).TryBeginRead(Timeout);
		}

		/// <summary>
		/// Waits until the semaphore identified by <paramref name="Key"/> is ready for writing.
		/// Each call to <see cref="BeginWrite"/> must be followed by exactly one call to <see cref="EndWrite"/>
		/// with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Key">Semaphore key.</param>
		/// <returns>Semaphore object that can be used for managing the semaphore.</returns>
		public static async Task<Semaphore> BeginWrite(string Key)
		{
			await GetSemaphore(Key).BeginWrite();
			return new Semaphore(Key, true);
		}

		/// <summary>
		/// Ends a writing session of the semaphore identified by <paramref name="Key"/>.
		/// Must be called once for each call to <see cref="BeginWrite"/> or successful call to <see cref="TryBeginWrite(int)"/>
		/// with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Key">Semaphore key.</param>
		public static Task EndWrite(string Key)
		{
			return GetSemaphore(Key).EndWrite();
		}

		/// <summary>
		/// Waits, at most <paramref name="Timeout"/> milliseconds, until 
		/// the semaphore identified by <paramref name="Key"/> is ready for writing.
		/// Each successful call to <see cref="TryBeginWrite"/> must be followed by 
		/// exactly one call to <see cref="EndWrite"/> with the same <paramref name="Key"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <param name="Key">Semaphore key.</param>
		public static Task<bool> TryBeginWrite(string Key, int Timeout)
		{
			return GetSemaphore(Key).TryBeginWrite(Timeout);
		}
	}
}
