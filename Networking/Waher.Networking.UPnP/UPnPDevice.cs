using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about a device.
	/// </summary>
	public class UPnPDevice
	{
		private XmlElement xml;
		private string deviceType;
		private string friendlyName;
		private string manufacturer;
		private string manufacturerURL;
		private string modelDescription;
		private string modelName;
		private string modelNumber;
		private string modelURL;
		private string serialNumber;
		private string udn;
		private string upc;
		private UPnPIcon[] icons;
		private UPnPService[] services;
		private UPnPDevice[] devices;
		private string presentationURL;
		private Uri manufacturerURI;
		private Uri modelURI;
		private Uri presentationURI;

		internal UPnPDevice(XmlElement Xml, Uri BaseUri, UPnPClient Client)
		{
			List<UPnPIcon> Icons = new List<UPnPIcon>();
			List<UPnPService> Services = new List<UPnPService>();
			List<UPnPDevice> Devices = new List<UPnPDevice>();

			this.xml = Xml;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "deviceType":
						this.deviceType = N.InnerText;
						break;

					case "friendlyName":
						this.friendlyName = N.InnerText;
						break;

					case "manufacturer":
						this.manufacturer = N.InnerText;
						break;

					case "manufacturerURL":
						this.manufacturerURL = N.InnerText;
						this.manufacturerURI = new Uri(BaseUri, this.manufacturerURL);
						break;

					case "modelDescription":
						this.modelDescription = N.InnerText;
						break;

					case "modelName":
						this.modelName = N.InnerText;
						break;

					case "modelNumber":
						this.modelNumber = N.InnerText;
						break;

					case "modelURL":
						this.modelURL = N.InnerText;
						this.modelURI = new Uri(BaseUri, this.modelURL);
						break;

					case "serialNumber":
						this.serialNumber = N.InnerText;
						break;

					case "UDN":
						this.udn = N.InnerText;
						break;

					case "UPC":
						this.upc = N.InnerText;
						break;

					case "iconList":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "icon")
								Icons.Add(new UPnPIcon((XmlElement)N2, BaseUri));
						}
						break;

					case "serviceList":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "service")
								Services.Add(new UPnPService((XmlElement)N2, BaseUri, Client));
						}
						break;

					case "deviceList":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "device")
								Devices.Add(new UPnPDevice((XmlElement)N2, BaseUri, Client));
						}
						break;

					case "presentationURL":
						this.presentationURL = N.InnerText;
						this.presentationURI = new Uri(BaseUri, this.presentationURL);
						break;
				}
			}

			this.icons = Icons.ToArray();
			this.services = Services.ToArray();
			this.devices = Devices.ToArray();
		}

		/// <summary>
		/// Underlying XML definition.
		/// </summary>
		public XmlElement Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// Device type
		/// </summary>
		public string DeviceType { get { return this.deviceType; } }

		/// <summary>
		/// Short user-friendly title
		/// </summary>
		public string FriendlyName { get { return this.friendlyName; } }

		/// <summary>
		/// Manufacturer name
		/// </summary>
		public string Manufacturer { get { return this.manufacturer; } }

		/// <summary>
		/// URL to manufacturer site
		/// </summary>
		public string ManufacturerURL { get { return this.manufacturerURL; } }

		/// <summary>
		/// Long user-friendly title
		/// </summary>
		public string ModelDescription { get { return this.modelDescription; } }

		/// <summary>
		/// Model name
		/// </summary>
		public string ModelName { get { return this.modelName; } }

		/// <summary>
		/// Model number
		/// </summary>
		public string ModelNumber { get { return this.modelNumber; } }

		/// <summary>
		/// URL to model site
		/// </summary>
		public string ModelURL { get { return this.modelURL; } }

		/// <summary>
		/// Manufacturer's serial number
		/// </summary>
		public string SerialNumber { get { return this.serialNumber; } }

		/// <summary>
		/// Unique Device Name (uuid:UUID)
		/// </summary>
		public string UDN { get { return this.udn; } }

		/// <summary>
		/// Universal Product Code
		/// </summary>
		public string UPC { get { return this.upc; } }

		/// <summary>
		/// Icons for the device.
		/// </summary>
		public UPnPIcon[] Icons { get { return this.icons; } }

		/// <summary>
		/// Services published by the device.
		/// </summary>
		public UPnPService[] Services { get { return this.services; } }

		/// <summary>
		/// Embedded devices.
		/// </summary>
		public UPnPDevice[] Devices { get { return this.devices; } }

		/// <summary>
		/// URL for presentation
		/// </summary>
		public string PresentationURL { get { return this.presentationURL; } }

		/// <summary>
		/// URI to manufacturer site
		/// </summary>
		public Uri ManufacturerURI { get { return this.manufacturerURI; } }

		/// <summary>
		/// URI to model site
		/// </summary>
		public Uri ModelURI { get { return this.modelURI; } }

		/// <summary>
		/// URI for presentation
		/// </summary>
		public Uri PresentationURI { get { return this.presentationURI; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.friendlyName;
		}

		/// <summary>
		/// Gets a device or embedded device, given its device type.
		/// </summary>
		/// <param name="DeviceType">Device type.</param>
		/// <returns>Device object, if found, null otherwise.</returns>
		public UPnPDevice GetDevice(string DeviceType)
		{
			UPnPDevice Result = null;

			if (this.deviceType == DeviceType)
				Result = this;
			else
			{
				foreach (UPnPDevice Device in this.devices)
				{
					Result = Device.GetDevice(DeviceType);
					if (!(Result is null))
						break;
				}
			}

			return Result;
		}

		/// <summary>
		/// Gets a service, given its service type.
		/// </summary>
		/// <param name="ServiceType">Service type.</param>
		/// <returns>Service object, if found, null otherwise.</returns>
		public UPnPService GetService(string ServiceType)
		{
			UPnPService Result = null;

			foreach (UPnPService Service in this.services)
			{
				if (Service.ServiceType == ServiceType)
					return Service;
			}

			foreach (UPnPDevice Device in this.devices)
			{
				Result = Device.GetService(ServiceType);
				if (!(Result is null))
					return Result;
			}

			return null;
		}

		/// <summary>
		/// Returns all devices, including the device itself and its embedded devices, and their embedded devices, and so on.
		/// </summary>
		public UPnPDevice[] DevicesRecursive
		{
			get
			{
				List<UPnPDevice> Result = new List<UPnPDevice>();

				Result.Add(this);
				foreach (UPnPDevice Device in this.devices)
					Result.AddRange(Device.DevicesRecursive);

				return Result.ToArray();
			}
		}

		/// <summary>
		/// Returns all services, including the service of itself and services of its embedded devices, and their embedded devices, and so on.
		/// </summary>
		public UPnPService[] ServicesRecursive
		{
			get
			{
				List<UPnPService> Result = new List<UPnPService>();

				Result.AddRange(this.services);
				foreach (UPnPDevice Device in this.devices)
					Result.AddRange(Device.ServicesRecursive);

				return Result.ToArray();
			}
		}

	}
}
