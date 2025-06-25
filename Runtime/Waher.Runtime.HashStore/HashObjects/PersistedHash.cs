using Waher.Persistence.Attributes;

namespace Waher.Runtime.HashStore.HashObjects
{
	/// <summary>
	/// Contains information about a persisted hash
	/// </summary>
	[CollectionName("PersistedHashes")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Realm", "Hash")]
	[ArchivingTime()]
	public class PersistedHash
	{
		/// <summary>
		/// Contains information about a persisted hash
		/// </summary>
		public PersistedHash()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Realm.
		/// </summary>
		public string Realm { get; set; }

		/// <summary>
		/// Hash
		/// </summary>
		public byte[] Hash { get; set; }
	}
}
