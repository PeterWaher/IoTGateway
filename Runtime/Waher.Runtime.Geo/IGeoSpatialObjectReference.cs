namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Reference to an object with a geo-spatial location
	/// </summary>
	public interface IGeoSpatialObjectReference
	{
		/// <summary>
		/// The ID of the geo-spatial object.
		/// </summary>
		string GeoId { get; }

		/// <summary>
		/// Gets the geo-spatial location of the object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		GeoPosition Location { get; }

		/// <summary>
		/// If the location of the geo-spatial object is ephemeral.
		/// </summary>
		bool EphemeralLocation { get; }
	}
}
