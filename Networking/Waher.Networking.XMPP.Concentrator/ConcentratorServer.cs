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
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Settings;
using Waher.Security;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator server interface.
	/// 
	/// The interface is defined in XEP-0326:
	/// http://xmpp.org/extensions/xep-0326.html
	/// </summary>
	public class ConcentratorServer : XmppExtension
	{
		/// <summary>
		/// urn:xmpp:iot:concentrators
		/// </summary>
		public const string NamespaceConcentrator = "urn:xmpp:iot:concentrators";

		private Dictionary<string, IDataSource> rootDataSources = new Dictionary<string, IDataSource>();
		private Dictionary<string, IDataSource> dataSources = new Dictionary<string, IDataSource>();
		private Dictionary<string, Query> queries = new Dictionary<string, Query>();
		private object synchObject = new object();
		private SensorServer sensorServer = null;
		private ControlServer controlServer = null;
		private ProvisioningClient provisioningClient = null;
		private ThingRegistryClient thingRegistryClient = null;
		private RandomNumberGenerator rnd = RandomNumberGenerator.Create();

		/// <summary>
		/// Implements an XMPP concentrator server interface.
		/// 
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
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
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
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

			if (this.thingRegistryClient != null)
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

			// getIndices
			// getNodesFromIndex
			// getNodesFromIndices
			// getAllIndexValues

			this.client.RegisterIqGetHandler("getNodeParametersForEdit", NamespaceConcentrator, this.GetNodeParametersForEditHandler, false);                   // ConcentratorClient.GetNodeParametersForEdit
			this.client.RegisterIqGetHandler("setNodeParametersAfterEdit", NamespaceConcentrator, this.SetNodeParametersAfterEditHandler, false);               // (ConcentratorClient.EditNode)
			this.client.RegisterIqGetHandler("getCommonNodeParametersForEdit", NamespaceConcentrator, this.GetCommonNodeParametersForEditHandler, false);       // TODO:
			this.client.RegisterIqGetHandler("setCommonNodeParametersAfterEdit", NamespaceConcentrator, this.SetCommonNodeParametersAfterEditHandler, false);   // TODO:

			this.client.RegisterIqGetHandler("getAddableNodeTypes", NamespaceConcentrator, this.GetAddableNodeTypesHandler, false);                             // ConcentratorClient.GetAddableNodeTypes
			this.client.RegisterIqGetHandler("getParametersForNewNode", NamespaceConcentrator, this.GetParametersForNewNodeHandler, false);                     // ConcentratorClient.GetParametersForNewNode
			this.client.RegisterIqGetHandler("createNewNode", NamespaceConcentrator, this.CreateNewNodeHandler, false);                                         // (ConcentratorClient.CreateNewNode, called when submitting form)
			this.client.RegisterIqGetHandler("destroyNode", NamespaceConcentrator, this.DestroyNodeHandler, false);                                             // ConcentratorClient.DestroyNode

			this.client.RegisterIqGetHandler("getAncestors", NamespaceConcentrator, this.GetAncestorsHandler, false);                                           // ConcentratorClient.GetAncestors

			this.client.RegisterIqGetHandler("getNodeCommands", NamespaceConcentrator, this.GetNodeCommandsHandler, false);                                     // TODO:
			this.client.RegisterIqGetHandler("getCommandParameters", NamespaceConcentrator, this.GetCommandParametersHandler, false);                           // TODO:
			this.client.RegisterIqGetHandler("executeNodeCommand", NamespaceConcentrator, this.ExecuteNodeCommandHandler, false);                               // TODO:
			this.client.RegisterIqGetHandler("executeNodeQuery", NamespaceConcentrator, this.ExecuteNodeQueryHandler, false);                                   // TODO:
			this.client.RegisterIqGetHandler("abortNodeQuery", NamespaceConcentrator, this.AbortNodeQueryHandler, false);                                       // TODO:
			this.client.RegisterIqGetHandler("getCommonNodeCommands", NamespaceConcentrator, this.GetCommonNodeCommandsHandler, false);                         // TODO:
			this.client.RegisterIqGetHandler("getCommonCommandParameters", NamespaceConcentrator, this.GetCommonCommandParametersHandler, false);               // TODO:
			this.client.RegisterIqGetHandler("executeCommonNodeCommand", NamespaceConcentrator, this.ExecuteCommonNodeCommandHandler, false);                   // TODO:
			this.client.RegisterIqGetHandler("executeCommonNodeQuery", NamespaceConcentrator, this.ExecuteCommonNodeQueryHandler, false);                       // TODO:

			this.client.RegisterIqGetHandler("moveNodeUp", NamespaceConcentrator, this.MoveNodeUpHandler, false);                                               // TODO:
			this.client.RegisterIqGetHandler("moveNodeDown", NamespaceConcentrator, this.MoveNodeDownHandler, false);                                           // TODO:
			this.client.RegisterIqGetHandler("moveNodesUp", NamespaceConcentrator, this.MoveNodesUpHandler, false);                                             // TODO:
			this.client.RegisterIqGetHandler("moveNodesDown", NamespaceConcentrator, this.MoveNodesDownHandler, false);                                         // TODO:

			// subscribe
			// unsubscribe
			// getDatabases
			// getDatabaseReadoutParameters
			// startDatabaseReadout

			this.client.RegisterIqSetHandler("registerSniffer", NamespaceConcentrator, this.RegisterSnifferHandler, false);
			this.client.RegisterIqSetHandler("unregisterSniffer", NamespaceConcentrator, this.UnregisterSnifferHandler, false);
		}

		private async void ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			try
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
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void ThingRegistryClient_Disowned(object Sender, NodeEventArgs e)
		{
			try
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
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async void ThingRegistryClient_Removed(object Sender, NodeEventArgs e)
		{
			try
			{
				if (!e.Node.IsEmpty)
				{
					IThingReference Ref = await this.OnGetNode(e.Node.NodeId, e.Node.SourceId, e.Node.Partition);
					if (Ref != null && Ref is ILifeCycleManagement LifeCycleManagement)
						await LifeCycleManagement.Removed();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private async Task<IThingReference> OnGetNode(string NodeId, string SourceId, string Partition)
		{
			IDataSource Source;

			lock (this.synchObject)
			{
				if (!this.dataSources.TryGetValue(SourceId, out Source))
					return null;
			}

			return await Source.GetNodeAsync(new ThingReference(NodeId, SourceId, Partition));
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

			// getIndices
			// getNodesFromIndex
			// getNodesFromIndices
			// getAllIndexValues

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

			// subscribe
			// unsubscribe
			// getDatabases
			// getDatabaseReadoutParameters
			// startDatabaseReadout

			this.client.UnregisterIqSetHandler("registerSniffer", NamespaceConcentrator, this.RegisterSnifferHandler, false);
			this.client.UnregisterIqSetHandler("unregisterSniffer", NamespaceConcentrator, this.UnregisterSnifferHandler, false);

			Query[] Queries;

			lock (this.synchObject)
			{
				Queries = new Query[this.queries.Count];
				this.queries.Values.CopyTo(Queries, 0);
				this.queries.Clear();
			}

			foreach (Query Query in Queries)
				Query.Abort();

			if (this.sensorServer != null)
			{
				this.sensorServer.Dispose();
				this.sensorServer = null;
			}

			if (this.controlServer != null)
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

		private void GetCapabilitiesHandler(object Sender, IqEventArgs e)
		{
			StringBuilder Xml = new StringBuilder();
			using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(false, true)))
			{
				w.WriteStartElement("getCapabilitiesResponse", NamespaceConcentrator);

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
				//w.WriteElementString("value", "getIndices");
				//w.WriteElementString("value", "getNodesFromIndex");
				//w.WriteElementString("value", "getNodesFromIndices");
				//w.WriteElementString("value", "getAllIndexValues");
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

				// TODO: Implement events.

				//w.WriteElementString("value", "subscribe");
				//w.WriteElementString("value", "unsubscribe");
				//w.WriteElementString("value", "getDatabases");
				//w.WriteElementString("value", "getDatabaseReadoutParameters");
				//w.WriteElementString("value", "startDatabaseReadout");

				w.WriteEndElement();
				w.Flush();
			}

			e.IqResult(Xml.ToString());
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

				this.dataSources[DataSource.SourceID] = DataSource;

				if (Root)
					this.rootDataSources[DataSource.SourceID] = DataSource;
			}

			IEnumerable<IDataSource> ChildSources = DataSource.ChildSources;
			if (ChildSources != null)
			{
				foreach (IDataSource Child in ChildSources)
					this.Register(Child, false);
			}

			return true;
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
				IDataSource[] Result;

				lock (this.synchObject)
				{
					Result = new IDataSource[this.dataSources.Count];
					this.dataSources.Values.CopyTo(Result, 0);
				}

				return Result;
			}
		}

		internal static Task<Language> GetLanguage(XmlElement E)
		{
			return GetLanguage(E, Translator.DefaultLanguageCode);
		}

		internal static async Task<Language> GetLanguage(XmlElement E, string DefaultLanguageCode)
		{
			string LanguageCode = XML.Attribute(E, "xml:lang");
			bool Default = LanguageCode == DefaultLanguageCode;

			if (string.IsNullOrEmpty(LanguageCode))
			{
				LanguageCode = DefaultLanguageCode;
				Default = true;
			}

			Language Language = await Translator.GetLanguageAsync(LanguageCode);
			if (Language == null)
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

		private async void GetAllDataSourcesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<getAllDataSourcesResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (IDataSource Source in this.DataSources)
				{
					if (await Source.CanViewAsync(Caller))
						await this.Export(Xml, Source, Language);
				}

				Xml.Append("</getAllDataSourcesResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
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

		private async void GetRootDataSourcesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<getRootDataSourcesResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (IDataSource Source in this.RootDataSources)
				{
					if (await Source.CanViewAsync(Caller))
						await this.Export(Xml, Source, Language);
				}

				Xml.Append("</getRootDataSourcesResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private static Task<string> GetErrorMessage(Language Language, int StringId, string Message)
		{
			return Language.GetStringAsync(typeof(ConcentratorServer), StringId, Message);
		}

		private async void GetChildDataSourcesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				string SourceId = XML.Attribute(e.Query, "src");
				IDataSource Source;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(SourceId, out Source))
						Source = null;
				}

				if (Source == null || !await Source.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getChildDataSourcesResponse xmlns='");
					Xml.Append(NamespaceConcentrator);

					IEnumerable<IDataSource> ChildSources = Source.ChildSources;
					if (ChildSources != null)
					{
						Xml.Append("'>");

						foreach (IDataSource S in ChildSources)
						{
							if (await Source.CanViewAsync(Caller))
								await this.Export(Xml, S, Language);
						}

						Xml.Append("</getChildDataSourcesResponse>");
					}
					else
						Xml.Append("'/>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

		#region Nodes

		private async void ContainsNodeHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				bool Result = (Node != null && await Node.CanViewAsync(Caller));

				e.IqResult("<containsNodeResponse xmlns='" + NamespaceConcentrator + "'>" + CommonTypes.Encode(Result) + "</containsNodeResponse>");
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void ContainsNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				StringBuilder Xml = new StringBuilder();
				ThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				bool Result;

				Xml.Append("<containsNodesResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					Result = (Node != null && await Node.CanViewAsync(Caller));

					Xml.Append("<value>");
					Xml.Append(CommonTypes.Encode(Result));
					Xml.Append("</value>");
				}

				Xml.Append("</containsNodesResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
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

			if (Node is ISniffable Sniffable)
				Xml.Append("' sniffable='true");

			IThingReference Parent = Node.Parent;
			if (Parent != null)
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

		private async void GetNodeHandler(object Sender, IqEventArgs e)
		{
			try
			{
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node != null && await Node.CanViewAsync(Caller))
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getNodeResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'");

					await ExportAttributes(Xml, Node, Language);

					if (Parameters || Messages)
					{
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
						Xml.Append("</getNodeResponse>");
					}
					else
						Xml.Append("/>");

					e.IqResult(Xml.ToString());
				}
				else
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async Task ExportParametersAndMessages(StringBuilder Xml, INode Node, bool Parameters, bool Messages,
			Language Language, RequestOrigin Caller)
		{
			if (Parameters)
			{
				IEnumerable<Parameter> Parameters2 = await Node.GetDisplayableParametersAsync(Language, Caller);

				if (Parameters2 != null)
				{
					foreach (Parameter P in Parameters2)
						P.Export(Xml);
				}
			}

			if (Messages)
			{
				IEnumerable<Message> Messages2 = await Node.GetMessagesAsync(Caller);

				if (Messages2 != null)
				{
					foreach (Message Msg in Messages2)
						Msg.Export(Xml);
				}
			}
		}

		private async void GetNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				Language Language = await GetLanguage(e.Query);
				StringBuilder Xml = new StringBuilder();
				IDataSource Source;
				ThingReference ThingRef;
				INode Node;
				XmlElement E;

				Xml.Append("<getNodesResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					if (Node == null || !(await Node.CanViewAsync(Caller)))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					Xml.Append("<nd");
					await ExportAttributes(Xml, Node, Language);

					if (Parameters || Messages)
					{
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
						Xml.Append("</nd>");
					}
					else
						Xml.Append("/>");
				}

				Xml.Append("</getNodesResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetAllNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				string SourceId = XML.Attribute(e.Query, "src");
				IDataSource Source;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(SourceId, out Source))
						Source = null;
				}

				if (Source == null || !await Source.CanViewAsync(Caller))
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
							if (T == null)
							{
								e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 9, "Type not found."), e.IQ));
								return;
							}

							if (OnlyIfDerivedFrom == null)
								OnlyIfDerivedFrom = new LinkedList<TypeInfo>();

							OnlyIfDerivedFrom.AddLast(T.GetTypeInfo());
						}
					}

					foreach (INode N in Source.RootNodes)
					{
						if ((OnlyIfDerivedFrom == null || this.IsAssignableFrom(OnlyIfDerivedFrom, N)) && await N.CanViewAsync(Caller))
							Nodes.AddLast(N);
					}

					Xml.Append("<getAllNodesResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					while (Nodes.First != null)
					{
						Node = Nodes.First.Value;
						Nodes.RemoveFirst();

						if (Node.HasChildren)
						{
							foreach (INode N in await Node.ChildNodes)
							{
								if ((OnlyIfDerivedFrom == null || this.IsAssignableFrom(OnlyIfDerivedFrom, N)) && await N.CanViewAsync(Caller))
									Nodes.AddLast(N);
							}
						}

						Xml.Append("<nd");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
							Xml.Append("</nd>");
						}
						else
							Xml.Append("/>");
					}

					Xml.Append("</getAllNodesResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private bool IsAssignableFrom(IEnumerable<TypeInfo> TypeList, INode Node)
		{
			if (TypeList == null)
				return true;

			TypeInfo NodeType = Node.GetType().GetTypeInfo();

			foreach (TypeInfo T in TypeList)
			{
				if (T.IsAssignableFrom(NodeType))
					return true;
			}

			return false;
		}

		private async void GetNodeInheritanceHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node != null && await Node.CanViewAsync(Caller))
				{
					StringBuilder Xml = new StringBuilder();
					Type T = Node.GetType();

					Xml.Append("<getNodeInheritanceResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'><baseClasses>");

					do
					{
						T = T.GetTypeInfo().BaseType;

						Xml.Append("<value>");
						Xml.Append(XML.Encode(T.FullName));
						Xml.Append("</value>");
					}
					while (T != typeof(object));

					Xml.Append("</baseClasses></getNodeInheritanceResponse>");

					e.IqResult(Xml.ToString());
				}
				else
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetRootNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				string SourceId = XML.Attribute(e.Query, "src");
				IDataSource Source;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(SourceId, out Source))
						Source = null;
				}

				if (Source == null || !await Source.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getRootNodesResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					foreach (INode Node in Source.RootNodes)
					{
						if (!await Node.CanViewAsync(Caller))
							continue;

						Xml.Append("<nd");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
							Xml.Append("</nd>");
						}
						else
							Xml.Append("/>");
					}

					Xml.Append("</getRootNodesResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetChildNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getChildNodesResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					if (Node.HasChildren)
					{
						foreach (INode ChildNode in await Node.ChildNodes)
						{
							if (!await ChildNode.CanViewAsync(Caller))
								continue;

							Xml.Append("<nd");
							await ExportAttributes(Xml, ChildNode, Language);

							if (Parameters || Messages)
							{
								Xml.Append(">");
								await this.ExportParametersAndMessages(Xml, ChildNode, Parameters, Messages, Language, Caller);
								Xml.Append("</nd>");
							}
							else
								Xml.Append("/>");
						}
					}

					Xml.Append("</getChildNodesResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetAncestorsHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
				bool Messages = XML.Attribute(e.Query, "messages", false);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				IThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getAncestorsResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					while (Node != null)
					{
						if (!await Node.CanViewAsync(Caller))
							break;

						Xml.Append("<nd");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Language, Caller);
							Xml.Append("</nd>");
						}
						else
							Xml.Append("/>");

						ThingRef = Node.Parent;
						Node = ThingRef as INode;
						if (Node == null)
							Node = await Source.GetNodeAsync(Node.Parent);
					}

					Xml.Append("</getAncestorsResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

		#region Editing

		private async void MoveNodeUpHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				IThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else if (!await Node.CanEditAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else
				{
					await Node.MoveUpAsync(Caller);

					StringBuilder Xml = new StringBuilder();

					Xml.Append("<moveNodeUpResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'/>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void MoveNodeDownHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				IThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else if (!await Node.CanEditAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else
				{
					await Node.MoveDownAsync(Caller);

					StringBuilder Xml = new StringBuilder();

					Xml.Append("<moveNodeDownResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'/>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void MoveNodesUpHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				LinkedList<INode> RootNodes = null;
				Dictionary<string, LinkedList<INode>> NodesPerParent = null;
				IThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				string Key;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					if (Node == null || !await Node.CanViewAsync(Caller))
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
					if (ThingRef == null)
					{
						if (RootNodes == null)
							RootNodes = new LinkedList<INode>();

						RootNodes.AddLast(Node);
					}
					else
					{
						if (NodesPerParent == null)
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

				if (RootNodes != null)
				{
					foreach (INode Node2 in RootNodes)
					{
						if (!await Node2.MoveUpAsync(Caller))
							break;
					}
				}

				if (NodesPerParent != null)
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

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<moveNodesUpResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'/>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void MoveNodesDownHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				LinkedList<INode> RootNodes = null;
				Dictionary<string, LinkedList<INode>> NodesPerParent = null;
				IThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				LinkedListNode<INode> Loop;
				string Key;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					if (Node == null || !await Node.CanViewAsync(Caller))
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
					if (ThingRef == null)
					{
						if (RootNodes == null)
							RootNodes = new LinkedList<INode>();

						RootNodes.AddLast(Node);
					}
					else
					{
						if (NodesPerParent == null)
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

				if (RootNodes != null)
				{
					Loop = RootNodes.Last;
					while (Loop != null)
					{
						if (!await Loop.Value.MoveDownAsync(Caller))
							break;

						Loop = Loop.Previous;
					}
				}

				if (NodesPerParent != null)
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

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<moveNodesDownResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'/>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetNodeParametersForEditHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else if (!await Node.CanEditAsync(Caller))
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
				else
				{
					DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getNodeParametersForEditResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					Form.SerializeForm(Xml);

					Xml.Append("</getNodeParametersForEditResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void SetNodeParametersAfterEditHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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

					if (Form == null)
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
								await Source.GetNodeAsync(new ThingReference(NewNodeId, NewSourceId, NewPartition)) != null)
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

						if (Result == null)
							Result = await Parameters.SetEditableForm(e, Node, Form, true);

						if (Result.Errors == null)
						{
							StringBuilder Xml = new StringBuilder();

							Xml.Append("<setNodeParametersAfterEditResponse xmlns='");
							Xml.Append(NamespaceConcentrator);
							Xml.Append("'>");

							await Node.UpdateAsync();

							Xml.Append("<nd");
							await ExportAttributes(Xml, Node, Language);
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, true, true, Language, Caller);
							Xml.Append("</nd>");
							Xml.Append("</setNodeParametersAfterEditResponse>");

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
							e.IqError(this.GetFormErrorsXml(Result.Errors, "setNodeParametersAfterEditResponse"));
						}
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private string GetFormErrorsXml(KeyValuePair<string, string>[] Errors, string ResponseTag)
		{
			StringBuilder Xml = new StringBuilder();

			Xml.Append("<error type='modify'>");
			Xml.Append("<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/><");
			Xml.Append(ResponseTag);
			Xml.Append(" xmlns='");
			Xml.Append(NamespaceConcentrator);
			Xml.Append("'>");

			foreach (KeyValuePair<string, string> Error in Errors)
			{
				Xml.Append("<error var='");
				Xml.Append(XML.Encode(Error.Key));
				Xml.Append("'>");
				Xml.Append(XML.Encode(Error.Value));
				Xml.Append("</error>");
			}

			Xml.Append("</");
			Xml.Append(ResponseTag);
			Xml.Append(">");
			Xml.Append("</error>");

			return Xml.ToString();
		}

		private async void GetCommonNodeParametersForEditHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				DataForm Form = null;
				DataForm Form2;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 7, "Source not found."), e.IQ));
						return;
					}

					Node = await Source.GetNodeAsync(ThingRef);
					if (Node == null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}
					else if (!await Node.CanEditAsync(Caller))
					{
						e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
						return;
					}

					if (Form == null)
						Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
					else
					{
						Form2 = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);
						Parameters.MergeForms(Form, Form2);
					}
				}

				if (Form == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
					return;
				}

				Form.RemoveExcluded();

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<getCommonNodeParametersForEditResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				Form.SerializeForm(Xml);

				Xml.Append("</getCommonNodeParametersForEditResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void SetCommonNodeParametersAfterEditHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				LinkedList<KeyValuePair<IDataSource, ThingReference>> Nodes = null;
				DataForm Form = null;
				ThingReference ThingRef;
				IDataSource Source;
				INode Node;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "nd":
							ThingRef = GetThingReference((XmlElement)N);

							lock (this.synchObject)
							{
								if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
									Source = null;
							}

							if (Source == null)
								Node = null;
							else
								Node = await Source.GetNodeAsync(ThingRef);

							if (Node == null || !await Node.CanViewAsync(Caller))
							{
								e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
								return;
							}
							else if (!await Node.CanEditAsync(Caller))
							{
								e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
								return;
							}

							if (Nodes == null)
								Nodes = new LinkedList<KeyValuePair<IDataSource, ThingReference>>();

							Nodes.AddLast(new KeyValuePair<IDataSource, ThingReference>(Source, ThingRef));
							break;

						case "x":
							Form = new DataForm(Sender as XmppClient, (XmlElement)N, null, null, e.From, e.To);
							break;
					}
				}

				if (Nodes == null)
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else if (Form == null)
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 10, "Data form missing."), e.IQ));
				else
				{
					foreach (KeyValuePair<IDataSource, ThingReference> P in Nodes)
					{
						string OldNodeId = P.Value.NodeId;
						string OldSourceId = P.Value.SourceId;
						string OldPartition = P.Value.Partition;
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
								await P.Key.GetNodeAsync(new ThingReference(NewNodeId, NewSourceId, NewPartition)) != null)
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

						ILifeCycleManagement LifeCycleManagement = P.Value as ILifeCycleManagement;
						bool PreProvisioned = LifeCycleManagement != null && LifeCycleManagement.IsProvisioned;

						if (Result == null)
							Result = await Parameters.SetEditableForm(e, P.Value, Form, true);

						if (Result.Errors != null)
						{
							e.IqError(this.GetFormErrorsXml(Result.Errors, "setCommonNodeParametersAfterEditResponse"));
							break;
						}

						Result.Tags.Add(new KeyValuePair<string, object>("Full", e.From));
						Result.Tags.Add(new KeyValuePair<string, object>("Source", P.Value.SourceId));
						Result.Tags.Add(new KeyValuePair<string, object>("Partition", P.Value.Partition));

						Log.Informational("Node edited.", P.Value.NodeId, e.FromBareJid, "NodeEdited", EventLevel.Medium, Result.Tags.ToArray());

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

					e.IqResult("<setCommonNodeParametersAfterEditResponse xmlns='" + NamespaceConcentrator + "'/>");
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

		#region Creating & Destroying nodes

		private async void GetAddableNodeTypesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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

				Xml.Append("<getAddableNodeTypesResponse xmlns='");
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

				Xml.Append("</getAddableNodeTypesResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetParametersForNewNodeHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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
				if (Type == null)
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

				Xml.Append("<getParametersForNewNodeResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				Form.SerializeForm(Xml);

				Xml.Append("</getParametersForNewNodeResponse>");

				string s = Xml.ToString();
				e.IqResult(s);
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void CreateNewNodeHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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
				if (Type == null)
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

				if (Form == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 12, "Missing form."), e.IQ));
					return;
				}

				Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, PresumptiveChild, Form, false);

				if (Result.Errors == null)
				{
					if (await Source.GetNodeAsync(PresumptiveChild) != null)
					{
						Result.Errors = new KeyValuePair<string, string>[]
						{
							new KeyValuePair<string, string>("NodeId", "Identity already exists.")
						};
					}
				}

				if (Result.Errors == null)
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<createNewNodeResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					await Node.AddAsync(PresumptiveChild);

					Xml.Append("<nd");
					await ExportAttributes(Xml, PresumptiveChild, Language);
					Xml.Append(">");
					await this.ExportParametersAndMessages(Xml, PresumptiveChild, true, true, Language, Caller);
					Xml.Append("</nd>");
					Xml.Append("</createNewNodeResponse>");

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
					e.IqError(this.GetFormErrorsXml(Result.Errors, "createNewNodeResponse"));
			}
			catch (Exception ex)
			{
				e.IqError(ex);
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
			}, null);
		}

		private async Task<MetaDataTag[]> GetTags(INode Node, KeyValuePair<string, object>[] MetaData, bool IncludeKey)
		{
			List<MetaDataTag> Result = new List<MetaDataTag>();
			object Value;

			foreach (KeyValuePair<string, object> P in MetaData)
			{
				Value = P.Value;
				if (Value == null)
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

		private async void DestroyNodeHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
					return;
				}
				else if (!await Node.CanDestroyAsync(Caller))
				{
					e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
					return;
				}

				IThingReference ParentRef = Node.Parent;
				INode Parent = await Source.GetNodeAsync(Node.Parent);
				KeyValuePair<string, object>[] Tags = new KeyValuePair<string, object>[]
				{
					new KeyValuePair<string, object>("Full", e.From),
					new KeyValuePair<string, object>("Source", Node.SourceId),
					new KeyValuePair<string, object>("Partition", Node.Partition)
				};

				if (Node is ILifeCycleManagement LifeCycleManagement)
					this.UnregisterNode(LifeCycleManagement);

				if (Parent != null)
					await Parent.RemoveAsync(Node);

				await Node.DestroyAsync();

				Log.Informational("Node destroyed.", Node.NodeId, e.FromBareJid, "NodeDestroyed", EventLevel.Major, Tags);

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<destroyNodeResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'/>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

		#region Commands

		private async void GetNodeCommandsHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getNodeCommandsResponse xmlns='");
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

					Xml.Append("</getNodeCommandsResponse>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
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

		private async void GetCommandParametersHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					string CommandId = XML.Attribute(e.Query, "command");
					ICommand Command = await FindCommand(CommandId, Node);

					if (Command == null)
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 14, "Command not found."), e.IQ));
					else if (!await Command.CanExecuteAsync(Caller))
						e.IqError(new StanzaErrors.ForbiddenException(await GetErrorMessage(Language, 13, "Not sufficient privileges."), e.IQ));
					else
					{
						DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Command, await Command.GetNameAsync(Language));
						StringBuilder Xml = new StringBuilder();

						Xml.Append("<getCommandParametersResponse xmlns='");
						Xml.Append(NamespaceConcentrator);
						Xml.Append("'>");

						Form.SerializeForm(Xml);

						Xml.Append("</getCommandParametersResponse>");

						e.IqResult(Xml.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private static void DisposeObject(object Object)
		{
			if (Object is IDisposable Disposable)
				Disposable.Dispose();
		}

		private async void ExecuteNodeCommandHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					string CommandId = XML.Attribute(e.Query, "command");
					ICommand Command = await FindCommand(CommandId, Node);

					if (Command == null)
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

						if (Form == null)
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

							Command = (ICommand)Activator.CreateInstance(Command.GetType());

							Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

							if (Result.Errors != null)
							{
								DisposeObject(Command);
								e.IqError(this.GetFormErrorsXml(Result.Errors, "executeNodeCommandResponse"));
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

						StringBuilder Xml = new StringBuilder();

						Xml.Append("<executeNodeCommandResponse xmlns='");
						Xml.Append(NamespaceConcentrator);
						Xml.Append("'/>");

						e.IqResult(Xml.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void ExecuteNodeQueryHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					string CommandId = XML.Attribute(e.Query, "command");
					string QueryId = XML.Attribute(e.Query, "queryId");
					ICommand Command = await FindCommand(CommandId, Node);

					if (Command == null)
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

						if (Form == null)
						{
							e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
							return;
						}

						Command = (ICommand)Activator.CreateInstance(Command.GetType());

						Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

						if (Result.Errors != null)
						{
							DisposeObject(Command);
							e.IqError(this.GetFormErrorsXml(Result.Errors, "executeNodeQueryResponse"));
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

						if (Query == null)
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

						try
						{
							await Command.StartQueryExecutionAsync(Query);
						}
						catch (Exception)
						{
							lock (this.synchObject)
							{
								this.queries.Remove(QueryId);
							}

							Query.Abort();
							throw;
						}
						finally
						{
							DisposeObject(Command);
						}

						StringBuilder Xml = new StringBuilder();

						Xml.Append("<executeNodeQueryResponse xmlns='");
						Xml.Append(NamespaceConcentrator);
						Xml.Append("'/>");

						e.IqResult(Xml.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void AbortNodeQueryHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
				else
				{
					string CommandId = XML.Attribute(e.Query, "command");
					string QueryId = XML.Attribute(e.Query, "queryId");
					ICommand Command = await FindCommand(CommandId, Node);

					if (Command == null)
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

						if (Query == null)
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 19, "Query not found."), e.IQ));
							return;
						}

						Query.Abort();

						StringBuilder Xml = new StringBuilder();

						Xml.Append("<abortNodeQueryResponse xmlns='");
						Xml.Append(NamespaceConcentrator);
						Xml.Append("'/>");

						e.IqResult(Xml.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private void Query_OnTitle(object Sender, QueryTitleEventArgs e)
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
			Xml.Append("'>");
		}

		private void EndQueryProgress(StringBuilder Xml)
		{
			Xml.Append("</queryProgress>");
		}

		private void Query_OnTableDone(object Sender, QueryTableEventArgs e)
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
		}

		private void Query_OnStatus(object Sender, QueryStatusEventArgs e)
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
		}

		private void Query_OnStarted(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryStarted/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void Query_OnNewTable(object Sender, QueryNewTableEventArgs e)
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

				Xml.Append("/>");
			}

			Xml.Append("</newTable>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void Query_OnNewRecords(object Sender, QueryNewRecordsEventArgs e)
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
					if (Element == null)
					{
						Xml.Append("<void/>");
					}
					else if (Element is bool)
					{
						Xml.Append("<boolean>");
						Xml.Append(CommonTypes.Encode((bool)Element));
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
					else if (Element is double)
					{
						Xml.Append("<double>");
						Xml.Append(CommonTypes.Encode((double)Element));
						Xml.Append("</double>");
					}
					else if (Element is Duration)
					{
						Xml.Append("<duration>");
						Xml.Append(Element.ToString());
						Xml.Append("</duration>");
					}
					else if (Element is int)
					{
						Xml.Append("<int>");
						Xml.Append(Element.ToString());
						Xml.Append("</int>");
					}
					else if (Element is long)
					{
						Xml.Append("<long>");
						Xml.Append(Element.ToString());
						Xml.Append("</long>");
					}
					else if (Element is string)
					{
						Xml.Append("<string>");
						Xml.Append(Element.ToString());
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

			Xml.Append("</newTable>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void Query_OnNewObject(object Sender, QueryObjectEventArgs e)
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
		}

		private void Query_OnMessage(object Sender, QueryMessageEventArgs e)
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
		}

		private void Query_OnEndSection(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<endSection/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void Query_OnDone(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryDone/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
		}

		private void Query_OnBeginSection(object Sender, QueryTitleEventArgs e)
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
		}

		private void Query_OnAborted(object Sender, QueryEventArgs e)
		{
			object[] P = (object[])e.Query.State;
			XmppClient Client = (XmppClient)P[0];
			IqEventArgs e0 = (IqEventArgs)P[1];
			StringBuilder Xml = new StringBuilder();

			this.StartQueryProgress(Xml, e);
			Xml.Append("<queryAborted/>");
			this.EndQueryProgress(Xml);

			Client.SendMessage(MessageType.Normal, e0.From, Xml.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
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

		private async void GetCommonNodeCommandsHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Dictionary<ICommand, bool> CommonCommands = null;
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				Language Language = await GetLanguage(e.Query);
				ThingReference ThingRef;
				IDataSource Source;
				XmlElement E;
				INode Node;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || N.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					if (Node == null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					if (!Node.HasCommands)
					{
						if (CommonCommands != null)
							CommonCommands.Clear();

						break;
					}

					if (CommonCommands == null)
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

				Xml.Append("<getNodeCommandsResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				if (CommonCommands != null)
				{
					foreach (ICommand Command in CommonCommands.Keys)
						await ExportXml(Xml, Command, Language);
				}

				Xml.Append("</getNodeCommandsResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetCommonCommandParametersHandler(object Sender, IqEventArgs e)
		{
			try
			{
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef;
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
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
					if (E == null || N.LocalName != "nd")
						continue;

					ThingRef = GetThingReference(E);

					lock (this.synchObject)
					{
						if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
							Source = null;
					}

					if (Source == null)
						Node = null;
					else
						Node = await Source.GetNodeAsync(ThingRef);

					if (Node == null || !await Node.CanViewAsync(Caller))
					{
						e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
						return;
					}

					if (Command == null)
					{
						Command = await FindCommand(CommandId, Node);

						if (Command == null)
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
						if (Command2 == null || !commandComparerInstance.Equals(Command, Command2))
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

				if (Form == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
					return;
				}

				Form.RemoveExcluded();

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<getCommonCommandParametersResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				Form.SerializeForm(Xml);

				Xml.Append("</getCommonCommandParametersResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void ExecuteCommonNodeCommandHandler(object Sender, IqEventArgs e)
		{
			try
			{
				LinkedList<INode> Nodes = null;
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef;
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
				XmlElement E;
				INode Node;
				ICommand Command = null;
				ICommand Command2;
				DataForm Form = null;
				string CommandId = XML.Attribute(e.Query, "command");

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null)
						continue;

					if (E.LocalName == "nd")
					{
						ThingRef = GetThingReference(E);

						lock (this.synchObject)
						{
							if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
								Source = null;
						}

						if (Source == null)
							Node = null;
						else
							Node = await Source.GetNodeAsync(ThingRef);

						if (Node == null || !await Node.CanViewAsync(Caller))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
							return;
						}

						if (Nodes == null)
							Nodes = new LinkedList<INode>();

						Nodes.AddLast(Node);

						if (Command == null)
						{
							Command = await FindCommand(CommandId, Node);

							if (Command == null)
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
							if (Command2 == null || !commandComparerInstance.Equals(Command, Command2))
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

				if (Nodes == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
					return;
				}

				if (Form == null)
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

					Command = (ICommand)Activator.CreateInstance(Command.GetType());

					Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

					if (Result.Errors != null)
					{
						DisposeObject(Command);
						e.IqError(this.GetFormErrorsXml(Result.Errors, "executeCommonNodeCommandResponse"));
						return;
					}
				}

				StringBuilder Xml = new StringBuilder();
				string ErrorMessage;

				Xml.Append("<executeCommonNodeCommandResponse xmlns='");
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

					if (ErrorMessage == null)
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

				Xml.Append("</executeCommonNodeCommandResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void ExecuteCommonNodeQueryHandler(object Sender, IqEventArgs e)
		{
			try
			{
				LinkedList<INode> Nodes = null;
				LinkedList<Query> Queries = null;
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef;
				Language Language = await GetLanguage(e.Query);
				IDataSource Source;
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
					if (E == null)
						continue;

					if (E.LocalName == "nd")
					{
						ThingRef = GetThingReference(E);

						lock (this.synchObject)
						{
							if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
								Source = null;
						}

						if (Source == null)
							Node = null;
						else
							Node = await Source.GetNodeAsync(ThingRef);

						if (Node == null || !await Node.CanViewAsync(Caller))
						{
							e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
							return;
						}

						if (Nodes == null)
						{
							Nodes = new LinkedList<INode>();
							Queries = new LinkedList<Query>();
						}

						Nodes.AddLast(Node);
						Queries.AddLast(new Query(CommandId, QueryId, new object[] { Sender, e }, Language, Node));

						if (Command == null)
						{
							Command = await FindCommand(CommandId, Node);

							if (Command == null)
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
							if (Command2 == null || !commandComparerInstance.Equals(Command, Command2))
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

				if (Nodes == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 20, "No nodes specified."), e.IQ));
					return;
				}

				if (Command.Type != CommandType.Query)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 17, "Query command expected."), e.IQ));
					return;
				}

				if (Form == null)
				{
					e.IqError(new StanzaErrors.BadRequestException(await GetErrorMessage(Language, 15, "Form expected."), e.IQ));
					return;
				}

				Command = (ICommand)Activator.CreateInstance(Command.GetType());

				Parameters.SetEditableFormResult Result = await Parameters.SetEditableForm(e, Command, Form, false);

				if (Result.Errors != null)
				{
					DisposeObject(Command);
					e.IqError(this.GetFormErrorsXml(Result.Errors, "executeCommonNodeQueryResponse"));
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

				if (Query == null)
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

				Xml.Append("<executeCommonNodeQueryResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (INode N in Nodes)
				{
					try
					{
						await Command.StartQueryExecutionAsync(Query);
						ErrorMessage = null;
					}
					catch (Exception ex)
					{
						ErrorMessage = ex.Message;
					}

					if (ErrorMessage == null)
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

					Query.Abort();
				}

				DisposeObject(Command);

				Xml.Append("</executeCommonNodeQueryResponse>");

				e.IqResult(Xml.ToString());
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

		#region Sensor Data interface

		private async void SensorServer_OnExecuteReadoutRequest(object Sender, SensorDataServerRequest Request)
		{
			DateTime Now = DateTime.Now;
			IThingReference[] Nodes = Request.Nodes;
			int i, c;

			try
			{
				if (Nodes == null || (c = Nodes.Length) == 0)
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
							if (Node == null)
							{
								Request.ReportErrors(i == c - 1, new ThingError(NodeRef, Now, "Node not found."));
								continue;
							}
						}

						ISensor Sensor = Node as ISensor;
						if (Sensor == null || !Sensor.IsReadable)
						{
							Request.ReportErrors(i == c - 1, new ThingError(NodeRef, Now, "Node not readable."));
							continue;
						}

						Sensor.StartReadout(Request);
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
			DateTime Now = DateTime.Now;

			try
			{
				if (!(Node is INode Node2))
				{
					if (!this.TryGetDataSource(Node.SourceId, out IDataSource DataSource))
						return null;

					Node2 = await DataSource.GetNodeAsync(Node);
					if (Node2 == null)
						return null;
				}

				IActuator Actuator = Node2 as IActuator;
				if (Actuator == null)
					return null;

				if (!Actuator.IsControllable)
					return new ControlParameter[0];

				return Actuator.GetControlParameters();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return new ControlParameter[0];
			}
		}

		#endregion

		#region Sniffers

		private async void RegisterSnifferHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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

					Xml.Append("<registerSniffer xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("' snifferId='");
					Xml.Append(Sniffer.Id);
					Xml.Append("' expires='");
					Xml.Append(XML.Encode(Expires));
					Xml.Append("'/>");

					e.IqResult(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void UnregisterSnifferHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				ThingReference ThingRef = GetThingReference(e.Query);
				IDataSource Source;
				INode Node;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(ThingRef.SourceId, out Source))
						Source = null;
				}

				if (Source == null)
					Node = null;
				else
					Node = await Source.GetNodeAsync(ThingRef);

				if (Node == null || !await Node.CanViewAsync(Caller))
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
							}
						}
					}

					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 22, "Sniffer not found."), e.IQ));
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		#endregion

	}
}
