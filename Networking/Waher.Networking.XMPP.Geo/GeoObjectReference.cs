using System;
using System.Xml;
using Waher.Runtime.Geo;

namespace Waher.Networking.XMPP.Geo
{
	/// <summary>
	/// Represents a reference to a geo-spatial object.
	/// </summary>
	public class GeoObjectReference
	{
		/// <summary>
		/// Represents a reference to a geo-spatial object.
		/// </summary>
		public GeoObjectReference()
		{
		}

		/// <summary>
		/// Geo-spatial object reference identifier.
		/// </summary>
		public string GeoId { get; set; }

		/// <summary>
		/// XML definition of object reference.
		/// </summary>
		public XmlElement Definition { get; set; }

		/// <summary>
		/// Bare JID of creator, if available.
		/// Publications resulting from internal processing, such as geo-localization of 
		/// contracts, or device registrations, will lack a creator registration.
		/// </summary>
		public string Creator { get; set; }

		/// <summary>
		/// Position of object
		/// </summary>
		public GeoPosition Position { get; set; }

		/// <summary>
		/// Number of seconds until the object expires, unless updated, if defined.
		/// </summary>
		public int? TimeToLive { get; set; }

		/// <summary>
		/// When information was created.
		/// Timestamps should use UTC.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		/// When information was last updated (if updated).
		/// Timestamps should use UTC.
		/// </summary>
		public DateTime? Updated { get; set; }

		/// <summary>
		/// Timestamp indicating from what point in time publication is valid.
		/// Can be used to publish a future location object reference.
		/// Timestamps should use UTC.
		/// </summary>
		public DateTime? From { get; set; }

		/// <summary>
		/// Timestamp indicating to what point in time publication is valid.
		/// Can be used to publish an expected expiration timestamp for a location object reference.
		/// Timestamps should use UTC.
		/// </summary>
		public DateTime? To { get; set; }
	}
}
