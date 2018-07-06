using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.PEP
{
	/// <summary>
	/// User Location event, as defined in XEP-0080:
	/// https://xmpp.org/extensions/xep-0080.html
	/// </summary>
	public class UserLocation : PersonalEvent
	{
		private decimal? accuracy = null;
		private decimal? alt = null;
		private decimal? altaccuracy = null;
		private string area = null;
		private decimal? bearing = null;
		private string building = null;
		private string country = null;
		private string countrycode = null;
		private string datum = null;
		private string description = null;
		private string floor = null;
		private decimal? lat = null;
		private string locality = null;
		private decimal? lon = null;
		private string postalcode = null;
		private string region = null;
		private string room = null;
		private decimal? speed = null;
		private string street = null;
		private string text = null;
		private DateTime? timestamp = null;
		private string tzo = null;
		private Uri uri = null;

		/// <summary>
		/// User Location event, as defined in XEP-0080:
		/// https://xmpp.org/extensions/xep-0080.html
		/// </summary>
		public UserLocation()
		{
		}

		/// <summary>
		/// Local name of the event element.
		/// </summary>
		public override string LocalName => "geoloc";

		/// <summary>
		/// Namespace of the event element.
		/// </summary>
		public override string Namespace => "http://jabber.org/protocol/geoloc";

		/// <summary>
		/// XML representation of the event.
		/// </summary>
		public override string PayloadXml
		{
			get
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<geoloc xmlns='");
				Xml.Append(this.Namespace);
				Xml.Append("'>");

				this.Append(Xml, "accuracy", this.accuracy);
				this.Append(Xml, "alt", this.alt);
				this.Append(Xml, "altaccuracy", this.altaccuracy);
				this.Append(Xml, "area", this.area);
				this.Append(Xml, "bearing", this.bearing);
				this.Append(Xml, "building", this.building);
				this.Append(Xml, "country", this.country);
				this.Append(Xml, "countrycode", this.countrycode);
				this.Append(Xml, "datum", this.datum);
				this.Append(Xml, "description", this.description);
				this.Append(Xml, "floor", this.floor);
				this.Append(Xml, "lat", this.lat);
				this.Append(Xml, "locality", this.locality);
				this.Append(Xml, "lon", this.lon);
				this.Append(Xml, "postalcode", this.postalcode);
				this.Append(Xml, "region", this.region);
				this.Append(Xml, "room", this.room);
				this.Append(Xml, "speed", this.speed);
				this.Append(Xml, "street", this.street);
				this.Append(Xml, "text", this.text);
				this.Append(Xml, "timestamp", this.timestamp);
				this.Append(Xml, "tzo", this.tzo);
				this.Append(Xml, "uri", this.uri?.ToString());

				Xml.Append("</geoloc>");

				return Xml.ToString();
			}
		}
		
		/// <summary>
		/// Parses a personal event from its XML representation
		/// </summary>
		/// <param name="E">XML representation of personal event.</param>
		/// <returns>Personal event object.</returns>
		public override IPersonalEvent Parse(XmlElement E)
		{
			UserLocation Result = new UserLocation();

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "accuracy":
							if (CommonTypes.TryParse(E2.InnerText, out decimal d))
								Result.accuracy = d;
							break;

						case "alt":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.alt = d;
							break;

						case "altaccuracy":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.altaccuracy = d;
							break;

						case "area":
							Result.area = E2.InnerText;
							break;

						case "bearing":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.bearing = d;
							break;

						case "building":
							Result.building = E2.InnerText;
							break;

						case "country":
							Result.country = E2.InnerText;
							break;

						case "countrycode":
							Result.countrycode = E2.InnerText;
							break;

						case "datum":
							Result.datum = E2.InnerText;
							break;

						case "description":
							Result.description = E2.InnerText;
							break;

						case "error":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.accuracy = d;
							break;

						case "floor":
							Result.floor = E2.InnerText;
							break;

						case "lat":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.lat = d;
							break;

						case "locality":
							Result.locality = E2.InnerText;
							break;

						case "lon":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.lon = d;
							break;

						case "postalcode":
							Result.postalcode = E2.InnerText;
							break;

						case "region":
							Result.region = E2.InnerText;
							break;

						case "room":
							Result.room = E2.InnerText;
							break;

						case "speed":
							if (CommonTypes.TryParse(E2.InnerText, out d))
								Result.speed = d;
							break;

						case "street":
							Result.street = E2.InnerText;
							break;

						case "text":
							Result.text = E2.InnerText;
							break;

						case "timestamp":
							if (XML.TryParse(E2.InnerText, out DateTime TP))
								Result.timestamp = TP;
							break;

						case "tzo":
							Result.tzo = E2.InnerText;
							break;

						case "uri":
							try
							{
								Result.uri = new Uri(E2.InnerText);
							}
							catch (Exception)
							{
								// Ignore.
							}
							break;
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// Horizontal GPS error in meters.
		/// </summary>
		public decimal? Accuracy
		{
			get => this.accuracy;
			set => this.accuracy = value;
		}

		/// <summary>
		/// Altitude in meters above or below sea level
		/// </summary>
		public decimal? Alt
		{
			get => this.alt;
			set => this.alt = value;
		}

		/// <summary>
		/// Vertical GPS error in meters
		/// </summary>
		public decimal? AltAccuracy
		{
			get => this.altaccuracy;
			set => this.altaccuracy = value;
		}

		/// <summary>
		/// A named area such as a campus or neighborhood
		/// </summary>
		public string Area
		{
			get => this.area;
			set => this.area = value;
		}

		/// <summary>
		/// GPS bearing (direction in which the entity is heading to reach its next waypoint), measured in decimal degrees relative to true north
		/// </summary>
		public decimal? Bearing
		{
			get => this.bearing;
			set => this.bearing = value;
		}

		/// <summary>
		/// A specific building on a street or in an area
		/// </summary>
		public string Building
		{
			get => this.building;
			set => this.building = value;
		}

		/// <summary>
		/// The nation where the user is located
		/// </summary>
		public string Country
		{
			get => this.country;
			set => this.country = value;
		}

		/// <summary>
		/// The ISO 3166 two-letter country code
		/// </summary>
		public string CountryCode
		{
			get => this.countrycode;
			set => this.countrycode = value;
		}

		/// <summary>
		/// GPS datum
		/// </summary>
		public string Datum
		{
			get => this.datum;
			set => this.datum = value;
		}

		/// <summary>
		/// A natural-language name for or description of the location
		/// </summary>
		public string Description
		{
			get => this.description;
			set => this.description = value;
		}

		/// <summary>
		/// A particular floor in a building
		/// </summary>
		public string Floor
		{
			get => this.floor;
			set => this.floor = value;
		}

		/// <summary>
		/// Latitude in decimal degrees North
		/// </summary>
		public decimal? Lat
		{
			get => this.lat;
			set => this.lat = value;
		}

		/// <summary>
		/// A locality within the administrative region, such as a town or city
		/// </summary>
		public string Locality
		{
			get => this.locality;
			set => this.locality = value;
		}

		/// <summary>
		/// Longitude in decimal degrees East
		/// </summary>
		public decimal? Lon
		{
			get => this.lon;
			set => this.lon = value;
		}

		/// <summary>
		/// A code used for postal delivery
		/// </summary>
		public string PostalCode
		{
			get => this.postalcode;
			set => this.postalcode = value;
		}

		/// <summary>
		/// An administrative region of the nation, such as a state or province
		/// </summary>
		public string Region
		{
			get => this.region;
			set => this.region = value;
		}

		/// <summary>
		/// A particular room in a building
		/// </summary>
		public string Room
		{
			get => this.room;
			set => this.room = value;
		}

		/// <summary>
		/// The speed at which the entity is moving, in meters per second
		/// </summary>
		public decimal? Speed
		{
			get => this.speed;
			set => this.speed = value;
		}

		/// <summary>
		/// A thoroughfare within the locality, or a crossing of two thoroughfares
		/// </summary>
		public string Street
		{
			get => this.street;
			set => this.street = value;
		}

		/// <summary>
		/// A catch-all element that captures any other information about the location
		/// </summary>
		public string Text
		{
			get => this.text;
			set => this.text = value;
		}

		/// <summary>
		/// UTC timestamp specifying the moment when the reading was taken.
		/// </summary>
		public DateTime? Timestamp
		{
			get => this.timestamp;
			set => this.timestamp = value;
		}

		/// <summary>
		/// The time zone offset from UTC for the current location
		/// </summary>
		public string TimeZone
		{
			get => this.tzo;
			set => this.tzo = value;
		}

		/// <summary>
		/// A URI or URL pointing to information about the location
		/// </summary>
		public Uri Uri
		{
			get => this.uri;
			set => this.uri = value;
		}

	}
}
