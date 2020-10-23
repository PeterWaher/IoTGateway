using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Things;
using Waher.Things.SourceEvents;
using Waher.Things.DisplayableParameters;
using Waher.Things.Queries;
using System.Runtime.CompilerServices;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator client interface.
	/// 
	/// The interface is defined in the IEEE XMPP IoT extensions:
	/// https://gitlab.com/IEEE-SA/XMPPI/IoT
	/// </summary>
	public class ConcentratorClient : XmppExtension
	{
		private readonly Dictionary<string, ISniffer> sniffers = new Dictionary<string, ISniffer>();
		private readonly Dictionary<string, NodeQuery> queries = new Dictionary<string, NodeQuery>();

		/// <summary>
		/// Implements an XMPP concentrator client interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public ConcentratorClient(XmppClient Client)
			: base(Client)
		{
			Client.RegisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentrator, this.QueryProgressHandler, false);
			Client.RegisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentrator, this.SniffMessageHandler, false);
			Client.RegisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentrator, this.NodeAddedMessageHandler, false);
			Client.RegisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentrator, this.NodeUpdatedMessageHandler, false);
			Client.RegisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentrator, this.NodeRemovedMessageHandler, false);
			Client.RegisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentrator, this.NodeStatusChangedMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentrator, this.NodeMovedUpMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentrator, this.NodeMovedDownMessageHandler, false);
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			Client.UnregisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentrator, this.QueryProgressHandler, false);
			Client.UnregisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentrator, this.SniffMessageHandler, false);
			Client.UnregisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentrator, this.NodeAddedMessageHandler, false);
			Client.UnregisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentrator, this.NodeUpdatedMessageHandler, false);
			Client.UnregisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentrator, this.NodeRemovedMessageHandler, false);
			Client.UnregisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentrator, this.NodeStatusChangedMessageHandler, false);
			Client.UnregisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentrator, this.NodeMovedUpMessageHandler, false);
			Client.UnregisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentrator, this.NodeMovedDownMessageHandler, false);
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0326" };

		/// <summary>
		/// Gets the capabilities of a concentrator server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetCapabilities(string To, CapabilitiesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getCapabilities xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", async (sender, e) =>
			{
				if (!(Callback is null))
				{
					List<string> Capabilities = new List<string>();
					XmlElement E;

					if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "strings" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
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
						await Callback(this, new CapabilitiesEventArgs(Capabilities.ToArray(), e));
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
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetAllDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getAllDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", async (sender, e) =>
			{
				if (!(Callback is null))
					await this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		private async Task DataSourcesResponse(IqResultEventArgs e, DataSourcesEventHandler Callback, object _)
		{
			List<DataSourceReference> DataSources = new List<DataSourceReference>();
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "dataSources" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "dataSource")
						DataSources.Add(new DataSourceReference(E2));
				}
			}
			else
				e.Ok = false;

			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new DataSourcesEventArgs(DataSources.ToArray(), e));
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
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetRootDataSources(string To, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getRootDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "'/>", async (sender, e) =>
			{
				if (!(Callback is null))
					await this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="SourceID">Parent Data Source ID.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void GetChildDataSources(string To, string SourceID, DataSourcesEventHandler Callback, object State)
		{
			this.client.SendIqGet(To, "<getChildDataSources xmlns='" + ConcentratorServer.NamespaceConcentrator + "' src='" + XML.Encode(SourceID) + "'/>", async (sender, e) =>
			{
				if (!(Callback is null))
					await this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		/// <summary>
		/// Checks if the concentrator contains a given node (that the user is allowed to see).
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.BooleanResponse(e, Callback, State);

			}, State);
		}

		private async Task BooleanResponse(IqResultEventArgs e, BooleanResponseEventHandler Callback, object _)
		{
			XmlElement E;

			if (!e.Ok || (E = e.FirstElement) is null || E.LocalName != "bool" || !CommonTypes.TryParse(E.InnerText, out bool Response))
			{
				e.Ok = false;
				Response = false;
			}

			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new BooleanResponseEventArgs(Response, e));
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.BooleansResponse(e, Callback, State);

			}, State);
		}

		private void AppendNode(StringBuilder Xml, IThingReference Node)
		{
			Xml.Append("<nd");
			this.AppendNodeAttributes(Xml, Node.NodeId, Node.SourceId, Node.Partition);
			Xml.Append("'/>");
		}

		private async Task BooleansResponse(IqResultEventArgs e, BooleansResponseEventHandler Callback, object _)
		{
			List<bool> Responses = new List<bool>();
			XmlElement E;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "bools" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "bool")
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

			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new BooleansResponseEventArgs(Responses.ToArray(), e));
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
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.NodeResponse(e, Parameters, Messages, Callback, State);

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

		private async Task NodeResponse(IqResultEventArgs e, bool Parameters, bool Messages, NodeInformationEventHandler Callback, object _)
		{
			XmlElement E;
			NodeInformation NodeInfo = null;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "nodeInfo")
			{
				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "nd")
					{
						NodeInfo = this.GetNodeInformation(E2, Parameters, Messages);
						break;
					}
				}

				if (NodeInfo is null)
					e.Ok = false;
			}
			else
			{
				e.Ok = false;
				NodeInfo = null;
			}

			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new NodeInformationEventArgs(NodeInfo, e));
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

			this.GetParameters(E, ref ParameterList, ref MessageList);

			return new NodeInformation(NodeId, SourceId, Partition, NodeType, DisplayName, NodeState, LocalId, LogId, HasChildren, ChildrenOrdered,
				IsReadable, IsControllable, HasCommands, Sniffable, ParentId, ParentPartition, LastChanged, ParameterList?.ToArray(), MessageList?.ToArray());
		}

		private void GetParameters(XmlElement E, ref List<Parameter> ParameterList, ref List<Message> MessageList)
		{
			foreach (XmlNode N in E.ChildNodes)
			{
				if (N is XmlElement E2)
				{
					switch (E2.LocalName)
					{
						case "boolean":
							string Id = XML.Attribute(E2, "id");
							string Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new BooleanParameter(Id, Name, XML.Attribute(E2, "value", false)));

							break;

						case "color":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							string s = XML.Attribute(E2, "value");
							TryParse(s, out SKColor Value);

							if (!(ParameterList is null))
								ParameterList.Add(new ColorParameter(Id, Name, Value));

							break;
						case "dateTime":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new DateTimeParameter(Id, Name, XML.Attribute(E2, "value", DateTime.MinValue)));

							break;

						case "double":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new DoubleParameter(Id, Name, XML.Attribute(E2, "value", 0.0)));

							break;

						case "duration":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new DurationParameter(Id, Name, XML.Attribute(E2, "value", Duration.Zero)));

							break;

						case "int":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new Int32Parameter(Id, Name, XML.Attribute(E2, "value", 0)));

							break;

						case "long":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new Int64Parameter(Id, Name, XML.Attribute(E2, "value", 0L)));

							break;

						case "string":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new StringParameter(Id, Name, XML.Attribute(E2, "value")));

							break;

						case "time":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							if (!(ParameterList is null))
								ParameterList.Add(new TimeSpanParameter(Id, Name, XML.Attribute(E2, "value", TimeSpan.Zero)));

							break;

						case "message":
							DateTime Timestamp = XML.Attribute(E2, "timestamp", DateTime.MinValue);
							string EventId = XML.Attribute(E2, "eventId");
							Things.DisplayableParameters.MessageType Type = (Things.DisplayableParameters.MessageType)XML.Attribute(E2, "type",
								Things.DisplayableParameters.MessageType.Information);

							if (!(MessageList is null))
								MessageList.Add(new Message(Timestamp, Type, EventId, E2.InnerText));

							break;
					}
				}
			}
		}

		/// <summary>
		/// Tries to parse a color value from its string representation.
		/// </summary>
		/// <param name="s">String representation (RRGGBB or RRGGBBAA) of the color.</param>
		/// <param name="Color">Parse color.</param>
		/// <returns>If a color was successfully parsed.</returns>
		public static bool TryParse(string s, out SKColor Color)
		{
			if (s.Length == 6)
			{
				if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, null, out byte R) &&
					byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, null, out byte G) &&
					byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, null, out byte B))
				{
					Color = new SKColor(R, G, B);
					return true;
				}
			}
			else if (s.Length == 8)
			{
				if (byte.TryParse(s.Substring(0, 2), NumberStyles.HexNumber, null, out byte R) &&
					byte.TryParse(s.Substring(2, 2), NumberStyles.HexNumber, null, out byte G) &&
					byte.TryParse(s.Substring(4, 2), NumberStyles.HexNumber, null, out byte B) &&
					byte.TryParse(s.Substring(6, 2), NumberStyles.HexNumber, null, out byte A))
				{
					Color = new SKColor(R, G, B, A);
					return true;
				}
			}

			Color = SKColors.Transparent;

			return false;
		}

		/// <summary>
		/// Gets information about a set of nodes in the concentrator.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		private async Task NodesResponse(IqResultEventArgs e, bool Parameters, bool Messages,
			NodesInformationEventHandler Callback, object _)
		{
			XmlElement E;
			NodeInformation[] NodeInfo;

			if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "nodeInfos")
			{
				List<NodeInformation> Nodes = new List<NodeInformation>();

				foreach (XmlNode N in E.ChildNodes)
				{
					if (N is XmlElement E2 && E2.LocalName == "nodeInfo")
						Nodes.Add(this.GetNodeInformation(E2, Parameters, Messages));
				}

				NodeInfo = Nodes.ToArray();
			}
			else
			{
				e.Ok = false;
				NodeInfo = null;
			}

			if (!(Callback is null))
			{
				try
				{
					await Callback(this, new NodesInformationEventArgs(NodeInfo, e));
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
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			if (!(OnlyIfDerivedFrom is null))
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
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about the inheritance of a node in the concentrator.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				List<string> BaseClasses = new List<string>();
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "inheritance" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
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

				if (!(Callback is null))
				{
					try
					{
						await Callback(this, new StringsResponseEventArgs(BaseClasses?.ToArray(), e));
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about all root nodes in a data source.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about all ancestors of a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets a list of what type of nodes can be added to a given node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				List<LocalizedString> Types = new List<LocalizedString>();
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "nodeTypes" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
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

				if (!(Callback is null))
				{
					try
					{
						await Callback(this, new LocalizedStringsResponseEventArgs(Types.ToArray(), e));
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
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.CreateNewNode, this.CancelCreateNewNode, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				if (FormCallback != null && Form != null)
				{
					try
					{
						await FormCallback(this, Form);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, new object[] { To, NodeID, SourceID, Partition, NodeType, Language, ServiceToken, DeviceToken, UserToken, FormCallback, NodeCallback, State });
		}

		private Task CreateNewNode(object Sender, DataForm Form)
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

			this.client.SendIqSet(To, Xml.ToString(), async (sender, e) =>
			{
				if (!e.Ok && e.ErrorElement != null && e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.ErrorElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
						{
							Form = new DataForm(this.client, E, this.CreateNewNode, this.CancelCreateNewNode, e.From, e.To)
							{
								State = Form.State
							};
						}
					}

					if (!(FormCallback is null))
					{
						try
						{
							await FormCallback(this, Form);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

						return;
					}
				}

				await this.NodeResponse(e, true, true, NodeCallback, State);
			}, P);

			return Task.CompletedTask;
		}

		private async Task CancelCreateNewNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[10];
			object State = P[11];

			if (!(NodeCallback is null))
			{
				try
				{
					await NodeCallback(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
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
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void DestroyNode(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			this.DestroyNode(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Destroys a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<destroyNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
		}

		/// <summary>
		/// Gets the set of parameters for the purpose of editing a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.EditNode, this.CancelEditNode, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				if (FormCallback != null && Form != null)
				{
					try
					{
						await FormCallback(this, Form);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, new object[] { To, NodeID, SourceID, Partition, Language, ServiceToken, DeviceToken, UserToken, FormCallback, NodeCallback, State });
		}

		private Task EditNode(object Sender, DataForm Form)
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

			this.client.SendIqSet(To, Xml.ToString(), (sender, e) =>
			{
				if (!e.Ok && e.ErrorElement != null && e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.ErrorElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
						{
							Form = new DataForm(this.client, E, this.EditNode, this.CancelEditNode, e.From, e.To)
							{
								State = Form.State
							};
						}
					}

					if (!(FormCallback is null))
					{
						try
						{
							FormCallback(this, Form);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}

						return Task.CompletedTask;
					}
				}

				return this.NodeResponse(e, true, true, NodeCallback, State);

			}, P);

			return Task.CompletedTask;
		}

		private async Task CancelEditNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			NodeInformationEventHandler NodeCallback = (NodeInformationEventHandler)P[9];
			object State = P[10];

			if (!(NodeCallback is null))
			{
				try
				{
					await NodeCallback(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
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
		/// <param name="To">Address of concentrator server.</param>
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
		/// <param name="To">Address of concentrator server.</param>
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

			this.client.SendIqSet(To, Xml.ToString(), async (sender, e) =>
			{
				XmlElement E;
				string SnifferId = null;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "sniffer" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
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

				if (!(Callback is null))
				{
					try
					{
						await Callback(this, new SnifferRegistrationEventArgs(SnifferId, Expires, e));
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
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="SnifferId">ID of sniffer to unregister.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the sniffer was found locally and removed.</returns>
		public bool UnregisterSniffer(string To, IThingReference Node, string SnifferId, string ServiceToken, string DeviceToken, string UserToken,
			IqResultEventHandlerAsync Callback, object State)
		{
			return this.UnregisterSniffer(To, Node.NodeId, Node.SourceId, Node.Partition, SnifferId, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Registers a new sniffer on a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="SnifferId">ID of sniffer to unregister.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <returns>If the sniffer was found locally and removed.</returns>
		public bool UnregisterSniffer(string To, string NodeID, string SourceID, string Partition, string SnifferId,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			bool Result;

			lock (this.sniffers)
			{
				Result = this.sniffers.Remove(SnifferId);
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

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);

			return Result;
		}

		private Task SniffMessageHandler(object Sender, MessageEventArgs e)
		{
			string SnifferId = XML.Attribute(e.Content, "snifferId");
			DateTime Timestamp = XML.Attribute(e.Content, "timestamp", DateTime.Now);
			ISniffer Sniffer;

			lock (this.sniffers)
			{
				if (!this.sniffers.TryGetValue(SnifferId, out Sniffer))
					return Task.CompletedTask;
			}

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					try
					{
						switch (E.LocalName)
						{
							case "rxBin":
								byte[] Bin = Convert.FromBase64String(E.InnerText);
								Sniffer.ReceiveBinary(Timestamp, Bin);
								break;

							case "txBin":
								Bin = Convert.FromBase64String(E.InnerText);
								Sniffer.TransmitBinary(Timestamp, Bin);
								break;

							case "rx":
								string s = E.InnerText;
								Sniffer.ReceiveText(Timestamp, s);
								break;

							case "tx":
								s = E.InnerText;
								Sniffer.TransmitText(Timestamp, s);
								break;

							case "info":
								s = E.InnerText;
								Sniffer.Information(Timestamp, s);
								break;

							case "warning":
								s = E.InnerText;
								Sniffer.Warning(Timestamp, s);
								break;

							case "error":
								s = E.InnerText;
								Sniffer.Error(Timestamp, s);
								break;

							case "exception":
								s = E.InnerText;
								Sniffer.Exception(Timestamp, s);
								break;

							case "expired":
								lock (this.sniffers)
								{
									this.sniffers.Remove(SnifferId);
								}

								Sniffer.Information(Timestamp, "Remote sniffer expired.");
								break;

							default:
								Sniffer.Error("Unrecognized sniffer event received: " + E.OuterXml);
								break;
						}
					}
					catch (Exception)
					{
						Sniffer.Error(Timestamp, "Badly encoded sniffer data was received: " + E.OuterXml);
					}
				}
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Gets available commands for a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetNodeCommands(string To, IThingReference Node,
			string ServiceToken, string DeviceToken, string UserToken, CommandsEventHandler Callback, object State)
		{
			this.GetNodeCommands(To, Node.NodeId, Node.SourceId, Node.Partition, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets available commands for a node.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when process has completed.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public void GetNodeCommands(string To, string NodeID, string SourceID, string Partition,
			string ServiceToken, string DeviceToken, string UserToken, CommandsEventHandler Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeCommands xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				XmlElement E;
				List<NodeCommand> Commands = new List<NodeCommand>();

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "commands" && E.NamespaceURI == ConcentratorServer.NamespaceConcentrator)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "command")
						{
							string Command = XML.Attribute(E2, "command");
							string Name = XML.Attribute(E2, "name");
							CommandType Type = (CommandType)XML.Attribute(E2, "type", CommandType.Simple);
							string SuccessString = XML.Attribute(E2, "successString");
							string FailureString = XML.Attribute(E2, "failureString");
							string ConfirmationString = XML.Attribute(E2, "confirmationString");
							string SortCategory = XML.Attribute(E2, "sortCategory");
							string SortKey = XML.Attribute(E2, "sortKey");

							Commands.Add(new NodeCommand(Command, Name, Type, SuccessString, FailureString, ConfirmationString, SortCategory, SortKey));
						}
					}

					Commands.Sort(this.CompareCommands);
				}
				else
					e.Ok = false;

				if (!(Callback is null))
				{
					try
					{
						await Callback(this, new CommandsEventArgs(Commands.ToArray(), e));
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, State);
		}

		private int CompareCommands(NodeCommand Cmd1, NodeCommand Cmd2)
		{
			int i = Cmd1.SortCategory.CompareTo(Cmd2.SortCategory);
			if (i != 0)
				return i;

			return Cmd1.SortKey.CompareTo(Cmd2.SortKey);
		}

		/// <summary>
		/// Gets the set of parameters for a parametrized command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="CommandCallback">Method to call after executing command.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetCommandParameters(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback,
			NodeCommandResponseEventHandler CommandCallback, object State)
		{
			this.GetCommandParameters(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Language, ServiceToken, DeviceToken, UserToken,
				FormCallback, CommandCallback, null, State);
		}

		/// <summary>
		/// Gets the set of parameters for a parametrized command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="CommandCallback">Method to call after executing command.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetCommandParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback, NodeCommandResponseEventHandler CommandCallback, object State)
		{
			this.GetCommandParameters(To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken, FormCallback, CommandCallback, null, State);
		}

		/// <summary>
		/// Gets the set of parameters for a parametrized query.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="QueryCallback">Method to call when query execution has begun.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetQueryParameters(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback,
			NodeQueryResponseEventHandler QueryCallback, object State)
		{
			this.GetCommandParameters(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Language, ServiceToken, DeviceToken, UserToken,
				FormCallback, null, QueryCallback, State);
		}

		/// <summary>
		/// Gets the set of parameters for a parametrized query.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="FormCallback">Method to call when parameter form is returned.</param>
		/// <param name="QueryCallback">Method to call when query execution has begun.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void GetQueryParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback, NodeQueryResponseEventHandler QueryCallback, object State)
		{
			this.GetCommandParameters(To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken, FormCallback, null, QueryCallback, State);
		}

		private void GetCommandParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, DataFormEventHandler FormCallback, NodeCommandResponseEventHandler CommandCallback,
			NodeQueryResponseEventHandler QueryCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getCommandParameters xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("' command='");
			Xml.Append(XML.Encode(Command));
			Xml.Append("'/>");

			this.client.SendIqGet(To, Xml.ToString(), async (sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && (E = e.FirstElement) != null && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.EditCommandParameters, this.CancelEditCommandParameters, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				if (FormCallback != null && Form != null)
				{
					try
					{
						await FormCallback(this, Form);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}

			}, new object[] { To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken, FormCallback, CommandCallback, QueryCallback, State });
		}

		private Task EditCommandParameters(object _, DataForm Form)
		{
			object[] P = (object[])Form.State;
			string To = (string)P[0];
			string NodeID = (string)P[1];
			string SourceID = (string)P[2];
			string Partition = (string)P[3];
			string Command = (string)P[4];
			string Language = (string)P[5];
			string ServiceToken = (string)P[6];
			string DeviceToken = (string)P[7];
			string UserToken = (string)P[8];
			DataFormEventHandler FormCallback = (DataFormEventHandler)P[9];
			NodeCommandResponseEventHandler CommandCallback = (NodeCommandResponseEventHandler)P[10];
			NodeQueryResponseEventHandler QueryCallback = (NodeQueryResponseEventHandler)P[11];
			object State = P[12];

			if (!(CommandCallback is null))
				this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Form, Language, ServiceToken, DeviceToken, UserToken, CommandCallback, State);
			else
				this.ExecuteQuery(To, NodeID, SourceID, Partition, Command, Form, Language, ServiceToken, DeviceToken, UserToken, QueryCallback, State);
		
			return Task.CompletedTask;
		}

		private async Task CancelEditCommandParameters(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			NodeCommandResponseEventHandler CommandCallback = (NodeCommandResponseEventHandler)P[10];
			object State = P[11];

			if (!(CommandCallback is null))
			{
				try
				{
					await CommandCallback(this, new NodeCommandResponseEventArgs(Form, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Executes a node command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void ExecuteCommand(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeCommandResponseEventHandler Callback, object State)
		{
			this.ExecuteCommand(To, Node.NodeId, Node.SourceId, Node.Partition, Command, null, null, Language, ServiceToken, DeviceToken, UserToken,
				Callback, null, State);
		}

		/// <summary>
		/// Executes a node command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeCommandResponseEventHandler Callback, object State)
		{
			this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, null, null, Language, ServiceToken, DeviceToken, UserToken, Callback, null, State);
		}

		/// <summary>
		/// Executes a node command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Parameters">Command parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void ExecuteCommand(string To, IThingReference Node, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeCommandResponseEventHandler Callback, object State)
		{
			this.ExecuteCommand(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Parameters, null, Language, ServiceToken, DeviceToken, UserToken,
				Callback, null, State);
		}

		/// <summary>
		/// Executes a node command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Parameters">Command parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeCommandResponseEventHandler Callback, object State)
		{
			this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Parameters, null, Language, ServiceToken, DeviceToken, UserToken, Callback, null, State);
		}


		/// <summary>
		/// Executes a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		/// <returns>Node query object where results will be made available</returns>
		public NodeQuery ExecuteQuery(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeQueryResponseEventHandler Callback, object State)
		{
			return this.ExecuteQuery(To, Node.NodeId, Node.SourceId, Node.Partition, Command, null, Language, ServiceToken, DeviceToken, UserToken,
				Callback, State);
		}

		/// <summary>
		/// Executes a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		/// <returns>Node query object where results will be made available</returns>
		public NodeQuery ExecuteQuery(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeQueryResponseEventHandler Callback, object State)
		{
			return this.ExecuteQuery(To, NodeID, SourceID, Partition, Command, null, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Executes a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Parameters">Command parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		/// <returns>Node query object where results will be made available</returns>
		public NodeQuery ExecuteQuery(string To, IThingReference Node, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeQueryResponseEventHandler Callback, object State)
		{
			return this.ExecuteQuery(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Parameters, Language, ServiceToken, DeviceToken, UserToken,
				Callback, State);
		}

		/// <summary>
		/// Executes a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="Parameters">Command parameters.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		/// <returns>Node query object where results will be made available</returns>
		public NodeQuery ExecuteQuery(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeQueryResponseEventHandler Callback, object State)
		{
			NodeQuery Query;

			lock (this.queries)
			{
				do
				{
					Query = new NodeQuery(this, To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken);
				}
				while (this.queries.ContainsKey(Query.QueryId));

				this.queries[Query.QueryId] = Query;
			}

			this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Parameters, Query, Language, ServiceToken, DeviceToken, UserToken, null, Callback, State);

			return Query;
		}

		private void ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, NodeQuery Query,
			string Language, string ServiceToken, string DeviceToken, string UserToken, NodeCommandResponseEventHandler CommandCallback,
			NodeQueryResponseEventHandler QueryCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();
			string TagName;

			if (!(Query is null))
				TagName = "executeNodeQuery";
			else
				TagName = "executeNodeCommand";

			Xml.Append('<');
			Xml.Append(TagName);
			Xml.Append(" xmlns='");

			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("' command='");
			Xml.Append(XML.Encode(Command));

			if (!(Query is null))
			{
				Xml.Append("' queryId='");
				Xml.Append(XML.Encode(Query.QueryId));
			}

			if (!(Parameters is null))
			{
				Xml.Append("'>");
				Parameters.SerializeSubmit(Xml);
				Xml.Append("</");
				Xml.Append(TagName);
				Xml.Append('>');
			}
			else
				Xml.Append("'/>");

			this.client.SendIqSet(To, Xml.ToString(), async (sender, e) =>
			{
				if (!e.Ok && e.ErrorElement != null && e.ErrorType == ErrorType.Modify)
				{
					foreach (XmlNode N in e.ErrorElement.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
						{
							Parameters = new DataForm(this.client, E, this.EditCommandParameters, this.CancelEditCommandParameters, e.From, e.To)
							{
								State = Parameters?.State
							};
						}
					}
				}

				try
				{
					if (!(CommandCallback is null))
						await CommandCallback(this, new NodeCommandResponseEventArgs(Parameters, e));
					else
						await QueryCallback(this, new NodeQueryResponseEventArgs(Query, Parameters, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

			}, State);
		}

		/// <summary>
		/// Aborts a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="QueryId">Query ID</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void AbortQuery(string To, IThingReference Node, string Command, string QueryId,
			string Language, string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			this.AbortQuery(To, Node.NodeId, Node.SourceId, Node.Partition, Command, QueryId, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Aborts a node query command.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="NodeID">Node ID</param>
		/// <param name="SourceID">Optional Source ID</param>
		/// <param name="Partition">Optional Partition</param>
		/// <param name="Command">Command for which to get parameters.</param>
		/// <param name="QueryId">Query ID</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public void AbortQuery(string To, string NodeID, string SourceID, string Partition, string Command, string QueryId,
			string Language, string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			lock (this.queries)
			{
				if (this.queries.TryGetValue(QueryId, out NodeQuery Query) &&
					Query.To == To && Query.NodeID == NodeID && Query.SourceID == SourceID && Query.Partition == Partition &&
					Query.Command == Command)
				{
					this.queries.Remove(QueryId);
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<abortNodeQuery xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("' command='");
			Xml.Append(XML.Encode(Command));
			Xml.Append("' queryId='");
			Xml.Append(XML.Encode(QueryId));
			Xml.Append("'/>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
		}

		private Task QueryProgressHandler(object Sender, MessageEventArgs e)
		{
			string NodeId = XML.Attribute(e.Content, "id");
			string SourceId = XML.Attribute(e.Content, "src");
			string Partition = XML.Attribute(e.Content, "pt");
			string QueryId = XML.Attribute(e.Content, "queryId");
			int SequenceNr = XML.Attribute(e.Content, "seqNr", 0);
			NodeQuery Query;

			lock (this.queries)
			{
				if (!this.queries.TryGetValue(QueryId, out Query))
					return Task.CompletedTask;
			}

			if (Query.NodeID != NodeId || Query.SourceID != SourceId || Query.Partition != Partition)
				return Task.CompletedTask;

			int ExpectedSeqNr = Query.SequenceNr + 1;
			if (SequenceNr < ExpectedSeqNr)
				return Task.CompletedTask;

			if (SequenceNr == ExpectedSeqNr)
			{
				Query.NextSequenceNr();
				this.ProcessQueryProgress(Query, e);

				if (Query.HasQueued)
				{
					ExpectedSeqNr++;

					e = Query.PopQueued(ExpectedSeqNr);
					while (e != null)
					{
						this.ProcessQueryProgress(Query, e);
						ExpectedSeqNr++;
						e = Query.PopQueued(ExpectedSeqNr);
					}
				}
			}
			else
				Query.Queue(SequenceNr, e);

			return Task.CompletedTask;
		}

		private void ProcessQueryProgress(NodeQuery Query, MessageEventArgs e)
		{
			string s, s2;

			foreach (XmlNode N in e.Content.ChildNodes)
			{
				if (N is XmlElement E)
				{
					try
					{
						switch (E.LocalName)
						{
							case "title":
								s = XML.Attribute(E, "name");
								Query.SetTitle(s, e);
								break;

							case "tableDone":
								s = XML.Attribute(E, "tableId");
								Query.TableDone(s, e);
								break;

							case "status":
								s = XML.Attribute(E, "message");
								Query.StatusMessage(s, e);
								break;

							case "queryStarted":
								Query.ReportStarted(e);
								break;

							case "newTable":
								s = XML.Attribute(E, "tableId");
								s2 = XML.Attribute(E, "tableName");

								List<Column> Columns = new List<Column>();

								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "column")
									{
										string ColumnId = XML.Attribute(E2, "columnId");
										string Header = XML.Attribute(E2, "header");
										string SourceID = XML.Attribute(E2, "src");
										string Partition2 = XML.Attribute(E2, "pt");
										SKColor? FgColor = null;
										SKColor? BgColor = null;
										ColumnAlignment? ColumnAlignment = null;
										byte? NrDecimals = null;

										if (E2.HasAttribute("fgColor") && TryParse(E2.GetAttribute("fgColor"), out SKColor Color))
											FgColor = Color;

										if (E2.HasAttribute("bgColor") && TryParse(E2.GetAttribute("bgColor"), out Color))
											BgColor = Color;

										if (E2.HasAttribute("alignment") && Enum.TryParse<ColumnAlignment>(E2.GetAttribute("alignment"), out ColumnAlignment ColumnAlignment2))
											ColumnAlignment = ColumnAlignment2;

										if (E2.HasAttribute("nrDecimals") && byte.TryParse(E2.GetAttribute("nrDecimals"), out byte b))
											NrDecimals = b;

										Columns.Add(new Column(ColumnId, Header, SourceID, Partition2, FgColor, BgColor, ColumnAlignment, NrDecimals));
									}
								}

								Query.NewTable(new Table(s, s2, Columns.ToArray()), e);
								break;

							case "newRecords":
								s = XML.Attribute(E, "tableId");

								List<Record> Records = new List<Record>();
								List<object> Record = null;

								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2 is XmlElement E2 && E2.LocalName == "record")
									{
										if (Record is null)
											Record = new List<object>();
										else
											Record.Clear();

										foreach (XmlNode N3 in E2.ChildNodes)
										{
											if (N3 is XmlElement E3)
											{
												switch (E3.LocalName)
												{
													case "void":
														Record.Add(null);
														break;

													case "boolean":
														if (CommonTypes.TryParse(E3.InnerText, out bool b))
															Record.Add(b);
														else
															Record.Add(null);
														break;

													case "color":
														if (TryParse(E3.InnerText, out SKColor Color))
															Record.Add(Color);
														else
															Record.Add(null);
														break;

													case "date":
													case "dateTime":
														if (XML.TryParse(E3.InnerText, out DateTime TP))
															Record.Add(TP);
														else
															Record.Add(null);
														break;

													case "double":
														if (CommonTypes.TryParse(E3.InnerText, out double d))
															Record.Add(d);
														else
															Record.Add(null);
														break;

													case "duration":
														if (Duration.TryParse(E3.InnerText, out Duration d2))
															Record.Add(d2);
														else
															Record.Add(null);
														break;

													case "int":
														if (int.TryParse(E3.InnerText, out int i))
															Record.Add(i);
														else
															Record.Add(null);
														break;

													case "long":
														if (long.TryParse(E3.InnerText, out long l))
															Record.Add(l);
														else
															Record.Add(null);
														break;

													case "string":
														Record.Add(E3.InnerText);
														break;

													case "time":
														if (TimeSpan.TryParse(E3.InnerText, out TimeSpan TS))
															Record.Add(TS);
														else
															Record.Add(null);
														break;

													case "base64":
														try
														{
															string ContentType = XML.Attribute(E3, "contentType");
															byte[] Bin = Convert.FromBase64String(E3.InnerText);
															object Decoded = InternetContent.Decode(ContentType, Bin, null);

															Record.Add(Decoded);
														}
														catch (Exception ex)
														{
															Query.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
															Record.Add(null);
														}
														break;

													default:
														Record.Add(null);
														break;
												}
											}
										}

										Records.Add(new Record(Record.ToArray()));
									}
								}

								Query.NewRecords(s, Records.ToArray(), e);
								break;

							case "newObject":
								try
								{
									string ContentType = XML.Attribute(E, "contentType");
									byte[] Bin = Convert.FromBase64String(E.InnerText);
									object Decoded = InternetContent.Decode(ContentType, Bin, null);

									Query.NewObject(Decoded, e);
								}
								catch (Exception ex)
								{
									Query.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
								}
								break;

							case "queryMessage":
								QueryEventType Type = (QueryEventType)XML.Attribute(E, "type", QueryEventType.Information);
								QueryEventLevel Level = (QueryEventLevel)XML.Attribute(E, "level", QueryEventLevel.Minor);

								Query.QueryMessage(Type, Level, E.InnerText, e);
								break;

							case "endSection":
								Query.EndSection(e);
								break;

							case "queryDone":
								Query.ReportDone(e);
								break;

							case "beginSection":
								s = XML.Attribute(E, "header");
								Query.BeginSection(s, e);
								break;

							case "queryAborted":
								Query.ReportAborted(e);
								break;

							default:
								Query.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, "Unrecognized sniffer event received: " + E.OuterXml, e);
								break;
						}
					}
					catch (Exception ex)
					{
						Query.QueryMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message, e);
					}
				}
			}
		}

		/// <summary>
		/// Subscribes to data source events.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="SourceID">ID of data source to subscribe to.</param>
		/// <param name="TimeoutSeconds">Timeout of subscription, in seconds. To maintain a subscription for longer time,
		/// send a new subscription before the time elapses.</param>
		/// <param name="EventTypes">Types of events to subscribe to. (Default=<see cref="SourceEventType.All"/>)</param>
		/// <param name="Parameters">If node parameters should be included in corresponding node events. (Default=true)</param>
		/// <param name="Messages">If node messages should be included in corresponding node events. (Default=true)</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Subscribe(string To, string SourceID, int TimeoutSeconds, SourceEventType EventTypes, bool Parameters, bool Messages,
			string Language, string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			if (TimeoutSeconds <= 0)
				throw new ArgumentException("Timeout must be positive.", nameof(TimeoutSeconds));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<subscribe xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			Xml.Append("' ttl='");
			Xml.Append(TimeoutSeconds.ToString());
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			this.AppendEventTypeAttributes(Xml, EventTypes);
			Xml.Append("'/>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
		}

		private void AppendEventTypeAttributes(StringBuilder Xml, SourceEventType EventTypes)
		{
			if (!EventTypes.HasFlag(SourceEventType.NodeAdded))
				Xml.Append("' nodeAdded='false");

			if (!EventTypes.HasFlag(SourceEventType.NodeUpdated))
				Xml.Append("' nodeUpdated='false");

			if (!EventTypes.HasFlag(SourceEventType.NodeStatusChanged))
				Xml.Append("' nodeStatusChanged='false");

			if (!EventTypes.HasFlag(SourceEventType.NodeRemoved))
				Xml.Append("' nodeRemoved='false");

			if (!EventTypes.HasFlag(SourceEventType.NodeMovedUp))
				Xml.Append("' nodeMovedUp='false");

			if (!EventTypes.HasFlag(SourceEventType.NodeMovedDown))
				Xml.Append("' nodeMovedDown='false");
		}

		/// <summary>
		/// Unsubscribes from data source events.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="SourceID">ID of data source to subscribe to.</param>
		/// <param name="EventTypes">Types of events to subscribe to. (Default=<see cref="SourceEventType.All"/>)</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public void Unsubscribe(string To, string SourceID, SourceEventType EventTypes, string Language,
			string ServiceToken, string DeviceToken, string UserToken, IqResultEventHandlerAsync Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<unsubscribe xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentrator);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			this.AppendEventTypeAttributes(Xml, EventTypes);
			Xml.Append("'/>");

			this.client.SendIqSet(To, Xml.ToString(), Callback, State);
		}

		private Task NodeAddedMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");
			List<Parameter> ParameterList = new List<Parameter>();
			List<Message> MessageList = null;

			this.GetParameters(E, ref ParameterList, ref MessageList);

			return this.SourceEventReceived(new NodeAdded()
			{
				AfterNodeId = XML.Attribute(E, "aid"),
				AfterPartition = XML.Attribute(E, "apt"),
				NodeType = XML.Attribute(E, "nodeType"),
				DisplayName = XML.Attribute(E, "displayName"),
				Sniffable = XML.Attribute(E, "sniffable", false),
				HasChildren = XML.Attribute(E, "hasChildren", false),
				IsReadable = XML.Attribute(E, "isReadable", false),
				IsControllable = XML.Attribute(E, "isControllable", false),
				HasCommands = XML.Attribute(E, "hasCommands", false),
				ChildrenOrdered = XML.Attribute(E, "childrenOrdered", false),
				ParentId = XML.Attribute(E, "parentId"),
				ParentPartition = XML.Attribute(E, "parentPartition"),
				Updated = XML.Attribute(E, "lastChanged", DateTime.MinValue),
				State = (NodeState)XML.Attribute(E, "state", NodeState.None),
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue),
				Parameters = ParameterList?.ToArray()
			}, e);
		}

		private Task NodeUpdatedMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");
			List<Parameter> ParameterList = new List<Parameter>();
			List<Message> MessageList = null;

			this.GetParameters(E, ref ParameterList, ref MessageList);

			return this.SourceEventReceived(new NodeUpdated()
			{
				OldId = XML.Attribute(E, "oid", NodeId),
				HasChildren = XML.Attribute(E, "hasChildren", false),
				IsReadable = XML.Attribute(E, "isReadable", false),
				IsControllable = XML.Attribute(E, "isControllable", false),
				HasCommands = XML.Attribute(E, "hasCommands", false),
				ChildrenOrdered = XML.Attribute(E, "childrenOrdered", false),
				ParentId = XML.Attribute(E, "parentId"),
				ParentPartition = XML.Attribute(E, "parentPartition"),
				Updated = XML.Attribute(E, "lastChanged", DateTime.MinValue),
				State = (NodeState)XML.Attribute(E, "state", NodeState.None),
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue),
				Parameters = ParameterList?.ToArray()
			}, e);
		}

		private Task NodeRemovedMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");

			return this.SourceEventReceived(new NodeRemoved()
			{
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue)
			}, e);
		}

		private Task NodeStatusChangedMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");
			List<Parameter> ParameterList = null;
			List<Message> MessageList = new List<Message>();

			this.GetParameters(E, ref ParameterList, ref MessageList);

			return this.SourceEventReceived(new NodeStatusChanged()
			{
				State = (NodeState)XML.Attribute(E, "state", NodeState.None),
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue),
				Messages = MessageList?.ToArray()
			}, e);
		}

		private Task NodeMovedUpMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");

			return this.SourceEventReceived(new NodeMovedUp()
			{
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue)
			}, e);
		}

		private Task NodeMovedDownMessageHandler(object Sender, MessageEventArgs e)
		{
			XmlElement E = e.Content;
			string NodeId = XML.Attribute(E, "id");

			return this.SourceEventReceived(new NodeMovedDown()
			{
				NodeId = NodeId,
				Partition = XML.Attribute(E, "pt"),
				LogId = XML.Attribute(E, "logId", NodeId),
				LocalId = XML.Attribute(E, "localId", NodeId),
				SourceId = XML.Attribute(E, "src"),
				Timestamp = XML.Attribute(E, "ts", DateTime.MinValue)
			}, e);
		}

		private async Task SourceEventReceived(SourceEvent Event, MessageEventArgs e)
		{
			SourceEventMessageEventHandler h = this.OnEvent;
			if (!(h is null))
			{
				try
				{
					await h(this, new SourceEventMessageEventArgs(Event, e));
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised when a data source event has been received.
		/// </summary>
		public event SourceEventMessageEventHandler OnEvent = null;

	}
}
