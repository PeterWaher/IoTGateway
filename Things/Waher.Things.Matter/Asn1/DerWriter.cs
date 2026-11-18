using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Waher.Things.Matter.Asn1
{
	/// <summary>
	/// Minimal ASN.1 DER encoder, used for X.509 certificates, CSRs and signatures.
	/// </summary>
	public class DerWriter
	{
		/// <summary>
		/// INTEGER tag.
		/// </summary>
		public const byte TagInteger = 0x02;

		/// <summary>
		/// BIT STRING tag.
		/// </summary>
		public const byte TagBitString = 0x03;

		/// <summary>
		/// OCTET STRING tag.
		/// </summary>
		public const byte TagOctetString = 0x04;

		/// <summary>
		/// NULL tag.
		/// </summary>
		public const byte TagNull = 0x05;

		/// <summary>
		/// OBJECT IDENTIFIER tag.
		/// </summary>
		public const byte TagOid = 0x06;

		/// <summary>
		/// UTF8String tag.
		/// </summary>
		public const byte TagUtf8String = 0x0c;

		/// <summary>
		/// PrintableString tag.
		/// </summary>
		public const byte TagPrintableString = 0x13;

		/// <summary>
		/// IA5String tag.
		/// </summary>
		public const byte TagIa5String = 0x16;

		/// <summary>
		/// UTCTime tag.
		/// </summary>
		public const byte TagUtcTime = 0x17;

		/// <summary>
		/// GeneralizedTime tag.
		/// </summary>
		public const byte TagGeneralizedTime = 0x18;

		/// <summary>
		/// BOOLEAN tag.
		/// </summary>
		public const byte TagBoolean = 0x01;

		/// <summary>
		/// SEQUENCE tag.
		/// </summary>
		public const byte TagSequence = 0x30;

		/// <summary>
		/// SET tag.
		/// </summary>
		public const byte TagSet = 0x31;

		private readonly Stack<KeyValuePair<byte, MemoryStream>> stack = new Stack<KeyValuePair<byte, MemoryStream>>();
		private MemoryStream current = new MemoryStream();

		/// <summary>
		/// Minimal ASN.1 DER encoder.
		/// </summary>
		public DerWriter()
		{
		}

		/// <summary>
		/// Returns the encoded output.
		/// </summary>
		/// <returns>DER encoding.</returns>
		public byte[] ToArray()
		{
			if (this.stack.Count > 0)
				throw new InvalidOperationException("Unclosed constructed elements.");

			return this.current.ToArray();
		}

		/// <summary>
		/// Writes a complete element.
		/// </summary>
		/// <param name="Tag">Tag octet.</param>
		/// <param name="Content">Content octets.</param>
		public void WriteElement(byte Tag, byte[] Content)
		{
			this.current.WriteByte(Tag);
			WriteLength(this.current, Content.Length);
			this.current.Write(Content, 0, Content.Length);
		}

		/// <summary>
		/// Writes raw, pre-encoded DER data.
		/// </summary>
		/// <param name="Encoded">Encoded data.</param>
		public void WriteRaw(byte[] Encoded)
		{
			this.current.Write(Encoded, 0, Encoded.Length);
		}

		private static void WriteLength(Stream Output, int Length)
		{
			if (Length < 128)
				Output.WriteByte((byte)Length);
			else if (Length < 256)
			{
				Output.WriteByte(0x81);
				Output.WriteByte((byte)Length);
			}
			else if (Length < 65536)
			{
				Output.WriteByte(0x82);
				Output.WriteByte((byte)(Length >> 8));
				Output.WriteByte((byte)Length);
			}
			else
			{
				Output.WriteByte(0x83);
				Output.WriteByte((byte)(Length >> 16));
				Output.WriteByte((byte)(Length >> 8));
				Output.WriteByte((byte)Length);
			}
		}

		/// <summary>
		/// Starts a constructed element.
		/// </summary>
		/// <param name="Tag">Tag octet.</param>
		public void StartConstructed(byte Tag)
		{
			this.stack.Push(new KeyValuePair<byte, MemoryStream>(Tag, this.current));
			this.current = new MemoryStream();
		}

		/// <summary>
		/// Ends the current constructed element.
		/// </summary>
		public void EndConstructed()
		{
			KeyValuePair<byte, MemoryStream> P = this.stack.Pop();
			byte[] Content = this.current.ToArray();
			this.current = P.Value;
			this.WriteElement(P.Key, Content);
		}

		/// <summary>
		/// Starts a SEQUENCE.
		/// </summary>
		public void StartSequence() => this.StartConstructed(TagSequence);

		/// <summary>
		/// Ends a SEQUENCE.
		/// </summary>
		public void EndSequence() => this.EndConstructed();

		/// <summary>
		/// Starts a SET.
		/// </summary>
		public void StartSet() => this.StartConstructed(TagSet);

		/// <summary>
		/// Ends a SET.
		/// </summary>
		public void EndSet() => this.EndConstructed();

		/// <summary>
		/// Starts a context-specific constructed element ([n] EXPLICIT or constructed IMPLICIT).
		/// </summary>
		/// <param name="Tag">Context tag number.</param>
		public void StartContextSpecific(int Tag) => this.StartConstructed((byte)(0xa0 | Tag));

		/// <summary>
		/// Ends a context-specific constructed element.
		/// </summary>
		public void EndContextSpecific() => this.EndConstructed();

		/// <summary>
		/// Writes a BOOLEAN.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WriteBoolean(bool Value)
		{
			this.WriteElement(TagBoolean, new byte[] { Value ? (byte)0xff : (byte)0x00 });
		}

		/// <summary>
		/// Writes a NULL.
		/// </summary>
		public void WriteNull()
		{
			this.WriteElement(TagNull, Array.Empty<byte>());
		}

		/// <summary>
		/// Writes an INTEGER.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WriteInteger(long Value)
		{
			List<byte> Bin = new List<byte>();

			do
			{
				Bin.Insert(0, (byte)Value);
				Value >>= 8;
			}
			while (!((Value == 0 && (Bin[0] & 0x80) == 0) || (Value == -1 && (Bin[0] & 0x80) != 0)));

			this.WriteElement(TagInteger, Bin.ToArray());
		}

		/// <summary>
		/// Writes a non-negative INTEGER given as big-endian unsigned bytes.
		/// </summary>
		/// <param name="BigEndian">Big-endian unsigned value.</param>
		public void WriteUnsignedInteger(byte[] BigEndian)
		{
			int i = 0;
			int c = BigEndian.Length;

			if (c == 0)
			{
				this.WriteElement(TagInteger, new byte[] { 0 });
				return;
			}

			while (i < c - 1 && BigEndian[i] == 0)
				i++;

			bool Pad = (BigEndian[i] & 0x80) != 0;
			byte[] Content = new byte[c - i + (Pad ? 1 : 0)];
			Array.Copy(BigEndian, i, Content, Pad ? 1 : 0, c - i);

			this.WriteElement(TagInteger, Content);
		}

		/// <summary>
		/// Writes an INTEGER, given its raw two's complement content octets.
		/// </summary>
		/// <param name="Content">Content octets.</param>
		public void WriteIntegerRaw(byte[] Content)
		{
			this.WriteElement(TagInteger, Content);
		}

		/// <summary>
		/// Writes an OBJECT IDENTIFIER.
		/// </summary>
		/// <param name="Oid">Dotted OID string.</param>
		public void WriteOid(string Oid)
		{
			this.WriteElement(TagOid, EncodeOid(Oid));
		}

		/// <summary>
		/// Encodes an OID into content octets.
		/// </summary>
		/// <param name="Oid">Dotted OID string.</param>
		/// <returns>Content octets.</returns>
		public static byte[] EncodeOid(string Oid)
		{
			string[] Parts = Oid.Split('.');
			if (Parts.Length < 2)
				throw new ArgumentException("Invalid OID.", nameof(Oid));

			MemoryStream ms = new MemoryStream();
			ulong First = ulong.Parse(Parts[0], CultureInfo.InvariantCulture);
			ulong Second = ulong.Parse(Parts[1], CultureInfo.InvariantCulture);

			WriteBase128(ms, First * 40 + Second);

			int i, c = Parts.Length;
			for (i = 2; i < c; i++)
				WriteBase128(ms, ulong.Parse(Parts[i], CultureInfo.InvariantCulture));

			return ms.ToArray();
		}

		private static void WriteBase128(Stream Output, ulong Value)
		{
			List<byte> Bin = new List<byte>()
			{
				(byte)(Value & 0x7f)
			};

			Value >>= 7;

			while (Value > 0)
			{
				Bin.Insert(0, (byte)(0x80 | (Value & 0x7f)));
				Value >>= 7;
			}

			foreach (byte b in Bin)
				Output.WriteByte(b);
		}

		/// <summary>
		/// Writes an OCTET STRING.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WriteOctetString(byte[] Value)
		{
			this.WriteElement(TagOctetString, Value);
		}

		/// <summary>
		/// Writes a BIT STRING.
		/// </summary>
		/// <param name="Value">Bits.</param>
		/// <param name="UnusedBits">Number of unused bits in the last octet.</param>
		public void WriteBitString(byte[] Value, int UnusedBits)
		{
			byte[] Content = new byte[Value.Length + 1];
			Content[0] = (byte)UnusedBits;
			Array.Copy(Value, 0, Content, 1, Value.Length);
			this.WriteElement(TagBitString, Content);
		}

		/// <summary>
		/// Writes a named-bit BIT STRING (e.g. KeyUsage), where bit 0 is the most significant bit of the first octet.
		/// Trailing zero bits are removed, as required by DER.
		/// </summary>
		/// <param name="Bits">Bit mask, where bit i corresponds to named bit i.</param>
		public void WriteNamedBitString(uint Bits)
		{
			if (Bits == 0)
			{
				this.WriteBitString(Array.Empty<byte>(), 0);
				return;
			}

			int HighestBit = 31;
			while (((Bits >> HighestBit) & 1) == 0)
				HighestBit--;

			int NrBytes = HighestBit / 8 + 1;
			byte[] Bin = new byte[NrBytes];
			int i;

			for (i = 0; i <= HighestBit; i++)
			{
				if (((Bits >> i) & 1) != 0)
					Bin[i / 8] |= (byte)(0x80 >> (i % 8));
			}

			int Unused = 7 - (HighestBit % 8);
			this.WriteBitString(Bin, Unused);
		}

		/// <summary>
		/// Writes a UTF8String.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WriteUtf8String(string Value)
		{
			this.WriteElement(TagUtf8String, Encoding.UTF8.GetBytes(Value));
		}

		/// <summary>
		/// Writes a PrintableString.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WritePrintableString(string Value)
		{
			this.WriteElement(TagPrintableString, Encoding.ASCII.GetBytes(Value));
		}

		/// <summary>
		/// Writes an IA5String.
		/// </summary>
		/// <param name="Value">Value.</param>
		public void WriteIa5String(string Value)
		{
			this.WriteElement(TagIa5String, Encoding.ASCII.GetBytes(Value));
		}

		/// <summary>
		/// Writes a time value, as UTCTime if the year is before 2050, or GeneralizedTime otherwise.
		/// </summary>
		/// <param name="Value">Time (UTC).</param>
		public void WriteTime(DateTime Value)
		{
			Value = Value.ToUniversalTime();

			if (Value.Year < 2050 && Value.Year >= 1950)
				this.WriteElement(TagUtcTime, Encoding.ASCII.GetBytes(Value.ToString("yyMMddHHmmss", CultureInfo.InvariantCulture) + "Z"));
			else
				this.WriteElement(TagGeneralizedTime, Encoding.ASCII.GetBytes(Value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "Z"));
		}
	}
}
