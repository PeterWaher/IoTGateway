using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.DataForms;
using Waher.Things;
using Waher.Things.DisplayableParameters;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator client interface.
	/// 
	/// The interface is defined in XEP-0326:
	/// http://xmpp.org/extensions/xep-0326.html
	/// </summary>
	public class ConcentratorClient : XmppExtension
	{
		private Dictionary<string, ISniffer> sniffers = new Dictionary<string, ISniffer>();

		/// <summary>
		/// Implements an XMPP concentrator client interface.
		/// 
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public ConcentratorClient(XmppClient Client)
			: base(Client)
		{
			Client.RegisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentrator, this.SniffMessageHandler, false);
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			Client.UnregisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentrator, this.SniffMessageHandler, false);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0326" };

		/// <summary>
		/// Gets the capabilities of a concentrator server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetCapabilities(string To, CapabilitiesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getCapabilities xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
				{
					List<string> Capabilities = new List<string>();
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getCapabilitiesResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
					{
						foreach (XmlNode N in E)
						{
							if (N.LocalName == "value")
								Capabilities.Add(N.InnerText);
						}
					}
					else
						e.Ok = false;

					try
					{
						Callback(this, new CapabilitiesEventArgs(Capabilities.ToArray(), e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}
			}, State);
		}

		/// <summary>
		/// Gets all data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAllDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getAllDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getAllDataSourcesResponse", Callback, State);
			}, State);
		}

		private void DataSourcesResponse(IqResultEventArgs e, string ExpectedElement, DataSourcesEventHandler Callback, object State)
		{
			List<DataSourceReference> DataSources = new List<DataSourceReference>();
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == ExpectedElement && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "dataSource")
						DataSources.Add(new DataSourceReference(E2));
				}
			}
			else
				e.Ok = false;

			if (Callback != null)
			{
				try
				{
					Callback(this, new DataSourcesEventArgs(DataSources.ToArray(), e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRootDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getRootDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getRootDataSourcesResponse", Callback, State);
			}, State);
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="SourceID">Parent Data Source ID.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetChildDataSources(string To, string SourceID, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getChildDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "' src='" + XML.Encode(SourceID) + "'/>", (sender, e) =>
			{
				if (Callback != null)
					this.DataSourcesResponse(e, "getChildDataSourcesResponse", Callback, State);
			}, State);
		}

		/// <summary>
		/// Checks if the concentrator contains a given node (that the user is allowed to see).
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ContainsNode(string To, IThingReference Node, string ServiceToken, string DeviceToken, string UserToken,
			BooleanResponseEventHandler Callback, object State)
		{
			this.ContainsNode(To, Node.NodeId, Node.SourceId, Node.Partition, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Checks if the concentrator contains a given node (that the user is allowed to see).
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ContainsNode(string To, string NodeID, string SourceID, string Partition, string ServiceToken, string DeviceToken, string UserToken,
			BooleanResponseEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<containsNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.BooleanResponse(e, "containsNodeResponse", Callback, State);

			}, State);
		}

		private void BooleanResponse(IqResultEventArgs e, string ExpectedElement, BooleanResponseEventHandler Callback, object State)
		{
			XmlElement E;

			if (!e.Ok || (E = e.FirstElement) == null || E.LocalName != ExpectedElement || !CommonTypes.TryParse(E.InnerText, out bool Response))
			{
				e.Ok = false;
				Response = false;
			}

			if (Callback != null)
			{
				try
				{
					Callback(this, new BooleanResponseEventArgs(Response, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private void AppendNodeAttributes(StringBuilder Xml, string NodeID, string SourceID, string Partition)
		{
			Xml.Append(" id='");
			Xml.Append(XML.Encode(NodeID));

			if (!string.IsNullOrEmpty(SourceID))
			{
				Xml.Append("' src='");
				Xml.Append(XML.Encode(SourceID));
			}

			if (!string.IsNullOrEmpty(Partition))
			{
				Xml.Append("' pt='");
				Xml.Append(XML.Encode(Partition));
			}
		}

		private void AppendTokenAttributes(StringBuilder Xml, string ServiceToken, string DeviceToken, string UserToken)
		{
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
		}

		/// <summary>
		/// Checks if the concentrator contains a set of nodes (that the user is allowed to see).
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Nodes">Nodes</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void ContainsNodes(string To, IThingReference[] Nodes, string ServiceToken, string DeviceToken, string UserToken,
			BooleansResponseEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<containsNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			Xml.Append("'>");

			foreach (IThingReference Node in Nodes)
				this.AppendNode(Xml, Node);

			Xml.Append("</containsNodes>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.BooleansResponse(e, "containsNodesResponse", Callback, State);

			}, State);
		}

		private void AppendNode(StringBuilder Xml, IThingReference Node)
		{
			Xml.Append("<nd");
			this.AppendNodeAttributes(Xml, Node.NodeId, Node.SourceId, Node.Partition);
			Xml.Append("'/>");
		}

		private void BooleansResponse(IqResultEventArgs e, string ExpectedElement, BooleansResponseEventHandler Callback, object State)
		{
			List<bool> Responses = new List<bool>();
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == ExpectedElement && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "value")
					{
						if (CommonTypes.TryParse(E2.InnerText, out bool Value))
							Responses.Add(Value);
						else
							e.Ok = false;
					}
				}
			}
			else
				e.Ok = false;

			if (Callback != null)
			{
				try
				{
					Callback(this, new BooleansResponseEventArgs(Responses.ToArray(), e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Gets information about a node in the concentrator.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNode(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeInformationEventHandler Callback, object State)
		{
			this.GetNode(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about a node in the concentrator.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNode(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodeResponse(e, "getNodeResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		private void AppendNodeInfoAttributes(StringBuilder Xml, bool Parameters, bool Messages, string Language)
		{
			if (Parameters)
			{
				Xml.Append("' parameters='");
				Xml.Append(CommonTypes.Encode(Parameters));
			}

			if (Messages)
			{
				Xml.Append("' messages='");
				Xml.Append(CommonTypes.Encode(Messages));
			}

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append("' xml:lang='");
				Xml.Append(XML.Encode(Language));
			}
		}

		private void NodeResponse(IqResultEventArgs e, string ExpectedElement, bool Parameters, bool Messages,
			NodeInformationEventHandler Callback, object State)
		{
			XmlElement E;
			NodeInformation NodeInfo = null;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == ExpectedElement)
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "nd")
					{
						NodeInfo = this.GetNodeInformation(E2, Parameters, Messages);
						break;
					}
				}

				if (NodeInfo == null)
					e.Ok = false;
			}
			else
			{
				e.Ok = false;
				NodeInfo = null;
			}

			if (Callback != null)
			{
				try
				{
					Callback(this, new NodeInformationEventArgs(NodeInfo, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		private NodeInformation GetNodeInformation(XmlElement E, bool Parameters, bool Messages)
		{
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			string NodeType = XML.Attribute(E, "nodeType");
			string DisplayName = XML.Attribute(E, "displayName");
			NodeState NodeState = (NodeState)XML.Attribute(E, "state", NodeState.None);
			string LocalId = XML.Attribute(E, "localId");
			string LogId = XML.Attribute(E, "logId");
			bool HasChildren = XML.Attribute(E, "hasChildren", false);
			bool ChildrenOrdered = XML.Attribute(E, "childrenOrdered", false);
			bool IsReadable = XML.Attribute(E, "isReadable", false);
			bool IsControllable = XML.Attribute(E, "isControllable", false);
			bool HasCommands = XML.Attribute(E, "hasCommands", false);
			bool Sniffable = XML.Attribute(E, "sniffable", false);
			string ParentId = XML.Attribute(E, "parentId");
			string ParentPartition = XML.Attribute(E, "parentPartition");
			DateTime LastChanged = XML.Attribute(E, "lastChanged", DateTime.MinValue);
			List<Parameter> ParameterList = Parameters ? new List<Parameter>() : null;
			List<Message> MessageList = Messages ? new List<Message>() : null;

			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "boolean":
							string Id = XML.Attribute(E2, "id");
							string Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new BooleanParameter(Id, Name, XML.Attribute(E2, "value", false)));

							break;

						case "color":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							string s = XML.Attribute(E2, "value");
							SKColor Value = SKColors.Transparent;

							if (s.Length == 6)
							{
								if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, null, out byte R) &&
									byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, null, out byte G) &&
									byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, null, out byte B))
								{
									Value = new SKColor(R, G, B);
								}
							}
							else if (s.Length == 8)
							{
								if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, null, out byte R) &&
									byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, null, out byte G) &&
									byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, null, out byte B) &&
									byte.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, null, out byte A))
								{
									Value = new SKColor(R, G, B, A);
								}
							}

							if (ParameterList != null)
								ParameterList.Add(new ColorParameter(Id, Name, Value));

							break;
						case "dateTime":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new DateTimeParameter(Id, Name, XML.Attribute(E2, "value", DateTime.MinValue)));

							break;

						case "double":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new DoubleParameter(Id, Name, XML.Attribute(E2, "value", 0.0)));

							break;

						case "duration":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new DurationParameter(Id, Name, XML.Attribute(E2, "value", Duration.Zero)));

							break;

						case "int":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new Int32Parameter(Id, Name, XML.Attribute(E2, "value", 0)));

							break;

						case "long":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new Int64Parameter(Id, Name, XML.Attribute(E2, "value", 0L)));

							break;

						case "string":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new StringParameter(Id, Name, XML.Attribute(E2, "value")));

							break;

						case "time":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (ParameterList != null)
								ParameterList.Add(new TimeSpanParameter(Id, Name, XML.Attribute(E2, "value", TimeSpan.Zero)));

							break;

						case "message":
							DateTime Timestamp = XML.Attribute(E2, "timestamp", DateTime.MinValue);
							string EventId = XML.Attribute(E2, "eventId");
							Things.DisplayableParameters.MessageType Type = (Things.DisplayableParameters.MessageType)XML.Attribute(E2, "type",
								Things.DisplayableParameters.MessageType.Information);

							if (MessageList != null)
								MessageList.Add(new Message(Timestamp, Type, EventId, E2.InnerText));

							break;
					}
				}
			}

			return new NodeInformation(NodeId, SourceId, Partition, NodeType, DisplayName, NodeState, LocalId, LogId, HasChildren, ChildrenOrdered,
				IsReadable, IsControllable, HasCommands, Sniffable, ParentId, ParentPartition, LastChanged, ParameterList?.ToArray(), MessageList?.ToArray());
		}

		/// <summary>
		/// Gets information about a set of nodes in the concentrator.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Nodes">Node references.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNodes(string To, IThingReference[] Nodes, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'>");

			foreach (IThingReference Node in Nodes)
				this.AppendNode(Xml, Node);

			Xml.Append("</getNodes>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodesResponse(e, "getNodesResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		private void NodesResponse(IqResultEventArgs e, string ExpectedElement, bool Parameters, bool Messages,
			NodesInformationEventHandler Callback, object State)
		{
			XmlElement E;
			NodeInformation[] NodeInfo;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == ExpectedElement)
			{
				List<NodeInformation> Nodes = new List<NodeInformation>();

				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "nd")
						Nodes.Add(this.GetNodeInformation(E2, Parameters, Messages));
				}

				NodeInfo = Nodes.ToArray();
			}
			else
			{
				e.Ok = false;
				NodeInfo = null;
			}

			if (Callback != null)
			{
				try
				{
					Callback(this, new NodesInformationEventArgs(NodeInfo, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Gets information about all nodes in a data source.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="SourceID">Data source ID.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAllNodes(string To, string SourceID, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			this.GetAllNodes(To, SourceID, null, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about all nodes in a data source.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="SourceID">Data source ID.</param>
		/// <param name="OnlyIfDerivedFrom">Array of types nodes must be derived from, to be included in the response.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAllNodes(string To, string SourceID, string[] OnlyIfDerivedFrom, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAllNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);

			if (OnlyIfDerivedFrom != null)
			{
				Xml.Append("'>");

				foreach (string TypeName in OnlyIfDerivedFrom)
				{
					Xml.Append("<onlyIfDerivedFrom>");
					Xml.Append(XML.Encode(TypeName));
					Xml.Append("</onlyIfDerivedFrom>");
				}

				Xml.Append("</getAllNodes>");
			}
			else
				Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodesResponse(e, "getAllNodesResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about the inheritance of a node in the concentrator.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNodeInheritance(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, StringsResponseEventHandler Callback, object State)
		{
			this.GetNodeInheritance(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about the inheritance of a node in the concentrator.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetNodeInheritance(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, StringsResponseEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeInheritance xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append("' xml:lang='");
				Xml.Append(XML.Encode(Language));
			}

			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				List<string> BaseClasses = new List<string>();
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getNodeInheritanceResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					foreach (XmlNode N in E)
					{
						if (N is XmlElement E2 && E2.LocalName == "baseClasses")
						{
							foreach (XmlNode N2 in E2.ChildNodes)
							{
								if (N2 is XmlElement E3 && E3.LocalName == "value")
									BaseClasses.Add(E3.InnerText);
							}
						}
					}
				}
				else
					e.Ok = false;

				if (Callback != null)
				{
					try
					{
						Callback(this, new StringsResponseEventArgs(BaseClasses?.ToArray(), e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, State);
		}

		/// <summary>
		/// Gets information about all root nodes in a data source.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="SourceID">Data source ID.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRootNodes(string To, string SourceID, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getRootNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodesResponse(e, "getRootNodesResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about all root nodes in a data source.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetChildNodes(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			this.GetChildNodes(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language,
				ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about all root nodes in a data source.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetChildNodes(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getChildNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodesResponse(e, "getChildNodesResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about all ancestors of a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAncestors(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			this.GetAncestors(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language,
				ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about all ancestors of a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAncestors(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodesInformationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAncestors xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				this.NodesResponse(e, "getAncestorsResponse", Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets a list of what type of nodes can be added to a given node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAddableNodeTypes(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, LocalizedStringsResponseEventHandler Callback, object State)
		{
			this.GetAddableNodeTypes(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets a list of what type of nodes can be added to a given node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAddableNodeTypes(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, LocalizedStringsResponseEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAddableNodeTypes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				List<LocalizedString> Types = new List<LocalizedString>();
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getAddableNodeTypesResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					foreach (XmlNode N in E)
					{
						if (N is XmlElement E2 && E2.LocalName == "nodeType")
						{
							string Type = XML.Attribute(E2, "type");
							string Name = XML.Attribute(E2, "name");

							Types.Add(new LocalizedString()
							{
								Unlocalized = Type,
								Localized = Name
							});
						}
					}
				}
				else
					e.Ok = false;

				if (Callback != null)
				{
					try
					{
						Callback(this, new LocalizedStringsResponseEventArgs(Types.ToArray(), e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, State);
		}

		/// <summary>
		/// Gets a set of parameters for the creation of a new node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="NodeType">Type of node to create.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="NodeCallback">Method to call when node creation response is returned.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetParametersForNewNode(string To, IThingReference Node, string NodeType, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback,
			NodeInformationEventHandler NodeCallback, object State)
		{
			this.GetParametersForNewNode(To, Node.NodeId, Node.SourceId, Node.Partition, NodeType, Language, ServiceToken, DeviceToken, UserToken,
				FormCallback, NodeCallback, State);
		}

		/// <summary>
		/// Gets a set of parameters for the creation of a new node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="NodeType">Type of node to create.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="NodeCallback">Method to call when node creation response is returned.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetParametersForNewNode(string To, string NodeID, string SourceID, string Partition, string NodeType, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback, NodeInformationEventHandler NodeCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getParametersForNewNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(NodeType));
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getParametersForNewNodeResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					foreach (XmlNode N in E)
					{
						if (N is XmlElement E2 && E2.LocalName == "x")
						{
							Form = new DataForm(this.client, E2, this.CreateNewNode, this.CancelCreateNewNode, e.From, e.To)
							{
								State = e.State
							};
							break;
						}
					}
				}
				else
					e.Ok = false;

				if (FormCallback != null && Form != null)
				{
					try
					{
						FormCallback(this, Form);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, new object[] { To, NodeID, SourceID, Partition, NodeType, Language, ServiceToken, DeviceToken, UserToken, FormCallback, NodeCallback, State });
		}

		private void CreateNewNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			string To = (string)P[0];
			string NodeID = (string)P[1];
			string SourceID = (string)P[2];
			string Partition = (string)P[3];
			string NodeType = (string)P[4];
			string Language = (string)P[5];
			string ServiceToken = (string)P[6];
			string DeviceToken = (string)P[7];
			string UserToken = (string)P[8];
			DataFormEventHandler FormCallback = (DataFormEventHandler)P[9];
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[10];
			object State = P[11];

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<createNewNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(NodeType));
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'>");
			Form.SerializeSubmit(Xml);
			Xml.Append("</createNewNode>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				if (!e.Ok && e.ErrorElement != null && e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.ErrorElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "createNewNodeResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
						{
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "error" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
								{
									string Var = XML.Attribute(E2, "var");
									string ErrorMsg = E2.InnerText;
									Field F = Form[Var];

									if (F != null)
										F.Error = ErrorMsg;
								}
							}
						}
					}

					if (FormCallback != null)
					{
						try
						{
							FormCallback(this, Form);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

						return;
					}
				}

				this.NodeResponse(e, "createNewNodeResponse", true, true, NodeCallback, State);

			}, P);
		}

		private void CancelCreateNewNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[10];
			object State = P[11];

			if (NodeCallback != null)
			{
				try
				{
					NodeCallback(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Destroys a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyNode(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandler Callback, object State)
		{
			this.DestroyNode(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Destroys a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyNode(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<destroyNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Gets the set of parameters for the purpose of editing a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="NodeCallback">Method to call when node creation response is returned.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetNodeParametersForEdit(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback,
			NodeInformationEventHandler NodeCallback, object State)
		{
			this.GetNodeParametersForEdit(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken,
				FormCallback, NodeCallback, State);
		}

		/// <summary>
		/// Gets the set of parameters for the purpose of editing a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="NodeCallback">Method to call when node creation response is returned.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetNodeParametersForEdit(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback, NodeInformationEventHandler NodeCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeParametersForEdit xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "getNodeParametersForEditResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					foreach (XmlNode N in E)
					{
						if (N is XmlElement E2 && E2.LocalName == "x")
						{
							Form = new DataForm(this.client, E2, this.EditNode, this.CancelEditNode, e.From, e.To)
							{
								State = e.State
							};
							break;
						}
					}
				}
				else
					e.Ok = false;

				if (FormCallback != null && Form != null)
				{
					try
					{
						FormCallback(this, Form);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, new object[] { To, NodeID, SourceID, Partition, Language, ServiceToken, DeviceToken, UserToken, FormCallback, NodeCallback, State });
		}

		private void EditNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			string To = (string)P[0];
			string NodeID = (string)P[1];
			string SourceID = (string)P[2];
			string Partition = (string)P[3];
			string Language = (string)P[4];
			string ServiceToken = (string)P[5];
			string DeviceToken = (string)P[6];
			string UserToken = (string)P[7];
			DataFormEventHandler FormCallback = (DataFormEventHandler)P[8];
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[9];
			object State = P[10];

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<setNodeParametersAfterEdit xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'>");
			Form.SerializeSubmit(Xml);
			Xml.Append("</setNodeParametersAfterEdit>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				if (!e.Ok && e.ErrorElement != null && e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.ErrorElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "setNodeParametersAfterEditResponse" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
						{
							foreach (XmlNode N2 in E.ChildNodes)
							{
								if (N2 is XmlElement E2 && E2.LocalName == "error" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
								{
									string Var = XML.Attribute(E2, "var");
									string ErrorMsg = E2.InnerText;
									Field F = Form[Var];

									if (F != null)
										F.Error = ErrorMsg;
								}
							}
						}
					}

					if (FormCallback != null)
					{
						try
						{
							FormCallback(this, Form);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

						return;
					}
				}

				this.NodeResponse(e, "setNodeParametersAfterEditResponse", true, true, NodeCallback, State);

			}, P);
		}

		private void CancelEditNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[10];
			object State = P[11];

			if (NodeCallback != null)
			{
				try
				{
					NodeCallback(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Registers a new sniffer on a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Expires">When the sniffer should expire, if not unregistered before.</param>
		/// <param name="Sniffer">Sniffer to register.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterSniffer(string To, IThingReference Node, DateTime Expires, ISniffer Sniffer,
			string ServiceToken, string DeviceToken, string UserToken, SnifferRegistrationEventHandler Callback, object State)
		{
			this.RegisterSniffer(To, Node.NodeId, Node.SourceId, Node.Partition, Expires, Sniffer, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Registers a new sniffer on a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Expires">When the sniffer should expire, if not unregistered before.</param>
		/// <param name="Sniffer">Sniffer to register.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void RegisterSniffer(string To, string NodeID, string SourceID, string Partition, DateTime Expires, ISniffer Sniffer,
			string ServiceToken, string DeviceToken, string UserToken, SnifferRegistrationEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<registerSniffer xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
			Xml.Append("' expires='");
			Xml.Append(XML.Encode(Expires));
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), (sender, e) =>
			{
				XmlElement E;
				string SnifferId = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "registerSniffer" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					SnifferId = XML.Attribute(E, "snifferId");
					Expires = XML.Attribute(E, "expires", DateTime.MinValue);

					lock (this.sniffers)
					{
						this.sniffers[SnifferId] = Sniffer;
					}
				}
				else
					e.Ok = false;

				if (Callback != null)
				{
					try
					{
						Callback(this, new SnifferRegistrationEventArgs(SnifferId, Expires, e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, State);
		}

		/// <summary>
		/// Registers a new sniffer on a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="SnifferId">ID of sniffer to unregister.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void UnregisterSniffer(string To, IThingReference Node, string SnifferId, string ServiceToken, string DeviceToken, string UserToken,
			IqResultEventHandler Callback, object State)
		{
			this.UnregisterSniffer(To, Node.NodeId, Node.SourceId, Node.Partition, SnifferId, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Registers a new sniffer on a node.
		/// </summary>
		/// <param name="To">Address of server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="SnifferId">ID of sniffer to unregister.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void UnregisterSniffer(string To, string NodeID, string SourceID, string Partition, string SnifferId,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandler Callback, object State)
		{
			lock (this.sniffers)
			{
				this.sniffers.Remove(SnifferId);
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<unregisterSniffer xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
			Xml.Append("' snifferId='");
			Xml.Append(XML.Encode(SnifferId));
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), Callback, State);
		}

		private void SniffMessageHandler(object Sender, MessageEventArgs e)
		{
			string SnifferId = XML.Attribute(e.Content, "snifferId");
			DateTime Timestamp = XML.Attribute(e.Content, "timestamp", DateTime.Now);
			ISniffer Sniffer;

			lock (this.sniffers)
			{
				if (!this.sniffers.TryGetValue(SnifferId, out Sniffer))
					return;
			}

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					try
					{
						switch (E.LocalName)
						{
							case "RxBin":
								byte[] Bin = Convert.FromBase64String(E.InnerText);
								Sniffer.ReceiveBinary(Bin);
								break;

							case "TxBin":
								Bin = Convert.FromBase64String(E.InnerText);
								Sniffer.TransmitBinary(Bin);
								break;

							case "Rx":
								string s = E.InnerText;
								Sniffer.ReceiveText(s);
								break;

							case "Tx":
								s = E.InnerText;
								Sniffer.TransmitText(s);
								break;

							case "Info":
								s = E.InnerText;
								Sniffer.Information(s);
								break;

							case "Warning":
								s = E.InnerText;
								Sniffer.Warning(s);
								break;

							case "Error":
								s = E.InnerText;
								Sniffer.Error(s);
								break;

							case "Exception":
								s = E.InnerText;
								Sniffer.Exception(s);
								break;

							case "Expired":
								lock (this.sniffers)
								{
									this.sniffers.Remove(SnifferId);
								}

								Sniffer.Information("Remote sniffer expired.");
								break;

							default:
								Sniffer.Error("Unrecognized sniffer event received: " + E.OuterXml);
								break;
						}
					}
					catch (Exception)
					{
						Sniffer.Error("Badly encoded sniffer data was received: " + E.OuterXml);
					}
				}
			}
		}

	}
}
