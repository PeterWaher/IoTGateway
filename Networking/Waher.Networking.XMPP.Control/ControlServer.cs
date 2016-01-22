using System;
using System.Collections.Generic;
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
	/// Get control parameters event handler delegate.
	/// </summary>
	/// <param name="Node">Node reference.</param>
	/// <returns>Collection of control parameters for node, or null if node not recognized.</returns>
	public delegate ControlParameter[] GetControlParametersEventHandler(ThingReference Node);

	/// <summary>
	/// Implements an XMPP control server interface.
	/// 
	/// The interface is defined in XEP-0325:
	/// http://xmpp.org/extensions/xep-0325.html
	/// </summary>
	public class ControlServer : IDisposable
	{
		private ControlParameter[] controlParameters;
		private Dictionary<string, ControlParameter> controlParametersByName = new Dictionary<string, ControlParameter>();
		private XmppClient client;

		/// <summary>
		/// Implements an XMPP control server interface.
		/// 
		/// The interface is defined in XEP-0325:
		/// http://xmpp.org/extensions/xep-0325.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="Parameters">Default set of control parameters. If set of control parameters vary depending on node, leave this
		/// field blank, and provide an event handler for the <see cref="OnGetControlParameters"/> event.</param>
		public ControlServer(XmppClient Client, params ControlParameter[] Parameters)
		{
			this.client = Client;

			this.controlParameters = Parameters;
			foreach (ControlParameter P in Parameters)
				this.controlParametersByName[P.Name] = P;

			this.client.RegisterIqSetHandler("set", ControlClient.NamespaceControl, this.SetHandler, true);
			this.client.RegisterIqGetHandler("getForm", ControlClient.NamespaceControl, this.GetFormHandler, false);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.controlParameters = null;
			this.controlParametersByName.Clear();

			this.client.UnregisterIqGetHandler("set", ControlClient.NamespaceControl, this.SetHandler, true);
			this.client.UnregisterIqGetHandler("getForm", ControlClient.NamespaceControl, this.GetFormHandler, false);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

		/// <summary>
		/// Event raised when the collection of control parameters is required. If not specified, the default collection of parameters 
		/// defined in the constructor will be used.
		/// </summary>
		public event GetControlParametersEventHandler OnGetControlParameters = null;

		private Dictionary<string, ControlParameter> GetControlParametersByName(ThingReference Node)
		{
			GetControlParametersEventHandler h = this.OnGetControlParameters;
			if (h == null)
				return this.controlParametersByName;
			else
			{
				Dictionary<string, ControlParameter> Result = new Dictionary<string, ControlParameter>();

				foreach (ControlParameter P in h(Node))
					Result[P.Name] = P;

				return Result;
			}
		}

		private ControlParameter[] GetControlParameters(ThingReference Node)
		{
			GetControlParametersEventHandler h = this.OnGetControlParameters;
			if (h == null)
				return this.controlParameters;
			else
				return h(Node);
		}

		private void ParameterNotFound(string Name, IqEventArgs e)
		{
			e.IqError("<item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" + 
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Parameter not found.</error>");
		}

		private void NotFound(IqEventArgs e)
		{
			e.IqError("<item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/>");
		}

		private void ParameterWrongType(string Name, IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" + 
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Invalid parameter type.</error>");
		}

		private void ParameterSyntaxError(string Name, IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Syntax error.</error>");
		}

		private void ParameterValueInvalid(string Name, IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Value not valid.</error>");
		}

		private void ParameterBadRequest(IqEventArgs e)
		{
			e.IqError("<bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/>");
		}

		private static readonly IEnumerable<ThingReference> NoNodes = new ThingReference[] { null };

		private void SetHandler(object Sender, IqEventArgs e)
		{
			string ServiceToken = XML.Attribute(e.Query, "serviceToken");
			string DeviceToken = XML.Attribute(e.Query, "deviceToken");
			string UserToken = XML.Attribute(e.Query, "userToken");

			LinkedList<ThingReference> Nodes = null;
			XmlElement E;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				switch (E.LocalName)
				{
					case "node":
						if (Nodes == null)
							Nodes = new LinkedList<ThingReference>();

						Nodes.AddLast(new ThingReference(
							XML.Attribute(E, "nodeId"),
							XML.Attribute(E, "sourceId"),
							XML.Attribute(E, "cacheType")));
						break;

					case "boolean":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetBooleanValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", false), e))
								return;
						}
						break;

					case "color":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetColorValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value"), e))
								return;
						}
						break;

					case "date":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetDateValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", DateTime.MinValue), e))
								return;
						}
						break;

					case "dateTime":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetDateTimeValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", DateTime.MinValue), e))
								return;
						}
						break;

					case "double":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetDoubleValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0.0), e))
								return;
						}
						break;

					case "duration":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetDurationValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", Duration.Zero), e))
								return;
						}
						break;

					case "int":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetInt32Value(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0), e))
								return;
						}
						break;

					case "long":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetInt64Value(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", 0L), e))
								return;
						}
						break;

					case "string":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetStringValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value"), e))
								return;
						}
						break;

					case "time":
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							if (!this.SetTimeValue(Node, XML.Attribute(E, "name"), XML.Attribute(E, "value", TimeSpan.Zero), e))
								return;
						}
						break;

					case "x":
						Dictionary<string, ControlParameter> Parameters;
						ControlParameter Parameter;
						DataForm Form = new DataForm(this.client, E, null, null, e.From, e.To);

						if (Form.Type != FormType.Submit)
						{
							this.ParameterBadRequest(e);
							return;
						}

						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameters = this.GetControlParametersByName(Node);
							if (Parameters == null)
							{
								this.NotFound(e);
								return;
							}

							foreach (Field Field in Form.Fields)
							{
								if (!Parameters.TryGetValue(Field.Var, out Parameter))
								{
									this.ParameterNotFound(Field.Var, e);
									return;
								}

								if (!Parameter.SetStringValue(Node, Field.ValueString))
								{
									this.ParameterSyntaxError(Field.Var, e);
									return;
								}
							}
						}
						break;
				}
			}

			e.IqResult("<setResponse xmlns=\"" + ControlClient.NamespaceControl + "\"/>");
		}

		private ControlParameter GetParameter(ThingReference Node, string Name, IqEventArgs e)
		{
			Dictionary<string, ControlParameter> Parameters = this.GetControlParametersByName(Node);
			ControlParameter Parameter;

			if (Parameters == null)
			{
				this.NotFound(e);
				return null;
			}

			if (!Parameters.TryGetValue(Name, out Parameter))
			{
				this.ParameterNotFound(Name, e);
				return null;
			}

			return Parameter;
		}

		private bool SetBooleanValue(ThingReference Node, string Name, bool BooleanValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is BooleanControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((BooleanControlParameter)Parameter).Set(Node, BooleanValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetColorValue(ThingReference Node, string Name, string StringValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is ColorControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!Parameter.SetStringValue(Node, StringValue))
			{
				this.ParameterSyntaxError(Name, e);
				return false;
			}

			return true;
		}

		private bool SetDateValue(ThingReference Node, string Name, DateTime DTValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is DateControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((DateControlParameter)Parameter).Set(Node, DTValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetDateTimeValue(ThingReference Node, string Name, DateTime DTValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is DateTimeControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((DateTimeControlParameter)Parameter).Set(Node, DTValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetDoubleValue(ThingReference Node, string Name, double DoubleValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is DoubleControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((DoubleControlParameter)Parameter).Set(Node, DoubleValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetDurationValue(ThingReference Node, string Name, Duration DurationValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is DurationControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((DurationControlParameter)Parameter).Set(Node, DurationValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetInt32Value(ThingReference Node, string Name, int Int32Value, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is Int32ControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((Int32ControlParameter)Parameter).Set(Node, Int32Value))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetInt64Value(ThingReference Node, string Name, long Int64Value, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is Int64ControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((Int64ControlParameter)Parameter).Set(Node, Int64Value))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetStringValue(ThingReference Node, string Name, string StringValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is StringControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((StringControlParameter)Parameter).Set(Node, StringValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private bool SetTimeValue(ThingReference Node, string Name, TimeSpan TimeValue, IqEventArgs e)
		{
			ControlParameter Parameter = this.GetParameter(Node, Name, e);
			if (Parameter == null)
				return false;

			if (!(Parameter is TimeControlParameter))
			{
				this.ParameterWrongType(Name, e);
				return false;
			}

			if (!((TimeControlParameter)Parameter).Set(Node, TimeValue))
			{
				this.ParameterValueInvalid(Name, e);
				return false;
			}

			return true;
		}

		private void GetFormHandler(object Sender, IqEventArgs e)
		{
			LinkedList<ThingReference> Nodes = null;
			XmlElement E;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				if (E.LocalName == "node")
				{
					if (Nodes == null)
						Nodes = new LinkedList<ThingReference>();

					Nodes.AddLast(new ThingReference(
						XML.Attribute(E, "nodeId"),
						XML.Attribute(E, "sourceId"),
						XML.Attribute(E, "cacheType")));
				}
			}

			ControlParameter[] Parameters;

			if (Nodes == null)
				Parameters = this.GetControlParameters(null);
			else
			{
				Dictionary<string, ControlParameter> Parameters1;
				Dictionary<string, ControlParameter> Parameters2;
				LinkedList<string> ToRemove = null;
				ControlParameter P2;

				Parameters = null;
				Parameters1 = null;

				foreach (ThingReference Node in Nodes)
				{
					if (Parameters1 == null)
					{
						Parameters = this.GetControlParameters(Node);

						foreach (ControlParameter P in Parameters)
							Parameters1[P.Name] = P;
					}
					else
					{
						Parameters2 = this.GetControlParametersByName(Node);

						foreach (KeyValuePair<string, ControlParameter> P in Parameters1)
						{
							if (!Parameters2.TryGetValue(P.Key, out P2) || !P.Value.Equals(P2))
							{
								if (ToRemove == null)
									ToRemove = new LinkedList<string>();

								ToRemove.AddLast(P.Key);
							}
						}

						if (ToRemove != null)
						{
							foreach (string Key in ToRemove)
								Parameters1.Remove(Key);

							ToRemove = null;
						}
					}
				}

				List<ControlParameter> Left = new List<ControlParameter>();

				foreach (ControlParameter P in Parameters)
				{
					if (Parameters1.ContainsKey(P.Name))
						Left.Add(P);
				}

				Parameters = Left.ToArray();
			}

			StringBuilder Xml = new StringBuilder();
			XmlWriter Output = XmlWriter.Create(Xml, XML.WriterSettings(false, true));
			ThingReference FirstNode;

			Output.WriteStartElement("x", XmppClient.NamespaceData);
			Output.WriteAttributeString("xmlns", "xdv", null, XmppClient.NamespaceDataValidate);
			Output.WriteAttributeString("xmlns", "xdl", null, XmppClient.NamespaceDataLayout);
			Output.WriteAttributeString("xmlns", "xdd", null, XmppClient.NamespaceDynamicForms);

			if (Nodes == null)
			{
				FirstNode = null;
				Output.WriteElementString("title", this.client.BareJID);
			}
			else
			{
				FirstNode = Nodes.First.Value;

				if (Nodes.First.Next == null)
					Output.WriteElementString("title", Nodes.First.Value.NodeId);
				else
					Output.WriteElementString("title", Nodes.Count.ToString() + " nodes");
			}

			LinkedList<string> PagesInOrder = new LinkedList<string>();
			Dictionary<string, LinkedList<ControlParameter>> ParametersPerPage = new Dictionary<string, LinkedList<ControlParameter>>();
			LinkedList<ControlParameter> List;

			foreach (ControlParameter P in Parameters)
			{
				if (!ParametersPerPage.TryGetValue(P.Page, out List))
				{
					PagesInOrder.AddLast(P.Page);
					List = new LinkedList<ControlParameter>();
					ParametersPerPage[P.Page] = List;
				}

				List.AddLast(P);
			}

			foreach (string Page in PagesInOrder)
			{
				Output.WriteStartElement("xdl", "page", null);
				Output.WriteAttributeString("label", Page);

				foreach (ControlParameter P in ParametersPerPage[Page])
				{
					Output.WriteStartElement("xdl", "fieldref", null);
					Output.WriteAttributeString("var", P.Name);
					Output.WriteEndElement();
				}

				Output.WriteEndElement();
			}

			foreach (ControlParameter P in Parameters)
				P.ExportToForm(Output, FirstNode);

			Output.WriteEndElement();
			Output.Flush();

			e.IqResult(Xml.ToString());
		}

	}
}
