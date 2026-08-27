using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Mcp.Xmpp.Resources;
using Waher.Mcp.Xmpp.Responses;
using Waher.Mcp.Xmpp.UserInput;
using Waher.Networking;
using Waher.Networking.DNS;
using Waher.Networking.DNS.Enumerations;
using Waher.Networking.DNS.ResourceRecords;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.JsonRpc;
using Waher.Networking.HTTP.JsonRpc.Transports;
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
		internal const string GetMessagePrivilege = MessagePrivilege + ".Get";
		internal const string SendMessagePrivilege = MessagePrivilege + ".Send";
		internal const string RosterPrivilege = ToolsPrivilege + ".Roster";
		internal const string AddRosterItemPrivilege = RosterPrivilege + ".Add";
		internal const string UpdateRosterItemPrivilege = RosterPrivilege + ".Update";
		internal const string RemoveRosterItemPrivilege = RosterPrivilege + ".Remove";
		internal const string PresencePrivilege = ToolsPrivilege + ".Presence";
		internal const string SubscribePresencePrivilege = PresencePrivilege + ".Subscribe";
		internal const string UnsubscribePresencePrivilege = PresencePrivilege + ".Unsubscribe";
		internal const string AcceptPresencePrivilege = PresencePrivilege + ".Accept";
		internal const string DeclinePresencePrivilege = PresencePrivilege + ".Decline";

		private static readonly Cache<string, ClientRec> clients = CreateCache();
		private static XmppMcpServer? instance = null;

		private static Cache<string, ClientRec> CreateCache()
		{
			Cache<string, ClientRec> Result = new Cache<string, ClientRec>(int.MaxValue,
				TimeSpan.MaxValue, TimeSpan.FromHours(1));
			Result.Removed += Clients_Removed;

			return Result;
		}

		private static async Task Clients_Removed(object Sender,
			CacheItemEventArgs<string, ClientRec> e)
		{
			await e.Value.Client.DisposeAsync();
			e.Value.Messages.Clear();
		}

		private class ClientRec
		{
			public ClientRec(string UserName, XmppClient Client)
			{
				this.UserName = UserName;
				this.Client = Client;
				this.Messages = new ChunkedList<MessageEventArgs>();
			}

			public string UserName;
			public XmppClient Client;
			public ChunkedList<MessageEventArgs> Messages = new ChunkedList<MessageEventArgs>();
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
				"perform information queries and publish their presence. Before messages " +
				"and queries can be made, the MCP client should first request the " +
				"presence subscription of the contact to whom messages are queries " +
				"should be sent.",
				Icons,
				WebSiteUri,
				"Each MCP Client gets associated with an XMPP account. If no user " +
				"account has been defined for the client, the user will be elicited " +
				"to input credentials for an XMPP account. Once connected, resources " +
				"show which items are available in the roster, or messages that have " +
				"been received. Tools can be used to send messages, information " +
				"queries and presence stanzas, as well as manipulate the roster. It is " +
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
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Cursor">Cursor for pagination.</param>
		/// <returns>Dictionary containing the list of resources.</returns>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task<Dictionary<string, object>?> Resources_List(
			IJsonRpcCall Call, string? Cursor = null)
		{
			return base.Resources_List(Call, Cursor);
		}

		/// <summary>
		/// Reads an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to read.</param>
		/// <param name="_Meta">Associated meta-data, if available.</param>
		/// <returns>Dictionary containing the result of the tool call.</returns>
		[RequiredPrivilege(ReadPrivilege)]
		protected override async Task<Dictionary<string, object>?> Resources_Read(
			IJsonRpcCall Call, Uri Uri, [JsonRpcMetaDataArgument] object? _Meta = null)
		{
			return await base.Resources_Read(Call, Uri, _Meta);
		}

		/// <summary>
		/// Subscribes to an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to subscribe to.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override Task Resources_Subscribe(IJsonRpcCall Call, Uri Uri)
		{
			return base.Resources_Subscribe(Call, Uri);
		}

		/// <summary>
		/// Unsubscribes from an MCP server resource.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Uri">URI of the resource to unsubscribe from.</param>
		[RequiredPrivilege(ListPrivilege)]
		protected override async Task Resources_Unsubscribe(IJsonRpcCall Call, Uri Uri)
		{
			await base.Resources_Unsubscribe(Call, Uri);
		}

		/// <summary>
		/// Tries to get a resource, given its URI.
		/// </summary>
		/// <param name="Call">JSON-RPC Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Uri">URI of resource.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Resource, if found (and user has access rights to it), null otherwise.</returns>
		public override async Task<Resource?> TryGetResource(IJsonRpcCall Call,
			IUser? User, Uri Uri, Session? Session)
		{
			if (Session is null || User is null)
				return await base.TryGetResource(Call, User, Uri, Session);

			switch (Uri.Scheme)
			{
				case "xmpp":
					XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
					if (Client is null)
						return null;

					RosterItem Contact = Client[Uri.AbsolutePath];
					if (!(Contact is null))
						return new RosterItemResource(Contact);
					break;

				case "mid":
					Client = await this.GetClient(this, Call, User, Session, true);
					if (Client is null)
						return null;

					if (Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
						!(McpXmppExtension is null))
					{
						MessageEventArgs? Message = McpXmppExtension.TryGetMessage(Uri.AbsolutePath, false);
						if (!(Message is null))
							return new MessageResource(Uri.AbsolutePath, Message);
					}
					break;
			}

			return await base.TryGetResource(Call, User, Uri, Session);
		}

		/// <summary>
		/// Gets available resources.
		/// </summary>
		/// <param name="Call">JSON-RPC Request object.</param>
		/// <param name="User">MCP Client user requesting resources.</param>
		/// <param name="Session">MCP Session, if available.</param>
		/// <returns>Array of resources.</returns>
		public override async Task<Resource[]> GetResources(
			IJsonRpcCall Call, IUser? User, Session? Session)
		{
			if (User is null || Session is null)
				return Array.Empty<Resource>();

			XmppClient? Client = await this.GetClient(this, Call, User, Session, false);
			if (Client is null)
				return Array.Empty<Resource>();

			ChunkedList<Resource> Resources = new ChunkedList<Resource>();

			foreach (RosterItem Item in Client.Roster)
				Resources.Add(new RosterItemResource(Item));

			if (Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				foreach (string MessageId in McpXmppExtension.GetMessageIds())
					Resources.Add(new MessageResource(MessageId, null));
			}

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

		/// <summary>
		/// Checks if the MCP session is connected.
		/// </summary>
		/// <param name="User">Authenticated user object.</param>
		/// <param name="Session">MCP session object.</param>
		/// <returns>If the session is connected or not.</returns>
		public bool IsConnected(IUser User, Session Session)
		{
			if (!clients.TryGetValue(User.UserName, out ClientRec? Rec))
				return false;

			if (!Rec.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return false;

			if (!McpXmppExtension.IsRegistered(Session.SessionId))
				return false;
			
			return Rec.Client.State == XmppState.Connected;
		}

		/// <summary>
		/// Gets the XMPP client associated with a user. If no client is available, 
		/// the user will be elicited to provide credentials for an XMPP account.
		/// </summary>
		/// <param name="McpServer">MCP Server resource requesting the client.</param>
		/// <param name="Call">JSON-RPC call originating the request.</param>
		/// <param name="User">Authenticated user object.</param>
		/// <param name="Session">MCP session object.</param>
		/// <param name="CreateIfNotDefined">Create a client if one is not defined.</param>
		/// <returns>XMPP Client, or null if not defined.</returns>
		public async Task<XmppClient?> GetClient(HttpMcpServerResource McpServer,
			IJsonRpcCall Call, IUser User, Session Session, bool CreateIfNotDefined)
		{
			if (clients.TryGetValue(User.UserName, out ClientRec? Rec))
			{
				if (Rec.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
					McpXmppExtension.Register(Session.SessionId);

				return Rec.Client;
			}

			using Semaphore Lock = new Semaphore("mcp:xmpp:" + User.UserName);

			if (!await Lock.TryBeginWrite(5 * 60 * 1000))
			{
				if (CreateIfNotDefined)
					throw new Exception("Unable to obtain exclusive access to XMPP client for user " + User.UserName + ".");
				else
					return null;
			}

			if (clients.TryGetValue(User.UserName, out Rec))
			{
				if (Rec.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
					McpXmppExtension.Register(Session.SessionId);

				return Rec.Client;
			}

			string Error = string.Empty;
			bool HasError = false;
			XmppClient? Client = null;
			ClientCredentials? Credentials = await Database.FindFirstDeleteRest<ClientCredentials>(
				new FilterFieldEqualTo("McpUserName", User.UserName));
			TaskCompletionSource<bool> ErrorReceived = new TaskCompletionSource<bool>();

			Task OnError(object sender, Exception e)
			{
				Error = Log.UnnestException(e).Message;
				HasError = true;
				ErrorReceived.TrySetResult(true);
				return Task.CompletedTask;
			};

			if (!(Credentials is null))
			{
				string Host = Credentials.Domain ?? string.Empty;
				int Port = XmppCredentials.DefaultPort;

				if (Host != "localhost" && !IPAddress.TryParse(Host, out _))
				{
					ResourceRecord[]? Records = await DnsResolver.TryResolve(
					"_xmpp-client._tcp." + Credentials.Domain, QTYPE.SRV, QCLASS.IN);

					if ((Records?.Length ?? 0) > 0 && Records![0] is SRV SRV)
					{
						Host = SRV.TargetHost;
						Port = SRV.Port;
					}
				}

				try
				{
					using InMemorySniffer Sniffer = new InMemorySniffer(200, "MCP." + User.UserName);

					Client = new XmppClient(Host, Port, Credentials.XmppAccountName,
						Credentials.PasswordHash, Credentials.PasswordHashType,
						string.Empty, typeof(XmppMcpServer).Assembly, Sniffer)
					{
						RequestRosterOnStartup = true,
						TrustServer = Credentials.TrustServer,
						AllowCramMD5 = Credentials.AllowInsecureMechanisms,
						AllowDigestMD5 = Credentials.AllowInsecureMechanisms,
						AllowEncryption = true,
						AllowPlain = Credentials.AllowInsecureMechanisms,
						AllowScramSHA1 = true,
						AllowScramSHA256 = true,
						AllowQuickLogin = true
					};

					Client.OnError += OnError;
					Client.OnConnectionError += OnError;

					await Client.Connect();
					int ConnectionResult = await Client.WaitStateAsync(30000,
						XmppState.Connected, XmppState.Error);

					if (ConnectionResult == 0)
					{
						Client.OnError -= OnError;
						Client.OnConnectionError -= OnError;

						Client.RegisterExtension(new McpXmppExtension(Call, Session.SessionId));
						this.Setup(Client, User);
						clients[User.UserName] = new ClientRec(User.UserName, Client);
						return Client;
					}
					else
					{
						await Client.DisposeAsync();
						Client = null;

						if (!HasError)
						{
							_ = Task.Delay(1000).ContinueWith(_ => ErrorReceived.TrySetResult(false));
							await ErrorReceived.Task;
						}

						if (!HasError)
						{
							Error = "Connection failed. Please review existing credentials, and try again.";
							HasError = true;
						}

						if (Types.TryGetModuleParameter("AppData", out string AppData))
						{
							// TODO: Remove
							using XmlFileSniffer Sniffer2 = new XmlFileSniffer("C:\\Temp\\Sniffers\\MCP.XMPP.xml",
								AppData + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
								BinaryPresentationMethod.ByteCount);

							Sniffer.Replay(Sniffer2);
							await Sniffer2.FlushAsync();
						}
					}
				}
				catch (Exception ex)
				{
					Error = Log.UnnestException(ex).Message;
					HasError = true;

					if (!(Client is null))
					{
						await Client.DisposeAsync();
						Client = null;
					}
				}
			}

			if (!CreateIfNotDefined)
				return null;

			XmppCredentialsInput NewCredentials = new XmppCredentialsInput()
			{
				UserName = Credentials?.XmppAccountName ?? string.Empty,
				Domain = Credentials?.Domain ?? string.Empty,
				TrustServer = Credentials?.TrustServer ?? false,
				AllowInsecureMechanisms = Credentials?.AllowInsecureMechanisms ?? false
			};

			if (Types.TryGetModuleParameter("Domain", out string Domain))
				NewCredentials.Domain = Domain;

			do
			{
				string Message = "Please provide credentials to your XMPP account you " +
					"want to use, or an API key if you want to create an account on a " +
					"server. (This input dialog is cancelled automatically after 5 " + 
					"minutes.)";

				if (HasError)
					Message = "Error: " + Error + "\r\n\r\n" + Message;

				bool? Result = await McpServer.ElicitUserInput(Call, Message,
					NewCredentials, true, Session, 5 * 60 * 1000);

				if (!Result.HasValue)
					throw new Exception("User did not provide credentials.");

				if (!Result.Value)
					throw new Exception("User cancelled the request.");

				string Host = NewCredentials.Domain;
				int Port = XmppCredentials.DefaultPort;

				if (Host != "localhost" && !IPAddress.TryParse(Host, out _))
				{
					ResourceRecord[]? Records = await DnsResolver.TryResolve(
					"_xmpp-client._tcp." + NewCredentials.Domain, QTYPE.SRV, QCLASS.IN);

					if ((Records?.Length ?? 0) > 0 && Records![0] is SRV SRV)
					{
						Host = SRV.TargetHost;
						Port = SRV.Port;
					}
				}

				try
				{
					ErrorReceived = new TaskCompletionSource<bool>();
					Error = string.Empty;
					HasError = false;

					Client = new XmppClient(Host, Port, NewCredentials.UserName,
						NewCredentials.Password, string.Empty,
						typeof(XmppMcpServer).Assembly)
					{
						RequestRosterOnStartup = true,
						TrustServer = NewCredentials.TrustServer,
						AllowCramMD5 = NewCredentials.AllowInsecureMechanisms,
						AllowDigestMD5 = NewCredentials.AllowInsecureMechanisms,
						AllowEncryption = true,
						AllowPlain = NewCredentials.AllowInsecureMechanisms,
						AllowScramSHA1 = true,
						AllowScramSHA256 = true,
						AllowQuickLogin = true
					};

					Client.OnError += OnError;
					Client.OnConnectionError += OnError;

					await Client.Connect();
					int ConnectionResult = await Client.WaitStateAsync(30000,
						XmppState.Connected, XmppState.Error);

					if (ConnectionResult == 0)
					{
						Client.OnError -= OnError;
						Client.OnConnectionError -= OnError;
					}
					else
					{
						await Client.DisposeAsync();
						Client = null;

						if (!HasError)
						{
							_ = Task.Delay(1000).ContinueWith(_ => ErrorReceived.TrySetResult(false));

							await ErrorReceived.Task;
						}

						if (!HasError)
						{
							Error = "Connection failed, please try again.";
							HasError = true;
						}
					}
				}
				catch (Exception ex)
				{
					if (!HasError)
					{
						Error = Log.UnnestException(ex).Message;
						HasError = true;
					}

					if (!(Client is null))
					{
						await Client.DisposeAsync();
						Client = null;
					}
				}
			}
			while (Client is null);

			Client.RegisterExtension(new McpXmppExtension(Call, Session.SessionId));
			this.Setup(Client, User);
			clients[User.UserName] = new ClientRec(User.UserName, Client);

			if (Credentials is null)
			{
				Credentials = new ClientCredentials()
				{
					McpUserName = User.UserName,
					XmppAccountName = NewCredentials.UserName,
					Domain = NewCredentials.Domain,
					PasswordHash = Client.PasswordHash,
					PasswordHashType = Client.PasswordHashMethod,
					TrustServer = NewCredentials.TrustServer,
					AllowInsecureMechanisms = NewCredentials.AllowInsecureMechanisms
				};

				await Database.Insert(Credentials);
			}
			else
			{
				Credentials.XmppAccountName = NewCredentials.UserName;
				Credentials.Domain = NewCredentials.Domain;
				Credentials.PasswordHash = Client.PasswordHash;
				Credentials.PasswordHashType = Client.PasswordHashMethod;
				Credentials.TrustServer = NewCredentials.TrustServer;
				Credentials.AllowInsecureMechanisms = NewCredentials.AllowInsecureMechanisms;

				await Database.Update(Credentials);
			}

			this.ResourcesUpdated(User);

			return Client;
		}

		private void Setup(XmppClient Client, IUser User)
		{
			Client.SetTag("User", User);
			Client.SetTag("Messages", new ChunkedList<MessageEventArgs>());

			Client.OnNormalMessage += this.Client_OnMessageReceived;
			Client.OnGroupChatMessage += this.Client_OnMessageReceived;
			Client.OnChatMessage += this.Client_OnMessageReceived;
			Client.OnError += this.Client_OnError;
			Client.OnPresence += this.Client_OnPresence;
			Client.OnPresenceSubscribe += this.Client_OnPresenceSubscribe;
			Client.OnPresenceSubscribed += this.Client_OnPresenceSubscribed;
			Client.OnPresenceUnsubscribe += this.Client_OnPresenceUnsubscribe;
			Client.OnPresenceUnsubscribed += this.Client_OnPresenceUnsubscribed;
			Client.OnRosterItemAdded += this.Client_OnRosterItemAdded;
			Client.OnRosterItemRemoved += this.Client_OnRosterItemRemoved;
			Client.OnRosterItemUpdated += this.Client_OnRosterItemUpdated;
			Client.OnStateChanged += this.Client_OnStateChanged;
		}

		/// <summary>
		/// MCP Server Tool to send a presence subscription request to another user.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="To">Bare JID of the recipient of the presence subscription request.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Request Presence Subscription",
			"Requests a presence subscription to another user's presence.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(SubscribePresencePrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> RequestPresenceSubscription(
			IJsonRpcCall Call,

			[McpStringParameter("To", "Bare JID of the user to whom send a presence subscription request.", 3, 256)]
			string To)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(To))
				return new GenericResponse(false, "Invalid Bare JID: " + To);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			await Client.RequestPresenceSubscription(To);

			return new GenericResponse(true, "Presence subscription request sent to " + To + ".");
		}

		/// <summary>
		/// MCP Server Tool to accept a presence subscription request from another user.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="From">Bare JID of the sender of the presence subscription to accept.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Accept Presence Subscription",
			"Accepts a presence subscription request from another user.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(AcceptPresencePrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> AcceptPresenceSubscription(
			IJsonRpcCall Call,

			[McpStringParameter("From", "Bare JID of the sender of the presence subscription request to accept.", 3, 256)]
			string From)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			if (!Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) ||
				McpXmppExtension is null)
			{
				return new GenericResponse(false, "MCP XMPP extension not available.");
			}

			if (!McpXmppExtension.TryGetPresenceSubscriptionRequest(From, out PresenceEventArgs? e))
				return new GenericResponse(false, "No presence subscription request from " + From + " available.");

			await e.Accept();

			return new GenericResponse(true, "Presence subscription request from " + From + " accepted.");
		}

		/// <summary>
		/// MCP Server Tool to decline a presence subscription request from another user.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="From">Bare JID of the sender of the presence subscription to decline.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Decline Presence Subscription",
			"Declines a presence subscription request from another user.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(DeclinePresencePrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> DeclinePresenceSubscription(
			IJsonRpcCall Call,

			[McpStringParameter("From", "Bare JID of the sender of the presence subscription request to decline.", 3, 256)]
			string From)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			if (!Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) ||
				McpXmppExtension is null)
			{
				return new GenericResponse(false, "MCP XMPP extension not available.");
			}

			if (!McpXmppExtension.TryGetPresenceSubscriptionRequest(From, out PresenceEventArgs? e))
				return new GenericResponse(false, "No presence subscription request from " + From + " available.");

			await e.Decline();

			return new GenericResponse(true, "Presence subscription request from " + From + " declined.");
		}

		/// <summary>
		/// MCP Server Tool to send a presence unsubscription request to another user.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="To">Bare JID of the recipient of the presence unsubscription request.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Request Presence Unsubscription",
			"Requests a presence unsubscription from another user's presence.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			true,   // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(UnsubscribePresencePrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> RequestPresenceUnsubscription(
			IJsonRpcCall Call,

			[McpStringParameter("To", "Bare JID of the user to whom send a presence subscription request.", 3, 256)]
			string To)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(To))
				return new GenericResponse(false, "Invalid Bare JID: " + To);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			await Client.RequestPresenceUnsubscription(To);

			return new GenericResponse(true, "Request to ubsubscribe from presence from " + To + " sent.");
		}

		/// <summary>
		/// MCP Server Tool to add a contact to the roster of the XMPP account associated 
		/// with the MCP Client.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="BareJid">Bare JID of the contact to add to roster.</param>
		/// <param name="NickName">Optional nick name of the contact to add to roster.</param>
		/// <param name="Groups">Optional groups to which the contact should be added.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Add to Roster",  // Title
			"Adds a contact to the roster of the XMPP account associated with the MCP Client.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(AddRosterItemPrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> AddToRoster(
			IJsonRpcCall Call,

			[McpStringParameter("Bare JID", "Bare JID of the contact to add to roster.", 3, 256)]
			string BareJid,

			[McpStringParameter("Nick Name", "Optional nick name of the contact to add to roster.", 0, 256)]
			string? NickName = null,

			[McpParameter("Groups", "Optional groups to which the contact should be added.")]
			string[]? Groups = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(BareJid))
				return new GenericResponse(false, "Invalid Bare JID: " + BareJid);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			if (!(Client.GetRosterItem(BareJid) is null))
				return new GenericResponse(false, "Contact already in roster.");

			TaskCompletionSource<string?> RosterItemAdded = new TaskCompletionSource<string?>();
			
			await Client.AddRosterItem(new RosterItem(BareJid, NickName ?? string.Empty,
				Groups ?? Array.Empty<string>()), (_, e) =>
				{
					RosterItemAdded.TrySetResult(e.Ok ? null : e.ErrorText ?? string.Empty);
					return Task.CompletedTask;
				}, null);

			string? Error = await RosterItemAdded.Task;
			if (Error is null)
			{
				this.ResourcesUpdated(User);
				return new GenericResponse(true, "Contact added to roster.");
			}
			else
				return new GenericResponse(false, "Unable to add contact to roster: " + Error);
		}

		/// <summary>
		/// MCP Server Tool to update a contact in the roster of the XMPP account associated 
		/// with the MCP Client.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="BareJid">Bare JID of the contact to update in roster.</param>
		/// <param name="NickName">Optional nick name of the contact to update in roster.</param>
		/// <param name="Groups">Optional groups to which the contact should belong.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Update Roster Item",  // Title
			"Updates a contact in the roster of the XMPP account associated with the MCP Client.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			true,   // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(UpdateRosterItemPrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> UpdateRosterItem(
			IJsonRpcCall Call,

			[McpStringParameter("Bare JID", "Bare JID of the contact to update in roster.", 3, 256)]
			string BareJid,

			[McpStringParameter("Nick Name", "Optional nick name of the contact to update in roster.", 0, 256)]
			string? NickName = null,

			[McpParameter("Groups", "Optional groups to which the contact should belong.")]
			string[]? Groups = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(BareJid))
				return new GenericResponse(false, "Invalid Bare JID: " + BareJid);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			RosterItem Item = Client.GetRosterItem(BareJid);
			if (Item is null)
				return new GenericResponse(false, "Contact not in roster.");

			TaskCompletionSource<string?> RosterItemUpdated = new TaskCompletionSource<string?>();

			await Client.UpdateRosterItem(BareJid, NickName ?? string.Empty,
				Groups ?? Array.Empty<string>(), (_, e) =>
				{
					RosterItemUpdated.TrySetResult(e.Ok ? null : e.ErrorText ?? string.Empty);
					return Task.CompletedTask;
				}, null);

			string? Error = await RosterItemUpdated.Task;
			if (Error is null)
			{
				this.ResourceUpdated(User, RosterItemResource.CreateRosterUri(BareJid));
				return new GenericResponse(true, "Contact updated in roster.");
			}
			else
				return new GenericResponse(false, "Unable to update contact in roster: " + Error);
		}

		/// <summary>
		/// MCP Server Tool to remove a contact from the roster of the XMPP account associated 
		/// with the MCP Client.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="BareJid">Bare JID of the contact to remove from roster.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Remove from Roster",  // Title
			"Removes a contact from the roster of the XMPP account associated with the MCP Client.",   // Description
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(RemoveRosterItemPrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> RemoveFromRoster(
			IJsonRpcCall Call,

			[McpStringParameter("Bare JID", "Bare JID of the contact to remove from roster.", 3, 256)]
			string BareJid)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(BareJid))
				return new GenericResponse(false, "Invalid Bare JID: " + BareJid);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			RosterItem Item = Client.GetRosterItem(BareJid);
			if (Item is null)
				return new GenericResponse(false, "Contact not in roster.");

			TaskCompletionSource<string?> RosterItemRemoved = new TaskCompletionSource<string?>();

			await Client.RemoveRosterItem(BareJid, (_, e) =>
				{
					RosterItemRemoved.TrySetResult(e.Ok ? null : e.ErrorText ?? string.Empty);
					return Task.CompletedTask;
				}, null);

			string? Error = await RosterItemRemoved.Task;
			if (Error is null)
			{
				this.ResourcesUpdated(User);
				return new GenericResponse(true, "Contact removed from roster.");
			}
			else
				return new GenericResponse(false, "Unable to remove contact from roster: " + Error);
		}

		/// <summary>
		/// MCP Server Tool to send a chat message to a recipient using XMPP.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="To">Bare JID of the recipient of the message.</param>
		/// <param name="Message">Message content. Can be either plain text, or Markdown.</param>
		/// <param name="IsMarkdown">Indicates if the message is in Markdown format (true),
		/// or plain text (false).</param>
		/// <param name="Language">ISO-639 code of language used in the message, if any.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Send Chat Message",
			"Sends a Chat Message to a recipient using XMPP.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(SendMessagePrivilege)]
		[return: McpParameter("Result", "Results of operation.")]
		public async Task<GenericResponse> SendChatMessage(IJsonRpcCall Call,

			[McpStringParameter("To", "Bare JID of the recipient of the message.", 3, 256)]
			string To,

			[McpStringParameter("Message", "Message content. Can be either plain text, or Markdown.", 1, 10000)]
			string Message,

			[McpParameter("Is Markdown", "Indicates if the message is in Markdown format (true), or plain text (false).")]
			bool IsMarkdown = false,

			[McpStringParameter("Language", "Optional ISO-639 code of language used in the message, if any.", 0, 10)]
			string? Language = null)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			if (!XmppClient.BareJidRegEx.IsMatch(To))
				return new GenericResponse(false, "Invalid Bare JID: " + To);

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP client not available.");

			string Markdown;
			string Html;
			string PlainText;

			if (IsMarkdown)
			{
				MarkdownSettings Settings = new MarkdownSettings()
				{
					AllowHtml = false,
					AllowInlineScript = false,
					AllowScriptTag = false,
					ParseMetaData = false
				};
				MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Message, Settings);
				Markdown = await Doc.GenerateMarkdown();
				Html = await Doc.GenerateHTML();
				PlainText = await Doc.GeneratePlainText();
			}
			else
			{
				Markdown = string.Empty;
				Html = string.Empty;
				PlainText = Message;
			}

			StringBuilder Xml = new StringBuilder();
			AppendMultiFormatChatMessageXml(Xml, PlainText, Html, Markdown);

			TaskCompletionSource<DeliveryEventArgs> MessageSent = new TaskCompletionSource<DeliveryEventArgs>();

			await Client.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat,
				To, Xml.ToString(), string.Empty, string.Empty, Language,
				string.Empty, string.Empty,
				(_, e) =>
				{
					MessageSent.TrySetResult(e);
					return Task.CompletedTask;
				}, null);

			DeliveryEventArgs e = await MessageSent.Task;
			if (e.Ok)
				return new GenericResponse(true, "Message sent to " + To + ".");
			else
				return new GenericResponse(false, "Unable to send message to " + To + ".");
		}

		/// <summary>
		/// Appends the XML for a multi-formatted chat message to a string being built.
		/// </summary>
		/// <param name="Xml">XML output.</param>
		/// <param name="Text">Plain-text version of message. If empty or null, plain text is excluded from message.</param>
		/// <param name="Html">HTML version of message. If empty or null, HTML is excluded from message.</param>
		/// <param name="Markdown">Markdown containing message text. If empty or null, markdown is excluded from message.</param>
		public static void AppendMultiFormatChatMessageXml(StringBuilder Xml, string Text, string Html, string Markdown)
		{
			if (string.IsNullOrEmpty(Text))
				Xml.Append("<body/>");
			else
			{
				Xml.Append("<body>");

				if (Text.Contains("]]>"))
					Xml.Append(XML.Encode(Text));
				else
				{
					Xml.Append("<![CDATA[");
					Xml.Append(Text);
					Xml.Append("]]>");
				}

				Xml.Append("</body>");
			}

			if (!string.IsNullOrEmpty(Markdown))
			{
				Xml.Append("<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">");

				if (Markdown.Contains("]]>"))
					Xml.Append(XML.Encode(Markdown));
				else
				{
					Xml.Append("<![CDATA[");
					Xml.Append(Markdown);
					Xml.Append("]]>");
				}

				Xml.Append("</content>");
			}

			if (!string.IsNullOrEmpty(Html))
			{
				Xml.Append("<html xmlns='http://jabber.org/protocol/xhtml-im'>");
				Xml.Append("<body xmlns='http://www.w3.org/1999/xhtml'>");

				HtmlDocument Doc = new HtmlDocument("<root>" + Html + "</root>");
				IEnumerable<HtmlNode> Children = (Doc.Body ?? Doc.Root).Children;

				if (!(Children is null))
				{
					foreach (HtmlNode N in Children)
						N.Export(Xml);
				}

				Xml.Append("</body></html>");
			}
		}

		/// <summary>
		/// MCP Server Tool to retrieve a message received over XMPP.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="MessageId">The ID of the message to retrieve. Can be the 
		/// identifier itself, or the message resource URI.</param>
		/// <param name="Remove">Determines if the message should be removed from the list 
		/// of received messages or not.</param>
		/// <returns>Message, if found.</returns>
		[McpServerTool(
			"Get Message",
			"Gets a message that has been received over XMPP.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(GetMessagePrivilege)]
		[return: McpParameter("Result", "Message, if found.")]
		public async Task<MessageResponse> GetMessage(IJsonRpcCall Call,

			[McpStringParameter("Message ID", "The ID of the message to retrieve. Can be the identifier itself, or the message resource URI.", 3, 256)]
			string MessageId,

			[McpParameter("Remove", "Determines if the message should be removed from the list of received messages or not.")]
			bool Remove = false)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new MessageResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new MessageResponse("User not authenticated.");

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new MessageResponse("MCP XMPP client not available.");

			if (!Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) ||
				McpXmppExtension is null)
			{
				return new MessageResponse("XMPP Client not setup properly.");
			}

			if (MessageId.StartsWith("mid:"))
				MessageId = MessageId[4..];

			MessageEventArgs? Message = McpXmppExtension.TryGetMessage(MessageId, Remove);
			if (Message is null)
				return new MessageResponse("Message not found.");

			if (Remove)
				this.ResourcesUpdated(User);

			return new MessageResponse(Message);
		}

		/// <summary>
		/// MCP Server Tool to retrieve the first message received over XMPP, and then
		/// remove it from the resource list.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <returns>Message, if found.</returns>
		[McpServerTool(
			"Pop Message",
			"Gets the first message that has been received over XMPP, and then removes it from the resource list.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(GetMessagePrivilege)]
		[return: McpParameter("Result", "Message, if found.")]
		public async Task<MessageResponse> PopMessage(IJsonRpcCall Call)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new MessageResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new MessageResponse("User not authenticated.");

			XmppClient? Client = await this.GetClient(this, Call, User, Session, true);
			if (Client is null)
				return new MessageResponse("MCP XMPP client not available.");

			if (!Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) ||
				McpXmppExtension is null)
			{
				return new MessageResponse("XMPP Client not setup properly.");
			}

			MessageEventArgs? Message = McpXmppExtension.TryGetFirstMessage(true);
			if (Message is null)
				return new MessageResponse("No messages available.");

			this.ResourcesUpdated(User);

			return new MessageResponse(Message);
		}

		private async Task Client_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			if (e.Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				McpXmppExtension.Add(e);

				await this.SendNotification(McpXmppExtension.IsRegistered,
					"Presence Subscription Request received from " + e.FromBareJID);

				RosterItem Contact = e.Client[e.FromBareJID];
				bool Mutual = !(Contact is null) && (Contact.State == SubscriptionState.To ||
					Contact.State == SubscriptionState.Both);

				foreach (string SessionId in McpXmppExtension.SessionIds)
				{
					if (!this.TryGetMcpSession(SessionId, out Session? Session))
						continue;

					PresenceSubscriptionRequest Request = new PresenceSubscriptionRequest()
					{
						Mutual = Mutual
					};

					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						"A Presence Subscription Request was received from " +
						e.FromBareJID + ". You can choose to Accept or Decline " +
						"this request. Below, you can also select if you want " +
						"the presence subscription to be mutual, in both directions.",
						Request, false, Session, 5 * 60 * 1000);

					if (!Result.HasValue)
						continue;

					if (!Result.Value)
						break;

					await e.Accept();

					if (Request.Mutual)
					{
						if (Contact is null ||
							Contact.State == SubscriptionState.None ||
							Contact.State == SubscriptionState.From)
						{
							await e.Client.RequestPresenceSubscription(e.FromBareJID);
						}
					}
					else
					{
						if (!(Contact is null) &&
							(Contact.State == SubscriptionState.To ||
							Contact.State == SubscriptionState.Both))
						{
							await e.Client.RequestPresenceUnsubscription(e.FromBareJID);
						}
					}
				}
			}
		}

		private async Task Client_OnPresenceSubscribed(object Sender, PresenceEventArgs e)
		{
			if (e.Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					"Presence Subscription Request to " + e.FromBareJID + " has been accepted.");
			}
		}

		private async Task Client_OnPresenceUnsubscribed(object Sender, PresenceEventArgs e)
		{
			if (e.Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					"Unsubscribed from Presence Subscription to " + e.FromBareJID + ".");
			}
		}

		private async Task Client_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			await e.Accept();

			if (e.Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					e.FromBareJID + " unsubscribed from your presence.");
			}
		}

		private async Task Client_OnPresence(object Sender, PresenceEventArgs e)
		{
			if (e.Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					"Presence received from " + e.FromBareJID + ": " +
					e.Availability.ToString());
			}
		}

		private async Task Client_OnError(object Sender, Exception e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					"Error logged in XMPP communication: " + e.Message);
			}
		}

		private async Task Client_OnStateChanged(object Sender, XmppState e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				await this.SendNotification(McpXmppExtension.IsRegistered,
					"State of connection changed: " + e.ToString());
			}
		}

		private Task Client_OnRosterItemAdded(object Sender, RosterItem e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				this.RosterResourcesUpdated(McpXmppExtension);
			}

			return Task.CompletedTask;
		}

		private Task Client_OnRosterItemRemoved(object Sender, RosterItem e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				this.RosterResourcesUpdated(McpXmppExtension);
			}

			return Task.CompletedTask;
		}

		private void RosterResourcesUpdated(McpXmppExtension McpXmppExtension)
		{
			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					this.ResourcesUpdated(Session.User);
				}
			}
		}

		private Task Client_OnRosterItemUpdated(object Sender, RosterItem e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				foreach (string SessionId in McpXmppExtension.SessionIds)
				{
					if (this.TryGetMcpSession(SessionId, out Session? Session) &&
						!(Session.User is null))
					{
						this.ResourceUpdated(Session.User,
							RosterItemResource.CreateRosterUri(e.BareJid));
					}
				}
			}

			return Task.CompletedTask;
		}

		private Task Client_OnMessageReceived(object Sender, MessageEventArgs e)
		{
			if (Sender is XmppClient Client &&
				Client.TryGetExtension(out McpXmppExtension? McpXmppExtension) &&
				!(McpXmppExtension is null))
			{
				McpXmppExtension.Register(e);

				foreach (string SessionId in McpXmppExtension.SessionIds)
				{
					if (this.TryGetMcpSession(SessionId, out Session? Session) &&
						!(Session.User is null))
					{
						this.ResourcesUpdated(Session.User);
					}
				}
			}

			return Task.CompletedTask;
		}
	}
}
