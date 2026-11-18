using System;
using System.Globalization;
using System.Text;

namespace Waher.Things.Matter.Asn1
{
	/// <summary>
	/// Minimal ASN.1 DER decoder, used for X.509 certificates, CSRs, CMS structures and signatures.
	/// </summary>
	public class DerReader
	{
		private readonly byte[] data;
		private readonly int end;
		private int pos;

		/// <summary>
		/// Minimal ASN.1 DER decoder.
		/// </summary>
		/// <param name="Data">DER encoded data.</param>
		public DerReader(byte[] Data)
			: this(Data, 0, Data.Length)
		{
		}

		/// <summary>
		/// Minimal ASN.1 DER decoder.
		/// </summary>
		/// <param name="Data">DER encoded data.</param>
		/// <param name="Offset">Start offset.</param>
		/// <param name="Count">Number of bytes.</param>
		public DerReader(byte[] Data, int Offset, int Count)
		{
			this.data = Data;
			this.pos = Offset;
			this.end = Offset + Count;

			if (this.end > Data.Length)
				throw new MatterDecodingException("Invalid DER buffer.");
		}

		/// <summary>
		/// If more elements are available.
		/// </summary>
		public bool HasMore => this.pos < this.end;

		/// <summary>
		/// Current position in the buffer.
		/// </summary>
		public int Position => this.pos;

		/// <summary>
		/// Underlying buffer.
		/// </summary>
		public byte[] Buffer => this.data;

		/// <summary>
		/// Peeks at the tag of the next element.
		/// </summary>
		/// <returns>Tag octet.</returns>
		public byte PeekTag()
		{
			if (this.pos >= this.end)
				throw new MatterDecodingException("Unexpected end of DER data.");

			return this.data[this.pos];
		}

		/// <summary>
		/// Reads the next element header and returns its tag, content offset and length.
		/// </summary>
		/// <param name="Tag">Tag octet.</param>
		/// <param name="ContentOffset">Offset of content.</param>
		/// <param name="ContentLength">Length of content.</param>
		/// <param name="ElementOffset">Offset of the start of the element (tag).</param>
		public void ReadHeader(out byte Tag, out int ContentOffset, out int ContentLength, out int ElementOffset)
		{
			if (this.pos >= this.end)
				throw new MatterDecodingException("Unexpected end of DER data.");

			ElementOffset = this.pos;
			Tag = this.data[this.pos++];

			if ((Tag & 0x1f) == 0x1f)
				throw new MatterDecodingException("Multi-octet DER tags not supported.");

			if (this.pos >= this.end)
				throw new MatterDecodingException("Unexpected end of DER data.");

			int Len = this.data[this.pos++];

			if ((Len & 0x80) != 0)
			{
				int NrBytes = Len & 0x7f;
				if (NrBytes == 0 || NrBytes > 4)
					throw new MatterDecodingException("Unsupported DER length encoding.");

				Len = 0;
				while (NrBytes-- > 0)
				{
					if (this.pos >= this.end)
						throw new MatterDecodingException("Unexpected end of DER data.");

					Len = (Len << 8) | this.data[this.pos++];
				}
			}

			if (Len < 0 || this.pos + Len > this.end)
				throw new MatterDecodingException("DER element length exceeds buffer.");

			ContentOffset = this.pos;
			ContentLength = Len;
			this.pos += Len;
		}

		/// <summary>
		/// Reads the next element.
		/// </summary>
		/// <param name="Tag">Tag octet.</param>
		/// <returns>Content octets.</returns>
		public byte[] ReadElement(out byte Tag)
		{
			this.ReadHeader(out Tag, out int Offset, out int Length, out _);
			byte[] Result = new byte[Length];
			Array.Copy(this.data, Offset, Result, 0, Length);
			return Result;
		}

		/// <summary>
		/// Reads the next element, and returns its complete encoding (including tag and length).
		/// </summary>
		/// <returns>Encoded element.</returns>
		public byte[] ReadEncodedElement()
		{
			this.ReadHeader(out _, out int Offset, out int Length, out int ElementOffset);
			int c = Offset + Length - ElementOffset;
			byte[] Result = new byte[c];
			Array.Copy(this.data, ElementOffset, Result, 0, c);
			return Result;
		}

		/// <summary>
		/// Reads the next element, expecting a given tag.
		/// </summary>
		/// <param name="ExpectedTag">Expected tag.</param>
		/// <returns>Content octets.</returns>
		public byte[] ReadElement(byte ExpectedTag)
		{
			byte[] Result = this.ReadElement(out byte Tag);
			if (Tag != ExpectedTag)
				throw new MatterDecodingException("Unexpected DER tag: 0x" + Tag.ToString("X2") + ", expected 0x" + ExpectedTag.ToString("X2"));
			return Result;
		}

		/// <summary>
		/// Reads a constructed element, returning a reader for its content.
		/// </summary>
		/// <param name="ExpectedTag">Expected tag.</param>
		/// <returns>Reader for content.</returns>
		public DerReader ReadConstructed(byte ExpectedTag)
		{
			this.ReadHeader(out byte Tag, out int Offset, out int Length, out _);
			if (Tag != ExpectedTag)
				throw new MatterDecodingException("Unexpected DER tag: 0x" + Tag.ToString("X2") + ", expected 0x" + ExpectedTag.ToString("X2"));

			return new DerReader(this.data, Offset, Length);
		}

		/// <summary>
		/// Reads a SEQUENCE.
		/// </summary>
		/// <returns>Reader for content.</returns>
		public DerReader ReadSequence() => this.ReadConstructed(DerWriter.TagSequence);

		/// <summary>
		/// Reads a SET.
		/// </summary>
		/// <returns>Reader for content.</returns>
		public DerReader ReadSet() => this.ReadConstructed(DerWriter.TagSet);

		/// <summary>
		/// If the next element is a context-specific element with the given tag number.
		/// </summary>
		/// <param name="Tag">Context tag number.</param>
		/// <returns>If next element matches.</returns>
		public bool IsContextSpecific(int Tag)
		{
			return this.HasMore && (this.data[this.pos] & 0xdf) == (0x80 | Tag);
		}

		/// <summary>
		/// Reads a constructed context-specific element.
		/// </summary>
		/// <param name="Tag">Context tag number.</param>
		/// <returns>Reader for content.</returns>
		public DerReader ReadContextSpecific(int Tag) => this.ReadConstructed((byte)(0xa0 | Tag));

		/// <summary>
		/// Reads an INTEGER, as raw content octets (two's complement, big-endian).
		/// </summary>
		/// <returns>Content octets.</returns>
		public byte[] ReadIntegerBytes()
		{
			return this.ReadElement(DerWriter.TagInteger);
		}

		/// <summary>
		/// Reads a small INTEGER.
		/// </summary>
		/// <returns>Integer value.</returns>
		public long ReadInteger()
		{
			byte[] Bin = this.ReadIntegerBytes();
			if (Bin.Length == 0 || Bin.Length > 8)
				throw new MatterDecodingException("Integer out of range.");

			long Result = (sbyte)Bin[0];
			int i;

			for (i = 1; i < Bin.Length; i++)
				Result = (Result << 8) | Bin[i];

			return Result;
		}

		/// <summary>
		/// Reads a BOOLEAN.
		/// </summary>
		/// <returns>Boolean value.</returns>
		public bool ReadBoolean()
		{
			byte[] Bin = this.ReadElement(DerWriter.TagBoolean);
			return Bin.Length > 0 && Bin[0] != 0;
		}

		/// <summary>
		/// Reads an OBJECT IDENTIFIER.
		/// </summary>
		/// <returns>Dotted OID string.</returns>
		public string ReadOid()
		{
			return DecodeOid(this.ReadElement(DerWriter.TagOid));
		}

		/// <summary>
		/// Decodes OID content octets.
		/// </summary>
		/// <param name="Bin">Content octets.</param>
		/// <returns>Dotted OID string.</returns>
		public static string DecodeOid(byte[] Bin)
		{
			StringBuilder sb = new StringBuilder();
			ulong Value = 0;
			bool First = true;

			foreach (byte b in Bin)
			{
				Value = (Value << 7) | (uint)(b & 0x7f);

				if ((b & 0x80) == 0)
				{
					if (First)
					{
						First = false;

						if (Value < 40)
							sb.Append("0.").Append(Value.ToString(CultureInfo.InvariantCulture));
						else if (Value < 80)
							sb.Append("1.").Append((Value - 40).ToString(CultureInfo.InvariantCulture));
						else
							sb.Append("2.").Append((Value - 80).ToString(CultureInfo.InvariantCulture));
					}
					else
						sb.Append('.').Append(Value.ToString(CultureInfo.InvariantCulture));

					Value = 0;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Reads an OCTET STRING.
		/// </summary>
		/// <returns>Octets.</returns>
		public byte[] ReadOctetString()
		{
			return this.ReadElement(DerWriter.TagOctetString);
		}

		/// <summary>
		/// Reads a BIT STRING.
		/// </summary>
		/// <param name="UnusedBits">Number of unused bits.</param>
		/// <returns>Bits.</returns>
		public byte[] ReadBitString(out int UnusedBits)
		{
			byte[] Bin = this.ReadElement(DerWriter.TagBitString);
			if (Bin.Length == 0)
				throw new MatterDecodingException("Invalid BIT STRING.");

			UnusedBits = Bin[0];
			byte[] Result = new byte[Bin.Length - 1];
			Array.Copy(Bin, 1, Result, 0, Result.Length);
			return Result;
		}

		/// <summary>
		/// Reads a string of any of the supported string types.
		/// </summary>
		/// <param name="Tag">Tag of string.</param>
		/// <returns>String.</returns>
		public string ReadString(out byte Tag)
		{
			byte[] Bin = this.ReadElement(out Tag);

			switch (Tag)
			{
				case DerWriter.TagUtf8String:
					return Encoding.UTF8.GetString(Bin);

				case DerWriter.TagPrintableString:
				case DerWriter.TagIa5String:
				case 0x14:  // TeletexString
				case 0x1a:  // VisibleString
					return Encoding.ASCII.GetString(Bin);

				case 0x1e:  // BMPString
					return Encoding.BigEndianUnicode.GetString(Bin);

				default:
					throw new MatterDecodingException("Unsupported string type: 0x" + Tag.ToString("X2"));
			}
		}

		/// <summary>
		/// Reads a UTCTime or GeneralizedTime.
		/// </summary>
		/// <returns>Time (UTC).</returns>
		public DateTime ReadTime()
		{
			byte[] Bin = this.ReadElement(out byte Tag);
			string s = Encoding.ASCII.GetString(Bin);

			if (Tag == DerWriter.TagUtcTime)
			{
				if (!DateTime.TryParseExact(s, "yyMMddHHmmss'Z'", CultureInfo.InvariantCulture,
					DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime TP))
				{
					throw new MatterDecodingException("Invalid UTCTime: " + s);
				}

				if (TP.Year >= 2050)
					TP = TP.AddYears(-100);

				return TP;
			}
			else if (Tag == DerWriter.TagGeneralizedTime)
			{
				if (!DateTime.TryParseExact(s, "yyyyMMddHHmmss'Z'", CultureInfo.InvariantCulture,
					DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out DateTime TP))
				{
					throw new MatterDecodingException("Invalid GeneralizedTime: " + s);
				}

				return TP;
			}
			else
				throw new MatterDecodingException("Expected time value.");
		}

		/// <summary>
		/// Skips the next element.
		/// </summary>
		public void Skip()
		{
			this.ReadHeader(out _, out _, out _, out _);
		}
	}
}
