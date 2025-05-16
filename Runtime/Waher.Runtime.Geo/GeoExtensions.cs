using System.Runtime.InteropServices;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Extension methods for geo-spatial calculations.
	/// </summary>
	public static class GeoExtensions
	{
		/// <summary>
		/// Checks if a position lies outside a given bounding box, using the Mercator projection.
		/// If Min.Longitude > Max?.Longitude, the bounding box is considered to pass the +-180 longitude.
		/// If any of the Min or Max coordinates are null, the corresponding coordinate is ignored.
		/// </summary>
		/// <param name="Position">Geo-spatial position</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <returns>If the position lies outside of the box.</returns>
		public static bool LiesOutside(this GeoPosition Position, GeoPosition Min, GeoPosition Max)
		{
			return Position.LiesOutside(Min, Max, false, false, false);
		}

		/// <summary>
		/// Checks if a position lies outside a given bounding box, using the Mercator projection.
		/// If Min.Longitude > Max?.Longitude, the bounding box is considered to pass the +-180 longitude.
		/// If any of the Min or Max coordinates are null, the corresponding coordinate is ignored.
		/// </summary>
		/// <param name="Position">Geo-spatial position</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <param name="IncludeMin">If the min latitude/longitude/altitude should be considered as outside.</param>
		/// <param name="IncludeMax">If the max latitude/longitude/altitude should be considered as outside.</param>
		/// <returns>If the position lies outside of the box.</returns>
		public static bool LiesOutside(this GeoPosition Position, GeoPosition Min, GeoPosition Max,
			bool IncludeMin, bool IncludeMax)
		{
			return Position.LiesOutside(Min, Max, IncludeMin, IncludeMax, false);
		}

		/// <summary>
		/// Checks if a position lies outside a given bounding box, using the Mercator projection.
		/// If Min.Longitude > Max?.Longitude, the bounding box is considered to pass the +-180 longitude.
		/// If any of the Min or Max coordinates are null, the corresponding coordinate is ignored.
		/// </summary>
		/// <param name="Position">Geo-spatial position</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <param name="IncludeMin">If the min latitude/longitude/altitude should be considered as outside.</param>
		/// <param name="IncludeMax">If the max latitude/longitude/altitude should be considered as outside.</param>
		/// <param name="IgnoreAltitude">If altitude should be ignored in the comparison.</param>
		/// <returns>If the position lies outside of the box.</returns>
		public static bool LiesOutside(this GeoPosition Position, GeoPosition Min, GeoPosition Max,
			bool IncludeMin, bool IncludeMax, bool IgnoreAltitude)
		{
			if (Position.Latitude.OutOfRange(Min?.Latitude, Max?.Latitude, IncludeMin, IncludeMax))
				return true;

			if (!(Min is null) && !(Max is null) && Min.Longitude > Max.Longitude)
			{
				if (!Position.Longitude.OutOfRange(Max.Longitude, Min.Longitude, !IncludeMin, !IncludeMax))
					return true;
			}
			else
			{
				if (Position.Longitude.OutOfRange(Min?.Longitude, Max?.Longitude, IncludeMin, IncludeMax))
					return true;
			}

			if (IgnoreAltitude)
				return false;

			if (Position.Altitude.HasValue &&
				Position.Altitude.Value.OutOfRange(Min?.Altitude, Max?.Altitude, IncludeMin, IncludeMax))
			{
				return true;
			}

			return false;
		}

		private static bool OutOfRange(this double Value, double? Min, double? Max,
			bool IncludeMin, bool IncludeMax)
		{
			int MinComp = Min.HasValue ? Value.CompareTo(Min.Value) : 1;
			int MaxComp = Max.HasValue ? Value.CompareTo(Max.Value) : -1;

			if (MinComp < 0 || IncludeMin && MinComp == 0)
				return true;

			if (MaxComp > 0 || IncludeMax && MaxComp == 0)
				return true;

			return false;
		}

		private static bool InRange(this double Value, double? Min, double? Max,
			bool IncludeMin, bool IncludeMax)
		{
			int MinComp = Min.HasValue ? Value.CompareTo(Min.Value) : 1;
			int MaxComp = Max.HasValue ? Value.CompareTo(Max.Value) : -1;

			if (MinComp < 0 || (!IncludeMin && MinComp == 0))
				return false;

			if (MaxComp > 0 || (!IncludeMax && MaxComp == 0))
				return false;

			return true;
		}

		/// <summary>
		/// If a <paramref name="Position"/> is north of a reference point
		/// <paramref name="Reference"/>. The reference point itself will be included
		/// if <paramref name="IncludeReference"/> is true.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference point.</param>
		/// <param name="IncludeReference">If reference point is included.</param>
		/// <returns>If Position is north of reference point.</returns>
		public static bool NorthOf(this GeoPosition Position, GeoPosition Reference, bool IncludeReference)
		{
			int i = Position.Latitude.CompareTo(Reference.Latitude);
			return i > 0 || (IncludeReference && i == 0);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is south of a reference point
		/// <paramref name="Reference"/>. The reference point itself will be included
		/// if <paramref name="IncludeReference"/> is true.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference point.</param>
		/// <param name="IncludeReference">If reference point is included.</param>
		/// <returns>If Position is south of reference point.</returns>
		public static bool SouthOf(this GeoPosition Position, GeoPosition Reference, bool IncludeReference)
		{
			int i = Position.Latitude.CompareTo(Reference.Latitude);
			return i < 0 || (IncludeReference && i == 0);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is east of a reference point
		/// <paramref name="Reference"/>. The reference point itself will be included
		/// if <paramref name="IncludeReference"/> is true.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference point.</param>
		/// <param name="IncludeReference">If reference point is included.</param>
		/// <returns>If Position is east of reference point.</returns>
		public static bool EastOf(this GeoPosition Position, GeoPosition Reference, bool IncludeReference)
		{
			int i = Position.Longitude.CompareTo(Reference.Longitude);
			return i > 0 || (IncludeReference && i == 0);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is west of a reference point
		/// <paramref name="Reference"/>. The reference point itself will be included
		/// if <paramref name="IncludeReference"/> is true.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference point.</param>
		/// <param name="IncludeReference">If reference point is included.</param>
		/// <returns>If Position is west of reference point.</returns>
		public static bool WestOf(this GeoPosition Position, GeoPosition Reference, bool IncludeReference)
		{
			int i = Position.Longitude.CompareTo(Reference.Longitude);
			return i < 0 || (IncludeReference && i == 0);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is north of a bounding box
		/// <paramref name="Reference"/>.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference bounding box.</param>
		/// <returns>If Position is north of reference bounding box.</returns>
		public static bool NorthOf(this GeoPosition Position, GeoBoundingBox Reference)
		{
			return Position.NorthOf(Reference.Max, !Reference.IncludeMax);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is south of a bounding box
		/// <paramref name="Reference"/>.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference bounding box.</param>
		/// <returns>If Position is south of reference bounding box.</returns>
		public static bool SouthOf(this GeoPosition Position, GeoBoundingBox Reference)
		{
			return Position.SouthOf(Reference.Min, !Reference.IncludeMin);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is east of a bounding box
		/// <paramref name="Reference"/>.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference bounding box.</param>
		/// <returns>If Position is east of reference bounding box.</returns>
		public static bool EastOf(this GeoPosition Position, GeoBoundingBox Reference)
		{
			if (Reference.LongitudeWrapped)
				return Position.EastOf(Reference.Min, !Reference.IncludeMin);
			else
				return Position.EastOf(Reference.Max, !Reference.IncludeMax);
		}

		/// <summary>
		/// If a <paramref name="Position"/> is west of a bounding box
		/// <paramref name="Reference"/>.
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference bounding box.</param>
		/// <returns>If Position is west of reference bounding box.</returns>
		public static bool WestOf(this GeoPosition Position, GeoBoundingBox Reference)
		{
			if (Reference.LongitudeWrapped)
				return Position.WestOf(Reference.Max, !Reference.IncludeMax);
			else
				return Position.WestOf(Reference.Min, !Reference.IncludeMin);
		}

		/// <summary>
		/// If the altitude defined by <paramref name="Position"/> (if any) lies within
		/// the scope of the bounding box <paramref name="Reference"/> (if defined).
		/// </summary>
		/// <param name="Position">Position.</param>
		/// <param name="Reference">Reference bounding box.</param>
		/// <returns>If Position altitude is within the bounding box (if defined).</returns>
		public static bool AltitudeCheck(this GeoPosition Position, GeoBoundingBox Reference)
		{
			if (Position.Altitude.HasValue)
			{
				return Position.Altitude.Value.InRange(Reference.Min?.Altitude, Reference.Max?.Altitude,
					Reference.IncludeMin, Reference.IncludeMax);
			}

			return true;
		}
	}
}
