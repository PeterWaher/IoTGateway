using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Basic Encoding Rules (BER), as defined in X.690
	/// </summary>
	public class Asn1DecoderBer : IAsn1Decoder
	{
		private readonly Stream input;

		/// <summary>
		/// Basic Encoding Rules (BER), as defined in X.690
		/// </summary>
		/// <param name="Input">Input stream.</param>
		public Asn1DecoderBer(Stream Input)
		{
			this.input = Input;
		}

		/// <summary>
		/// Decodes an identifier from the stream.
		/// </summary>
		/// <param name="Constructed">If the identifier is constructed (true) or primitive (false)</param>
		/// <param name="Class">Class of tag.</param>
		/// <returns>TAG</returns>
		public long DecodeIdentifier(out bool Constructed, out TagClass Class)
		{
			return BER.DecodeIdentifier(this.input, out Constructed, out Class);
		}

		/// <summary>
		/// Decodes the length of a contents section.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Length of contents (if definite), or -1 (if indefinite, and determined by an end-of-contents section).</returns>
		public long DecodeLength(Stream Input)
		{
			return BER.DecodeLength(this.input);
		}

		/// <summary>
		/// Decodes a BOOLEAN value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BOOLEAN value</returns>
		public bool DecodeBOOLEAN(Stream Input)
		{
			return BER.DecodeBOOLEAN(this.input);
		}

		/// <summary>
		/// Decodes an INTEGER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>INTEGER value</returns>
		public long DecodeINTEGER(Stream Input)
		{
			return BER.DecodeINTEGER(this.input);
		}

		/// <summary>
		/// Decodes an enumerated value.
		/// </summary>
		/// <typeparam name="T">Enumeration type.</typeparam>
		/// <param name="Input">Input stream.</param>
		/// <returns>Enumeration value</returns>
		public Enum DecodeEnum<T>(Stream Input)
			where T : Enum
		{
			return BER.DecodeEnum<T>(this.input);
		}

		/// <summary>
		/// Decodes a REAL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>REAL value</returns>
		public double DecodeREAL(Stream Input)
		{
			return BER.DecodeREAL(this.input);
		}

		/// <summary>
		/// Decodes a BIT STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <param name="NrUnusedBits">Number of unused bits.</param>
		/// <returns>BIT STRING value</returns>
		public byte[] DecodeBitString(Stream Input, out int NrUnusedBits)
		{
			return BER.DecodeBitString(this.input, out NrUnusedBits);
		}

		/// <summary>
		/// Decodes a OCTET STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OCTET STRING value</returns>
		public byte[] DecodeOctetString(Stream Input)
		{
			return BER.DecodeOctetString(this.input);
		}

		/// <summary>
		/// Decodes a NULL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NULL value</returns>
		public void DecodeNull(Stream Input)
		{
			BER.DecodeNull(this.input);
		}

		/// <summary>
		/// Decodes an OBJECT IDENTIFIER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OBJECT IDENTIFIER value</returns>
		public int[] DecodeObjectId(Stream Input)
		{
			return BER.DecodeObjectId(this.input);
		}


		/// <summary>
		/// Decodes a RELATIVE-OID value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>RELATIVE-OID value</returns>
		public int[] DecodeRelativeObjectId(Stream Input)
		{
			return BER.DecodeRelativeObjectId(this.input);
		}

		/// <summary>
		/// Decodes a BmpString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BmpString value</returns>
		public string DecodeBmpString(Stream Input)
		{
			return BER.DecodeBmpString(this.input);
		}

		/// <summary>
		/// Decodes a IA5String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>IA5String value</returns>
		public string DecodeIa5String(Stream Input)
		{
			return BER.DecodeIa5String(this.input);
		}

		/// <summary>
		/// Decodes a VisibleString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>VisibleString value</returns>
		public string DecodeVisibleString(Stream Input)
		{
			return BER.DecodeVisibleString(this.input);
		}

		/// <summary>
		/// Decodes a Utf8String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Utf8String value</returns>
		public string DecodeUtf8String(Stream Input)
		{
			return BER.DecodeUtf8String(this.input);
		}

		/// <summary>
		/// Decodes a UniversalString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>UniversalString value</returns>
		public string DecodeUniversalString(Stream Input)
		{
			return BER.DecodeUniversalString(this.input);
		}

		/// <summary>
		/// Decodes a PrintableString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>PrintableString value</returns>
		public string DecodePrintableString(Stream Input)
		{
			return BER.DecodePrintableString(this.input);
		}

		/// <summary>
		/// Decodes a NumericString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NumericString value</returns>
		public string DecodeNumericString(Stream Input)
		{
			return BER.DecodeNumericString(this.input);
		}

		/// <summary>
		/// Decodes a TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME value</returns>
		public TimeSpan DecodeTime(Stream Input)
		{
			return BER.DecodeTime(this.input);
		}

		/// <summary>
		/// Decodes a DATE value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE value</returns>
		public DateTime DecodeDate(Stream Input)
		{
			return BER.DecodeDate(this.input);
		}

		/// <summary>
		/// Decodes a TIME-OF-DAY value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME-OF-DAY value</returns>
		public TimeSpan DecodeTimeOfDay(Stream Input)
		{
			return BER.DecodeTimeOfDay(this.input);
		}

		/// <summary>
		/// Decodes a DATE-TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE-TIME value</returns>
		public DateTime DecodeDateTime(Stream Input)
		{
			return BER.DecodeDateTime(this.input);
		}

		/// <summary>
		/// Decodes a DURATION value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DURATION value</returns>
		public Duration DecodeDuration(Stream Input)
		{
			return BER.DecodeDuration(this.input);
		}

	}
}
