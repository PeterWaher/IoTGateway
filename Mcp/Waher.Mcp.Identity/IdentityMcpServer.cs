using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Events;
using Waher.Mcp.Identity.Resources;
using Waher.Mcp.Identity.Responses;
using Waher.Mcp.Identity.UserInput;
using Waher.Mcp.Xmpp;
using Waher.Mcp.Xmpp.Responses;
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
using Waher.Networking.XMPP.HttpFileUpload;
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
		internal const string PromptsPrivilege = BasePrivilege + ".Prompts";
		internal const string ResourcesPrivilege = BasePrivilege + ".Resources";
		internal const string ListPrivilege = ResourcesPrivilege + ".List";
		internal const string ReadPrivilege = ResourcesPrivilege + ".Read";
		internal const string ApplyPrivilege = ToolsPrivilege + ".Apply";
		internal const string ObsoletePrivilege = ToolsPrivilege + ".Obsolete";
		internal const string CompromisePrivilege = ToolsPrivilege + ".Compromise";
		internal const string ReadyForApprovalPrivilege = ToolsPrivilege + ".ReadyForApproval";
		internal const string AttachmentPrivilege = ToolsPrivilege + ".Attachment";
		internal const string AddAttachmentPrivilege = AttachmentPrivilege + ".Add";
		internal const string RemoveAttachmentPrivilege = AttachmentPrivilege + ".Remove";
		internal const string CreatePromptPrivilege = PromptsPrivilege + ".Create";
		internal const string CreatePersonalIdentityPrivilege = CreatePromptPrivilege + ".Create.PersonalId";
		internal const string PetitionPrivilege = ToolsPrivilege + ".Petition";
		internal const string PetitionPeerReviewPrivilege = PetitionPrivilege + ".PeerReview";
		internal const string PetitionIdentityPrivilege = PetitionPrivilege + ".Identity";
		internal const string PetitionContractPrivilege = PetitionPrivilege + ".Contract";
		internal const string PetitionSignaturePrivilege = PetitionPrivilege + ".Signature";
		internal const string ContractPrivilege = ToolsPrivilege + ".Contract";
		internal const string SignContractPrivilege = ContractPrivilege + ".Sign";
		internal const string ProposeContractPrivilege = ContractPrivilege + ".Propose";

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
				"expected. When applying, the user will be elicited to input " +
				"the necessary personal information required to create a digital " +
				"identity. A digital identity has a state, which can be Created, " +
				"Approved, Rejected, Obsoleted or Compromised. An Approved digital " +
				"identity can be used to authenticate the user, and sign digital " +
				"information. To create a Legal Identity, you first Apply for one " +
				"with the Trust Providing hosting the MCP Server. You then attach " +
				"photos, and then flags the application as ready to get approved. " +
				"To get it Approved the Trust Provider reviews the application, and " +
				"if it is valid, the Trust Provider will Approve it. If it is invalid, " +
				"the Trust Provider will Reject it. In both cases, the Trust Provider " +
				"will inform the applicant of the results.Once an application has been " +
				"Approved, it may be Obsoleted when it expires, and Compromised if the " +
				"identity is reported as compromised.",
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
						"their various states, or petitioned smart contracts, or " +
						"contract templates you have used."),
					new KeyValuePair<bool, string>(true,
						"Each identity has a State, a date when it was Created, a date " +
						"when it was last Updated, a date from when the identity is " +
						"valid to be used and a To date until when it is valid and " +
						"after which it expires."),
					new KeyValuePair<bool, string>(true,
						"Each smart contract, or smart contract temaplte is recognized " +
						"by the use of the `iotsc` URI scheme. Each smart contract has " +
						"a State, a date when it was Created, a date when it was last " +
						"updated, a duration, roles, parts who have signed the contract, " +
						"parameters and human-readable and machine-readable contents.")
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

			object? Object = await PetitionCache.TryGetObject(User.UserName, Uri, ContractsClient);

			if (Object is LegalIdentity LegalIdentity)
				return new IdentityResource(LegalIdentity);
			else if (Object is Contract Contract)
				return new ContractResource(Contract);

			switch (Uri.Scheme)
			{
				case "iotid":
					LegalIdentity Identity = await ContractsClient.GetLegalIdentityAsync(Uri.AbsolutePath);
					await PetitionCache.AddLegalIdentity(User.UserName, Identity);
					return new IdentityResource(Identity);

				case "iotsc":
					Contract Contract = await ContractsClient.GetContractAsync(Uri.AbsolutePath);
					await PetitionCache.AddContract(User.UserName, Contract);
					return new ContractResource(Contract);

				default:
					throw new Exception("Identity MCP server does not recognize resource URI schema: " + Uri.Scheme);
			}
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

			string LegalComponent = await Client.FindComponentAsync(Client.Domain,
				ContractsClient.NamespaceLegalIdentitiesCurrent);

			if (string.IsNullOrEmpty(LegalComponent))
			{
				if (!CreateIfNotDefined)
					return null;

				throw new ServiceUnavailableException("No Legal Component found on the XMPP broker.");
			}

			ContractsClient = new ContractsClient(Client, LegalComponent);
			ContractsClient.SetKeySettingsInstance("MCP." + User.UserName, true);
			Client.RegisterExtension(ContractsClient);

			if (!await ContractsClient.LoadKeys(false))
				await ContractsClient.GenerateNewKeys();

			ContractsClient.IdentityUpdated += this.ContractsClient_IdentityUpdated;
			ContractsClient.IdentityReview += this.ContractsClient_IdentityReview;
			ContractsClient.ClientMessage += this.ContractsClient_ClientMessage;
			ContractsClient.ContractCreated += this.ContractsClient_ContractCreated;
			ContractsClient.ContractDeleted += this.ContractsClient_ContractDeleted;
			ContractsClient.ContractProposalReceived += this.ContractsClient_ContractProposalReceived;
			ContractsClient.ContractSigned += this.ContractsClient_ContractSigned;
			ContractsClient.ContractUpdated += this.ContractsClient_ContractUpdated;
			ContractsClient.PetitionClientUrlReceived += this.ContractsClient_PetitionClientUrlReceived;
			ContractsClient.PetitionedContractResponseReceived += this.ContractsClient_PetitionedContractResponseReceived;
			ContractsClient.PetitionedIdentityResponseReceived += this.ContractsClient_PetitionedIdentityResponseReceived;
			ContractsClient.PetitionedPeerReviewIDResponseReceived += this.ContractsClient_PetitionedPeerReviewIDResponseReceived;
			ContractsClient.PetitionedSignatureResponseReceived += this.ContractsClient_PetitionedSignatureResponseReceived;
			ContractsClient.PetitionForContractReceived += this.ContractsClient_PetitionForContractReceived;
			ContractsClient.PetitionForIdentityReceived += this.ContractsClient_PetitionForIdentityReceived;
			ContractsClient.PetitionForPeerReviewIDReceived += this.ContractsClient_PetitionForPeerReviewIDReceived;
			ContractsClient.PetitionForSignatureReceived += this.ContractsClient_PetitionForSignatureReceived;

			if (!Client.TryGetExtension(out HttpFileUploadClient HttpFileUploadClient))
			{
				HttpFileUploadClient UploadClient = new HttpFileUploadClient(Client);
				await UploadClient.DiscoverAsync();
			}

			if (CreateIfNotDefined) // From a tool; resources need updating
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
			HashSet<string> Uris = new HashSet<string>();

			foreach (LegalIdentity Identity in await ContractsClient.GetLegalIdentitiesAsync())
			{
				Uris.Add(Identity.IdUriString);
				Resources.Add(new IdentityResource(Identity));
			}

			foreach (object Object in await PetitionCache.GetCachedObjects(User.UserName, ContractsClient))
			{
				if (Object is LegalIdentity LegalIdentity)
				{
					if (Uris.Contains(LegalIdentity.IdUriString))
						continue;

					Uris.Add(LegalIdentity.IdUriString);
					Resources.Add(new IdentityResource(LegalIdentity));
				}
				else if (Object is Contract Contract)
				{
					if (Uris.Contains(Contract.ContractIdUriString))
						continue;

					Uris.Add(Contract.ContractIdUriString);
					Resources.Add(new ContractResource(Contract));
				}
			}

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
				return new IdentityApplicationAttributesResponse("MCP XMPP Contracts client not available.");

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
			"Applies for a new personal identity. Personal information will be " +
			"elicited from the user.",
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
				return new IdentityResponse("MCP XMPP Contracts client not available.");

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

		private async Task ContractsClient_IdentityUpdated(object Sender, LegalIdentityEventArgs e)
		{
			if (Sender is ContractsClient ContractsClient &&
				ContractsClient.Client.TryGetTag("User", out IUser? User) &&
				!(User is null))
			{
				if (await PetitionCache.AddLegalIdentity(User.UserName, e.Identity))
					this.ResourceUpdated(User, ContractsClient.LegalIdUri(e.Identity.Id));
			}
		}

		/// <summary>
		/// MCP Server Tool to add a photo attachment by eliciting the photo from the user.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource URI to 
		/// which the attachment shall be added.</param>
		/// <param name="PhotoType">Type of photo to be uploaded.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Add Photo Attachment (User Input)",
			"Adds a photo attachment to an identity application, by eliciting the user " +
			"for the photo.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(AddAttachmentPrivilege)]
		[return: McpParameter("Result", "Identity update result.")]
		public async Task<IdentityResponse> AddPhotoAttachmentElicitation(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity " +
			"resource URI to which the attachment shall be added.")]
			string LegalId,

			[McpParameter("PhotoType", "Type of photo to be uploaded.")]
			[McpEnumValue(PhotoType.ProfilePhoto, "Profile photo.")]
			[McpEnumValue(PhotoType.Passport, "Photo of passport.")]
			[McpEnumValue(PhotoType.IdCardFront, "Front of ID card.")]
			[McpEnumValue(PhotoType.IdCardBack, "Back of ID card.")]
			[McpEnumValue(PhotoType.DriverLicenseFront, "Front of Driver's License.")]
			[McpEnumValue(PhotoType.DriverLicenseBack, "Back of Driver's License.")]
			PhotoType PhotoType = PhotoType.ProfilePhoto)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			PhotoInput UserInput;
			LegalIdentity? Identity = null;
			string? Error = null;
			string FileName;
			string OrgMessage;

			LegalId = RemoveUriScheme(LegalId);

			switch (PhotoType)
			{
				case PhotoType.ProfilePhoto:
					UserInput = new UserPhotoInput();
					OrgMessage = "Please provide a recent profile photo to be added to " +
						"the identity application.";
					break;

				case PhotoType.Passport:
					UserInput = new EnvironmentPhotoInput();
					OrgMessage = "Please provide a photo of your passport to be added to " +
						"the identity application.";
					break;

				case PhotoType.IdCardFront:
					UserInput = new EnvironmentPhotoInput();
					OrgMessage = "Please provide a photo of the front of your ID card " +
						"to be added to the identity application.";
					break;

				case PhotoType.IdCardBack:
					UserInput = new EnvironmentPhotoInput();
					OrgMessage = "Please provide a photo of the back of your ID card " +
						"to be added to the identity application.";
					break;

				case PhotoType.DriverLicenseFront:
					UserInput = new EnvironmentPhotoInput();
					OrgMessage = "Please provide a photo of the front of your Driver's " +
						"License to be added to the identity application.";
					break;

				case PhotoType.DriverLicenseBack:
					UserInput = new EnvironmentPhotoInput();
					OrgMessage = "Please provide a photo of the back of your Driver's " +
						"License to be added to the identity application.";
					break;

				default:
					return new IdentityResponse("Invalid photo type: " + PhotoType.ToString());
			}

			OrgMessage += " The photo will be verified. " +
				"(This input dialog is cancelled automatically after 15 minutes.)";

			do
			{
				string Message = OrgMessage;

				if (!string.IsNullOrEmpty(Error))
					Message = "Error: " + Error + "\r\n\r\n" + Message;

				bool? Result = await this.ElicitUserInput(Call, Message, UserInput, true,
					Session, 15 * 60 * 1000);

				if (!Result.HasValue)
					return new IdentityResponse("User did not provide the photo.");

				if (!Result.Value)
					return new IdentityResponse("User cancelled the request.");

				Error = null;
				CustomEncoding? Content = UserInput.GetContent();

				if (Content?.Encoded is null || Content.Encoded.Length == 0)
				{
					Error = "No photo provided.";
					continue;
				}

				if (!Content.ContentType.StartsWith("image/"))
				{
					Error = "Attachment not an image.";
					continue;
				}

				if (!InternetContent.TryGetFileExtension(
					Content.ContentType, out string FileExtension))
				{
					Error = "Unrecognized content type: " + Content.ContentType;
					continue;
				}

				FileName = PhotoType.ToString() + "." + FileExtension;

				try
				{
					Identity = await Client.UploadLegalIdAttachmentAsync(LegalId,
						FileName, Content.Encoded, Content.ContentType);
				}
				catch (Exception ex)
				{
					Error = Log.UnnestException(ex).Message;
				}
			}
			while (Identity is null);

			this.ResourceUpdated(User, ContractsClient.LegalIdUri(LegalId));

			return new IdentityResponse(Identity, "Identity attachment successfully added.");
		}

		/// <summary>
		/// MCP Server Tool to add a photo attachment by providing a BASE64-encoded
		/// binary of the photo together with its corresponding Content-Type.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource URI to 
		/// which the attachment shall be added.</param>
		/// <param name="Base64Photo">BASE64-encoded photo to be uploaded.</param>
		/// <param name="ContentTypePhoto">Content-Type of the photo to be uploaded.</param>
		/// <param name="PhotoType">Type of photo to be uploaded.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Add Photo Attachment (BASE64 upload)",
			"Adds a photo attachment to an identity application, by providing a " +
			"BASE64-encoded binary of the photo together with its corresponding " +
			"Content-Type.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(AddAttachmentPrivilege)]
		[return: McpParameter("Result", "Identity update result.")]
		public async Task<IdentityResponse> AddPhotoAttachmentBase64Upload(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity " +
			"resource URI to which the attachment shall be added.")]
			string LegalId,

			[McpStringParameter("Base64Photo", "BASE64-encoded photo to be uploaded.")]
			string Base64Photo,

			[McpStringParameter("ContentTypePhoto", "Content-Type of the photo to be uploaded.")]
			string ContentTypePhoto,

			[McpParameter("PhotoType", "Type of photo to be uploaded.")]
			[McpEnumValue(PhotoType.ProfilePhoto, "Profile photo.")]
			[McpEnumValue(PhotoType.Passport, "Photo of passport.")]
			[McpEnumValue(PhotoType.IdCardFront, "Front of ID card.")]
			[McpEnumValue(PhotoType.IdCardBack, "Back of ID card.")]
			[McpEnumValue(PhotoType.DriverLicenseFront, "Front of Driver's License.")]
			[McpEnumValue(PhotoType.DriverLicenseBack, "Back of Driver's License.")]
			PhotoType PhotoType = PhotoType.ProfilePhoto)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			if (!ContentTypePhoto.StartsWith("image/"))
				return new IdentityResponse("Attachment must be an image.");

			if (!InternetContent.TryGetFileExtension(
				ContentTypePhoto, out string FileExtension))
			{
				return new IdentityResponse("Unrecognized content type: " + ContentTypePhoto);
			}

			LegalId = RemoveUriScheme(LegalId);

			string FileName = PhotoType.ToString() + "." + FileExtension;
			byte[] Bin;

			try
			{
				Bin = Convert.FromBase64String(Base64Photo);
				ContentResponse Decoded = await InternetContent.DecodeAsync(ContentTypePhoto, Bin, null);
				if (Decoded.HasError)
					return new IdentityResponse("Invalid photo: " + Decoded.Error);
			}
			catch (Exception ex)
			{
				return new IdentityResponse("Invalid photo: " + Log.UnnestException(ex).Message);
			}

			try
			{
				LegalIdentity Identity = await Client.UploadLegalIdAttachmentAsync(LegalId,
					FileName, Bin, ContentTypePhoto);

				return new IdentityResponse(Identity, "Identity attachment successfully added.");
			}
			catch (Exception ex)
			{
				return new IdentityResponse("Unable to add attachment: " +
					Log.UnnestException(ex).Message);
			}
		}

		/// <summary>
		/// MCP Server Tool to add a photo attachment by providing a URL to the photo
		/// to be downloaded and then added as an attachment.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource URI to 
		/// which the attachment shall be added.</param>
		/// <param name="PhotoUrl">URL of the photo to be downloaded.</param>
		/// <param name="PhotoType">Type of photo to be uploaded.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Add Photo Attachment (URL Download)",
			"Adds a photo attachment to an identity application, by providing a URL to " +
			"the photo to be downloaded and then added as an attachment.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(AddAttachmentPrivilege)]
		[return: McpParameter("Result", "Identity update result.")]
		public async Task<IdentityResponse> AddPhotoAttachmentUrlDownload(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity " +
			"resource URI to which the attachment shall be added.")]
			string LegalId,

			[McpUriParameter("PhotoUrl", "URL of the photo to be downloaded.")]
			Uri PhotoUrl,

			[McpParameter("PhotoType", "Type of photo to be uploaded.")]
			[McpEnumValue(PhotoType.ProfilePhoto, "Profile photo.")]
			[McpEnumValue(PhotoType.Passport, "Photo of passport.")]
			[McpEnumValue(PhotoType.IdCardFront, "Front of ID card.")]
			[McpEnumValue(PhotoType.IdCardBack, "Back of ID card.")]
			[McpEnumValue(PhotoType.DriverLicenseFront, "Front of Driver's License.")]
			[McpEnumValue(PhotoType.DriverLicenseBack, "Back of Driver's License.")]
			PhotoType PhotoType = PhotoType.ProfilePhoto)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			ContentResponse Content = await InternetContent.GetAsync(PhotoUrl,
				new KeyValuePair<string, string>("Accept", "image/*"));

			if (Content.HasError)
				return new IdentityResponse("Unable to download photo: " + Content.Error);

			if (!Content.ContentType.StartsWith("image/"))
				return new IdentityResponse("Downloaded content not an image.");

			if (!InternetContent.TryGetFileExtension(
				Content.ContentType, out string FileExtension))
			{
				return new IdentityResponse("Unrecognized content type: " + Content.ContentType);
			}

			LegalId = RemoveUriScheme(LegalId);

			string FileName = PhotoType.ToString() + "." + FileExtension;

			try
			{
				LegalIdentity Identity = await Client.UploadLegalIdAttachmentAsync(LegalId,
					FileName, Content.Encoded, Content.ContentType);

				return new IdentityResponse(Identity, "Identity attachment successfully added.");
			}
			catch (Exception ex)
			{
				return new IdentityResponse("Unable to add attachment: " +
					Log.UnnestException(ex).Message);
			}
		}

		/// <summary>
		/// MCP Server Tool to remove an attachment from an identity application.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="AttachmentId">Attachment ID to be removed.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Remove Attachment",
			"Removes an attachment from an identity application.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(RemoveAttachmentPrivilege)]
		[return: McpParameter("Result", "Identity update result.")]
		public async Task<IdentityResponse> RemoveAttachment(
			IJsonRpcCall Call,

			[McpStringParameter("AttachmentId", "Attachment ID to be removed.")]
			string AttachmentId)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			try
			{
				LegalIdentity Identity = await Client.RemoveLegalIdAttachmentAsync(AttachmentId);

				return new IdentityResponse(Identity, "Identity attachment successfully removed.");
			}
			catch (Exception ex)
			{
				return new IdentityResponse("Unable to remove attachment: " +
					Log.UnnestException(ex).Message);
			}
		}

		private static string RemoveUriScheme(string LegalId)
		{
			if (LegalId.StartsWith("iotid:", StringComparison.InvariantCultureIgnoreCase))
				LegalId = LegalId[6..];

			return LegalId;
		}

		/// <summary>
		/// MCP Server Tool to obsolete an identity or identity application.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource 
		/// URI to obsolete.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Obsolete Identity",
			"Obsoletes one of the identities or identity applications registered by " +
			"the user.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(ObsoletePrivilege)]
		[return: McpParameter("Result", "Identity obsoletion result.")]
		public async Task<IdentityResponse> ObsoleteIdentity(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity resource URI to obsolete.")]
			string LegalId)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			LegalId = RemoveUriScheme(LegalId);

			try
			{
				LegalIdentity? Identity = await Client.ObsoleteLegalIdentityAsync(LegalId);
				return new IdentityResponse(Identity, "Identity obsoleted.");
			}
			catch (Exception ex)
			{
				return new IdentityResponse(Log.UnnestException(ex).Message);
			}
		}

		/// <summary>
		/// MCP Server Tool to report an identity or identity application as compromised.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource 
		/// URI to report as compromised.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Compromise Identity",
			"Reports one of the identities or identity applications registered by " +
			"the user as compromised.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(CompromisePrivilege)]
		[return: McpParameter("Result", "Identity compromised report result.")]
		public async Task<IdentityResponse> CompromiseIdentity(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity resource URI to report as compromised.")]
			string LegalId)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new IdentityResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new IdentityResponse("MCP XMPP Contracts client not available.");

			LegalId = RemoveUriScheme(LegalId);

			try
			{
				LegalIdentity? Identity = await Client.CompromisedLegalIdentityAsync(LegalId);
				return new IdentityResponse(Identity, "Identity reported as compromised.");
			}
			catch (Exception ex)
			{
				return new IdentityResponse(Log.UnnestException(ex).Message);
			}
		}

		/// <summary>
		/// MCP Server Tool to report an identity application as ready for approval.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Legal Identity Identifier or Identity resource 
		/// URI to report as ready for approval.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Ready For Approval Identity",
			"Reports one of the identities or identity applications registered by " +
			"the user as ready for approval.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			true,   // CanDestroyEnvironment
			false,  // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(ReadyForApprovalPrivilege)]
		[return: McpParameter("Result", "Identity report result.")]
		public async Task<GenericResponse> ReadyForApproval(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Legal Identity Identifier or Identity resource URI to report as ready for approval.")]
			string LegalId)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new IdentityResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP Contracts client not available.");

			LegalId = RemoveUriScheme(LegalId);

			try
			{
				Client.Client.SetTag(LegalId, new ApplicationTag(Call, Session, User));

				await Client.ReadyForApprovalAsync(LegalId);
				return new GenericResponse(true, "Identity reported as ready for approval.");
			}
			catch (Exception ex)
			{
				return new GenericResponse(false, Log.UnnestException(ex).Message);
			}
		}

		private class ApplicationTag
		{
			public ApplicationTag(IJsonRpcCall Call, Session Session, IUser User)
			{
				this.Call = Call;
				this.Session = Session;
				this.User = User;
			}

			public IJsonRpcCall Call;
			public Session Session;
			public IUser User;
		}

		/// <summary>
		/// MCP Server Prompt to create a new personal legal identity for the user.
		/// </summary>
		/// <returns>Prompt messages</returns>
		[McpServerPrompt("Create Personal Legal Identity",          // Title
			"Creates a new Legal Personal Identity for the user.",  // Description
			"")]                                                    // IconsMethod, use default icons
		[RequiredPrivilege(CreatePersonalIdentityPrivilege)]
		public PromptMessage[] CreatePersonalLegalIdentity()
		{
			return new PromptMessage[]
			{
				new PromptMessage(McpRole.User,
					"Create a Personal Legal Identity for me."),
				new PromptMessage(McpRole.Assistant,
					"To create a Personal Legal Identity, follow these steps: You first " +
					"need to Apply for a Personal Legal Identity. This will elicit " +
					"sensitive personal information from the user. Once the Identity " +
					"Application has been registered, a resource URI will be created " +
					"correspondingly. The successful response to the identity " +
					"application call will also contain a copy of the identity " +
					"object created. This object contains the identifier of the identity " +
					"application."),
				new PromptMessage(McpRole.Assistant,
					"The next step is to add a Profile Photo of the user, as a " +
					"photo attachment to the registered identity application. You " +
					"reference the application either using the identity identifier " +
					"received from in the result to the application call, or from the " +
					"identity resource URI generated as a result. You can choose " +
					"either to elicit the user for a photo, upload a photo encoded using " +
					"BASE64 or provide an URL to the photo. Once the photo has been attached, the "),
				new PromptMessage(McpRole.Assistant,
					"Once the profile photo has been attached, you need to upload proof " +
					"of the validity of the claims in the application and the profile " +
					"photo. This can be done either by taking a photo of a passport, " +
					"two photos of the front and back of an ID card, or two photos " +
					"of the front and back of a Driver's License. This or these photos " +
					"also need to be added as attachments to the identity application."),
				new PromptMessage(McpRole.Assistant,
					"Once the photos have been uploaded, you report the identity " +
					"application as being ready for approval. This will start a process " +
					"review of the applcation. The application can either be automatically " +
					"approved, automatically rejected, or passed on to manual review. " +
					"Messages can also be sent indicating the state of the review, which " +
					"claims have been approved, which ones have been rejected, amd which " +
					"ones were unable to be validated.")
			};
		}

		private async Task ContractsClient_ClientMessage(object Sender, ClientMessageEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetTag(e.LegalId, out ApplicationTag Tag))
				return;

			ContractsClient.Client.RemoveTag(e.LegalId);

			StringBuilder sb = new StringBuilder();

			switch (e.ValidationErrorType)
			{
				case ValidationErrorType.Client:
					if (e.Code != "ManualReview")
					{
						sb.Append("Some of the information provided could not be verified, " +
							"or was incorrect. ");
					}
					break;

				case ValidationErrorType.Service:
					sb.Append("A service on the broker failed. ");
					break;

				case ValidationErrorType.Server:
					sb.Append("An error occurred on the broker. ");
					break;
			}

			sb.Append("Review the information below, and try again.");

			if (!string.IsNullOrEmpty(e.Code))
			{
				sb.Append(" (Error Code: ");
				sb.Append(e.Code);
				sb.Append(')');
			}

			sb.AppendLine();
			sb.AppendLine();

			sb.AppendLine(e.Body);

			AppendApplicationInfo(sb, e);

			await this.ElicitUserInput(Tag.Call, sb.ToString(), new Acknowledgement(),
				false, Tag.Session, 5 * 60 * 1000);
		}

		private static void AppendApplicationInfo(StringBuilder sb, IdentityReviewEventArgs e)
		{
			if (e.HasErrors)
			{
				sb.AppendLine();
				sb.AppendLine("Errors reported:");
				sb.AppendLine();

				foreach (ValidationError Error in e.ValidationErrors)
				{
					sb.Append("* ");

					switch (Error.ErrorType)
					{
						case ValidationErrorType.Client:
							sb.Append("Client error: ");
							break;

						case ValidationErrorType.Service:
							sb.Append("Service error: ");
							break;

						case ValidationErrorType.Server:
							sb.Append("Server error: ");
							break;
					}

					sb.Append(Error.ErrorMessage);

					bool Parenthesis = false;
					bool First = true;

					if (!string.IsNullOrEmpty(Error.ErrorCode))
					{
						sb.Append(" (Error Code: ");
						sb.Append(Error.ErrorCode);

						Parenthesis = true;
						First = false;
					}

					if (!(Error.Tags is null) && Error.Tags.Length > 0)
					{
						if (!Parenthesis)
						{
							sb.Append(" (");
							Parenthesis = true;
						}

						foreach (KeyValuePair<string, object> P in Error.Tags)
						{
							if (First)
								First = false;
							else
								sb.Append(", ");

							sb.Append(P.Key);
							sb.Append("=");
							sb.Append(P.Value?.ToString());
						}
					}

					if (Parenthesis)
						sb.Append(')');

					sb.Append(" (");
					sb.Append(Error.Service);
					sb.AppendLine(")");
				}
			}

			if ((e.InvalidClaims?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Invalid claims:");
				sb.AppendLine();

				foreach (InvalidClaim Claim in e.InvalidClaims!)
				{
					sb.Append("* ");
					sb.Append(Claim.Claim);
					sb.Append(": ");
					sb.Append(Claim.Reason);

					if (!string.IsNullOrEmpty(Claim.ReasonCode))
					{
						sb.Append(" (");
						sb.Append(Claim.ReasonCode);
						sb.Append(')');
					}

					sb.Append(" (");
					sb.Append(Claim.Service);
					sb.AppendLine(")");
				}
			}

			if ((e.InvalidPhotos?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Invalid photos:");
				sb.AppendLine();

				foreach (InvalidPhoto Photo in e.InvalidPhotos!)
				{
					sb.Append("* ");
					sb.Append(Photo.FileName);
					sb.Append(": ");
					sb.Append(Photo.Reason);

					if (!string.IsNullOrEmpty(Photo.ReasonCode))
					{
						sb.Append(" (");
						sb.Append(Photo.ReasonCode);
						sb.Append(')');
					}

					sb.Append(" (");
					sb.Append(Photo.Service);
					sb.AppendLine(")");
				}
			}

			if ((e.UnvalidatedClaims?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Unvalidated claims:");
				sb.AppendLine();

				foreach (string Claim in e.UnvalidatedClaims!)
				{
					sb.Append("* ");
					sb.AppendLine(Claim);
				}
			}

			if ((e.UnvalidatedPhotos?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine("Unvalidated photos:");
				sb.AppendLine();

				foreach (string Photo in e.UnvalidatedPhotos!)
				{
					sb.Append("* ");
					sb.AppendLine(Photo);
				}
			}

			if (e.HasValidatedClaims)
			{
				sb.AppendLine();
				sb.AppendLine("Valid claims:");
				sb.AppendLine();

				foreach (ValidClaim Claim in e.ValidClaims)
				{
					sb.Append("* ");
					sb.Append(Claim.Claim);
					sb.Append(" (");
					sb.Append(Claim.Service);
					sb.AppendLine(")");
				}
			}

			if (e.HasValidatedPhotos)
			{
				sb.AppendLine();
				sb.AppendLine("Valid photos:");
				sb.AppendLine();

				foreach (ValidPhoto Photo in e.ValidPhotos)
				{
					sb.Append("* ");
					sb.Append(Photo.FileName);
					sb.Append(" (");
					sb.Append(Photo.Service);
					sb.AppendLine(")");
				}
			}

			if (e.HasPotentialClaims)
			{
				sb.AppendLine();
				sb.AppendLine("Potential claims that could be added:");
				sb.AppendLine();

				foreach (PotentialClaim Claim in e.PotentialClaims)
				{
					sb.Append("* ");
					sb.Append(Claim.Claim);
					sb.Append(": ");
					sb.Append(Claim.Value);
					sb.Append(" (");
					sb.Append(Claim.Service);
					sb.AppendLine(")");
				}
			}
		}

		private async Task ContractsClient_IdentityReview(object Sender, IdentityReviewEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetTag(e.LegalId, out ApplicationTag Tag))
				return;

			ContractsClient.Client.RemoveTag(e.LegalId);

			StringBuilder sb = new StringBuilder();

			sb.Append("A review of the application has been completed.");
			AppendApplicationInfo(sb, e);

			await this.ElicitUserInput(Tag.Call, sb.ToString(), new Acknowledgement(),
				false, Tag.Session, 5 * 60 * 1000);
		}

		private async Task ContractsClient_PetitionForIdentityReceived(object Sender,
			LegalIdentityPetitionEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append("A petition for one of your Legal Identities (");
			sb.Append(e.RequestedIdentityId);
			sb.Append(") has been received: ");
			sb.Append(e.Purpose);

			AppendQuestionAndRequestor(sb, e.RequestorIdentity, e.RequestorFullJid,
				e.Properties, e.Attachments, e.ClientEndpoint);

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					Petition Petition = new Petition();
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), Petition, true, Session, 5 * 60 * 1000);

					if (Result.HasValue)
					{
						await ContractsClient.PetitionIdentityResponseAsync(
							e.RequestedIdentityId, e.PetitionId, e.RequestorFullJid,
							Result.Value);
						break;
					}
				}
			}
		}

		private static void AppendQuestionAndRequestor(StringBuilder sb, LegalIdentity Identity,
			string FullJid, string RemoteEndpoint)
		{
			AppendQuestionAndRequestor(sb, Identity, FullJid,
				Array.Empty<string>(), Array.Empty<string>(), RemoteEndpoint);
		}

		private static void AppendQuestionAndRequestor(StringBuilder sb, LegalIdentity Identity,
			string FullJid, string[] Properties, string[] Attachments, string RemoteEndpoint)
		{
			AppendQuestionAndRequestor(sb, Identity,
				"Do you want to accept or decline the request?", "requestor", FullJid,
				Properties, Attachments, RemoteEndpoint);
		}

		private static void AppendQuestionAndRequestor(StringBuilder sb, LegalIdentity Identity,
			string Question, string Title, string FullJid, string[] Properties,
			string[] Attachments, string RemoteEndpoint)
		{
			if (!string.IsNullOrEmpty(RemoteEndpoint))
			{
				sb.Append(" (Source: ");
				sb.Append(RemoteEndpoint);
				sb.Append(')');
			}

			if (!string.IsNullOrEmpty(Question))
			{
				sb.Append(' ');
				sb.Append(Question);
			}

			sb.Append(" Information about the ");
			sb.Append(Title);
			sb.Append(" follows:");

			bool JidIncluded = false;

			foreach (Property P in Identity.Properties)
			{
				JidIncluded |= P.Name == PersonalInformation.JidTag;

				sb.AppendLine();
				sb.Append(P.Name);
				sb.Append(": ");
				sb.Append(P.Value);
			}

			if (!JidIncluded && !string.IsNullOrEmpty(FullJid))
			{
				sb.AppendLine();
				sb.Append(PersonalInformation.JidTag);
				sb.Append(": ");
				sb.Append(XmppClient.GetBareJID(FullJid));
			}

			if ((Properties?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Only these properties of your identity will be returned:");

				foreach (string Name in Properties!)
				{
					sb.AppendLine();
					sb.Append("* ");
					sb.Append(Name);
				}
			}

			if ((Attachments?.Length ?? 0) > 0)
			{
				sb.AppendLine();
				sb.AppendLine();
				sb.AppendLine("Only these attachments of your identity will be returned:");

				foreach (string Name in Attachments!)
				{
					sb.AppendLine();
					sb.Append("* ");
					sb.Append(Name);
				}
			}
		}

		private async Task ContractsClient_PetitionForSignatureReceived(object Sender, SignaturePetitionEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append("A petition for a digital signature has been received: ");
			sb.Append(e.Purpose);

			AppendQuestionAndRequestor(sb, e.RequestorIdentity, e.RequestorFullJid,
				e.Properties, e.Attachments, e.ClientEndpoint);

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					Petition Petition = new Petition();
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), Petition, true, Session, 5 * 60 * 1000);

					if (Result.HasValue)
					{
						byte[] Signature;

						if (Result.Value)
						{
							Signature = await ContractsClient.SignAsync(e.ContentToSign,
								SignWith.CurrentKeys);
						}
						else
							Signature = Array.Empty<byte>();

						await ContractsClient.PetitionSignatureResponseAsync(
							e.SignatoryIdentityId, e.ContentToSign, Signature, e.PetitionId,
							e.RequestorFullJid, Result.Value);
						break;
					}
				}
			}
		}

		private async Task ContractsClient_PetitionForPeerReviewIDReceived(object Sender, SignaturePetitionEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			await ContractsClient.PetitionSignatureResponseAsync(
				e.SignatoryIdentityId, e.ContentToSign, Array.Empty<byte>(), e.PetitionId,
				e.RequestorFullJid, false);
		}

		private async Task ContractsClient_PetitionForContractReceived(object Sender, ContractPetitionEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			sb.Append("A petition for one of your Smart Contracts (");
			sb.Append(e.RequestedContractId);
			sb.Append(") has been received: ");
			sb.Append(e.Purpose);

			AppendQuestionAndRequestor(sb, e.RequestorIdentity, e.RequestorFullJid,
				e.Properties, e.Attachments, e.ClientEndpoint);

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					Petition Petition = new Petition();
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), Petition, true, Session, 5 * 60 * 1000);

					if (Result.HasValue)
					{
						await ContractsClient.PetitionContractResponseAsync(
							e.RequestedContractId, e.PetitionId, e.RequestorFullJid,
							Result.Value);
						break;
					}
				}
			}
		}

		private async Task ContractsClient_PetitionClientUrlReceived(object Sender, PetitionClientUrlEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			string Message = "The petition requests you to enter additional information " +
				"using an online form.";

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					if (await this.ElicitOpenUrl(McpXmppExtension.FirstCall,
						Message, e.ClientUrl, Session))
					{
						break;
					}

					if (await this.ElicitUserInput(McpXmppExtension.FirstCall, Message,
						new OpenUrl(e.ClientUrl), false, Session, 5 * 60 * 1000) ?? false)
					{
						break;
					}
				}
			}
		}

		/// <summary>
		/// MCP Server Tool to get Identity Peer Review providers.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Get Peer Review Providers",
			"Gets a list of peer review providers that can be used to review identity applications.",
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(PetitionPeerReviewPrivilege)]
		[return: McpParameter("Result", "Available peer review providers.")]
		public async Task<PeerReviewProvidersResponse> GetPeerReviewProviders(
			IJsonRpcCall Call)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new PeerReviewProvidersResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new PeerReviewProvidersResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new PeerReviewProvidersResponse("MCP XMPP Contracts client not available.");

			ServiceProviderWithLegalId[] Providers = await Client.GetPeerReviewIdServiceProvidersAsync();
			int i, c = Providers.Length;
			PeerReviewProvider[] Providers2 = new PeerReviewProvider[c];

			for (i = 0; i < c; i++)
				Providers2[i] = new PeerReviewProvider(Providers[i]);

			return new PeerReviewProvidersResponse(Providers2);
		}

		/// <summary>
		/// MCP Server Tool to select an internal Peer Review provider.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="Id">Peer Review Provider ID.</param>
		/// <param name="Type">Peer Review Provider Type.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Select Peer Review Provider",
			"Selects an internal Peer Review provider.",
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			true,   // Idempotent
			false)] // OpenWorldAccess
		[RequiredPrivilege(PetitionPeerReviewPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<GenericResponse> SelectPeerReviewProvider(
			IJsonRpcCall Call,

			[McpStringParameter("Id", "Peer Review Provider ID.")]
			string Id,

			[McpStringParameter("Type", "Peer Review Provider Type.")]
			string Type)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP Contracts client not available.");

			await Client.SelectPeerReviewServiceAsync(Id, Type);

			return new GenericResponse(true, "Peer Review provider selected.");
		}

		/// <summary>
		/// MCP Server Tool to petition a peer for a review of an identity application.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Identifier of Legal Identity to send the petition 
		/// to.</param>
		/// <param name="Purpose">Message to recipient of petition, explaining the purpose 
		/// of the review.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Petition Peer Review",
			"Sends a petition to a peer for a review of an identity application.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(PetitionPeerReviewPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<PetitionResponse> PetitionPeerReview(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Identifier of Legal Identity to send the petition to.")]
			string LegalId,

			[McpStringParameter("Purpose", "Message to recipient of petition, explaining the purpose of the review.", 1, 1024)]
			string Purpose)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new PetitionResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new PetitionResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new PetitionResponse("MCP XMPP Contracts client not available.");

			LegalIdentity? LatestCreated = await GetLatestApplication(Client);
			if (LatestCreated is null)
				return new PetitionResponse("No current Legal Identity application found.");

			string PetitionId = Guid.NewGuid().ToString();

			await Client.PetitionPeerReviewIDAsync(LegalId, LatestCreated, PetitionId, Purpose);

			return new PetitionResponse(PetitionId, "Peer Review petition sent.");
		}

		private static async Task<LegalIdentity?> GetLatestApplication(ContractsClient Client)
		{
			LegalIdentity[] Identities = await Client.GetLegalIdentitiesAsync();
			LegalIdentity? LatestCreated = null;

			foreach (LegalIdentity Identity in Identities)
			{
				if (Identity.State != Networking.XMPP.Contracts.IdentityState.Created)
					continue;

				if (LatestCreated is null || Identity.Created > LatestCreated.Created)
					LatestCreated = Identity;
			}

			return LatestCreated;
		}

		/// <summary>
		/// MCP Server Prompt to request a peer review of a current identity application.
		/// </summary>
		/// <returns>Prompt messages</returns>
		[McpServerPrompt("Request Peer Review",          // Title
			"Requests a Peer Review of a current Identity Application.",  // Description
			"")]                                                    // IconsMethod, use default icons
		[RequiredPrivilege(CreatePersonalIdentityPrivilege)]
		[RequiredPrivilege(PetitionPeerReviewPrivilege)]
		public PromptMessage[] RequestPeerReview()
		{
			return new PromptMessage[]
			{
				new PromptMessage(McpRole.User,
					"Request a peer review of my Identity Application."),
				new PromptMessage(McpRole.Assistant,
					"One way to get an identity application approved, is Peer Review. " +
					"If the XMPP broker to which the client is connected supports " +
					"and permits peer review, it will accept a given number of peer " +
					"review results as proof the identity application is valid, and " +
					"therefore approve it. To request a peer review, there are different " +
					"options to take."),
				new PromptMessage(McpRole.Assistant,
					"One way is to beforehand know the identifier of the Legal Identity of " +
					"the Peer of which the peer review will be petitioned. In that case, " +
					"requesting a Peer Review is as simple as petitioning the peer for a " +
					"peer review, using the tool for that purpose."),
				new PromptMessage(McpRole.Assistant,
					"If such an identifier is not known, you can check for featured peer " +
					"reviewers, provided or referenced by the broker itself. If selecting " +
					"this method, first use the tool to get Peer Review Providers from " +
					"the broker. Each provider can be either External (i.e. referenced " +
					"to by the Broker) or Internal (i.e. hosted on or in conjunction " +
					"with the Broker itself). All providers also include an identifier " +
					"of the Legal Identity to which the peer review should be " +
					"petitioned. If an external peer reviewer is selected, you can " +
					"continue directly to petitioning the peer review from the provider " +
					"using the Peer Review petitioning tool. If the provider is Internal " +
					"however, meaning, it is hosted by the Broker itself, and therefore " +
					"shares the Legal Identifier of the broker itself, the provider " +
					"must first be selected. Selecting a peer review provider is done " +
					"using the tool for that purpose. After the peer review provider " +
					"has been selected, you can continue to petition the peer review " +
					"from the Legal Identity identifier provided for the provider."),
				new PromptMessage(McpRole.Assistant,
					"A petition does not guarantee that a peer review will actually " +
					"be performed. It is only a request, and the request may be " +
					"performed at a later time. Any results of the peer review will " +
					"be notified to you, when the result is received.")
			};
		}

		private async Task ContractsClient_PetitionedPeerReviewIDResponseReceived(object Sender,
			SignaturePetitionResponseEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			LegalIdentity? ReviewedIdentity = await GetLatestApplication(ContractsClient);
			if (ReviewedIdentity is null)
				return;

			StringBuilder sb = new StringBuilder();
			LegalIdentity? ReviewerIdentity = null;
			object? UserInput;

			if (e.Response)
			{
				ReviewerIdentity = e.RequestedIdentity;
				if (ReviewerIdentity is null)
					return;

				StringBuilder Xml = new StringBuilder();
				ReviewedIdentity.Serialize(Xml, true, true, true, true, true, true, true);
				string s = Xml.ToString();
				byte[] Data = Encoding.UTF8.GetBytes(s);

				bool? Valid = ContractsClient.ValidateSignature(ReviewerIdentity, Data, e.Signature);
				if (!Valid.HasValue || !Valid.Value)
				{
					Log.Warning("A peer review was rejected as the signature could not be validated.",
						new KeyValuePair<string, object>("PetitionId", e.PetitionId),
						new KeyValuePair<string, object>("Reviewed", ReviewedIdentity.Id),
						new KeyValuePair<string, object>("Reviewer", ReviewerIdentity.Id));
					return;
				}

				UserInput = new Petition();

				sb.Append("A peer review (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been successfully returned. ");
				sb.Append("The review has been uploaded as an attachment to the ");
				sb.Append("application. ");

				AppendQuestionAndRequestor(sb, ReviewerIdentity,
					"Do you want to upload it as an attachment? Once sufficient " +
					"successful peer reviews have been uploaded, the application " +
					"will become approved.", "peer", string.Empty, Array.Empty<string>(),
					Array.Empty<string>(), e.ClientEndpoint);
			}
			else
			{
				sb.Append("Your peer review petition (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been declined by the recipient.");

				UserInput = new Acknowledgement();
			}

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), UserInput, true, Session, 5 * 60 * 1000);

					if (Result.HasValue && Result.Value)
					{
						if (!(ReviewerIdentity is null))
						{
							if (await PetitionCache.AddLegalIdentity(Session.UserName, ReviewerIdentity))
								this.ResourcesUpdated(Session.User);
						}

						if (UserInput is Petition Petition &&
							Petition.Accept.HasValue &&
							Petition.Accept.Value &&
							!(ReviewerIdentity is null))
						{
							ReviewedIdentity = await ContractsClient.AddPeerReviewIDAttachment(
								ReviewedIdentity, ReviewerIdentity, e.Signature);
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// MCP Server Tool to petition a user for one of its Legal Identities.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Identifier of Legal Identity to send the petition 
		/// to.</param>
		/// <param name="Purpose">Message to recipient of petition, explaining the purpose 
		/// of the review.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Petition Legal Identity",
			"Sends a petition to a user for one of its legal identities.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(PetitionIdentityPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<PetitionResponse> PetitionLegalIdentity(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Identifier of Legal Identity to petition.")]
			string LegalId,

			[McpStringParameter("Purpose", "A message to the recipient of the petition, explaining the purpose of the petition.", 1, 1024)]
			string Purpose)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new PetitionResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new PetitionResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new PetitionResponse("MCP XMPP Contracts client not available.");

			try
			{
				LegalIdentity? Identity = await PetitionCache.TryGetLegalIdentity(
					User.UserName, LegalId);

				if (!(Identity is null))
					return new PetitionResponse(Identity.ToJson(), "Legal Identity available in QuickResponse property. Taken from petition cache. No petition sent.");

				Identity = await Client.GetLegalIdentityAsync(LegalId);

				if (!(Identity is null))
				{
					await PetitionCache.AddLegalIdentity(User.UserName, Identity);
					return new PetitionResponse(Identity.ToJson(), "Legal Identity available in QuickResponse property. Access already granted. No petition sent.");
				}
			}
			catch (Exception)
			{
				// Access not authorized; send petition.
			}

			string PetitionId = Guid.NewGuid().ToString();

			await Client.PetitionIdentityAsync(LegalId, PetitionId, Purpose);

			return new PetitionResponse(PetitionId, "Legal Identity petition sent.");
		}

		private async Task ContractsClient_PetitionedIdentityResponseReceived(object Sender,
			LegalIdentityPetitionResponseEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			if (e.Response)
			{
				sb.Append("A Legal Identity you petitioned (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been successfully returned. ");

				AppendQuestionAndRequestor(sb, e.RequestedIdentity, string.Empty,
					"peer", string.Empty, Array.Empty<string>(), Array.Empty<string>(),
					e.ClientEndpoint);
			}
			else
			{
				sb.Append("Your legal identity petition (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been declined by the recipient.");
			}

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), new Acknowledgement(), true, Session, 5 * 60 * 1000);

					if (Result.HasValue && Result.Value)
					{
						if (!(e.RequestedIdentity is null))
						{
							if (await PetitionCache.AddLegalIdentity(Session.UserName, e.RequestedIdentity))
								this.ResourcesUpdated(Session.User);
						}

						break;
					}
				}
			}
		}

		/// <summary>
		/// MCP Server Tool to petition a user for a digital signature.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="LegalId">Identifier of Legal Identity to send the petition 
		/// to.</param>
		/// <param name="Content">BASE64-encoded content to sign.</param>
		/// <param name="Purpose">Message to recipient of petition, explaining the purpose 
		/// of the review.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Petition Digital Signature",
			"Sends a petition to a user for a digital signature.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(PetitionSignaturePrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<PetitionResponse> PetitionSignature(
			IJsonRpcCall Call,

			[McpStringParameter("LegalId", "Identifier of Legal Identity to petition.")]
			string LegalId,

			[McpStringParameter("Content", "BASE64-encoded content to sign.")]
			string Content,

			[McpStringParameter("Purpose", "A message to the recipient of the petition, explaining the purpose of the petition.", 1, 1024)]
			string Purpose)
		{
			byte[] ContentBin;

			try
			{
				ContentBin = Convert.FromBase64String(Content);
			}
			catch (Exception)
			{
				return new PetitionResponse("Content is not a valid BASE64-encoded string.");
			}

			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new PetitionResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new PetitionResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new PetitionResponse("MCP XMPP Contracts client not available.");

			string PetitionId = Guid.NewGuid().ToString();

			await Client.PetitionSignatureAsync(LegalId, ContentBin, PetitionId, Purpose);

			return new PetitionResponse(PetitionId, "Signature petition sent.");
		}

		private async Task ContractsClient_PetitionedSignatureResponseReceived(object Sender,
			SignaturePetitionResponseEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			if (e.Response)
			{
				sb.Append("A digital signature you petitioned (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been successfully returned. ");

				AppendQuestionAndRequestor(sb, e.RequestedIdentity, string.Empty,
					"signatory", string.Empty, Array.Empty<string>(), Array.Empty<string>(),
					e.ClientEndpoint);
			}
			else
			{
				sb.Append("Your digital signature petition (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been declined by the recipient.");
			}

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), new Acknowledgement(), true, Session, 5 * 60 * 1000);

					if (Result.HasValue && Result.Value)
					{
						if (!(e.RequestedIdentity is null))
						{
							if (await PetitionCache.AddLegalIdentity(Session.UserName, e.RequestedIdentity))
								this.ResourcesUpdated(Session.User);
						}

						break;
					}
				}
			}
		}

		/// <summary>
		/// MCP Server Tool to petition the parts of a smart contract, for the smart contract.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="ContractId">Identifier of the Smart Contract to petition.</param>
		/// <param name="Purpose">Message to recipients of petition, explaining the purpose 
		/// of the review.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Petition Smart Contract",
			"Sends a petition to the parts of a smart contract, for access to the smart contract.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(PetitionContractPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<PetitionResponse> PetitionSmartContract(
			IJsonRpcCall Call,

			[McpStringParameter("ContractId", "Identifier of the Smart Contract to petition.")]
			string ContractId,

			[McpStringParameter("Purpose", "A message to the recipients of the petition, explaining the purpose of the petition.", 1, 1024)]
			string Purpose)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new PetitionResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new PetitionResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new PetitionResponse("MCP XMPP Contracts client not available.");

			try
			{
				Contract Contract = await Client.GetContractAsync(ContractId);
				return new PetitionResponse(Contract.ToJson(), "Smart Contract available in QuickResponse property. Access already granted. No petition sent.");
			}
			catch (Exception)
			{
				// Access not authorized; send petition.
			}

			string PetitionId = Guid.NewGuid().ToString();

			await Client.PetitionContractAsync(ContractId, PetitionId, Purpose);

			return new PetitionResponse(PetitionId, "Smart Contract petition sent.");
		}

		private async Task ContractsClient_PetitionedContractResponseReceived(object Sender,
			ContractPetitionResponseEventArgs e)
		{
			if (!(Sender is ContractsClient ContractsClient))
				return;

			if (!ContractsClient.Client.TryGetExtension(out McpXmppExtension McpXmppExtension))
				return;

			StringBuilder sb = new StringBuilder();

			if (e.Response)
			{
				sb.Append("A smart contract you petitioned (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been approved by a part in the contract and ");
				sb.AppendLine("been successfully returned.");
				sb.AppendLine();
				sb.Append("You can access the smart contract via the resource: ");
				sb.AppendLine(Networking.XMPP.Contracts.ContractsClient.ContractIdUri(
					e.RequestedContract.ContractId).ToString());
			}
			else
			{
				sb.Append("Your smart contract petition (Petition ID ");
				sb.Append(e.PetitionId);
				sb.Append(") has been declined by a part in the contract.");
			}

			foreach (string SessionId in McpXmppExtension.SessionIds)
			{
				if (this.TryGetMcpSession(SessionId, out Session? Session) &&
					!(Session.User is null))
				{
					bool? Result = await this.ElicitUserInput(McpXmppExtension.FirstCall,
						sb.ToString(), new Acknowledgement(), true, Session, 5 * 60 * 1000);

					if (Result.HasValue && Result.Value)
					{
						if (!(e.RequestedContract is null))
						{
							if (await PetitionCache.AddContract(Session.UserName, e.RequestedContract))
								this.ResourcesUpdated(Session.User);
						}

						break;
					}
				}
			}
		}

		private async Task ContractsClient_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			if (Sender is ContractsClient ContractsClient &&
				ContractsClient.Client.TryGetTag("User", out IUser? User) &&
				!(User is null))
			{
				Contract Contract = await ContractsClient.GetContractAsync(e.ContractId);

				if (await PetitionCache.AddContract(User.UserName, Contract))
					this.ResourceUpdated(User, e.ContractIdUri);
			}
		}

		/// <summary>
		/// MCP Server Tool to sign a smart contract.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="ContractId">Identifier of the Smart Contract to sign.</param>
		/// <param name="Role">Role to sign the contract as.</param>
		/// <param name="Transferable">If the signature is transferable to another party 
		/// during the life cycle of the contract.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Sign Smart Contract",
			"Signs a smart contract as a specific role.",
			"",     // IconsMethod, use default icons
			true,   // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(SignContractPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<ContractResponse> SignContract(
			IJsonRpcCall Call,

			[McpStringParameter("ContractId", "Identifier of the Smart Contract to sign.")]
			string ContractId,

			[McpStringParameter("Role", "Role to sign the contract as.")]
			string Role,

			[McpParameter("Transferable", "If the signature is transferable to another " +
			"party during the life cycle of the contract.")]
			bool Transferable = false)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new ContractResponse("No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new ContractResponse("User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new ContractResponse("MCP XMPP Contracts client not available.");

			try
			{
				Contract? Contract = await PetitionCache.TryGetContract(User.UserName,
					ContractId, Client)
					?? await Client.GetContractAsync(ContractId);

				Contract = await Client.SignContractAsync(Contract, Role, Transferable);

				if (await PetitionCache.AddContract(User.UserName, Contract))
					this.ResourceUpdated(User, Contract.ContractIdUri);

				return new ContractResponse(Contract.ToJson(), "Smart Contract signed.");
			}
			catch (Exception ex)
			{
				return new ContractResponse(ex.Message);
			}
		}

		private async Task ContractsClient_ContractSigned(object Sender, ContractSignedEventArgs e)
		{
			if (Sender is ContractsClient ContractsClient &&
				ContractsClient.Client.TryGetTag("User", out IUser? User) &&
				!(User is null) &&
				await PetitionCache.AddContract(User.UserName, e.Contract))
			{
				this.ResourceUpdated(User, e.Contract.ContractIdUri);
			}
		}

		/// <summary>
		/// MCP Server Tool to send a smart contract proposal.
		/// </summary>
		/// <param name="Call">JSON-RPC call object.</param>
		/// <param name="ContractId">Identifier of the Smart Contract to propose.</param>
		/// <param name="Role">Proposed role.</param>
		/// <param name="To">Bare JID of recipient of proposal.</param>
		/// <param name="Message">Message to show the recipient of the proposal.</param>
		/// <returns>Results of operation.</returns>
		[McpServerTool(
			"Propose Smart Contract",
			"Sends a proposal to sign a smart contract as a specific role.",
			"",     // IconsMethod, use default icons
			false,  // CanModifyEnvironment
			false,  // CanDestroyEnvironment
			false,  // Idempotent
			true)]  // OpenWorldAccess
		[RequiredPrivilege(ProposeContractPrivilege)]
		[return: McpParameter("Result", "Result of operation.")]
		public async Task<GenericResponse> ProposeContract(
			IJsonRpcCall Call,

			[McpStringParameter("ContractId", "Identifier of the Smart Contract to propose.")]
			string ContractId,

			[McpStringParameter("Role", "Role to propose.")]
			string Role,

			[McpStringParameter("To", "Bare JID of recipient of proposal.")]
			string To,

			[McpStringParameter("Message", "Message to show the recipient of the proposal.")]
			string Message)
		{
			Session? Session = await this.TryGetMcpSession(Call);
			if (Session is null)
				return new GenericResponse(false, "No MCP session.");

			IUser? User = await this.GetAuthenticatedUser(Call, Session);
			if (Call.ResponseSent || User is null)
				return new GenericResponse(false, "User not authenticated.");

			ContractsClient? Client = await this.GetClient(Call, User, Session, true);
			if (Client is null)
				return new GenericResponse(false, "MCP XMPP Contracts client not available.");

			try
			{
				await Client.SendContractProposal(ContractId, Role, To, Message);
				return new GenericResponse(true, "Contract proposal sent.");
			}
			catch (Exception ex)
			{
				return new GenericResponse(false, ex.Message);
			}
		}

		private Task ContractsClient_ContractProposalReceived(object Sender, ContractProposalEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}

		private Task ContractsClient_ContractDeleted(object Sender, ContractReferenceEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}

		private Task ContractsClient_ContractCreated(object Sender, ContractReferenceEventArgs e)
		{
			return Task.CompletedTask;  // TODO
		}
	}
}
