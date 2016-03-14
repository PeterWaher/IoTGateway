using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator server interface.
	/// 
	/// The interface is defined in XEP-0326:
	/// http://xmpp.org/extensions/xep-0326.html
	/// </summary>
	public class ConcentratorServer : IDisposable
	{
		/// <summary>
		/// urn:xmpp:iot:concentrators
		/// </summary>
		public const string NamespaceConcentrator = "urn:xmpp:iot:concentrators";

		private XmppClient client;

		/// <summary>
		/// Implements an XMPP concentrator server interface.
		/// 
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public ConcentratorServer(XmppClient Client)
		{
			this.client = Client;

			this.client.RegisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		private void GetCapabilitiesHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();
			using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(false, true)))
			{
				w.WriteStartElement("getCapabilitiesResponse", NamespaceConcentrator);
				w.WriteAttributeString("result", "OK");

				w.WriteElementString("value", "getCapabilities");

				w.WriteEndElement();
				w.Flush();
			}

			e.IqResult(Xml.ToString());
		}

	}
}
