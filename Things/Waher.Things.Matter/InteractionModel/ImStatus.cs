using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.InteractionModel
{
	/// <summary>
	/// Interaction Model status codes (Matter Core Specification, 8.10).
	/// </summary>
	public enum ImStatusCode : byte
	{
		/// <summary>
		/// Operation was successful.
		/// </summary>
		Success = 0x00,

		/// <summary>
		/// Operation was not successful.
		/// </summary>
		Failure = 0x01,

		/// <summary>
		/// Subscription ID is not active.
		/// </summary>
		InvalidSubscription = 0x7d,

		/// <summary>
		/// The sender of the action or command does not have authorization or access.
		/// </summary>
		UnsupportedAccess = 0x7e,

		/// <summary>
		/// The endpoint indicated is unsupported on the node.
		/// </summary>
		UnsupportedEndpoint = 0x7f,

		/// <summary>
		/// The action is malformed, has missing information, or incorrect information.
		/// </summary>
		InvalidAction = 0x80,

		/// <summary>
		/// The command is not supported.
		/// </summary>
		UnsupportedCommand = 0x81,

		/// <summary>
		/// The command is invalid.
		/// </summary>
		InvalidCommand = 0x85,

		/// <summary>
		/// The attribute is not supported.
		/// </summary>
		UnsupportedAttribute = 0x86,

		/// <summary>
		/// Out of range error, or set to a reserved value.
		/// </summary>
		ConstraintError = 0x87,

		/// <summary>
		/// Attempt to write a read-only attribute.
		/// </summary>
		UnsupportedWrite = 0x88,

		/// <summary>
		/// An action or operation failed due to insufficient available resources.
		/// </summary>
		ResourceExhausted = 0x89,

		/// <summary>
		/// The indicated data field or entry could not be found.
		/// </summary>
		NotFound = 0x8b,

		/// <summary>
		/// Reports cannot be issued for this attribute.
		/// </summary>
		UnreportableAttribute = 0x8c,

		/// <summary>
		/// The data type indicated is undefined or invalid for the indicated data field.
		/// </summary>
		InvalidDataType = 0x8d,

		/// <summary>
		/// Attempt to read a write-only attribute.
		/// </summary>
		UnsupportedRead = 0x8f,

		/// <summary>
		/// Cluster-specific data version mismatch.
		/// </summary>
		DataVersionMismatch = 0x92,

		/// <summary>
		/// The transaction was aborted due to time being exceeded.
		/// </summary>
		Timeout = 0x94,

		/// <summary>
		/// The receiver is busy processing another action.
		/// </summary>
		Busy = 0x9c,

		/// <summary>
		/// Access to the requested resource is restricted.
		/// </summary>
		AccessRestricted = 0x9d,

		/// <summary>
		/// The cluster indicated is not supported.
		/// </summary>
		UnsupportedCluster = 0xc3,

		/// <summary>
		/// Used by proxies to signal that there is no upstream subscription.
		/// </summary>
		NoUpstreamSubscription = 0xc5,

		/// <summary>
		/// A timed interaction is required.
		/// </summary>
		NeedsTimedInteraction = 0xc6,

		/// <summary>
		/// The event indicated is unsupported on the cluster.
		/// </summary>
		UnsupportedEvent = 0xc7,

		/// <summary>
		/// The receiver has insufficient resources to support the specified number of paths.
		/// </summary>
		PathsExhausted = 0xc8,

		/// <summary>
		/// A request with TimedRequest field set to TRUE was issued outside a Timed transaction or vice versa.
		/// </summary>
		TimedRequestMismatch = 0xc9,

		/// <summary>
		/// A request requiring a fail-safe timer was received without an armed fail-safe.
		/// </summary>
		FailsafeRequired = 0xca,

		/// <summary>
		/// The received request cannot be handled in the current state.
		/// </summary>
		InvalidInState = 0xcb,

		/// <summary>
		/// A CommandDataIB is missing a response in the InvokeResponses of an Invoke Response action.
		/// </summary>
		NoCommandResponse = 0xcc,

		/// <summary>
		/// Terms and conditions changed.
		/// </summary>
		TermsAndConditionsChanged = 0xcd,

		/// <summary>
		/// Maintenance required.
		/// </summary>
		MaintenanceRequired = 0xce
	}

	/// <summary>
	/// Interaction model status (StatusIB).
	/// </summary>
	public class ImStatus
	{
		/// <summary>
		/// Success status.
		/// </summary>
		public static readonly ImStatus Success = new ImStatus(ImStatusCode.Success, null);

		/// <summary>
		/// Interaction model status.
		/// </summary>
		/// <param name="Status">Status code.</param>
		/// <param name="ClusterStatus">Cluster-specific status code, if any.</param>
		public ImStatus(ImStatusCode Status, byte? ClusterStatus)
		{
			this.Status = Status;
			this.ClusterStatus = ClusterStatus;
		}

		/// <summary>
		/// Status code.
		/// </summary>
		public ImStatusCode Status { get; }

		/// <summary>
		/// Cluster-specific status code, if any.
		/// </summary>
		public byte? ClusterStatus { get; }

		/// <summary>
		/// If the status represents success.
		/// </summary>
		public bool IsSuccess => this.Status == ImStatusCode.Success;

		/// <summary>
		/// Parses a StatusIB structure.
		/// </summary>
		/// <param name="Element">TLV structure.</param>
		/// <returns>Status.</returns>
		public static ImStatus Parse(TlvElement Element)
		{
			ImStatusCode Code = (ImStatusCode)Element.Required(0).AsByte();
			byte? ClusterStatus = null;

			if (Element.TryGet(1, out TlvElement E) && !E.IsNull)
				ClusterStatus = E.AsByte();

			return new ImStatus(Code, ClusterStatus);
		}

		/// <summary>
		/// Encodes the status as a StatusIB structure.
		/// </summary>
		/// <param name="Output">TLV writer.</param>
		/// <param name="Tag">Tag.</param>
		public void Write(TlvWriter Output, TlvTag Tag)
		{
			Output.StartStructure(Tag);
			Output.WriteUInt(0, (byte)this.Status);

			if (this.ClusterStatus.HasValue)
				Output.WriteUInt(1, this.ClusterStatus.Value);

			Output.EndContainer();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			if (this.ClusterStatus.HasValue)
				return this.Status.ToString() + " (cluster status 0x" + this.ClusterStatus.Value.ToString("X2") + ")";
			else
				return this.Status.ToString();
		}
	}

	/// <summary>
	/// Exception raised when an interaction fails with a status code.
	/// </summary>
	public class ImStatusException : MatterException
	{
		/// <summary>
		/// Exception raised when an interaction fails with a status code.
		/// </summary>
		/// <param name="Status">Status.</param>
		public ImStatusException(ImStatus Status)
			: base("Interaction failed: " + Status.ToString())
		{
			this.Status = Status;
		}

		/// <summary>
		/// Exception raised when an interaction fails with a status code.
		/// </summary>
		/// <param name="Status">Status.</param>
		/// <param name="Context">Context information.</param>
		public ImStatusException(ImStatus Status, string Context)
			: base(Context + ": " + Status.ToString())
		{
			this.Status = Status;
		}

		/// <summary>
		/// Status.
		/// </summary>
		public ImStatus Status { get; }
	}
}
