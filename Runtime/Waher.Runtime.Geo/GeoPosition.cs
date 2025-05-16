using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Waher.Persistence.Attributes;
using Waher.Script.Graphs;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Contains information about a position in a geo-spatial coordinate system.
	/// </summary>
	[DebuggerDisplay("{XmlValue}")]
	[TypeName(TypeNameSerialization.None)]
	public sealed class GeoPosition
	{
		/// <summary>
		/// Pattern for parsing a geo-position XML values. The names groups Lat, Lon and Alt 
		/// can be used to extract the latitude, longitude and altitude values respectively.
		/// </summary>
		public static readonly Regex XmlGeoPositionPattern = new Regex(@"(?'Lat'-?\d+(\.\d+)?),(?'Lon'-?\d+(\.\d+)?)(,(?'Alt'-?\d+(\.\d+)?))?", RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Pattern for parsing a geo-position GPS strings. The names groups Lat, Lon and Alt 
		/// can be used to extract the latitude, longitude and altitude values respectively.
		/// </summary>
		public static readonly Regex GpsGeoPositionPattern = new Regex(@"^\s*(?'LatDeg'\d+)\s*°?(\s*(?'LatMin'\d+)'(\s*(?'LatSec'\d+(\.\d+)?)"")?)?\s*(?'LatSign'[nNsS])\s*(?'LonDeg'\d+)\s*°?(\s*(?'LonMin'\d+)'(\s*(?'LonSec'\d+(\.\d+)?)"")?)?\s*(?'LonSign'[eEwW])(\s*,)?\s*((Alt|Altitude)\:?\s*)?((?'Alt'-?\d+(\.\d+)?)\s*(?'AltUnit'(m|ft))?)?\s*$", RegexOptions.Singleline | RegexOptions.Compiled);

		private double latitude;
		private double longitude;
		private double? altitude;

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		public GeoPosition()
		{
		}

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
		/// Normalized longitude. The range [-90,90] is linearly mapped to [0,1].
		/// </summary>
		public double NormalizedLatitude
		{
			get
			{
				if (this.latitude < -90 || this.latitude > 90)
					throw new InvalidOperationException("Latitude must be between -90 and 90 degrees.");

				return (this.latitude + 90) / 180;
			}
		}

		/// <summary>
		/// Normalized longitude. The range [-180,180] is linearly mapped to [0,1].
		/// </summary>
		public double NormalizedLongitude
		{
			get
			{
				if (this.longitude < -180 || this.longitude > 180)
					throw new InvalidOperationException("Longitude must be between -180 and 180 degrees.");

				return (this.longitude + 180) / 360;
			}
		}

		/// <summary>
		/// Parse a string representation of a GeoPosition. If first attempts the
		/// XML format. If not successful, it tries the GPS format.
		/// </summary>
		/// <returns>Parsed geo-position.</returns>
		/// <exception cref="FormatException">If the string could not be parsed.</exception>
		public static GeoPosition Parse(string s)
		{
			if (TryParse(s, out GeoPosition Value))
				return Value;
			else
				throw new FormatException("Invalid geo-position string: " + s);
		}

		/// <summary>
		/// Tries to parse a string representation of a GeoPosition. If first attempts the
		/// XML format. If not successful, it tries the GPS format.
		/// </summary>
		/// <param name="s">String value.</param>
		/// <param name="Value">Parsed geo-position.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParse(string s, out GeoPosition Value)
		{
			if (TryParseXml(s, out Value))
				return true;

			if (TryParseGps(s, out Value))
				return true;

			return false;
		}

		/// <summary>
		/// Tries to parse an XML string representation of a GeoPosition.
		/// </summary>
		/// <param name="s">String value.</param>
		/// <param name="Value">Parsed geo-position.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParseXml(string s, out GeoPosition Value)
		{
			int i = s.IndexOf(',');
			if (i < 0 ||
				!TryParse(s.Substring(0, i), out double Latitude) ||
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
				if (!TryParse(s.Substring(i), out Longitude) ||
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
				if (!TryParse(s.Substring(i, j - i), out Longitude) ||
					Longitude < -180 ||
					Longitude > 180)
				{
					Value = null;
					return false;
				}

				if (!TryParse(s.Substring(j + 1), out double d))
				{
					Value = null;
					return false;
				}

				Altitude = d;
			}

			Value = new GeoPosition(Latitude, Longitude, Altitude);

			return true;
		}

		private static bool TryParse(string s, out double Value)
		{
			return double.TryParse(s.Replace(".", System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator), out Value);
		}

		internal static string ToString(double Value)
		{
			string s = Value.ToString();
			s = s.Replace(System.Globalization.NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, ".");
			return Graph.TrimLabel(s);
		}

		/// <summary>
		/// Tries to parse a GPS string representation of a GeoPosition.
		/// </summary>
		/// <param name="s">String value.</param>
		/// <param name="Value">Parsed geo-position.</param>
		/// <returns>If the string could be parsed.</returns>
		public static bool TryParseGps(string s, out GeoPosition Value)
		{
			Value = null;

			Match M = GpsGeoPositionPattern.Match(s);
			if (!M.Success || M.Index > 0 || M.Length < s.Length)
				return false;

			if (!TryParse(M.Groups["LatDeg"], M.Groups["LatMin"], M.Groups["LatSec"],
				M.Groups["LatSign"], -90, 90, out double Latitude))
			{
				return false;
			}

			if (!TryParse(M.Groups["LonDeg"], M.Groups["LonMin"], M.Groups["LonSec"],
				M.Groups["LonSign"], -180, 180, out double Longitude))
			{
				return false;
			}

			string AltitudeStr = M.Groups["Alt"].Value;
			if (string.IsNullOrEmpty(AltitudeStr))
				Value = new GeoPosition(Latitude, Longitude);
			else
			{
				if (!TryParse(AltitudeStr, out double Altitude))
					return false;

				string Unit = M.Groups["AltUnit"].Value;

				if (!string.IsNullOrEmpty(Unit))
				{
					switch (Unit.ToLower())
					{
						case "m":
							break;

						case "ft":
							Altitude *= 0.3048;
							break;

						default:
							return false;
					}
				}

				Value = new GeoPosition(Latitude, Longitude, Altitude);
			}

			return true;
		}

		private static bool TryParse(Group Deg, Group Minute, Group Second, Group Sign,
			double Min, double Max, out double Value)
		{
			int i;

			if (Second is null || string.IsNullOrEmpty(Second.Value))
				Value = 0;
			else if (!TryParse(Second.Value, out Value))
				return false;

			Value /= 60;

			if (!string.IsNullOrEmpty(Minute?.Value))
			{
				if (!int.TryParse(Minute.Value, out i))
					return false;

				Value += i;
			}

			Value /= 60;

			if (!int.TryParse(Deg.Value, out i))
				return false;

			Value += i;

			if (Sign is null || string.IsNullOrEmpty(Sign.Value) || Sign.Value.Length != 1)
				return false;

			char ch = char.ToUpper(Sign.Value[0]);

			if (ch == 'S' || ch == 'W')
				Value = -Value;
			else if (ch != 'N' && ch != 'E')
				return false;

			if (Value < Min || Value > Max)
				return false;

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

				sb.Append(ToString(this.latitude));
				sb.Append(',');
				sb.Append(ToString(this.longitude));

				if (this.altitude.HasValue)
				{
					sb.Append(',');
					sb.Append(ToString(this.altitude.Value));
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

				if (this.altitude.HasValue)
				{
					sb.Append(", ");
					sb.Append(ToString(this.altitude.Value));
					sb.Append(" m");
				}

				return sb.ToString();
			}
		}

		private static void Append(double Nr, char PosChar, char NegChar, StringBuilder sb)
		{
			int i;
			char Char;

			if (Nr >= 0)
				Char = PosChar;
			else
			{
				Char = NegChar;
				Nr = -Nr;
			}

			i = (int)Nr;
			Nr -= i;
			Nr *= 60;

			sb.Append(i);
			sb.Append("° ");

			i = (int)Nr;
			Nr -= i;
			Nr *= 60;

			sb.Append(i);
			sb.Append("' ");

			sb.Append(ToString(Nr));
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
		/// Checks if two geo-positions are not equal.
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
