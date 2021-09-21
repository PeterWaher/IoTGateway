using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Things.Metering
{
	/// <summary>
	/// Base class for metering nodes with interoperable meta-information.
	/// </summary>
	public abstract class MetaMeteringNode : MeteringNode
	{
		private string name = string.Empty;                     // NAME
		private string className = string.Empty;                // CLASS
		private string serialNumber = string.Empty;             // SN
		private string meterNumber = string.Empty;              // MNR
		private string meterLocation = string.Empty;            // MLOC
		private string manufacturerDomain = string.Empty;       // MAN
		private string model = string.Empty;                    // MODEL
		private string productUrl = string.Empty;               // PURL

		private string country = string.Empty;                  // COUNTRY
		private string region = string.Empty;                   // REGION
		private string city = string.Empty;                     // CITY
		private string area = string.Empty;                     // AREA
		private string street = string.Empty;                   // STREET
		private string streetNr = string.Empty;                 // STREETNR
		private string building = string.Empty;                 // BLD
		private string apartment = string.Empty;                // APT
		private string room = string.Empty;                     // ROOM

		private double? latitude = null;                        // LAT
		private double? longitude = null;                       // LON
		private double? altitude = null;                        // ALT
		private double? version = null;                         // V

		/// <summary>
		/// Base class for metering nodes with interoperable meta-information.
		/// </summary>
		public MetaMeteringNode()
			: base()
		{
		}

		/// <summary>
		/// If the node is provisioned is not. Property is editable.
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(28, "Name:", 10)]
		[ToolTip(29, "An additional name can be provided here.")]
		[DefaultValueStringEmpty]
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// A class name or the node.
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(31, "Class:", 20)]
		[ToolTip(32, "Class of node.")]
		[DefaultValueStringEmpty]
		public string Class
		{
			get { return this.className; }
			set { this.className = value; }
		}

		/// <summary>
		/// Serial number
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(34, "Serial number:", 30)]
		[ToolTip(35, "Serial number of device.")]
		[DefaultValueStringEmpty]
		public string SerialNumber
		{
			get { return this.serialNumber; }
			set { this.serialNumber = value; }
		}

		/// <summary>
		/// Meter number
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(37, "Meter number:", 40)]
		[ToolTip(38, "Additional identity of meter.")]
		[DefaultValueStringEmpty]
		public string MeterNumber
		{
			get { return this.meterNumber; }
			set { this.meterNumber = value; }
		}

		/// <summary>
		/// Meter number
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(40, "Meter location:", 50)]
		[ToolTip(41, "Identity of the location of the meter.")]
		[DefaultValueStringEmpty]
		public string MeterLocation
		{
			get { return this.meterLocation; }
			set { this.meterLocation = value; }
		}

		/// <summary>
		/// Manufacturer (domain)
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(43, "Manufacturer domain:", 60)]
		[ToolTip(44, "A domain controlled by the manufacturer.")]
		[DefaultValueStringEmpty]
		public string ManufacturerDomain
		{
			get { return this.manufacturerDomain; }
			set { this.manufacturerDomain = value; }
		}

		/// <summary>
		/// Model
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(46, "Model:", 70)]
		[ToolTip(47, "Model name of the device.")]
		[DefaultValueStringEmpty]
		public string Model
		{
			get { return this.model; }
			set { this.model = value; }
		}

		/// <summary>
		/// Version
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(78, "Version:", 80)]
		[ToolTip(79, "Version number of model of device.")]
		[DefaultValueNull]
		public double? Version
		{
			get { return this.version; }
			set { this.version = value; }
		}

		/// <summary>
		/// Product URL
		/// </summary>
		[Page(16, "Identity", 0)]
		[Header(49, "Product URL:", 90)]
		[ToolTip(50, "A URL with more information about the product.")]
		[DefaultValueStringEmpty]
		public string ProductUrl
		{
			get { return this.productUrl; }
			set { this.productUrl = value; }
		}

		/// <summary>
		/// Country
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(52, "Country:", 10)]
		[ToolTip(53, "Country of device.")]
		[DefaultValueStringEmpty]
		public string Country
		{
			get { return this.country; }
			set { this.country = value; }
		}

		/// <summary>
		/// Region
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(55, "Region:", 20)]
		[ToolTip(56, "Region of device.")]
		[DefaultValueStringEmpty]
		public string Region
		{
			get { return this.region; }
			set { this.region = value; }
		}

		/// <summary>
		/// City
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(58, "City:", 30)]
		[ToolTip(59, "City of device.")]
		[DefaultValueStringEmpty]
		public string City
		{
			get { return this.city; }
			set { this.city = value; }
		}

		/// <summary>
		/// Area
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(61, "Area:", 40)]
		[ToolTip(62, "Area of device.")]
		[DefaultValueStringEmpty]
		public string Area
		{
			get { return this.area; }
			set { this.area = value; }
		}

		/// <summary>
		/// Street
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(64, "Street:", 50)]
		[ToolTip(65, "Street device resides on.")]
		[DefaultValueStringEmpty]
		public string Street
		{
			get { return this.street; }
			set { this.street = value; }
		}

		/// <summary>
		/// Street number
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(67, "Street number:", 60)]
		[ToolTip(68, "Street number where device resides.")]
		[DefaultValueStringEmpty]
		public string StreetNr
		{
			get { return this.streetNr; }
			set { this.streetNr = value; }
		}

		/// <summary>
		/// Combination of both <see cref="Street"/> and <see cref="StreetNr"/>.
		/// </summary>
		public string StreetAndNr
		{
			get
			{
				if (string.IsNullOrEmpty(this.streetNr))
					return this.street;
				else if (string.IsNullOrEmpty(this.street))
					return this.streetNr;
				else
					return this.street + " " + this.streetNr;
			}
		}

		/// <summary>
		/// Building
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(69, "Building:", 70)]
		[ToolTip(70, "Building device resides in.")]
		[DefaultValueStringEmpty]
		public string Building
		{
			get { return this.building; }
			set { this.building = value; }
		}

		/// <summary>
		/// Apartment
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(72, "Apartment:", 80)]
		[ToolTip(73, "Apartment device resides in.")]
		[DefaultValueStringEmpty]
		public string Apartment
		{
			get { return this.apartment; }
			set { this.apartment = value; }
		}

		/// <summary>
		/// Room
		/// </summary>
		[Page(51, "Location", 10)]
		[Header(75, "Room:", 90)]
		[ToolTip(76, "Room device resides in.")]
		[DefaultValueStringEmpty]
		public string Room
		{
			get { return this.room; }
			set { this.room = value; }
		}

		/// <summary>
		/// Latitude
		/// </summary>
		[Page(81, "Position", 20)]
		[Header(82, "Latitude:", 10)]
		[ToolTip(83, "Latitude of device.")]
		[DefaultValueNull]
		public double? Latitude
		{
			get { return this.latitude; }
			set { this.latitude = value; }
		}

		/// <summary>
		/// Longitude
		/// </summary>
		[Page(81, "Position", 20)]
		[Header(84, "Longitude:", 20)]
		[ToolTip(85, "Longitude of device.")]
		[DefaultValueNull]
		public double? Longitude
		{
			get { return this.longitude; }
			set { this.longitude = value; }
		}

		/// <summary>
		/// Altitude
		/// </summary>
		[Page(81, "Position", 20)]
		[Header(86, "Altitude:", 30)]
		[ToolTip(87, "Altitude of device.")]
		[DefaultValueNull]
		public double? Altitude
		{
			get { return this.altitude; }
			set { this.altitude = value; }
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public override async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			if (!string.IsNullOrEmpty(this.name))
				Result.AddLast(new StringParameter("Name", await Language.GetStringAsync(typeof(MeteringTopology), 30, "Name"), this.name));

			if (!string.IsNullOrEmpty(this.className))
				Result.AddLast(new StringParameter("Class", await Language.GetStringAsync(typeof(MeteringTopology), 33, "Class"), this.className));

			if (!string.IsNullOrEmpty(this.serialNumber))
				Result.AddLast(new StringParameter("SerialNumber", await Language.GetStringAsync(typeof(MeteringTopology), 36, "S/N"), this.serialNumber));

			if (!string.IsNullOrEmpty(this.meterNumber))
				Result.AddLast(new StringParameter("MeterNumber", await Language.GetStringAsync(typeof(MeteringTopology), 39, "Meter Nr."), this.meterNumber));

			if (!string.IsNullOrEmpty(this.meterLocation))
				Result.AddLast(new StringParameter("MeterLocation", await Language.GetStringAsync(typeof(MeteringTopology), 42, "Meter Loc."), this.meterLocation));

			if (!string.IsNullOrEmpty(this.manufacturerDomain))
				Result.AddLast(new StringParameter("Manufacturer", await Language.GetStringAsync(typeof(MeteringTopology), 45, "Manufacturer"), this.manufacturerDomain));

			if (!string.IsNullOrEmpty(this.model))
				Result.AddLast(new StringParameter("Model", await Language.GetStringAsync(typeof(MeteringTopology), 48, "Model"), this.model));

			if (this.version.HasValue)
				Result.AddLast(new StringParameter("Version", await Language.GetStringAsync(typeof(MeteringTopology), 80, "Version"), this.version.ToString()));

			if (!string.IsNullOrEmpty(this.country))
				Result.AddLast(new StringParameter("Country", await Language.GetStringAsync(typeof(MeteringTopology), 54, "Country"), this.country));

			if (!string.IsNullOrEmpty(this.region))
				Result.AddLast(new StringParameter("Region", await Language.GetStringAsync(typeof(MeteringTopology), 57, "Region"), this.region));

			if (!string.IsNullOrEmpty(this.city))
				Result.AddLast(new StringParameter("City", await Language.GetStringAsync(typeof(MeteringTopology), 60, "City"), this.city));

			if (!string.IsNullOrEmpty(this.area))
				Result.AddLast(new StringParameter("Area", await Language.GetStringAsync(typeof(MeteringTopology), 63, "Area"), this.area));

			string s = this.StreetAndNr;
			if (!string.IsNullOrEmpty(s))
				Result.AddLast(new StringParameter("Street", await Language.GetStringAsync(typeof(MeteringTopology), 66, "Street"), s));

			if (!string.IsNullOrEmpty(this.building))
				Result.AddLast(new StringParameter("Building", await Language.GetStringAsync(typeof(MeteringTopology), 71, "Building"), this.building));

			if (!string.IsNullOrEmpty(this.apartment))
				Result.AddLast(new StringParameter("Apartment", await Language.GetStringAsync(typeof(MeteringTopology), 74, "Apartment"), this.apartment));

			if (!string.IsNullOrEmpty(this.room))
				Result.AddLast(new StringParameter("Room", await Language.GetStringAsync(typeof(MeteringTopology), 77, "Room"), this.room));

			if (this.latitude.HasValue)
				Result.AddLast(new StringParameter("Latitude", await Language.GetStringAsync(typeof(MeteringTopology), 88, "Latitude"), this.latitude.ToString()));

			if (this.longitude.HasValue)
				Result.AddLast(new StringParameter("Longitude", await Language.GetStringAsync(typeof(MeteringTopology), 89, "Longitude"), this.longitude.ToString()));

			if (this.altitude.HasValue)
				Result.AddLast(new StringParameter("Altitude", await Language.GetStringAsync(typeof(MeteringTopology), 90, "Altitude"), this.altitude.ToString()));

			return Result;
		}

		/// <summary>
		/// Gets meta-data about the node.
		/// </summary>
		/// <returns>Meta data.</returns>
		public override async Task<KeyValuePair<string, object>[]> GetMetaData()
		{
			List<KeyValuePair<string, object>> Result = new List<KeyValuePair<string, object>>(await base.GetMetaData());

			if (!string.IsNullOrEmpty(this.name))
				Result.Add(new KeyValuePair<string, object>("NAME", this.name));

			if (!string.IsNullOrEmpty(this.className))
				Result.Add(new KeyValuePair<string, object>("CLASS", this.className));

			if (!string.IsNullOrEmpty(this.serialNumber))
				Result.Add(new KeyValuePair<string, object>("SN", this.serialNumber));

			if (!string.IsNullOrEmpty(this.meterNumber))
				Result.Add(new KeyValuePair<string, object>("MNR", this.meterNumber));

			if (!string.IsNullOrEmpty(this.meterLocation))
				Result.Add(new KeyValuePair<string, object>("MLOC", this.meterLocation));

			if (!string.IsNullOrEmpty(this.manufacturerDomain))
				Result.Add(new KeyValuePair<string, object>("MAN", this.manufacturerDomain));

			if (!string.IsNullOrEmpty(this.model))
				Result.Add(new KeyValuePair<string, object>("MODEL", this.model));

			if (this.version.HasValue)
				Result.Add(new KeyValuePair<string, object>("V", this.version.Value));

			if (!string.IsNullOrEmpty(this.productUrl))
				Result.Add(new KeyValuePair<string, object>("PURL", this.productUrl));

			if (!string.IsNullOrEmpty(this.country))
				Result.Add(new KeyValuePair<string, object>("COUNTRY", this.country));

			if (!string.IsNullOrEmpty(this.region))
				Result.Add(new KeyValuePair<string, object>("REGION", this.region));

			if (!string.IsNullOrEmpty(this.city))
				Result.Add(new KeyValuePair<string, object>("CITY", this.city));

			if (!string.IsNullOrEmpty(this.area))
				Result.Add(new KeyValuePair<string, object>("AREA", this.area));

			if (!string.IsNullOrEmpty(this.street))
				Result.Add(new KeyValuePair<string, object>("STREET", this.street));

			if (!string.IsNullOrEmpty(this.streetNr))
				Result.Add(new KeyValuePair<string, object>("STREETNR", this.streetNr));

			if (!string.IsNullOrEmpty(this.building))
				Result.Add(new KeyValuePair<string, object>("BLD", this.building));

			if (!string.IsNullOrEmpty(this.apartment))
				Result.Add(new KeyValuePair<string, object>("APT", this.apartment));

			if (!string.IsNullOrEmpty(this.room))
				Result.Add(new KeyValuePair<string, object>("ROOM", this.room));

			if (this.latitude.HasValue)
				Result.Add(new KeyValuePair<string, object>("LAT", this.latitude.Value));

			if (this.longitude.HasValue)
				Result.Add(new KeyValuePair<string, object>("LON", this.longitude.Value));

			if (this.altitude.HasValue)
				Result.Add(new KeyValuePair<string, object>("ALT", this.altitude.Value));

			return Result.ToArray();
		}

		/// <summary>
		/// Adds defined identity fields to a sensor data readout.
		/// </summary>
		/// <param name="Fields">List of fields being constructed.</param>
		/// <param name="Now">Timestamp of readout.</param>
		public virtual void AddIdentityReadout(List<Field> Fields, DateTime Now)
		{
			string Module = typeof(MeteringTopology).Namespace;

			if (!string.IsNullOrEmpty(this.name))
				Fields.Add(new StringField(this, Now, "Name", this.name, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 30));

			if (!string.IsNullOrEmpty(this.className))
				Fields.Add(new StringField(this, Now, "Class", this.className, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 33));

			if (!string.IsNullOrEmpty(this.serialNumber))
				Fields.Add(new StringField(this, Now, "Serial Number", this.serialNumber, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 91));

			if (!string.IsNullOrEmpty(this.meterNumber))
				Fields.Add(new StringField(this, Now, "Meter Number", this.meterNumber, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 92));

			if (!string.IsNullOrEmpty(this.meterLocation))
				Fields.Add(new StringField(this, Now, "Meter Location", this.meterLocation, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 93));

			if (!string.IsNullOrEmpty(this.manufacturerDomain))
				Fields.Add(new StringField(this, Now, "Manufacturer", this.manufacturerDomain, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 45));

			if (!string.IsNullOrEmpty(this.model))
				Fields.Add(new StringField(this, Now, "Model", this.model, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 48));

			if (this.version.HasValue)
				Fields.Add(new QuantityField(this, Now, "Version", this.version.Value, CommonTypes.GetNrDecimals(this.version.Value), string.Empty, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 80));

			if (!string.IsNullOrEmpty(this.country))
				Fields.Add(new StringField(this, Now, "Country", this.country, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 54));

			if (!string.IsNullOrEmpty(this.region))
				Fields.Add(new StringField(this, Now, "Region", this.region, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 57));

			if (!string.IsNullOrEmpty(this.city))
				Fields.Add(new StringField(this, Now, "City", this.city, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 60));

			if (!string.IsNullOrEmpty(this.area))
				Fields.Add(new StringField(this, Now, "Area", this.area, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 63));

			string s = this.StreetAndNr;
			if (!string.IsNullOrEmpty(s))
				Fields.Add(new StringField(this, Now, "Street", s, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 66));

			if (!string.IsNullOrEmpty(this.building))
				Fields.Add(new StringField(this, Now, "Building", this.building, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 71));

			if (!string.IsNullOrEmpty(this.apartment))
				Fields.Add(new StringField(this, Now, "Apartment", this.apartment, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 74));

			if (!string.IsNullOrEmpty(this.room))
				Fields.Add(new StringField(this, Now, "Room", this.room, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 77));

			if (this.latitude.HasValue)
				Fields.Add(new QuantityField(this, Now, "Latitude", this.latitude.Value, CommonTypes.GetNrDecimals(this.latitude.Value), string.Empty, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 88));

			if (this.longitude.HasValue)
				Fields.Add(new QuantityField(this, Now, "Longitude", this.longitude.Value, CommonTypes.GetNrDecimals(this.longitude.Value), string.Empty, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 89));

			if (this.altitude.HasValue)
				Fields.Add(new QuantityField(this, Now, "Altitude", this.altitude.Value, CommonTypes.GetNrDecimals(this.altitude.Value), string.Empty, FieldType.Identity, FieldQoS.AutomaticReadout, Module, 90));
		}

	}
}
