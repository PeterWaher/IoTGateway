using System;
using System.IO;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// General status codes of status reports (Matter Core Specification, Appendix D).
	/// </summary>
	public enum GeneralStatusCode : ushort
	{
		/// <summary>
		/// Operation completed successfully.
		/// </summary>
		Success = 0,

		/// <summary>
		/// Generic failure.
		/// </summary>
		Failure = 1,

		/// <summary>
		/// Operation was rejected due to a precondition not met.
		/// </summary>
		BadPrecondition = 2,

		/// <summary>
		/// A value was out of range.
		/// </summary>
		OutOfRange = 3,

		/// <summary>
		/// Request was malformed.
		/// </summary>
		BadRequest = 4,

		/// <summary>
		/// Request is not supported.
		/// </summary>
		Unsupported = 5,

		/// <summary>
		/// Message was unexpected.
		/// </summary>
		Unexpected = 6,

		/// <summary>
		/// Resources exhausted.
		/// </summary>
		ResourceExhausted = 7,

		/// <summary>
		/// Busy.
		/// </summary>
		Busy = 8,

		/// <summary>
		/// Timeout.
		/// </summary>
		Timeout = 9,

		/// <summary>
		/// Continue.
		/// </summary>
		Continue = 10,

		/// <summary>
		/// Aborted.
		/// </summary>
		Aborted = 11,

		/// <summary>
		/// Invalid argument.
		/// </summary>
		InvalidArgument = 12,

		/// <summary>
		/// Not found.
		/// </summary>
		NotFound = 13,

		/// <summary>
		/// Already exists.
		/// </summary>
		AlreadyExists = 14,

		/// <summary>
		/// Permission denied.
		/// </summary>
		PermissionDenied = 15,

		/// <summary>
		/// Data loss.
		/// </summary>
		DataLoss = 16
	}

	/// <summary>
	/// Secure channel protocol-specific status codes (Matter Core Specification, 4.11.1.3).
	/// </summary>
	public enum SecureChannelStatusCode : ushort
	{
		/// <summary>
		/// Session establishment successful.
		/// </summary>
		SessionEstablishmentSuccess = 0x0000,

		/// <summary>
		/// No shared trust roots.
		/// </summary>
		NoSharedTrustRoots = 0x0001,

		/// <summary>
		/// Invalid parameter.
		/// </summary>
		InvalidParameter = 0x0002,

		/// <summary>
		/// Close session.
		/// </summary>
		CloseSession = 0x0003,

		/// <summary>
		/// Busy.
		/// </summary>
		Busy = 0x0004,

		/// <summary>
		/// Session not found (session resumption).
		/// </summary>
		SessionNotFound = 0x0005
	}

	/// <summary>
	/// Status report message (Matter Core Specification, Appendix D).
	/// </summary>
	public class StatusReport
	{
		/// <summary>
		/// Status report message.
		/// </summary>
		/// <param name="GeneralCode">General code.</param>
		/// <param name="ProtocolId">Protocol ID (including vendor ID in the upper 16 bits).</param>
		/// <param name="ProtocolCode">Protocol-specific code.</param>
		/// <param name="ProtocolData">Optional protocol-specific data.</param>
		public StatusReport(GeneralStatusCode GeneralCode, uint ProtocolId, ushort ProtocolCode, byte[] ProtocolData = null)
		{
			this.GeneralCode = GeneralCode;
			this.ProtocolId = ProtocolId;
			this.ProtocolCode = ProtocolCode;
			this.ProtocolData = ProtocolData;
		}

		/// <summary>
		/// General status code.
		/// </summary>
		public GeneralStatusCode GeneralCode { get; }

		/// <summary>
		/// Protocol ID (vendor ID in upper 16 bits).
		/// </summary>
		public uint ProtocolId { get; }

		/// <summary>
		/// Protocol-specific status code.
		/// </summary>
		public ushort ProtocolCode { get; }

		/// <summary>
		/// Protocol-specific data.
		/// </summary>
		public byte[] ProtocolData { get; }

		/// <summary>
		/// If the report signals success.
		/// </summary>
		public bool IsSuccess => this.GeneralCode == GeneralStatusCode.Success;

		/// <summary>
		/// If the report is a secure channel CloseSession report.
		/// </summary>
		public bool IsCloseSession =>
			this.ProtocolId == ProtocolIds.SecureChannel &&
			this.ProtocolCode == (ushort)SecureChannelStatusCode.CloseSession;

		/// <summary>
		/// Encodes the status report.
		/// </summary>
		/// <returns>Encoded report.</returns>
		public byte[] Encode()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				MessageHeader.WriteLE(ms, (ushort)this.GeneralCode, 2);
				MessageHeader.WriteLE(ms, this.ProtocolId, 4);
				MessageHeader.WriteLE(ms, this.ProtocolCode, 2);

				if (!(this.ProtocolData is null))
					ms.Write(this.ProtocolData, 0, this.ProtocolData.Length);

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Decodes a status report.
		/// </summary>
		/// <param name="Data">Encoded report.</param>
		/// <returns>Status report.</returns>
		public static StatusReport Decode(byte[] Data)
		{
			if (Data.Length < 8)
				throw new MatterDecodingException("Status report too short.");

			int Pos = 0;
			GeneralStatusCode GeneralCode = (GeneralStatusCode)MessageHeader.ReadLE(Data, ref Pos, 2);
			uint ProtocolId = (uint)MessageHeader.ReadLE(Data, ref Pos, 4);
			ushort ProtocolCode = (ushort)MessageHeader.ReadLE(Data, ref Pos, 2);
			byte[] ProtocolData = null;

			if (Pos < Data.Length)
			{
				ProtocolData = new byte[Data.Length - Pos];
				Array.Copy(Data, Pos, ProtocolData, 0, ProtocolData.Length);
			}

			return new StatusReport(GeneralCode, ProtocolId, ProtocolCode, ProtocolData);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			string s = this.GeneralCode.ToString();

			if (this.ProtocolId == ProtocolIds.SecureChannel)
				s += ", " + ((SecureChannelStatusCode)this.ProtocolCode).ToString();
			else
				s += ", Protocol 0x" + this.ProtocolId.ToString("X8") + ", Code 0x" + this.ProtocolCode.ToString("X4");

			return s;
		}
	}

	/// <summary>
	/// Exception raised when a peer responds with an error status report.
	/// </summary>
	public class StatusReportException : MatterException
	{
		/// <summary>
		/// Exception raised when a peer responds with an error status report.
		/// </summary>
		/// <param name="Report">Status report.</param>
		public StatusReportException(StatusReport Report)
			: base("Peer reported: " + Report.ToString())
		{
			this.Report = Report;
		}

		/// <summary>
		/// Status report.
		/// </summary>
		public StatusReport Report { get; }
	}
}
