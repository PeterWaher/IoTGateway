using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Things.Matter.Certificates;
using Waher.Things.Matter.Crypto;
using Waher.Things.Matter.Fabric;
using Waher.Things.Matter.Messaging;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.SecureChannel
{
	/// <summary>
	/// Information needed to resume a CASE session (Matter Core Specification, 4.14.2.2).
	/// </summary>
	public class ResumptionRecord
	{
		/// <summary>
		/// Information needed to resume a CASE session.
		/// </summary>
		/// <param name="PeerNodeId">Peer node ID.</param>
		/// <param name="SharedSecret">Shared secret of the original session.</param>
		/// <param name="ResumptionId">Resumption ID.</param>
		public ResumptionRecord(ulong PeerNodeId, byte[] SharedSecret, byte[] ResumptionId)
		{
			this.PeerNodeId = PeerNodeId;
			this.SharedSecret = SharedSecret;
			this.ResumptionId = ResumptionId;
		}

		/// <summary>
		/// Peer node ID.
		/// </summary>
		public ulong PeerNodeId { get; }

		/// <summary>
		/// Shared secret of the original session.
		/// </summary>
		public byte[] SharedSecret { get; }

		/// <summary>
		/// Resumption ID.
		/// </summary>
		public byte[] ResumptionId { get; internal set; }
	}

	/// <summary>
	/// Certificate Authenticated Session Establishment (CASE), initiator side (Matter Core Specification, 4.14.2).
	/// </summary>
	public static class CaseClient
	{
		private static readonly byte[] sigma2Info = Encoding.ASCII.GetBytes("Sigma2");
		private static readonly byte[] sigma3Info = Encoding.ASCII.GetBytes("Sigma3");
		private static readonly byte[] sigma1ResumeInfo = Encoding.ASCII.GetBytes("Sigma1_Resume");
		private static readonly byte[] sigma2ResumeInfo = Encoding.ASCII.GetBytes("Sigma2_Resume");
		private static readonly byte[] sigma2Nonce = Encoding.ASCII.GetBytes("NCASE_Sigma2N");
		private static readonly byte[] sigma3Nonce = Encoding.ASCII.GetBytes("NCASE_Sigma3N");
		private static readonly byte[] resume1Nonce = Encoding.ASCII.GetBytes("NCASE_SigmaS1");
		private static readonly byte[] resume2Nonce = Encoding.ASCII.GetBytes("NCASE_SigmaS2");

		/// <summary>
		/// Establishes a CASE session with an operational node.
		/// </summary>
		/// <param name="Manager">Exchange manager.</param>
		/// <param name="Peer">Address of peer.</param>
		/// <param name="Fabric">Fabric.</param>
		/// <param name="PeerNodeId">Operational node ID of peer.</param>
		/// <param name="Resumption">Resumption record from a previous session, or null.</param>
		/// <returns>Secure session, and the resumption record to use for future sessions.</returns>
		public static async Task<(SecureSession, ResumptionRecord)> EstablishAsync(ExchangeManager Manager, IPEndPoint Peer,
			MatterFabric Fabric, ulong PeerNodeId, ResumptionRecord Resumption = null)
		{
			UnsecuredSession Unsecured = Manager.CreateUnsecuredSession(Peer);
			Exchange Exchange = Manager.CreateExchange(Unsecured);
			ushort LocalSessionId = Manager.AllocateSessionId();
			bool Success = false;

			try
			{
				using (EcKeyPair Ephemeral = EcKeyPair.Generate())
				{
					byte[] InitiatorRandom = MatterCrypto.RandomBytes(32);
					byte[] DestinationId = Fabric.ComputeDestinationId(InitiatorRandom, PeerNodeId);

					// Sigma1

					TlvWriter w = new TlvWriter();
					w.StartStructure();
					w.WriteBytes(1, InitiatorRandom);
					w.WriteUInt(2, LocalSessionId);
					w.WriteBytes(3, DestinationId);
					w.WriteBytes(4, Ephemeral.PublicKey);
					Manager.OwnParameters.Write(w, 5);

					if (!(Resumption is null) && Resumption.PeerNodeId == PeerNodeId)
					{
						byte[] ResumeKey = MatterCrypto.Hkdf(Resumption.SharedSecret,
							MatterCrypto.Concat(InitiatorRandom, Resumption.ResumptionId), sigma1ResumeInfo, 16);
						byte[] ResumeMic = AesCcm128.Encrypt(ResumeKey, resume1Nonce, Array.Empty<byte>(), null);

						w.WriteBytes(6, Resumption.ResumptionId);
						w.WriteBytes(7, ResumeMic);
					}
					else
						Resumption = null;

					w.EndContainer();

					byte[] Sigma1 = w.ToArray();
					MatterMessage Response = await Exchange.RequestAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.CaseSigma1,
						Sigma1, TimeSpan.FromSeconds(10));

					PaseClient.CheckStatusReport(Response);

					if (Response.Is(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.CaseSigma2Resume))
					{
						if (Resumption is null)
							throw new MatterSecurityException("Unexpected Sigma2_Resume.");

						TlvElement Resume = Response.DecodeTlv();
						byte[] NewResumptionId = Resume.Required(1).AsBytes();
						byte[] Mic = Resume.Required(2).AsBytes();
						ushort PeerSessionId2 = Resume.Required(3).AsUInt16();
						SessionParameters PeerParameters2 = SessionParameters.Parse(Resume[4]);

						byte[] ResumeKey2 = MatterCrypto.Hkdf(Resumption.SharedSecret,
							MatterCrypto.Concat(InitiatorRandom, NewResumptionId), sigma2ResumeInfo, 16);

						AesCcm128.Decrypt(ResumeKey2, resume2Nonce, Mic, null);    // Throws if invalid.

						SecureSession Resumed = SecureSession.FromSharedSecret(Unsecured.PeerAddress, PeerParameters2, LocalSessionId,
							PeerSessionId2, true, false, Fabric.ControllerNodeId, PeerNodeId, Resumption.SharedSecret,
							MatterCrypto.Concat(InitiatorRandom, Resumption.ResumptionId), "SessionResumptionKeys");

						Resumed.Tag = Fabric;

						StatusReport Ok = new StatusReport(GeneralStatusCode.Success, ProtocolIds.SecureChannel,
							(ushort)SecureChannelStatusCode.SessionEstablishmentSuccess);

						Manager.AddSecureSession(Resumed);
						await Exchange.SendAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.StatusReport, Ok.Encode());

						if (!await Exchange.WaitForAcknowledgementAsync())
						{
							Manager.RemoveSession(Resumed);
							throw new MatterTimeoutException("Session resumption not acknowledged by peer.");
						}

						Success = true;

						return (Resumed, new ResumptionRecord(PeerNodeId, Resumption.SharedSecret, NewResumptionId));
					}

					PaseClient.CheckOpcode(Response, SecureChannelOpcode.CaseSigma2);

					// Sigma2

					byte[] Sigma2 = Response.Payload;
					TlvElement S2 = Response.DecodeTlv();
					byte[] ResponderRandom = S2.Required(1).AsBytes();
					ushort PeerSessionId = S2.Required(2).AsUInt16();
					byte[] ResponderEphPubKey = S2.Required(3).AsBytes();
					byte[] Encrypted2 = S2.Required(4).AsBytes();
					SessionParameters PeerParameters = SessionParameters.Parse(S2[5]);

					Unsecured.PeerParameters = PeerParameters;

					byte[] SharedSecret = Ephemeral.Ecdh(ResponderEphPubKey);
					byte[] S2kSalt = MatterCrypto.Concat(Fabric.OperationalIpk, ResponderRandom, ResponderEphPubKey, MatterCrypto.Sha256(Sigma1));
					byte[] S2k = MatterCrypto.Hkdf(SharedSecret, S2kSalt, sigma2Info, 16);
					byte[] TbeData2 = AesCcm128.Decrypt(S2k, sigma2Nonce, Encrypted2, null);

					TlvElement Tbe2 = TlvElement.Decode(TbeData2);
					byte[] ResponderNoc = Tbe2.Required(1).AsBytes();
					byte[] ResponderIcac = Tbe2[2]?.AsBytes();
					byte[] Signature2 = Tbe2.Required(3).AsBytes();
					byte[] ResumptionId = Tbe2.Required(4).AsBytes();

					MatterCertificate PeerNoc;

					try
					{
						PeerNoc = Fabric.ValidatePeerCredentials(ResponderNoc, ResponderIcac);
					}
					catch (Exception)
					{
						await PaseClient.SendFailureAsync(Exchange, SecureChannelStatusCode.InvalidParameter);
						throw;
					}

					if (PeerNoc.Subject.NodeId.Value != PeerNodeId)
					{
						await PaseClient.SendFailureAsync(Exchange, SecureChannelStatusCode.InvalidParameter);
						throw new MatterSecurityException("Peer node ID mismatch.");
					}

					w = new TlvWriter();
					w.StartStructure();
					w.WriteBytes(1, ResponderNoc);
					if (!(ResponderIcac is null))
						w.WriteBytes(2, ResponderIcac);
					w.WriteBytes(3, ResponderEphPubKey);
					w.WriteBytes(4, Ephemeral.PublicKey);
					w.EndContainer();

					if (!EcKeyPair.Verify(PeerNoc.PublicKey, w.ToArray(), Signature2))
					{
						await PaseClient.SendFailureAsync(Exchange, SecureChannelStatusCode.InvalidParameter);
						throw new MatterSecurityException("Invalid Sigma2 signature.");
					}

					// Sigma3

					w = new TlvWriter();
					w.StartStructure();
					w.WriteBytes(1, Fabric.ControllerNoc.EncodeTlv());
					w.WriteBytes(3, Ephemeral.PublicKey);
					w.WriteBytes(4, ResponderEphPubKey);
					w.EndContainer();

					byte[] Signature3 = Fabric.OperationalKey.Sign(w.ToArray());

					w = new TlvWriter();
					w.StartStructure();
					w.WriteBytes(1, Fabric.ControllerNoc.EncodeTlv());
					w.WriteBytes(3, Signature3);
					w.EndContainer();

					byte[] S3kSalt = MatterCrypto.Concat(Fabric.OperationalIpk, MatterCrypto.Sha256(Sigma1, Sigma2));
					byte[] S3k = MatterCrypto.Hkdf(SharedSecret, S3kSalt, sigma3Info, 16);
					byte[] Encrypted3 = AesCcm128.Encrypt(S3k, sigma3Nonce, w.ToArray(), null);

					w = new TlvWriter();
					w.StartStructure();
					w.WriteBytes(1, Encrypted3);
					w.EndContainer();

					byte[] Sigma3 = w.ToArray();

					Response = await Exchange.RequestAsync(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.CaseSigma3, Sigma3,
						TimeSpan.FromSeconds(10));

					if (!Response.IsStatusReport)
						throw new MatterException("Unexpected response to Sigma3.");

					StatusReport Report = StatusReport.Decode(Response.Payload);
					if (!Report.IsSuccess)
						throw new StatusReportException(Report);

					byte[] SessionSalt = MatterCrypto.Concat(Fabric.OperationalIpk, MatterCrypto.Sha256(Sigma1, Sigma2, Sigma3));
					SecureSession Session = SecureSession.FromSharedSecret(Unsecured.PeerAddress, PeerParameters, LocalSessionId,
						PeerSessionId, true, false, Fabric.ControllerNodeId, PeerNodeId, SharedSecret, SessionSalt);

					Session.Tag = Fabric;

					Manager.AddSecureSession(Session);
					Success = true;

					return (Session, new ResumptionRecord(PeerNodeId, SharedSecret, ResumptionId));
				}
			}
			finally
			{
				await Exchange.CloseAsync();
				Manager.RemoveSession(Unsecured);

				if (!Success)
					Manager.ReleaseSessionId(LocalSessionId);
			}
		}
	}
}
