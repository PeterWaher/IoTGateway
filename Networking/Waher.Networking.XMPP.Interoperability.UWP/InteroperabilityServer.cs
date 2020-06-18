using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Implements the server-side for Interoperability interfaces, as defined in:
	/// 
	/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// </summary>
	public class InteroperabilityServer : XmppExtension
	{
		/// <summary>
		/// urn:xmpp:iot:interoperability
		/// </summary>
		public const string NamespaceInteroperability = "urn:xmpp:iot:interoperability";

		/// <summary>
		/// Implements the server-side for Interoperability interfaces, as defined in:
		/// 
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public InteroperabilityServer(XmppClient Client)
			: base(Client)
		{
			this.client.RegisterIqGetHandler("getInterfaces", NamespaceInteroperability, this.GetInterfacesHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			this.client.UnregisterIqGetHandler("getInterfaces", NamespaceInteroperability, this.GetInterfacesHandler, true);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[0];

		private async Task GetInterfacesHandler(object Sender, IqEventArgs e)
		{
			XmlElement E = e.Query;
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			string ServiceToken = XML.Attribute(E, "st");
			string DeviceToken = XML.Attribute(E, "dt");
			string UserToken = XML.Attribute(E, "ut");

			InteroperabilityServerInterfacesEventHandler h = this.OnGetInterfaces;
			InteroperabilityServerEventArgs e2 = new InteroperabilityServerEventArgs(NodeId, SourceId, Partition, ServiceToken, DeviceToken, UserToken);
			if (!(h is null))
			{
				try
				{
					await h(this, e2);
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
