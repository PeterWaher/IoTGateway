using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Things;
using Waher.Networking.XMPP.Concentrator.Parameters;

namespace Waher.Networking.XMPP.Concentrator
{
	public enum ConcentratorResponseCode
	{
		OK,
		NotFound,
		InsufficientPrivileges,
		Locked,
		NotImplemented,
		FormError,
		OtherError
	}

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
				//w.WriteElementString("value", "getAllNodes");
				//w.WriteElementString("value", "getNodeInheritance");
				//w.WriteElementString("value", "getRootNodes");
				//w.WriteElementString("value", "getChildNodes");
				//w.WriteElementString("value", "getIndices");
				//w.WriteElementString("value", "getNodesFromIndex");
				//w.WriteElementString("value", "getNodesFromIndices");
				//w.WriteElementString("value", "getAllIndexValues");
				//w.WriteElementString("value", "getNodeParametersForEdit");
				//w.WriteElementString("value", "setNodeParametersAfterEdit");
				//w.WriteElementString("value", "getCommonNodeParametersForEdit");
				//w.WriteElementString("value", "setCommonNodeParametersAfterEdit");
				//w.WriteElementString("value", "getAddableNodeTypes");
				//w.WriteElementString("value", "getParametersForNewNode");
				//w.WriteElementString("value", "createNewNode");
				//w.WriteElementString("value", "destroyNode");
				//w.WriteElementString("value", "getAncestors");
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
				//w.WriteElementString("value", "moveNodeUp");
				//w.WriteElementString("value", "moveNodeDown");
				//w.WriteElementString("value", "moveNodesUp");
				//w.WriteElementString("value", "moveNodesDown");
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

		private static async Task<Language> GetLanguage(XmlElement E)
		{
			string LanguageCode = XML.Attribute(E, "xml:lang");
			bool Default = LanguageCode == Translator.DefaultLanguageCode;

			if (string.IsNullOrEmpty(LanguageCode))
			{
				LanguageCode = Translator.DefaultLanguageCode;
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

		private async void GetChildDataSourcesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				Language Language = await GetLanguage(e.Query);
				RequestOrigin Caller = GetTokens(e.FromBareJid, e.Query);
				string SourceId = XML.Attribute(e.Query, "MeteringRoot");
				IDataSource Source;

				lock (this.synchObject)
				{
					if (!this.dataSources.TryGetValue(SourceId, out Source))
						Source = null;
				}

				if (Source == null || !await Source.CanViewAsync(Caller))
					e.IqError(new StanzaErrors.ItemNotFoundException("Source not found.", e.IQ));
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
			Xml.Append(XML.Encode(await Node.GetNameAsync(Language)));
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

					if (Parameters)
					{
						Xml.Append(">");

						foreach (Parameter P in await Node.GetParametersAsync(Caller))
							P.Export(Xml);

						Xml.Append("</getNodeResponse>");
					}
					else
						Xml.Append("/>");

					e.IqResult(Xml.ToString());
				}
				else
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
			}
			catch (Exception ex)
			{
				e.IqError(ex);
			}
		}

		private async void GetNodesHandler(object Sender, IqEventArgs e)
		{
			try
			{
				bool Parameters = XML.Attribute(e.Query, "parameters", false);
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
						e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
						return;
					}

					Xml.Append("<node");
					await ExportAttributes(Xml, Node, Language);

					if (Parameters)
					{
						Xml.Append(">");

						foreach (Parameter P in await Node.GetParametersAsync(Caller))
							P.Export(Xml);

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

		#endregion

	}
}
