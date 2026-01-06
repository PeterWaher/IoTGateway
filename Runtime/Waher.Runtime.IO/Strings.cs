using System.Text;
using System;

namespace Waher.Runtime.IO
{
	/// <summary>
	/// Static class managing binary representations of strings.
	/// </summary>
	public static class Strings
	{
		/// <summary>
		/// Gets the encoding of a string from a its binary representation, taking
		/// any Byte Order Mark (BOM) into account.
		/// 
		/// If no BOM is found, the default encoding in <paramref name="DefaultEncoding"/>
		/// is used, if defined. If not, ISO-8859-1 is used
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="DefaultEncoding">Default encoding to use, in case
		/// a Byte Order Mark (BOM) is not found in the binary representation.</param>
		/// <param name="BomLength">Length of Byte Order Mark (BOM).</param>
		/// <returns>Encoding</returns>
		public static Encoding GetEncoding(byte[] Data, Encoding DefaultEncoding, out int BomLength)
		{
			return GetEncoding(Data, 0, Data.Length, DefaultEncoding, out BomLength);
		}

		/// <summary>
		/// Gets the encoding of a string from a its binary representation, taking
		/// any Byte Order Mark (BOM) into account.
		/// 
		/// If no BOM is found, the default encoding in <paramref name="DefaultEncoding"/>
		/// is used, if defined. If not, ISO-8859-1 is used
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="Offset">Offset into array where string is encoded.</param>
		/// <param name="Count">Number of bytes of encoded string.</param>
		/// <param name="DefaultEncoding">Default encoding to use, in case
		/// a Byte Order Mark (BOM) is not found in the binary representation.</param>
		/// <param name="BomLength">Length of Byte Order Mark (BOM).</param>
		/// <returns>Encoding</returns>
		public static Encoding GetEncoding(byte[] Data, int Offset, int Count, Encoding DefaultEncoding, out int BomLength)
		{
			BomLength = 0;

			if (Data is null)
				return DefaultEncoding ?? ISO_8859_1;

			if (Count == 0)
				return DefaultEncoding ?? ISO_8859_1;

			/************************************************************
			 * See: https://en.wikipedia.org/wiki/Byte_order_mark
			 * 
			 * Script to extract known BOMs from the system:
			 * 
			 * foreach x in 0..65535 do 
			 *	if exists((Encoding:=System.Text.Encoding.GetEncoding(x);BOM:=Encoding.GetPreamble())) and BOM.Length>0 then 
			 *		print([x,Encoding.WebName,Encoding.EncodingName,BOM])
			 *		
			 * Script to extract existing encodings in the system:
			 * 
			 * foreach x in 0..65535 do 
			 *	if exists(Encoding:=System.Text.Encoding.GetEncoding(x)) then
			 *		print([x,Encoding.WebName,Encoding.EncodingName])
			 ************************************************************/


			BomLength = 0;

			if (Count >= 3 && Data[Offset] == 0xef && Data[Offset + 1] == 0xbb && Data[Offset + 2] == 0xbf)
			{
				BomLength = 3;
				return Encoding.UTF8;
			}
			else if (Count >= 2 && Data[Offset] == 0xfe && Data[Offset + 1] == 0xff)
			{
				BomLength = 2;
				return Encoding.BigEndianUnicode;
			}
			else if (Count >= 2 && Data[Offset] == 0xff && Data[Offset + 1] == 0xfe)
			{
				if (Count >= 4 && Data[Offset + 2] == 0 && Data[Offset + 3] == 0)
				{
					BomLength = 4;
					return Encoding.UTF32;
				}
				else
				{
					BomLength = 2;
					return Encoding.Unicode;
				}
			}
			else if (Count >= 4 && Data[Offset] == 0 && Data[Offset + 1] == 0 && Data[Offset + 2] == 0xfe && Data[Offset + 3] == 0xff)
			{
				BomLength = 4;
				return BigEndianUnicode32;
			}
			else if (Count >= 4 && Data[Offset] == 0x2b && Data[Offset + 1] == 0x2f && Data[Offset + 2] == 0x76)
			{
				if (Data[Offset + 3] == 0x39 ||
					Data[Offset + 3] == 0x2b ||
					Data[Offset + 3] == 0x2f)
				{
					BomLength = 4;
					return Encoding.UTF7;
				}
				else if (Data[Offset + 3] == 0x38)
				{
					if (Count >= 5 && Data[Offset + 4] == 0x2d)
						BomLength = 5;
					else
						BomLength = 4;

					return Encoding.UTF7;
				}
			}
			else if (Count >= 3 && Data[Offset] == 0xf7 && Data[Offset + 1] == 0x64 && Data[Offset + 2] == 0x4c)
				throw new ArgumentException("UTF-1 encoding not supported.", nameof(Data));
			else if (Count >= 4 && Data[Offset] == 0xdd && Data[Offset + 1] == 0x73 && Data[Offset + 2] == 0x66 && Data[Offset + 3] == 0x73)
				throw new ArgumentException("UTF-EBCDIC encoding not supported.", nameof(Data));
			else if (Count >= 3 && Data[Offset] == 0x0e && Data[Offset + 1] == 0xfe && Data[Offset + 2] == 0xff)
				throw new ArgumentException("SCSU encoding not supported.", nameof(Data));
			else if (Count >= 3 && Data[Offset] == 0xfb && Data[Offset + 1] == 0xee && Data[Offset + 2] == 0x28)
				throw new ArgumentException("BOCU encoding not supported.", nameof(Data));
			else if (Count >= 4 && Data[Offset] == 0x84 && Data[Offset + 1] == 0x31 && Data[Offset + 2] == 0x95 && Data[Offset + 3] == 0x33)
			{
				BomLength = 4;
				return GB18030;
			}

			return DefaultEncoding ?? ISO_8859_1;
		}

		/// <summary>
		/// Gets a string from its binary representation, taking
		/// any Byte Order Mark (BOM) into account.
		/// 
		/// If no BOM is found, the default encoding in <paramref name="DefaultEncoding"/>
		/// is used, if defined. If not, ISO-8859-1 is used
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="Offset">Offset into array where string is encoded.</param>
		/// <param name="Count">Number of bytes of encoded string.</param>
		/// <param name="DefaultEncoding">Default encoding to use, in case
		/// a Byte Order Mark (BOM) is not found in the binary representation.</param>
		/// <returns>Decoded string</returns>
		public static string GetString(byte[] Data, int Offset, int Count, Encoding DefaultEncoding)
		{
			if (Data is null)
				return null;

			Encoding Encoding = GetEncoding(Data, Offset, Count, DefaultEncoding, out int BomLength);
			
			return Encoding.GetString(Data, Offset + BomLength, Count - BomLength);
		}

		/// <summary>
		/// Gets a string from its binary representation, taking
		/// any Byte Order Mark (BOM) into account.
		/// 
		/// If no BOM is found, the default encoding in <paramref name="DefaultEncoding"/>
		/// is used, if defined. If not, ISO-8859-1 is used
		/// </summary>
		/// <param name="Data">Binary Data</param>
		/// <param name="DefaultEncoding">Default encoding to use, in case
		/// a Byte Order Mark (BOM) is not found in the binary representation.</param>
		/// <returns>Decoded string</returns>
		public static string GetString(byte[] Data, Encoding DefaultEncoding)
		{
			if (Data is null)
				return null;

			Encoding Encoding = GetEncoding(Data, DefaultEncoding, out int Offset);

			return Encoding.GetString(Data, Offset, Data.Length - Offset);
		}

		/// <summary>
		/// ISO-8859-1 encoding.
		/// </summary>
		public static Encoding ISO_8859_1
		{
			get
			{
				if (iso_8859_1 is null)
					iso_8859_1 = Encoding.GetEncoding("iso-8859-1");

				return iso_8859_1;
			}
		}

		private static Encoding iso_8859_1 = null;

		/// <summary>
		/// ISO-8859-1 encoding.
		/// </summary>
		public static Encoding BigEndianUnicode32
		{
			get
			{
				if (utf_32be is null)
					utf_32be = Encoding.GetEncoding("utf-32BE");

				return utf_32be;
			}
		}

		private static Encoding utf_32be = null;

		/// <summary>
		/// GB18030 encoding (simplified Chinese).
		/// </summary>
		public static Encoding GB18030
		{
			get
			{
				if (gb18030 is null)
					gb18030 = Encoding.GetEncoding("GB18030");

				return gb18030;
			}
		}

		private static Encoding gb18030 = null;

		/// <summary>
		/// UTF-8 encoding with Byte Order Mark (BOM)
		/// </summary>
		public static Encoding Utf8WithBom
		{
			get
			{
				if (utf8WithBom is null)
					utf8WithBom = new UTF8Encoding(true);

				return utf8WithBom;
			}
		}

		private static Encoding utf8WithBom = null;

		/// <summary>
		/// UTF-8 encoding without Byte Order Mark (BOM)
		/// </summary>
		public static Encoding Utf8WithoutBom
		{
			get
			{
				if (utf8WithoutBom is null)
					utf8WithoutBom = new UTF8Encoding(false);

				return utf8WithoutBom;
			}
		}

		private static Encoding utf8WithoutBom = null;

		/// <summary>
		/// Returns the port number at the end of a string, if found.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String without port number.</returns>
		public static string RemovePortNumber(this string s)
		{
			if (s is null)
				return null;

			int i;

			if (s.StartsWith("["))
			{
				i = s.LastIndexOf("]:");
				if (i < 0)
					return s;

				i++;
			}
			else
			{
				i = s.LastIndexOf(':');
				if (i < 0)
					return s;
			}

			if (int.TryParse(s.Substring(i + 1), out int j) && j >= 0 && j <= 65535)
				return s.Substring(0, i);
			else
				return s;
		}
	}
}
