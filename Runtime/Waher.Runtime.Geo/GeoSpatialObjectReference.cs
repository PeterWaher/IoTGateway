using System.Threading.Tasks;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// A base class for geo-spatial object references, implementing the basic members of the
	/// <see cref="IGeoSpatialObject"/> interface.
	/// </summary>
	public class GeoSpatialObjectReference : IGeoSpatialObjectReference
	{
		/// <summary>
		/// A base class for geo-spatial object references, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		public GeoSpatialObjectReference()
		{
		}

		/// <summary>
		/// A base class for geo-spatial object references, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="Object">Geo-spatial object.</param>
		public GeoSpatialObjectReference(GeoSpatialObject Object)
		{
			this.GeoId = Object.GeoId;
			this.Location = Object.Location;
			this.EphemeralLocation = Object.EphemeralLocation;
		}

		/// <summary>
		/// Create a geo-spatial object reference from a geo-spatial object.
		/// </summary>
		/// <param name="Object">Object.</param>
		/// <returns>Object reference.</returns>
		public static async Task<GeoSpatialObjectReference> Create(IGeoSpatialObject Object)
		{
			return new GeoSpatialObjectReference()
			{
				GeoId = Object.GeoId,
				EphemeralLocation = Object.EphemeralLocation,
				Location = await Object.GetLocation()
			};
		}

		/// <summary>
		/// The ID of the geo-spatial object.
		/// </summary>
		public string GeoId { get; set; }

		/// <summary>
		/// Gets the geo-spatial location of the object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		public GeoPosition Location { get; set; }

		/// <summary>
		/// If the location of the geo-spatial object is ephemeral.
		/// </summary>
		public bool EphemeralLocation { get; set; }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is GeoSpatialObjectReference Reference))
				return false;

			return
				this.GeoId.Equals(Reference.GeoId) &&
				this.EphemeralLocation.Equals(Reference.EphemeralLocation) &&
				(this.Location?.Equals(Reference.Location) ?? (Reference.Location is null));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.GeoId.GetHashCode();
			Result ^= Result << 5 ^ this.EphemeralLocation.GetHashCode();
			Result ^= Result << 5 ^ (this.Location?.GetHashCode() ?? 0);

			return Result;
		}
	}
}
