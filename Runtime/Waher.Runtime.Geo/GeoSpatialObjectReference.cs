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
		public GeoSpatialObjectReference(GeoSpatialObject Object)
		{
			this.GeoId = Object.GeoId;
			this.Location = Object.Location;
			this.Reference = Object;
		}

		/// <summary>
		/// The ID of the geo-spatial object.
		/// </summary>
		public string GeoId { get; }

		/// <summary>
		/// Gets the geo-spatial location of the object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		public GeoPosition Location { get; }

		/// <summary>
		/// Optional reference, if in memory.
		/// </summary>
		public IGeoSpatialObject Reference { get; }

		/// <summary>
		/// Gets a reference to the geo-spatial object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		public virtual Task<IGeoSpatialObject> GetObject() => Task.FromResult(this.Reference);

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (!(obj is GeoSpatialObjectReference Reference))
				return false;

			return
				this.GeoId.Equals(Reference.GeoId) &&
				(this.Location?.Equals(Reference.Location) ?? (Reference.Location is null)) &&
				(this.Reference?.Equals(Reference.Reference) ?? (Reference.Reference is null));
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.GeoId.GetHashCode();
			Result ^= Result << 5 ^ (this.Location?.GetHashCode() ?? 0);
			Result ^= Result << 5 ^ (this.Reference?.GetHashCode() ?? 0);

			return Result;
		}
	}
}
