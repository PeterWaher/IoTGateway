using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Things;
using Waher.Networking.XMPP.Control.ControlOperations;
using Waher.Networking.XMPP.Control.ParameterTypes;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Provisioning;
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
		private ProvisioningClient provisioningClient;

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
			: this(Client, null, Parameters)
		{
		}

		/// <summary>
		/// Implements an XMPP control server interface.
		/// 
		/// The interface is defined in XEP-0325:
		/// http://xmpp.org/extensions/xep-0325.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ProvisioningClient">Provisioning client, if actuator supports provisioning.</param>
		/// <param name="Parameters">Default set of control parameters. If set of control parameters vary depending on node, leave this
		/// field blank, and provide an event handler for the <see cref="OnGetControlParameters"/> event.</param>
		public ControlServer(XmppClient Client, ProvisioningClient ProvisioningClient, params ControlParameter[] Parameters)
		{
			this.client = Client;
			this.provisioningClient = ProvisioningClient;

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

		/// <summary>
		/// Gets an array of control parameters for a node, ordered by parameter name.
		/// </summary>
		/// <param name="Node">Optional null reference. If not behind a concentrator, use null.</param>
		/// <returns>Control parameters by parameter name.</returns>
		public Dictionary<string, ControlParameter> GetControlParametersByName(ThingReference Node)
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

		/// <summary>
		/// Gets an array of control parameters for a node.
		/// </summary>
		/// <param name="Node">Optional null reference. If not behind a concentrator, use null.</param>
		/// <returns>Control parameters.</returns>
		public ControlParameter[] GetControlParameters(ThingReference Node)
		{
			GetControlParametersEventHandler h = this.OnGetControlParameters;
			if (h == null)
				return this.controlParameters;
			else
				return h(Node);
		}

		internal static void ParameterNotFound(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Parameter not found.</paramError></error>");
		}

		internal static void NotFound(IqEventArgs e)
		{
			e.IqError("<error type='modify'><item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/></error>");
		}

		internal static void ParameterWrongType(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Invalid parameter type.</paramError></error>");
		}

		internal static void ParameterSyntaxError(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Syntax error.</paramError></error>");
		}

		internal static void ParameterValueInvalid(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" var=\"" + Name + "\">Value not valid.</paramError></error>");
		}

		internal static void ParameterBadRequest(IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/></error>");
		}

		/// <summary>
		/// Array consisting of a null reference, implying no underlying nodes are referenced in the control operation.
		/// </summary>
		public static readonly IEnumerable<ThingReference> NoNodes = new ThingReference[] { null };

		private void SetHandler(object Sender, IqEventArgs e)
		{
			string ServiceToken = XML.Attribute(e.Query, "serviceToken");
			string DeviceToken = XML.Attribute(e.Query, "deviceToken");
			string UserToken = XML.Attribute(e.Query, "userToken");

			LinkedList<ThingReference> Nodes = null;
			SortedDictionary<string, bool> ParameterNames = this.provisioningClient == null ? null : new SortedDictionary<string, bool>();
			LinkedList<ControlOperation> Operations = new LinkedList<ControlOperation>();
			ControlParameter Parameter;
			DataForm Form = null;
			XmlElement E;
			string Name;

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
					case "color":
					case "date":
					case "dateTime":
					case "double":
					case "duration":
					case "int":
					case "long":
					case "string":
					case "time":
						if (ParameterNames != null)
							ParameterNames[XML.Attribute(E, "name")] = true;
						break;

					case "x":
						Form = new DataForm(this.client, E, null, null, e.From, e.To);
						if (Form.Type != FormType.Submit)
						{
							ParameterBadRequest(e);
							return;
						}

						if (ParameterNames != null)
						{
							foreach (Field Field in Form.Fields)
								ParameterNames[Field.Var] = true;
						}
						break;

					default:
						ParameterBadRequest(e);
						return;
				}
			}

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				switch (E.LocalName)
				{
					case "boolean":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							BooleanControlParameter BooleanControlParameter = Parameter as BooleanControlParameter;
							if (BooleanControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new BooleanControlOperation(Node, BooleanControlParameter, XML.Attribute(E, "value", false), e));
						}
						break;

					case "color":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							ColorControlParameter ColorControlParameter = Parameter as ColorControlParameter;
							if (ColorControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new ColorControlOperation(Node, ColorControlParameter, XML.Attribute(E, "value"), e));
						}
						break;

					case "date":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							DateControlParameter DateControlParameter = Parameter as DateControlParameter;
							if (DateControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DateControlOperation(Node, DateControlParameter, XML.Attribute(E, "value", DateTime.MinValue), e));
						}
						break;

					case "dateTime":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							DateTimeControlParameter DateTimeControlParameter = Parameter as DateTimeControlParameter;
							if (DateTimeControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DateTimeControlOperation(Node, DateTimeControlParameter, XML.Attribute(E, "value", DateTime.MinValue), e));
						}
						break;

					case "double":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							DoubleControlParameter DoubleControlParameter = Parameter as DoubleControlParameter;
							if (DoubleControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DoubleControlOperation(Node, DoubleControlParameter, XML.Attribute(E, "value", 0.0), e));
						}
						break;

					case "duration":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							DurationControlParameter DurationControlParameter = Parameter as DurationControlParameter;
							if (DurationControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DurationControlOperation(Node, DurationControlParameter, XML.Attribute(E, "value", Duration.Zero), e));
						}
						break;

					case "int":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							Int32ControlParameter Int32ControlParameter = Parameter as Int32ControlParameter;
							if (Int32ControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new Int32ControlOperation(Node, Int32ControlParameter, XML.Attribute(E, "value", 0), e));
						}
						break;

					case "long":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							Int64ControlParameter Int64ControlParameter = Parameter as Int64ControlParameter;
							if (Int64ControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new Int64ControlOperation(Node, Int64ControlParameter, XML.Attribute(E, "value", 0L), e));
						}
						break;

					case "string":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							StringControlParameter StringControlParameter = Parameter as StringControlParameter;
							if (StringControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new StringControlOperation(Node, StringControlParameter, XML.Attribute(E, "value"), e));
						}
						break;

					case "time":
						Name = XML.Attribute(E, "name");
						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameter = this.GetParameter(Node, Name, e);
							if (Parameter == null)
								return;

							TimeControlParameter TimeControlParameter = Parameter as TimeControlParameter;
							if (TimeControlParameter == null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new TimeControlOperation(Node, TimeControlParameter, XML.Attribute(E, "value", TimeSpan.Zero), e));
						}
						break;

					case "x":
						Dictionary<string, ControlParameter> Parameters;

						foreach (ThingReference Node in Nodes == null ? NoNodes : Nodes)
						{
							Parameters = this.GetControlParametersByName(Node);
							if (Parameters == null)
							{
								NotFound(e);
								return;
							}

							foreach (Field Field in Form.Fields)
							{
								if (!Parameters.TryGetValue(Field.Var, out Parameter))
								{
									ParameterNotFound(Field.Var, e);
									return;
								}

								Operations.AddLast(new FormControlOperation(Node, Parameter, Field.ValueString, e));
							}
						}
						break;
				}
			}

			if (this.provisioningClient != null)
			{
				string[] ParameterNames2 = new string[ParameterNames.Count];
				ParameterNames.Keys.CopyTo(ParameterNames2, 0);

				this.provisioningClient.CanControl(e.FromBareJid, Nodes, ParameterNames2,
					ServiceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					DeviceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					UserToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					(sender2, e2) =>
					{
						if (e2.Ok && e2.CanControl)
						{
							LinkedList<ControlOperation> Operations2 = null;
							bool Restricted;

							if (e2.Nodes != null || e2.ParameterNames != null)
							{
								Dictionary<ThingReference, bool> AllowedNodes = null;
								Dictionary<string, bool> AllowedParameterNames = null;

								Operations2 = new LinkedList<ControlOperation>();
								Restricted = false;

								if (e2.Nodes != null)
								{
									AllowedNodes = new Dictionary<ThingReference, bool>();
									foreach (ThingReference Node in e2.Nodes)
										AllowedNodes[Node] = true;
								}

								if (e2.ParameterNames != null)
								{
									AllowedParameterNames = new Dictionary<string, bool>();
									foreach (string ParameterName in e2.ParameterNames)
										AllowedParameterNames[ParameterName] = true;
								}

								foreach (ControlOperation Operation in Operations)
								{
									if (AllowedNodes != null && !AllowedNodes.ContainsKey(Operation.Node))
									{
										Restricted = true;
										continue;
									}

									if (AllowedParameterNames != null && !AllowedParameterNames.ContainsKey(Operation.ParameterName))
									{
										Restricted = true;
										continue;
									}

									Operations2.AddLast(Operation);
								}
							}
							else
								Restricted = false;

							if (Restricted)
								this.PerformOperations(Operations2, e, e2.Nodes, e2.ParameterNames);
							else
								this.PerformOperations(Operations, e, null, null);
						}
						else
						{
							e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' />" +
								"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
						}

					}, null);
			}
			else
				this.PerformOperations(Operations, e, null, null);
		}

		private static readonly char[] space = new char[] { ' ' };

		private void PerformOperations(LinkedList<ControlOperation> Operations, IqEventArgs e, IEnumerable<ThingReference> Nodes, 
			IEnumerable<string> ParameterNames)
		{
			foreach (ControlOperation Operation in Operations)
			{
				if (!Operation.Set())
					break;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<setResponse xmlns=\"");
			Xml.Append(ControlClient.NamespaceControl);

			if (Nodes != null || ParameterNames != null)
			{
				Xml.Append("\">");

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

				if (ParameterNames != null)
				{
					foreach (string ParameterName in ParameterNames)
					{
						Xml.Append("<parameter name='");
						Xml.Append(XML.Encode(ParameterName));
						Xml.Append("'/>");
					}
				}

				Xml.Append("</setResponse>");
			}
			else
				Xml.Append("\"/>");

			e.IqResult(Xml.ToString());
		}

		private ControlParameter GetParameter(ThingReference Node, string Name, IqEventArgs e)
		{
			Dictionary<string, ControlParameter> Parameters = this.GetControlParametersByName(Node);
			ControlParameter Parameter;

			if (Parameters == null)
			{
				NotFound(e);
				return null;
			}

			if (!Parameters.TryGetValue(Name, out Parameter))
			{
				ParameterNotFound(Name, e);
				return null;
			}

			return Parameter;
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
