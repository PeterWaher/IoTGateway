using System.Threading.Tasks;

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
		/// Optional reference, if in memory, null otherwise.
		/// </summary>
		IGeoSpatialObject Reference { get; }

		/// <summary>
		/// Gets a reference to the geo-spatial object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		Task<IGeoSpatialObject> GetObject();
	}
}
