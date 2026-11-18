using System;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.SecureChannel
{
	/// <summary>
	/// Passcode-Authenticated Session Establishment (PASE), initiator side (Matter Core Specification, 4.14.1).
	/// </summary>
	public static class PaseClient
	{
		private static readonly byte[] pakeContextPrefix = Encoding.ASCII.GetBytes("CHIP PAKE V1 Commissioning");

		/// <summary>
		/// Minimum number of PBKDF2 iterations.
		/// </summary>
		public const int MinIterations = 1000;

		/// <summary>
		/// Maximum number of PBKDF2 iterations.
		/// </summary>
		public const int MaxIterations = 100000;

		/// <summary>
		/// Establishes a PASE session with a commissionee.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <param name="Peer">Address of commissionee.</param>
		/// <param name="Passcode">Setup passcode.</param>
		/// <returns>Secure session.</returns>
		public static async Task<SecureSession> EstablishAsync(ExchangeManager Manager, IPEndPoint Peer, uint Passcode)
		{
			UnsecuredSession Unsecured = Manager.CreateUnsecuredSession(Peer);
			Exchange Exchange = Manager.CreateExchange(Unsecured);
			ushort LocalSessionId = Manager.AllocateSessionId();
			bool Success = false;

			try
			{
				// PBKDFParamRequest

				byte[] InitiatorRandom = MatterCrypto.RandomBytes(32);
				TlvWriter w = new TlvWriter();

				w.StartStructure();
				w.WriteBytes(1, InitiatorRandom);
				w.WriteUInt(2, LocalSessionId);
				w.WriteUInt(3, 0);  // Passcode ID
				w.WriteBoolean(4, false);
				Manager.OwnParameters.Write(w, 5);
				w.EndContainer();

				byte[] Request = w.ToArray();
				MatterMessage Response = await Exchange.RequestAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.PbkdfParamRequest, Request);

				CheckStatusReport(Response);
				CheckOpcode(Response, SecureChannelOpcode.PbkdfParamResponse);

				TlvElement Resp = Response.DecodeTlv();
				byte[] EchoedRandom = Resp.Required(1).AsBytes();
				if (!MatterCrypto.AreEqual(EchoedRandom, InitiatorRandom))
					throw new MatterSecurityException("Initiator random mismatch in PBKDFParamResponse.");

				ushort PeerSessionId = Resp.Required(3).AsUInt16();
				TlvElement Pbkdf = Resp.Required(4);
				int Iterations = (int)Pbkdf.Required(1).AsUInt32();
				byte[] Salt = Pbkdf.Required(2).AsBytes();

				if (Iterations < MinIterations || Iterations > MaxIterations)
					throw new MatterSecurityException("Invalid PBKDF iteration count: " + Iterations.ToString());

				if (Salt.Length < 16 || Salt.Length > 32)
					throw new MatterSecurityException("Invalid PBKDF salt length.");

				SessionParameters PeerParameters = SessionParameters.Parse(Resp[5]);
				Unsecured.PeerParameters = PeerParameters;

				// SPAKE2+

				Spake2Plus.ComputeW0W1(Passcode, Salt, Iterations, out BigInteger W0, out BigInteger W1);

				byte[] Context = MatterCrypto.Sha256(pakeContextPrefix, Request, Response.Payload);
				Spake2Plus Prover = Spake2Plus.CreateProver(Context, W0, W1);
				byte[] X = Prover.ComputeShare();

				// Pake1

				w = new TlvWriter();
				w.StartStructure();
				w.WriteBytes(1, X);
				w.EndContainer();

				Response = await Exchange.RequestAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.PasePake1, w.ToArray(),
					TimeSpan.FromSeconds(10));

				CheckStatusReport(Response);
				CheckOpcode(Response, SecureChannelOpcode.PasePake2);

				TlvElement Pake2 = Response.DecodeTlv();
				byte[] Y = Pake2.Required(1).AsBytes();
				byte[] cB = Pake2.Required(2).AsBytes();

				Prover.ProcessPeerShare(Y);

				if (!MatterCrypto.AreEqual(cB, Prover.ConfirmationB))
				{
					await SendFailureAsync(Exchange, SecureChannelStatusCode.InvalidParameter);
					throw new MatterSecurityException("PASE verification failed (invalid passcode?).");
				}

				// Pake3

				w = new TlvWriter();
				w.StartStructure();
				w.WriteBytes(1, Prover.ConfirmationA);
				w.EndContainer();

				Response = await Exchange.RequestAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.PasePake3, w.ToArray());

				if (!Response.IsStatusReport)
					throw new MatterException("Unexpected response to Pake3.");

				StatusReport Report = StatusReport.Decode(Response.Payload);
				if (!Report.IsSuccess)
					throw new StatusReportException(Report);

				SecureSession Session = SecureSession.FromSharedSecret(Unsecured.PeerAddress, PeerParameters,
					LocalSessionId, PeerSessionId, true, true, 0, 0, Prover.Ke, null);

				Manager.AddSecureSession(Session);
				Success = true;

				return Session;
			}
			finally
			{
				await Exchange.CloseAsync();
				Manager.RemoveSession(Unsecured);

				if (!Success)
					Manager.ReleaseSessionId(LocalSessionId);
			}
		}

		/// <summary>
		/// Sends a failure status report on an exchange, terminating a session establishment attempt on the peer.
		/// Errors are ignored.
		/// </summary>
		/// <param name="Exchange">Exchange.</param>
		/// <param name="Code">Protocol status code.</param>
		internal static async Task SendFailureAsync(Exchange Exchange, SecureChannelStatusCode Code)
		{
			try
			{
				StatusReport Report = new StatusReport(GeneralStatusCode.Failure, ProtocolIds.SecureChannel, (ushort)Code);
				await Exchange.SendAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.StatusReport, Report.Encode());
				await Exchange.WaitForAcknowledgementAsync();
			}
			catch (Exception)
			{
				// Ignore
			}
		}

		internal static void CheckStatusReport(MatterMessage Message)
		{
			if (Message.IsStatusReport)
			{
				StatusReport Report = StatusReport.Decode(Message.Payload);
				throw new StatusReportException(Report);
			}
		}

		internal static void CheckOpcode(MatterMessage Message, SecureChannelOpcode Expected)
		{
			if (!Message.Is(ProtocolIds.SecureChannel, (byte)Expected))
				throw new MatterException("Unexpected message: " + Message.ProtocolHeader.ToString() + ", expected " + Expected.ToString() + ".");
		}
	}
}
