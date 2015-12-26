using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking
{
	/// <summary>
	/// Helps with parsing of commong data types.
	/// </summary>
	public static class CommonTypes
	{
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
		/// Tries to decode a string encoded DateTime.
		/// </summary>
		/// <param name="s">Encoded value.</param>
		/// <param name="Value">Decoded value.</param>
		/// <returns>If the value could be decoded.</returns>
		public static bool TryParse(string s, out DateTime Value)
		{
			int i = s.IndexOf('T');

			if (i == 10 || s.Length == 10)
			{
				int Year, Month, Day;

				if (!int.TryParse(s.Substring(0, 4), out Year) ||
					!int.TryParse(s.Substring(5, 2), out Month) ||
					!int.TryParse(s.Substring(8, 2), out Day))
				{
					Value = DateTime.MinValue;
					return false;
				}

				Value = new DateTime(Year, Month, Day);

				if (i == 10)
				{
					TimeSpan TS;

					if (TimeSpan.TryParse(s.Substring(11), out TS))
					{
						Value += TS;
						return true;
					}
				}
				else
					return true;
			}

			Value = DateTime.MinValue;
			return false;
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
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Encodes a <see cref="Single"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(float x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
		}

		/// <summary>
		/// Encodes a <see cref="Decimal"/> for use in XML and other formats.
		/// </summary>
		/// <param name="x">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(decimal x)
		{
			return x.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
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

		#endregion

	}
}
