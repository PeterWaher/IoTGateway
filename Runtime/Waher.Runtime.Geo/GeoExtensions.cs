namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Extension methods for geo-spatial calculations.
	/// </summary>
	public static class GeoExtensions
	{
		/// <summary>
		/// Checks if a position lies outside a given bounding box, using the Mercator projection.
		/// If Min.Longitude > Max.Longitude, the bounding box is considered to pass the +-180 longitude.
		/// If any of the Min or Max coordinates are null, the corresponding coordinate is ignored.
		/// </summary>
		/// <param name="Position">Geo-spatial position</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <returns>If the position lies outside of the box.</returns>
		public static bool LiesOutside(this GeoPosition Position, GeoPosition Min, GeoPosition Max)
		{
			return Position.LiesOutside(Min, Max, false, false);
		}

		/// <summary>
		/// Checks if a position lies outside a given bounding box, using the Mercator projection.
		/// If Min.Longitude > Max.Longitude, the bounding box is considered to pass the +-180 longitude.
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
			int MinLat = Min is null ? 1 : Position.Latitude.CompareTo(Min.Latitude);
			int MaxLat = Max is null ? -1 : Position.Latitude.CompareTo(Max.Latitude);

			if (MinLat < 0 || IncludeMin && MinLat == 0)
				return true;

			if (MaxLat > 0 || IncludeMax && MaxLat == 0)
				return true;

			int MinLong = Min is null ? 1 : Position.Longitude.CompareTo(Min.Longitude);
			int MaxLong = Max is null ? -1 : Position.Longitude.CompareTo(Max.Longitude);

			if (!(Min is null || Max is null) && Min.Longitude > Max.Longitude)
			{
				MinLong = -MinLong;
				MaxLong = -MaxLong;
			}

			if (MinLong < 0 || IncludeMin && MinLong == 0)
				return true;

			if (MaxLong > 0 || IncludeMax && MaxLong == 0)
				return true;

			if (Position.Altitude.HasValue && Min.Altitude.HasValue && Max.Altitude.HasValue)
			{
				if (Position.Altitude.Value < Min.Altitude.Value ||
					Position.Altitude.Value > Max.Altitude.Value)
				{
					return true;
				}
			}

			return false;
		}
	}
}
