using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Mcp.Identity.Resources;
using Waher.Mcp.Identity.Responses;
using Waher.Mcp.Identity.UserInput;
using Waher.Mcp.Xmpp;
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
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Runtime.Collections;
using Waher.Security;

namespace Waher.Mcp.Identity
{
	/// <summary>
	/// MCP Server resource for managing digital identities.
	/// </summary>
	[OAuthResourceName("MCP Server for managing digital identities")]
	[McpScopeRoot("MCP:Identity")]
	public class IdentityMcpServer : HttpMcpServerResource
	{
		internal const string BasePrivilege = OAuthResource.OAuthScopePrivilegePrefix + "MCP.Identity";
		internal const string ToolsPrivilege = BasePrivilege + ".Tools";
		internal const string ResourcesPrivilege = BasePrivilege + ".Resources";
		internal const string ListPrivilege = ResourcesPrivilege + ".List";
		internal const string ReadPrivilege = ResourcesPrivilege + ".Read";
		internal const string ApplyPrivilege = ToolsPrivilege + ".Apply";

		private readonly XmppMcpServer xmppMcpServer;

		/// <summary>
		/// MCP Server resource for managing digital identities.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="XmppMcpServer">XMPP MCP server used to manage XMPP clients.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public IdentityMcpServer(string ResourceName, XmppMcpServer XmppMcpServer,
			ISnifferSet? SnifferSet)
			: this(ResourceName, GetDefaultIcons(), null, XmppMcpServer, SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for managing digital identities.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="XmppMcpServer">XMPP MCP server used to manage XMPP clients.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public IdentityMcpServer(string ResourceName, Icon[] Icons,
			Uri? WebSiteUri, XmppMcpServer XmppMcpServer, ISnifferSet? SnifferSet)
			: this(ResourceName,
				"IdentityMcpServer",      // Name
				"Identity MCP Server",    // Title
				typeof(IdentityMcpServer).Assembly.GetName().Version.ToString(),
				"A Model Context Protocol (MCP) server resource permitting MCP clients " +
				"to apply for, and manage digital legal identities.",
				Icons,
				WebSiteUri,
				"To be able to apply for a digital identity, the client first needs " +
				"an XMPP account. Before applying, check what identity properties are " +
				"are expected. When applying, the user will be elicited to input " +
				"the necessary personal information required to create a digital " +
				"identity. A digital identity has a state, which can be Created, " +
				"Approved, Rejected, Obsoleted or Compromised. An Approved digital " +
				"identity can be used to authenticate the user, and sign digital " +
				"information. To create a Legal Identity, you first Apply for one " +
				"with the Trust Providing hosting the MCP Server. To get it Approved " +
				"the Trust Provider reviews the application, and if valid, will " +
				"Approve it. If it is invalid, the Trust Provider will Reject it. " +
				"Once an application has been Approved, it may be Obsoleted when " +
				"it expires, and Compromised if the identity is reported as " +
				"compromised.",
				XmppMcpServer,
				SnifferSet)
		{
		}

		/// <summary>
		/// MCP Server resource for managing digital identities.
		/// </summary>
		/// <param name="ResourceName">Name of resource.</param>
		/// <param name="Name">Name of server.</param>
		/// <param name="Title">Title of server.</param>
		/// <param name="Version">Version of server.</param>
		/// <param name="Description">Description of server.</param>
		/// <param name="Icons">Icons of server.</param>
		/// <param name="WebSiteUri">Website URI of server.</param>
		/// <param name="Instructions">Instructions for server.</param>
		/// <param name="XmppMcpServer">XMPP MCP server used to manage XMPP clients.</param>
		/// <param name="SnifferSet">Optional sniffer set used to log agent interaction 
		/// with MCP service.</param>
		public IdentityMcpServer(string ResourceName, string Name,
			string Title, string Version, string Description, Icon[] Icons, Uri? WebSiteUri,
			string Instructions, XmppMcpServer XmppMcpServer, ISnifferSet? SnifferSet)
			: base(ResourceName, Name, Title, Version, Description, Icons, WebSiteUri,
				Instructions, SnifferSet)
		{
			this.xmppMcpServer = XmppMcpServer;
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
						"Resources on the MCP server represents digital identities in " +
						"their various states. Each digital identity is recognized " +
						"by the use of the `iotid` URI scheme. Each identity as a " +
						"State, a date when it was Created, a date when it was last " +
						"Updated, a date from when the identity is valid to be used " +
						"and a To date until when it is valid and after which it " +
						"expires.")
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
			if (Session is null || User is null || Uri.Scheme != "iotid")
				return await base.TryGetResource(Call, User, Uri, Session);

			ContractsClient? ContractsClient = await this.GetClient(Call, User, Session, true);
			if (ContractsClient is null)
				return null;

			LegalIdentity Identity = await ContractsClient.GetLegalIdentityAsync(Uri.AbsolutePath);

			return new IdentityResource(Identity);
		}

		/// <summary>
		/// Gets the Contracts Client associated with the XMPP client associated with a 
		/// user. If no client is available, the user will be elicited to provide 
		/// credentials for an XMPP account.
		/// </summary>
		/// <param name="Call">JSON-RPC call originating the request.</param>
		/// <param name="User">Authenticated user object.</param>
		/// <param name="Session">MCP session object.</param>
		/// <param name="CreateIfNotDefined">Create a client if one is not defined.</param>
		/// <returns>Contracts Client, if found.</returns>
		public async Task<ContractsClient?> GetClient(IJsonRpcCall Call, IUser User,
			Session Session, bool CreateIfNotDefined)
		{
			XmppClient? Client = await this.xmppMcpServer.GetClient(this, Call, User,
				Session, CreateIfNotDefined);

			if (Client is null)
				return null;

			if (Client.TryGetExtension(out ContractsClient ContractsClient))
				return ContractsClient;

			if (!CreateIfNotDefined)
				return null;

			string LegalComponent = await Client.FindComponentAsync(Client.Domain,
				ContractsClient.NamespaceLegalIdentitiesCurrent);

			if (string.IsNullOrEmpty(LegalComponent))
				throw new ServiceUnavailableException("No Legal Component found on the XMPP broker.");

			ContractsClient = new ContractsClient(Client, LegalComponent);
			ContractsClient.SetKeySettingsInstance("MCP." + User.UserName, true);
			Client.RegisterExtension(ContractsClient);

			if (!await ContractsClient.LoadKeys(false))
				await ContractsClient.GenerateNewKeys();

			this.ResourcesUpdated(User);

			return ContractsClient;
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

			ContractsClient? ContractsClient = await this.GetClient(Call, User, Session, false);
			if (ContractsClient is null)
				return Array.Empty<Resource>();

			ChunkedList<Resource> Resources = new ChunkedList<Resource>();

			foreach (LegalIdentity Item in await ContractsClient.GetLegalIdentitiesAsync())
				Resources.Add(new IdentityResource(Item));

			return Resources.ToArray();
		}

		/// <summary>
		/// MCP Server Tool to get identity application properties recommended by the
		/// server should be available in identity applications.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Get Identity Application Properties",
			"Gets identity application properties recommended by the server should " +
			"be available in identity applications.",
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(ApplyPrivilege)]
		[return: McpParameter("Result", "Identity application attributes.")]
		public async Task<IdentityApplicationAttributesResponse> GetIdentityApplicationProperties(
			IJsonRpcCall Call)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityApplicationAttributesResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityApplicationAttributesResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityApplicationAttributesResponse("MCP XMPP Contracts client available.");

			IdApplicationAttributesEventArgs e = await Client.GetIdApplicationAttributesAsync();

			return new IdentityApplicationAttributesResponse(e);
		}

		/// <summary>
		/// MCP Server Tool to apply for a new personal identity.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Apply For New Personal Identity",
			"Applies for a new personal identity.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(ApplyPrivilege)]
		[return: McpParameter("Result", "Identity application result.")]
		public async Task<IdentityResponse> ApplyForNewPersonalIdentity(
			IJsonRpcCall Call)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client available.");

			PersonalInformationInput UserInput = new PersonalInformationInput();
			LegalIdentity? Identity = null;
			string? Error = null;

			do
			{
				string Message = "Please provide the necessary personal information " +
					"required to create a digital identity. The information must be " +
					"true, and will be verified. (This input dialog is cancelled " +
					"automatically after 15 minutes.)";

				if (!string.IsNullOrEmpty(Error))
					Message = "Error: " + Error + "\r\n\r\n" + Message;

				bool? Result = await this.ElicitUserInput(Call, Message, UserInput, true,
					Session, 15 * 60 * 1000);

				Error = null;

				if (!Result.HasValue)
					return new IdentityResponse("User did not provide personal information.");

				if (!Result.Value)
					return new IdentityResponse("User cancelled the request.");

				ChunkedList<Property> Properties = new ChunkedList<Property>();

				if (!string.IsNullOrEmpty(UserInput.FirstName))
					Properties.Add(new Property(PersonalInformation.FirstNameTag, UserInput.FirstName));

				if (!string.IsNullOrEmpty(UserInput.MiddleNames))
					Properties.Add(new Property(PersonalInformation.MiddleNamesTag, UserInput.MiddleNames));

				if (!string.IsNullOrEmpty(UserInput.LastNames))
					Properties.Add(new Property(PersonalInformation.LastNamesTag, UserInput.LastNames));

				if (!string.IsNullOrEmpty(UserInput.PersonalNumber))
				{
					if (!UserInput.Country.HasValue)
						Error = "Missing country.";
					else
					{
						PersonalNumberValidationEventArgs e = 
							new PersonalNumberValidationEventArgs(
							UserInput.PersonalNumber, UserInput.Country.Value.ToString());

						await ValidatePersonalNumber.Raise(this, e);

						if (e.IsValid.HasValue)
						{
							if (e.IsValid.Value)
							{
								UserInput.PersonalNumber = e.NormalizedPersonalNumber
									?? UserInput.PersonalNumber;
							}
							else
								Error = "Invalid personal number.";
						}
					}

					Properties.Add(new Property(PersonalInformation.PersonalNumberTag, UserInput.PersonalNumber));
				}

				if (!string.IsNullOrEmpty(UserInput.Address))
					Properties.Add(new Property(PersonalInformation.AddressTag, UserInput.Address));

				if (!string.IsNullOrEmpty(UserInput.Address2))
					Properties.Add(new Property(PersonalInformation.Address2Tag, UserInput.Address2));

				if (!string.IsNullOrEmpty(UserInput.Zip))
					Properties.Add(new Property(PersonalInformation.PostalCodeTag, UserInput.Zip));

				if (!string.IsNullOrEmpty(UserInput.Area))
					Properties.Add(new Property(PersonalInformation.AreaTag, UserInput.Area));

				if (!string.IsNullOrEmpty(UserInput.City))
					Properties.Add(new Property(PersonalInformation.CityTag, UserInput.City));

				if (!string.IsNullOrEmpty(UserInput.Region))
					Properties.Add(new Property(PersonalInformation.RegionTag, UserInput.Region));

				if (UserInput.Country.HasValue)
					Properties.Add(new Property(PersonalInformation.CountryTag, UserInput.Country.Value.ToString()));

				if (UserInput.Nationality.HasValue)
					Properties.Add(new Property(PersonalInformation.NationalityTag, UserInput.Nationality.Value.ToString()));

				if (UserInput.BirthDate.HasValue)
				{
					Properties.Add(new Property(PersonalInformation.BirthDayTag, UserInput.BirthDate.Value.Day.ToString()));
					Properties.Add(new Property(PersonalInformation.BirthMonthTag, UserInput.BirthDate.Value.Month.ToString()));
					Properties.Add(new Property(PersonalInformation.BirthYearTag, UserInput.BirthDate.Value.Year.ToString()));
				}

				if (UserInput.Gender.HasValue)
				{
					Properties.Add(new Property(PersonalInformation.GenderTag,
						UserInput.Gender.Value.ToString()));
				}

				if (Error is null)
				{
					try
					{
						Identity = await Client.ApplyAsync(Properties.ToArray());
					}
					catch (Exception ex)
					{
						Error = Log.UnnestException(ex).Message;
					}
				}
			}
			while (Identity is null);

			this.ResourcesUpdated(User);

			return new IdentityResponse(Identity, "Identity application successfully registered.");
		}

		/// <summary>
		/// Event raised when a personal number needs to be validated.
		/// </summary>
		public static event EventHandlerAsync<PersonalNumberValidationEventArgs>? ValidatePersonalNumber;
	}
}
