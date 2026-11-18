using System.Text;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.InteractionModel
{
	/// <summary>
	/// Attribute path (AttributePathIB), possibly containing wildcards (null values).
	/// </summary>
	public class AttributePath
	{
		/// <summary>
		/// Attribute path.
		/// </summary>
		public AttributePath()
		{
		}

		/// <summary>
		/// Attribute path.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID (null = wildcard).</param>
		/// <param name="ClusterId">Cluster ID (null = wildcard).</param>
		/// <param name="AttributeId">Attribute ID (null = wildcard).</param>
		public AttributePath(ushort? EndpointId, uint? ClusterId, uint? AttributeId)
		{
			this.EndpointId = EndpointId;
			this.ClusterId = ClusterId;
			this.AttributeId = AttributeId;
		}

		/// <summary>
		/// Node ID, if specified.
		/// </summary>
		public ulong? NodeId { get; set; }

		/// <summary>
		/// Endpoint ID (null = wildcard).
		/// </summary>
		public ushort? EndpointId { get; set; }

		/// <summary>
		/// Cluster ID (null = wildcard).
		/// </summary>
		public uint? ClusterId { get; set; }

		/// <summary>
		/// Attribute ID (null = wildcard).
		/// </summary>
		public uint? AttributeId { get; set; }

		/// <summary>
		/// List index, if present and not null.
		/// </summary>
		public ushort? ListIndex { get; set; }

		/// <summary>
		/// If the list index is present and null, meaning the data should be appended to a list.
		/// </summary>
		public bool ListAppend { get; set; }

		/// <summary>
		/// Writes the path as an AttributePathIB list.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartList(Tag);

			if (this.NodeId.HasValue)
				Output.WriteUInt(1, this.NodeId.Value);

			if (this.EndpointId.HasValue)
				Output.WriteUInt(2, this.EndpointId.Value);

			if (this.ClusterId.HasValue)
				Output.WriteUInt(3, this.ClusterId.Value);

			if (this.AttributeId.HasValue)
				Output.WriteUInt(4, this.AttributeId.Value);

			if (this.ListAppend)
				Output.WriteNull(5);
			else if (this.ListIndex.HasValue)
				Output.WriteUInt(5, this.ListIndex.Value);

			Output.EndContainer();
		}

		/// <summary>
		/// Parses an AttributePathIB.
		/// </summary>
		/// <param name="Element">TLV list.</param>
		/// <param name="Previous">Previous path, used for tag compression.</param>
		/// <returns>Path.</returns>
		public static AttributePath Parse(TlvElement Element, AttributePath Previous)
		{
			AttributePath Result = new AttributePath();
			bool Compressed = Element[0]?.AsBoolean() ?? false;

			if (Compressed && !(Previous is null))
			{
				Result.NodeId = Previous.NodeId;
				Result.EndpointId = Previous.EndpointId;
				Result.ClusterId = Previous.ClusterId;
				Result.AttributeId = Previous.AttributeId;
			}

			if (Element.TryGet(1, out TlvElement E))
				Result.NodeId = E.AsUInt64();

			if (Element.TryGet(2, out E))
				Result.EndpointId = E.AsUInt16();

			if (Element.TryGet(3, out E))
				Result.ClusterId = E.AsUInt32();

			if (Element.TryGet(4, out E))
				Result.AttributeId = E.AsUInt32();

			if (Element.TryGet(5, out E))
			{
				if (E.IsNull)
					Result.ListAppend = true;
				else
					Result.ListIndex = E.AsUInt16();
			}

			return Result;
		}

		/// <summary>
		/// Key identifying a concrete attribute (endpoint, cluster, attribute).
		/// </summary>
		public string Key => (this.EndpointId?.ToString() ?? "*") + "/" + (this.ClusterId?.ToString("X8") ?? "*") + "/" +
			(this.AttributeId?.ToString("X8") ?? "*");

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.EndpointId?.ToString() ?? "*");
			sb.Append('/');
			sb.Append(this.ClusterId.HasValue ? "0x" + this.ClusterId.Value.ToString("X4") : "*");
			sb.Append('/');
			sb.Append(this.AttributeId.HasValue ? "0x" + this.AttributeId.Value.ToString("X4") : "*");

			if (this.ListAppend)
				sb.Append("[+]");
			else if (this.ListIndex.HasValue)
			{
				sb.Append('[');
				sb.Append(this.ListIndex.Value.ToString());
				sb.Append(']');
			}

			return sb.ToString();
		}
	}

	/// <summary>
	/// Event path (EventPathIB), possibly containing wildcards (null values).
	/// </summary>
	public class EventPath
	{
		/// <summary>
		/// Event path.
		/// </summary>
		public EventPath()
		{
		}

		/// <summary>
		/// Event path.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID (null = wildcard).</param>
		/// <param name="ClusterId">Cluster ID (null = wildcard).</param>
		/// <param name="EventId">Event ID (null = wildcard).</param>
		/// <param name="IsUrgent">If events should be reported urgently.</param>
		public EventPath(ushort? EndpointId, uint? ClusterId, uint? EventId, bool IsUrgent = false)
		{
			this.EndpointId = EndpointId;
			this.ClusterId = ClusterId;
			this.EventId = EventId;
			this.IsUrgent = IsUrgent;
		}

		/// <summary>
		/// Node ID, if specified.
		/// </summary>
		public ulong? NodeId { get; set; }

		/// <summary>
		/// Endpoint ID (null = wildcard).
		/// </summary>
		public ushort? EndpointId { get; set; }

		/// <summary>
		/// Cluster ID (null = wildcard).
		/// </summary>
		public uint? ClusterId { get; set; }

		/// <summary>
		/// Event ID (null = wildcard).
		/// </summary>
		public uint? EventId { get; set; }

		/// <summary>
		/// If events should be reported urgently.
		/// </summary>
		public bool IsUrgent { get; set; }

		/// <summary>
		/// Writes the path as an EventPathIB list.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartList(Tag);

			if (this.NodeId.HasValue)
				Output.WriteUInt(0, this.NodeId.Value);

			if (this.EndpointId.HasValue)
				Output.WriteUInt(1, this.EndpointId.Value);

			if (this.ClusterId.HasValue)
				Output.WriteUInt(2, this.ClusterId.Value);

			if (this.EventId.HasValue)
				Output.WriteUInt(3, this.EventId.Value);

			if (this.IsUrgent)
				Output.WriteBoolean(4, true);

			Output.EndContainer();
		}

		/// <summary>
		/// Parses an EventPathIB.
		/// </summary>
		/// <param name="Element">TLV list.</param>
		/// <returns>Path.</returns>
		public static EventPath Parse(TlvElement Element)
		{
			EventPath Result = new EventPath();

			if (Element.TryGet(0, out TlvElement E))
				Result.NodeId = E.AsUInt64();

			if (Element.TryGet(1, out E))
				Result.EndpointId = E.AsUInt16();

			if (Element.TryGet(2, out E))
				Result.ClusterId = E.AsUInt32();

			if (Element.TryGet(3, out E))
				Result.EventId = E.AsUInt32();

			if (Element.TryGet(4, out E))
				Result.IsUrgent = E.AsBoolean();

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return (this.EndpointId?.ToString() ?? "*") + "/" +
				(this.ClusterId.HasValue ? "0x" + this.ClusterId.Value.ToString("X4") : "*") + "/E" +
				(this.EventId.HasValue ? "0x" + this.EventId.Value.ToString("X2") : "*");
		}
	}

	/// <summary>
	/// Command path (CommandPathIB).
	/// </summary>
	public class CommandPath
	{
		/// <summary>
		/// Command path.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="CommandId">Command ID.</param>
		public CommandPath(ushort EndpointId, uint ClusterId, uint CommandId)
		{
			this.EndpointId = EndpointId;
			this.ClusterId = ClusterId;
			this.CommandId = CommandId;
		}

		/// <summary>
		/// Endpoint ID.
		/// </summary>
		public ushort EndpointId { get; }

		/// <summary>
		/// Cluster ID.
		/// </summary>
		public uint ClusterId { get; }

		/// <summary>
		/// Command ID.
		/// </summary>
		public uint CommandId { get; }

		/// <summary>
		/// Writes the path as a CommandPathIB list.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartList(Tag);
			Output.WriteUInt(0, this.EndpointId);
			Output.WriteUInt(1, this.ClusterId);
			Output.WriteUInt(2, this.CommandId);
			Output.EndContainer();
		}

		/// <summary>
		/// Parses a CommandPathIB.
		/// </summary>
		/// <param name="Element">TLV list.</param>
		/// <returns>Path.</returns>
		public static CommandPath Parse(TlvElement Element)
		{
			return new CommandPath(
				Element[0]?.AsUInt16() ?? 0,
				Element.Required(1).AsUInt32(),
				Element.Required(2).AsUInt32());
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.EndpointId.ToString() + "/0x" + this.ClusterId.ToString("X4") + "/C0x" + this.CommandId.ToString("X2");
		}
	}

	/// <summary>
	/// Data version filter (DataVersionFilterIB).
	/// </summary>
	public class DataVersionFilter
	{
		/// <summary>
		/// Data version filter.
		/// </summary>
		/// <param name="EndpointId">Endpoint ID.</param>
		/// <param name="ClusterId">Cluster ID.</param>
		/// <param name="DataVersion">Known data version.</param>
		public DataVersionFilter(ushort EndpointId, uint ClusterId, uint DataVersion)
		{
			this.EndpointId = EndpointId;
			this.ClusterId = ClusterId;
			this.DataVersion = DataVersion;
		}

		/// <summary>
		/// Endpoint ID.
		/// </summary>
		public ushort EndpointId { get; }

		/// <summary>
		/// Cluster ID.
		/// </summary>
		public uint ClusterId { get; }

		/// <summary>
		/// Known data version.
		/// </summary>
		public uint DataVersion { get; }

		/// <summary>
		/// Writes the filter as a DataVersionFilterIB structure.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartStructure(Tag);
			Output.StartList(0);
			Output.WriteUInt(1, this.EndpointId);
			Output.WriteUInt(2, this.ClusterId);
			Output.EndContainer();
			Output.WriteUInt(1, this.DataVersion);
			Output.EndContainer();
		}
	}
}
