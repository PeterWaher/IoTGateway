using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator client interface.
	/// 
	/// The interface is defined in XEP-0326:
	/// http://xmpp.org/extensions/xep-0326.html
	/// </summary>
	public class ConcentratorClient : IDisposable
	{
		private XmppClient client;

		/// <summary>
		/// urn:xmpp:iot:concentrators
		/// </summary>
		public const string NamespaceConcentrator = "urn:xmpp:iot:concentrators";

		/// <summary>
		/// Implements an XMPP concentrator client interface.
		/// 
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public ConcentratorClient(XmppClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			if (this.client != null)
			{
			}
		}

		/// <summary>
		/// Gets the capabilities of a concentrator server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetCapabilities(string To, CapabilitiesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getCapabilities xmlns='" + NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
				{
					List<string> Capabilities = new List<string>();
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getCapabilitiesResponse" && E.NamespaceURI == NamespaceConcentrator)
					{
						foreach (XmlNode N in E)
						{
							if (N.LocalName == "value")
								Capabilities.Add(N.InnerText);
						}
					}
					else
						e.Ok = false;

					try
					{
						Callback(this, new CapabilitiesEventArgs(Capabilities.ToArray(), e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
		}

		/// <summary>
		/// Gets all data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAllDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getAllDataSources xmlns='" + NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getAllDataSourcesResponse", Callback, State);
			}, State);
		}

		private void DataSourcesResponse(IqResultEventArgs e, string ExpectedElement, DataSourcesEventHandler Callback, object State)
		{
			List<DataSourceReference> DataSources = new List<DataSourceReference>();
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == ExpectedElement && E.NamespaceURI == NamespaceConcentrator)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "dataSource")
						DataSources.Add(new DataSourceReference(E2));
				}
			}
			else
				e.Ok = false;

			try
			{
				Callback(this, new DataSourcesEventArgs(DataSources.ToArray(), e));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRootDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getRootDataSources xmlns='" + NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getRootDataSourcesResponse", Callback, State);
			}, State);
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="SourceID">Parent Data Source ID.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetChildDataSources(string To, string SourceID, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getChildDataSources xmlns='" + NamespaceConcentrator + "' src='" + XML.Encode(SourceID) + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getChildDataSourcesResponse", Callback, State);
			}, State);
		}

	}
}
