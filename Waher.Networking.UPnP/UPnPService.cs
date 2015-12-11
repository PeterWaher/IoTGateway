using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about a service.
	/// </summary>
	public class UPnPService
	{
		private UPnPClient client;
		private XmlElement xml;
		private string serviceType;
		private string serviceId;
		private string scpdURL;
		private string controlURL;
		private string eventSubURL;
		private Uri scpdURI;
		private Uri controlURI;
		private Uri eventSubURI;

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
		/// This method is the synchronous version of <see cref="StartGetService"/>.
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
		/// This method is the synchronous version of <see cref="StartGetService"/>.
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
		/// <param name="Callback">Callback method. Will be called when the document has been downloaded, or an error has occurred.</param>
		/// <param name="State">State object propagated to the callback method.</param>
		public void StartGetService(ServiceDescriptionEventHandler Callback, object State)
		{
			this.client.StartGetService(this, Callback, State);
		}
	}
}
