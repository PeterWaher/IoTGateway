using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.Control
{
	/// <summary>
	/// Delegate for set reslt callback methods.
	/// </summary>
	/// <param name="Sender">Sender of event.</param>
	/// <param name="e">Event arguments.</param>
	public delegate void SetResultCallback(object Sender, SetResultEventArgs e);

	/// <summary>
	/// Implements an XMPP control client interface.
	/// 
	/// The interface is defined in XEP-0325:
	/// http://xmpp.org/extensions/xep-0325.html
	/// </summary>
	public class ControlClient : IDisposable
	{
		/// <summary>
		/// urn:xmpp:iot:control
		/// </summary>
		public const string NamespaceControl = "urn:xmpp:iot:control";

		private XmppClient client;

		/// <summary>
		/// Implements an XMPP control client interface.
		/// 
		/// The interface is defined in XEP-0325:
		/// http://xmpp.org/extensions/xep-0325.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		public ControlClient(XmppClient Client)
		{
			this.client = Client;
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, bool Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, bool Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, bool Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<b n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(CommonTypes.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, ColorReference Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, ColorReference Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, ColorReference Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<cl n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="DateOnly">If only the date-portion should be configured (true), or if both date and time should be configured (false).</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, DateTime Value, bool DateOnly, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, DateOnly, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="DateOnly">If only the date-portion should be configured (true), or if both date and time should be configured (false).</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, DateTime Value, bool DateOnly, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, DateOnly, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="DateOnly">If only the date-portion should be configured (true), or if both date and time should be configured (false).</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, DateTime Value, bool DateOnly, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			if (DateOnly)
				Xml.Append("<d n='");
			else
				Xml.Append("<dt n='");

			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(XML.Encode(Value, DateOnly));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, double Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, double Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, double Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<dn n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(CommonTypes.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, Duration Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, Duration Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, Duration Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<dr n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, int Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, int Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, int Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<i n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, long Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, long Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, long Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<l n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, string Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, string Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, string Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<s n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(XML.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, TimeSpan Value, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, TimeSpan Value, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			this.Set(To, ControlParameterName, Value, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Sets a control parameter in a remote actuator.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="ControlParameterName">Control parameter name.</param>
		/// <param name="Value">Value to set.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when set operation completes or fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to set the parameter on, if residing behind a concentrator.</param>
		public void Set(string To, string ControlParameterName, TimeSpan Value, string ServiceToken, string DeviceToken, string UserToken,
			SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<t n='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' v='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		private void SetResultCallback(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			SetResultCallback Callback = (SetResultCallback)P[0];
			if (Callback == null)
				return;

			object State = P[1];
			XmlElement E = e.FirstElement;
			XmlElement E2;
			SetResultEventArgs e2;

			if (e.Ok)
			{
				if (E != null && E.LocalName == "resp" && E.NamespaceURI == NamespaceControl)
				{
					List<ThingReference> Nodes = null;
					List<string> ParameterNames = null;

					foreach (XmlNode N in E.ChildNodes)
					{
						E2 = N as XmlElement;
						if (E2 == null)
							continue;

						switch (N.LocalName)
						{
							case "nd":
								if (Nodes == null)
									Nodes = new List<ThingReference>();

								Nodes.Add(new ThingReference(
									XML.Attribute(E2, "id"),
									XML.Attribute(E2, "src"),
									XML.Attribute(E2, "pt")));

								break;

							case "p":
								if (ParameterNames == null)
									ParameterNames = new List<string>();

								ParameterNames.Add(XML.Attribute(E2, "n"));
								break;
						}
					}

					e2 = new SetResultEventArgs(e, true, Nodes?.ToArray(), ParameterNames?.ToArray(), State);
				}
				else
					e2 = new SetResultEventArgs(e, true, null, null, State);
			}
			else
				e2 = new SetResultEventArgs(e, false, null, null, State);

			try
			{
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private StringBuilder SetHeader(string ServiceToken, string DeviceToken, string UserToken, params ThingReference[] Nodes)
		{
			return this.CommandHeader("set", null, ServiceToken, DeviceToken, UserToken, Nodes);
		}

		private StringBuilder CommandHeader(string Command, string Language, string ServiceToken, string DeviceToken, string UserToken, params ThingReference[] Nodes)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<");
			Xml.Append(Command);
			Xml.Append(" xmlns='");
			Xml.Append(NamespaceControl);

			if (!string.IsNullOrEmpty(ServiceToken))
			{
				Xml.Append("' st='");
				Xml.Append(XML.Encode(ServiceToken));
			}

			if (!string.IsNullOrEmpty(DeviceToken))
			{
				Xml.Append("' dt='");
				Xml.Append(XML.Encode(DeviceToken));
			}

			if (!string.IsNullOrEmpty(UserToken))
			{
				Xml.Append("' ut='");
				Xml.Append(XML.Encode(UserToken));
			}

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append("' xml:lang='");
				Xml.Append(XML.Encode(Language));
			}

			Xml.Append("'>");

			this.Serialize(Xml, Nodes);

			return Xml;
		}

		private void Serialize(StringBuilder Xml, ThingReference[] Nodes)
		{
			if (Nodes != null)
			{
				foreach (ThingReference Node in Nodes)
				{
					Xml.Append("<nd id='");
					Xml.Append(XML.Encode(Node.NodeId));

					if (!string.IsNullOrEmpty(Node.SourceId))
					{
						Xml.Append("' src='");
						Xml.Append(XML.Encode(Node.SourceId));
					}

					if (!string.IsNullOrEmpty(Node.Partition))
					{
						Xml.Append("' pt='");
						Xml.Append(XML.Encode(Node.Partition));
					}

					Xml.Append("'/>");
				}
			}
		}

		/// <summary>
		/// Gets a control form.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Nodes">Node(s) to get the form from, if residing behind a concentrator.</param>
		public void GetForm(string To, string Language, params ThingReference[] Nodes)
		{
			this.GetForm(To, Language, string.Empty, string.Empty, string.Empty, null, null, Nodes);
		}

		/// <summary>
		/// Gets a control form.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Callback">Method called when form is returned or when operation fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to get the form from, if residing behind a concentrator.</param>
		public void GetForm(string To, string Language, DataFormResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			this.GetForm(To, Language, string.Empty, string.Empty, string.Empty, Callback, State, Nodes);
		}

		/// <summary>
		/// Gets a control form.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="ServiceToken">Service token of original sender, if available.</param>
		/// <param name="DeviceToken">Device token of original sender, if available.</param>
		/// <param name="UserToken">User token of original sender, if available.</param>
		/// <param name="Callback">Method called when form is returned or when operation fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <param name="Nodes">Node(s) to get the form from, if residing behind a concentrator.</param>
		public void GetForm(string To, string Language, string ServiceToken, string DeviceToken, string UserToken,
			DataFormResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.CommandHeader("getForm", Language, ServiceToken, DeviceToken, UserToken, Nodes);
			Xml.Append("</getForm>");

			this.client.SendIqGet(To, Xml.ToString(), this.GetFormResult, new object[] { Callback, State, Nodes });
		}

		private void GetFormResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			DataFormResultEventHandler Callback = (DataFormResultEventHandler)P[0];
			if (Callback == null)
				return;

			object State = P[1];
			ThingReference[] Nodes = (ThingReference[])P[2];
			DataForm Form = null;

			if (e.Ok)
			{
				foreach (XmlNode N in e.Response.ChildNodes)
				{
					if (N.LocalName == "x")
					{
						Form = new DataForm(this.client, (XmlElement)N, this.SubmitForm, this.CancelForm, e.From, e.To);
						break;
					}
				}

				if (Form != null)
					Form.State = Nodes;
			}

			DataFormEventArgs e2 = new DataFormEventArgs(Form, e);
			try
			{
				e2.State = State;
				Callback(this, e2);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private void SubmitForm(object Sender, DataForm Form)
		{
			this.Set(Form, null, null, (ThingReference[])Form.State);
		}

		/// <summary>
		/// Sets control parameters from a data form, previously fetched using any of the GetForm methods.
		/// </summary>
		/// <param name="Form">Data form.</param>
		/// <param name="Callback">Callback method.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		/// <param name="Nodes">Any nodes to control.</param>
		public void Set(DataForm Form, SetResultCallback Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<set xmlns='");
			Xml.Append(NamespaceControl);
			Xml.Append("'>");

			this.Serialize(Xml, Nodes);
			Form.SerializeSubmit(Xml);

			Xml.Append("</set>");

			Form.Client.SendIqSet(Form.From, Xml.ToString(), SetResultCallback, new object[] { Callback, State });
		}

		private void CancelForm(object Sender, DataForm Form)
		{
			StringBuilder Xml = new StringBuilder();

			Form.SerializeCancel(Xml);

			// TODO: Cancel dynamic form.
		}

	}
}
