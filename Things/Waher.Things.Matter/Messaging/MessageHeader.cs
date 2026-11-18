using System;
using System.IO;
using System.Text;

namespace Waher.Things.Matter.Messaging
{
	/// <summary>
	/// Session type, as encoded in the security flags of the message header.
	/// </summary>
	public enum SessionType
	{
		/// <summary>
		/// Unicast session.
		/// </summary>
		Unicast = 0,

		/// <summary>
		/// Group session.
		/// </summary>
		Group = 1
	}

	/// <summary>
	/// Matter message header (Matter Core Specification, 4.4.1).
	/// </summary>
	public class MessageHeader
	{
		/// <summary>
		/// Flag: privacy enhancements (P).
		/// </summary>
		public const byte SecurityFlagPrivacy = 0x80;

		/// <summary>
		/// Flag: control message (C).
		/// </summary>
		public const byte SecurityFlagControl = 0x40;

		/// <summary>
		/// Flag: message extensions (MX).
		/// </summary>
		public const byte SecurityFlagExtensions = 0x20;

		/// <summary>
		/// Matter message header.
		/// </summary>
		public MessageHeader()
		{
		}

		/// <summary>
		/// Message format version.
		/// </summary>
		public byte Version { get; set; }

		/// <summary>
		/// Session ID (0 for unsecured sessions).
		/// </summary>
		public ushort SessionId { get; set; }

		/// <summary>
		/// Raw security flags.
		/// </summary>
		public byte SecurityFlags { get; set; }

		/// <summary>
		/// Session type.
		/// </summary>
		public SessionType SessionType
		{
			get => (SessionType)(this.SecurityFlags & 0x03);
			set => this.SecurityFlags = (byte)((this.SecurityFlags & 0xfc) | ((int)value & 0x03));
		}

		/// <summary>
		/// If privacy enhancements are used.
		/// </summary>
		public bool HasPrivacy => (this.SecurityFlags & SecurityFlagPrivacy) != 0;

		/// <summary>
		/// If the message is a control message.
		/// </summary>
		public bool IsControl => (this.SecurityFlags & SecurityFlagControl) != 0;

		/// <summary>
		/// If message extensions are present.
		/// </summary>
		public bool HasExtensions => (this.SecurityFlags & SecurityFlagExtensions) != 0;

		/// <summary>
		/// Message counter.
		/// </summary>
		public uint MessageCounter { get; set; }

		/// <summary>
		/// Source node ID, if present.
		/// </summary>
		public ulong? SourceNodeId { get; set; }

		/// <summary>
		/// Destination node ID, if present.
		/// </summary>
		public ulong? DestinationNodeId { get; set; }

		/// <summary>
		/// Destination group ID, if present.
		/// </summary>
		public ushort? DestinationGroupId { get; set; }

		/// <summary>
		/// Message extensions, if present.
		/// </summary>
		public byte[] Extensions { get; set; }

		/// <summary>
		/// Encodes the header.
		/// </summary>
		/// <returns>Encoded header.</returns>
		public byte[] Encode()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				byte Flags = (byte)((this.Version & 0x0f) << 4);

				if (this.SourceNodeId.HasValue)
					Flags |= 0x04;

				if (this.DestinationNodeId.HasValue)
					Flags |= 0x01;
				else if (this.DestinationGroupId.HasValue)
					Flags |= 0x02;

				byte SecFlags = this.SecurityFlags;
				if (!(this.Extensions is null))
					SecFlags |= SecurityFlagExtensions;
				else
					SecFlags &= unchecked((byte)~SecurityFlagExtensions);

				ms.WriteByte(Flags);
				WriteLE(ms, this.SessionId, 2);
				ms.WriteByte(SecFlags);
				WriteLE(ms, this.MessageCounter, 4);

				if (this.SourceNodeId.HasValue)
					WriteLE(ms, this.SourceNodeId.Value, 8);

				if (this.DestinationNodeId.HasValue)
					WriteLE(ms, this.DestinationNodeId.Value, 8);
				else if (this.DestinationGroupId.HasValue)
					WriteLE(ms, this.DestinationGroupId.Value, 2);

				if (!(this.Extensions is null))
				{
					WriteLE(ms, (ulong)this.Extensions.Length, 2);
					ms.Write(this.Extensions, 0, this.Extensions.Length);
				}

				return ms.ToArray();
			}
		}

		/// <summary>
		/// Decodes a message header.
		/// </summary>
		/// <param name="Data">Message data.</param>
		/// <param name="HeaderLength">Length of the encoded header.</param>
		/// <returns>Decoded header.</returns>
		public static MessageHeader Decode(byte[] Data, out int HeaderLength)
		{
			if (Data.Length < 8)
				throw new MatterDecodingException("Message too short.");

			int Pos = 0;
			byte Flags = Data[Pos++];
			MessageHeader Result = new MessageHeader()
			{
				Version = (byte)(Flags >> 4)
			};

			if (Result.Version != 0)
				throw new MatterDecodingException("Unsupported message version: " + Result.Version.ToString());

			Result.SessionId = (ushort)ReadLE(Data, ref Pos, 2);
			Result.SecurityFlags = Data[Pos++];
			Result.MessageCounter = (uint)ReadLE(Data, ref Pos, 4);

			if ((Flags & 0x04) != 0)
				Result.SourceNodeId = ReadLE(Data, ref Pos, 8);

			switch (Flags & 0x03)
			{
				case 1:
					Result.DestinationNodeId = ReadLE(Data, ref Pos, 8);
					break;

				case 2:
					Result.DestinationGroupId = (ushort)ReadLE(Data, ref Pos, 2);
					break;

				case 3:
					throw new MatterDecodingException("Invalid DSIZ value.");
			}

			if ((Result.SecurityFlags & SecurityFlagExtensions) != 0)
			{
				int Len = (int)ReadLE(Data, ref Pos, 2);
				if (Pos + Len > Data.Length)
					throw new MatterDecodingException("Message extensions exceed message.");

				byte[] Ext = new byte[Len];
				Array.Copy(Data, Pos, Ext, 0, Len);
				Pos += Len;
				Result.Extensions = Ext;
			}

			HeaderLength = Pos;
			return Result;
		}

		internal static void WriteLE(Stream Output, ulong Value, int NrBytes)
		{
			while (NrBytes-- > 0)
			{
				Output.WriteByte((byte)Value);
				Value >>= 8;
			}
		}

		internal static ulong ReadLE(byte[] Data, ref int Pos, int NrBytes)
		{
			if (Pos + NrBytes > Data.Length)
				throw new MatterDecodingException("Unexpected end of message.");

			ulong Result = 0;
			int i;

			for (i = NrBytes - 1; i >= 0; i--)
				Result = (Result << 8) | Data[Pos + i];

			Pos += NrBytes;
			return Result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("Session=");
			sb.Append(this.SessionId.ToString());
			sb.Append(", Counter=");
			sb.Append(this.MessageCounter.ToString());

			if (this.SourceNodeId.HasValue)
			{
				sb.Append(", Src=");
				sb.Append(this.SourceNodeId.Value.ToString("X16"));
			}

			if (this.DestinationNodeId.HasValue)
			{
				sb.Append(", Dst=");
				sb.Append(this.DestinationNodeId.Value.ToString("X16"));
			}

			if (this.DestinationGroupId.HasValue)
			{
				sb.Append(", Group=");
				sb.Append(this.DestinationGroupId.Value.ToString());
			}

			return sb.ToString();
		}
	}
}
