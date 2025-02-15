﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP.Events;

namespace Waher.Networking.XMPP.Interoperability
{
	/// <summary>
	/// Implements the client-side for Interoperability interfaces, as defined in:
	/// 
	/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
	/// </summary>
	public class InteroperabilityClient : XmppExtension
	{
		/// <summary>
		/// Implements the client-side for Interoperability interfaces, as defined in:
		/// 
		/// https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// http://htmlpreview.github.io/?https://github.com/joachimlindborg/XMPP-IoT/blob/master/xep-0000-IoT-Interoperability.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public InteroperabilityClient(XmppClient Client)
			: base(Client)
		{
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[0];

		/// <summary>
		/// Sends a request for interoperability interfaces from an entity.
		/// </summary>
		/// <param name="Destination">JID of entity.</param>
		/// <param name="Callback">Method to call when response or error is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task SendGetInterfacesRequest(string Destination, EventHandlerAsync<InteroperabilityClientEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(Destination, "<getInterfaces xmlns='" + InteroperabilityServer.NamespaceInteroperability + "'/>",
				this.GetInterfacesResult, new object[] { Callback, State });
		}

		private async Task GetInterfacesResult(object Sender, IqResultEventArgs e)
		{
			List<string> Interfaces = new List<string>();
			object[] P = (object[])e.State;
			EventHandlerAsync<InteroperabilityClientEventArgs> Callback = (EventHandlerAsync<InteroperabilityClientEventArgs>)P[0];
			object State = P[1];

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "getInterfacesResponse")
					{
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "interface")
								Interfaces.Add(XML.Attribute((XmlElement)N2, "name"));
						}
					}
				}
			}

			InteroperabilityClientEventArgs e2 = new InteroperabilityClientEventArgs(Interfaces.ToArray(), e);
			await Callback.Raise(this, e2);
		}


		/// <summary>
		/// Gets interoperability interfaces from an entity.
		/// </summary>
		/// <param name="Destination">JID of entity.</param>
		/// <param name="Timeout">Timeout in milliseconds.</param>
		/// <exception cref="TimeoutException">If timeout occurs.</exception>
		/// <returns>Array of Interfaces.</returns>
		public string[] GetInterfaces(string Destination, int Timeout)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			InteroperabilityClientEventArgs e = null;

			try
			{
				this.SendGetInterfacesRequest(Destination, (sender, e2) =>
				{
					e = e2;
					Done.Set();

					return Task.CompletedTask;

				}, null);

				if (!Done.WaitOne(Timeout))
					throw new TimeoutException();
			}
			finally
			{
				Done.Dispose();
			}

			if (!e.Ok)
				throw e.StanzaError;

			return e.Interfaces;
		}

	}
}
