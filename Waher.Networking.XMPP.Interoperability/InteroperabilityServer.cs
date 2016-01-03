using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Implements the server-side for Interoperability interfaces, as defined in:
	/// 
	/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// </summary>
	public class InteroperabilityServer : IDisposable
	{
		/// <summary>
		/// urn:xmpp:iot:interoperability
		/// </summary>
		public const string NamespaceInteroperability = "urn:xmpp:iot:interoperability";

		private XmppClient client;

		/// <summary>
		/// Implements the server-side for Interoperability interfaces, as defined in:
		/// 
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public InteroperabilityServer(XmppClient Client)
		{
			this.client = Client;

			this.client.RegisterIqGetHandler("getInterfaces", NamespaceInteroperability, this.GetInterfacesHandler, true);
		}

		/// <summary>
		/// <see cref="Object.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqGetHandler("getInterfaces", NamespaceInteroperability, this.GetInterfacesHandler, true);
		}

		/// <summary>
		/// XMPP Client.
		/// </summary>
		public XmppClient Client { get { return this.client; } }

		private void GetInterfacesHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "nodeId");
			string SourceId = XML.Attribute(E, "sourceId");
			string CacheType = XML.Attribute(E, "cacheType");
			string ServiceToken = XML.Attribute(E, "serviceToken");
			string DeviceToken = XML.Attribute(E, "deviceToken");
			string UserToken = XML.Attribute(E, "userToken");

			InteroperabilityServerInterfacesEventHandler h = this.OnGetInterfaces;
			InteroperabilityServerEventArgs e2 = new InteroperabilityServerEventArgs(NodeId, SourceId, CacheType, ServiceToken, DeviceToken, UserToken);
			if (h != null)
			{
				try
				{
					h(this, e2);
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getInterfacesResponse xmlns='");
			Xml.Append(NamespaceInteroperability);
			Xml.Append("'>");

			foreach (string Interface in e2.Interfaces)
			{
				Xml.Append("<interface name='");
				Xml.Append(XML.Encode(Interface));
				Xml.Append("'/>");
			}

			Xml.Append("</getInterfacesResponse>");

			e.IqResult(Xml.ToString());
		}

		/// <summary>
		/// Event raised when a client requests supported interoperability interfaces.
		/// </summary>
		public event InteroperabilityServerInterfacesEventHandler OnGetInterfaces = null;

	}
}
