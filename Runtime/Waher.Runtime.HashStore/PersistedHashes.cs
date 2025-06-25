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
			return AddHash(string.Empty, Hash);
		}

		/// <summary>
		/// Persists a hash.
		/// </summary>
		/// <param name="Realm">Realm of hash value.</param>
		/// <param name="Hash">Hash digest to persist</param>
		/// <returns>If hash digest was stored (true), or if the hash already existed (false).</returns>
		public static async Task<bool> AddHash(string Realm, byte[] Hash)
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
					Hash = Hash
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
	}
}
