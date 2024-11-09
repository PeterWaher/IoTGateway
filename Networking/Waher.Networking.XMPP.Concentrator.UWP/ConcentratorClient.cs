using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Concentrator.Queries;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;
using Waher.Things;
using Waher.Things.DisplayableParameters;
using Waher.Things.SourceEvents;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator client interface.
	/// 
	/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
	/// https://neuro-foundation.io
	/// </summary>
	public class ConcentratorClient : XmppExtension
	{
		private readonly Dictionary<string, ISniffer> sniffers = new Dictionary<string, ISniffer>();
		private readonly Dictionary<string, NodeQuery> queries = new Dictionary<string, NodeQuery>();

		/// <summary>
		/// Implements an XMPP concentrator client interface.
		/// 
		/// The interface is defined in the Neuro-Foundation XMPP IoT extensions:
		/// https://neuro-foundation.io
		/// </summary>
		/// <param name="Client">XMPP Client.</param>
		public ConcentratorClient(XmppClient Client)
			: base(Client)
		{
			#region Neuro-Foundation V1 handlers

			Client.RegisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.QueryProgressHandler, false);
			Client.RegisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.SniffMessageHandler, false);
			Client.RegisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeAddedMessageHandler, false);
			Client.RegisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeUpdatedMessageHandler, false);
			Client.RegisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeRemovedMessageHandler, false);
			Client.RegisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeStatusChangedMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeMovedUpMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeMovedDownMessageHandler, false);

			#endregion

			#region IEEE V1 handlers

			Client.RegisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentratorIeeeV1, this.QueryProgressHandler, false);
			Client.RegisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentratorIeeeV1, this.SniffMessageHandler, false);
			Client.RegisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeAddedMessageHandler, false);
			Client.RegisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeUpdatedMessageHandler, false);
			Client.RegisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeRemovedMessageHandler, false);
			Client.RegisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeStatusChangedMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeMovedUpMessageHandler, false);
			Client.RegisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeMovedDownMessageHandler, false);

			#endregion
		}

		/// <summary>
		/// Disposes of the extension.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			#region Neuro-Foundation V1 handlers

			this.Client.UnregisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.QueryProgressHandler, false);
			this.Client.UnregisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.SniffMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeAddedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeUpdatedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeRemovedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeStatusChangedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeMovedUpMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentratorNeuroFoundationV1, this.NodeMovedDownMessageHandler, false);

			#endregion

			#region IEEE V1 handlers

			this.Client.UnregisterMessageHandler("queryProgress", ConcentratorServer.NamespaceConcentratorIeeeV1, this.QueryProgressHandler, false);
			this.Client.UnregisterMessageHandler("sniff", ConcentratorServer.NamespaceConcentratorIeeeV1, this.SniffMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeAdded", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeAddedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeUpdated", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeUpdatedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeRemoved", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeRemovedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeStatusChanged", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeStatusChangedMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeMovedUp", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeMovedUpMessageHandler, false);
			this.Client.UnregisterMessageHandler("nodeMovedDown", ConcentratorServer.NamespaceConcentratorIeeeV1, this.NodeMovedDownMessageHandler, false);

			#endregion
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
		public Task GetCapabilities(string To, EventHandlerAsync<CapabilitiesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(To, "<getCapabilities xmlns='" + ConcentratorServer.NamespaceConcentratorCurrent + "'/>", async (Sender, e) =>
			{
				List<string> Capabilities = new List<string>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "strings" &&
					E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
				{
					foreach (XmlNode N in E)
					{
						if (N.LocalName == "value")
							Capabilities.Add(N.InnerText);
					}
				}
				else
					e.Ok = false;

				await Callback.Raise(this, new CapabilitiesEventArgs(Capabilities.ToArray(), e));
			}, State);
		}

		/// <summary>
		/// Gets all data sources from the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetAllDataSources(string To, EventHandlerAsync<DataSourcesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(To, "<getAllDataSources xmlns='" + ConcentratorServer.NamespaceConcentratorCurrent + "'/>", (Sender, e) =>
			{
				return this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		private async Task DataSourcesResponse(IqResultEventArgs e, EventHandlerAsync<DataSourcesEventArgs> Callback, object _)
		{
			List<DataSourceReference> DataSources = new List<DataSourceReference>();
			XmlElement E;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "dataSources" &&
				E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
			{
				foreach (XmlNode N in E)
				{
					if (N is XmlElement E2 && E2.LocalName == "dataSource")
						DataSources.Add(new DataSourceReference(E2));
				}
			}
			else
				e.Ok = false;

			await Callback.Raise(this, new DataSourcesEventArgs(DataSources.ToArray(), e));
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetRootDataSources(string To, EventHandlerAsync<DataSourcesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(To, "<getRootDataSources xmlns='" + ConcentratorServer.NamespaceConcentratorCurrent + "'/>", (Sender, e) =>
			{
				return this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		/// <summary>
		/// Gets all root data sources from the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <returns>Result</returns>
		public async Task<DataSourceReference[]> GetRootDataSourcesAsync(string To)
		{
			TaskCompletionSource<DataSourceReference[]> Result = new TaskCompletionSource<DataSourceReference[]>();

			await this.GetRootDataSources(To, (_, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.DataSources);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get root data sources."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets all child data sources for a data source on the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="SourceID">Parent Data Source ID.</param>
		/// <param name="Callback">Method to call when response is returned.</param>
		/// <param name="State">State object to pass on to callback method.</param>
		public Task GetChildDataSources(string To, string SourceID, EventHandlerAsync<DataSourcesEventArgs> Callback, object State)
		{
			return this.client.SendIqGet(To, "<getChildDataSources xmlns='" + ConcentratorServer.NamespaceConcentratorCurrent + "' src='" + XML.Encode(SourceID) + "'/>", (Sender, e) =>
			{
				return this.DataSourcesResponse(e, Callback, State);
			}, State);
		}

		/// <summary>
		/// Gets all child data sources for a data source on the server.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="SourceID">Parent Data Source ID.</param>
		/// <returns>Result</returns>
		public async Task<DataSourceReference[]> GetChildDataSourcesAsync(string To, string SourceID)
		{
			TaskCompletionSource<DataSourceReference[]> Result = new TaskCompletionSource<DataSourceReference[]>();

			await this.GetChildDataSources(To, SourceID, (_, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.DataSources);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get child data sources."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
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
		public Task ContainsNode(string To, IThingReference Node, string ServiceToken, string DeviceToken, string UserToken,
			EventHandlerAsync<BooleanResponseEventArgs> Callback, object State)
		{
			return this.ContainsNode(To, Node.NodeId, Node.SourceId, Node.Partition, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task ContainsNode(string To, string NodeID, string SourceID, string Partition, string ServiceToken, string DeviceToken, string UserToken,
			EventHandlerAsync<BooleanResponseEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<containsNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
			{
				return this.BooleanResponse(e, Callback, State);

			}, State);
		}

		private async Task BooleanResponse(IqResultEventArgs e, EventHandlerAsync<BooleanResponseEventArgs> Callback, object _)
		{
			XmlElement E;

			if (!e.Ok || (E = e.FirstElement) is null || E.LocalName != "bool" || !CommonTypes.TryParse(E.InnerText, out bool Response))
			{
				e.Ok = false;
				Response = false;
			}

			await Callback.Raise(this, new BooleanResponseEventArgs(Response, e));
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
		public Task ContainsNodes(string To, IThingReference[] Nodes, string ServiceToken, string DeviceToken, string UserToken,
			EventHandlerAsync<BooleansResponseEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<containsNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			Xml.Append("'>");

			foreach (IThingReference Node in Nodes)
				this.AppendNode(Xml, Node);

			Xml.Append("</containsNodes>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
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

		private async Task BooleansResponse(IqResultEventArgs e, EventHandlerAsync<BooleansResponseEventArgs> Callback, object _)
		{
			List<bool> Responses = new List<bool>();
			XmlElement E;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "bools" &&
				E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
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

			await Callback.Raise(this, new BooleansResponseEventArgs(Responses.ToArray(), e));
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
		public Task GetNode(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeInformationEventArgs> Callback, object State)
		{
			return this.GetNode(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task GetNode(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
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

		private async Task NodeResponse(IqResultEventArgs e, bool Parameters, bool Messages, EventHandlerAsync<NodeInformationEventArgs> Callback, object _)
		{
			XmlElement E;
			NodeInformation NodeInfo;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "nodeInfo")
				NodeInfo = this.GetNodeInformation(E, Parameters, Messages);
			else
			{
				e.Ok = false;
				NodeInfo = null;
			}

			await Callback.Raise(this, new NodeInformationEventArgs(NodeInfo, e));
		}

		private NodeInformation GetNodeInformation(XmlElement E, bool Parameters, bool Messages)
		{
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");
			string NodeType = XML.Attribute(E, "nodeType");
			string DisplayName = XML.Attribute(E, "displayName");
			NodeState NodeState = XML.Attribute(E, "state", NodeState.None);
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

							ParameterList?.Add(new BooleanParameter(Id, Name, XML.Attribute(E2, "value", false)));
							break;

						case "color":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							string s = XML.Attribute(E2, "value");
							TryParse(s, out SKColor Value);

							ParameterList?.Add(new ColorParameter(Id, Name, Value));
							break;

						case "dateTime":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new DateTimeParameter(Id, Name, XML.Attribute(E2, "value", DateTime.MinValue)));
							break;

						case "double":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new DoubleParameter(Id, Name, XML.Attribute(E2, "value", 0.0)));
							break;

						case "duration":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new DurationParameter(Id, Name, XML.Attribute(E2, "value", Duration.Zero)));
							break;

						case "int":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new Int32Parameter(Id, Name, XML.Attribute(E2, "value", 0)));
							break;

						case "long":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new Int64Parameter(Id, Name, XML.Attribute(E2, "value", 0L)));
							break;

						case "string":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new StringParameter(Id, Name, XML.Attribute(E2, "value")));
							break;

						case "time":
							Id = XML.Attribute(E2, "id");
							Name = XML.Attribute(E2, "name");

							ParameterList?.Add(new TimeSpanParameter(Id, Name, XML.Attribute(E2, "value", TimeSpan.Zero)));
							break;

						case "message":
							DateTime Timestamp = XML.Attribute(E2, "timestamp", DateTime.MinValue);
							string EventId = XML.Attribute(E2, "eventId");
							Things.DisplayableParameters.MessageType Type = XML.Attribute(E2, "type",
								Things.DisplayableParameters.MessageType.Information);

							MessageList?.Add(new Message(Timestamp, Type, EventId, E2.InnerText));
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
		public Task GetNodes(string To, IThingReference[] Nodes, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'>");

			foreach (IThingReference Node in Nodes)
				this.AppendNode(Xml, Node);

			Xml.Append("</getNodes>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
			{
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		private async Task NodesResponse(IqResultEventArgs e, bool Parameters, bool Messages,
			EventHandlerAsync<NodesInformationEventArgs> Callback, object _)
		{
			XmlElement E;
			NodeInformation[] NodeInfo;

			if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "nodeInfos")
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

			await Callback.Raise(this, new NodesInformationEventArgs(NodeInfo, e));
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
		public Task GetAllNodes(string To, string SourceID, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			return this.GetAllNodes(To, SourceID, null, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task GetAllNodes(string To, string SourceID, string[] OnlyIfDerivedFrom, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAllNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
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

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
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
		public Task GetNodeInheritance(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<StringsResponseEventArgs> Callback, object State)
		{
			return this.GetNodeInheritance(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task GetNodeInheritance(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<StringsResponseEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeInheritance xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);

			if (!string.IsNullOrEmpty(Language))
			{
				Xml.Append("' xml:lang='");
				Xml.Append(XML.Encode(Language));
			}

			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				List<string> BaseClasses = new List<string>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "inheritance" &&
					E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
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

				await Callback.Raise(this, new StringsResponseEventArgs(BaseClasses?.ToArray(), e));

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
		public Task GetRootNodes(string To, string SourceID, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getRootNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
			{
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

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
		/// <returns>Information about the root nodes.</returns>
		public async Task<NodeInformation[]> GetRootNodesAsync(string To, string SourceID, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken)
		{
			TaskCompletionSource<NodeInformation[]> Result = new TaskCompletionSource<NodeInformation[]>();

			await this.GetRootNodes(To, SourceID, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, (_, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.NodesInformation);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get root nodes."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
		}

		/// <summary>
		/// Gets information about all child nodes of a node in a data source.
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
		public Task GetChildNodes(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			return this.GetChildNodes(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language,
				ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Gets information about all child nodes of a node in a data source.
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
		public Task GetChildNodes(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getChildNodes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
			{
				return this.NodesResponse(e, Parameters, Messages, Callback, State);

			}, State);
		}

		/// <summary>
		/// Gets information about all child nodes of a node in a data source.
		/// </summary>
		/// <param name="To">Address of concentrator server.</param>
		/// <param name="Node">Node reference.</param>
		/// <param name="Parameters">If node parameters should be included in response.</param>
		/// <param name="Messages">If messages should be included in the response.</param>
		/// <param name="Language">Code of desired language.</param>
		/// <param name="ServiceToken">Optional Service token.</param>
		/// <param name="DeviceToken">Optional Device token.</param>
		/// <param name="UserToken">Optional User token.</param>
		/// <returns>Information about the root nodes.</returns>
		public Task<NodeInformation[]> GetChildNodesAsync(string To, IThingReference Node, bool Parameters, bool Messages,
			string Language, string ServiceToken, string DeviceToken, string UserToken)
		{
			return this.GetChildNodesAsync(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language,
				ServiceToken, DeviceToken, UserToken);
		}

		/// <summary>
		/// Gets information about all child nodes of a node in a data source.
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
		/// <returns>Information about the root nodes.</returns>
		public async Task<NodeInformation[]> GetChildNodesAsync(string To, string NodeID, string SourceID, string Partition,
			bool Parameters, bool Messages, string Language, string ServiceToken, string DeviceToken, string UserToken)
		{
			TaskCompletionSource<NodeInformation[]> Result = new TaskCompletionSource<NodeInformation[]>();

			await this.GetChildNodes(To, NodeID, SourceID, Partition, Parameters, Messages, Language, ServiceToken, DeviceToken, UserToken, (_, e) =>
			{
				if (e.Ok)
					Result.TrySetResult(e.NodesInformation);
				else
					Result.TrySetException(e.StanzaError ?? new Exception("Unable to get child nodes."));

				return Task.CompletedTask;

			}, null);

			return await Result.Task;
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
		public Task GetAncestors(string To, IThingReference Node, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			return this.GetAncestors(To, Node.NodeId, Node.SourceId, Node.Partition, Parameters, Messages, Language,
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
		public Task GetAncestors(string To, string NodeID, string SourceID, string Partition, bool Parameters, bool Messages, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodesInformationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAncestors xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), (Sender, e) =>
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
		public Task GetAddableNodeTypes(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<LocalizedStringsResponseEventArgs> Callback, object State)
		{
			return this.GetAddableNodeTypes(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task GetAddableNodeTypes(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<LocalizedStringsResponseEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getAddableNodeTypes xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				List<LocalizedString> Types = new List<LocalizedString>();
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "nodeTypes" &&
					E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
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

				await Callback.Raise(this, new LocalizedStringsResponseEventArgs(Types.ToArray(), e));

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
		public Task GetParametersForNewNode(string To, IThingReference Node, string NodeType, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback, object State)
		{
			return this.GetParametersForNewNode(To, Node.NodeId, Node.SourceId, Node.Partition, NodeType, Language,
				ServiceToken, DeviceToken, UserToken, FormCallback, NodeCallback, State);
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
		public Task GetParametersForNewNode(string To, string NodeID, string SourceID, string Partition, string NodeType, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getParametersForNewNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(NodeType));
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.CreateNewNode, this.CancelCreateNewNode, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				await FormCallback.Raise(this, new DataFormEventArgs(Form, e));

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
			EventHandlerAsync<DataFormEventArgs> FormCallback = (EventHandlerAsync<DataFormEventArgs>)P[9];
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback = (EventHandlerAsync<NodeInformationEventArgs>)P[10];
			object State = P[11];

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<createNewNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("' type='");
			Xml.Append(XML.Encode(NodeType));
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'>");
			Form.SerializeSubmit(Xml);
			Xml.Append("</createNewNode>");

			return this.client.SendIqSet(To, Xml.ToString(), async (_, e) =>
			{
				if (!e.Ok && !(e.ErrorElement is null) && e.ErrorType == ErrorType.Modify)
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

					e.Ok = true;
					await FormCallback.Raise(this, new DataFormEventArgs(Form, e));

					return;
				}

				await this.NodeResponse(e, true, true, NodeCallback, State);
			}, P);
		}

		private async Task CancelCreateNewNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback = (EventHandlerAsync<NodeInformationEventArgs>)P[10];
			object State = P[11];

			await NodeCallback.Raise(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
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
		public Task DestroyNode(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.DestroyNode(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task DestroyNode(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<destroyNode xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			return this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public Task GetNodeParametersForEdit(string To, IThingReference Node, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback, object State)
		{
			return this.GetNodeParametersForEdit(To, Node.NodeId, Node.SourceId, Node.Partition, Language, ServiceToken, DeviceToken, UserToken,
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
		public Task GetNodeParametersForEdit(string To, string NodeID, string SourceID, string Partition, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback, EventHandlerAsync<NodeInformationEventArgs> NodeCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeParametersForEdit xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.EditNode, this.CancelEditNode, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				await FormCallback.Raise(this, new DataFormEventArgs(Form, e));

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
			EventHandlerAsync<DataFormEventArgs> FormCallback = (EventHandlerAsync<DataFormEventArgs>)P[8];
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback = (EventHandlerAsync<NodeInformationEventArgs>)P[9];
			object State = P[10];

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<setNodeParametersAfterEdit xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("'>");
			Form.SerializeSubmit(Xml);
			Xml.Append("</setNodeParametersAfterEdit>");

			return this.client.SendIqSet(To, Xml.ToString(), (_, e) =>
			{
				if (!e.Ok && !(e.ErrorElement is null) && e.ErrorType == ErrorType.Modify)
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

					e.Ok = true;
					return FormCallback.Raise(this, new DataFormEventArgs(Form, e));
				}
				else
					return this.NodeResponse(e, true, true, NodeCallback, State);

			}, P);
		}

		private async Task CancelEditNode(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			EventHandlerAsync<NodeInformationEventArgs> NodeCallback = (EventHandlerAsync<NodeInformationEventArgs>)P[9];
			object State = P[10];

			await NodeCallback.Raise(this, new NodeInformationEventArgs(null, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
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
		public Task RegisterSniffer(string To, IThingReference Node, DateTime Expires, ISniffer Sniffer,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<SnifferRegistrationEventArgs> Callback, object State)
		{
			return this.RegisterSniffer(To, Node.NodeId, Node.SourceId, Node.Partition, Expires, Sniffer, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task RegisterSniffer(string To, string NodeID, string SourceID, string Partition, DateTime Expires, ISniffer Sniffer,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<SnifferRegistrationEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<registerSniffer xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
			Xml.Append("' expires='");
			Xml.Append(XML.Encode(Expires.ToUniversalTime()));
			Xml.Append("'/>");

			return this.client.SendIqSet(To, Xml.ToString(), async (Sender, e) =>
			{
				XmlElement E;
				string SnifferId = null;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "sniffer" &&
					E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
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

				await Callback.Raise(this, new SnifferRegistrationEventArgs(SnifferId, Expires, e));

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
		public Task<bool> UnregisterSniffer(string To, IThingReference Node, string SnifferId, string ServiceToken, string DeviceToken, string UserToken,
			EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.UnregisterSniffer(To, Node.NodeId, Node.SourceId, Node.Partition, SnifferId, ServiceToken, DeviceToken, UserToken, Callback, State);
		}

		/// <summary>
		/// Unregisters a sniffer, without sending an unregistration message.
		/// </summary>
		/// <param name="SnifferId">ID of sniffer to unregister.</param>
		/// <returns>If the sniffer was found locally and removed.</returns>
		public Task<bool> UnregisterSniffer(string SnifferId)
		{
			return this.UnregisterSniffer(string.Empty, null, SnifferId, string.Empty, string.Empty, string.Empty, null, null);
		}

		/// <summary>
		/// Unregisters a sniffer on a node.
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
		public async Task<bool> UnregisterSniffer(string To, string NodeID, string SourceID, string Partition, string SnifferId,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			bool Result;

			lock (this.sniffers)
			{
				Result = this.sniffers.Remove(SnifferId);
			}

			if (!string.IsNullOrEmpty(To))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<unregisterSniffer xmlns='");
				Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
				Xml.Append("'");
				this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
				this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
				this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
				Xml.Append("' snifferId='");
				Xml.Append(XML.Encode(SnifferId));
				Xml.Append("'/>");

				await this.client.SendIqSet(To, Xml.ToString(), Callback, State);
			}

			return Result;
		}

		private async Task SniffMessageHandler(object Sender, MessageEventArgs e)
		{
			string SnifferId = XML.Attribute(e.Content, "snifferId");
			DateTime Timestamp = XML.Attribute(e.Content, "timestamp", DateTime.Now);
			ISniffer Sniffer;

			lock (this.sniffers)
			{
				if (!this.sniffers.TryGetValue(SnifferId, out Sniffer))
					Sniffer = null;
			}

			if (Sniffer is null)
			{
				CustomSnifferEventArgs e2 = new CustomSnifferEventArgs(SnifferId, e);

				await this.OnCustomSnifferMessage.Raise(this, e2);

				Sniffer = e2.Sniffer;
				if (Sniffer is null)
					return;

				lock (this.sniffers)
				{
					this.sniffers[SnifferId] = Sniffer;
				}
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
								await Sniffer.ReceiveBinary(Timestamp, Bin);
								break;

							case "txBin":
								Bin = Convert.FromBase64String(E.InnerText);
								await Sniffer.TransmitBinary(Timestamp, Bin);
								break;

							case "rx":
								string s = E.InnerText;
								await Sniffer.ReceiveText(Timestamp, s);
								break;

							case "tx":
								s = E.InnerText;
								await Sniffer.TransmitText(Timestamp, s);
								break;

							case "info":
								s = E.InnerText;
								await Sniffer.Information(Timestamp, s);
								break;

							case "warning":
								s = E.InnerText;
								await Sniffer.Warning(Timestamp, s);
								break;

							case "error":
								s = E.InnerText;
								await Sniffer.Error(Timestamp, s);
								break;

							case "exception":
								s = E.InnerText;
								await Sniffer.Exception(Timestamp, s);
								break;

							case "expired":
								lock (this.sniffers)
								{
									this.sniffers.Remove(SnifferId);
								}

								await Sniffer.Information(Timestamp, "Remote sniffer expired.");
								break;

							default:
								await Sniffer.Error("Unrecognized sniffer event received: " + E.OuterXml);
								break;
						}
					}
					catch (Exception)
					{
						await Sniffer.Error(Timestamp, "Badly encoded sniffer data was received: " + E.OuterXml);
					}
				}
			}
		}

		/// <summary>
		/// Event raised when a sniffer message has been received from a source without a registered sniffer.
		/// </summary>
		public event EventHandlerAsync<CustomSnifferEventArgs> OnCustomSnifferMessage = null;

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
		public Task GetNodeCommands(string To, IThingReference Node,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<CommandsEventArgs> Callback, object State)
		{
			return this.GetNodeCommands(To, Node.NodeId, Node.SourceId, Node.Partition, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task GetNodeCommands(string To, string NodeID, string SourceID, string Partition,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<CommandsEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getNodeCommands xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, this.client.Language);
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				XmlElement E;
				List<NodeCommand> Commands = new List<NodeCommand>();

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "commands" &&
					E.NamespaceURI == ConcentratorServer.NamespaceConcentratorCurrent)
				{
					foreach (XmlNode N in E.ChildNodes)
					{
						if (N is XmlElement E2 && E2.LocalName == "command")
						{
							string Command = XML.Attribute(E2, "command");
							string Name = XML.Attribute(E2, "name");
							CommandType Type = XML.Attribute(E2, "type", CommandType.Simple);
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

				await Callback.Raise(this, new CommandsEventArgs(Commands.ToArray(), e));

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
		public Task GetCommandParameters(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback, object State)
		{
			return this.GetCommandParameters(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Language, ServiceToken, DeviceToken, UserToken,
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
		public Task GetCommandParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback, object State)
		{
			return this.GetCommandParameters(To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken, FormCallback, CommandCallback, null, State);
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
		public Task GetQueryParameters(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeQueryResponseEventArgs> QueryCallback, object State)
		{
			return this.GetCommandParameters(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Language, ServiceToken, DeviceToken, UserToken,
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
		public Task GetQueryParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeQueryResponseEventArgs> QueryCallback, object State)
		{
			return this.GetCommandParameters(To, NodeID, SourceID, Partition, Command, Language, ServiceToken, DeviceToken, UserToken, FormCallback, null, QueryCallback, State);
		}

		private Task GetCommandParameters(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<DataFormEventArgs> FormCallback,
			EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback,
			EventHandlerAsync<NodeQueryResponseEventArgs> QueryCallback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<getCommandParameters xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("' command='");
			Xml.Append(XML.Encode(Command));
			Xml.Append("'/>");

			return this.client.SendIqGet(To, Xml.ToString(), async (Sender, e) =>
			{
				DataForm Form = null;
				XmlElement E;

				if (e.Ok && !((E = e.FirstElement) is null) && E.LocalName == "x" && E.NamespaceURI == XmppClient.NamespaceData)
				{
					Form = new DataForm(this.client, E, this.EditCommandParameters, this.CancelEditCommandParameters, e.From, e.To)
					{
						State = e.State
					};
				}
				else
					e.Ok = false;

				await FormCallback.Raise(this, new DataFormEventArgs(Form, e));

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
			EventHandlerAsync<DataFormEventArgs> FormCallback = (EventHandlerAsync<DataFormEventArgs>)P[9];
			EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback = (EventHandlerAsync<NodeCommandResponseEventArgs>)P[10];
			EventHandlerAsync<NodeQueryResponseEventArgs> QueryCallback = (EventHandlerAsync<NodeQueryResponseEventArgs>)P[11];
			object State = P[12];

			if (!(CommandCallback is null))
				return this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Form, Language, ServiceToken, DeviceToken, UserToken, CommandCallback, State);
			else
				return this.ExecuteQuery(To, NodeID, SourceID, Partition, Command, Form, Language, ServiceToken, DeviceToken, UserToken, QueryCallback, State);
		}

		private async Task CancelEditCommandParameters(object Sender, DataForm Form)
		{
			object[] P = (object[])Form.State;
			EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback = (EventHandlerAsync<NodeCommandResponseEventArgs>)P[10];
			object State = P[11];

			await CommandCallback.Raise(this, new NodeCommandResponseEventArgs(Form, new IqResultEventArgs(null, string.Empty, string.Empty, string.Empty, false, State)));
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
		public Task ExecuteCommand(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeCommandResponseEventArgs> Callback, object State)
		{
			return this.ExecuteCommand(To, Node.NodeId, Node.SourceId, Node.Partition, Command, null, null, Language, ServiceToken, DeviceToken, UserToken,
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
		public Task ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeCommandResponseEventArgs> Callback, object State)
		{
			return this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, null, null, Language, ServiceToken, DeviceToken, UserToken, Callback, null, State);
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
		public Task ExecuteCommand(string To, IThingReference Node, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeCommandResponseEventArgs> Callback, object State)
		{
			return this.ExecuteCommand(To, Node.NodeId, Node.SourceId, Node.Partition, Command, Parameters, null, Language, ServiceToken, DeviceToken, UserToken,
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
		public Task ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeCommandResponseEventArgs> Callback, object State)
		{
			return this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Parameters, null, Language, ServiceToken, DeviceToken, UserToken, Callback, null, State);
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
		public Task<NodeQuery> ExecuteQuery(string To, IThingReference Node, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeQueryResponseEventArgs> Callback, object State)
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
		public Task<NodeQuery> ExecuteQuery(string To, string NodeID, string SourceID, string Partition, string Command, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeQueryResponseEventArgs> Callback, object State)
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
		public Task<NodeQuery> ExecuteQuery(string To, IThingReference Node, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeQueryResponseEventArgs> Callback, object State)
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
		public async Task<NodeQuery> ExecuteQuery(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeQueryResponseEventArgs> Callback, object State)
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

			await this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Parameters, Query, Language, ServiceToken, DeviceToken, UserToken, null, Callback, State);

			return Query;
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
		/// <param name="Query">Node query object where results will be made available</param>
		/// <param name="Callback">Method to call when operation has been executed.</param>
		/// <param name="State">State object to pass on to the node callback method.</param>
		public Task ExecuteQuery(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, string Language,
			string ServiceToken, string DeviceToken, string UserToken, NodeQuery Query, EventHandlerAsync<NodeQueryResponseEventArgs> Callback, object State)
		{
			lock (this.queries)
			{
				if (this.queries.ContainsKey(Query.QueryId))
					throw new ArgumentException("Query ID already registered.", nameof(Query));

				this.queries[Query.QueryId] = Query;
			}

			return this.ExecuteCommand(To, NodeID, SourceID, Partition, Command, Parameters, Query, Language, ServiceToken, DeviceToken, UserToken, null, Callback, State);
		}

		private Task ExecuteCommand(string To, string NodeID, string SourceID, string Partition, string Command, DataForm Parameters, NodeQuery Query,
			string Language, string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<NodeCommandResponseEventArgs> CommandCallback,
			EventHandlerAsync<NodeQueryResponseEventArgs> QueryCallback, object State)
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

			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
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

			return this.client.SendIqSet(To, Xml.ToString(), async (Sender, e) =>
			{
				if (!e.Ok && !(e.ErrorElement is null) && e.ErrorType == ErrorType.Modify)
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

				if (!(CommandCallback is null))
					await CommandCallback.Raise(this, new NodeCommandResponseEventArgs(Parameters, e));
				else
					await QueryCallback.Raise(this, new NodeQueryResponseEventArgs(Query, Parameters, e));

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
		public Task AbortQuery(string To, IThingReference Node, string Command, string QueryId,
			string Language, string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			return this.AbortQuery(To, Node.NodeId, Node.SourceId, Node.Partition, Command, QueryId, Language, ServiceToken, DeviceToken, UserToken, Callback, State);
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
		public Task AbortQuery(string To, string NodeID, string SourceID, string Partition, string Command, string QueryId,
			string Language, string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
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
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("'");
			this.AppendNodeAttributes(Xml, NodeID, SourceID, Partition);
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			Xml.Append("' command='");
			Xml.Append(XML.Encode(Command));
			Xml.Append("' queryId='");
			Xml.Append(XML.Encode(QueryId));
			Xml.Append("'/>");

			return this.client.SendIqSet(To, Xml.ToString(), Callback, State);
		}

		private Task QueryProgressHandler(object Sender, MessageEventArgs e)
		{
			string NodeId = XML.Attribute(e.Content, "id");
			string SourceId = XML.Attribute(e.Content, "src");
			string Partition = XML.Attribute(e.Content, "pt");
			string QueryId = XML.Attribute(e.Content, "queryId");
			NodeQuery Query;

			lock (this.queries)
			{
				if (!this.queries.TryGetValue(QueryId, out Query))
					return Task.CompletedTask;
			}

			if (Query.NodeID != NodeId || Query.SourceID != SourceId || Query.Partition != Partition)
				return Task.CompletedTask;

			return Query.Process(e);
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
		public Task Subscribe(string To, string SourceID, int TimeoutSeconds, SourceEventType EventTypes, bool Parameters, bool Messages,
			string Language, string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			if (TimeoutSeconds <= 0)
				throw new ArgumentException("Timeout must be positive.", nameof(TimeoutSeconds));

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<subscribe xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			Xml.Append("' ttl='");
			Xml.Append(TimeoutSeconds.ToString());
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, Parameters, Messages, Language);
			this.AppendEventTypeAttributes(Xml, EventTypes);
			Xml.Append("'/>");

			return this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
		public Task Unsubscribe(string To, string SourceID, SourceEventType EventTypes, string Language,
			string ServiceToken, string DeviceToken, string UserToken, EventHandlerAsync<IqResultEventArgs> Callback, object State)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<unsubscribe xmlns='");
			Xml.Append(ConcentratorServer.NamespaceConcentratorCurrent);
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceID));
			this.AppendTokenAttributes(Xml, ServiceToken, DeviceToken, UserToken);
			this.AppendNodeInfoAttributes(Xml, false, false, Language);
			this.AppendEventTypeAttributes(Xml, EventTypes);
			Xml.Append("'/>");

			return this.client.SendIqSet(To, Xml.ToString(), Callback, State);
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
				State = XML.Attribute(E, "state", NodeState.None),
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
				State = XML.Attribute(E, "state", NodeState.None),
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
				State = XML.Attribute(E, "state", NodeState.None),
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

		private Task SourceEventReceived(SourceEvent Event, MessageEventArgs e)
		{
			return this.OnEvent.Raise(e, new SourceEventMessageEventArgs(Event, e));
		}

		/// <summary>
		/// Event raised when a data source event has been received.
		/// </summary>
		public event EventHandlerAsync<SourceEventMessageEventArgs> OnEvent = null;
	}
}
