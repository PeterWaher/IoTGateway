using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Things;
using Waher.Networking.XMPP.Control.ControlOperations;
using Waher.Things.ControlParameters;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.StanzaErrors;
using Waher.Runtime.Inventory;

namespace Waher.Networking.XMPP.Control
{
	/// <summary>
	/// Get control parameters event handler delegate.
	/// </summary>
	/// <param name="Node">Node reference.</param>
	/// <returns>Collection of control parameters for node, or null if node not recognized.</returns>
	public delegate Task<ControlParameter[]> GetControlParametersEventHandler(IThingReference Node);

	/// <summary>
	/// Implements an XMPP control server interface.
	/// 
	/// The interface is defined in the IEEE XMPP IoT extensions:
	/// https://gitlab.com/IEEE-SA/XMPPI/IoT
	/// </summary>
	public class ControlServer : XmppExtension
	{
		private readonly Dictionary<string, ControlParameter> controlParametersByName = new Dictionary<string, ControlParameter>();
		private readonly ProvisioningClient provisioningClient;
		private ControlParameter[] controlParameters;

		/// <summary>
		/// Implements an XMPP control server interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
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
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ProvisioningClient">Provisioning client, if actuator supports provisioning.</param>
		/// <param name="Parameters">Default set of control parameters. If set of control parameters vary depending on node, leave this
		/// field blank, and provide an event handler for the <see cref="OnGetControlParameters"/> event.</param>
		public ControlServer(XmppClient Client, ProvisioningClient ProvisioningClient, params ControlParameter[] Parameters)
			: base(Client)
		{
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
		public override void Dispose()
		{
			base.Dispose();

			this.controlParameters = null;
			this.controlParametersByName.Clear();

			this.client.UnregisterIqGetHandler("set", ControlClient.NamespaceControl, this.SetHandler, true);
			this.client.UnregisterIqGetHandler("getForm", ControlClient.NamespaceControl, this.GetFormHandler, false);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0325" };

		/// <summary>
		/// Event raised when the collection of control parameters is required. If not specified, the default collection of parameters 
		/// defined in the constructor will be used. If node is not found, null should be returned.
		/// </summary>
		public event GetControlParametersEventHandler OnGetControlParameters = null;

		/// <summary>
		/// Gets an array of control parameters for a node, ordered by parameter name.
		/// </summary>
		/// <param name="Node">Optional null reference. If not behind a concentrator, use null.</param>
		/// <returns>Control parameters by parameter name.</returns>
		public async Task<Dictionary<string, ControlParameter>> GetControlParametersByName(IThingReference Node)
		{
			GetControlParametersEventHandler h = this.OnGetControlParameters;
			if (h is null)
				return this.controlParametersByName;
			else
			{
				ControlParameter[] Parameters = await h(Node);

				if (Parameters is null)
					return null;

				Dictionary<string, ControlParameter> Result = new Dictionary<string, ControlParameter>();

				foreach (ControlParameter P in Parameters)
					Result[P.Name] = P;

				return Result;
			}
		}

		/// <summary>
		/// Gets an array of control parameters for a node.
		/// </summary>
		/// <param name="Node">Optional null reference. If not behind a concentrator, use null.</param>
		/// <returns>Control parameters.</returns>
		public Task<ControlParameter[]> GetControlParameters(IThingReference Node)
		{
			GetControlParametersEventHandler h = this.OnGetControlParameters;
			if (h is null)
				return Task.FromResult<ControlParameter[]>(this.controlParameters);
			else
				return h(Node);
		}

		internal static void ParameterNotFound(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Parameter not found.</paramError></error>");
		}

		internal static void NotFound(IqEventArgs e)
		{
			e.IqError("<error type='modify'><item-not-found xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/></error>");
		}

		internal static void ParameterWrongType(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Invalid parameter type.</paramError></error>");
		}

		internal static void ParameterSyntaxError(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Syntax error.</paramError></error>");
		}

		internal static void ParameterValueInvalid(string Name, IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
				ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Value not valid.</paramError></error>");
		}

		internal static void ParameterBadRequest(IqEventArgs e)
		{
			e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/></error>");
		}

		/// <summary>
		/// Array consisting of a null reference, implying no underlying nodes are referenced in the control operation.
		/// </summary>
		public static readonly IEnumerable<IThingReference> NoNodes = new ThingReference[] { null };

		/// <summary>
		/// Event raised when a node is referenced in a request. Can be used to check if nodes exist, and
		/// return the proper node class. If not provided, <see cref="ThingReference"/> objects will be created
		/// for each node reference.
		/// </summary>
		public event GetThingReferenceMethod OnGetNode = null;

		private async Task SetHandler(object Sender, IqEventArgs e)
		{
			string ServiceToken = XML.Attribute(e.Query, "st");
			string DeviceToken = XML.Attribute(e.Query, "dt");
			string UserToken = XML.Attribute(e.Query, "ut");

			LinkedList<IThingReference> Nodes = null;
			SortedDictionary<string, bool> ParameterNames = this.provisioningClient is null ? null : new SortedDictionary<string, bool>();
			LinkedList<ControlOperation> Operations = new LinkedList<ControlOperation>();
			ControlParameter Parameter;
			DataForm Form = null;
			XmlElement E;
			string Name;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				switch (E.LocalName)
				{
					case "nd":
						if (Nodes is null)
							Nodes = new LinkedList<IThingReference>();

						string NodeId = XML.Attribute(E, "id");
						string SourceId = XML.Attribute(E, "src");
						string Partition = XML.Attribute(E, "pt");

						if (this.OnGetNode is null)
							Nodes.AddLast(new ThingReference(NodeId, SourceId, Partition));
						else
						{
							IThingReference Ref = await this.OnGetNode(NodeId, SourceId, Partition);
							if (Ref is null)
								throw new ItemNotFoundException("Node not found.", e.IQ);

							Nodes.AddLast(Ref);
						}
						break;

					case "b":
					case "cl":
					case "d":
					case "dt":
					case "db":
					case "dr":
					case "e":
					case "i":
					case "l":
					case "s":
					case "t":
						if (!(ParameterNames is null))
							ParameterNames[XML.Attribute(E, "n")] = true;
						break;

					case "x":
						Form = new DataForm(this.client, E, null, null, e.From, e.To);
						if (Form.Type != FormType.Submit)
						{
							ParameterBadRequest(e);
							return;
						}

						if (!(ParameterNames is null))
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
				if (E is null)
					continue;

				switch (E.LocalName)
				{
					case "b":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							BooleanControlParameter BooleanControlParameter = Parameter as BooleanControlParameter;
							if (BooleanControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new BooleanControlOperation(Node, BooleanControlParameter, XML.Attribute(E, "v", false), e));
						}
						break;

					case "cl":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							ColorControlParameter ColorControlParameter = Parameter as ColorControlParameter;
							if (ColorControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new ColorControlOperation(Node, ColorControlParameter, XML.Attribute(E, "v"), e));
						}
						break;

					case "d":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							DateControlParameter DateControlParameter = Parameter as DateControlParameter;
							if (DateControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DateControlOperation(Node, DateControlParameter, XML.Attribute(E, "v", DateTime.MinValue), e));
						}
						break;

					case "dt":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							DateTimeControlParameter DateTimeControlParameter = Parameter as DateTimeControlParameter;
							if (DateTimeControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DateTimeControlOperation(Node, DateTimeControlParameter, XML.Attribute(E, "v", DateTime.MinValue), e));
						}
						break;

					case "db":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							DoubleControlParameter DoubleControlParameter = Parameter as DoubleControlParameter;
							if (DoubleControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DoubleControlOperation(Node, DoubleControlParameter, XML.Attribute(E, "v", 0.0), e));
						}
						break;

					case "dr":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							DurationControlParameter DurationControlParameter = Parameter as DurationControlParameter;
							if (DurationControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new DurationControlOperation(Node, DurationControlParameter, XML.Attribute(E, "v", Duration.Zero), e));
						}
						break;

					case "e":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							string StringValue = XML.Attribute(E, "v");

							if (Parameter is EnumControlParameter EnumControlParameter)
							{
								Type T = Types.GetType(XML.Attribute(E, "t"));
								if (T is null)
								{
									e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
										ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Type not found.</paramError></error>");
									return;
								}

								if (!T.GetTypeInfo().IsEnum)
								{
									e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
										ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Type is not an enumeration.</paramError></error>");
									return;
								}

								Enum Value;

								try
								{
									Value = (Enum)Enum.Parse(T, StringValue);
								}
								catch (Exception)
								{
									e.IqError("<error type='modify'><bad-request xmlns=\"urn:ietf:params:xml:ns:xmpp-stanzas\"/><paramError xmlns=\"" +
										ControlClient.NamespaceControl + "\" n=\"" + Name + "\">Value not valid element of enumeration.</paramError></error>");
									return;
								}

								Operations.AddLast(new EnumControlOperation(Node, EnumControlParameter, Value, e));
							}
							else if (Parameter is StringControlParameter StringControlParameter)
								Operations.AddLast(new StringControlOperation(Node, StringControlParameter, StringValue, e));
							else if (Parameter is MultiLineTextControlParameter MultiLineTextControlParameter)
								Operations.AddLast(new MultiLineTextControlOperation(Node, MultiLineTextControlParameter, StringValue, e));
							else
							{
								ParameterWrongType(Name, e);
								return;
							}
						}
						break;

					case "i":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							Int32ControlParameter Int32ControlParameter = Parameter as Int32ControlParameter;
							if (Int32ControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new Int32ControlOperation(Node, Int32ControlParameter, XML.Attribute(E, "v", 0), e));
						}
						break;

					case "l":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							Int64ControlParameter Int64ControlParameter = Parameter as Int64ControlParameter;
							if (Int64ControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new Int64ControlOperation(Node, Int64ControlParameter, XML.Attribute(E, "v", 0L), e));
						}
						break;

					case "s":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							if (Parameter is StringControlParameter StringControlParameter)
								Operations.AddLast(new StringControlOperation(Node, StringControlParameter, XML.Attribute(E, "v"), e));
							else if (Parameter is MultiLineTextControlParameter MultiLineTextControlParameter)
								Operations.AddLast(new MultiLineTextControlOperation(Node, MultiLineTextControlParameter, XML.Attribute(E, "v"), e));
							else
							{
								ParameterWrongType(Name, e);
								return;
							}
						}
						break;

					case "t":
						Name = XML.Attribute(E, "n");
						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameter = await this.GetParameter(Node, Name, e);
							if (Parameter is null)
								return;

							TimeControlParameter TimeControlParameter = Parameter as TimeControlParameter;
							if (TimeControlParameter is null)
							{
								ParameterWrongType(Name, e);
								return;
							}

							Operations.AddLast(new TimeControlOperation(Node, TimeControlParameter, XML.Attribute(E, "v", TimeSpan.Zero), e));
						}
						break;

					case "x":
						Dictionary<string, ControlParameter> Parameters;

						foreach (IThingReference Node in Nodes ?? NoNodes)
						{
							Parameters = await this.GetControlParametersByName(Node);
							if (Parameters is null)
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
					async (sender2, e2) =>
					{
						if (e2.Ok && e2.CanControl)
						{
							LinkedList<ControlOperation> Operations2 = null;
							bool Restricted;

							if (e2.Nodes != null || e2.ParameterNames != null)
							{
								Dictionary<IThingReference, bool> AllowedNodes = null;
								Dictionary<string, bool> AllowedParameterNames = null;

								Operations2 = new LinkedList<ControlOperation>();
								Restricted = false;

								if (e2.Nodes != null)
								{
									AllowedNodes = new Dictionary<IThingReference, bool>();
									foreach (IThingReference Node in e2.Nodes)
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
									if (AllowedNodes != null && !AllowedNodes.ContainsKey(Operation.Node ?? ThingReference.Empty))
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
								await this.PerformOperations(Operations2, e, e2.Nodes, e2.ParameterNames);
							else
								await this.PerformOperations(Operations, e, null, null);
						}
						else
						{
							e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>" +
								"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
						}

					}, null);
			}
			else
				await this.PerformOperations(Operations, e, null, null);
		}

		private static readonly char[] space = new char[] { ' ' };

		private async Task PerformOperations(LinkedList<ControlOperation> Operations, IqEventArgs e, IEnumerable<IThingReference> Nodes,
			IEnumerable<string> ParameterNames)
		{
			foreach (ControlOperation Operation in Operations)
			{
				if (!await Operation.Set())
					break;
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<resp xmlns=\"");
			Xml.Append(ControlClient.NamespaceControl);

			if (Nodes != null || ParameterNames != null)
			{
				Xml.Append("\">");

				if (!(Nodes is null))
				{
					foreach (IThingReference Node in Nodes)
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

				if (!(ParameterNames is null))
				{
					foreach (string ParameterName in ParameterNames)
					{
						Xml.Append("<p n='");
						Xml.Append(XML.Encode(ParameterName));
						Xml.Append("'/>");
					}
				}

				Xml.Append("</resp>");
			}
			else
				Xml.Append("\"/>");

			e.IqResult(Xml.ToString());
		}

		private async Task<ControlParameter> GetParameter(IThingReference Node, string Name, IqEventArgs e)
		{
			Dictionary<string, ControlParameter> Parameters = await this.GetControlParametersByName(Node);

			if (Parameters is null)
			{
				NotFound(e);
				return null;
			}

			if (!Parameters.TryGetValue(Name, out ControlParameter Parameter))
			{
				ParameterNotFound(Name, e);
				return null;
			}

			return Parameter;
		}

		private async Task GetFormHandler(object Sender, IqEventArgs e)
		{
			LinkedList<IThingReference> Nodes = null;
			XmlElement E;
			string ServiceToken = XML.Attribute(e.Query, "st");
			string DeviceToken = XML.Attribute(e.Query, "dt");
			string UserToken = XML.Attribute(e.Query, "ut");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.LocalName == "nd")
				{
					if (Nodes is null)
						Nodes = new LinkedList<IThingReference>();

					string NodeId = XML.Attribute(E, "id");
					string SourceId = XML.Attribute(E, "src");
					string Partition = XML.Attribute(E, "pt");

					if (this.OnGetNode is null)
						Nodes.AddLast(new ThingReference(NodeId, SourceId, Partition));
					else
					{
						IThingReference Ref = await this.OnGetNode(NodeId, SourceId, Partition);
						if (Ref is null)
							throw new ItemNotFoundException("Node not found.", e.IQ);

						Nodes.AddLast(Ref);
					}
				}
			}

			ControlParameter[] Parameters;

			if (Nodes is null)
			{
				Parameters = await this.GetControlParameters(null);
				if (Parameters is null)
				{
					NotFound(e);
					return;
				}
			}
			else
			{
				Dictionary<string, ControlParameter> Parameters1;
				Dictionary<string, ControlParameter> Parameters2;
				LinkedList<string> ToRemove = null;

				Parameters = null;
				Parameters1 = null;

				foreach (IThingReference Node in Nodes)
				{
					if (Parameters1 is null)
					{
						Parameters = await this.GetControlParameters(Node);
						if (Parameters is null)
						{
							NotFound(e);
							return;
						}

						Parameters1 = new Dictionary<string, ControlParameter>();

						foreach (ControlParameter P in Parameters)
							Parameters1[P.Name] = P;
					}
					else
					{
						Parameters2 = await this.GetControlParametersByName(Node);
						if (Parameters2 is null)
						{
							NotFound(e);
							return;
						}

						foreach (KeyValuePair<string, ControlParameter> P in Parameters1)
						{
							if (!Parameters2.TryGetValue(P.Key, out ControlParameter P2) || !P.Value.Equals(P2))
							{
								if (ToRemove is null)
									ToRemove = new LinkedList<string>();

								ToRemove.AddLast(P.Key);
							}
						}

						if (!(ToRemove is null))
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

			if (this.provisioningClient != null)
			{
				int i, c = Parameters.Length;
				string[] ParameterNames = new string[c];

				for (i = 0; i < c; i++)
					ParameterNames[i] = Parameters[i].Name;

				this.provisioningClient.CanControl(e.FromBareJid, Nodes, ParameterNames,
					ServiceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					DeviceToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					UserToken.Split(space, StringSplitOptions.RemoveEmptyEntries),
					async (sender2, e2) =>
					{
						if (e2.Ok && e2.CanControl)
						{
							if (e2.ParameterNames != null)
							{
								List<ControlParameter> Parameters2 = new List<ControlParameter>();

								foreach (ControlParameter P in Parameters)
								{
									if (Array.IndexOf<string>(e2.ParameterNames, P.Name) >= 0)
										Parameters2.Add(P);
								}

								Parameters = Parameters2.ToArray();

								if (Parameters.Length == 0)
								{
									e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>" +
										"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
									return;
								}
							}

							await this.ReturnForm(e, Parameters, Nodes);
						}
						else
						{
							e.IqError("<error type='cancel'><forbidden xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>" +
								"<text xmlns='urn:ietf:params:xml:ns:xmpp-stanzas' xml:lang='en'>Access denied.</text></error>");
						}
					}, null);
			}
			else
				await this.ReturnForm(e, Parameters, Nodes);
		}

		private async Task ReturnForm(IqEventArgs e, ControlParameter[] Parameters, LinkedList<IThingReference> Nodes)
		{
			StringBuilder Xml = new StringBuilder();
			XmlWriter Output = XmlWriter.Create(Xml, XML.WriterSettings(false, true));
			IThingReference FirstNode;

			Output.WriteStartElement("x", XmppClient.NamespaceData);
			Output.WriteAttributeString("xmlns", "xdv", null, XmppClient.NamespaceDataValidate);
			Output.WriteAttributeString("xmlns", "xdl", null, XmppClient.NamespaceDataLayout);
			Output.WriteAttributeString("xmlns", "xdd", null, XmppClient.NamespaceDynamicForms);

			if (Nodes is null)
			{
				FirstNode = null;
				Output.WriteElementString("title", this.client.BareJID);
			}
			else
			{
				FirstNode = Nodes.First.Value;

				if (Nodes.First.Next is null)
					Output.WriteElementString("title", Nodes.First.Value.NodeId);
				else
					Output.WriteElementString("title", Nodes.Count.ToString() + " nodes");
			}

			LinkedList<string> PagesInOrder = new LinkedList<string>();
			Dictionary<string, LinkedList<ControlParameter>> ParametersPerPage = new Dictionary<string, LinkedList<ControlParameter>>();

			foreach (ControlParameter P in Parameters)
			{
				if (!ParametersPerPage.TryGetValue(P.Page, out LinkedList<ControlParameter> List))
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
				await P.ExportToForm(Output, FirstNode);

			Output.WriteEndElement();
			Output.Flush();

			e.IqResult(Xml.ToString());
		}


	}
}
