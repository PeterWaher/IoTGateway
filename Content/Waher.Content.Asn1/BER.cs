using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content.Xml;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Implements static methods for Basic Encoding Rules (BER), as defined in X.690
	/// </summary>
	public static class BER
	{
		/// <summary>
		/// Decodes an identifier from the stream.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <param name="Constructed">If the identifier is constructed (true) or primitive (false)</param>
		/// <param name="Class">Class of tag.</param>
		/// <returns>TAG</returns>
		public static long DecodeIdentifier(Stream Input, out bool Constructed, out TagClass Class)
		{
			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			Class = (TagClass)((i >> 6) & 3);
			Constructed = (i & 32) != 0;

			i &= 31;
			if (i < 31)
				return i;

			return DecodeVarLenInt(Input);
		}

		private static long DecodeVarLenInt(Stream Input)
		{
			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			long Result = (uint)(i & 127);
			while ((i & 128) > 0)
			{
				i = Input.ReadByte();
				if (i < 0)
					throw new EndOfStreamException();

				Result <<= 7;
				Result |= (uint)(i & 127);
			}

			return Result;
		}

		/// <summary>
		/// Decodes the length of a contents section.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Length of contents (if definite), or -1 (if indefinite, and determined by an end-of-contents section).</returns>
		public static long DecodeLength(Stream Input)
		{
			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			if ((i & 128) != 0)
			{
				if (i == 128)
					return -1;          // Indefinite form
				else
					return (i & 127);   // Short form
			}

			if (i > 8)
				throw new NotSupportedException("Too long.");

			return ReadInteger(Input, i, false);
		}

		private static long ReadInteger(Stream Input, int NrBytes, bool Signed)
		{
			long Result = 0;
			int i;
			bool First = true;

			while (NrBytes-- > 0)
			{
				i = Input.ReadByte();
				if (i < 0)
					throw new EndOfStreamException();

				if (First)
				{
					First = false;
					if (Signed && (i & 128) != 0)
						Result = -1;
				}

				Result <<= 8;
				Result |= (byte)i;
			}

			return Result;
		}

		/// <summary>
		/// Decodes a BOOLEAN value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BOOLEAN value</returns>
		public static bool DecodeBOOLEAN(Stream Input)
		{
			if (DecodeLength(Input) != 1)
				throw new InvalidOperationException("Invalid BOOLEAN value.");

			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			return i != 0;
		}

		/// <summary>
		/// Decodes an INTEGER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>INTEGER value</returns>
		public static long DecodeINTEGER(Stream Input)
		{
			long Len = DecodeLength(Input);
			if (Len < 0)
				throw new NotSupportedException("Indefinite length not supported.");

			if (Len > 8)
				throw new NotSupportedException("INTEGER too large.");

			if (Len == 0)
				return 0;

			return ReadInteger(Input, (int)Len, true);
		}

		/// <summary>
		/// Decodes an enumerated value.
		/// </summary>
		/// <typeparam name="T">Enumeration type.</typeparam>
		/// <param name="Input">Input stream.</param>
		/// <returns>Enumeration value</returns>
		public static Enum DecodeEnum<T>(Stream Input)
			where T : Enum
		{
			long i = DecodeINTEGER(Input);
			return (T)((object)i);
		}

		/// <summary>
		/// Decodes a REAL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>REAL value</returns>
		public static double DecodeREAL(Stream Input)
		{
			long Len = DecodeLength(Input);
			if (Len == 0)
				return 0;

			if (Len < 0)
				throw new NotSupportedException("Indefinite length not supported.");

			if (Len > 8)
				throw new NotSupportedException("REAL size not supported.");

			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException();

			int c = (int)(Len - 1);

			if ((i & 128) != 0)         // Binary encoding 
			{
				bool Negative = (i & 64) != 0;
				byte F = (byte)((i >> 2) & 3);
				long Exponent;

				switch (i & 3)
				{
					case 0:
						c--;
						Exponent = ReadInteger(Input, 1, true);
						break;

					case 1:
						c -= 2;
						Exponent = ReadInteger(Input, 2, true);
						break;

					case 2:
						c -= 3;
						Exponent = ReadInteger(Input, 3, true);
						break;

					case 3:
					default:
						int j = (int)ReadInteger(Input, 1, false);
						c -= j;
						Exponent = ReadInteger(Input, j, true);
						break;
				}

				if (c < 0)
					throw new InvalidOperationException("Invalid REAL.");

				double Mantissa = ReadInteger(Input, c, false);

				Mantissa *= Math.Pow(2.0, F);
				if (Negative)
					Mantissa = -Mantissa;

				if (Exponent == 0)
					return Mantissa;

				switch ((i >> 4) & 3)
				{
					case 0:     // Base 2
						return Mantissa * Math.Pow(2, Exponent);

					case 1:     // Base 8
						return Mantissa * Math.Pow(8, Exponent);

					case 2:     // Base 16
						return Mantissa * Math.Pow(16, Exponent);

					case 3:     // Reserved
					default:
						throw new NotSupportedException("Reserved base.");
				}
			}
			else if ((i & 64) == 0)     // Decimal encoding
			{
				byte[] Bin = new byte[c];
				if (Input.Read(Bin, 0, c) != c)
					throw new EndOfStreamException();

				string s = CommonTypes.GetString(Bin, Encoding.ASCII).Trim();

				if (!CommonTypes.TryParse(s, out double d))
					throw new NotSupportedException("Unsupported REAL value.");

				return d;
			}
			else                        // Special value
			{
				if (c != 1)
					throw new NotSupportedException("Invalid special value");

				i = Input.ReadByte();
				if (i < 0)
					throw new EndOfStreamException();

				switch (i)
				{
					case 64: return double.PositiveInfinity;
					case 65: return double.NegativeInfinity;
					case 66: return double.NaN;
					case 67: return 0;
					default: throw new NotSupportedException("Invalid special value");
				}
			}
		}

		/// <summary>
		/// Decodes a BIT STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <param name="NrUnusedBits">Number of unused bits.</param>
		/// <returns>BIT STRING value</returns>
		public static byte[] DecodeBitString(Stream Input, out int NrUnusedBits)
		{
			long Len = DecodeLength(Input);
			if (Len == 0)
				throw new InvalidOperationException("Invalid BIT STRING.");

			if (Len < 0)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					NrUnusedBits = 0;
					while ((Len = DecodeIdentifier(Input, out bool _, out TagClass _)) != 0)
					{
						if (Len != 3)
							throw new NotSupportedException("Expected BIT STRING.");

						byte[] Bin = DecodeBitString(Input, out NrUnusedBits);
						ms.Write(Bin, 0, Bin.Length);
					}

					if (DecodeLength(Input) != 0)
						throw new InvalidOperationException("Expected zero length.");

					return ms.ToArray();
				}
			}
			else
			{
				NrUnusedBits = Input.ReadByte();
				if (NrUnusedBits < 0)
					throw new EndOfStreamException();

				if (--Len > int.MaxValue)
					throw new NotSupportedException("BIT STRING too large.");

				int c = (int)Len;
				byte[] Bin = new byte[c];
				int i = Input.Read(Bin, 0, c);

				if (i != c)
					throw new EndOfStreamException();

				return Bin;
			}
		}

		/// <summary>
		/// Decodes a OCTET STRING value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OCTET STRING value</returns>
		public static byte[] DecodeOctetString(Stream Input)
		{
			long Len = DecodeLength(Input);
			if (Len == 0)
				return new byte[0];

			if (Len < 0)
			{
				using (MemoryStream ms = new MemoryStream())
				{
					while ((Len = DecodeIdentifier(Input, out bool _, out TagClass _)) != 0)
					{
						if (Len != 4)
							throw new NotSupportedException("Expected OCTET STRING.");

						byte[] Bin = DecodeOctetString(Input);
						ms.Write(Bin, 0, Bin.Length);
					}

					if (DecodeLength(Input) != 0)
						throw new InvalidOperationException("Expected zero length.");

					return ms.ToArray();
				}
			}
			else
			{
				if (Len > int.MaxValue)
					throw new NotSupportedException("OCTET STRING too large.");

				int c = (int)Len;
				byte[] Bin = new byte[c];
				int i = Input.Read(Bin, 0, c);

				if (i != c)
					throw new EndOfStreamException();

				return Bin;
			}
		}

		/// <summary>
		/// Decodes a NULL value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NULL value</returns>
		public static void DecodeNull(Stream Input)
		{
			if (DecodeLength(Input) != 0)
				throw new InvalidOperationException("Expected zero length.");
		}

		/// <summary>
		/// Decodes an OBJECT IDENTIFIER value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>OBJECT IDENTIFIER value</returns>
		public static int[] DecodeObjectId(Stream Input)
		{
			long Len = DecodeLength(Input);
			if (Len == 0)
				return new int[0];

			if (Len < 0)
				throw new NotSupportedException("Indefinite length not supported.");

			List<int> Result = new List<int>();
			long EndPos = Input.Position + Len;

			Len = DecodeVarLenInt(Input);
			Result.Add((int)(Len % 40));

			Len /= 40;
			if (Len > int.MaxValue)
				throw new NotSupportedException("Invalid OBJECT IDENTIFIER");

			Result.Add((int)Len);

			while (Input.Position < EndPos)
			{
				Len = DecodeVarLenInt(Input);
				if (Len > int.MaxValue)
					throw new NotSupportedException("Invalid OBJECT IDENTIFIER");

				Result.Add((int)Len);
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Decodes a RELATIVE-OID value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>RELATIVE-OID value</returns>
		public static int[] DecodeRelativeObjectId(Stream Input)
		{
			return DecodeObjectId(Input);
		}

		/// <summary>
		/// Decodes a BmpString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>BmpString value</returns>
		public static string DecodeBmpString(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.BigEndianUnicode);
		}

		/// <summary>
		/// Decodes a IA5String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>IA5String value</returns>
		public static string DecodeIa5String(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.ASCII);
		}

		/// <summary>
		/// Decodes a VisibleString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>VisibleString value</returns>
		public static string DecodeVisibleString(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.ASCII);
		}

		/// <summary>
		/// Decodes a Utf8String value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>Utf8String value</returns>
		public static string DecodeUtf8String(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8);
		}

		/// <summary>
		/// Decodes a UniversalString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>UniversalString value</returns>
		public static string DecodeUniversalString(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF32);
		}

		/// <summary>
		/// Decodes a PrintableString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>PrintableString value</returns>
		public static string DecodePrintableString(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.ASCII);
		}

		/// <summary>
		/// Decodes a NumericString value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>NumericString value</returns>
		public static string DecodeNumericString(Stream Input)
		{
			return CommonTypes.GetString(DecodeOctetString(Input), Encoding.ASCII);
		}

		/// <summary>
		/// Decodes a TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME value</returns>
		public static TimeSpan DecodeTime(Stream Input)
		{
			return TimeSpan.Parse(CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8));
		}

		/// <summary>
		/// Decodes a DATE value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE value</returns>
		public static DateTime DecodeDate(Stream Input)
		{
			if (!XML.TryParse(CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8), out DateTime TP))
				throw new NotSupportedException("Unsupported DATE format.");

			return TP;
		}

		/// <summary>
		/// Decodes a TIME-OF-DAY value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>TIME-OF-DAY value</returns>
		public static TimeSpan DecodeTimeOfDay(Stream Input)
		{
			return TimeSpan.Parse(CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8));
		}

		/// <summary>
		/// Decodes a DATE-TIME value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DATE-TIME value</returns>
		public static DateTime DecodeDateTime(Stream Input)
		{
			if (!XML.TryParse(CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8), out DateTime TP))
				throw new NotSupportedException("Unsupported DATE-TIME format.");

			return TP;
		}

		/// <summary>
		/// Decodes a DURATION value.
		/// </summary>
		/// <param name="Input">Input stream.</param>
		/// <returns>DURATION value</returns>
		public static Duration DecodeDuration(Stream Input)
		{
			if (!Duration.TryParse(CommonTypes.GetString(DecodeOctetString(Input), Encoding.UTF8), out Duration D))
				throw new NotSupportedException("Unsupported DURATION format.");

			return D;
		}

	}
}
