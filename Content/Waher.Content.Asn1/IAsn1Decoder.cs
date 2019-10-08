using System;
using System.Collections.Generic;
using System.IO;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Interface for ASN.1 decoders
	/// </summary>
	public interface IAsn1Decoder
	{
		/// <summary>
		/// Decodes an identifier from the stream.
		/// </summary>
		/// <param name="Constructed">If the identifier is constructed (true) or primitive (false)</param>
		/// <param name="Class">Class of tag.</param>
		/// <returns>TAG</returns>
		long DecodeIdentifier(out bool Constructed, out TagClass Class);

		/// <summary>
		/// Decodes the length of a contents section.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Length of contents (if definite), or -1 (if indefinite, and determined by an end-of-contents section).</returns>
		long DecodeLength(Stream Input);

		/// <summary>
		/// Decodes a BOOLEAN value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BOOLEAN value</returns>
		bool DecodeBOOLEAN(Stream Input);

		/// <summary>
		/// Decodes an INTEGER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>INTEGER value</returns>
		long DecodeINTEGER(Stream Input);

		/// <summary>
		/// Decodes an enumerated value.
		/// </summary>
		/// <typeparam name="T">Enumeration type.</typeparam>
		/// <param name="Input">Input stream.</param>
		/// <returns>Enumeration value</returns>
		Enum DecodeEnum<T>(Stream Input)
			where T : Enum;

		/// <summary>
		/// Decodes a REAL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>REAL value</returns>
		double DecodeREAL(Stream Input);

		/// <summary>
		/// Decodes a BIT STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <param name="NrUnusedBits">Number of unused bits.</param>
		/// <returns>BIT STRING value</returns>
		byte[] DecodeBitString(Stream Input, out int NrUnusedBits);

		/// <summary>
		/// Decodes a OCTET STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OCTET STRING value</returns>
		byte[] DecodeOctetString(Stream Input);

		/// <summary>
		/// Decodes a NULL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NULL value</returns>
		void DecodeNull(Stream Input);

		/// <summary>
		/// Decodes an OBJECT IDENTIFIER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OBJECT IDENTIFIER value</returns>
		int[] DecodeObjectId(Stream Input);

		/// <summary>
		/// Decodes a RELATIVE-OID value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>RELATIVE-OID value</returns>
		int[] DecodeRelativeObjectId(Stream Input);

		/// <summary>
		/// Decodes a BmpString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BmpString value</returns>
		string DecodeBmpString(Stream Input);

		/// <summary>
		/// Decodes a IA5String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>IA5String value</returns>
		string DecodeIa5String(Stream Input);

		/// <summary>
		/// Decodes a VisibleString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>VisibleString value</returns>
		string DecodeVisibleString(Stream Input);

		/// <summary>
		/// Decodes a Utf8String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Utf8String value</returns>
		string DecodeUtf8String(Stream Input);

		/// <summary>
		/// Decodes a UniversalString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>UniversalString value</returns>
		string DecodeUniversalString(Stream Input);

		/// <summary>
		/// Decodes a PrintableString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>PrintableString value</returns>
		string DecodePrintableString(Stream Input);

		/// <summary>
		/// Decodes a NumericString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NumericString value</returns>
		string DecodeNumericString(Stream Input);

		/// <summary>
		/// Decodes a TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME value</returns>
		TimeSpan DecodeTime(Stream Input);

		/// <summary>
		/// Decodes a DATE value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE value</returns>
		DateTime DecodeDate(Stream Input);

		/// <summary>
		/// Decodes a TIME-OF-DAY value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME-OF-DAY value</returns>
		TimeSpan DecodeTimeOfDay(Stream Input);

		/// <summary>
		/// Decodes a DATE-TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE-TIME value</returns>
		DateTime DecodeDateTime(Stream Input);

		/// <summary>
		/// Decodes a DURATION value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DURATION value</returns>
		Duration DecodeDuration(Stream Input);
	}
}
