using System;
using Waher.Persistence.Attributes;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Groups
{
	/// <summary>
	/// Defines a message logged on a group node.
	/// </summary>
	[CollectionName("GroupMessages")]
	[TypeName(TypeNameSerialization.None)]
	[ArchivingTime(731)]	// 2 years
	[Index("NodeId", "Created")]
	public class GroupMessage
	{
		private Guid objectId = Guid.Empty;
		private Guid nodeId = Guid.Empty;
		private MessageType type = MessageType.Information;
		private string eventId = string.Empty;
		private string body = string.Empty;
		private DateTime created = DateTime.Now;
		private DateTime updated = DateTime.MinValue;
		private int count = 1;

		/// <summary>
		/// Defines a message logged on a group node.
		/// </summary>
		public GroupMessage()
		{
		}

		/// <summary>
		/// Defines a message logged on a geoup node.
		/// </summary>
		/// <param name="NodeId">Object ID of group node on which message has been logged.</param>
		/// <param name="Timestamp">Message Timestamp.</param>
		/// <param name="Type">Type of message.</param>
		/// <param name="EventId">Optional Event ID.</param>
		/// <param name="Body">Message body.</param>
		public GroupMessage(Guid NodeId, DateTime Timestamp, MessageType Type, string EventId, string Body)
		{
			this.nodeId = NodeId;
			this.created = Timestamp;
			this.updated = Timestamp;
			this.type = Type;
			this.eventId = EventId;
			this.body = Body;
			this.count = 1;
		}

		/// <summary>
		/// Object ID in persistence layer.
		/// </summary>
		[ObjectId]
		public Guid ObjectId
		{
			get => this.objectId;
			set => this.objectId = value;
		}

		/// <summary>
		/// Object ID of group node on which message has been logged.
		/// </summary>
		public Guid NodeId
		{
			get => this.nodeId;
			set => this.nodeId = value;
		}

		/// <summary>
		/// When node was created.
		/// </summary>
		public DateTime Created
		{
			get => this.created;
			set => this.created = value;
		}

		/// <summary>
		/// When node was last updated. If it has not been updated, value will be <see cref="DateTime.MinValue"/>.
		/// </summary>
		[DefaultValueDateTimeMinValue]
		public DateTime Updated
		{
			get => this.updated;
			set => this.updated = value;
		}

		/// <summary>
		/// Number of times the message has been reported (updated).
		/// </summary>
		public int Count
		{
			get => this.count;
			set => this.count = value;
		}

		/// <summary>
		/// Message Type
		/// </summary>
		[DefaultValue(MessageType.Information)]
		public MessageType Type
		{
			get => this.type;
			set => this.type = value;
		}

		/// <summary>
		/// Optional Event ID.
		/// </summary>
		[DefaultValueStringEmpty]
		public string EventId
		{
			get => this.eventId;
			set => this.eventId = value;
		}

		/// <summary>
		/// Message body.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Body
		{
			get => this.body;
			set => this.body = value;
		}
	}
}
