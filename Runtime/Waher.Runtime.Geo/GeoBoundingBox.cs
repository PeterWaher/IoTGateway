using System;
using System.Text;

namespace Waher.Runtime.Geo
{
	/// <summary>
	/// Contains information about a geo-spatial bounding box using the Mercator Projection.
	/// </summary>
	public sealed class GeoBoundingBox : IGeoBoundingBox
	{
		private readonly GeoPosition min;
		private readonly GeoPosition max;
		private readonly string boxId;
		private readonly bool includeMin;
		private readonly bool includeMax;

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		public GeoBoundingBox(GeoPosition Min, GeoPosition Max)
			: this(Min, Max, true, true)
		{
		}

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <param name="IncludeMin">If the min latitude/longitude/altitude should be considered as included in the boundoing box.</param>
		/// <param name="IncludeMax">If the max latitude/longitude/altitude should be considered as included in the boundoing box.</param>
		public GeoBoundingBox(GeoPosition Min, GeoPosition Max, bool IncludeMin, bool IncludeMax)
			: this(Guid.NewGuid().ToString(), Min, Max, IncludeMin, IncludeMax)
		{
		}

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="BoxId">The ID of the geo-spatial object.</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		public GeoBoundingBox(string BoxId, GeoPosition Min, GeoPosition Max)
			: this(BoxId, Min, Max, true, true)
		{
		}

		/// <summary>
		/// Contains information about a position in a geo-spatial coordinate system.
		/// </summary>
		/// <param name="BoxId">The ID of the geo-spatial object.</param>
		/// <param name="Min">Lower-left corner of bounding box.</param>
		/// <param name="Max">Upper-right corner of bounding box.</param>
		/// <param name="IncludeMin">If the min latitude/longitude/altitude should be considered as included in the boundoing box.</param>
		/// <param name="IncludeMax">If the max latitude/longitude/altitude should be considered as included in the boundoing box.</param>
		public GeoBoundingBox(string BoxId, GeoPosition Min, GeoPosition Max, bool IncludeMin, bool IncludeMax)
		{
			this.boxId = BoxId;
			this.min = Min;
			this.max = Max;
			this.includeMin = IncludeMin;
			this.includeMax = IncludeMax;
		}
		/// <summary>
		/// The ID of the geo-spatial bounding box.
		/// </summary>
		public string BoxId => this.boxId;

		/// <summary>
		/// Lower-left corner of bounding box.
		/// </summary>
		public GeoPosition Min => this.min;

		/// <summary>
		/// Upper-right corner of bounding box.
		/// </summary>
		public GeoPosition Max => this.max;

		/// <summary>
		/// If the min latitude/longitude/altitude should be considered as included in the boundoing box.
		/// </summary>
		public bool IncludeMin => this.includeMin;

		/// <summary>
		/// If the max latitude/longitude/altitude should be considered as included in the boundoing box.
		/// </summary>
		public bool IncludeMax => this.includeMax;

		/// <summary>
		/// If the bounding box is longitude-wrapped, i.e. if the box passes the +-180 degrees
		/// longitude.
		/// </summary>
		public bool LongitudeWrapped
		{
			get
			{
				return
					!(this.min is null) &&
					!(this.max is null) &&
					this.min.Longitude > this.max.Longitude;
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.includeMin)
				sb.Append('[');
			else
				sb.Append('(');

			sb.Append(this.min?.ToString());
			sb.Append(" - ");
			sb.Append(this.max?.ToString());

			if (this.includeMax)
				sb.Append(']');
			else
				sb.Append(')');

			return sb.ToString();
		}

		/// <inheritdoc/>
		public string ToNormalizedString()
		{
			StringBuilder sb = new StringBuilder();

			if (this.includeMin)
				sb.Append('[');
			else
				sb.Append('(');

			sb.Append(this.min?.NormalizedValue);
			sb.Append(" - ");
			sb.Append(this.max?.NormalizedValue);

			if (this.includeMax)
				sb.Append(']');
			else
				sb.Append(')');

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return
				obj is GeoBoundingBox Obj &&
				(this.min?.Equals(Obj.Min) ?? Obj.Min is null) &&
				(this.max?.Equals(Obj.Max) ?? Obj.Max is null) &&
				this.includeMin == Obj.includeMin &&
				this.includeMax == Obj.includeMax;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = this.min?.GetHashCode() ?? 0;
			Result ^= Result << 5 ^ (this.max?.GetHashCode() ?? 0);
			Result ^= Result << 5 ^ this.includeMin.GetHashCode();
			Result ^= Result << 5 ^ this.includeMax.GetHashCode();

			return Result;
		}

		/// <summary>
		/// Checks if two geo-spatial bounding boxes are equal.
		/// </summary>
		/// <param name="A">Geo-position A</param>
		/// <param name="B">Geo-position B</param>
		/// <returns>If they are equal</returns>
		public static bool operator ==(GeoBoundingBox A, GeoBoundingBox B)
		{
			if (A is null ^ B is null)
				return false;

			if (A is null)
				return true;

			return A.Equals(B);
		}

		/// <summary>
		/// Checks if two geo-spatial bounding boxes are not equal.
		/// </summary>
		/// <param name="A">Geo-position A</param>
		/// <param name="B">Geo-position B</param>
		/// <returns>If they are equal</returns>
		public static bool operator !=(GeoBoundingBox A, GeoBoundingBox B)
		{
			if (A is null ^ B is null)
				return true;

			if (A is null)
				return false;

			return !A.Equals(B);
		}

		/// <summary>
		/// If the bounding box contains a location.
		/// </summary>
		/// <param name="Location">Location</param>
		/// <returns>If the bounding box contains the location.</returns>
		public bool Contains(GeoPosition Location)
		{
			return !Location.LiesOutside(this.min, this.max, !this.includeMin, !this.includeMax);
		}

		/// <summary>
		/// If the bounding box contains a location.
		/// </summary>
		/// <param name="Location">Location</param>
		/// <param name="IgnoreAltitude">If altitude component should be ignored.</param>
		/// <returns>If the bounding box contains the location.</returns>
		public bool Contains(GeoPosition Location, bool IgnoreAltitude)
		{
			return !Location.LiesOutside(this.min, this.max, !this.includeMin, !this.includeMax, IgnoreAltitude);
		}
	}
}
