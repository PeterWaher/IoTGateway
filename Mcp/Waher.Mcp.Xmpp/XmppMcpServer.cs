using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Mcp.Xmpp.UserInput;
using Waher.Networking.DNS;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.Mcp;
using Waher.Networking.HTTP.Mcp.Model;
using Waher.Networking.HTTP.Mcp.Model.Attributes;
using Waher.Networking.HTTP.Mcp.Model.Server;
using Waher.Networking.HTTP.OAuth;
using Waher.Networking.HTTP.OAuth.MetaData;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Runtime.Cache;
using Waher.Runtime.Collections;
using Waher.Runtime.Inventory;
using Waher.Runtime.Threading;
using Waher.Security;

namespace Waher.Mcp.Xmpp
{
	/// <summary>
	/// MCP Server resource for XMPP communication.
	/// </summary>
	[OAuthResourceName("MCP Server for XMPP communication")]
	[McpScopeRoot("MCP:XMPP")]
	public class XmppMcpServer : HttpMcpServerResource
	{
		internal const string BasePrivilege = OAuthResource.OAuthScopePrivilegePrefix + "MCP.XMPP";
		internal const string ToolsPrivilege = BasePrivilege + ".Tools";
		internal const string ResourcesPrivilege = BasePrivilege + ".Resources";
		internal const string ListPrivilege = ResourcesPrivilege + ".List";
		internal const string ReadPrivilege = ResourcesPrivilege + ".Read";
		internal const string MessagePrivilege = ToolsPrivilege + ".Message";
		internal const string InformationQueryPrivilege = ToolsPrivilege + ".InformationQuery";
		internal const string PresencePrivilege = ToolsPrivilege + ".Presence";
		internal const string EditPrivilege = ToolsPrivilege + ".Edit";

		private static readonly Cache<string, XmppClient> clients = CreateCache();
		private static XmppMcpServer? instance = null;

		private static Cache<string, XmppClient> CreateCache()
		{
			Cache<string, XmppClient> Result = new Cache<string, XmppClient>(int.MaxValue,
				TimeSpan.MaxValue, TimeSpan.FromHours(1));
			Result.Removed += Clients_Removed;

			return Result;
		}

		private static async Task Clients_Removed(object Sender,
			CacheItemEventArgs<string, XmppClient> e)
		{
			await e.Value.DisposeAsync();
		}

		/// <summary>
		/// MCP Server resource for XMPP communication.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public XmppMcpServer(string ResourceName, ISnifferSet? SnifferSet)
			: this(ResourceName, GetDefaultIcons(), null, SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for XMPP communication.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public XmppMcpServer(string ResourceName, Icon[] Icons,
			Uri? WebSiteUri, ISnifferSet? SnifferSet)
			: this(ResourceName,
				"XmppMcpServer",      // Name
				"XMPP MCP Server",    // Title
				typeof(XmppMcpServer).Assembly.GetName().Version.ToString(),
				"A Model Context Protocol (MCP) server resource permitting MCP clients " +
				"to access the federated XMPP network and send and receive messages, " +
				"perform information queries and publish their presence.",
				Icons,
				WebSiteUri,
				"Each MCP Client gets associated with an XMPP account. If no user " +
				"account has been defined for the client, the user will be elicited " +
				"to input credentials for an XMPP account. Once connected, resources " +
				"show which items are available in the roster, or messages that have " +
				"been received. Tools can be used to send messages, information " +
				"queries and presence stanzas, as well as manipulate the roster.It is " +
				"the responsability of the MCP Client to read incoming messages and " +
				"store them if necessary.",
				SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for XMPP communication.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public XmppMcpServer(string ResourceName, string Name,
			string Title, string Version, string Description, Icon[] Icons, Uri? WebSiteUri,
			string Instructions, ISnifferSet? SnifferSet)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions, SnifferSet)
		{
		}

		/// <summary>
		/// If resources published by the MCP Server require authentication. If true, 
		/// the client must authenticate before resources can be listed or read.
		/// </summary>
		public override bool ResourcesRequireAuthentication => true;

		/// <summary>
		/// If the MCP server has resource capabilities.
		/// </summary>
		public override bool HasResources => true;

		/// <summary>
		/// MCP server resource documentation, as an array of key-value pairs.
		/// The Key represents Markdown (true) or plain text (false), and the Value
		/// represents the documentation text. Each entry in the array represents a
		/// paragraph.
		/// </summary>
		public override KeyValuePair<bool, string>[] ResourceDocumentation
		{
			get
			{
				return new KeyValuePair<bool, string>[]
				{
					new KeyValuePair<bool, string>(true,
						"There are different types of resources published by the XMPP " +
						"MCP Server. The type of resource is identified by the URI scheme " +
						"of each resource."),
					new KeyValuePair<bool, string>(true,
						"The `xmpp` URI scheme represents a contact in the account-specific " +
						"roster. Read the resource to gain information about what groups " +
						"the contact belongs to, and the status of presence subscriptions, " +
						"as well as nick names and last published presence information."),
					new KeyValuePair<bool, string>(true,
						"**Note**: Rosters are account-specific. When working with XMPP " +
						"each MCP Client gets associated with an XMPP account. The roster " +
						"belongs to the XMPP account. Different MCP Clients have their " +
						"separate rosters. Resources can be shared between MCP clients " +
						"however, as they only point to the different contacts. When " +
						"different clients read the resources however, different responses " +
						"are expected."),
					new KeyValuePair<bool, string>(true,
						"The `mid` URI scheme represents a message stanza that has been " +
						"received. Read the resource, pop the message from the queue, and " +
						"read it. Once the message has been read, it will be removed from " +
						"the resource list. If the MCP client needs to store it, it can "+
						"use file persistence to store the contents of the resource."),
					new KeyValuePair<bool, string>(true,
						"**Note**: Messages are not persisted by the MCP Server. It is " +
						"the responsability of the MCP Client to read incoming messages " +
						"and store them if necessary.")
				};
			}
		}

		/// <summary>
		/// Lists available MCP server resources.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of resources.</returns>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task<Dictionary<string, object>?> Resources_List(HttpRequest Request,
			HttpResponse Response, string? Cursor = null)
		{
			return base.Resources_List(Request, Response, Cursor);
		}

		/// <summary>
		/// Reads an MCP server resource.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Uri">URI of the resource to read.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[RequiredPrivilege(ReadPrivilege)]
		protected override async Task<Dictionary<string, object>?> Resources_Read(HttpRequest Request,
			HttpResponse Response, Uri Uri, [JsonRpcMetaDataArgument] object? _Meta = null)
		{
			return await base.Resources_Read(Request, Response, Uri, _Meta);
		}

		/// <summary>
		/// Subscribes to an MCP server resource.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Uri">URI of the resource to subscribe to.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task Resources_Subscribe(HttpRequest Request, HttpResponse Response,
			Uri Uri)
		{
			return base.Resources_Subscribe(Request, Response, Uri);
		}

		/// <summary>
		/// Unsubscribes from an MCP server resource.
		/// </summary>
		/// <param name="Request">HTTP request object.</param>
		/// <param name="Response">HTTP response object.</param>
		/// <param name="Uri">URI of the resource to unsubscribe from.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override async Task Resources_Unsubscribe(HttpRequest Request, HttpResponse Response,
			Uri Uri)
		{
			await base.Resources_Unsubscribe(Request, Response, Uri);
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Request">HTTP Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public override async Task<Resource[]> GetResources(HttpRequest Request, IUser? User,
			Session? Session)
		{
			if (User is null || Session is null)
				return Array.Empty<Resource>();

			XmppClient Client = await this.GetClient(Request, User, Session);
			ChunkedList<Resource> Resources = new ChunkedList<Resource>();

			foreach (RosterItem Item in Client.Roster)
				Resources.Add(new RosterItemResource(Item));

			return Resources.ToArray();
		}

		/// <summary>
		/// Method called when a resource has been registered on a server.
		/// </summary>
		/// <param name="Server">Server</param>
		public override void AddReference(HttpServer Server)
		{
			base.AddReference(Server);
			instance ??= this;
		}

		private async Task<XmppClient> GetClient(HttpRequest Request, IUser User,
			Session Session)
		{
			if (clients.TryGetValue(User.UserName, out XmppClient? Client))
				return Client;

			using Semaphore Lock = await Semaphores.BeginWrite("mcp:xmpp:" + User.UserName);

			if (clients.TryGetValue(User.UserName, out Client))
				return Client;

			ClientCredentials? Credentials = await Database.FindFirstIgnoreRest<ClientCredentials>(
				new FilterFieldEqualTo("UserName", User.UserName));

			if (!(Credentials is null))
			{
				string Host = Credentials.Domain ?? string.Empty;
				int Port = XmppCredentials.DefaultPort;

				ResourceRecord[]? Records = await DnsResolver.TryResolve(
					"_xmpp-client._tcp." + Credentials.Domain, QTYPE.SRV, QCLASS.IN);

				if ((Records?.Length ?? 0) > 0 && Records![0] is SRV SRV)
				{
					Host = SRV.TargetHost;
					Port = SRV.Port;
				}

				try
				{
					Client = new XmppClient(Host, Port, Credentials.UserName,
						Credentials.PasswordHash, Credentials.PasswordHashType,
						typeof(XmppMcpServer).Assembly)
					{
						RequestRosterOnStartup = true,
						AllowCramMD5 = Credentials.AllowInsecureMechanisms,
						AllowDigestMD5 = Credentials.AllowInsecureMechanisms,
						AllowEncryption = true,
						AllowPlain = Credentials.AllowInsecureMechanisms,
						AllowScramSHA1 = true,
						AllowScramSHA256 = true,
						AllowQuickLogin = true
					};

					int ConnectionResult = await Client.WaitStateAsync(30000,
						XmppState.Connected, XmppState.Offline, XmppState.Error);

					if (ConnectionResult == 0)
					{
						this.Setup(Client, User);
						clients[User.UserName] = Client;
						return Client;
					}

					await Client.DisposeAsync();
					Client = null;
				}
				catch (Exception)
				{
					if (!(Client is null))
					{
						await Client.DisposeAsync();
						Client = null;
					}
				}
			}

			XmppCredentialsInput NewCredentials = new XmppCredentialsInput()
			{
				UserName = Credentials?.UserName ?? string.Empty,
				Domain = Credentials?.Domain ?? string.Empty,
			};

			if (Types.TryGetModuleParameter("Domain", out string Domain))
				NewCredentials.Domain = Domain;

			do
			{
				bool? Result = await this.ElicitUserInput(Request,
					"Please provide credentials to your XMPP account you want to use, " +
					"or an API key if you want to create an account on a server.",
					NewCredentials, true, Session, 5 * 60 * 1000);

				if (!Result.HasValue)
					throw new Exception("User did not provide credentials.");

				if (!Result.Value)
					throw new Exception("User cancelled the request.");

				ResourceRecord[]? Records = await DnsResolver.TryResolve(
					"_xmpp-client._tcp." + NewCredentials.Domain, QTYPE.SRV, QCLASS.IN);

				string Host = NewCredentials.Domain;
				int Port = XmppCredentials.DefaultPort;

				if ((Records?.Length ?? 0) > 0 && Records![0] is SRV SRV)
				{
					Host = SRV.TargetHost;
					Port = SRV.Port;
				}

				try
				{
					Client = new XmppClient(Host, Port, NewCredentials.UserName,
						NewCredentials.Password, string.Empty,
						typeof(XmppMcpServer).Assembly)
					{
						RequestRosterOnStartup = true,
						AllowCramMD5 = NewCredentials.AllowInsecureMechanisms,
						AllowDigestMD5 = NewCredentials.AllowInsecureMechanisms,
						AllowEncryption = true,
						AllowPlain = NewCredentials.AllowInsecureMechanisms,
						AllowScramSHA1 = true,
						AllowScramSHA256 = true,
						AllowQuickLogin = true
					};

					int ConnectionResult = await Client.WaitStateAsync(30000,
						XmppState.Connected, XmppState.Offline, XmppState.Error);

					if (ConnectionResult != 0)
					{
						await Client.DisposeAsync();
						Client = null;
					}
				}
				catch (Exception)
				{
					if (!(Client is null))
					{
						await Client.DisposeAsync();
						Client = null;
					}
				}
			}
			while (Client is null);

			this.Setup(Client, User);
			clients[User.UserName] = Client;

			Credentials = new ClientCredentials()
			{
				UserName = NewCredentials.Domain,
				Domain = NewCredentials.Domain,
				PasswordHash = Client.PasswordHash,
				PasswordHashType = Client.PasswordHashMethod,
				AllowInsecureMechanisms = NewCredentials.AllowInsecureMechanisms
			};

			await Database.Insert(Credentials);

			return Client;
		}

		private void Setup(XmppClient Client, IUser User)
		{
			Client.SetTag("User", User);
			Client.SetTag("Messages", new ChunkedList<MessageEventArgs>());

			Client.OnNormalMessage += this.Client_OnNormalMessage;
			Client.OnGroupChatMessage += this.Client_OnGroupChatMessage;
			Client.OnChatMessage += this.Client_OnChatMessage;
			Client.OnError += this.Client_OnError;
		}

		private Task Client_OnError(object Sender, Exception e)
		{
			return Task.CompletedTask;  // TODO
		}

		private Task Client_OnNormalMessage(object Sender, MessageEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}

		private Task Client_OnChatMessage(object Sender, MessageEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}

		private Task Client_OnGroupChatMessage(object Sender, MessageEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}
	}
}
