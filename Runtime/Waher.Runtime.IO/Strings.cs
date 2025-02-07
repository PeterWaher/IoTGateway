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
			BomLength = 0;

			if (Data is null)
				return DefaultEncoding ?? ISO_8859_1;

			int c = Data.Length;
			if (c == 0)
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

			if (c >= 3 && Data[0] == 0xef && Data[1] == 0xbb && Data[2] == 0xbf)
			{
				BomLength = 3;
				return Encoding.UTF8;
			}
			else if (c >= 2 && Data[0] == 0xfe && Data[1] == 0xff)
			{
				BomLength = 2;
				return Encoding.BigEndianUnicode;
			}
			else if (c >= 2 && Data[0] == 0xff && Data[1] == 0xfe)
			{
				if (c >= 4 && Data[2] == 0 && Data[3] == 0)
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
			else if (c >= 4 && Data[0] == 0 && Data[1] == 0 && Data[2] == 0xfe && Data[3] == 0xff)
			{
				BomLength = 4;
				return BigEndianUnicode32;
			}
			else if (c >= 4 && Data[0] == 0x2b && Data[1] == 0x2f && Data[2] == 0x76)
			{
				if (Data[3] == 0x39 ||
					Data[3] == 0x2b ||
					Data[3] == 0x2f)
				{
					BomLength = 4;
					return Encoding.UTF7;
				}
				else if (Data[3] == 0x38)
				{
					if (c >= 5 && Data[4] == 0x2d)
						BomLength = 5;
					else
						BomLength = 4;

					return Encoding.UTF7;
				}
			}
			else if (c >= 3 && Data[0] == 0xf7 && Data[1] == 0x64 && Data[2] == 0x4c)
				throw new ArgumentException("UTF-1 encoding not supported.", nameof(Data));
			else if (c >= 4 && Data[0] == 0xdd && Data[1] == 0x73 && Data[2] == 0x66 && Data[3] == 0x73)
				throw new ArgumentException("UTF-EBCDIC encoding not supported.", nameof(Data));
			else if (c >= 3 && Data[0] == 0x0e && Data[1] == 0xfe && Data[2] == 0xff)
				throw new ArgumentException("SCSU encoding not supported.", nameof(Data));
			else if (c >= 3 && Data[0] == 0xfb && Data[1] == 0xee && Data[2] == 0x28)
				throw new ArgumentException("BOCU encoding not supported.", nameof(Data));
			else if (c >= 4 && Data[0] == 0x84 && Data[1] == 0x31 && Data[2] == 0x95 && Data[3] == 0x33)
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
		/// Returns the port number at the end of a string, if found.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>String without port number.</returns>
		public static string RemovePortNumber(this string s)
		{
			if (s is null)
				return null;

			int i = s.LastIndexOf(':');
			if (i >= 0 && int.TryParse(s.Substring(i + 1), out int j) && j >= 0 && j <= 65535)
				return s.Substring(0, i);
			else
				return s;
		}
	}
}
