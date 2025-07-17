using System;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.HashStore.HashObjects;
using Waher.Runtime.Threading;

namespace Waher.Runtime.HashStore
{
	/// <summary>
	/// Static class managing persistent counters.
	/// </summary>
	public static class PersistedHashes
	{
		/// <summary>
		/// Persists a hash, using the default (empty) realm.
		/// </summary>
		/// <param name="Hash">Hash digest to persist</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static Task<bool> AddHash(byte[] Hash)
		{
			return AddHash(string.Empty, DateTime.MaxValue, Hash);
		}

		/// <summary>
		/// Persists a hash, using the default (empty) realm.
		/// </summary>
		/// <param name="ExpiresUtc">When hash expires (in UTC).</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static Task<bool> AddHash(DateTime ExpiresUtc, byte[] Hash)
		{
			return AddHash(string.Empty, ExpiresUtc, Hash);
		}

		/// <summary>
		/// Persists a hash.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static Task<bool> AddHash(string Realm, byte[] Hash)
		{
			return AddHash(Realm, DateTime.MaxValue, Hash);
		}

		/// <summary>
		/// Persists a hash.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="ExpiresUtc">When hash expires (in UTC).</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static Task<bool> AddHash(string Realm, DateTime ExpiresUtc, byte[] Hash)
		{
			return AddHash(Realm, ExpiresUtc, Hash, null);
		}

		/// <summary>
		/// Persists a hash.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <param name="AssociatedObject">Associated object value.</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static Task<bool> AddHash(string Realm, byte[] Hash, object AssociatedObject)
		{
			return AddHash(Realm, DateTime.MaxValue, Hash, AssociatedObject);
		}

		/// <summary>
		/// Persists a hash.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="ExpiresUtc">When hash expires (in UTC).</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <param name="AssociatedObject">Associated object value.</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static async Task<bool> AddHash(string Realm, DateTime ExpiresUtc, byte[] Hash,
			object AssociatedObject)
		{
			using (Semaphore Semaphore = await Semaphores.BeginWrite("hashrealm:" + Realm))
			{
				PersistedHash Obj = await Database.FindFirstDeleteRest<PersistedHash>(
					new FilterAnd(
						new FilterFieldEqualTo("Realm", Realm),
						new FilterFieldEqualTo("Hash", Hash)));

				if (!(Obj is null))
					return false;

				Obj = new PersistedHash()
				{
					Realm = Realm,
					ExpiresUtc = ExpiresUtc.ToUniversalTime(),
					Hash = Hash,
					AssociatedObject = AssociatedObject
				};

				await Database.Insert(Obj);

				return true;
			}
		}

		/// <summary>
		/// Verifies if a hash value has been persisted in the database, using the 
		/// default (empty) realm.
		/// </summary>
		/// <param name="Hash">Hash digest to verify</param>
		/// <returns>If hash digest was found.</returns>
		public static Task<bool> VerifyHash(byte[] Hash)
		{
			return VerifyHash(string.Empty, Hash);
		}

		/// <summary>
		/// Verifies if a hash value has been persisted in the database.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="Hash">Hash digest to verify</param>
		/// <returns>If hash digest was found.</returns>
		public static async Task<bool> VerifyHash(string Realm, byte[] Hash)
		{
			using (Semaphore Semaphore = await Semaphores.BeginRead("hashrealm:" + Realm))
			{
				PersistedHash Obj = await Database.FindFirstDeleteRest<PersistedHash>(
					new FilterAnd(
						new FilterFieldEqualTo("Realm", Realm),
						new FilterFieldEqualTo("Hash", Hash)));

				return !(Obj is null);
			}
		}

		/// <summary>
		/// Tries to get the associated object of a hash value that has been persisted 
		/// in the database, using the default (empty) realm.
		/// </summary>
		/// <param name="Hash">Hash digest to verify</param>
		/// <returns>Associated object, if found.</returns>
		public static Task<object> TryGetAssociatedObject(byte[] Hash)
		{
			return TryGetAssociatedObject(string.Empty, Hash);
		}

		/// <summary>
		/// Tries to get the associated object of a hash value that has been persisted 
		/// in the database.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="Hash">Hash digest to verify</param>
		/// <returns>Associated object, if found.</returns>
		public static async Task<object> TryGetAssociatedObject(string Realm, byte[] Hash)
		{
			using (Semaphore Semaphore = await Semaphores.BeginRead("hashrealm:" + Realm))
			{
				PersistedHash Obj = await Database.FindFirstDeleteRest<PersistedHash>(
					new FilterAnd(
						new FilterFieldEqualTo("Realm", Realm),
						new FilterFieldEqualTo("Hash", Hash)));

				return Obj?.AssociatedObject;
			}
		}
	}
}
