using System;
using System.Collections.Generic;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.InteractionModel
{
	/// <summary>
	/// Reported attribute value or status.
	/// </summary>
	public class AttributeReport
	{
		/// <summary>
		/// Reported attribute value or status.
		/// </summary>
		/// <param name="Path">Attribute path.</param>
		/// <param name="DataVersion">Data version of the cluster instance, if reported.</param>
		/// <param name="Data">Attribute value, or null if status is reported.</param>
		/// <param name="Status">Status, or null if value is reported.</param>
		public AttributeReport(AttributePath Path, uint? DataVersion, TlvElement Data, ImStatus Status)
		{
			this.Path = Path;
			this.DataVersion = DataVersion;
			this.Data = Data;
			this.Status = Status;
		}

		/// <summary>
		/// Attribute path.
		/// </summary>
		public AttributePath Path { get; }

		/// <summary>
		/// Data version, if reported.
		/// </summary>
		public uint? DataVersion { get; }

		/// <summary>
		/// Attribute value (null if a status was reported).
		/// </summary>
		public TlvElement Data { get; internal set; }

		/// <summary>
		/// Status, if an error was reported for the attribute.
		/// </summary>
		public ImStatus Status { get; }

		/// <summary>
		/// If a value was reported.
		/// </summary>
		public bool HasValue => !(this.Data is null);

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.HasValue)
				return this.Path.ToString() + " = " + this.Data.ToString();
			else
				return this.Path.ToString() + ": " + this.Status?.ToString();
		}
	}

	/// <summary>
	/// Event priority levels.
	/// </summary>
	public enum EventPriority : byte
	{
		/// <summary>
		/// Debug.
		/// </summary>
		Debug = 0,

		/// <summary>
		/// Informational.
		/// </summary>
		Info = 1,

		/// <summary>
		/// Critical.
		/// </summary>
		Critical = 2
	}

	/// <summary>
	/// Reported event.
	/// </summary>
	public class EventReport
	{
		/// <summary>
		/// Reported event.
		/// </summary>
		/// <param name="Path">Event path.</param>
		/// <param name="EventNumber">Event number.</param>
		/// <param name="Priority">Priority.</param>
		/// <param name="Timestamp">Timestamp (UTC), if an epoch timestamp was reported.</param>
		/// <param name="SystemTimestampMs">System timestamp (ms since boot), if reported.</param>
		/// <param name="Data">Event data.</param>
		/// <param name="Status">Status, if an error was reported.</param>
		public EventReport(EventPath Path, ulong EventNumber, EventPriority Priority, DateTime? Timestamp, ulong? SystemTimestampMs,
			TlvElement Data, ImStatus Status)
		{
			this.Path = Path;
			this.EventNumber = EventNumber;
			this.Priority = Priority;
			this.Timestamp = Timestamp;
			this.SystemTimestampMs = SystemTimestampMs;
			this.Data = Data;
			this.Status = Status;
		}

		/// <summary>
		/// Event path.
		/// </summary>
		public EventPath Path { get; }

		/// <summary>
		/// Event number.
		/// </summary>
		public ulong EventNumber { get; }

		/// <summary>
		/// Priority.
		/// </summary>
		public EventPriority Priority { get; }

		/// <summary>
		/// Timestamp (UTC), if an epoch timestamp was reported.
		/// </summary>
		public DateTime? Timestamp { get; }

		/// <summary>
		/// System timestamp (ms since boot), if reported.
		/// </summary>
		public ulong? SystemTimestampMs { get; }

		/// <summary>
		/// Event data.
		/// </summary>
		public TlvElement Data { get; }

		/// <summary>
		/// Status, if an error was reported.
		/// </summary>
		public ImStatus Status { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Path.ToString() + " #" + this.EventNumber.ToString() + ": " + (this.Data?.ToString() ?? this.Status?.ToString());
		}
	}

	/// <summary>
	/// Result of a read or subscription report.
	/// </summary>
	public class ReadResult
	{
		private readonly List<AttributeReport> attributes = new List<AttributeReport>();
		private readonly List<EventReport> events = new List<EventReport>();
		private readonly Dictionary<string, AttributeReport> byKey = new Dictionary<string, AttributeReport>();

		/// <summary>
		/// Result of a read or subscription report.
		/// </summary>
		public ReadResult()
		{
		}

		/// <summary>
		/// Reported attributes.
		/// </summary>
		public IReadOnlyList<AttributeReport> Attributes => this.attributes;

		/// <summary>
		/// Reported events.
		/// </summary>
		public IReadOnlyList<EventReport> Events => this.events;

		/// <summary>
		/// Subscription ID, if the report was part of a subscription.
		/// </summary>
		public uint? SubscriptionId { get; internal set; }

		/// <summary>
		/// Adds an attribute report, merging chunked list items.
		/// </summary>
		/// <param name="Report">Report.</param>
		internal void Add(AttributeReport Report)
		{
			string Key = Report.Path.Key;

			if (Report.HasValue && Report.Path.ListAppend)
			{
				if (this.byKey.TryGetValue(Key, out AttributeReport Existing) && !(Existing.Data is null) && Existing.Data.IsContainer)
				{
					TlvElement Item = Report.Data.Clone();
					Item.Tag = TlvTag.Anonymous;
					Existing.Data.Add(Item);
					return;
				}

				TlvElement List = TlvElement.Array(Report.Data.Tag);
				TlvElement Item2 = Report.Data.Clone();
				Item2.Tag = TlvTag.Anonymous;
				List.Add(Item2);

				AttributePath Path = new AttributePath(Report.Path.EndpointId, Report.Path.ClusterId, Report.Path.AttributeId)
				{
					NodeId = Report.Path.NodeId
				};

				Report = new AttributeReport(Path, Report.DataVersion, List, null);
			}

			if (this.byKey.TryGetValue(Key, out AttributeReport Prev))
				this.attributes.Remove(Prev);

			this.byKey[Key] = Report;
			this.attributes.Add(Report);
		}

		/// <summary>
		/// Adds an event report.
		/// </summary>
		/// <param name="Report">Report.</param>
		internal void Add(EventReport Report)
		{
			this.events.Add(Report);
		}

		/// <summary>
		/// Gets the reported value of a given attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Value, or null if not reported.</returns>
		public TlvElement GetValue(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			if (this.byKey.TryGetValue(new AttributePath(EndpointId, ClusterId, AttributeId).Key, out AttributeReport Report))
				return Report.Data;
			else
				return null;
		}

		/// <summary>
		/// Gets the report of a given attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <returns>Report, or null if not reported.</returns>
		public AttributeReport GetReport(ushort EndpointId, uint ClusterId, uint AttributeId)
		{
			if (this.byKey.TryGetValue(new AttributePath(EndpointId, ClusterId, AttributeId).Key, out AttributeReport Report))
				return Report;
			else
				return null;
		}
	}

	/// <summary>
	/// Result of a command invocation.
	/// </summary>
	public class InvokeResult
	{
		/// <summary>
		/// Result of a command invocation.
		/// </summary>
		/// <param name="Path">Path of response command, or of the invoked command if a status was returned.</param>
		/// <param name="Fields">Response command fields, if a response command was returned.</param>
		/// <param name="Status">Status, if a status was returned.</param>
		/// <param name="CommandRef">Command reference, if any.</param>
		public InvokeResult(CommandPath Path, TlvElement Fields, ImStatus Status, ushort? CommandRef)
		{
			this.Path = Path;
			this.Fields = Fields;
			this.Status = Status;
			this.CommandRef = CommandRef;
		}

		/// <summary>
		/// Path of response command, or of the invoked command if a status was returned.
		/// </summary>
		public CommandPath Path { get; }

		/// <summary>
		/// Response command fields, if a response command was returned.
		/// </summary>
		public TlvElement Fields { get; }

		/// <summary>
		/// Status, if a status was returned instead of a response command.
		/// </summary>
		public ImStatus Status { get; }

		/// <summary>
		/// Command reference.
		/// </summary>
		public ushort? CommandRef { get; }

		/// <summary>
		/// If the invocation was successful (either a response command, or a success status).
		/// </summary>
		public bool IsSuccess => this.Status is null || this.Status.IsSuccess;

		/// <summary>
		/// If a response command was returned.
		/// </summary>
		public bool HasResponse => this.Status is null;

		/// <summary>
		/// Throws an exception if the invocation was not successful.
		/// </summary>
		/// <returns>The current object.</returns>
		public InvokeResult AssertSuccess()
		{
			if (!this.IsSuccess)
				throw new ImStatusException(this.Status, "Command " + this.Path.ToString() + " failed");

			return this;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.HasResponse)
				return this.Path.ToString() + ": " + this.Fields?.ToString();
			else
				return this.Path.ToString() + ": " + this.Status.ToString();
		}
	}

	/// <summary>
	/// Data to write to an attribute.
	/// </summary>
	public class AttributeWrite
	{
		/// <summary>
		/// Data to write to an attribute.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="AttributeId">Attribute ID.</param>
		/// <param name="Value">Value (TLV element, or .NET value).</param>
		/// <param name="DataVersion">Optional data version, for conditional writes.</param>
		public AttributeWrite(ushort EndpointId, uint ClusterId, uint AttributeId, object Value, uint? DataVersion = null)
		{
			this.Path = new AttributePath(EndpointId, ClusterId, AttributeId);
			this.Value = Value;
			this.DataVersion = DataVersion;
		}

		/// <summary>
		/// Attribute path.
		/// </summary>
		public AttributePath Path { get; }

		/// <summary>
		/// Value (TLV element, or .NET value).
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Optional data version, for conditional writes.
		/// </summary>
		public uint? DataVersion { get; }
	}

	/// <summary>
	/// Result of an attribute write.
	/// </summary>
	public class WriteResult
	{
		/// <summary>
		/// Result of an attribute write.
		/// </summary>
		/// <param name="Path">Attribute path.</param>
		/// <param name="Status">Status.</param>
		public WriteResult(AttributePath Path, ImStatus Status)
		{
			this.Path = Path;
			this.Status = Status;
		}

		/// <summary>
		/// Attribute path.
		/// </summary>
		public AttributePath Path { get; }

		/// <summary>
		/// Status.
		/// </summary>
		public ImStatus Status { get; }

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Path.ToString() + ": " + this.Status.ToString();
		}
	}
}
