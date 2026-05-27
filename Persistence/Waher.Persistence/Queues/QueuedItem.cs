using System;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.Queues
{
	/// <summary>
	/// Represents one item in a queue.
	/// </summary>
	[CollectionName(QueuedItemCollectionName)]
	[TypeName(TypeNameSerialization.None)]
	[Index("QueueName", "CreatedUtc")]
	public class QueuedItem
	{
		/// <summary>
		/// Collection name of queued items: QueuedItems
		/// </summary>
		public const string QueuedItemCollectionName = "QueuedItems";

		/// <summary>
		/// Represents one item in a queue.
		/// </summary>
		public QueuedItem()
		{
		}

		/// <summary>
		/// Object ID
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Name of queue.
		/// </summary>
		public string QueueName { get; set; }

		/// <summary>
		/// Timestamp of creation, in UTC.
		/// </summary>
		public DateTime CreatedUtc { get; set; }

		/// <summary>
		/// Object content of item.
		/// </summary>
		public object Content { get; set; }
	}
}
