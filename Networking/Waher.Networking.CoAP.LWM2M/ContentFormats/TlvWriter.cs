using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.LWM2M.ContentFormats
{
	/// <summary>
	/// Class used to serialize data into the TLV (Type-Length-Value) binary format.
	/// </summary>
	public class TlvWriter : ILwm2mWriter
	{
		private LinkedList<Tuple<IdentifierType?, ushort?, MemoryStream>> nested = null;
		private MemoryStream ms;
		private IdentifierType? currentIdentifierType = null;
		private ushort? currentIdentifier = null;

		/// <summary>
		/// Content format of generated payload.
		/// </summary>
		public ushort ContentFormat => Tlv.ContentFormatCode;

		/// <summary>
		/// Class used to serialize data into the TLV (Type-Length-Value) binary format.
		/// </summary>
		public TlvWriter()
		{
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Writes a TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, byte[] Value)
		{
			int c = Value.Length;
			byte b;

			b = (byte)IdentifierType;
			if (Identifier >= 256)
				b |= 32;

			if (c > 0xffffff)
				throw new ArgumentException("Value too large.", nameof(Value));
			else if (c > 0xffff)
				b |= 24;
			else if (c > 0xff)
				b |= 16;
			else if (c > 7)
				b |= 8;
			else
				b |= (byte)c;

			this.ms.WriteByte(b);

			if (Identifier >= 256)
				this.ms.WriteByte((byte)(Identifier >> 8)); // TODO: Network order?

			this.ms.WriteByte((byte)Identifier);

			if (c > 0xffff)
				this.ms.WriteByte((byte)(c >> 16)); // TODO: Network order?

			if (c > 0xff)
				this.ms.WriteByte((byte)(c >> 8)); // TODO: Network order?

			if (c > 7)
				this.ms.WriteByte((byte)c); // TODO: Network order?

			this.ms.Write(Value, 0, Value.Length);
		}

		/// <summary>
		/// Binary serialization of what has been written.
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			if (this.nested != null && this.nested.Last != null)
				throw new Exception("Nested TLVs not completed.");

			return this.ms.ToArray();
		}

		/// <summary>
		/// Begins a new nested TLV
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public void Begin(IdentifierType IdentifierType, ushort Identifier)
		{
			if (this.nested == null)
				this.nested = new LinkedList<Tuple<IdentifierType?, ushort?, MemoryStream>>();

			this.nested.AddLast(new Tuple<IdentifierType?, ushort?, MemoryStream>(
				this.currentIdentifierType, this.currentIdentifier, this.ms));

			this.currentIdentifierType = IdentifierType;
			this.currentIdentifier = Identifier;
			this.ms = new MemoryStream();
		}

		/// <summary>
		/// Ends a nested TLV
		/// </summary>
		public void End()
		{
			if (this.nested == null || this.nested.Last == null)
				throw new Exception("No nested TLV available.");

			byte[] Payload = this.ms.ToArray();
			Tuple<IdentifierType?, ushort?, MemoryStream> Top = this.nested.Last.Value;
			this.nested.RemoveLast();

			this.ms = Top.Item3;

			this.Write(this.currentIdentifierType.Value, this.currentIdentifier.Value, Payload);

			this.currentIdentifierType = Top.Item1;
			this.currentIdentifier = Top.Item2;
		}

		/// <summary>
		/// Writes a string-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, string Value)
		{
			this.Write(IdentifierType, Identifier, Encoding.UTF8.GetBytes(Value));
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, sbyte Value)
		{
			this.Write(IdentifierType, Identifier, new byte[] { (byte)Value });
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, short Value)
		{
			this.Write(IdentifierType, Identifier, new byte[] { (byte)(Value >> 8), (byte)Value });
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, int Value)
		{
			byte[] Payload = new byte[4];
			int i;

			for (i = 3; i >= 0; i--)
			{
				Payload[i] = (byte)Value;
				Value >>= 8;
			}

			this.Write(IdentifierType, Identifier, Payload);
		}

		/// <summary>
		/// Writes an Integer-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, long Value)
		{
			byte[] Payload = new byte[8];
			int i;

			for (i = 7; i >= 0; i--)
			{
				Payload[i] = (byte)Value;
				Value >>= 8;
			}

			this.Write(IdentifierType, Identifier, Payload);
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, float Value)
		{
			byte[] Payload = BitConverter.GetBytes(Value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(Payload);

			this.Write(IdentifierType, Identifier, Payload);
		}

		/// <summary>
		/// Writes a floating point-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, double Value)
		{
			byte[] Payload = BitConverter.GetBytes(Value);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(Payload);

			this.Write(IdentifierType, Identifier, Payload);
		}

		/// <summary>
		/// Writes a Boolean valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, bool Value)
		{
			this.Write(IdentifierType, Identifier, new byte[] { Value ? (byte)1 : (byte)0 });
		}

		/// <summary>
		/// Writes a DateTime-valued TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Value">Value</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, DateTime Value)
		{
			int UnixTime = (int)((Value.ToUniversalTime() - DtlsEndpoint.UnixEpoch).TotalSeconds + 0.5);

			this.Write(IdentifierType, Identifier, UnixTime);
		}

		/// <summary>
		/// Writes an object link TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="ObjectInstanceId">Object Instance ID</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier, ushort ObjectId,
			ushort ObjectInstanceId)
		{
			byte[] Payload = new byte[4];

			Payload[0] = (byte)(ObjectId >> 8);
			Payload[1] = (byte)ObjectId;
			Payload[2] = (byte)(ObjectInstanceId >> 8);
			Payload[3] = (byte)ObjectInstanceId;

			this.Write(IdentifierType, Identifier, Payload);
		}

		/// <summary>
		/// Writes a none-valued (or void-valued) TLV.
		/// </summary>
		/// <param name="IdentifierType">Type of identifier.</param>
		/// <param name="Identifier">Identifier.</param>
		public void Write(IdentifierType IdentifierType, ushort Identifier)
		{
			this.Write(IdentifierType, Identifier, new byte[0]);
		}

	}
}
