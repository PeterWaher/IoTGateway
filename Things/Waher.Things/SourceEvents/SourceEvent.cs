using System;
using Waher.Persistence.Attributes;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Abstract base class for all data source events.
	/// </summary>
	[CollectionName("DataSourceEvents")]
	[ArchivingTime(90)]
	[Index("SourceId", "Timestamp")]
	public abstract class SourceEvent
    {
		private string objectId = null;
		private string sourceId = string.Empty;
		private DateTime timestamp = DateTime.MinValue;

		/// <summary>
		/// Abstract base class for all data source events.
		/// </summary>
		public SourceEvent()
		{
		}

		/// <summary>
		/// Persisted object ID. Is null if object not persisted.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Data source identity.
		/// </summary>
		[ShortName("s")]
		public string SourceId
		{
			get => this.sourceId;
			set => this.sourceId = value;
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		[ShortName("ts")]
		public DateTime Timestamp
		{
			get => this.timestamp;
			set => this.timestamp = value;
		}

		/// <summary>
		/// Type of data source event.
		/// </summary>
		public abstract SourceEventType EventType
		{
			get;
		}
	}
}
