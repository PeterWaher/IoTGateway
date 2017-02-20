using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Networking.XMPP.Concentrator.Attributes;
using Waher.Networking.XMPP.Concentrator.DisplayableParameters;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.DataTypes;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.DataForms.Layout;
using Waher.Networking.XMPP.DataForms.ValidationMethods;

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
				//w.WriteElementString("value", "setNodeParametersAfterEdit");
				//w.WriteElementString("value", "getCommonNodeParametersForEdit");
				//w.WriteElementString("value", "setCommonNodeParametersAfterEdit");
				//w.WriteElementString("value", "getAddableNodeTypes");
				//w.WriteElementString("value", "getParametersForNewNode");
				//w.WriteElementString("value", "createNewNode");
				//w.WriteElementString("value", "destroyNode");
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

		private static Task<Language> GetLanguage(XmlElement E)
		{
			return GetLanguage(E, Translator.DefaultLanguageCode);
		}

		private static async Task<Language> GetLanguage(XmlElement E, string DefaultLanguageCode)
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
						e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Source not found.", e.IQ));
				else
				{
					Language Language = await GetLanguage(e.Query);
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
								e.IqError(new StanzaErrors.ItemNotFoundException("Type not found", e.IQ));
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
							foreach (INode N in Node.ChildNodes)
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Source not found.", e.IQ));
				else
				{
					Language Language = await GetLanguage(e.Query);
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
				else
				{
					Language Language = await GetLanguage(e.Query);
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
				else
				{
					Language Language = await GetLanguage(e.Query);
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

		private async void MoveNodeUpHandler(object Sender, IqEventArgs e)
		{
			try
			{
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
					e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
						e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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
						e.IqError(new StanzaErrors.ItemNotFoundException("Node not found.", e.IQ));
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

		#endregion

		#region Editable Parameters

		private async void GetNodeParametersForEditHandler(object Sender, IqEventArgs e)
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

				if (Node != null && await Node.CanViewAsync(Caller))
				{
					Type T = Node.GetType();
					string DefaultLanguageCode = null;

					foreach (DefaultLanguageAttribute Attr in T.GetCustomAttributes(typeof(DefaultLanguageAttribute), true))
					{
						DefaultLanguageCode = Attr.LanguageCode;
						if (!string.IsNullOrEmpty(DefaultLanguageCode))
							break;
					}

					if (string.IsNullOrEmpty(DefaultLanguageCode))
						DefaultLanguageCode = Translator.DefaultLanguageCode;

					DataForm Parameters = new DataForm(Sender as XmppClient, FormType.Form, e.To, e.From);
					Language Language = await GetLanguage(e.Query, DefaultLanguageCode);
					Namespace Namespace = await Language.GetNamespaceAsync(T.Namespace);
					List<Field> Fields = new List<Field>();
					List<Page> Pages = new List<Page>();
					Dictionary<string, Page> PageByLabel = new Dictionary<string, Page>();
					Dictionary<string, Section> SectionByPageAndSectionLabel = null;
					List<KeyValuePair<string, string>> Options = null;
					string Header;
					string ToolTip;
					string PageLabel;
					string SectionLabel;
					string s;
					int StringId;
					bool Required;
					bool ReadOnly;
					bool Masked;
					bool Alpha;
					bool DateOnly;
					HeaderAttribute HeaderAttribute;
					ToolTipAttribute ToolTipAttribute;
					PageAttribute PageAttribute;
					SectionAttribute SectionAttribute;
					OptionAttribute OptionAttribute;
					TextAttribute TextAttribute;
					RegularExpressionAttribute RegularExpressionAttribute;
					LinkedList<string> TextAttributes;
					RangeAttribute RangeAttribute;
					ValidationMethod ValidationMethod;
					Type PropertyType;
					Field Field;
					Page DefaultPage = null;
					Page Page;
					Section Section;

					if (Namespace == null)
						Namespace = await Language.CreateNamespaceAsync(T.Namespace);

					foreach (PropertyInfo PI in T.GetProperties(BindingFlags.Instance | BindingFlags.Public))
					{
						if (!PI.CanRead || !PI.CanWrite)
							continue;

						Header = ToolTip = PageLabel = SectionLabel = null;
						TextAttributes = null;
						ValidationMethod = null;
						Required = ReadOnly = Masked = Alpha = DateOnly = false;

						foreach (Attribute Attr in PI.GetCustomAttributes())
						{
							if ((HeaderAttribute = Attr as HeaderAttribute) != null)
							{
								Header = HeaderAttribute.Header;
								StringId = HeaderAttribute.StringId;
								if (StringId > 0)
									Header = await Namespace.GetStringAsync(StringId, Header);
							}
							else if ((ToolTipAttribute = Attr as ToolTipAttribute) != null)
							{
								ToolTip = ToolTipAttribute.ToolTip;
								StringId = ToolTipAttribute.StringId;
								if (StringId > 0)
									ToolTip = await Namespace.GetStringAsync(StringId, ToolTip);
							}
							else if ((PageAttribute = Attr as PageAttribute) != null)
							{
								PageLabel = PageAttribute.Label;
								StringId = PageAttribute.StringId;
								if (StringId > 0)
									PageLabel = await Namespace.GetStringAsync(StringId, PageLabel);
							}
							else if ((SectionAttribute = Attr as SectionAttribute) != null)
							{
								SectionLabel = SectionAttribute.Label;
								StringId = SectionAttribute.StringId;
								if (StringId > 0)
									SectionLabel = await Namespace.GetStringAsync(StringId, SectionLabel);
							}
							else if ((TextAttribute = Attr as TextAttribute) != null)
							{
								if (TextAttributes == null)
									TextAttributes = new LinkedList<string>();

								StringId = TextAttribute.StringId;
								if (StringId > 0)
									TextAttributes.AddLast(await Namespace.GetStringAsync(StringId, TextAttribute.Label));
								else
									TextAttributes.AddLast(TextAttribute.Label);
							}
							else if ((OptionAttribute = Attr as OptionAttribute) != null)
							{
								if (Options == null)
									Options = new List<KeyValuePair<string, string>>();

								StringId = OptionAttribute.StringId;
								if (StringId > 0)
								{
									Options.Add(new KeyValuePair<string, string>(OptionAttribute.Option.ToString(),
										await Namespace.GetStringAsync(StringId, TextAttribute.Label)));
								}
								else
									Options.Add(new KeyValuePair<string, string>(OptionAttribute.Option.ToString(), OptionAttribute.Label));
							}
							else if ((RegularExpressionAttribute = Attr as RegularExpressionAttribute) != null)
								ValidationMethod = new RegexValidation(RegularExpressionAttribute.Pattern);
							else if ((RangeAttribute = Attr as RangeAttribute) != null)
								ValidationMethod = new RangeValidation(RangeAttribute.Min, RangeAttribute.Max);
							else if (Attr is OpenAttribute)
								ValidationMethod = new OpenValidation();
							else if (Attr is RequiredAttribute)
								Required = true;
							else if (Attr is ReadOnlyAttribute)
								ReadOnly = true;
							else if (Attr is MaskedAttribute)
								Masked = true;
							else if (Attr is AlphaChannelAttribute)
								Alpha = true;
							else if (Attr is DateOnlyAttribute)
								DateOnly = true;
						}

						if (Header == null)
							continue;

						PropertyType = PI.PropertyType;
						Field = null;

						if (PropertyType == typeof(string[]))
						{
							if (ValidationMethod == null)
								ValidationMethod = new BasicValidation();

							if (Options == null)
							{
								Field = new TextMultiField(Parameters, PI.Name, Header, Required, (string[])PI.GetValue(Node),
									null, ToolTip, new StringDataType(), ValidationMethod, string.Empty, false, ReadOnly, false);
							}
							else
							{
								Field = new ListMultiField(Parameters, PI.Name, Header, Required, (string[])PI.GetValue(Node),
									Options.ToArray(), ToolTip, new StringDataType(), ValidationMethod, string.Empty, false, ReadOnly, false);
							}
						}
						else if (PropertyType == typeof(Enum))
						{
							if (ValidationMethod == null)
								ValidationMethod = new BasicValidation();

							if (Options == null)
							{
								Options = new List<KeyValuePair<string, string>>();

								foreach (string Option in Enum.GetNames(PropertyType))
									Options.Add(new KeyValuePair<string, string>(Option, Option));
							}

							Field = new ListSingleField(Parameters, PI.Name, Header, Required, new string[] { PI.GetValue(Node).ToString() },
								Options.ToArray(), ToolTip, null, ValidationMethod, string.Empty, false, ReadOnly, false);
						}
						else if (PropertyType == typeof(bool))
						{
							if (ValidationMethod == null)
								ValidationMethod = new BasicValidation();

							Field = new BooleanField(Parameters, PI.Name, Header, Required,
								new string[] { CommonTypes.Encode((bool)PI.GetValue(Node)) },
								Options == null ? null : Options.ToArray(), ToolTip, new BooleanDataType(), ValidationMethod,
								string.Empty, false, ReadOnly, false);
						}
						else
						{
							DataType DataType;

							if (PropertyType == typeof(string))
								DataType = new StringDataType();
							else if (PropertyType == typeof(byte))
								DataType = new ByteDataType();
							else if (PropertyType == typeof(short))
								DataType = new ShortDataType();
							else if (PropertyType == typeof(int))
								DataType = new IntDataType();
							else if (PropertyType == typeof(long))
								DataType = new LongDataType();
							else if (PropertyType == typeof(sbyte))
							{
								DataType = new ShortDataType();

								if (ValidationMethod == null)
									ValidationMethod = new RangeValidation(sbyte.MinValue.ToString(), sbyte.MaxValue.ToString());
							}
							else if (PropertyType == typeof(ushort))
							{
								DataType = new IntDataType();

								if (ValidationMethod == null)
									ValidationMethod = new RangeValidation(ushort.MinValue.ToString(), ushort.MaxValue.ToString());
							}
							else if (PropertyType == typeof(uint))
							{
								DataType = new LongDataType();

								if (ValidationMethod == null)
									ValidationMethod = new RangeValidation(uint.MinValue.ToString(), uint.MaxValue.ToString());
							}
							else if (PropertyType == typeof(ulong))
							{
								DataType = new IntegerDataType();

								if (ValidationMethod == null)
									ValidationMethod = new RangeValidation(ulong.MinValue.ToString(), ulong.MaxValue.ToString());
							}
							else if (PropertyType == typeof(DateTime))
							{
								if (DateOnly)
									DataType = new DateDataType();
								else
									DataType = new DateTimeDataType();
							}
							else if (PropertyType == typeof(decimal))
								DataType = new DecimalDataType();
							else if (PropertyType == typeof(double))
								DataType = new DoubleDataType();
							else if (PropertyType == typeof(float))
								DataType = new DoubleDataType();	// Use xs:double anyway
							else if (PropertyType == typeof(TimeSpan))
								DataType = new TimeDataType();
							else if (PropertyType == typeof(Uri))
								DataType = new AnyUriDataType();
							else if (PropertyType == typeof(Color))
							{
								if (Alpha)
									DataType = new ColorAlphaDataType();
								else
									DataType = new ColorDataType();
							}
							else
								DataType = null;

							if (ValidationMethod == null)
								ValidationMethod = new BasicValidation();

							if (Masked)
							{
								Field = new TextPrivateField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(Node) },
									Options == null ? null : Options.ToArray(), ToolTip, new StringDataType(), ValidationMethod,
									string.Empty, false, ReadOnly, false);
							}
							else if (Options == null)
							{
								Field = new TextSingleField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(Node) },
									null, ToolTip, new StringDataType(), ValidationMethod, string.Empty, false, ReadOnly, false);
							}
							else
							{
								Field = new ListSingleField(Parameters, PI.Name, Header, Required, new string[] { (string)PI.GetValue(Node) },
									Options.ToArray(), ToolTip, new StringDataType(), ValidationMethod, string.Empty, false, ReadOnly, false);
							}
						}

						if (Field == null)
							continue;

						Fields.Add(Field);

						if (string.IsNullOrEmpty(PageLabel))
						{
							if (DefaultPage == null)
							{
								DefaultPage = new Page(string.Empty);
								Pages.Add(DefaultPage);
								PageByLabel[string.Empty] = DefaultPage;
							}

							Page = DefaultPage;
							PageLabel = string.Empty;
						}
						else
						{
							if (!PageByLabel.TryGetValue(PageLabel, out Page))
							{
								Page = new Page(PageLabel);
								Pages.Add(Page);
								PageByLabel[PageLabel] = Page;
							}
						}

						if (string.IsNullOrEmpty(SectionLabel))
						{
							if (TextAttributes != null)
							{
								foreach (string Text in TextAttributes)
									Page.Add(new TextElement(Text));
							}

							Page.Add(new FieldReference(Field.Var));
						}
						else
						{
							if (SectionByPageAndSectionLabel == null)
								SectionByPageAndSectionLabel = new Dictionary<string, Section>();

							s = PageLabel + " \xa0 " + SectionLabel;
							if (!SectionByPageAndSectionLabel.TryGetValue(s, out Section))
							{
								Section = new Section(SectionLabel);
								SectionByPageAndSectionLabel[s] = Section;

								Page.Add(Section);
							}

							if (TextAttributes != null)
							{
								foreach (string Text in TextAttributes)
									Section.Add(new TextElement(Text));
							}

							Section.Add(new FieldReference(Field.Var));
						}
					}

					Parameters.Title = Node.NodeId;
					Parameters.Fields = Fields.ToArray();
					Parameters.Pages = Pages.ToArray();

					StringBuilder Xml = new StringBuilder();

					Xml.Append("<getNodeParametersForEditResponse xmlns='");
					Xml.Append(NamespaceConcentrator);
					Xml.Append("'>");

					Parameters.SerializeForm(Xml);

					Xml.Append("</getNodeParametersForEditResponse>");

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

		#endregion

	}
}
