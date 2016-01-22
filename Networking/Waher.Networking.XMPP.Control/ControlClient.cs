using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Things;
using Waher.Networking.XMPP.Control.ParameterTypes;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.Control
{
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
		public void Set(string To, string ControlParameterName, bool Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<boolean name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(CommonTypes.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, ColorReference Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<color name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, DateTime Value, bool DateOnly, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			if (DateOnly)
				Xml.Append("<date name='");
			else
				Xml.Append("<dateTime name='");

			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(XML.Encode(Value, DateOnly));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, double Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<double name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(CommonTypes.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, Duration Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<duration name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, int Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<int name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, long Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<long name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, string Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<string name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(XML.Encode(Value));
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public void Set(string To, string ControlParameterName, TimeSpan Value, IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
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
			IqResultEventHandler Callback, object State, params ThingReference[] Nodes)
		{
			StringBuilder Xml = this.SetHeader(ServiceToken, DeviceToken, UserToken, Nodes);

			Xml.Append("<time name='");
			Xml.Append(XML.Encode(ControlParameterName));
			Xml.Append("' value='");
			Xml.Append(Value.ToString());
			Xml.Append("'/></set>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
				Xml.Append("' serviceToken='");
				Xml.Append(XML.Encode(ServiceToken));
			}

			if (!string.IsNullOrEmpty(DeviceToken))
			{
				Xml.Append("' deviceToken='");
				Xml.Append(XML.Encode(DeviceToken));
			}

			if (!string.IsNullOrEmpty(UserToken))
			{
				Xml.Append("' userToken='");
				Xml.Append(XML.Encode(UserToken));
			}

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append("' language='");
				Xml.Append(XML.Encode(Language));
			}

			Xml.Append("'>");

			if (Nodes != null)
			{
				foreach (ThingReference Node in Nodes)
				{
					Xml.Append("<node nodeId='");
					Xml.Append(XML.Encode(Node.NodeId));

					if (!string.IsNullOrEmpty(Node.SourceId))
					{
						Xml.Append("' sourceId='");
						Xml.Append(XML.Encode(Node.SourceId));
					}

					if (!string.IsNullOrEmpty(Node.CacheType))
					{
						Xml.Append("' cacheType='");
						Xml.Append(XML.Encode(Node.CacheType));
					}

					Xml.Append("'/>");
				}
			}

			return Xml;
		}

		/// <summary>
		/// Gets a control form.
		/// </summary>
		/// <param name="To">Full JID of remote actuator.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Callback">Method called when form is returned or when operation fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
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

			this.client.SendIqGet(To, Xml.ToString(), this.GetFormResult, new object[] { Callback, State });
		}

		private void GetFormResult(object Sender, IqResultEventArgs e)
		{
			object[] P = (object[])e.State;
			DataFormResultEventHandler Callback = (DataFormResultEventHandler)P[0];
			if (Callback == null)
				return;

			object State = P[1];
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
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<set xmlns='");
			Xml.Append(NamespaceControl);
			Xml.Append("'>");

			Form.SerializeSubmit(Xml);

			Xml.Append("</set>");

			Form.Client.SendIqSet(Form.From, Xml.ToString(), null, null);
		}

		private void CancelForm(object Sender, DataForm Form)
		{
			StringBuilder Xml = new StringBuilder();

			Form.SerializeCancel(Xml);

			// TODO: Cancel dynamic form.
		}

	}
}
