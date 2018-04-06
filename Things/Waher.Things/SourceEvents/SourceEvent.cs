using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Things.SourceEvents
{
	/// <summary>
	/// Delegate for data source events.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="Event">Event object.</param>
	public delegate void SourceEventEventHandler(object Sender, SourceEvent Event);

	/// <summary>
	/// Abstract base class for all data source events.
	/// </summary>
	[CollectionName("DataSourceEvents")]
	[Index("SourceId", "Timestamp")]
	public abstract class SourceEvent
    {
		private string objectId = null;
		private string sourceId = string.Empty;
		private DateTime timestamp = DateTime.MinValue;

		public SourceEvent()
		{
		}

		/// <summary>
		/// Persisted object ID. Is null if object not persisted.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Data source identity.
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("s")]
		public string SourceId
		{
			get { return this.sourceId; }
			set { this.sourceId = value; }
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		[ShortName("ts")]
		public DateTime Timestamp
		{
			get { return this.timestamp; }
			set { this.timestamp = value; }
		}
	}
}
