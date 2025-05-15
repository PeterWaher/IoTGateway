using System;
using System.Threading.Tasks;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// A base class for geo-spatial objects, implementing the basic members of the
	/// <see cref="IGeoSpatialObject"/> interface.
	/// </summary>
	public class GeoSpatialObject : IGeoSpatialObject
	{
		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		public GeoSpatialObject()
			: this(Guid.NewGuid().ToString(), null, true)
		{
		}

		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="Location">Location of object.</param>
		public GeoSpatialObject(GeoPosition Location)
			: this(Guid.NewGuid().ToString(), Location, true)
		{
		}

		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="GeoId">The ID of the geo-spatial object.</param>
		public GeoSpatialObject(string GeoId)
			: this(GeoId, null, true)
		{
		}

		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="GeoId">The ID of the geo-spatial object.</param>
		/// <param name="Location">Location of object.</param>
		public GeoSpatialObject(string GeoId, GeoPosition Location)
			: this(GeoId, Location, true)
		{
		}

		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="Location">String representation of location of object.
		/// <see cref="GeoPosition.Parse(string)"/> will be called to convert string
		/// to a <see cref="GeoPosition"/>, if possible.</param>
		/// <param name="Ephemeral">If the location of the geo-spatial object is ephemeral.</param>
		public GeoSpatialObject(string Location, bool Ephemeral)
			: this(Location, GeoPosition.Parse(Location), Ephemeral)
		{
		}

		/// <summary>
		/// A base class for geo-spatial objects, implementing the basic members of the
		/// <see cref="IGeoSpatialObject"/> interface.
		/// </summary>
		/// <param name="GeoId">The ID of the geo-spatial object.</param>
		/// <param name="Location">Location of object.</param>
		/// <param name="Ephemeral">If the location of the geo-spatial object is ephemeral.</param>
		public GeoSpatialObject(string GeoId, GeoPosition Location, bool Ephemeral)
		{
			this.GeoId = GeoId;
			this.Location = Location;
			this.EphemeralLocation = Ephemeral;
		}

		/// <summary>
		/// Location of object.
		/// </summary>
		public GeoPosition Location { get; set; }

		/// <summary>
		/// If the object has a geo-spatial location.
		/// </summary>
		public bool HasGeoLocation => !(this.Location is null);

		/// <summary>
		/// The ID of the geo-spatial object.
		/// </summary>
		public string GeoId { get; }

		/// <summary>
		/// Gets the geo-spatial location of the object.
		/// </summary>
		/// <returns>Geo-spatial location, if any, or null if none.</returns>
		public virtual Task<GeoPosition> GetLocation() => Task.FromResult(this.Location);

		/// <summary>
		/// If the location of the geo-spatial object is ephemeral.
		/// </summary>
		public bool EphemeralLocation { get; }

		/// <summary>
		/// Implicitly converts a <see cref="GeoSpatialObject"/> to a 
		/// <see cref="GeoSpatialObjectReference"/>.
		/// </summary>
		/// <param name="Object">Geo-spatial object reference</param>
		public static implicit operator GeoSpatialObjectReference(GeoSpatialObject Object)
		{
			if (Object is null)
				return null;
			else
				return new GeoSpatialObjectReference(Object);
		}
	}
}
