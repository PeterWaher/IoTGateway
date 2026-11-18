using System;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Session parameters exchanged during session establishment (Matter Core Specification, 4.12.8 and 4.13.1).
	/// </summary>
	public class SessionParameters
	{
		/// <summary>
		/// Default idle retransmission interval (SESSION_IDLE_INTERVAL), in milliseconds.
		/// </summary>
		public const uint DefaultIdleIntervalMs = 500;

		/// <summary>
		/// Default active retransmission interval (SESSION_ACTIVE_INTERVAL), in milliseconds.
		/// </summary>
		public const uint DefaultActiveIntervalMs = 300;

		/// <summary>
		/// Default active threshold (SESSION_ACTIVE_THRESHOLD), in milliseconds.
		/// </summary>
		public const ushort DefaultActiveThresholdMs = 4000;

		/// <summary>
		/// Data model revision supported by this implementation.
		/// </summary>
		public const ushort OwnDataModelRevision = 19;

		/// <summary>
		/// Interaction model revision supported by this implementation.
		/// </summary>
		public const ushort OwnInteractionModelRevision = 12;

		/// <summary>
		/// Specification version supported by this implementation (1.4.0.0).
		/// </summary>
		public const uint OwnSpecificationVersion = 0x01040000;

		/// <summary>
		/// Session parameters.
		/// </summary>
		public SessionParameters()
		{
		}

		/// <summary>
		/// Idle retransmission interval, in milliseconds.
		/// </summary>
		public uint IdleIntervalMs { get; set; } = DefaultIdleIntervalMs;

		/// <summary>
		/// Active retransmission interval, in milliseconds.
		/// </summary>
		public uint ActiveIntervalMs { get; set; } = DefaultActiveIntervalMs;

		/// <summary>
		/// Active threshold, in milliseconds.
		/// </summary>
		public ushort ActiveThresholdMs { get; set; } = DefaultActiveThresholdMs;

		/// <summary>
		/// Data model revision.
		/// </summary>
		public ushort? DataModelRevision { get; set; }

		/// <summary>
		/// Interaction model revision.
		/// </summary>
		public ushort? InteractionModelRevision { get; set; }

		/// <summary>
		/// Specification version.
		/// </summary>
		public uint? SpecificationVersion { get; set; }

		/// <summary>
		/// Maximum number of paths per invoke request.
		/// </summary>
		public ushort? MaxPathsPerInvoke { get; set; }

		/// <summary>
		/// Session parameters of this implementation.
		/// </summary>
		public static SessionParameters Own => new SessionParameters()
		{
			DataModelRevision = OwnDataModelRevision,
			InteractionModelRevision = OwnInteractionModelRevision,
			SpecificationVersion = OwnSpecificationVersion,
			MaxPathsPerInvoke = 1
		};

		/// <summary>
		/// Writes the parameters as a TLV structure.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag of structure.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartStructure(Tag);
			Output.WriteUInt(1, this.IdleIntervalMs);
			Output.WriteUInt(2, this.ActiveIntervalMs);
			Output.WriteUInt(3, this.ActiveThresholdMs);

			if (this.DataModelRevision.HasValue)
				Output.WriteUInt(4, this.DataModelRevision.Value);

			if (this.InteractionModelRevision.HasValue)
				Output.WriteUInt(5, this.InteractionModelRevision.Value);

			if (this.SpecificationVersion.HasValue)
				Output.WriteUInt(6, this.SpecificationVersion.Value);

			if (this.MaxPathsPerInvoke.HasValue)
				Output.WriteUInt(7, this.MaxPathsPerInvoke.Value);

			Output.EndContainer();
		}

		/// <summary>
		/// Parses session parameters from a TLV structure.
		/// </summary>
		/// <param name="Element">TLV structure (may be null).</param>
		/// <returns>Session parameters (defaults if element is null).</returns>
		public static SessionParameters Parse(TlvElement Element)
		{
			SessionParameters Result = new SessionParameters();

			if (Element is null)
				return Result;

			if (Element.TryGet(1, out TlvElement E))
				Result.IdleIntervalMs = E.AsUInt32();

			if (Element.TryGet(2, out E))
				Result.ActiveIntervalMs = E.AsUInt32();

			if (Element.TryGet(3, out E))
				Result.ActiveThresholdMs = E.AsUInt16();

			if (Element.TryGet(4, out E))
				Result.DataModelRevision = E.AsUInt16();

			if (Element.TryGet(5, out E))
				Result.InteractionModelRevision = E.AsUInt16();

			if (Element.TryGet(6, out E))
				Result.SpecificationVersion = E.AsUInt32();

			if (Element.TryGet(7, out E))
				Result.MaxPathsPerInvoke = E.AsUInt16();

			return Result;
		}

		/// <summary>
		/// Creates a copy of the parameters.
		/// </summary>
		/// <returns>Copy.</returns>
		public SessionParameters Copy()
		{
			return (SessionParameters)this.MemberwiseClone();
		}

		/// <summary>
		/// Gets the base retransmission interval to use, depending on whether the peer is active.
		/// </summary>
		/// <param name="PeerActive">If the peer is considered active.</param>
		/// <returns>Base interval.</returns>
		public TimeSpan GetBaseInterval(bool PeerActive)
		{
			return TimeSpan.FromMilliseconds(PeerActive ? this.ActiveIntervalMs : this.IdleIntervalMs);
		}
	}
}
