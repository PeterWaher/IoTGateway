using System.Threading.Tasks;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Interface for objects with a geo-spatial location
	/// </summary>
	public interface IGeoSpatialObject
	{
		/// <summary>
		/// If the object has a geo-spatial location.
		/// </summary>
		bool HasGeoLocation { get; }

		/// <summary>
		/// The ID of the geo-spatial object.
		/// </summary>
		string GeoId { get; }

		/// <summary>
		/// Gets the geo-spatial location of the object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		Task<GeoPosition> GetLocation();

		/// <summary>
		/// If the location of the geo-spatial object is ephemeral.
		/// </summary>
		bool EphemeralLocation { get; }
	}
}
