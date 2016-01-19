using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Things;
using Waher.Networking.XMPP.Control.ParameterTypes;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.StanzaErrors;

namespace Waher.Networking.XMPP.Control
{
	/// <summary>
	/// Implements an XMPP control server interface.
	/// 
	/// The interface is defined in XEP-0325:
	/// http://xmpp.org/extensions/xep-0325.html
	/// </summary>
	public class ControlServer : IDisposable
	{
		/// <summary>
		/// urn:xmpp:iot:control
		/// </summary>
		public const string NamespaceControl = "urn:xmpp:iot:control";

		private Dictionary<string, ControlParameter> controlParameters = new Dictionary<string, ControlParameter>();
		private XmppClient client;

		/// <summary>
		/// Implements an XMPP control server interface.
		/// 
		/// The interface is defined in XEP-0325:
		/// http://xmpp.org/extensions/xep-0325.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Parameters">Control parameters.</param>
		public ControlServer(XmppClient Client, params ControlParameter[] Parameters)
		{
			this.client = Client;

			foreach (ControlParameter P in Parameters)
				this.controlParameters[P.Name] = P;

			this.client.RegisterIqGetHandler("set", NamespaceControl, this.SetHandler, true);
			this.client.RegisterIqGetHandler("getForm", NamespaceControl, this.GetFormHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.controlParameters.Clear();

			this.client.UnregisterIqGetHandler("set", NamespaceControl, this.SetHandler, true);
			this.client.UnregisterIqGetHandler("getForm", NamespaceControl, this.GetFormHandler, false);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		private void ParameterNotFound(string Name, IqEventArgs e)
		{
			e.IqError("<item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" + NamespaceControl + "\" var=\"" + Name +
				"\">Parameter not found.</error>");
		}

		private void ParameterWrongType(string Name, IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" + NamespaceControl + "\" var=\"" + Name +
				"\">Invalid parameter type.</error>");
		}

		private void ParameterSyntaxError(string Name, IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" + NamespaceControl + "\" var=\"" + Name +
				"\">Syntax error.</error>");
		}

		private void ParameterBadRequest(IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/>");
		}

		private void SetHandler(object Sender, IqEventArgs e)
		{
			string ServiceToken = XML.Attribute(e.Query, "serviceToken");
			string DeviceToken = XML.Attribute(e.Query, "deviceToken");
			string UserToken = XML.Attribute(e.Query, "userToken");

			ThingReference Node = null;
			XmlElement E;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				switch (E.LocalName)
				{
					case "node":
						Node = new ThingReference(
							XML.Attribute(E, "nodeId"),
							XML.Attribute(E, "sourceId"),
							XML.Attribute(E, "cacheType"));
						break;

					case "boolean":
						if (!this.SetBooleanValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", false), e))
							return;
						break;

					case "color":
						if (!this.SetColorValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value"), e))
							return;
						break;

					case "date":
						if (!this.SetDateValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", DateTime.MinValue), e))
							return;
						break;

					case "dateTime":
						if (!this.SetDateTimeValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", DateTime.MinValue), e))
							return;
						break;

					case "double":
						if (!this.SetDoubleValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0.0), e))
							return;
						break;

					case "duration":
						if (!this.SetDurationValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", Duration.Zero), e))
							return;
						break;

					case "int":
						if (!this.SetInt32Value(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0), e))
							return;
						break;

					case "long":
						if (!this.SetInt64Value(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0L), e))
							return;
						break;

					case "string":
						if (!this.SetStringValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value"), e))
							return;
						break;

					case "time":
						if (!this.SetTimeValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", TimeSpan.Zero), e))
							return;
						break;

					case "x":
						DataForm Form = new DataForm(this.client, E, null, null, e.From, e.To);
						if (Form.Type != FormType.Submit)
						{
							this.ParameterBadRequest(e);
							return;
						}

						foreach (Field Field in Form.Fields)
						{
						}
						// TODO
						break;
				}
			}

			e.IqResult("<setResponse xmlns=\"" + NamespaceControl + "\"/>");
		}

		private bool SetBooleanValue(ThingReference Node, string Name, bool BooleanValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is BooleanControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((BooleanControlParameter)Parameter).Set(Node, BooleanValue);

			return true;
		}

		private bool SetColorValue(ThingReference Node, string Name, string StringValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is ColorControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			byte R, G, B, A;

			if (StringValue.Length == 6)
			{
				if (byte.TryParse(StringValue.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
					byte.TryParse(StringValue.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
					byte.TryParse(StringValue.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B))
				{
					((ColorControlParameter)Parameter).Set(Node, new ColorReference(R, G, B));
				}
				else
				{
					this.ParameterSyntaxError(Name, e);
					return false;
				}
			}
			else if (StringValue.Length == 8)
			{
				if (byte.TryParse(StringValue.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out R) &&
					byte.TryParse(StringValue.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out G) &&
					byte.TryParse(StringValue.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out B) &&
					byte.TryParse(StringValue.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out A))
				{
					((ColorControlParameter)Parameter).Set(Node, new ColorReference(R, G, B, A));
				}
				else
				{
					this.ParameterSyntaxError(Name, e);
					return false;
				}
			}
			else
			{
				this.ParameterSyntaxError(Name, e);
				return false;
			}

			return true;
		}

		private bool SetDateValue(ThingReference Node, string Name, DateTime DTValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is DateControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((DateControlParameter)Parameter).Set(Node, DTValue);

			return true;
		}

		private bool SetDateTimeValue(ThingReference Node, string Name, DateTime DTValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is DateTimeControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((DateTimeControlParameter)Parameter).Set(Node, DTValue);

			return true;
		}

		private bool SetDoubleValue(ThingReference Node, string Name, double DoubleValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is DoubleControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((DoubleControlParameter)Parameter).Set(Node, DoubleValue);

			return true;
		}

		private bool SetDurationValue(ThingReference Node, string Name, Duration DurationValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is DurationControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((DurationControlParameter)Parameter).Set(Node, DurationValue);

			return true;
		}

		private bool SetInt32Value(ThingReference Node, string Name, int Int32Value, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is Int32ControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((Int32ControlParameter)Parameter).Set(Node, Int32Value);
			return true;
		}

		private bool SetInt64Value(ThingReference Node, string Name, long Int64Value, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is Int64ControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((Int64ControlParameter)Parameter).Set(Node, Int64Value);

			return true;
		}

		private bool SetStringValue(ThingReference Node, string Name, string StringValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is StringControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((StringControlParameter)Parameter).Set(Node, StringValue);

			return true;
		}

		private bool SetTimeValue(ThingReference Node, string Name, TimeSpan TimeValue, IqEventArgs e)
		{
			ControlParameter Parameter;

			if (!this.controlParameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return false;
			}

			if (!(Parameter is TimeControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			((TimeControlParameter)Parameter).Set(Node, TimeValue);

			return true;
		}

		private void GetFormHandler(object Sender, IqEventArgs e)
		{
			// TODO
		}

	}
}
