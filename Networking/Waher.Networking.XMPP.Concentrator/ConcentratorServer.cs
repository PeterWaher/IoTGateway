using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using SkiaSharp;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Language;
using Waher.Runtime.Inventory;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.DisplayableParameters;
using Waher.Things.Queries;
using Waher.Things.SourceEvents;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Settings;
using Waher.Security;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator server interface.
	/// 
	/// The interface is defined in the IEEE XMPP IoT extensions:
	/// https://gitlab.com/IEEE-SA/XMPPI/IoT
	/// </summary>
	public class ConcentratorServer : XmppExtension
	{
		/// <summary>
		/// urn:ieee:iot:concentrator:1.0
		/// </summary>
		public const string NamespaceConcentrator = "urn:ieee:iot:concentrator:1.0";

		private readonly Dictionary<string, IDataSource> rootDataSources = new Dictionary<string, IDataSource>();
		private readonly Dictionary<string, DataSourceRec> dataSources = new Dictionary<string, DataSourceRec>();
		private readonly Dictionary<string, Query> queries = new Dictionary<string, Query>();
		private readonly object synchObject = new object();
		private SensorServer sensorServer = null;
		private ControlServer controlServer = null;
		private readonly ProvisioningClient provisioningClient = null;
		private readonly ThingRegistryClient thingRegistryClient = null;
		private readonly RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		/// <summary>
		/// Implements an XMPP concentrator server interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="DataSources">Data sources.</param>
		public ConcentratorServer(XmppClient Client, params IDataSource[] DataSources)
			: this(Client, null, null, DataSources)
		{
		}

		/// <summary>
		/// Implements an XMPP concentrator server interface.
		/// 
		/// The interface is defined in the IEEE XMPP IoT extensions:
		/// https://gitlab.com/IEEE-SA/XMPPI/IoT
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="ThingRegistryClient">Thing Registry client.</param>
		/// <param name="ProvisioningClient">Provisioning client.</param>
		/// <param name="DataSources">Data sources.</param>
		public ConcentratorServer(XmppClient Client, ThingRegistryClient ThingRegistryClient, ProvisioningClient ProvisioningClient, params IDataSource[] DataSources)
			: base(Client)
		{
			this.thingRegistryClient = ThingRegistryClient;
			this.provisioningClient = ProvisioningClient;

			this.sensorServer = new SensorServer(this.client, ProvisioningClient, true);
			this.sensorServer.OnGetNode += OnGetNode;
			this.sensorServer.OnExecuteReadoutRequest += SensorServer_OnExecuteReadoutRequest;

			this.controlServer = new ControlServer(this.client, ProvisioningClient);
			this.controlServer.OnGetNode += OnGetNode;
			this.controlServer.OnGetControlParameters += ControlServer_OnGetControlParameters;

			if (!(this.thingRegistryClient is null))
			{
				this.thingRegistryClient.Claimed += ThingRegistryClient_Claimed;
				this.thingRegistryClient.Disowned += ThingRegistryClient_Disowned;
				this.thingRegistryClient.Removed += ThingRegistryClient_Removed;
			}

			foreach (IDataSource DataSource in DataSources)
				this.Register(DataSource);

			this.client.RegisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);                                      // ConcentratorClient.GetCapabilities

			this.client.RegisterIqGetHandler("getAllDataSources", NamespaceConcentrator, this.GetAllDataSourcesHandler, false);                                 // ConcentratorClient.GetAllDataSources
			this.client.RegisterIqGetHandler("getRootDataSources", NamespaceConcentrator, this.GetRootDataSourcesHandler, false);                               // ConcentratorClient.GetRootDataSources
			this.client.RegisterIqGetHandler("getChildDataSources", NamespaceConcentrator, this.GetChildDataSourcesHandler, false);                             // ConcentratorClient.GetChildDataSources

			this.client.RegisterIqGetHandler("containsNode", NamespaceConcentrator, this.ContainsNodeHandler, false);                                           // ConcentratorClient.ContainsNode
			this.client.RegisterIqGetHandler("containsNodes", NamespaceConcentrator, this.ContainsNodesHandler, false);                                         // ConcentratorClient.ContainsNodes
			this.client.RegisterIqGetHandler("getNode", NamespaceConcentrator, this.GetNodeHandler, false);                                                     // ConcentratorClient.GetNode
			this.client.RegisterIqGetHandler("getNodes", NamespaceConcentrator, this.GetNodesHandler, false);                                                   // ConcentratorClient.GetNodes
			this.client.RegisterIqGetHandler("getAllNodes", NamespaceConcentrator, this.GetAllNodesHandler, false);                                             // ConcentratorClient.GetAllNodes
			this.client.RegisterIqGetHandler("getNodeInheritance", NamespaceConcentrator, this.GetNodeInheritanceHandler, false);                               // ConcentratorClient.GetNodeInheritance
			this.client.RegisterIqGetHandler("getRootNodes", NamespaceConcentrator, this.GetRootNodesHandler, false);                                           // ConcentratorClient.GetRootNodes
			this.client.RegisterIqGetHandler("getChildNodes", NamespaceConcentrator, this.GetChildNodesHandler, false);                                         // ConcentratorClient.GetChildNodes

			this.client.RegisterIqGetHandler("getNodeParametersForEdit", NamespaceConcentrator, this.GetNodeParametersForEditHandler, false);                   // ConcentratorClient.GetNodeParametersForEdit
			this.client.RegisterIqSetHandler("setNodeParametersAfterEdit", NamespaceConcentrator, this.SetNodeParametersAfterEditHandler, false);               // (ConcentratorClient.EditNode)
			this.client.RegisterIqGetHandler("getCommonNodeParametersForEdit", NamespaceConcentrator, this.GetCommonNodeParametersForEditHandler, false);       // TODO:
			this.client.RegisterIqSetHandler("setCommonNodeParametersAfterEdit", NamespaceConcentrator, this.SetCommonNodeParametersAfterEditHandler, false);   // TODO:

			this.client.RegisterIqGetHandler("getAddableNodeTypes", NamespaceConcentrator, this.GetAddableNodeTypesHandler, false);                             // ConcentratorClient.GetAddableNodeTypes
			this.client.RegisterIqGetHandler("getParametersForNewNode", NamespaceConcentrator, this.GetParametersForNewNodeHandler, false);                     // ConcentratorClient.GetParametersForNewNode
			this.client.RegisterIqSetHandler("createNewNode", NamespaceConcentrator, this.CreateNewNodeHandler, false);                                         // (ConcentratorClient.CreateNewNode, called when submitting form)
			this.client.RegisterIqSetHandler("destroyNode", NamespaceConcentrator, this.DestroyNodeHandler, false);                                             // ConcentratorClient.DestroyNode

			this.client.RegisterIqGetHandler("getAncestors", NamespaceConcentrator, this.GetAncestorsHandler, false);                                           // ConcentratorClient.GetAncestors

			this.client.RegisterIqGetHandler("getNodeCommands", NamespaceConcentrator, this.GetNodeCommandsHandler, false);                                     // ConcentratorClient.GetNodeCommands
			this.client.RegisterIqGetHandler("getCommandParameters", NamespaceConcentrator, this.GetCommandParametersHandler, false);                           // ConcentratorClient.GetCommandParameters, ConcentratorClient.GetQueryParameters.
			this.client.RegisterIqSetHandler("executeNodeCommand", NamespaceConcentrator, this.ExecuteNodeCommandHandler, false);                               // ConcentratorClient.ExecuteCommand
			this.client.RegisterIqSetHandler("executeNodeQuery", NamespaceConcentrator, this.ExecuteNodeQueryHandler, false);                                   // ConcentratorClient.ExecuteQuery
			this.client.RegisterIqSetHandler("abortNodeQuery", NamespaceConcentrator, this.AbortNodeQueryHandler, false);                                       // ConcentratorClient.AbortQuery
			this.client.RegisterIqGetHandler("getCommonNodeCommands", NamespaceConcentrator, this.GetCommonNodeCommandsHandler, false);                         // TODO:
			this.client.RegisterIqGetHandler("getCommonCommandParameters", NamespaceConcentrator, this.GetCommonCommandParametersHandler, false);               // TODO:
			this.client.RegisterIqGetHandler("executeCommonNodeCommand", NamespaceConcentrator, this.ExecuteCommonNodeCommandHandler, false);                   // TODO:
			this.client.RegisterIqGetHandler("executeCommonNodeQuery", NamespaceConcentrator, this.ExecuteCommonNodeQueryHandler, false);                       // TODO:
			this.client.RegisterIqSetHandler("abortCommonNodeQuery", NamespaceConcentrator, this.AbortCommonNodeQueryHandler, false);                                       // ConcentratorClient.AbortQuery

			this.client.RegisterIqSetHandler("moveNodeUp", NamespaceConcentrator, this.MoveNodeUpHandler, false);                                               // TODO:
			this.client.RegisterIqSetHandler("moveNodeDown", NamespaceConcentrator, this.MoveNodeDownHandler, false);                                           // TODO:
			this.client.RegisterIqSetHandler("moveNodesUp", NamespaceConcentrator, this.MoveNodesUpHandler, false);                                             // TODO:
			this.client.RegisterIqSetHandler("moveNodesDown", NamespaceConcentrator, this.MoveNodesDownHandler, false);                                         // TODO:

			this.client.RegisterIqSetHandler("subscribe", NamespaceConcentrator, this.SubscribeHandler, false);                                                 // ConcentratorClient.Subscribe
			this.client.RegisterIqSetHandler("unsubscribe", NamespaceConcentrator, this.UnsubscribeHandler, false);                                             // ConcentratorClient.Unsubscribe

			this.client.RegisterIqSetHandler("registerSniffer", NamespaceConcentrator, this.RegisterSnifferHandler, false);
			this.client.RegisterIqSetHandler("unregisterSniffer", NamespaceConcentrator, this.UnregisterSnifferHandler, false);
		}

		private async Task ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			if (!e.Node.IsEmpty)
			{
				IThingReference Ref = await this.OnGetNode(e.Node.NodeId, e.Node.SourceId, e.Node.Partition);
				if (Ref != null && Ref is ILifeCycleManagement LifeCycleManagement)
					await LifeCycleManagement.Claimed(e.JID, e.IsPublic);

				string KeyId = this.KeyId(Ref);

				await RuntimeSettings.SetAsync(KeyId, string.Empty);
				await RuntimeSettings.SetAsync("IoTDisco." + KeyId, string.Empty);
			}
		}

		private async Task ThingRegistryClient_Disowned(object Sender, NodeEventArgs e)
		{
			if (!e.Node.IsEmpty)
			{
				IThingReference Ref = await this.OnGetNode(e.Node.NodeId, e.Node.SourceId, e.Node.Partition);

				if (Ref != null && Ref is ILifeCycleManagement LifeCycleManagement)
				{
					await LifeCycleManagement.Disowned();
					await this.RegisterNode(LifeCycleManagement);
				}
			}
		}

		private async Task ThingRegistryClient_Removed(object Sender, NodeEventArgs e)
		{
			if (!e.Node.IsEmpty)
			{
				IThingReference Ref = await this.OnGetNode(e.Node.NodeId, e.Node.SourceId, e.Node.Partition);
				if (Ref != null && Ref is ILifeCycleManagement LifeCycleManagement)
					await LifeCycleManagement.Removed();
			}
		}

		private async Task<IThingReference> OnGetNode(string NodeId, string SourceId, string Partition)
		{
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					return null;
			}

			return await Rec.Source.GetNodeAsync(new ThingReference(NodeId, SourceId, Partition));
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.client.UnregisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);

			this.client.UnregisterIqGetHandler("getAllDataSources", NamespaceConcentrator, this.GetAllDataSourcesHandler, false);
			this.client.UnregisterIqGetHandler("getRootDataSources", NamespaceConcentrator, this.GetRootDataSourcesHandler, false);
			this.client.UnregisterIqGetHandler("getChildDataSources", NamespaceConcentrator, this.GetChildDataSourcesHandler, false);

			this.client.UnregisterIqGetHandler("containsNode", NamespaceConcentrator, this.ContainsNodeHandler, false);
			this.client.UnregisterIqGetHandler("containsNodes", NamespaceConcentrator, this.ContainsNodesHandler, false);
			this.client.UnregisterIqGetHandler("getNode", NamespaceConcentrator, this.GetNodeHandler, false);
			this.client.UnregisterIqGetHandler("getNodes", NamespaceConcentrator, this.GetNodesHandler, false);
			this.client.UnregisterIqGetHandler("getAllNodes", NamespaceConcentrator, this.GetAllNodesHandler, false);
			this.client.UnregisterIqGetHandler("getNodeInheritance", NamespaceConcentrator, this.GetNodeInheritanceHandler, false);
			this.client.UnregisterIqGetHandler("getRootNodes", NamespaceConcentrator, this.GetRootNodesHandler, false);
			this.client.UnregisterIqGetHandler("getChildNodes", NamespaceConcentrator, this.GetChildNodesHandler, false);

			this.client.UnregisterIqGetHandler("getNodeParametersForEdit", NamespaceConcentrator, this.GetNodeParametersForEditHandler, false);
			this.client.UnregisterIqGetHandler("setNodeParametersAfterEdit", NamespaceConcentrator, this.SetNodeParametersAfterEditHandler, false);
			this.client.UnregisterIqGetHandler("getCommonNodeParametersForEdit", NamespaceConcentrator, this.GetCommonNodeParametersForEditHandler, false);
			this.client.UnregisterIqGetHandler("setCommonNodeParametersAfterEdit", NamespaceConcentrator, this.SetCommonNodeParametersAfterEditHandler, false);

			this.client.UnregisterIqGetHandler("getAddableNodeTypes", NamespaceConcentrator, this.GetAddableNodeTypesHandler, false);
			this.client.UnregisterIqGetHandler("getParametersForNewNode", NamespaceConcentrator, this.GetParametersForNewNodeHandler, false);
			this.client.UnregisterIqGetHandler("createNewNode", NamespaceConcentrator, this.CreateNewNodeHandler, false);
			this.client.UnregisterIqGetHandler("destroyNode", NamespaceConcentrator, this.DestroyNodeHandler, false);


			this.client.UnregisterIqGetHandler("getAncestors", NamespaceConcentrator, this.GetAncestorsHandler, false);

			this.client.UnregisterIqGetHandler("getNodeCommands", NamespaceConcentrator, this.GetNodeCommandsHandler, false);
			this.client.UnregisterIqGetHandler("getCommandParameters", NamespaceConcentrator, this.GetCommandParametersHandler, false);
			this.client.UnregisterIqGetHandler("executeNodeCommand", NamespaceConcentrator, this.ExecuteNodeCommandHandler, false);
			this.client.UnregisterIqGetHandler("executeNodeQuery", NamespaceConcentrator, this.ExecuteNodeQueryHandler, false);
			this.client.UnregisterIqGetHandler("abortNodeQuery", NamespaceConcentrator, this.AbortNodeQueryHandler, false);
			this.client.UnregisterIqGetHandler("getCommonNodeCommands", NamespaceConcentrator, this.GetCommonNodeCommandsHandler, false);
			this.client.UnregisterIqGetHandler("getCommonCommandParameters", NamespaceConcentrator, this.GetCommonCommandParametersHandler, false);
			this.client.UnregisterIqGetHandler("executeCommonNodeCommand", NamespaceConcentrator, this.ExecuteCommonNodeCommandHandler, false);
			this.client.UnregisterIqGetHandler("executeCommonNodeQuery", NamespaceConcentrator, this.ExecuteCommonNodeQueryHandler, false);

			this.client.UnregisterIqGetHandler("moveNodeUp", NamespaceConcentrator, this.MoveNodeUpHandler, false);
			this.client.UnregisterIqGetHandler("moveNodeDown", NamespaceConcentrator, this.MoveNodeDownHandler, false);
			this.client.UnregisterIqGetHandler("moveNodesUp", NamespaceConcentrator, this.MoveNodesUpHandler, false);
			this.client.UnregisterIqGetHandler("moveNodesDown", NamespaceConcentrator, this.MoveNodesDownHandler, false);

			this.client.UnregisterIqSetHandler("subscribe", NamespaceConcentrator, this.SubscribeHandler, false);
			this.client.UnregisterIqSetHandler("unsubscribe", NamespaceConcentrator, this.UnsubscribeHandler, false);

			this.client.UnregisterIqSetHandler("registerSniffer", NamespaceConcentrator, this.RegisterSnifferHandler, false);
			this.client.UnregisterIqSetHandler("unregisterSniffer", NamespaceConcentrator, this.UnregisterSnifferHandler, false);

			Query[] Queries;
			DataSourceRec[] Sources;

			lock (this.synchObject)
			{
				Queries = new Query[this.queries.Count];
				this.queries.Values.CopyTo(Queries, 0);
				this.queries.Clear();

				Sources = new DataSourceRec[this.dataSources.Count];
				this.dataSources.Values.CopyTo(Sources, 0);
				this.dataSources.Clear();
				this.rootDataSources.Clear();
			}

			foreach (Query Query in Queries)
			{
				Task _ = Query.Abort();
			}

			foreach (DataSourceRec Rec in Sources)
				Rec.Source.OnEvent -= this.DataSource_OnEvent;

			if (!(this.sensorServer is null))
			{
				this.sensorServer.Dispose();
				this.sensorServer = null;
			}

			if (!(this.controlServer is null))
			{
				this.controlServer.Dispose();
				this.controlServer = null;
			}
		}

		/// <summary>
		/// Implemented extensions.
		/// </summary>
		public override string[] Extensions => new string[] { "XEP-0326" };

		/// <summary>
		/// Sensor server.
		/// </summary>
		public SensorServer SensorServer => this.sensorServer;

		/// <summary>
		/// Control server.
		/// </summary>
		public ControlServer ControlServer => this.controlServer;

		/// <summary>
		/// Thing Registry client being used.
		/// </summary>
		public ThingRegistryClient ThingRegistryClient => this.thingRegistryClient;

		/// <summary>
		/// Provisioning client being used.
		/// </summary>
		public ProvisioningClient ProvisioningClient => this.provisioningClient;

		#region Capabilities

		private Task GetCapabilitiesHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();
			using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(false, true)))
			{
				w.WriteStartElement("strings", NamespaceConcentrator);

				w.WriteElementString("value", "getCapabilities");

				w.WriteElementString("value", "getAllDataSources");
				w.WriteElementString("value", "getRootDataSources");
				w.WriteElementString("value", "getChildDataSources");

				w.WriteElementString("value", "containsNode");
				w.WriteElementString("value", "containsNodes");
				w.WriteElementString("value", "getNode");
				w.WriteElementString("value", "getNodes");
				w.WriteElementString("value", "getAllNodes");
				w.WriteElementString("value", "getNodeInheritance");
				w.WriteElementString("value", "getRootNodes");
				w.WriteElementString("value", "getChildNodes");
				w.WriteElementString("value", "getAncestors");

				w.WriteElementString("value", "moveNodeUp");
				w.WriteElementString("value", "moveNodeDown");
				w.WriteElementString("value", "moveNodesUp");
				w.WriteElementString("value", "moveNodesDown");
				w.WriteElementString("value", "getNodeParametersForEdit");
				w.WriteElementString("value", "setNodeParametersAfterEdit");
				w.WriteElementString("value", "getCommonNodeParametersForEdit");
				w.WriteElementString("value", "setCommonNodeParametersAfterEdit");

				w.WriteElementString("value", "getAddableNodeTypes");
				w.WriteElementString("value", "getParametersForNewNode");
				w.WriteElementString("value", "createNewNode");
				w.WriteElementString("value", "destroyNode");

				w.WriteElementString("value", "getNodeCommands");
				w.WriteElementString("value", "getCommandParameters");
				w.WriteElementString("value", "executeNodeCommand");
				w.WriteElementString("value", "executeNodeQuery");
				w.WriteElementString("value", "abortNodeQuery");
				w.WriteElementString("value", "getCommonNodeCommands");
				w.WriteElementString("value", "getCommonCommandParameters");
				w.WriteElementString("value", "executeCommonNodeCommand");
				w.WriteElementString("value", "executeCommonNodeQuery");

				w.WriteElementString("value", "subscribe");
				w.WriteElementString("value", "unsubscribe");

				w.WriteEndElement();
				w.Flush();
			}

			e.IqResult(Xml.ToString());

			return Task.CompletedTask;
		}

		#endregion

		#region Tokens

		#endregion

		#region Data Sources

		/// <summary>
		/// Tries to get a data source.
		/// </summary>
		/// <param name="SourceId">Data Source ID</param>
		/// <param name="DataSource">Data Source, if found.</param>
		/// <returns>If a data source was found with the same ID.</returns>
		public bool TryGetDataSource(string SourceId, out IDataSource DataSource)
		{
			lock (this.synchObject)
			{
				return this.rootDataSources.TryGetValue(SourceId, out DataSource);
			}
		}

		/// <summary>
		/// Registers a new data source with the concentrator.
		/// </summary>
		/// <param name="DataSource">Data Source.</param>
		/// <returns>If the data source was registered (true), or if another data source with the same ID has already been registered (false).</returns>
		public bool Register(IDataSource DataSource)
		{
			return this.Register(DataSource, true);
		}

		private bool Register(IDataSource DataSource, bool Root)
		{
			lock (this.synchObject)
			{
				if (this.dataSources.ContainsKey(DataSource.SourceID))
					return false;

				this.dataSources[DataSource.SourceID] = new DataSourceRec(DataSource);

				if (Root)
					this.rootDataSources[DataSource.SourceID] = DataSource;
			}

			DataSource.OnEvent += this.DataSource_OnEvent;

			IEnumerable<IDataSource> ChildSources = DataSource.ChildSources;
			if (!(ChildSources is null))
			{
				foreach (IDataSource Child in ChildSources)
					this.Register(Child, false);
			}

			return true;
		}

		private class DataSourceRec
		{
			public IDataSource Source;
			public Dictionary<string, SubscriptionRec> Subscriptions = new Dictionary<string, SubscriptionRec>();
			public SubscriptionRec[] SubscriptionsStatic = new SubscriptionRec[0];

			public DataSourceRec(IDataSource Source)
			{
				this.Source = Source;
			}
		}

		private class SubscriptionRec
		{
			public string Jid;
			public SourceEventType EventTypes;
			public Language Language;
			public DateTime Expires;
			public bool Messages;
			public bool Parameters;
		}

		/// <summary>
		/// Root data sources.
		/// </summary>
		public IDataSource[] RootDataSources
		{
			get
			{
				IDataSource[] Result;

				lock (this.synchObject)
				{
					Result = new IDataSource[this.rootDataSources.Count];
					this.rootDataSources.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		/// <summary>
		/// All data sources.
		/// </summary>
		public IDataSource[] DataSources
		{
			get
			{
				List<IDataSource> Result = new List<IDataSource>();

				lock (this.synchObject)
				{
					foreach (DataSourceRec Rec in this.dataSources.Values)
						Result.Add(Rec.Source);

				}

				return Result.ToArray();
			}
		}

		internal static Task<Language> GetLanguage(XmlElement E)
		{
			return GetLanguage(E, Translator.DefaultLanguageCode);
		}

		internal static async Task<Language> GetLanguage(XmlElement E, string DefaultLanguageCode)
		{
			string LanguageCode = XML.Attribute(E, "xml:lang");

			if (string.IsNullOrEmpty(LanguageCode))
				LanguageCode = DefaultLanguageCode;

			Language Language = await Translator.GetLanguageAsync(LanguageCode);
			if (Language is null)
				Language = await Translator.GetDefaultLanguageAsync();

			return Language;
		}

		private static RequestOrigin GetTokens(string From, XmlElement E)
		{
			string[] DeviceTokens;
			string[] ServiceTokens;
			string[] UserTokens;

			if (E.HasAttribute("dt"))
				DeviceTokens = XML.Attribute(E, "dt").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				DeviceTokens = null;

			if (E.HasAttribute("st"))
				ServiceTokens = XML.Attribute(E, "st").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				ServiceTokens = null;

			if (E.HasAttribute("ut"))
				UserTokens = XML.Attribute(E, "ut").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				UserTokens = null;

			return new RequestOrigin(From, DeviceTokens, ServiceTokens, UserTokens);
		}

		private static ThingReference GetThingReference(XmlElement E)
		{
			string NodeId = XML.Attribute(E, "id");
			string SourceId = XML.Attribute(E, "src");
			string Partition = XML.Attribute(E, "pt");

			return new ThingReference(NodeId, SourceId, Partition);
		}


		private static readonly char[] Space = new char[] { ' ' };

		private async Task GetAllDataSourcesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<dataSources xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (IDataSource Source in this.DataSources)
			{
				if (await Source.CanViewAsync(Caller))
					await this.Export(Xml, Source, Language);
			}

			Xml.Append("</dataSources>");

			e.IqResult(Xml.ToString());
		}

		private async Task Export(StringBuilder Xml, IDataSource DataSource, Language Language)
		{
			Xml.Append("<dataSource src='");
			Xml.Append(XML.Encode(DataSource.SourceID));
			Xml.Append("' name='");
			Xml.Append(XML.Encode(await DataSource.GetNameAsync(Language)));
			Xml.Append("' hasChildren='");
			Xml.Append(CommonTypes.Encode(DataSource.HasChildren));
			Xml.Append("' lastChanged='");
			Xml.Append(XML.Encode(DataSource.LastChanged));
			Xml.Append("'/>");
		}

		private async Task GetRootDataSourcesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<dataSources xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (IDataSource Source in this.RootDataSources)
			{
				if (await Source.CanViewAsync(Caller))
					await this.Export(Xml, Source, Language);
			}

			Xml.Append("</dataSources>");

			e.IqResult(Xml.ToString());
		}

		private static Task<string> GetErrorMessage(Language Language, int StringId, string Message)
		{
			return Language.GetStringAsync(typeof(ConcentratorServer), StringId, Message);
		}

		private async Task GetChildDataSourcesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			string SourceId = XML.Attribute(e.Query, "src");
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null || !await Rec.Source.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<dataSources xmlns='");
				Xml.Append(NamespaceConcentrator);

				IEnumerable<IDataSource> ChildSources = Rec.Source.ChildSources;
				if (!(ChildSources is null))
				{
					Xml.Append("'>");

					foreach (IDataSource S in ChildSources)
					{
						if (await Rec.Source.CanViewAsync(Caller))
							await this.Export(Xml, S, Language);
					}

					Xml.Append("</dataSources>");
				}
				else
					Xml.Append("'/>");

				e.IqResult(Xml.ToString());
			}
		}

		#endregion

		#region Nodes

		private async Task ContainsNodeHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			bool Result = (Node != null && await Node.CanViewAsync(Caller));

			e.IqResult("<bool xmlns='" + NamespaceConcentrator + "'>" + CommonTypes.Encode(Result) + "</bool>");
		}

		private async Task ContainsNodesHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			StringBuilder Xml = new StringBuilder();
			ThingReference ThingRef;
			DataSourceRec Rec;
			INode Node;
			XmlElement E;
			bool Result;

			Xml.Append("<bools xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || E.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				Result = (Node != null && await Node.CanViewAsync(Caller));

				Xml.Append("<bool>");
				Xml.Append(CommonTypes.Encode(Result));
				Xml.Append("</bool>");
			}

			Xml.Append("</bools>");

			e.IqResult(Xml.ToString());
		}

		private async Task ExportAttributes(StringBuilder Xml, INode Node, Language Language)
		{
			string s;

			Xml.Append(" id='");
			Xml.Append(XML.Encode(Node.NodeId));

			if (!string.IsNullOrEmpty(s = Node.SourceId))
			{
				Xml.Append("' src='");
				Xml.Append(XML.Encode(s));
			}

			if (!string.IsNullOrEmpty(s = Node.Partition))
			{
				Xml.Append("' pt='");
				Xml.Append(XML.Encode(s));
			}

			Xml.Append("' nodeType='");
			Xml.Append(XML.Encode(Node.GetType().FullName));
			Xml.Append("' displayName='");
			Xml.Append(XML.Encode(await Node.GetTypeNameAsync(Language)));
			Xml.Append("' state='");
			Xml.Append(Node.State.ToString());

			if (!string.IsNullOrEmpty(s = Node.LocalId))
			{
				Xml.Append("' localId='");
				Xml.Append(XML.Encode(s));
			}

			if (!string.IsNullOrEmpty(s = Node.LogId))
			{
				Xml.Append("' logId='");
				Xml.Append(XML.Encode(s));
			}

			Xml.Append("' hasChildren='");
			Xml.Append(CommonTypes.Encode(Node.HasChildren));

			if (Node.ChildrenOrdered)
				Xml.Append("' childrenOrdered='true");

			if (Node.IsReadable)
				Xml.Append("' isReadable='true");

			if (Node.IsControllable)
				Xml.Append("' isControllable='true");

			if (Node.HasCommands)
				Xml.Append("' hasCommands='true");

			if (Node is ISniffable)
				Xml.Append("' sniffable='true");

			IThingReference Parent = Node.Parent;
			if (!(Parent is null))
			{
				Xml.Append("' parentId='");
				Xml.Append(XML.Encode(Parent.NodeId));

				if (!string.IsNullOrEmpty(s = Parent.Partition))
				{
					Xml.Append("' parentPartition='");
					Xml.Append(XML.Encode(s));
				}
			}

			Xml.Append("' lastChanged='");
			Xml.Append(XML.Encode(Node.LastChanged));
			Xml.Append("'");
		}

		private async Task GetNodeHandler(object Sender, IqEventArgs e)
		{
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node != null && await Node.CanViewAsync(Caller))
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<nodeInfo xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'");

				await ExportAttributes(Xml, Node, Language);

				if (Parameters || Messages)
				{
					Xml.Append(">");
					await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
					Xml.Append("</nodeInfo>");
				}
				else
					Xml.Append("/>");

				e.IqResult(Xml.ToString());
			}
			else
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
		}

		private async Task ExportParametersAndMessages(StringBuilder Xml, INode Node, bool Parameters, bool Messages,
			Language Language, RequestOrigin Caller)
		{
			if (Parameters)
			{
				IEnumerable<Parameter> Parameters2 = await Node.GetDisplayableParametersAsync(Language, Caller);

				if (!(Parameters2 is null))
				{
					foreach (Parameter P in Parameters2)
						P.Export(Xml);
				}
			}

			if (Messages)
			{
				IEnumerable<Message> Messages2 = await Node.GetMessagesAsync(Caller);

				if (!(Messages2 is null))
				{
					foreach (Message Msg in Messages2)
						Msg.Export(Xml);
				}
			}
		}

		private async Task GetNodesHandler(object Sender, IqEventArgs e)
		{
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			Language Language = await GetLanguage(e.Query);
			StringBuilder Xml = new StringBuilder();
			DataSourceRec Rec;
			ThingReference ThingRef;
			INode Node;
			XmlElement E;

			Xml.Append("<nodeInfos xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || E.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				if (Node is null || !(await Node.CanViewAsync(Caller)))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}

				Xml.Append("<nodeInfo");
				await ExportAttributes(Xml, Node, Language);

				if (Parameters || Messages)
				{
					Xml.Append(">");
					await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
					Xml.Append("</nodeInfo>");
				}
				else
					Xml.Append("/>");
			}

			Xml.Append("</nodeInfos>");

			e.IqResult(Xml.ToString());
		}

		private async Task GetAllNodesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			string SourceId = XML.Attribute(e.Query, "src");
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null || !await Rec.Source.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();
				LinkedList<INode> Nodes = new LinkedList<INode>();
				INode Node;
				LinkedList<TypeInfo> OnlyIfDerivedFrom = null;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					if (N.LocalName == "onlyIfDerivedFrom")
					{
						Type T = Types.GetType(N.InnerText.Trim());
						if (T is null)
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 9, "Type not found."), e.IQ));
							return;
						}

						if (OnlyIfDerivedFrom is null)
							OnlyIfDerivedFrom = new LinkedList<TypeInfo>();

						OnlyIfDerivedFrom.AddLast(T.GetTypeInfo());
					}
				}

				foreach (INode N in Rec.Source.RootNodes)
					Nodes.AddLast(N);

				Xml.Append("<nodeInfos xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				while (Nodes.First != null)
				{
					Node = Nodes.First.Value;
					Nodes.RemoveFirst();

					if (Node.HasChildren)
					{
						foreach (INode N in await Node.ChildNodes)
							Nodes.AddLast(N);
					}

					if ((OnlyIfDerivedFrom is null || this.IsAssignableFrom(OnlyIfDerivedFrom, Node)) && await Node.CanViewAsync(Caller))
					{
						Xml.Append("<nodeInfo");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
							Xml.Append("</nodeInfo>");
						}
						else
							Xml.Append("/>");
					}
				}

				Xml.Append("</nodeInfos>");

				e.IqResult(Xml.ToString());
			}
		}

		private bool IsAssignableFrom(IEnumerable<TypeInfo> TypeList, INode Node)
		{
			if (TypeList is null)
				return true;

			TypeInfo NodeType = Node.GetType().GetTypeInfo();

			foreach (TypeInfo T in TypeList)
			{
				if (T.IsAssignableFrom(NodeType))
					return true;
			}

			return false;
		}

		private async Task GetNodeInheritanceHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node != null && await Node.CanViewAsync(Caller))
			{
				StringBuilder Xml = new StringBuilder();
				Type T = Node.GetType();
				SortedDictionary<string, bool> Interfaces = null;

				Xml.Append("<inheritance xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'><baseClasses>");

				do
				{
					TypeInfo TI = T.GetTypeInfo();

					foreach (Type Interface in TI.ImplementedInterfaces)
					{
						if (Interfaces is null)
							Interfaces = new SortedDictionary<string, bool>();

						Interfaces[Interface.FullName] = true;
					}

					T = TI.BaseType;

					Xml.Append("<value>");
					Xml.Append(XML.Encode(T.FullName));
					Xml.Append("</value>");
				}
				while (T != typeof(object));

				Xml.Append("</baseClasses>");

				if (!(Interfaces is null))
				{
					Xml.Append("<interfaces>");

					foreach (string Name in Interfaces.Keys)
					{
						Xml.Append("<value>");
						Xml.Append(XML.Encode(Name));
						Xml.Append("</value>");
					}

					Xml.Append("</interfaces>");
				}

				Xml.Append("</inheritance>");

				e.IqResult(Xml.ToString());
			}
			else
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
		}

		private async Task GetRootNodesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			string SourceId = XML.Attribute(e.Query, "src");
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null || !await Rec.Source.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<nodeInfos xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (INode Node in Rec.Source.RootNodes)
				{
					if (!await Node.CanViewAsync(Caller))
						continue;

					Xml.Append("<nodeInfo");
					await ExportAttributes(Xml, Node, Language);

					if (Parameters || Messages)
					{
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
						Xml.Append("</nodeInfo>");
					}
					else
						Xml.Append("/>");
				}

				Xml.Append("</nodeInfos>");

				e.IqResult(Xml.ToString());
			}
		}

		private async Task GetChildNodesHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<nodeInfos xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				if (Node.HasChildren)
				{
					foreach (INode ChildNode in await Node.ChildNodes)
					{
						if (!await ChildNode.CanViewAsync(Caller))
							continue;

						Xml.Append("<nodeInfo");
						await ExportAttributes(Xml, ChildNode, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, ChildNode, Parameters, Messages, Language, Caller);
							Xml.Append("</nodeInfo>");
						}
						else
							Xml.Append("/>");
					}
				}

				Xml.Append("</nodeInfos>");

				e.IqResult(Xml.ToString());
			}
		}

		private async Task GetAncestorsHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			bool Parameters = XML.Attribute(e.Query, "parameters", false);
			bool Messages = XML.Attribute(e.Query, "messages", false);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			IThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<nodeInfos xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				while (Node != null)
				{
					if (!await Node.CanViewAsync(Caller))
						break;

					Xml.Append("<nodeInfo");
					await ExportAttributes(Xml, Node, Language);

					if (Parameters || Messages)
					{
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
						Xml.Append("</nodeInfo>");
					}
					else
						Xml.Append("/>");

					ThingRef = Node.Parent;
					Node = ThingRef as INode;
					if (Node is null)
						Node = await Rec.Source.GetNodeAsync(Node.Parent);
				}

				Xml.Append("</nodeInfos>");

				e.IqResult(Xml.ToString());
			}
		}

		#endregion

		#region Editing

		private async Task MoveNodeUpHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			IThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else
			{
				await Node.MoveUpAsync(Caller);

				e.IqResult(string.Empty);
			}
		}

		private async Task MoveNodeDownHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			IThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else
			{
				await Node.MoveDownAsync(Caller);

				e.IqResult(string.Empty);
			}
		}

		private async Task MoveNodesUpHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			LinkedList<INode> RootNodes = null;
			Dictionary<string, LinkedList<INode>> NodesPerParent = null;
			IThingReference ThingRef;
			DataSourceRec Rec;
			INode Node;
			XmlElement E;
			string Key;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || E.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				if (Node is null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}
				else if (!await Node.CanEditAsync(Caller))
				{
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
					return;
				}

				ThingRef = Node.Parent;
				if (ThingRef is null)
				{
					if (RootNodes is null)
						RootNodes = new LinkedList<INode>();

					RootNodes.AddLast(Node);
				}
				else
				{
					if (NodesPerParent is null)
						NodesPerParent = new Dictionary<string, LinkedList<INode>>();

					Key = ThingRef.SourceId + " \xa0 " + ThingRef.Partition + " \xa0 " + ThingRef.NodeId;
					if (!NodesPerParent.TryGetValue(Key, out LinkedList<INode> Nodes))
					{
						Nodes = new LinkedList<INode>();
						NodesPerParent[Key] = Nodes;
					}

					Nodes.AddLast(Node);
				}
			}

			if (!(RootNodes is null))
			{
				foreach (INode Node2 in RootNodes)
				{
					if (!await Node2.MoveUpAsync(Caller))
						break;
				}
			}

			if (!(NodesPerParent is null))
			{
				foreach (LinkedList<INode> Nodes2 in NodesPerParent.Values)
				{
					foreach (INode Node2 in Nodes2)
					{
						if (!await Node2.MoveUpAsync(Caller))
							break;
					}
				}
			}

			e.IqResult(string.Empty);
		}

		private async Task MoveNodesDownHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			LinkedList<INode> RootNodes = null;
			Dictionary<string, LinkedList<INode>> NodesPerParent = null;
			IThingReference ThingRef;
			DataSourceRec Rec;
			INode Node;
			XmlElement E;
			LinkedListNode<INode> Loop;
			string Key;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || E.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				if (Node is null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}
				else if (!await Node.CanEditAsync(Caller))
				{
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
					return;
				}

				ThingRef = Node.Parent;
				if (ThingRef is null)
				{
					if (RootNodes is null)
						RootNodes = new LinkedList<INode>();

					RootNodes.AddLast(Node);
				}
				else
				{
					if (NodesPerParent is null)
						NodesPerParent = new Dictionary<string, LinkedList<INode>>();

					Key = ThingRef.SourceId + " \xa0 " + ThingRef.Partition + " \xa0 " + ThingRef.NodeId;
					if (!NodesPerParent.TryGetValue(Key, out LinkedList<INode> Nodes))
					{
						Nodes = new LinkedList<INode>();
						NodesPerParent[Key] = Nodes;
					}

					Nodes.AddLast(Node);
				}
			}

			if (!(RootNodes is null))
			{
				Loop = RootNodes.Last;
				while (Loop != null)
				{
					if (!await Loop.Value.MoveDownAsync(Caller))
						break;

					Loop = Loop.Previous;
				}
			}

			if (!(NodesPerParent is null))
			{
				foreach (LinkedList<INode> Nodes2 in NodesPerParent.Values)
				{
					Loop = Nodes2.Last;
					while (Loop != null)
					{
						if (!await Loop.Value.MoveDownAsync(Caller))
							break;

						Loop = Loop.Previous;
					}
				}
			}

			e.IqResult(string.Empty);
		}

		private async Task GetNodeParametersForEditHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else
			{
				DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
				StringBuilder Xml = new StringBuilder();

				Form.SerializeForm(Xml);

				e.IqResult(Xml.ToString());
			}
		}

		private async Task SetNodeParametersAfterEditHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else
			{
				DataForm Form = null;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					if (N.LocalName == "x")
					{
						Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
						break;
					}
				}

				if (Form is null)
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 10, "Data form missing."), e.IQ));
				else
				{
					string OldNodeId = Node.NodeId;
					string OldSourceId = Node.SourceId;
					string OldPartition = Node.Partition;
					string NewNodeId = Form["NodeId"]?.ValueString;
					string NewSourceId = Form["SourceId"]?.ValueString;
					string NewPartition = Form["Partition"]?.ValueString;
					Parameters.SetEditableFormResult Result = null;

					if (!string.IsNullOrEmpty(NewNodeId))
					{
						if (string.IsNullOrEmpty(NewSourceId))
							NewSourceId = OldSourceId;

						if (string.IsNullOrEmpty(NewPartition))
							NewPartition = OldPartition;

						if ((NewNodeId != OldNodeId ||
							NewSourceId != OldSourceId ||
							NewPartition != OldPartition) &&
							await Rec.Source.GetNodeAsync(new ThingReference(NewNodeId, NewSourceId, NewPartition)) != null)
						{
							Result = new Parameters.SetEditableFormResult()
							{
								Errors = new KeyValuePair<string, string>[]
								{
										new KeyValuePair<string, string>("NodeId", "Identity already exists.")
								}
							};
						}
					}

					ILifeCycleManagement LifeCycleManagement = Node as ILifeCycleManagement;
					bool PreProvisioned = LifeCycleManagement != null && LifeCycleManagement.IsProvisioned;

					if (Result is null)
						Result = await Parameters.SetEditableForm(e, Node, Form, true);

					if (Result.Errors is null)
					{
						StringBuilder Xml = new StringBuilder();

						await Node.UpdateAsync();

						Xml.Append("<nodeInfo xmlns='");
						Xml.Append(NamespaceConcentrator);
						Xml.Append("'");
						await ExportAttributes(Xml, Node, Language);
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, true, true, Language, Caller);
						Xml.Append("</nodeInfo>");

						e.IqResult(Xml.ToString());

						Result.Tags.Add(new KeyValuePair<string, object>("Full", e.From));
						Result.Tags.Add(new KeyValuePair<string, object>("Source", Node.SourceId));
						Result.Tags.Add(new KeyValuePair<string, object>("Partition", Node.Partition));

						Log.Informational("Node edited.", Node.NodeId, e.FromBareJid, "NodeEdited", EventLevel.Medium, Result.Tags.ToArray());

						if (this.thingRegistryClient != null && LifeCycleManagement != null)
						{
							if (LifeCycleManagement.IsProvisioned)
							{
								if (string.IsNullOrEmpty(LifeCycleManagement.Owner))
									await this.RegisterNode(LifeCycleManagement);
								else
									await this.UpdateNodeRegistration(LifeCycleManagement);
							}
							else if (PreProvisioned)
								this.UnregisterNode(LifeCycleManagement);
						}
					}
					else
					{
						Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
						e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
					}
				}
			}
		}

		private string GetFormErrorsXml(KeyValuePair<string, string>[] Errors, DataForm Form)
		{
			StringBuilder Xml = new StringBuilder();
			Field Field;

			foreach (KeyValuePair<string, string> P in Errors)
			{
				if ((Field = Form[P.Key]) != null)
					Field.Error = P.Value;
			}

			Xml.Append("<error type='modify'>");
			Xml.Append("<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");

			Form.SerializeForm(Xml);

			Xml.Append("</error>");

			return Xml.ToString();
		}

		private async Task GetCommonNodeParametersForEditHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef;
			DataSourceRec Rec;
			INode Node;
			XmlElement E;
			DataForm Form = null;
			DataForm Form2;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || E.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
					return;
				}

				Node = await Rec.Source.GetNodeAsync(ThingRef);
				if (Node is null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}
				else if (!await Node.CanEditAsync(Caller))
				{
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
					return;
				}

				if (Form is null)
					Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
				else
				{
					Form2 = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
					Parameters.MergeForms(Form, Form2);
				}
			}

			if (Form is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
				return;
			}

			Form.RemoveExcluded();

			StringBuilder Xml = new StringBuilder();

			Form.SerializeForm(Xml);

			e.IqResult(Xml.ToString());
		}

		private async Task SetCommonNodeParametersAfterEditHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			LinkedList<Tuple<IDataSource, INode>> Nodes = null;
			DataForm Form = null;
			ThingReference ThingRef;
			DataSourceRec Rec;
			INode Node;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "nd":
						ThingRef = GetThingReference((XmlElement)N);

						lock (this.synchObject)
						{
							if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
								Rec = null;
						}

						if (Rec is null)
							Node = null;
						else
							Node = await Rec.Source.GetNodeAsync(ThingRef);

						if (Node is null || !await Node.CanViewAsync(Caller))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
							return;
						}
						else if (!await Node.CanEditAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}

						if (Nodes is null)
							Nodes = new LinkedList<Tuple<IDataSource, INode>>();

						Nodes.AddLast(new Tuple<IDataSource, INode>(Rec.Source, Node));
						break;

					case "x":
						Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
						break;
				}
			}

			if (Nodes is null)
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (Form is null)
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 10, "Data form missing."), e.IQ));
			else
			{
				foreach (Tuple<IDataSource, INode> P in Nodes)
				{
					string OldNodeId = P.Item2.NodeId;
					string OldSourceId = P.Item2.SourceId;
					string OldPartition = P.Item2.Partition;
					string NewNodeId = Form["NodeId"]?.ValueString;
					string NewSourceId = Form["SourceId"]?.ValueString;
					string NewPartition = Form["Partition"]?.ValueString;
					Parameters.SetEditableFormResult Result = null;

					if (!string.IsNullOrEmpty(NewNodeId))
					{
						if (string.IsNullOrEmpty(NewSourceId))
							NewSourceId = OldSourceId;

						if (string.IsNullOrEmpty(NewPartition))
							NewPartition = OldPartition;

						if ((NewNodeId != OldNodeId ||
							NewSourceId != OldSourceId ||
							NewPartition != OldPartition) &&
							await P.Item1.GetNodeAsync(new ThingReference(NewNodeId, NewSourceId, NewPartition)) != null)
						{
							Result = new Parameters.SetEditableFormResult()
							{
								Errors = new KeyValuePair<string, string>[]
								{
										new KeyValuePair<string, string>("NodeId", "Identity already exists.")
								}
							};
						}
					}

					ILifeCycleManagement LifeCycleManagement = P.Item2 as ILifeCycleManagement;
					bool PreProvisioned = LifeCycleManagement != null && LifeCycleManagement.IsProvisioned;

					if (Result is null)
						Result = await Parameters.SetEditableForm(e, P.Item2, Form, true);

					if (Result.Errors != null)
					{
						Form = null;
						DataForm Form2 = null;

						foreach (Tuple<IDataSource, INode> NodeRef in Nodes)
						{
							Node = NodeRef.Item2;

							if (Form is null)
								Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
							else
							{
								Form2 = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
								Parameters.MergeForms(Form, Form2);
							}
						}

						Form.RemoveExcluded();

						e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
						break;
					}

					Result.Tags.Add(new KeyValuePair<string, object>("Full", e.From));
					Result.Tags.Add(new KeyValuePair<string, object>("Source", P.Item2.SourceId));
					Result.Tags.Add(new KeyValuePair<string, object>("Partition", P.Item2.Partition));

					Log.Informational("Node edited.", P.Item2.NodeId, e.FromBareJid, "NodeEdited", EventLevel.Medium, Result.Tags.ToArray());

					if (this.thingRegistryClient != null && LifeCycleManagement != null)
					{
						if (LifeCycleManagement.IsProvisioned)
						{
							if (string.IsNullOrEmpty(LifeCycleManagement.Owner))
								await this.RegisterNode(LifeCycleManagement);
							else
								await this.UpdateNodeRegistration(LifeCycleManagement);
						}
						else if (PreProvisioned)
							this.UnregisterNode(LifeCycleManagement);
					}
				}

				e.IqResult(string.Empty);
			}
		}

		#endregion

		#region Creating & Destroying nodes

		private async Task GetAddableNodeTypesHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				return;
			}
			else if (!await Node.CanAddAsync(Caller))
			{
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				return;
			}

			StringBuilder Xml = new StringBuilder();
			INode PresumptiveChild;

			Xml.Append("<nodeTypes xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (Type T in Types.GetTypesImplementingInterface(typeof(INode)))
			{
				try
				{
					PresumptiveChild = (INode)Activator.CreateInstance(T);

					if (await Node.AcceptsChildAsync(PresumptiveChild) && await PresumptiveChild.AcceptsParentAsync(Node))
					{
						Xml.Append("<nodeType type='");
						Xml.Append(XML.Encode(T.FullName));
						Xml.Append("' name='");
						Xml.Append(XML.Encode(await PresumptiveChild.GetTypeNameAsync(Language)));
						Xml.Append("'/>");
					}
				}
				catch (Exception)
				{
					continue;
				}
			}

			Xml.Append("</nodeTypes>");

			e.IqResult(Xml.ToString());
		}

		private async Task GetParametersForNewNodeHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				return;
			}
			else if (!await Node.CanAddAsync(Caller))
			{
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				return;
			}

			string TypeName = XML.Attribute(e.Query, "type");
			Type Type = Types.GetType(TypeName);
			if (Type is null)
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 9, "Type not found."), e.IQ));
				return;
			}

			if (!typeof(INode).GetTypeInfo().IsAssignableFrom(Type.GetTypeInfo()))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
				return;
			}

			INode PresumptiveChild;

			try
			{
				PresumptiveChild = (INode)Activator.CreateInstance(Type);
			}
			catch (Exception)
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
				return;
			}

			DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, PresumptiveChild,
				await PresumptiveChild.GetTypeNameAsync(Language));

			StringBuilder Xml = new StringBuilder();

			Form.SerializeForm(Xml);

			string s = Xml.ToString();
			e.IqResult(s);
		}

		private async Task CreateNewNodeHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				return;
			}
			else if (!await Node.CanAddAsync(Caller))
			{
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				return;
			}

			string TypeName = XML.Attribute(e.Query, "type");
			Type Type = Types.GetType(TypeName);
			if (Type is null)
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 9, "Type not found."), e.IQ));
				return;
			}

			if (!typeof(INode).GetTypeInfo().IsAssignableFrom(Type.GetTypeInfo()))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
				return;
			}

			INode PresumptiveChild;

			try
			{
				PresumptiveChild = (INode)Activator.CreateInstance(Type);
			}
			catch (Exception)
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
				return;
			}

			if (!await Node.AcceptsChildAsync(PresumptiveChild) || !await PresumptiveChild.AcceptsParentAsync(Node))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
				return;
			}

			DataForm Form = null;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				if (N.LocalName == "x")
				{
					Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
					break;
				}
			}

			if (Form is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 12, "Missing form."), e.IQ));
				return;
			}

			Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, PresumptiveChild, Form, false);

			if (Result.Errors is null)
			{
				if (await Rec.Source.GetNodeAsync(PresumptiveChild) != null)
				{
					Result.Errors = new KeyValuePair<string, string>[]
					{
							new KeyValuePair<string, string>("NodeId", "Identity already exists.")
					};
				}
			}

			if (Result.Errors is null)
			{
				StringBuilder Xml = new StringBuilder();

				await Node.AddAsync(PresumptiveChild);

				Xml.Append("<nodeInfo xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'");
				await ExportAttributes(Xml, PresumptiveChild, Language);
				Xml.Append(">");
				await this.ExportParametersAndMessages(Xml, PresumptiveChild, true, true, Language, Caller);
				Xml.Append("</nodeInfo>");

				e.IqResult(Xml.ToString());

				Result.Tags.Add(new KeyValuePair<string, object>("Full", e.From));
				Result.Tags.Add(new KeyValuePair<string, object>("Parent", Node.NodeId));
				Result.Tags.Add(new KeyValuePair<string, object>("Source", PresumptiveChild.SourceId));
				Result.Tags.Add(new KeyValuePair<string, object>("Partition", PresumptiveChild.Partition));

				Log.Informational("Node created.", PresumptiveChild.NodeId, e.FromBareJid, "NodeCreated", EventLevel.Major, Result.Tags.ToArray());

				if (this.thingRegistryClient != null && PresumptiveChild is ILifeCycleManagement LifeCycleManagement && LifeCycleManagement.IsProvisioned)
					await this.RegisterNode(LifeCycleManagement);
			}
			else
			{
				Form = await Parameters.GetEditableForm(Sender as XmppClient, e, PresumptiveChild, await PresumptiveChild.GetTypeNameAsync(Language));
				e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
			}
		}

		/// <summary>
		/// Registers a node in the thing registry.
		/// </summary>
		/// <param name="Node">Node to register.</param>
		public async Task RegisterNode(ILifeCycleManagement Node)
		{
			KeyValuePair<string, object>[] MetaData = await Node.GetMetaData();

			this.thingRegistryClient?.RegisterThing(false, Node.NodeId, Node.SourceId, Node.Partition, await this.GetTags(Node, MetaData, true),
				async (sender, e) =>
				{
					try
					{
						if (e.Ok)
						{
							if (e.IsClaimed)
							{
								Log.Informational("Node is owned.", Node.NodeId,
									new KeyValuePair<string, object>("SourceId", Node.SourceId),
									new KeyValuePair<string, object>("Partition", Node.Partition),
									new KeyValuePair<string, object>("Owner", e.OwnerJid),
									new KeyValuePair<string, object>("IsPublic", e.IsPublic));

								await Node.Claimed(e.OwnerJid, e.IsPublic);
								await RuntimeSettings.SetAsync(this.KeyId(Node), string.Empty);
								await this.UpdateNodeRegistration(Node);
							}
							else
							{
								Log.Informational("Node registration successful.", Node.NodeId,
									new KeyValuePair<string, object>("SourceId", Node.SourceId),
									new KeyValuePair<string, object>("Partition", Node.Partition));
							}
						}
						else
						{
							Log.Error("Unable to register node in thing registry.", Node.NodeId,
								new KeyValuePair<string, object>("SourceId", Node.SourceId),
								new KeyValuePair<string, object>("Partition", Node.Partition));
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}, null);
		}

		private string KeyId(IThingReference Node)
		{
			return "KEY." + Node.NodeId + "." + Node.SourceId + "." + Node.Partition;
		}

		/// <summary>
		/// Updates a node in the thing registry.
		/// </summary>
		/// <param name="Node">Node to update.</param>
		public async Task UpdateNodeRegistration(ILifeCycleManagement Node)
		{
			KeyValuePair<string, object>[] MetaData = await Node.GetMetaData();

			this.thingRegistryClient?.UpdateThing(Node.NodeId, Node.SourceId, Node.Partition, await this.GetTags(Node, MetaData, false),
				async (sender, e) =>
				{
					try
					{
						if (e.Ok)
						{
							if (e.Disowned)
							{
								Log.Informational("Node is disowned.", Node.NodeId,
									new KeyValuePair<string, object>("SourceId", Node.SourceId),
									new KeyValuePair<string, object>("Partition", Node.Partition));

								await Node.Disowned();
								await this.RegisterNode(Node);
							}
							else
							{
								Log.Informational("Node registration updated.", Node.NodeId,
									new KeyValuePair<string, object>("SourceId", Node.SourceId),
									new KeyValuePair<string, object>("Partition", Node.Partition));
							}
						}
						else
						{
							Log.Error("Unable to update node registration in thing registry.", Node.NodeId,
								new KeyValuePair<string, object>("SourceId", Node.SourceId),
								new KeyValuePair<string, object>("Partition", Node.Partition));
						}
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				}, null);
		}

		/// <summary>
		/// Unregisters a node from the thing registry.
		/// </summary>
		/// <param name="Node">Node to unregister.</param>
		public void UnregisterNode(ILifeCycleManagement Node)
		{
			string NodeId = Node.NodeId;
			string SourceId = Node.SourceId;
			string Partition = Node.Partition;

			this.thingRegistryClient?.Unregister(NodeId, SourceId, Partition, (sender, e) =>
			{
				try
				{
					if (e.Ok)
					{
						Log.Informational("Node is unregistered.", NodeId,
							new KeyValuePair<string, object>("SourceId", SourceId),
							new KeyValuePair<string, object>("Partition", Partition));
					}
					else
					{
						Log.Error("Unable to unregister node from thing registry.", NodeId,
							new KeyValuePair<string, object>("SourceId", SourceId),
							new KeyValuePair<string, object>("Partition", Partition));
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return Task.CompletedTask;
			}, null);
		}

		private async Task<MetaDataTag[]> GetTags(INode Node, KeyValuePair<string, object>[] MetaData, bool IncludeKey)
		{
			List<MetaDataTag> Result = new List<MetaDataTag>();
			object Value;

			foreach (KeyValuePair<string, object> P in MetaData)
			{
				Value = P.Value;
				if (Value is null)
					Result.Add(new MetaDataStringTag(P.Key, string.Empty));
				else if (Value is string s)
					Result.Add(new MetaDataStringTag(P.Key, s));
				else if (Value is double dbl)
					Result.Add(new MetaDataNumericTag(P.Key, dbl));
				else if (Value is float sng)
					Result.Add(new MetaDataNumericTag(P.Key, sng));
				else if (Value is decimal dec)
					Result.Add(new MetaDataNumericTag(P.Key, (double)dec));
				else if (Value is byte ui8)
					Result.Add(new MetaDataNumericTag(P.Key, ui8));
				else if (Value is short i16)
					Result.Add(new MetaDataNumericTag(P.Key, i16));
				else if (Value is int i32)
					Result.Add(new MetaDataNumericTag(P.Key, i32));
				else if (Value is long i64)
					Result.Add(new MetaDataNumericTag(P.Key, i64));
				else if (Value is sbyte i8)
					Result.Add(new MetaDataNumericTag(P.Key, i8));
				else if (Value is ushort ui16)
					Result.Add(new MetaDataNumericTag(P.Key, ui16));
				else if (Value is uint ui32)
					Result.Add(new MetaDataNumericTag(P.Key, ui32));
				else if (Value is ulong ui64)
					Result.Add(new MetaDataNumericTag(P.Key, ui64));
				else
					Result.Add(new MetaDataStringTag(P.Key, P.Value.ToString()));
			}

			if (IncludeKey)
			{
				string KeyId = this.KeyId(Node);
				string Key = await RuntimeSettings.GetAsync(KeyId, string.Empty);

				if (string.IsNullOrEmpty(Key))
				{
					byte[] Bin = new byte[32];

					lock (this.rnd)
					{
						this.rnd.GetBytes(Bin);
					}

					Key = Hashes.BinaryToString(Bin);
					await RuntimeSettings.SetAsync(KeyId, Key);
				}

				Result.Add(new MetaDataStringTag("KEY", Key));

				MetaDataTag[] Tags = Result.ToArray();
				string IoTDisco = ThingRegistryClient.EncodeAsIoTDiscoURI(Tags);

				await RuntimeSettings.SetAsync("IoTDisco." + KeyId, IoTDisco);

				return Tags;
			}
			else
				return Result.ToArray();
		}

		private async Task DestroyNodeHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				return;
			}
			else if (!await Node.CanDestroyAsync(Caller))
			{
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				return;
			}

			INode Parent = await Rec.Source.GetNodeAsync(Node.Parent);
			KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
			{
					new KeyValuePair<string, object>("Full", e.From),
					new KeyValuePair<string, object>("Source", Node.SourceId),
					new KeyValuePair<string, object>("Partition", Node.Partition)
			};

			if (Node is ILifeCycleManagement LifeCycleManagement)
				this.UnregisterNode(LifeCycleManagement);

			if (!(Parent is null))
				await Parent.RemoveAsync(Node);

			await Node.DestroyAsync();

			Log.Informational("Node destroyed.", Node.NodeId, e.FromBareJid, "NodeDestroyed", EventLevel.Major, Tags);

			e.IqResult(string.Empty);
		}

		#endregion

		#region Commands

		private async Task GetNodeCommandsHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<commands xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				if (Node.HasCommands)
				{
					foreach (ICommand Command in await Node.Commands)
					{
						if (!await Command.CanExecuteAsync(Caller))
							continue;

						await ExportXml(Xml, Command, Language);
					}
				}

				Xml.Append("</commands>");

				e.IqResult(Xml.ToString());
			}
		}

		private static async Task ExportXml(StringBuilder Xml, ICommand Command, Language Language)
		{
			string s;

			Xml.Append("<command command='");
			Xml.Append(XML.Encode(Command.CommandID));
			Xml.Append("' name='");
			Xml.Append(XML.Encode(await Command.GetNameAsync(Language)));
			Xml.Append("' type='");
			Xml.Append(Command.Type.ToString());

			s = await Command.GetSuccessStringAsync(Language);
			if (!string.IsNullOrEmpty(s))
			{
				Xml.Append("' successString='");
				Xml.Append(XML.Encode(s));
			}

			s = await Command.GetFailureStringAsync(Language);
			if (!string.IsNullOrEmpty(s))
			{
				Xml.Append("' failureString='");
				Xml.Append(XML.Encode(s));
			}

			s = await Command.GetConfirmationStringAsync(Language);
			if (!string.IsNullOrEmpty(s))
			{
				Xml.Append("' confirmationString='");
				Xml.Append(XML.Encode(s));
			}

			s = Command.SortCategory;
			if (!string.IsNullOrEmpty(s))
			{
				Xml.Append("' sortCategory='");
				Xml.Append(XML.Encode(s));
			}

			s = Command.SortKey;
			if (!string.IsNullOrEmpty(s))
			{
				Xml.Append("' sortKey='");
				Xml.Append(XML.Encode(s));
			}

			Xml.Append("'/>");
		}

		private static async Task<ICommand> FindCommand(string CommandId, INode Node)
		{
			if (Node.HasCommands)
			{
				foreach (ICommand C in await Node.Commands)
				{
					if (C.CommandID == CommandId)
						return C;
				}
			}

			return null;
		}

		private async Task GetCommandParametersHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				string CommandId = XML.Attribute(e.Query, "command");
				ICommand Command = await FindCommand(CommandId, Node);

				if (Command is null)
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
				else if (!await Command.CanExecuteAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else
				{
					DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Command, await Command.GetNameAsync(Language));
					StringBuilder Xml = new StringBuilder();

					Form.SerializeForm(Xml);

					e.IqResult(Xml.ToString());
				}
			}
		}

		private static void DisposeObject(object Object)
		{
			if (Object is IDisposable Disposable)
				Disposable.Dispose();
		}

		private async Task ExecuteNodeCommandHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				string CommandId = XML.Attribute(e.Query, "command");
				ICommand Command = await FindCommand(CommandId, Node);

				if (Command is null)
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
				else if (!await Command.CanExecuteAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else
				{
					DataForm Form = null;

					foreach (XmlNode N in e.Query.ChildNodes)
					{
						if (N.LocalName == "x")
						{
							Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
							break;
						}
					}

					if (Form is null)
					{
						if (Command.Type != CommandType.Simple)
						{
							e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
							return;
						}
					}
					else
					{
						if (Command.Type != CommandType.Parametrized)
						{
							e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 16, "Parametrized command expected."), e.IQ));
							return;
						}

						Command = Command.Copy();

						Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

						if (Result.Errors != null)
						{
							DisposeObject(Command);

							Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Command, await Command.GetNameAsync(Language));
							e.IqError(this.GetFormErrorsXml(Result.Errors, Form));

							return;
						}
					}

					try
					{
						await Command.ExecuteCommandAsync();
					}
					finally
					{
						if (Command.Type != CommandType.Simple)
							DisposeObject(Command);
					}

					e.IqResult(string.Empty);
				}
			}
		}

		private async Task ExecuteNodeQueryHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				string CommandId = XML.Attribute(e.Query, "command");
				string QueryId = XML.Attribute(e.Query, "queryId");
				ICommand Command = await FindCommand(CommandId, Node);

				if (Command is null)
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
				else if (!await Command.CanExecuteAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else if (Command.Type != CommandType.Query)
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 17, "Query command expected."), e.IQ));
				else
				{
					DataForm Form = null;

					foreach (XmlNode N in e.Query.ChildNodes)
					{
						if (N.LocalName == "x")
						{
							Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
							break;
						}
					}

					if (Form is null)
					{
						e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
						return;
					}

					Command = Command.Copy();

					Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

					if (Result.Errors != null)
					{
						DisposeObject(Command);
						e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
						return;
					}

					Query Query = new Query(CommandId, QueryId, new object[] { Sender, e }, Language, Node);

					lock (this.synchObject)
					{
						if (this.queries.ContainsKey(QueryId))
							Query = null;
						else
							this.queries[QueryId] = Query;
					}

					if (Query is null)
					{
						DisposeObject(Command);
						e.IqError(new StanzaErrors.ConflictException(await GetErrorMessage(Language, 18, "Query with same ID already running."), e.IQ));
						return;
					}

					e.IqResult(string.Empty);

					Query.OnAborted += Query_OnAborted;
					Query.OnBeginSection += Query_OnBeginSection;
					Query.OnDone += Query_OnDone;
					Query.OnEndSection += Query_OnEndSection;
					Query.OnMessage += Query_OnMessage;
					Query.OnNewObject += Query_OnNewObject;
					Query.OnNewRecords += Query_OnNewRecords;
					Query.OnNewTable += Query_OnNewTable;
					Query.OnStarted += Query_OnStarted;
					Query.OnStatus += Query_OnStatus;
					Query.OnTableDone += Query_OnTableDone;
					Query.OnTitle += Query_OnTitle;

					try
					{
						await Query.Start();
						await Command.StartQueryExecutionAsync(Query, Language);
					}
					catch (Exception ex)
					{
						lock (this.synchObject)
						{
							this.queries.Remove(QueryId);
						}

						await Query.LogMessage(QueryEventType.Exception, QueryEventLevel.Major, ex.Message);
						await Query.Abort();

						System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
					}
					finally
					{
						DisposeObject(Command);
					}
				}
			}
		}

		private async Task AbortNodeQueryHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else
			{
				string CommandId = XML.Attribute(e.Query, "command");
				string QueryId = XML.Attribute(e.Query, "queryId");
				ICommand Command = await FindCommand(CommandId, Node);

				if (Command is null)
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
				else if (!await Command.CanExecuteAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else if (Command.Type != CommandType.Query)
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 17, "Query command expected."), e.IQ));
				else
				{
					Query Query;

					lock (this.synchObject)
					{
						if (this.queries.TryGetValue(QueryId, out Query))
							this.queries.Remove(QueryId);
						else
							Query = null;
					}

					if (Query is null)
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 19, "Query not found."), e.IQ));
						return;
					}

					await Query.Abort();

					e.IqResult(string.Empty);
				}
			}
		}

		private Task Query_OnTitle(object Sender, QueryTitleEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<title name='");
			Xml.Append(XML.Encode(e.Title));
			Xml.Append("'/>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			return Task.CompletedTask;
		}

		private void StartQueryProgress(StringBuilder Xml, QueryEventArgs e)
		{
			INode Node = e.Query.NodeReference;
			string s;

			Xml.Append("<queryProgress xmlns='");
			Xml.Append(NamespaceConcentrator);

			if (!string.IsNullOrEmpty(s = Node.SourceId))
			{
				Xml.Append("' src='");
				Xml.Append(XML.Encode(s));
			}

			if (!string.IsNullOrEmpty(s = Node.Partition))
			{
				Xml.Append("' pt='");
				Xml.Append(XML.Encode(s));
			}

			Xml.Append("' id='");
			Xml.Append(XML.Encode(Node.NodeId));
			Xml.Append("' queryId='");
			Xml.Append(XML.Encode(e.Query.QueryID));
			Xml.Append("' seqNr='");
			Xml.Append(e.Query.NextSequenceNumber().ToString());
			Xml.Append("'>");
		}

		private void EndQueryProgress(StringBuilder Xml)
		{
			Xml.Append("</queryProgress>");
		}

		private Task Query_OnTableDone(object Sender, QueryTableEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<tableDone tableId='");
			Xml.Append(XML.Encode(e.TableId));
			Xml.Append("'/>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

			return Task.CompletedTask;
		}

		private Task Query_OnStatus(object Sender, QueryStatusEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<status message='");
			Xml.Append(XML.Encode(e.Status));
			Xml.Append("'/>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnStarted(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryStarted/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnNewTable(object Sender, QueryNewTableEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();
			SKColor cl;

			this.StartQueryProgress(Xml, e);

			Xml.Append("<newTable tableId='");
			Xml.Append(XML.Encode(e.TableId));
			Xml.Append("' tableName='");
			Xml.Append(XML.Encode(e.TableName));
			Xml.Append("'>");

			foreach (Column Column in e.Columns)
			{
				Xml.Append("<column columnId='");
				Xml.Append(XML.Encode(Column.ColumnId));

				if (!string.IsNullOrEmpty(Column.Header))
				{
					Xml.Append("' header='");
					Xml.Append(XML.Encode(Column.Header));
				}

				if (!string.IsNullOrEmpty(Column.DataSourceId))
				{
					Xml.Append("' src='");
					Xml.Append(XML.Encode(Column.DataSourceId));
				}

				if (!string.IsNullOrEmpty(Column.Partition))
				{
					Xml.Append("' pt='");
					Xml.Append(XML.Encode(Column.Partition));
				}

				if (Column.FgColor.HasValue)
				{
					cl = Column.FgColor.Value;

					Xml.Append("' fgColor='");
					Xml.Append(cl.Red.ToString("X2"));
					Xml.Append(cl.Green.ToString("X2"));
					Xml.Append(cl.Blue.ToString("X2"));
				}

				if (Column.BgColor.HasValue)
				{
					cl = Column.BgColor.Value;

					Xml.Append("' bgColor='");
					Xml.Append(cl.Red.ToString("X2"));
					Xml.Append(cl.Green.ToString("X2"));
					Xml.Append(cl.Blue.ToString("X2"));
				}

				if (Column.Alignment.HasValue)
				{
					Xml.Append("' alignment='");
					Xml.Append(Column.Alignment.Value.ToString());
				}

				if (Column.NrDecimals.HasValue)
				{
					Xml.Append("' nrDecimals='");
					Xml.Append(Column.NrDecimals.Value.ToString());
				}

				Xml.Append("'/>");
			}

			Xml.Append("</newTable>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnNewRecords(object Sender, QueryNewRecordsEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<newRecords tableId='");
			Xml.Append(XML.Encode(e.TableId));
			Xml.Append("'>");

			foreach (Record Record in e.Records)
			{
				Xml.Append("<record>");

				foreach (object Element in Record.Elements)
				{
					if (Element is null)
					{
						Xml.Append("<void/>");
					}
					else if (Element is bool b)
					{
						Xml.Append("<boolean>");
						Xml.Append(CommonTypes.Encode(b));
						Xml.Append("</boolean>");
					}
					else if (Element is SKColor cl)
					{
						Xml.Append("<color>");
						Xml.Append(cl.Red.ToString("X2"));
						Xml.Append(cl.Green.ToString("X2"));
						Xml.Append(cl.Blue.ToString("X2"));
						Xml.Append("</color>");
					}
					else if (Element is DateTime TP)
					{
						if (TP.TimeOfDay == TimeSpan.Zero)
						{
							Xml.Append("<date>");
							Xml.Append(XML.Encode((DateTime)Element, true));
							Xml.Append("</date>");
						}
						else
						{
							Xml.Append("<dateTime>");
							Xml.Append(XML.Encode((DateTime)Element));
							Xml.Append("</dateTime>");
						}
					}
					else if (Element is double d)
					{
						Xml.Append("<double>");
						Xml.Append(CommonTypes.Encode(d));
						Xml.Append("</double>");
					}
					else if (Element is float f)
					{
						Xml.Append("<double>");
						Xml.Append(CommonTypes.Encode(f));
						Xml.Append("</double>");
					}
					else if (Element is decimal dec)
					{
						Xml.Append("<double>");
						Xml.Append(CommonTypes.Encode(dec));
						Xml.Append("</double>");
					}
					else if (Element is Duration)
					{
						Xml.Append("<duration>");
						Xml.Append(Element.ToString());
						Xml.Append("</duration>");
					}
					else if (Element is int || Element is byte || Element is sbyte || Element is short || Element is ushort)
					{
						Xml.Append("<int>");
						Xml.Append(Element.ToString());
						Xml.Append("</int>");
					}
					else if (Element is long || Element is uint || (Element is ulong ul && ul <= long.MaxValue))
					{
						Xml.Append("<long>");
						Xml.Append(Element.ToString());
						Xml.Append("</long>");
					}
					else if (Element is ulong)
					{
						Xml.Append("<double>");
						Xml.Append(Element.ToString());
						Xml.Append("</double>");
					}
					else if (Element is string || Element is char)
					{
						Xml.Append("<string>");
						Xml.Append(XML.Encode(Element.ToString()));
						Xml.Append("</string>");
					}
					else if (Element is TimeSpan)
					{
						Xml.Append("<time>");
						Xml.Append(Element.ToString());
						Xml.Append("</time>");
					}
					else
					{
						byte[] Bin = InternetContent.Encode(Element, Encoding.UTF8, out string ContentType);

						Xml.Append("<base64 contentType='");
						Xml.Append(XML.Encode(ContentType));
						Xml.Append("'>");
						Xml.Append(Convert.ToBase64String(Bin));
						Xml.Append("</base64>");
					}
				}

				Xml.Append("</record>");
			}

			Xml.Append("</newRecords>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnNewObject(object Sender, QueryObjectEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();
			byte[] Bin = InternetContent.Encode(e.Object, Encoding.UTF8, out string ContentType);

			this.StartQueryProgress(Xml, e);

			Xml.Append("<newObject contentType='");
			Xml.Append(XML.Encode(ContentType));
			Xml.Append("'>");
			Xml.Append(Convert.ToBase64String(Bin));
			Xml.Append("</newObject>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnMessage(object Sender, QueryMessageEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<queryMessage type='");
			Xml.Append(e.Type.ToString());
			Xml.Append("' level='");
			Xml.Append(e.Level.ToString());
			Xml.Append("'>");
			Xml.Append(XML.Encode(e.Body));
			Xml.Append("</queryMessage>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnEndSection(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<endSection/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnDone(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryDone/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnBeginSection(object Sender, QueryTitleEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);

			Xml.Append("<beginSection header='");
			Xml.Append(XML.Encode(e.Title));
			Xml.Append("'/>");

			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private Task Query_OnAborted(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryAborted/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		
			return Task.CompletedTask;
		}

		private class CommandComparer : IEqualityComparer<ICommand>
		{
			public bool Equals(ICommand x, ICommand y)
			{
				return x.GetType() == y.GetType() &&
					x.Type == y.Type &&
					x.CommandID == y.CommandID &&
					x.SortCategory == y.SortCategory &&
					x.SortKey == y.SortKey;
			}

			public int GetHashCode(ICommand obj)
			{
				return obj.GetType().GetHashCode() ^
					obj.Type.GetHashCode() ^
					obj.CommandID.GetHashCode() ^
					obj.SortCategory.GetHashCode() ^
					obj.SortKey.GetHashCode();
			}
		}

		private static readonly CommandComparer commandComparerInstance = new CommandComparer();

		private async Task GetCommonNodeCommandsHandler(object Sender, IqEventArgs e)
		{
			Dictionary<ICommand, bool> CommonCommands = null;
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			Language Language = await GetLanguage(e.Query);
			ThingReference ThingRef;
			DataSourceRec Rec;
			XmlElement E;
			INode Node;

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || N.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				if (Node is null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}

				if (!Node.HasCommands)
				{
					if (!(CommonCommands is null))
						CommonCommands.Clear();

					break;
				}

				if (CommonCommands is null)
				{
					CommonCommands = new Dictionary<ICommand, bool>(commandComparerInstance);

					foreach (ICommand Command in await Node.Commands)
					{
						if (await Command.CanExecuteAsync(Caller))
							CommonCommands[Command] = true;
					}
				}
				else
				{
					Dictionary<ICommand, bool> CommonCommands2 = new Dictionary<ICommand, bool>(commandComparerInstance);

					foreach (ICommand Command in await Node.Commands)
					{
						if (CommonCommands.ContainsKey(Command) && await Command.CanExecuteAsync(Caller))
							CommonCommands2[Command] = true;
					}

					CommonCommands = CommonCommands2;
				}
			}

			StringBuilder Xml = new StringBuilder();

			Xml.Append("<commands xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			if (!(CommonCommands is null))
			{
				foreach (ICommand Command in CommonCommands.Keys)
					await ExportXml(Xml, Command, Language);
			}

			Xml.Append("</commands>");

			e.IqResult(Xml.ToString());
		}

		private async Task GetCommonCommandParametersHandler(object Sender, IqEventArgs e)
		{
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef;
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			XmlElement E;
			INode Node;
			ICommand Command = null;
			ICommand Command2;
			DataForm Form = null;
			DataForm Form2;
			string CommandId = XML.Attribute(e.Query, "command");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null || N.LocalName != "nd")
					continue;

				ThingRef = GetThingReference(E);

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
						Rec = null;
				}

				if (Rec is null)
					Node = null;
				else
					Node = await Rec.Source.GetNodeAsync(ThingRef);

				if (Node is null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}

				if (Command is null)
				{
					Command = await FindCommand(CommandId, Node);

					if (Command is null)
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
						return;
					}
					else if (!await Command.CanExecuteAsync(Caller))
					{
						e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
						return;
					}

					Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Command, await Command.GetNameAsync(Language));
				}
				else
				{
					Command2 = await FindCommand(CommandId, Node);
					if (Command2 is null || !commandComparerInstance.Equals(Command, Command2))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
						return;
					}
					else if (!await Command2.CanExecuteAsync(Caller))
					{
						e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
						return;
					}

					Form2 = await Parameters.GetEditableForm(Sender as XmppClient, e, Command2, await Command2.GetNameAsync(Language));
					Parameters.MergeForms(Form, Form2);
				}
			}

			if (Form is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
				return;
			}

			Form.RemoveExcluded();

			StringBuilder Xml = new StringBuilder();

			Form.SerializeForm(Xml);

			e.IqResult(Xml.ToString());
		}

		private async Task ExecuteCommonNodeCommandHandler(object Sender, IqEventArgs e)
		{
			LinkedList<INode> Nodes = null;
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef;
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			XmlElement E;
			INode Node;
			ICommand Command = null;
			ICommand Command2;
			DataForm Form = null;
			string CommandId = XML.Attribute(e.Query, "command");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.LocalName == "nd")
				{
					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
							Rec = null;
					}

					if (Rec is null)
						Node = null;
					else
						Node = await Rec.Source.GetNodeAsync(ThingRef);

					if (Node is null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					if (Nodes is null)
						Nodes = new LinkedList<INode>();

					Nodes.AddLast(Node);

					if (Command is null)
					{
						Command = await FindCommand(CommandId, Node);

						if (Command is null)
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
					else
					{
						Command2 = await FindCommand(CommandId, Node);
						if (Command2 is null || !commandComparerInstance.Equals(Command, Command2))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command2.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
				}
				else if (E.LocalName == "x")
					Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
			}

			if (Nodes is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
				return;
			}

			if (Form is null)
			{
				if (Command.Type != CommandType.Simple)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
					return;
				}
			}
			else
			{
				if (Command.Type != CommandType.Parametrized)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 16, "Parametrized command expected."), e.IQ));
					return;
				}

				Command = Command.Copy();

				Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

				if (Result.Errors != null)
				{
					DisposeObject(Command);
					e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
					return;
				}
			}

			StringBuilder Xml = new StringBuilder();
			string ErrorMessage;

			Xml.Append("<partialExecution xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (INode N in Nodes)
			{
				try
				{
					await Command.ExecuteCommandAsync();
					ErrorMessage = null;
				}
				catch (Exception ex)
				{
					ErrorMessage = ex.Message;
				}

				if (ErrorMessage is null)
					Xml.Append("<result>true</result>");
				else
				{
					Xml.Append("<result error='");
					Xml.Append(XML.Encode(ErrorMessage));
					Xml.Append("'>false</result>");
				}
			}

			if (Command.Type != CommandType.Simple)
				DisposeObject(Command);

			Xml.Append("</partialExecution>");

			e.IqResult(Xml.ToString());
		}

		private async Task ExecuteCommonNodeQueryHandler(object Sender, IqEventArgs e)
		{
			LinkedList<INode> Nodes = null;
			LinkedList<Query> Queries = null;
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef;
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			XmlElement E;
			INode Node;
			ICommand Command = null;
			ICommand Command2;
			DataForm Form = null;
			string CommandId = XML.Attribute(e.Query, "command");
			string QueryId = XML.Attribute(e.Query, "queryId");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.LocalName == "nd")
				{
					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
							Rec = null;
					}

					if (Rec is null)
						Node = null;
					else
						Node = await Rec.Source.GetNodeAsync(ThingRef);

					if (Node is null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					if (Nodes is null)
					{
						Nodes = new LinkedList<INode>();
						Queries = new LinkedList<Query>();
					}

					Nodes.AddLast(Node);
					Queries.AddLast(new Query(CommandId, QueryId, new object[] { Sender, e }, Language, Node));

					if (Command is null)
					{
						Command = await FindCommand(CommandId, Node);

						if (Command is null)
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
					else
					{
						Command2 = await FindCommand(CommandId, Node);
						if (Command2 is null || !commandComparerInstance.Equals(Command, Command2))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command2.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
				}
				else if (E.LocalName == "x")
					Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
			}

			if (Nodes is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
				return;
			}

			if (Command.Type != CommandType.Query)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 17, "Query command expected."), e.IQ));
				return;
			}

			if (Form is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
				return;
			}

			Command = Command.Copy();

			Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

			if (Result.Errors != null)
			{
				DisposeObject(Command);
				e.IqError(this.GetFormErrorsXml(Result.Errors, Form));
				return;
			}

			CompoundQuery Query = new CompoundQuery(CommandId, QueryId, new object[] { Sender, e }, Language, Queries);

			lock (this.synchObject)
			{
				if (this.queries.ContainsKey(QueryId))
					Query = null;
				else
					this.queries[QueryId] = Query;
			}

			if (Query is null)
			{
				DisposeObject(Command);
				e.IqError(new StanzaErrors.ConflictException(await GetErrorMessage(Language, 18, "Query with same ID already running."), e.IQ));
				return;
			}

			Query.OnAborted += Query_OnAborted;
			Query.OnBeginSection += Query_OnBeginSection;
			Query.OnDone += Query_OnDone;
			Query.OnEndSection += Query_OnEndSection;
			Query.OnMessage += Query_OnMessage;
			Query.OnNewObject += Query_OnNewObject;
			Query.OnNewRecords += Query_OnNewRecords;
			Query.OnNewTable += Query_OnNewTable;
			Query.OnStarted += Query_OnStarted;
			Query.OnStatus += Query_OnStatus;
			Query.OnTableDone += Query_OnTableDone;
			Query.OnTitle += Query_OnTitle;

			StringBuilder Xml = new StringBuilder();
			string ErrorMessage;
			bool PartialSuccess = false;

			Xml.Append("<partialExecution xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (INode N in Nodes)
			{
				try
				{
					await Command.StartQueryExecutionAsync(Query, Language);
					ErrorMessage = null;
				}
				catch (Exception ex)
				{
					ErrorMessage = ex.Message;
				}

				if (ErrorMessage is null)
				{
					Xml.Append("<result>true</result>");
					PartialSuccess = true;
				}
				else
				{
					Xml.Append("<result error='");
					Xml.Append(XML.Encode(ErrorMessage));
					Xml.Append("'>false</result>");
				}
			}

			if (!PartialSuccess)
			{
				lock (this.synchObject)
				{
					this.queries.Remove(QueryId);
				}

				await Query.Abort();
			}

			DisposeObject(Command);

			Xml.Append("</partialExecution>");

			e.IqResult(Xml.ToString());
		}

		private async Task AbortCommonNodeQueryHandler(object Sender, IqEventArgs e)
		{
			LinkedList<INode> Nodes = null;
			LinkedList<Query> Queries = null;
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef;
			Language Language = await GetLanguage(e.Query);
			DataSourceRec Rec;
			XmlElement E;
			INode Node;
			ICommand Command = null;
			ICommand Command2;
			string CommandId = XML.Attribute(e.Query, "command");
			string QueryId = XML.Attribute(e.Query, "queryId");

			foreach (XmlNode N in e.Query.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (E.LocalName == "nd")
				{
					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
							Rec = null;
					}

					if (Rec is null)
						Node = null;
					else
						Node = await Rec.Source.GetNodeAsync(ThingRef);

					if (Node is null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					if (Nodes is null)
					{
						Nodes = new LinkedList<INode>();
						Queries = new LinkedList<Query>();
					}

					Nodes.AddLast(Node);
					Queries.AddLast(new Query(CommandId, QueryId, new object[] { Sender, e }, Language, Node));

					if (Command is null)
					{
						Command = await FindCommand(CommandId, Node);

						if (Command is null)
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
					else
					{
						Command2 = await FindCommand(CommandId, Node);
						if (Command2 is null || !commandComparerInstance.Equals(Command, Command2))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
							return;
						}
						else if (!await Command2.CanExecuteAsync(Caller))
						{
							e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
							return;
						}
					}
				}
			}

			if (Nodes is null)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
				return;
			}

			if (Command.Type != CommandType.Query)
			{
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 17, "Query command expected."), e.IQ));
				return;
			}

			Query Query;

			lock (this.synchObject)
			{
				if (this.queries.TryGetValue(QueryId, out Query))
					this.queries.Remove(QueryId);
				else
					Query = null;
			}

			if (Query is null)
			{
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 19, "Query not found."), e.IQ));
				return;
			}

			await Query.Abort();

			e.IqResult(string.Empty);
		}

		#endregion

		#region Sensor Data interface

		private async Task SensorServer_OnExecuteReadoutRequest(object _, SensorDataServerRequest Request)
		{
			DateTime Now = DateTime.Now;
			IThingReference[] Nodes = Request.Nodes;
			int i, c;

			try
			{
				if (Nodes is null || (c = Nodes.Length) == 0)
					Request.ReportErrors(true, new ThingError(ThingReference.Empty, Now, "Node specification required for concentrators."));
				else
				{
					for (i = 0; i < c; i++)
					{
						IThingReference NodeRef = Nodes[i];

						if (!(NodeRef is INode Node))
						{
							if (!this.TryGetDataSource(NodeRef.SourceId, out IDataSource DataSource))
							{
								Request.ReportErrors(i == c - 1, new ThingError(NodeRef, Now, "Data source not found."));
								continue;
							}

							Node = await DataSource.GetNodeAsync(NodeRef);
							if (Node is null)
							{
								Request.ReportErrors(i == c - 1, new ThingError(NodeRef, Now, "Node not found."));
								continue;
							}
						}

						if (!(Node is ISensor Sensor) || !Sensor.IsReadable)
						{
							Request.ReportErrors(i == c - 1, new ThingError(NodeRef, Now, "Node not readable."));
							continue;
						}

						await Sensor.StartReadout(Request);
					}
				}
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(ThingReference.Empty, Now, ex.Message));
			}
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(ThingReference Reference, params Things.SensorData.Field[] Values)
		{
			this.sensorServer.NewMomentaryValues(Reference, Values);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public void NewMomentaryValues(ThingReference Reference, IEnumerable<Things.SensorData.Field> Values)
		{
			this.sensorServer.NewMomentaryValues(Reference, Values);
		}

		#endregion

		#region Control interface

		private async Task<ControlParameter[]> ControlServer_OnGetControlParameters(IThingReference Node)
		{
			try
			{
				if (!(Node is INode Node2))
				{
					if (!this.TryGetDataSource(Node.SourceId, out IDataSource DataSource))
						return null;

					Node2 = await DataSource.GetNodeAsync(Node);
					if (Node2 is null)
						return null;
				}

				if (!(Node2 is IActuator Actuator))
					return null;

				if (!Actuator.IsControllable)
					return new ControlParameter[0];

				return await Actuator.GetControlParameters();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return new ControlParameter[0];
			}
		}

		#endregion

		#region Sniffers

		private async Task RegisterSnifferHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else if (!(Node is ISniffable Sniffable))
				e.IqError(new StanzaErrors.NotAcceptableException(await GetErrorMessage(Language, 21, "Node is not sniffable."), e.IQ));
			else
			{
				DateTime Expires = XML.Attribute(e.Query, "expires", DateTime.Now.AddHours(1));
				RemoteSniffer Sniffer = new RemoteSniffer(e.From, Expires, Sniffable, this);
				DateTime MaxExpires = DateTime.Now.AddDays(1);

				if (Expires > MaxExpires)
					Expires = MaxExpires;

				Sniffable.Add(Sniffer);

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<sniffer xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("' snifferId='");
				Xml.Append(Sniffer.Id);
				Xml.Append("' expires='");
				Xml.Append(XML.Encode(Expires));
				Xml.Append("'/>");

				e.IqResult(Xml.ToString());
			}
		}

		private async Task UnregisterSnifferHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			ThingReference ThingRef = GetThingReference(e.Query);
			DataSourceRec Rec;
			INode Node;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null)
				Node = null;
			else
				Node = await Rec.Source.GetNodeAsync(ThingRef);

			if (Node is null || !await Node.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			else if (!await Node.CanEditAsync(Caller))
				e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
			else if (!(Node is ISniffable Sniffable))
				e.IqError(new StanzaErrors.NotAcceptableException(await GetErrorMessage(Language, 21, "Node is not sniffable."), e.IQ));
			else
			{
				string Id = XML.Attribute(e.Query, "snifferId");

				if (Sniffable.HasSniffers)
				{
					foreach (ISniffer Sniffer in Sniffable.Sniffers)
					{
						if (Sniffer is RemoteSniffer RemoteSniffer && RemoteSniffer.Id == Id)
						{
							Sniffable.Remove(RemoteSniffer);

							e.IqResult(string.Empty);
							return;
						}
					}
				}

				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 22, "Sniffer not found."), e.IQ));
			}
		}

		#endregion

		#region Data Source Events

		private Task DataSource_OnEvent(object _, SourceEvent Event)
		{
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(Event.SourceId, out Rec))
					return Task.CompletedTask;
			}

			SubscriptionRec[] Subscriptions = Rec.SubscriptionsStatic;
			if (Subscriptions is null || Subscriptions.Length == 0 || this.client.State != XmppState.Connected)
				return Task.CompletedTask;

			StringBuilder Xml = null;
			LinkedList<string> ToRemove = null;

			foreach (SubscriptionRec Subscription in Subscriptions)
				this.SendEventMessage(Subscription, Event, ref ToRemove, ref Xml);

			if (!(ToRemove is null))
				this.Remove(Rec, ToRemove);

			return Task.CompletedTask;
		}

		private void Remove(DataSourceRec Rec, LinkedList<string> ToRemove)
		{
			lock (Rec.Subscriptions)
			{
				foreach (string Jid in ToRemove)
					Rec.Subscriptions.Remove(Jid);

				Rec.SubscriptionsStatic = ToArray(Rec.Subscriptions);
			}
		}

		private void SendEventMessage(SubscriptionRec Subscription, SourceEvent Event, ref LinkedList<string> ToRemove, ref StringBuilder Xml)
		{
			SourceEventType EventType = Event.EventType;

			if (Subscription.Expires <= DateTime.Now)
			{
				if (ToRemove is null)
					ToRemove = new LinkedList<string>();

				ToRemove.AddLast(Subscription.Jid);
			}
			else if ((Subscription.EventTypes & EventType) != 0)
			{
				if (Xml is null)
					Xml = new StringBuilder();
				else
					Xml.Clear();

				switch (EventType)
				{
					case SourceEventType.NodeAdded:
						NodeAdded NodeAdded = (NodeAdded)Event;
						Xml.Append("<nodeAdded xmlns='");
						Xml.Append(NamespaceConcentrator);

						if (!string.IsNullOrEmpty(NodeAdded.AfterNodeId))
						{
							Xml.Append("' aid='");
							Xml.Append(XML.Encode(NodeAdded.AfterNodeId));
						}

						if (!string.IsNullOrEmpty(NodeAdded.AfterPartition))
						{
							Xml.Append("' apt='");
							Xml.Append(XML.Encode(NodeAdded.AfterPartition));
						}

						Xml.Append("' nodeType='");
						Xml.Append(XML.Encode(NodeAdded.NodeType));

						Xml.Append("' displayName='");
						Xml.Append(XML.Encode(NodeAdded.DisplayName));

						if (NodeAdded.Sniffable)
							Xml.Append("' sniffable='true");

						this.Append(Xml, NodeAdded, Subscription.Parameters, "nodeAdded");
						break;

					case SourceEventType.NodeUpdated:
						NodeUpdated NodeUpdated = (NodeUpdated)Event;

						Xml.Append("<nodeUpdated xmlns='");
						Xml.Append(NamespaceConcentrator);

						if (!string.IsNullOrEmpty(NodeUpdated.OldId) && NodeUpdated.OldId != NodeUpdated.NodeId)
						{
							Xml.Append("' oid='");
							Xml.Append(XML.Encode(NodeUpdated.OldId));
						}

						this.Append(Xml, NodeUpdated, Subscription.Parameters, "nodeUpdated");
						break;

					case SourceEventType.NodeStatusChanged:
						NodeStatusChanged NodeStatusChanged = (NodeStatusChanged)Event;
						Xml.Append("<nodeStatusChanged xmlns='");
						Xml.Append(NamespaceConcentrator);

						this.Append(Xml, NodeStatusChanged);

						if (Subscription.Messages && NodeStatusChanged.Messages != null)
						{
							Xml.Append("'>");

							foreach (Message Message in NodeStatusChanged.Messages)
								Message.Export(Xml);

							Xml.Append("</nodeStatusChanged>");
						}
						else
							Xml.Append("'/>");
						break;

					case SourceEventType.NodeRemoved:
						Xml.Append("<nodeRemoved xmlns='");
						Xml.Append(NamespaceConcentrator);
						this.Append(Xml, (NodeRemoved)Event);
						Xml.Append("'/>");
						break;

					case SourceEventType.NodeMovedUp:
						Xml.Append("<nodeMovedUp xmlns='");
						Xml.Append(NamespaceConcentrator);
						this.Append(Xml, (NodeMovedUp)Event);
						Xml.Append("'/>");
						break;

					case SourceEventType.NodeMovedDown:
						Xml.Append("<nodeMovedDown xmlns='");
						Xml.Append(NamespaceConcentrator);
						this.Append(Xml, (NodeMovedDown)Event);
						Xml.Append("'/>");
						break;

					default:
						return;
				}

				this.client.SendMessage(MessageType.Normal, Subscription.Jid, Xml.ToString(), string.Empty, string.Empty, Translator.DefaultLanguageCode, string.Empty, string.Empty);
			}
		}

		private static TValue[] ToArray<TKey, TValue>(Dictionary<TKey, TValue> Dictionary)
		{
			TValue[] Result = new TValue[Dictionary.Count];
			Dictionary.Values.CopyTo(Result, 0);
			return Result;
		}

		private void Append(StringBuilder Xml, NodeParametersEvent NodeParametersEvent, bool IncludeParameters, string TagName)
		{
			Xml.Append("' hasChildren='");
			Xml.Append(CommonTypes.Encode(NodeParametersEvent.HasChildren));

			if (NodeParametersEvent.IsReadable)
				Xml.Append("' isReadable='true");

			if (NodeParametersEvent.IsControllable)
				Xml.Append("' isControllable='true");

			if (NodeParametersEvent.HasCommands)
				Xml.Append("' hasCommands='true");

			if (NodeParametersEvent.ChildrenOrdered)
				Xml.Append("' childrenOrdered='true");

			if (!string.IsNullOrEmpty(NodeParametersEvent.ParentId))
			{
				Xml.Append("' parentId='");
				Xml.Append(XML.Encode(NodeParametersEvent.ParentId));
			}

			if (!string.IsNullOrEmpty(NodeParametersEvent.ParentPartition))
			{
				Xml.Append("' parentPartition='");
				Xml.Append(XML.Encode(NodeParametersEvent.ParentPartition));
			}

			if (NodeParametersEvent.Updated != DateTime.MinValue)
			{
				Xml.Append("' lastChanged='");
				Xml.Append(XML.Encode(NodeParametersEvent.Updated));
			}

			this.Append(Xml, (NodeStatusEvent)NodeParametersEvent);

			if (IncludeParameters)
			{
				Xml.Append("'>");

				if (IncludeParameters && NodeParametersEvent.Parameters != null)
				{
					foreach (Parameter Parameter in NodeParametersEvent.Parameters)
						Parameter.Export(Xml);
				}

				Xml.Append("</");
				Xml.Append(TagName);
				Xml.Append('>');
			}
			else
				Xml.Append("'/>");
		}

		private void Append(StringBuilder Xml, NodeStatusEvent NodeStatusEvent)
		{
			Xml.Append("' state='");
			Xml.Append(NodeStatusEvent.State.ToString());

			this.Append(Xml, (NodeEvent)NodeStatusEvent);
		}

		private void Append(StringBuilder Xml, NodeEvent NodeEvent)
		{
			Xml.Append("' id='");
			Xml.Append(XML.Encode(NodeEvent.NodeId));

			if (!string.IsNullOrEmpty(NodeEvent.Partition))
			{
				Xml.Append("' pt='");
				Xml.Append(XML.Encode(NodeEvent.Partition));
			}

			if (!string.IsNullOrEmpty(NodeEvent.LogId))
			{
				Xml.Append("' logId='");
				Xml.Append(XML.Encode(NodeEvent.LogId));
			}

			if (!string.IsNullOrEmpty(NodeEvent.LocalId))
			{
				Xml.Append("' localId='");
				Xml.Append(XML.Encode(NodeEvent.LocalId));
			}

			this.Append(Xml, (SourceEvent)NodeEvent);
		}

		private void Append(StringBuilder Xml, SourceEvent SourceEvent)
		{
			Xml.Append("' src='");
			Xml.Append(XML.Encode(SourceEvent.SourceId));

			Xml.Append("' ts='");
			Xml.Append(XML.Encode(SourceEvent.Timestamp));
		}

		private async Task SubscribeHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			string SourceId = XML.Attribute(e.Query, "src");
			int TtlSeconds = XML.Attribute(e.Query, "ttl", 0);
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null || !await Rec.Source.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
			else if (TtlSeconds <= 0)
				e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 23, "Invalid timeout value."), e.IQ));
			else
			{
				DateTime GetEventsSince = XML.Attribute(e.Query, "getEventsSince", DateTime.MaxValue);
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				SourceEventType Types = SourceEventType.None;

				if (XML.Attribute(e.Query, "nodeAdded", true))
					Types |= SourceEventType.NodeAdded;

				if (XML.Attribute(e.Query, "nodeUpdated", true))
					Types |= SourceEventType.NodeUpdated;

				if (XML.Attribute(e.Query, "nodeStatusChanged", true))
					Types |= SourceEventType.NodeStatusChanged;

				if (XML.Attribute(e.Query, "nodeRemoved", true))
					Types |= SourceEventType.NodeRemoved;

				if (XML.Attribute(e.Query, "nodeMovedUp", true))
					Types |= SourceEventType.NodeMovedUp;

				if (XML.Attribute(e.Query, "nodeMovedDown", true))
					Types |= SourceEventType.NodeMovedDown;

				SubscriptionRec SubscriptionRec;

				lock (Rec.Subscriptions)
				{
					if (Rec.Subscriptions.TryGetValue(e.From, out SubscriptionRec))
					{
						SubscriptionRec.EventTypes |= Types;
						SubscriptionRec.Messages |= Messages;
						SubscriptionRec.Parameters |= Parameters;
						SubscriptionRec.Language = Language;
						SubscriptionRec.Expires = DateTime.Now.AddSeconds(TtlSeconds);
					}
					else
					{
						Rec.Subscriptions[e.From] = SubscriptionRec = new SubscriptionRec()
						{
							Jid = e.From,
							EventTypes = Types,
							Messages = Messages,
							Parameters = Parameters,
							Language = Language,
							Expires = DateTime.Now.AddSeconds(TtlSeconds)
						};

						Rec.SubscriptionsStatic = ToArray(Rec.Subscriptions);
					}
				}

				e.IqResult(string.Empty);

				if (GetEventsSince <= DateTime.Now)
				{
					StringBuilder Xml = null;
					LinkedList<string> ToRemove = null;

					foreach (SourceEvent Event in await Database.Find<SourceEvent>(new FilterAnd(new FilterFieldEqualTo("SourceId", SourceId),
						new FilterFieldGreaterOrEqualTo("Timestamp", GetEventsSince)), "SourceId", "Timestamp"))
					{
						this.SendEventMessage(SubscriptionRec, Event, ref ToRemove, ref Xml);
						if (!(ToRemove is null))
						{
							this.Remove(Rec, ToRemove);
							break;
						}
					}
				}
			}
		}

		private async Task UnsubscribeHandler(object Sender, IqEventArgs e)
		{
			Language Language = await GetLanguage(e.Query);
			RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
			string SourceId = XML.Attribute(e.Query, "src");
			DataSourceRec Rec;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Rec))
					Rec = null;
			}

			if (Rec is null || !await Rec.Source.CanViewAsync(Caller))
				e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
			else
			{
				SourceEventType Types = SourceEventType.None;

				if (XML.Attribute(e.Query, "nodeAdded", true))
					Types |= SourceEventType.NodeAdded;

				if (XML.Attribute(e.Query, "nodeUpdated", true))
					Types |= SourceEventType.NodeUpdated;

				if (XML.Attribute(e.Query, "nodeStatusChanged", true))
					Types |= SourceEventType.NodeStatusChanged;

				if (XML.Attribute(e.Query, "nodeRemoved", true))
					Types |= SourceEventType.NodeRemoved;

				if (XML.Attribute(e.Query, "nodeMovedUp", true))
					Types |= SourceEventType.NodeMovedUp;

				if (XML.Attribute(e.Query, "nodeMovedDown", true))
					Types |= SourceEventType.NodeMovedDown;

				lock (Rec.Subscriptions)
				{
					if (Rec.Subscriptions.TryGetValue(e.From, out SubscriptionRec SubscriptionRec))
					{
						SubscriptionRec.EventTypes &= ~Types;

						if (SubscriptionRec.EventTypes == SourceEventType.None)
						{
							Rec.Subscriptions.Remove(e.From);
							Rec.SubscriptionsStatic = ToArray(Rec.Subscriptions);
						}
					}
				}

				e.IqResult(string.Empty);
			}
		}

		#endregion

	}
}
