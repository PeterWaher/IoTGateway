using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.DisplayableParameters;
using Waher.Networking.XMPP.DataForms;

namespace Waher.Networking.XMPP.Concentrator
{
	/// <summary>
	/// Implements an XMPP concentrator server interface.
	/// 
	/// The interface is defined in XEP-0326:
	/// http://xmpp.org/extensions/xep-0326.html
	/// </summary>
	public class ConcentratorServer : IDisposable
	{
		/// <summary>
		/// urn:xmpp:iot:concentrators
		/// </summary>
		public const string NamespaceConcentrator = "urn:xmpp:iot:concentrators";

		private XmppClient client;
		private Dictionary<string, IDataSource> rootDataSources = new Dictionary<string, IDataSource>();
		private Dictionary<string, IDataSource> dataSources = new Dictionary<string, IDataSource>();
		private object synchObject = new object();

		/// <summary>
		/// Implements an XMPP concentrator server interface.
		/// 
		/// The interface is defined in XEP-0326:
		/// http://xmpp.org/extensions/xep-0326.html
		/// </summary>
		/// <param name="Client">XMPP Client</param>
		/// <param name="DataSources">Data sources.</param>
		public ConcentratorServer(XmppClient Client, params IDataSource[] DataSources)
		{
			this.client = Client;

			foreach (IDataSource DataSource in DataSources)
				this.Register(DataSource);

			this.client.RegisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);

			this.client.RegisterIqGetHandler("getAllDataSources", NamespaceConcentrator, this.GetAllDataSourcesHandler, true);
			this.client.RegisterIqGetHandler("getRootDataSources", NamespaceConcentrator, this.GetRootDataSourcesHandler, true);
			this.client.RegisterIqGetHandler("getChildDataSources", NamespaceConcentrator, this.GetChildDataSourcesHandler, true);

			this.client.RegisterIqGetHandler("containsNode", NamespaceConcentrator, this.ContainsNodeHandler, true);
			this.client.RegisterIqGetHandler("containsNodes", NamespaceConcentrator, this.ContainsNodesHandler, true);
			this.client.RegisterIqGetHandler("getNode", NamespaceConcentrator, this.GetNodeHandler, true);
			this.client.RegisterIqGetHandler("getNodes", NamespaceConcentrator, this.GetNodesHandler, true);
			this.client.RegisterIqGetHandler("getAllNodes", NamespaceConcentrator, this.GetAllNodesHandler, true);
			this.client.RegisterIqGetHandler("getNodeInheritance", NamespaceConcentrator, this.GetNodeInheritanceHandler, true);
			this.client.RegisterIqGetHandler("getRootNodes", NamespaceConcentrator, this.GetRootNodesHandler, true);
			this.client.RegisterIqGetHandler("getChildNodes", NamespaceConcentrator, this.GetChildNodesHandler, true);
			this.client.RegisterIqGetHandler("getAncestors", NamespaceConcentrator, this.GetAncestorsHandler, true);
			this.client.RegisterIqGetHandler("moveNodeUp", NamespaceConcentrator, this.MoveNodeUpHandler, true);
			this.client.RegisterIqGetHandler("moveNodeDown", NamespaceConcentrator, this.MoveNodeDownHandler, true);
			this.client.RegisterIqGetHandler("moveNodesUp", NamespaceConcentrator, this.MoveNodesUpHandler, true);
			this.client.RegisterIqGetHandler("moveNodesDown", NamespaceConcentrator, this.MoveNodesDownHandler, true);

			this.client.RegisterIqGetHandler("getNodeParametersForEdit", NamespaceConcentrator, this.GetNodeParametersForEditHandler, true);
			this.client.RegisterIqGetHandler("setNodeParametersAfterEdit", NamespaceConcentrator, this.SetNodeParametersAfterEditHandler, true);
			this.client.RegisterIqGetHandler("getCommonNodeParametersForEdit", NamespaceConcentrator, this.GetCommonNodeParametersForEditHandler, true);
			this.client.RegisterIqGetHandler("setCommonNodeParametersAfterEdit", NamespaceConcentrator, this.SetCommonNodeParametersAfterEditHandler, true);

			this.client.RegisterIqGetHandler("getAddableNodeTypes", NamespaceConcentrator, this.GetAddableNodeTypesHandler, true);
			this.client.RegisterIqGetHandler("getParametersForNewNode", NamespaceConcentrator, this.GetParametersForNewNodeHandler, true);
			this.client.RegisterIqGetHandler("createNewNode", NamespaceConcentrator, this.CreateNewNodeHandler, true);
			this.client.RegisterIqGetHandler("destroyNode", NamespaceConcentrator, this.DestroyNodeHandler, true);
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.client.UnregisterIqGetHandler("getCapabilities", NamespaceConcentrator, this.GetCapabilitiesHandler, true);

			this.client.UnregisterIqGetHandler("getAllDataSources", NamespaceConcentrator, this.GetAllDataSourcesHandler, true);
			this.client.UnregisterIqGetHandler("getRootDataSources", NamespaceConcentrator, this.GetRootDataSourcesHandler, true);
			this.client.UnregisterIqGetHandler("getChildDataSources", NamespaceConcentrator, this.GetChildDataSourcesHandler, true);

			this.client.UnregisterIqGetHandler("containsNode", NamespaceConcentrator, this.ContainsNodeHandler, true);
			this.client.UnregisterIqGetHandler("containsNodes", NamespaceConcentrator, this.ContainsNodesHandler, true);
			this.client.UnregisterIqGetHandler("getNode", NamespaceConcentrator, this.GetNodeHandler, true);
			this.client.UnregisterIqGetHandler("getNodes", NamespaceConcentrator, this.GetNodesHandler, true);
			this.client.UnregisterIqGetHandler("getAllNodes", NamespaceConcentrator, this.GetAllNodesHandler, true);
			this.client.UnregisterIqGetHandler("getNodeInheritance", NamespaceConcentrator, this.GetNodeInheritanceHandler, true);
			this.client.UnregisterIqGetHandler("getRootNodes", NamespaceConcentrator, this.GetRootNodesHandler, true);
			this.client.UnregisterIqGetHandler("getChildNodes", NamespaceConcentrator, this.GetChildNodesHandler, true);
			this.client.UnregisterIqGetHandler("getAncestors", NamespaceConcentrator, this.GetAncestorsHandler, true);

			this.client.UnregisterIqGetHandler("moveNodeUp", NamespaceConcentrator, this.MoveNodeUpHandler, true);
			this.client.UnregisterIqGetHandler("moveNodeDown", NamespaceConcentrator, this.MoveNodeDownHandler, true);
			this.client.UnregisterIqGetHandler("moveNodesUp", NamespaceConcentrator, this.MoveNodesUpHandler, true);
			this.client.UnregisterIqGetHandler("moveNodesDown", NamespaceConcentrator, this.MoveNodesDownHandler, true);
			this.client.UnregisterIqGetHandler("getNodeParametersForEdit", NamespaceConcentrator, this.GetNodeParametersForEditHandler, true);
			this.client.UnregisterIqGetHandler("setNodeParametersAfterEdit", NamespaceConcentrator, this.SetNodeParametersAfterEditHandler, true);
			this.client.UnregisterIqGetHandler("getCommonNodeParametersForEdit", NamespaceConcentrator, this.GetCommonNodeParametersForEditHandler, true);
			this.client.UnregisterIqGetHandler("setCommonNodeParametersAfterEdit", NamespaceConcentrator, this.SetCommonNodeParametersAfterEditHandler, true);

			this.client.UnregisterIqGetHandler("getAddableNodeTypes", NamespaceConcentrator, this.GetAddableNodeTypesHandler, true);
			this.client.UnregisterIqGetHandler("getParametersForNewNode", NamespaceConcentrator, this.GetParametersForNewNodeHandler, true);
			this.client.UnregisterIqGetHandler("createNewNode", NamespaceConcentrator, this.CreateNewNodeHandler, true);
			this.client.UnregisterIqGetHandler("destroyNode", NamespaceConcentrator, this.DestroyNodeHandler, true);
		}

		/// <summary>
		/// XMPP Client
		/// </summary>
		public XmppClient Client
		{
			get { return this.client; }
		}

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

				//w.WriteElementString("value", "getNodeCommands");
				//w.WriteElementString("value", "getCommandParameters");
				//w.WriteElementString("value", "executeNodeCommand");
				//w.WriteElementString("value", "executeNodeQuery");
				//w.WriteElementString("value", "abortNodeQuery");

				//w.WriteElementString("value", "getCommonNodeCommands");
				//w.WriteElementString("value", "getCommonCommandParameters");
				//w.WriteElementString("value", "executeCommonNodeCommand");
				//w.WriteElementString("value", "executeCommonNodeQuery");
				//w.WriteElementString("value", "abortCommonNodeQuery");

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

			foreach (IDataSource Child in DataSource.ChildSources)
				this.Register(Child, false);

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

			if (E.HasAttribute("deviceToken"))
				DeviceTokens = XML.Attribute(E, "deviceToken").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				DeviceTokens = null;

			if (E.HasAttribute("serviceToken"))
				ServiceTokens = XML.Attribute(E, "serviceToken").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				ServiceTokens = null;

			if (E.HasAttribute("userToken"))
				UserTokens = XML.Attribute(E, "userToken").Split(Space, StringSplitOptions.RemoveEmptyEntries);
			else
				UserTokens = null;

			return new RequestOrigin(From, DeviceTokens, ServiceTokens, UserTokens);
		}

		private static ThingReference GetThingReference(XmlElement E)
		{
			string NodeId = XML.Attribute(E, "nodeId");
			string SourceId = XML.Attribute(E, "sourceId");
			string CacheType = XML.Attribute(E, "cacheType");

			return new ThingReference(NodeId, SourceId, CacheType);
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
			Xml.Append("<dataSource sourceId='");
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

		private static async Task<string> GetErrorMessage(Language Language, int StringId, string Message)
		{
			Namespace Namespace = await Language.GetNamespaceAsync(typeof(ConcentratorServer).Namespace);
			if (Namespace == null)
				Namespace = await Language.CreateNamespaceAsync(typeof(ConcentratorServer).Namespace);

			return await Namespace.GetStringAsync(StringId, Message);
		}

		private async void GetChildDataSourcesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				string SourceId = XML.Attribute(e.Query, "sourceId");
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
					Xml.Append("'>");

					foreach (IDataSource S in Source.ChildSources)
					{
						if (await Source.CanViewAsync(Caller))
							await this.Export(Xml, S, Language);
					}

					Xml.Append("</getChildDataSourcesResponse>");

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

				Xml.Append("<containsNodeResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "node")
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

				Xml.Append("</containsNodeResponse>");

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

			Xml.Append(" nodeId='");
			Xml.Append(XML.Encode(Node.NodeId));

			if (!string.IsNullOrEmpty(s = Node.SourceId))
			{
				Xml.Append("' sourceId='");
				Xml.Append(XML.Encode(s));
			}

			if (!string.IsNullOrEmpty(s = Node.CacheType))
			{
				Xml.Append("' cacheType='");
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

			IThingReference Parent = Node.Parent;
			if (Parent != null)
			{
				Xml.Append("parentId='");
				Xml.Append(XML.Encode(Parent.NodeId));

				if (!string.IsNullOrEmpty(s = Parent.CacheType))
				{
					Xml.Append("' parentCacheType='");
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
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
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

		private async Task ExportParametersAndMessages(StringBuilder Xml, INode Node, bool Parameters, bool Messages, RequestOrigin Caller)
		{
			if (Parameters)
			{
				foreach (Parameter P in await Node.GetDisplayableParametersAsync(Caller))
					P.Export(Xml);
			}

			if (Messages)
			{
				foreach (Message Msg in await Node.GetMessagesAsync(Caller))
					Msg.Export(Xml);
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
					if (E == null || E.LocalName != "node")
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

					Xml.Append("<node");
					await ExportAttributes(Xml, Node, Language);

					if (Parameters || Messages)
					{
						Xml.Append(">");
						await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
						Xml.Append("</node>");
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
				string SourceId = XML.Attribute(e.Query, "sourceId");
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
					Type OnlyIfDerivedFrom = null;

					foreach (XmlNode N in e.Query.ChildNodes)
					{
						if (N.LocalName == "onlyIfDerivedFrom")
						{
							OnlyIfDerivedFrom = Types.GetType(N.InnerText.Trim());
							if (OnlyIfDerivedFrom == null)
							{
								e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 9, "Type not found."), e.IQ));
								return;
							}
						}
					}

					foreach (INode N in Source.RootNodes)
					{
						if (OnlyIfDerivedFrom != null && !OnlyIfDerivedFrom.IsAssignableFrom(N.GetType()))
							continue;

						if (await N.CanViewAsync(Caller))
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
								if (OnlyIfDerivedFrom != null && !OnlyIfDerivedFrom.IsAssignableFrom(N.GetType()))
									continue;

								if (await N.CanViewAsync(Caller))
									Nodes.AddLast(N);
							}
						}

						Xml.Append("<node");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
							Xml.Append("</node>");
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
						T = T.BaseType;

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
				string SourceId = XML.Attribute(e.Query, "sourceId");
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

						Xml.Append("<node");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
							Xml.Append("</node>");
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

					foreach (INode ChildNode in Source.RootNodes)
					{
						if (!await ChildNode.CanViewAsync(Caller))
							continue;

						Xml.Append("<node");
						await ExportAttributes(Xml, ChildNode, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
							Xml.Append("</node>");
						}
						else
							Xml.Append("/>");
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

						Xml.Append("<node");
						await ExportAttributes(Xml, Node, Language);

						if (Parameters || Messages)
						{
							Xml.Append(">");
							await this.ExportParametersAndMessages(Xml, Node, Parameters, Messages, Caller);
							Xml.Append("</node>");
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
				LinkedList<INode> Nodes;
				IThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				string Key;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "node")
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

						Key = ThingRef.SourceId + " \xa0 " + ThingRef.CacheType + " \xa0 " + ThingRef.NodeId;
						if (!NodesPerParent.TryGetValue(Key, out Nodes))
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
				LinkedList<INode> Nodes;
				IThingReference ThingRef;
				IDataSource Source;
				INode Node;
				XmlElement E;
				LinkedListNode<INode> Loop;
				string Key;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null || E.LocalName != "node")
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

						Key = ThingRef.SourceId + " \xa0 " + ThingRef.CacheType + " \xa0 " + ThingRef.NodeId;
						if (!NodesPerParent.TryGetValue(Key, out Nodes))
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
						KeyValuePair<string, string>[] Errors = await Parameters.SetEditableForm(e, Node, Form);

						if (Errors == null)
							e.IqResult("<setNodeParametersAfterEditResponse xmlns='" + NamespaceConcentrator + "'/>");
						else
						{
							Form = await Parameters.GetEditableForm(Sender as XmppClient, e, Node, Node.NodeId);

							StringBuilder Xml = new StringBuilder();

							Xml.Append("<error type='modify'>");
							Xml.Append("<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
							Xml.Append("<setNodeParametersAfterEditResponse xmlns='");
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

							Xml.Append("</setNodeParametersAfterEditResponse>");
							Xml.Append("</error>");

							e.IqError(Xml.ToString());
						}
					}
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
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
					if (E == null || E.LocalName != "node")
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
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 8, "Node not found."), e.IQ));
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
				LinkedList<ThingReference> Nodes = null;
				DataForm Form = null;
				ThingReference ThingRef;
				IDataSource Source;
				INode Node;

				foreach (XmlNode N in e.Query.ChildNodes)
				{
					switch (N.LocalName)
					{
						case "node":
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
								Nodes = new LinkedList<ThingReference>();

							Nodes.AddLast(ThingRef);
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
					KeyValuePair<string, string>[] Errors = await Parameters.SetEditableForm(e, Nodes, Form);

					if (Errors == null)
						e.IqResult("<setCommonNodeParametersAfterEditResponse xmlns='" + NamespaceConcentrator + "'/>");
					else
					{
						StringBuilder Xml = new StringBuilder();

						Xml.Append("<error type='modify'>");
						Xml.Append("<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
						Xml.Append("<setCommonNodeParametersAfterEditResponse xmlns='");
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

						Xml.Append("</setCommonNodeParametersAfterEditResponse>");
						Xml.Append("</error>");

						e.IqError(Xml.ToString());
					}
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
				ConstructorInfo CI;
				INode PresumptiveChild;

				Xml.Append("<getAddableNodeTypesResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				foreach (Type T in Types.GetTypesImplementingInterface(typeof(INode)))
				{
					CI = T.GetConstructor(Types.NoTypes);
					if (CI == null)
						continue;

					try
					{
						PresumptiveChild = (INode)CI.Invoke(Types.NoParameters);

						if (await Node.AcceptsChild(PresumptiveChild) && await PresumptiveChild.AcceptsParent(Node))
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

				ConstructorInfo CI = Type.GetConstructor(Types.NoTypes);

				if (!typeof(INode).IsAssignableFrom(Type) || CI == null)
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
					return;
				}

				INode PresumptiveChild = (INode)CI.Invoke(Types.NoParameters);
				DataForm Form = await Parameters.GetEditableForm(Sender as XmppClient, e, PresumptiveChild,
					await PresumptiveChild.GetTypeNameAsync(Language));

				StringBuilder Xml = new StringBuilder();

				Xml.Append("<getParametersForNewNodeResponse xmlns='");
				Xml.Append(NamespaceConcentrator);
				Xml.Append("'>");

				Form.SerializeForm(Xml);

				Xml.Append("</getParametersForNewNodeResponse>");

				e.IqResult(Xml.ToString());
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

				ConstructorInfo CI = Type.GetConstructor(Types.NoTypes);

				if (!typeof(INode).IsAssignableFrom(Type) || CI == null)
				{
					e.IqError(new StanzaErrors.ItemNotFoundException(await GetErrorMessage(Language, 11, "Invalid type."), e.IQ));
					return;
				}

				INode PresumptiveChild = (INode)CI.Invoke(Types.NoParameters);

				if (!await Node.AcceptsChild(PresumptiveChild) || !await PresumptiveChild.AcceptsParent(Node))
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

				KeyValuePair<string, string>[] Errors = await Parameters.SetEditableForm(e, PresumptiveChild, Form);
				StringBuilder Xml = new StringBuilder();

				if (Errors == null)
				{
					Xml.Append("<createNewNodeResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					await Node.Add(PresumptiveChild);
					await PresumptiveChild.SetParent(Node);

					Xml.Append("<node");
					await ExportAttributes(Xml, Node, Language);
					Xml.Append(">");
					await this.ExportParametersAndMessages(Xml, Node, true, true, Caller);
					Xml.Append("</node>");
					Xml.Append("</createNewNodeResponse>");

					e.IqResult(Xml.ToString());
				}
				else
				{
					Xml.Append("<error type='modify'>");
					Xml.Append("<not-acceptable xmlns='urn:ietf:params:xml:ns:xmpp-stanzas'/>");
					Xml.Append("<createNewNodeResponse xmlns='");
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

					Xml.Append("</createNewNodeResponse>");
					Xml.Append("</error>");

					e.IqError(Xml.ToString());
				}
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
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

				if (Parent != null)
					await Parent.Remove(Node);

				await Node.Destroy();

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



		#endregion

	}
}
