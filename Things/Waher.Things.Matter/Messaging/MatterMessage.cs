using System.Net;
using Waher.Things.Matter.Tlv;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// A received (decoded and decrypted) Matter message.
	/// </summary>
	public class MatterMessage
	{
		private TlvElement tlv;

		/// <summary>
		/// A received (decoded and decrypted) Matter message.
		/// </summary>
		/// <param name="Header">Message header.</param>
		/// <param name="ProtocolHeader">Protocol header.</param>
		/// <param name="Payload">Application payload.</param>
		/// <param name="From">Remote endpoint.</param>
		/// <param name="Session">Session on which the message was received.</param>
		public MatterMessage(MessageHeader Header, ProtocolHeader ProtocolHeader, byte[] Payload, IPEndPoint From, Session Session)
		{
			this.Header = Header;
			this.ProtocolHeader = ProtocolHeader;
			this.Payload = Payload;
			this.From = From;
			this.Session = Session;
		}

		/// <summary>
		/// Message header.
		/// </summary>
		public MessageHeader Header { get; }

		/// <summary>
		/// Protocol header.
		/// </summary>
		public ProtocolHeader ProtocolHeader { get; }

		/// <summary>
		/// Application payload.
		/// </summary>
		public byte[] Payload { get; }

		/// <summary>
		/// Remote endpoint.
		/// </summary>
		public IPEndPoint From { get; }

		/// <summary>
		/// Session.
		/// </summary>
		public Session Session { get; }

		/// <summary>
		/// Protocol ID.
		/// </summary>
		public ushort ProtocolId => this.ProtocolHeader.ProtocolId;

		/// <summary>
		/// Opcode.
		/// </summary>
		public byte Opcode => this.ProtocolHeader.Opcode;

		/// <summary>
		/// Checks if the message is of a given protocol and opcode.
		/// </summary>
		/// <param name="ProtocolId">Protocol ID.</param>
		/// <param name="Opcode">Opcode.</param>
		/// <returns>If message matches.</returns>
		public bool Is(ushort ProtocolId, byte Opcode)
		{
			return this.ProtocolHeader.ProtocolId == ProtocolId &&
				this.ProtocolHeader.Opcode == Opcode &&
				this.ProtocolHeader.VendorId == 0;
		}

		/// <summary>
		/// If the message is a secure channel status report.
		/// </summary>
		public bool IsStatusReport => this.Is(ProtocolIds.SecureChannel, (byte)SecureChannelOpcode.StatusReport);

		/// <summary>
		/// Decodes the payload as TLV.
		/// </summary>
		/// <returns>Decoded TLV.</returns>
		public TlvElement DecodeTlv()
		{
			if (this.tlv is null)
				this.tlv = TlvElement.Decode(this.Payload);

			return this.tlv;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.ProtocolHeader.ToString() + ", " + this.Header.ToString();
		}
	}
}
