using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about the location of a device on the network.
	/// </summary>
	public class DeviceLocation
	{
		private UPnPClient client;
		private UPnPHeaders headers;
		private string searchTarget;
		private string server;
		private string location;
		private string uniqueServiceName;

		/// <summary>
		/// Contains information about the location of a device on the network.
		/// </summary>
		/// <param name="Client">UPnP Client.</param>
		/// <param name="SearchTarget">SSDP Search Target</param>
		/// <param name="Server">Server</param>
		/// <param name="Location">Location of device information</param>
		/// <param name="UniqueServiceName">Unique Service Name (USN)</param>
		/// <param name="Headers">All headers in response.</param>
		internal DeviceLocation(UPnPClient Client, string SearchTarget, string Server, string Location, string UniqueServiceName, UPnPHeaders Headers)
		{
			this.client = Client;
			this.searchTarget = SearchTarget;
			this.server = Server;
			this.location = Location;
			this.uniqueServiceName = UniqueServiceName;
			this.headers = Headers;
		}

		/// <summary>
		/// SSDP Search Target
		/// </summary>
		public string SearchTarget
		{ 
			get { return this.searchTarget; } 
		}

		/// <summary>
		/// Server
		/// </summary>
		public string Server
		{
			get { return this.server; }
		}

		/// <summary>
		/// Location of device information
		/// </summary>
		public string Location
		{
			get { return this.location; }
		}

		/// <summary>
		/// Unique Service Name (USN)
		/// </summary>
		public string UniqueServiceName
		{
			get { return this.uniqueServiceName; }
		}

		/// <summary>
		/// Gets the device description document from a device in the network. 
		/// This method is the synchronous version of <see cref="GetDeviceAsync"/>.
		/// </summary>
		/// <returns>Device Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public DeviceDescriptionDocument GetDevice()
		{
			return this.client.GetDevice(this.location);
		}

		/// <summary>
		/// Gets the device description document from a device in the network. 
		/// This method is the synchronous version of <see cref="GetDeviceAsync"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Device Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public DeviceDescriptionDocument GetDevice(int Timeout)
		{
			return this.client.GetDevice(this.location, Timeout);
		}

		/// <summary>
		/// Gets a Device Description Document from a device.
		/// </summary>
		/// <returns>Device description document, if found, or null otherwise.</returns>
		public Task<DeviceDescriptionDocument> GetDeviceAsync()
		{
			return this.client.GetDeviceAsync(this.location);
		}

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.location;
		}
	}
}
