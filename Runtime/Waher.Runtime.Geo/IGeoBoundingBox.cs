namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Interface for a geo-spatial bounding box using the Mercator Projection.
	/// </summary>
	public interface IGeoBoundingBox
	{
		/// <summary>
		/// The ID of the geo-spatial bounding box.
		/// </summary>
		string BoxId { get; }

		/// <summary>
		/// Lower-left corner of bounding box.
		/// </summary>
		GeoPosition Min { get; }

		/// <summary>
		/// Upper-right corner of bounding box.
		/// </summary>
		GeoPosition Max { get; }

		/// <summary>
		/// If the min latitude/longitude/altitude should be considered as included in the boundoing box.
		/// </summary>
		bool IncludeMin { get; }

		/// <summary>
		/// If the max latitude/longitude/altitude should be considered as included in the boundoing box.
		/// </summary>
		bool IncludeMax { get; }

		/// <summary>
		/// If the bounding box is longitude-wrapped, i.e. if the box passes the +-180 degrees
		/// longitude.
		/// </summary>
		bool LongitudeWrapped { get; }

		/// <summary>
		/// If the bounding box contains a location.
		/// </summary>
		/// <param name="Location">Location</param>
		/// <returns>If the bounding box contains the location.</returns>
		bool Contains(GeoPosition Location);

		/// <summary>
		/// If the bounding box contains a location.
		/// </summary>
		/// <param name="Location">Location</param>
		/// <param name="IgnoreAltitude">If altitude component should be ignored.</param>
		/// <returns>If the bounding box contains the location.</returns>
		bool Contains(GeoPosition Location, bool IgnoreAltitude);
	}
}
