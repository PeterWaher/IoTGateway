using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about a service.
	/// </summary>
	public class UPnPService
	{
		private readonly UPnPClient client;
		private readonly XmlElement xml;
		private readonly string serviceType;
		private readonly string serviceId;
		private readonly string scpdURL;
		private readonly string controlURL;
		private readonly string eventSubURL;
		private readonly Uri scpdURI;
		private readonly Uri controlURI;
		private readonly Uri eventSubURI;

		/// <summary>
		/// Contains information about a service.
		/// </summary>
		/// <param name="Client">UPnP Client</param>
		/// <param name="ServiceType">Service type.</param>
		/// <param name="ServiceId">Service ID</param>
		/// <param name="ScpdUrl">URL to SCPD specification.</param>
		public UPnPService(UPnPClient Client, string ServiceType, string ServiceId, string ScpdUrl)
		{
			this.client = Client;
			this.xml = null;
			this.serviceType = ServiceType;
			this.serviceId = ServiceId;
			this.scpdURL = ScpdUrl;
			this.scpdURI = new Uri(ScpdUrl);
			this.controlURL = null;
			this.controlURI = null;
			this.eventSubURL = null;
			this.eventSubURI = null;
		}

		/// <summary>
		/// Contains information about a service.
		/// </summary>
		/// <param name="Xml">XML element</param>
		/// <param name="BaseUri">Base URI</param>
		/// <param name="Client">UPnP Client</param>
		public UPnPService(XmlElement Xml, Uri BaseUri, UPnPClient Client)
		{
			this.client = Client;
			this.xml = Xml;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "serviceType":
						this.serviceType = N.InnerText;
						break;

					case "serviceId":
						this.serviceId = N.InnerText;
						break;

					case "SCPDURL":
						this.scpdURL = N.InnerText;
						this.scpdURI = new Uri(BaseUri, this.scpdURL);
						break;

					case "controlURL":
						this.controlURL = N.InnerText;
						this.controlURI = new Uri(BaseUri, this.controlURL);
						break;

					case "eventSubURL":
						this.eventSubURL = N.InnerText;
						this.eventSubURI = new Uri(BaseUri, this.eventSubURL);
						break;
				}
			}
		}

		/// <summary>
		/// Underlying XML definition.
		/// </summary>
		public XmlElement Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// Service Type
		/// </summary>
		public string ServiceType { get { return this.serviceType; } }

		/// <summary>
		/// Service ID
		/// </summary>
		public string ServiceId { get { return this.serviceId; } }

		/// <summary>
		/// URL to service description
		/// </summary>
		public string SCPDURL { get { return this.scpdURL; } }

		/// <summary>
		/// URL for control
		/// </summary>
		public string ControlURL { get { return this.controlURL; } }

		/// <summary>
		/// URL for eventing
		/// </summary>
		public string EventSubURL { get { return this.eventSubURL; } }

		/// <summary>
		/// URI to service description
		/// </summary>
		public Uri SCPDURI { get { return this.scpdURI; } }

		/// <summary>
		/// URI for control
		/// </summary>
		public Uri ControlURI { get { return this.controlURI; } }

		/// <summary>
		/// URI for eventing
		/// </summary>
		public Uri EventSubURI { get { return this.eventSubURI; } }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			return this.serviceType;
		}

		/// <summary>
		/// Gets the service description document from a service in the network. 
		/// This method is the synchronous version of <see cref="GetServiceAsync()"/>.
		/// </summary>
		/// <returns>Service Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public ServiceDescriptionDocument GetService()
		{
			return this.client.GetService(this);
		}

		/// <summary>
		/// Gets the service description document from a service in the network. 
		/// This method is the synchronous version of <see cref="GetServiceAsync(int)"/>.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Service Description Document.</returns>
		/// <exception cref="TimeoutException">If the document could not be retrieved within the timeout time.</exception>
		/// <exception cref="Exception">If the document could not be retrieved, or could not be parsed.</exception>
		public ServiceDescriptionDocument GetService(int Timeout)
		{
			return this.client.GetService(this, Timeout);
		}

		/// <summary>
		/// Starts the retrieval of a Service Description Document.
		/// </summary>
		/// <returns>Service description document, if found, or null otherwise.</returns>
		public Task<ServiceDescriptionDocument> GetServiceAsync()
		{
			return this.client.GetServiceAsync(this);
		}

		/// <summary>
		/// Starts the retrieval of a Service Description Document.
		/// </summary>
		/// <param name="Timeout">Timeout, in milliseconds.</param>
		/// <returns>Service description document, if found, or null otherwise.</returns>
		public Task<ServiceDescriptionDocument> GetServiceAsync(int Timeout)
		{
			return this.client.GetServiceAsync(this, Timeout);
		}
	}
}
