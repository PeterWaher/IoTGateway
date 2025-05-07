using System;
using System.Diagnostics;
using System.Text;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Contains information about a position in a geo-spatial coordinate system.
	/// </summary>
	[DebuggerDisplay("{XmlValue}")]
	public sealed class GeoPosition
	{
		private double latitude;
		private double longitude;
		private double? altitude;

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="Latitude">Latitude in degrees.</param>
		/// <param name="Longitude">Longitude in degrees.</param>
		public GeoPosition(double Latitude, double Longitude)
		{
			this.latitude = Latitude;
			this.longitude = Longitude;
			this.altitude = null;
		}

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="Latitude">Latitude in degrees.</param>
		/// <param name="Longitude">Longitude in degrees.</param>
		/// <param name="Altitude">Altitude in meters.</param>
		public GeoPosition(double Latitude, double Longitude, double? Altitude)
		{
			this.latitude = Latitude;
			this.longitude = Longitude;
			this.altitude = Altitude;
		}

		/// <summary>
		/// Latitude in degrees.
		/// </summary>
		public double Latitude
		{
			get => this.latitude;
			set
			{
				if (value < -90 || value > 90)
					throw new ArgumentOutOfRangeException(nameof(this.Latitude), "Latitude must be between -90 and 90 degrees.");

				this.latitude = value;
			}
		}

		/// <summary>
		/// Longitude in degrees.
		/// </summary>
		public double Longitude
		{
			get => this.longitude;
			set
			{
				if (value < -180 || value > 180)
					throw new ArgumentOutOfRangeException(nameof(this.Longitude), "Longitude must be between -180 and 180 degrees.");

				this.longitude = value;
			}
		}

		/// <summary>
		/// Altitude in meters. Can be null.
		/// </summary>
		public double? Altitude
		{
			get => this.altitude;
			set => this.altitude = value;
		}

		/// <summary>
		/// Tries to parse a string representation of a GeoPosition.
		/// </summary>
		/// <param name="s">String value.</param>
		/// <param name="Value">Parsed geo-position.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParse(string s, out GeoPosition Value)
		{
			int i = s.IndexOf(',');
			if (i < 0 ||
				!double.TryParse(s.Substring(0, i).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out double Latitude) ||
				Latitude < -90 ||
				Latitude > 90)
			{
				Value = null;
				return false;
			}

			i++;

			int j = s.IndexOf(',', i);
			double Longitude;
			double? Altitude;

			if (j < 0)
			{
				if (!double.TryParse(s.Substring(i).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Longitude) ||
					Longitude < -180 ||
					Longitude > 180)
				{
					Value = null;
					return false;
				}

				Altitude = null;
			}
			else
			{
				if (!double.TryParse(s.Substring(i, j - i).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Longitude) ||
					Longitude < -180 ||
					Longitude > 180)
				{
					Value = null;
					return false;
				}

				if (!double.TryParse(s.Substring(j + 1).Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out double d))
				{
					Value = null;
					return false;
				}

				Altitude = d;
			}

			Value = new GeoPosition(Latitude, Longitude, Altitude);

			return true;
		}

		/// <summary>
		/// String-representation of the geo-position, for use in XML.
		/// </summary>
		public string XmlValue
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(this.latitude.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
				sb.Append(',');
				sb.Append(this.longitude.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));

				if (this.altitude.HasValue)
				{
					sb.Append(',');
					sb.Append(this.altitude.Value.ToString().Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, "."));
				}

				return sb.ToString();
			}
		}

		/// <summary>
		/// Human-readable string-representation of the geo-position.
		/// </summary>
		public string HumanReadable
		{
			get
			{
				StringBuilder sb = new StringBuilder();

				Append(this.latitude, 'N', 'S', sb);
				sb.Append(' ');
				Append(this.longitude, 'E', 'W', sb);

				return sb.ToString();
			}
		}

		private static void Append(double Lat, char PosChar, char NegChar, StringBuilder sb)
		{
			int i;
			char Char;

			if (Lat >= 0)
				Char = PosChar;
			else
			{
				Char = NegChar;
				Lat = -Lat;
			}

			i = (int)Lat;
			Lat -= i;
			Lat *= 60;

			sb.Append(i);
			sb.Append("° ");

			i = (int)Lat;
			Lat -= i;
			Lat *= 60;

			sb.Append(i);
			sb.Append("' ");

			sb.Append(Lat.ToString().Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator));
			sb.Append("\" ");
			sb.Append(Char);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.XmlValue;
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return 
				obj is GeoPosition P &&
				this.latitude == P.latitude &&
				this.longitude == P.longitude &&
				this.altitude == P.altitude;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.latitude.GetHashCode();
			Result ^= Result << 5 ^ this.longitude.GetHashCode();

			if (this.altitude.HasValue)
				Result ^= Result << 5 ^ this.altitude.Value.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Checks if two geo-positions are equal.
		/// </summary>
		/// <param name="A">Geo-position A</param>
		/// <param name="B">Geo-position B</param>
		/// <returns>If they are equal</returns>
		public static bool operator ==(GeoPosition A, GeoPosition B)
		{
			if (A is null ^ B is null)
				return false;

			if (A is null)
				return true;
			
			return A.Equals(B);
		}

		/// <summary>
		/// Checks if two geo-positions are equal.
		/// </summary>
		/// <param name="A">Geo-position A</param>
		/// <param name="B">Geo-position B</param>
		/// <returns>If they are equal</returns>
		public static bool operator !=(GeoPosition A, GeoPosition B)
		{
			if (A is null ^ B is null)
				return true;

			if (A is null)
				return false;

			return !A.Equals(B);
		}

	}
}
