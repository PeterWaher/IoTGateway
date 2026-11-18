using System;
using System.IO;
using System.Text;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Exchange flags of the protocol header (Matter Core Specification, 4.4.3.1).
	/// </summary>
	[Flags]
	public enum ExchangeFlags : byte
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None = 0,

		/// <summary>
		/// Message sent by the initiator of the exchange.
		/// </summary>
		Initiator = 0x01,

		/// <summary>
		/// Message acknowledges a previous message.
		/// </summary>
		Acknowledgement = 0x02,

		/// <summary>
		/// Message requires an acknowledgement (reliable).
		/// </summary>
		Reliability = 0x04,

		/// <summary>
		/// Secured extensions present.
		/// </summary>
		SecuredExtensions = 0x08,

		/// <summary>
		/// Vendor ID present.
		/// </summary>
		Vendor = 0x10
	}

	/// <summary>
	/// Matter protocol header (Matter Core Specification, 4.4.3).
	/// </summary>
	public class ProtocolHeader
	{
		/// <summary>
		/// Matter protocol header.
		/// </summary>
		public ProtocolHeader()
		{
		}

		/// <summary>
		/// Exchange flags.
		/// </summary>
		public ExchangeFlags Flags { get; set; }

		/// <summary>
		/// Protocol opcode.
		/// </summary>
		public byte Opcode { get; set; }

		/// <summary>
		/// Exchange ID.
		/// </summary>
		public ushort ExchangeId { get; set; }

		/// <summary>
		/// Protocol vendor ID (0 for standard protocols).
		/// </summary>
		public ushort VendorId { get; set; }

		/// <summary>
		/// Protocol ID.
		/// </summary>
		public ushort ProtocolId { get; set; }

		/// <summary>
		/// Acknowledged message counter, if present.
		/// </summary>
		public uint? AckedMessageCounter { get; set; }

		/// <summary>
		/// Secured extensions, if present.
		/// </summary>
		public byte[] SecuredExtensions { get; set; }

		/// <summary>
		/// If message was sent by the exchange initiator.
		/// </summary>
		public bool IsInitiator => (this.Flags & ExchangeFlags.Initiator) != 0;

		/// <summary>
		/// If message requires an acknowledgement.
		/// </summary>
		public bool NeedsAck => (this.Flags & ExchangeFlags.Reliability) != 0;

		/// <summary>
		/// Encodes the header followed by the application payload.
		/// </summary>
		/// <param name="Payload">Application payload.</param>
		/// <returns>Encoded protocol header and payload.</returns>
		public byte[] Encode(byte[] Payload)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				ExchangeFlags Flags = this.Flags & (ExchangeFlags.Initiator | ExchangeFlags.Reliability);

				if (this.AckedMessageCounter.HasValue)
					Flags |= ExchangeFlags.Acknowledgement;

				if (this.VendorId != 0)
					Flags |= ExchangeFlags.Vendor;

				if (!(this.SecuredExtensions is null))
					Flags |= ExchangeFlags.SecuredExtensions;

				ms.WriteByte((byte)Flags);
				ms.WriteByte(this.Opcode);
				MessageHeader.WriteLE(ms, this.ExchangeId, 2);

				if (this.VendorId != 0)
					MessageHeader.WriteLE(ms, this.VendorId, 2);

				MessageHeader.WriteLE(ms, this.ProtocolId, 2);

				if (this.AckedMessageCounter.HasValue)
					MessageHeader.WriteLE(ms, this.AckedMessageCounter.Value, 4);

				if (!(this.SecuredExtensions is null))
				{
					MessageHeader.WriteLE(ms, (ulong)this.SecuredExtensions.Length, 2);
					ms.Write(this.SecuredExtensions, 0, this.SecuredExtensions.Length);
				}

				if (!(Payload is null))
					ms.Write(Payload, 0, Payload.Length);

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Decodes a protocol header.
		/// </summary>
		/// <param name="Data">Decrypted message payload.</param>
		/// <param name="Payload">Application payload following the header.</param>
		/// <returns>Protocol header.</returns>
		public static ProtocolHeader Decode(byte[] Data, out byte[] Payload)
		{
			if (Data.Length < 6)
				throw new MatterDecodingException("Protocol header too short.");

			int Pos = 0;
			ProtocolHeader Result = new ProtocolHeader()
			{
				Flags = (ExchangeFlags)Data[Pos++],
				Opcode = Data[Pos++]
			};

			Result.ExchangeId = (ushort)MessageHeader.ReadLE(Data, ref Pos, 2);

			if ((Result.Flags & ExchangeFlags.Vendor) != 0)
				Result.VendorId = (ushort)MessageHeader.ReadLE(Data, ref Pos, 2);

			Result.ProtocolId = (ushort)MessageHeader.ReadLE(Data, ref Pos, 2);

			if ((Result.Flags & ExchangeFlags.Acknowledgement) != 0)
				Result.AckedMessageCounter = (uint)MessageHeader.ReadLE(Data, ref Pos, 4);

			if ((Result.Flags & ExchangeFlags.SecuredExtensions) != 0)
			{
				int Len = (int)MessageHeader.ReadLE(Data, ref Pos, 2);
				if (Pos + Len > Data.Length)
					throw new MatterDecodingException("Secured extensions exceed message.");

				Result.SecuredExtensions = new byte[Len];
				Array.Copy(Data, Pos, Result.SecuredExtensions, 0, Len);
				Pos += Len;
			}

			Payload = new byte[Data.Length - Pos];
			Array.Copy(Data, Pos, Payload, 0, Payload.Length);

			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(ProtocolIds.GetName(this.ProtocolId));
			sb.Append(':');

			if (this.ProtocolId == ProtocolIds.SecureChannel && this.VendorId == 0)
				sb.Append(((SecureChannelOpcode)this.Opcode).ToString());
			else if (this.ProtocolId == ProtocolIds.InteractionModel && this.VendorId == 0)
				sb.Append(((InteractionModelOpcode)this.Opcode).ToString());
			else
			{
				sb.Append("0x");
				sb.Append(this.Opcode.ToString("X2"));
			}

			sb.Append(", Exchange=");
			sb.Append(this.ExchangeId.ToString());
			sb.Append(this.IsInitiator ? "i" : "r");

			if (this.NeedsAck)
				sb.Append(", R");

			if (this.AckedMessageCounter.HasValue)
			{
				sb.Append(", Ack=");
				sb.Append(this.AckedMessageCounter.Value.ToString());
			}

			return sb.ToString();
		}
	}
}
