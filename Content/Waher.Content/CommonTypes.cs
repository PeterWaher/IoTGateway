using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Waher.Script;

namespace Waher.Content
{
	/// <summary>
	/// Helps with parsing of commong data types.
	/// </summary>
	public static class CommonTypes
	{
		/// <summary>
		/// Contains the CR LF character sequence.
		/// </summary>
		public static readonly char[] CRLF = new char[] { '\r', '\n' };

		#region Parsing

		/// <summary>
		/// Tries to decode a string encoded double.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out double Value)
		{
			return double.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Tries to decode a string encoded float.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out float Value)
		{
			return float.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Tries to decode a string encoded decimal.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out decimal Value)
		{
			return decimal.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		/// <summary>
		/// Tries to decode a string encoded double.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <param name="NrDecimals">Number of decimals found.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out double Value, out byte NrDecimals)
		{
			return double.TryParse(Prepare(s, out NrDecimals), out Value);
		}

		private static string Prepare(string s, out byte NrDecimals)
		{
			string DecimalSeparator = System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator;
			s = s.Replace(".", DecimalSeparator);

			int i = s.IndexOf(DecimalSeparator);
			if (i < 0)
				NrDecimals = 0;
			else
			{
				int c = s.Length;
				char ch;

				i += DecimalSeparator.Length;
				NrDecimals = 0;

				while (i < c)
				{
					ch = s[i++];

					if (ch >= '0' && ch <= '9')
						NrDecimals++;
				}
			}

			return s;
		}

		/// <summary>
		/// Tries to decode a string encoded float.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <param name="NrDecimals">Number of decimals found.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out float Value, out byte NrDecimals)
		{
			return float.TryParse(Prepare(s, out NrDecimals), out Value);
		}

		/// <summary>
		/// Tries to decode a string encoded decimal.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <param name="NrDecimals">Number of decimals found.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out decimal Value, out byte NrDecimals)
		{
			return decimal.TryParse(Prepare(s, out NrDecimals), out Value);
		}

		/// <summary>
		/// Tries to decode a string encoded boolean.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out bool Value)
		{
			s = s.ToLower();

			if (s == "1" || s == "true" || s == "yes" || s == "on")
			{
				Value = true;
				return true;
			}
			else if (s == "0" || s == "false" || s == "no" || s == "off")
			{
				Value = false;
				return true;
			}
			else
			{
				Value = false;
				return false;
			}
		}

		/// <summary>
		/// Parses a date and time value encoded according to RFC 822, §5.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParseRfc822(string s, out DateTimeOffset Value)
		{
			Match M = rfc822datetime.Match(s);
			if (!M.Success)
			{
				Value = DateTimeOffset.MinValue;
				return false;
			}

			int Day = int.Parse(M.Groups["Day"].Value);
			string MonthStr = M.Groups["Month"].Value;
			int Month = Array.IndexOf<string>(months, MonthStr) + 1;
			int Year = int.Parse(M.Groups["Year"].Value);
			int Hour = int.Parse(M.Groups["Hour"].Value);
			int Minute = int.Parse(M.Groups["Minute"].Value);
			int Second = int.Parse(M.Groups["Second"].Value);
			string TimeZoneStr = M.Groups["TimeZone"].Value;
			TimeSpan TimeZone;

			switch (TimeZoneStr)
			{
				case "UT":
				case "GMT":
				case "Z":
					TimeZone = TimeSpan.Zero;
					break;

				case "A":
					TimeZone = TimeSpan.FromHours(-1);
					break;

				case "B":
					TimeZone = TimeSpan.FromHours(-2);
					break;

				case "C":
					TimeZone = TimeSpan.FromHours(-3);
					break;

				case "EDT":
				case "D":
					TimeZone = TimeSpan.FromHours(-4);
					break;

				case "EST":
				case "CDT":
				case "E":
					TimeZone = TimeSpan.FromHours(-5);
					break;

				case "CST":
				case "MDT":
				case "F":
					TimeZone = TimeSpan.FromHours(-6);
					break;

				case "MST":
				case "PDT":
				case "G":
					TimeZone = TimeSpan.FromHours(-7);
					break;

				case "PST":
				case "H":
					TimeZone = TimeSpan.FromHours(-8);
					break;

				case "I":
					TimeZone = TimeSpan.FromHours(-9);
					break;

				case "K":
					TimeZone = TimeSpan.FromHours(-10);
					break;

				case "L":
					TimeZone = TimeSpan.FromHours(-11);
					break;

				case "M":
					TimeZone = TimeSpan.FromHours(-12);
					break;

				case "N":
					TimeZone = TimeSpan.FromHours(1);
					break;

				case "O":
					TimeZone = TimeSpan.FromHours(2);
					break;

				case "P":
					TimeZone = TimeSpan.FromHours(3);
					break;

				case "Q":
					TimeZone = TimeSpan.FromHours(4);
					break;

				case "R":
					TimeZone = TimeSpan.FromHours(5);
					break;

				case "S":
					TimeZone = TimeSpan.FromHours(6);
					break;

				case "T":
					TimeZone = TimeSpan.FromHours(7);
					break;

				case "U":
					TimeZone = TimeSpan.FromHours(8);
					break;

				case "V":
					TimeZone = TimeSpan.FromHours(9);
					break;

				case "w":
					TimeZone = TimeSpan.FromHours(10);
					break;

				case "X":
					TimeZone = TimeSpan.FromHours(11);
					break;

				case "Y":
					TimeZone = TimeSpan.FromHours(12);
					break;

				default:
					TimeZone = new TimeSpan(int.Parse(TimeZoneStr.Substring(1, 2)), int.Parse(TimeZoneStr.Substring(3, 2)), 0);
					if (TimeZoneStr.StartsWith("-"))
						TimeZone = TimeZone.Negate();
					break;
			}

			Value = new DateTimeOffset(Year, Month, Day, Hour, Minute, Second, TimeZone);
			return true;
		}

		private static readonly Regex rfc822datetime = new Regex(@"^((?'WeekDay'Mon|Tue|Wed|Thu|Fri|Sat|Sun),\s*)?(?'Day'\d+)\s+(?'Month'Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\s+(?'Year'\d+)\s+(?'Hour'\d+)[:](?'Minute'\d+)([:](?'Second'\d+))?\s+(?'TimeZone'UT|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT|[A-IK-Z]|[+-]\d{4})\s*([(].*[)])?$", RegexOptions.Singleline | RegexOptions.Compiled);
		private static readonly string[] months = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
		private static readonly Regex rfc822timezone = new Regex(@"^UT|GMT|EST|EDT|CST|CDT|MST|MDT|PST|PDT|[A-IK-Z]|[+-]\d{4}$", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Parses a timezone string, according to RFC 822.
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="Value">Timezone</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParseTimeZone(string s, out TimeSpan Value)
		{
			Match M = rfc822timezone.Match(s);
			if (!M.Success)
			{
				Value = TimeSpan.Zero;
				return false;
			}

			switch (s)
			{
				case "UT":
				case "GMT":
				case "Z":
					Value = TimeSpan.Zero;
					break;

				case "A":
					Value = TimeSpan.FromHours(-1);
					break;

				case "B":
					Value = TimeSpan.FromHours(-2);
					break;

				case "C":
					Value = TimeSpan.FromHours(-3);
					break;

				case "EDT":
				case "D":
					Value = TimeSpan.FromHours(-4);
					break;

				case "EST":
				case "CDT":
				case "E":
					Value = TimeSpan.FromHours(-5);
					break;

				case "CST":
				case "MDT":
				case "F":
					Value = TimeSpan.FromHours(-6);
					break;

				case "MST":
				case "PDT":
				case "G":
					Value = TimeSpan.FromHours(-7);
					break;

				case "PST":
				case "H":
					Value = TimeSpan.FromHours(-8);
					break;

				case "I":
					Value = TimeSpan.FromHours(-9);
					break;

				case "K":
					Value = TimeSpan.FromHours(-10);
					break;

				case "L":
					Value = TimeSpan.FromHours(-11);
					break;

				case "M":
					Value = TimeSpan.FromHours(-12);
					break;

				case "N":
					Value = TimeSpan.FromHours(1);
					break;

				case "O":
					Value = TimeSpan.FromHours(2);
					break;

				case "P":
					Value = TimeSpan.FromHours(3);
					break;

				case "Q":
					Value = TimeSpan.FromHours(4);
					break;

				case "R":
					Value = TimeSpan.FromHours(5);
					break;

				case "S":
					Value = TimeSpan.FromHours(6);
					break;

				case "T":
					Value = TimeSpan.FromHours(7);
					break;

				case "U":
					Value = TimeSpan.FromHours(8);
					break;

				case "V":
					Value = TimeSpan.FromHours(9);
					break;

				case "w":
					Value = TimeSpan.FromHours(10);
					break;

				case "X":
					Value = TimeSpan.FromHours(11);
					break;

				case "Y":
					Value = TimeSpan.FromHours(12);
					break;

				default:
					Value = new TimeSpan(int.Parse(s.Substring(1, 2)), int.Parse(s.Substring(3, 2)), 0);
					if (s.StartsWith("-"))
						Value = Value.Negate();
					break;
			}

			return true;
		}

		/// <summary>
		/// Parses a set of comma or semicolon-separated field values, optionaly delimited by ' or " characters.
		/// </summary>
		/// <param name="Value">Field Value</param>
		/// <returns>Parsed set of field values.</returns>
		public static KeyValuePair<string, string>[] ParseFieldValues(string Value)
		{
			List<KeyValuePair<string, string>> Result = new List<KeyValuePair<string, string>>();
			StringBuilder sb = new StringBuilder();
			string Key = null;
			int State = 0;

			foreach (char ch in Value)
			{
				switch (State)
				{
					case 0: // Waiting for Parameter Name.
						if (ch <= 32)
							break;
						else if (ch == '=')
						{
							State = 2;
							Key = string.Empty;
						}
						else
						{
							State++;
							sb.Append(ch);
						}
						break;

					case 1: // Parameter
						if (ch == '=')
						{
							Key = sb.ToString().TrimEnd();
							sb.Clear();
							State = 2;
						}
						else
							sb.Append(ch);
						break;

					case 2: // First character in Value
						if (ch == '"')
							State += 2;
						else if (ch == '\'')
							State += 4;
						else
						{
							sb.Append(ch);
							State++;
						}
						break;

					case 3: // Normal value
						if (ch == ',' || ch == ';')
						{
							Value = sb.ToString().Trim();
							Result.Add(new KeyValuePair<string, string>(Key, Value));
							sb.Clear();
							State = 0;
						}
						else
							sb.Append(ch);
						break;

					case 4: // "Value"
						if (ch == '"')
							State--;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 5: // Escape
						if (ch != '"' && ch != '\"' && ch != '\\')
							sb.Append('\\');

						sb.Append(ch);
						State--;
						break;

					case 6: // 'Value'
						if (ch == '\'')
							State = 3;
						else if (ch == '\\')
							State++;
						else
							sb.Append(ch);
						break;

					case 7: // Escape
						if (ch != '"' && ch != '\"' && ch != '\\')
							sb.Append('\\');

						sb.Append(ch);
						State--;
						break;
				}
			}

			if (State == 3)
			{
				Value = sb.ToString().Trim();
				Result.Add(new KeyValuePair<string, string>(Key, Value));
			}

			return Result.ToArray();
		}

		#endregion

		#region Encoding

		/// <summary>
		/// Encodes a <see cref="Boolean"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(bool x)
		{
			return x ? "true" : "false";
		}

		/// <summary>
		/// Encodes a <see cref="Double"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(double x)
		{
			return Expression.ToString(x);
		}

		/// <summary>
		/// Encodes a <see cref="Single"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(float x)
		{
			return Expression.ToString(x);
		}

		/// <summary>
		/// Encodes a <see cref="Decimal"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(decimal x)
		{
			return Expression.ToString(x);
		}

		/// <summary>
		/// Encodes a <see cref="Double"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(double x, byte NrDecimals)
		{
			return x.ToString("F" + NrDecimals.ToString()).Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Encodes a <see cref="Single"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(float x, byte NrDecimals)
		{
			return x.ToString("F" + NrDecimals.ToString()).Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Encodes a <see cref="Decimal"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <param name="NrDecimals">Number of decimals.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(decimal x, byte NrDecimals)
		{
			return x.ToString("F" + NrDecimals.ToString()).Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Encodes a date and time, according to RFC 822 §5.
		/// </summary>
		/// <param name="Timestamp">Timestamp to encode.</param>
		/// <returns>Encoded date and time.</returns>
		public static string EncodeRfc822(DateTime Timestamp)
		{
			TimeSpan TimeZone = Timestamp - Timestamp.ToUniversalTime();
			return EncodeRfc822(new DateTimeOffset(Timestamp, TimeZone));
		}

		/// <summary>
		/// Encodes a date and time, according to RFC 822 §5.
		/// </summary>
		/// <param name="Timestamp">Timestamp to encode.</param>
		/// <returns>Encoded date and time.</returns>
		public static string EncodeRfc822(DateTimeOffset Timestamp)
		{
			StringBuilder Output = new StringBuilder();

			switch (Timestamp.DayOfWeek)
			{
				case DayOfWeek.Monday:
					Output.Append("Mon, ");
					break;

				case DayOfWeek.Tuesday:
					Output.Append("Tue, ");
					break;

				case DayOfWeek.Wednesday:
					Output.Append("Wed, ");
					break;

				case DayOfWeek.Thursday:
					Output.Append("Thu, ");
					break;

				case DayOfWeek.Friday:
					Output.Append("Fri, ");
					break;

				case DayOfWeek.Saturday:
					Output.Append("Sat, ");
					break;

				case DayOfWeek.Sunday:
					Output.Append("Sun, ");
					break;
			}

			Output.Append(Timestamp.Day.ToString());
			Output.Append(' ');

			switch (Timestamp.Month)
			{
				case 1:
					Output.Append("Jan");
					break;

				case 2:
					Output.Append("Feb");
					break;

				case 3:
					Output.Append("Mar");
					break;

				case 4:
					Output.Append("Apr");
					break;

				case 5:
					Output.Append("May");
					break;

				case 6:
					Output.Append("Jun");
					break;

				case 7:
					Output.Append("Jul");
					break;

				case 8:
					Output.Append("Aug");
					break;

				case 9:
					Output.Append("Sep");
					break;

				case 10:
					Output.Append("Oct");
					break;

				case 11:
					Output.Append("Nov");
					break;

				case 12:
					Output.Append("Dec");
					break;
			}

			Output.Append(' ');
			Output.Append(Timestamp.Year.ToString());

			Output.Append(' ');
			Output.Append(Timestamp.Hour.ToString("D2"));
			Output.Append(':');
			Output.Append(Timestamp.Minute.ToString("D2"));
			Output.Append(':');
			Output.Append(Timestamp.Second.ToString("D2"));

			TimeSpan TimeZone = Timestamp.Offset;

			if (TimeZone == TimeSpan.Zero)
				Output.Append(" GMT");
			else
			{
				if (TimeZone < TimeSpan.Zero)
				{
					Output.Append(" -");
					TimeZone = TimeZone.Negate();
				}
				else
					Output.Append(" +");

				Output.Append(TimeZone.Hours.ToString("D2"));
				Output.Append(TimeZone.Minutes.ToString("D2"));
			}

			return Output.ToString();
		}

		/// <summary>
		/// Encodes a string for inclusion in JSON.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string JsonStringEncode(string s)
		{
			return JSON.Encode(s);
		}

		/// <summary>
		/// Encodes a string for inclusion in a regular expression.
		/// </summary>
		/// <param name="s">String to encode.</param>
		/// <returns>Encoded string.</returns>
		public static string RegexStringEncode(string s)
		{
			return Escape(s, regexCharactersToEscape, regexCharacterEscapes);
		}

		private static readonly char[] regexCharactersToEscape = new char[] { '\\', '[', '^', '$', '.', '|', '?', '*', '+', '(', ')', '{', '}', '\n', '\r', '\t', '\b', '\f', '\a' };
		private static readonly string[] regexCharacterEscapes = new string[] { "\\\\", "\\[", "\\^", "\\$", "\\.", "\\|", "\\?", "\\*", "\\+", "\\(", "\\)", "\\{", "\\}", "\\n", "\\r", "\\t", "\\b", "\\f", "\\a" };

		/// <summary>
		/// Escapes a set of characters in a string.
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <param name="CharactersToEscape">Characters that needs to be escaped.</param>
		/// <param name="EscapeSequence">Escape sequence.</param>
		/// <returns>Escaped string.</returns>
		public static string Escape(string s, char[] CharactersToEscape, string EscapeSequence)
		{
			if (s is null)
				return null;

			int i = s.IndexOfAny(CharactersToEscape);
			if (i < 0)
				return s;

			StringBuilder sb = new StringBuilder();
			int j = 0;

			while (i >= 0)
			{
				if (i > j)
					sb.Append(s.Substring(j, i - j));

				sb.Append(EscapeSequence);
				sb.Append(s[i]);
				j = i + 1;
				i = s.IndexOfAny(CharactersToEscape, j);
			}

			if (j < s.Length)
				sb.Append(s.Substring(j));

			return sb.ToString();
		}

		/// <summary>
		/// Escapes a set of characters in a string.
		/// </summary>
		/// <param name="s">String to escape.</param>
		/// <param name="CharactersToEscape">Characters that needs to be escaped.</param>
		/// <param name="EscapeSequences">Individual escape sequences.</param>
		/// <returns>Escaped string.</returns>
		public static string Escape(string s, char[] CharactersToEscape, string[] EscapeSequences)
		{
			if (s is null)
				return null;

			int i = s.IndexOfAny(CharactersToEscape);
			if (i < 0)
				return s;

			StringBuilder sb = new StringBuilder();
			int j = 0;
			int k;

			while (i >= 0)
			{
				if (i > j)
					sb.Append(s.Substring(j, i - j));

				k = Array.IndexOf<char>(CharactersToEscape, s[i]);
				sb.Append(EscapeSequences[k]);
				j = i + 1;
				i = s.IndexOfAny(CharactersToEscape, j);
			}

			if (j < s.Length)
				sb.Append(s.Substring(j));

			return sb.ToString();
		}

		#endregion

		#region Number of decimals
		
		/// <summary>
		/// Calculates the number of decimals of a floating-point number.
		/// </summary>
		/// <param name="x">Value</param>
		/// <returns>Number of decimals.</returns>
		public static byte GetNrDecimals(double x)
		{
			return GetNrDecimals(Encode(x));
		}

		/// <summary>
		/// Calculates the number of decimals of a floating-point number.
		/// </summary>
		/// <param name="x">Value</param>
		/// <returns>Number of decimals.</returns>
		public static byte GetNrDecimals(float x)
		{
			return GetNrDecimals(Encode(x));
		}

		/// <summary>
		/// Calculates the number of decimals of a floating-point number.
		/// </summary>
		/// <param name="x">Value</param>
		/// <returns>Number of decimals.</returns>
		public static byte GetNrDecimals(decimal x)
		{
			return GetNrDecimals(Encode(x));
		}


		/// <summary>
		/// Calculates the number of decimals of a floating-point number.
		/// </summary>
		/// <param name="s">Value</param>
		/// <returns>Number of decimals.</returns>
		private static byte GetNrDecimals(string s)
		{
			int i = s.IndexOf('.');
			if (i < 0)
				return 0;
			else
			{
				i = s.Length - i - 1;
				if (i > byte.MaxValue)
					return byte.MaxValue;
				else
					return (byte)i;
			}
		}


		#endregion

		#region Byte Order Mark

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
		/// <returns></returns>
		public static string GetString(byte[] Data, Encoding DefaultEncoding)
		{
			if (Data is null)
				return null;

			int c = Data.Length;
			if (c == 0)
				return string.Empty;

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


			Encoding Encoding = DefaultEncoding ?? ISO_8859_1;
			int Offset = 0;

			if (c >= 3 && Data[0] == 0xef && Data[1] == 0xbb && Data[2] == 0xbf)
			{
				Encoding = Encoding.UTF8;
				Offset = 3;
			}
			else if (c >= 2 && Data[0] == 0xfe && Data[1] == 0xff)
			{
				Encoding = Encoding.BigEndianUnicode;
				Offset = 2;
			}
			else if (c >= 2 && Data[0] == 0xff && Data[1] == 0xfe)
			{
				if (c >= 4 && Data[2] == 0 && Data[3] == 0)
				{
					Encoding = Encoding.UTF32;
					Offset = 4;
				}
				else
				{
					Encoding = Encoding.Unicode;
					Offset = 2;
				}
			}
			else if (c >= 4 && Data[0] == 0 && Data[1] == 0 && Data[2] == 0xfe && Data[3] == 0xff)
			{
				Encoding = BigEndianUnicode32;
				Offset = 4;
			}
			else if (c >= 4 && Data[0] == 0x2b && Data[1] == 0x2f && Data[2] == 0x76)
			{
				if (Data[3] == 0x39 ||
					Data[3] == 0x2b ||
					Data[3] == 0x2f)
				{
					Encoding = Encoding.UTF7;
					Offset = 4;
				}
				else if (Data[3] == 0x38)
				{
					if (c >= 5 && Data[4] == 0x2d)
						Offset = 5;
					else
						Offset = 4;

					Encoding = Encoding.UTF7;
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
				Encoding = GB18030;
				Offset = 4;
			}

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

		#endregion
	}
}
