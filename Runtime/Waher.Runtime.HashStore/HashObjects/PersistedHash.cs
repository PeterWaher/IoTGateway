using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.HashStore.HashObjects
{
	/// <summary>
	/// Contains information about a persisted hash
	/// </summary>
	[CollectionName("PersistedHashes")]
	[TypeName(TypeNameSerialization.None)]
	[Index("Realm", "Hash")]
	[Index("ExpiresUtc")]
	[ArchivingTime(nameof(ArchiveDays))]
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
		/// When hash expires and can be removed.
		/// </summary>
		public DateTime ExpiresUtc { get; set; }

		/// <summary>
		/// Realm.
		/// </summary>
		public string Realm { get; set; }

		/// <summary>
		/// Hash
		/// </summary>
		public byte[] Hash { get; set; }

		/// <summary>
		/// Associated object
		/// </summary>
		[DefaultValueNull]
		public object AssociatedObject { get; set; }

		/// <summary>
		/// Number of days to archive hash.
		/// </summary>
		public int ArchiveDays
		{
			get
			{
				if (this.ExpiresUtc.Year >= 9999)
					return int.MaxValue;

				TimeSpan Span = this.ExpiresUtc - DateTime.UtcNow;
				double Days = Math.Ceiling(Span.TotalDays);

				if (Days < 0)
					return 0;
				else if (Days > int.MaxValue)
					return int.MaxValue;
				else
					return (int)Days;
			}
		}
	}
}
