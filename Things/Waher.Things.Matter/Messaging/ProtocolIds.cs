namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Matter protocol IDs (Matter Core Specification, 4.4.3.1).
	/// </summary>
	public static class ProtocolIds
	{
		/// <summary>
		/// Secure Channel protocol.
		/// </summary>
		public const ushort SecureChannel = 0x0000;

		/// <summary>
		/// Interaction Model protocol.
		/// </summary>
		public const ushort InteractionModel = 0x0001;

		/// <summary>
		/// Bulk Data Exchange protocol.
		/// </summary>
		public const ushort BulkDataExchange = 0x0002;

		/// <summary>
		/// User Directed Commissioning protocol.
		/// </summary>
		public const ushort UserDirectedCommissioning = 0x0003;

		/// <summary>
		/// For testing.
		/// </summary>
		public const ushort ForTesting = 0x0004;

		/// <summary>
		/// Gets a human-readable name of a protocol.
		/// </summary>
		/// <param name="ProtocolId">Protocol ID.</param>
		/// <returns>Name.</returns>
		public static string GetName(ushort ProtocolId)
		{
			switch (ProtocolId)
			{
				case SecureChannel: return "SecureChannel";
				case InteractionModel: return "IM";
				case BulkDataExchange: return "BDX";
				case UserDirectedCommissioning: return "UDC";
				case ForTesting: return "Test";
				default: return "0x" + ProtocolId.ToString("X4");
			}
		}
	}

	/// <summary>
	/// Secure Channel protocol opcodes (Matter Core Specification, 4.11.1).
	/// </summary>
	public enum SecureChannelOpcode : byte
	{
		/// <summary>
		/// Message counter synchronization request.
		/// </summary>
		MsgCounterSyncReq = 0x00,

		/// <summary>
		/// Message counter synchronization response.
		/// </summary>
		MsgCounterSyncRsp = 0x01,

		/// <summary>
		/// MRP standalone acknowledgement.
		/// </summary>
		StandaloneAck = 0x10,

		/// <summary>
		/// PASE: PBKDF parameter request.
		/// </summary>
		PbkdfParamRequest = 0x20,

		/// <summary>
		/// PASE: PBKDF parameter response.
		/// </summary>
		PbkdfParamResponse = 0x21,

		/// <summary>
		/// PASE: Pake1.
		/// </summary>
		PasePake1 = 0x22,

		/// <summary>
		/// PASE: Pake2.
		/// </summary>
		PasePake2 = 0x23,

		/// <summary>
		/// PASE: Pake3.
		/// </summary>
		PasePake3 = 0x24,

		/// <summary>
		/// CASE: Sigma1.
		/// </summary>
		CaseSigma1 = 0x30,

		/// <summary>
		/// CASE: Sigma2.
		/// </summary>
		CaseSigma2 = 0x31,

		/// <summary>
		/// CASE: Sigma3.
		/// </summary>
		CaseSigma3 = 0x32,

		/// <summary>
		/// CASE: Sigma2 resume.
		/// </summary>
		CaseSigma2Resume = 0x33,

		/// <summary>
		/// Status report.
		/// </summary>
		StatusReport = 0x40,

		/// <summary>
		/// ICD check-in message.
		/// </summary>
		IcdCheckIn = 0x50
	}

	/// <summary>
	/// Interaction Model protocol opcodes (Matter Core Specification, 10.2.1).
	/// </summary>
	public enum InteractionModelOpcode : byte
	{
		/// <summary>
		/// Status response.
		/// </summary>
		StatusResponse = 0x01,

		/// <summary>
		/// Read request.
		/// </summary>
		ReadRequest = 0x02,

		/// <summary>
		/// Subscribe request.
		/// </summary>
		SubscribeRequest = 0x03,

		/// <summary>
		/// Subscribe response.
		/// </summary>
		SubscribeResponse = 0x04,

		/// <summary>
		/// Report data.
		/// </summary>
		ReportData = 0x05,

		/// <summary>
		/// Write request.
		/// </summary>
		WriteRequest = 0x06,

		/// <summary>
		/// Write response.
		/// </summary>
		WriteResponse = 0x07,

		/// <summary>
		/// Invoke request.
		/// </summary>
		InvokeRequest = 0x08,

		/// <summary>
		/// Invoke response.
		/// </summary>
		InvokeResponse = 0x09,

		/// <summary>
		/// Timed request.
		/// </summary>
		TimedRequest = 0x0a
	}
}
