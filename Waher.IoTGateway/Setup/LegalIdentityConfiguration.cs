using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Json;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.IoTGateway.Setup.Legal;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.EventArguments;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.IO;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Security;
using Waher.Security.CallStack;
using Waher.Security.TOTP;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Configures legal identity for the gateway.
	/// </summary>
	public class LegalIdentityConfiguration : SystemMultiStepConfiguration
	{
		internal static readonly Regex FromSaveUnsavedRegex = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]((<SaveUnsaved>\w*[.]\w*)|(SaveUnsavedLocked))",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex FromUpdateObjectRegex = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]((<UpdateObject>\w*[.]\w*)|(UpdateObjectLocked))",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex GatewayStartupRegex = new Regex(@"Waher[.]IoTGateway[.]Gateway([.]Start|[.+]<?Start>?\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex ApplyLegalIdentityRegex = new Regex(@"Waher[.]IoTGateway[.]Setup[.]LegalIdentityConfiguration([.]ApplyLegalIdentity|[.+]<?ApplyLegalIdentity>?\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex ApplyIdRegex = new Regex(@"Waher[.]IoTGateway[.]Setup[.]LegalIdentityConfiguration([.]ApplyId|[.+]<?ApplyId>?\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex GenerateNewKeysRegex = new Regex(@"Waher[.]Networking[.]XMPP[.]Contracts[.]ContractsClient([.]GenerateNewKeys|[.+]<?GenerateNewKeys>?\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex GetAttachmentRegex = new Regex(@"Waher[.]Networking[.]XMPP[.]Contracts[.]ContractsClient[.+]<?GetAttachmentAsync>?\w*[.]\w*",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex CreateCredentialRegex = new Regex(@"Waher[.]Security[.]TOTP[.]ExternalCredential[.+]<?CreateAsync>?\w*[.]\w*",
			RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly ICallStackCheck[] approvedSources = Assert.Convert(new object[]
		{
			"Waher.Persistence.NeuroLedger.NeuroLedgerProvider",
			typeof(Content.Markdown.Web.MarkdownToHtmlConverter),
			"Waher.IoTGateway.Setup.LegalIdentityConfiguration.UpdateClients",
			FromSaveUnsavedRegex,
			FromUpdateObjectRegex,
			GatewayStartupRegex
		});
		private static readonly ICallStackCheck[] approvedContractClientSources = Assert.Convert(new object[]
		{
			"Waher.Service.IoTBroker.Legal.MFA.QuickLogin",
			"Waher.Service.IoTBroker.Marketplace.MarketplaceProcessor",
			"Waher.Service.IoTBroker.WebServices.Agent.Account.RemoteQuickLogin",
			"Waher.Service.Abc4Io.Model.Actions.Contract.SignContract",
			ApplyLegalIdentityRegex,
			ApplyIdRegex,
			typeof(LegalIdentityConfiguration),
			GenerateNewKeysRegex,
			GetAttachmentRegex
		});
		private static readonly ICallStackCheck[] approvedOtpSources = Assert.Convert(new object[]
		{
			typeof(Content.Markdown.Web.MarkdownToHtmlConverter),
			FromSaveUnsavedRegex,
			FromUpdateObjectRegex,
			GatewayStartupRegex,
			CreateCredentialRegex
		});

		private static LegalIdentityConfiguration instance = null;
		private static LegalIdentity[] allIdentities = null;
		private static LegalIdentity[] approvedIdentities = null;

		private HttpResource applyLegalIdentity = null;
		private HttpResource contractAction = null;

		private bool useLegalIdentity = false;
		private bool protectWithPassword = false;
		private string firstName = string.Empty;
		private string middleName = string.Empty;
		private string lastName = string.Empty;
		private string personalNumber = string.Empty;
		private string address = string.Empty;
		private string address2 = string.Empty;
		private string postalCode = string.Empty;
		private string area = string.Empty;
		private string city = string.Empty;
		private string region = string.Empty;
		private string country = string.Empty;
		private string nationality = string.Empty;
		private string gender = string.Empty;
		private string orgName = string.Empty;
		private string orgDepartment = string.Empty;
		private string orgRole = string.Empty;
		private string orgNumber = string.Empty;
		private string orgAddress = string.Empty;
		private string orgAddress2 = string.Empty;
		private string orgPostalCode = string.Empty;
		private string orgArea = string.Empty;
		private string orgCity = string.Empty;
		private string orgRegion = string.Empty;
		private string orgCountry = string.Empty;
		private AlternativeField[] altFields = null;
		private AlternativeField[] passwordHashes = null;
		private DateTime checkApprovedExpiry = DateTime.MinValue;
		private DateTime? birthDate = null;

		/// <summary>
		/// Configures legal identity for the gateway.
		/// </summary>
		public LegalIdentityConfiguration()
		{
		}

		/// <summary>
		/// Instance of configuration object.
		/// </summary>
		public static LegalIdentityConfiguration Instance => instance;

		/// <summary>
		/// If the gateway will use a legal identity.
		/// </summary>
		[DefaultValue(false)]
		public bool UseLegalIdentity
		{
			get => this.useLegalIdentity;
			set => this.useLegalIdentity = value;
		}

		/// <summary>
		/// First Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string FirstName
		{
			get => this.firstName;
			set => this.firstName = value;
		}

		/// <summary>
		/// Middle Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string MiddleName
		{
			get => this.middleName;
			set => this.middleName = value;
		}

		/// <summary>
		/// Last Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string LastName
		{
			get => this.lastName;
			set => this.lastName = value;
		}

		/// <summary>
		/// Personal number (or organizational number)
		/// </summary>
		[DefaultValueStringEmpty]
		public string PersonalNumber
		{
			get => this.personalNumber;
			set => this.personalNumber = value;
		}

		/// <summary>
		/// Address
		/// </summary>
		[DefaultValueStringEmpty]
		public string Address
		{
			get => this.address;
			set => this.address = value;
		}

		/// <summary>
		/// Address, 2nd row
		/// </summary>
		[DefaultValueStringEmpty]
		public string Address2
		{
			get => this.address2;
			set => this.address2 = value;
		}

		/// <summary>
		/// Postal Code (or zip code)
		/// </summary>
		[DefaultValueStringEmpty]
		public string PostalCode
		{
			get => this.postalCode;
			set => this.postalCode = value;
		}

		/// <summary>
		/// Area
		/// </summary>
		[DefaultValueStringEmpty]
		public string Area
		{
			get => this.area;
			set => this.area = value;
		}

		/// <summary>
		/// City
		/// </summary>
		[DefaultValueStringEmpty]
		public string City
		{
			get => this.city;
			set => this.city = value;
		}

		/// <summary>
		/// Region
		/// </summary>
		[DefaultValueStringEmpty]
		public string Region
		{
			get => this.region;
			set => this.region = value;
		}

		/// <summary>
		/// Country
		/// </summary>
		[DefaultValueStringEmpty]
		public string Country
		{
			get => this.country;
			set => this.country = value;
		}

		/// <summary>
		/// Nationality
		/// </summary>
		[DefaultValueStringEmpty]
		public string Nationality
		{
			get => this.nationality;
			set => this.nationality = value;
		}

		/// <summary>
		/// Gender
		/// </summary>
		[DefaultValueStringEmpty]
		public string Gender
		{
			get => this.gender;
			set => this.gender = value;
		}

		/// <summary>
		/// Birth Date
		/// </summary>
		[DefaultValueNull]
		public DateTime? BirthDate
		{
			get => this.birthDate;
			set => this.birthDate = value;
		}

		/// <summary>
		/// Organization Name
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgName
		{
			get => this.orgName;
			set => this.orgName = value;
		}

		/// <summary>
		/// Organization Department
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgDepartment
		{
			get => this.orgDepartment;
			set => this.orgDepartment = value;
		}

		/// <summary>
		/// Organization Role
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgRole
		{
			get => this.orgRole;
			set => this.orgRole = value;
		}

		/// <summary>
		/// Organization number
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgNumber
		{
			get => this.orgNumber;
			set => this.orgNumber = value;
		}

		/// <summary>
		/// Organization Address
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgAddress
		{
			get => this.orgAddress;
			set => this.orgAddress = value;
		}

		/// <summary>
		/// Organization Address, 2nd row
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgAddress2
		{
			get => this.orgAddress2;
			set => this.orgAddress2 = value;
		}

		/// <summary>
		/// Organization Postal Code (or zip code)
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgPostalCode
		{
			get => this.orgPostalCode;
			set => this.orgPostalCode = value;
		}

		/// <summary>
		/// Organization Area
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgArea
		{
			get => this.orgArea;
			set => this.orgArea = value;
		}

		/// <summary>
		/// Organization City
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgCity
		{
			get => this.orgCity;
			set => this.orgCity = value;
		}

		/// <summary>
		/// Organization Region
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgRegion
		{
			get => this.orgRegion;
			set => this.orgRegion = value;
		}

		/// <summary>
		/// Organization Country
		/// </summary>
		[DefaultValueStringEmpty]
		public string OrgCountry
		{
			get => this.orgCountry;
			set => this.orgCountry = value;
		}

		/// <summary>
		/// Alternative fields.
		/// </summary>
		[DefaultValueNull]
		public AlternativeField[] AlternativeFields
		{
			get => this.altFields;
			set => this.altFields = value;
		}

		/// <summary>
		/// If the legal identity should be protected with a password.
		/// </summary>
		[DefaultValue(false)]
		public bool ProtectWithPassword
		{
			get => this.protectWithPassword;
			set
			{
				if (this.protectWithPassword != value)
				{
					if (this.protectWithPassword)
						Assert.CallFromSource(approvedSources);

					this.protectWithPassword = value;
				}
			}
		}

		/// <summary>
		/// Password hash, if legal identity is protected by password.
		/// </summary>
		[DefaultValueNull]
		public AlternativeField[] PasswordHashes
		{
			get
			{
				if (!(this.passwordHashes is null))
					Assert.CallFromSource(approvedSources);

				return this.passwordHashes;
			}

			set
			{
				if (!(this.passwordHashes is null))
					Assert.CallFromSource(approvedSources);

				this.passwordHashes = value;
			}
		}

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/LegalIdentity.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 320;

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as LegalIdentityConfiguration;
		}

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 10, "Legal Identity");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override async Task ConfigureSystem()
		{
			if (this.useLegalIdentity && !(Gateway.ContractsClient is null))
				await this.AddHandlers();
		}

		private async Task AddHandlers()
		{
			if (!this.handlersAdded || Gateway.ContractsClient != this.prevClient)
			{
				this.handlersAdded = true;
				this.prevClient = Gateway.ContractsClient;

				Gateway.XmppClient.OnStateChanged += this.XmppClient_OnStateChanged;

				Gateway.ContractsClient.ContractDeleted += this.ContractsClient_ContractDeleted;
				Gateway.ContractsClient.ContractSigned += this.ContractsClient_ContractSigned;
				Gateway.ContractsClient.ContractUpdated += this.ContractsClient_ContractUpdated;
				Gateway.ContractsClient.IdentityUpdated += this.ContractsClient_IdentityUpdated;
				Gateway.ContractsClient.PetitionedIdentityResponseReceived += this.ContractsClient_PetitionedIdentityResponseReceived;
				Gateway.ContractsClient.PetitionedContractResponseReceived += this.ContractsClient_PetitionedContractResponseReceived;

				Gateway.ContractsClient.SetAllowedSources(approvedContractClientSources);
				ExternalCredential.SetAllowedSources(approvedOtpSources);

				if (Gateway.XmppClient.State == XmppState.Connected)
					await this.GetLegalIdentities(null);
			}
		}

		private bool handlersAdded = false;
		private ContractsClient prevClient = null;

		private async Task XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.Connected)
				await this.GetLegalIdentities(null);
		}

		private Task GetLegalIdentities(object P)
		{
			if (Gateway.ContractsClient is null || Gateway.XmppClient.State != XmppState.Connected)
				return Task.CompletedTask;

			return Gateway.ContractsClient.GetLegalIdentities((Sender, e) =>
			{
				if (e.Ok)
				{
					approvedIdentities = this.SetLegalIdentities(e.Identities, null, false);
					allIdentities = this.SetLegalIdentities(e.Identities, null, true);
				}
				else
					Gateway.ScheduleEvent(this.GetLegalIdentities, DateTime.Now.AddMinutes(1), null);

				return Task.CompletedTask;

			}, null);
		}

		private LegalIdentity[] SetLegalIdentities(LegalIdentity[] Identities, LegalIdentity Changed, bool All)
		{
			List<LegalIdentity> Result = new List<LegalIdentity>();
			LegalIdentity ID2;
			bool Added = false;

			if (!(Identities is null))
			{
				foreach (LegalIdentity ID in Identities)
				{
					if (Changed is null || ID.Id != Changed.Id)
						ID2 = ID;
					else
					{
						ID2 = Changed;
						Added = true;
					}

					if (All || ID2.State == IdentityState.Approved)
						Result.Add(ID2);
				}
			}

			if (!Added && !(Changed is null) && (All || Changed.State == IdentityState.Approved))
				Result.Add(Changed);

			Result.Sort((i1, i2) => Math.Sign((i2.Created - i1.Created).Ticks));

			return Result.ToArray();
		}

		/// <summary>
		/// All Legal Identities associated with account.
		/// </summary>
		public static LegalIdentity[] AllIdentities
		{
			get
			{
				Assert.CallFromSource(approvedSources);
				return allIdentities;
			}
		}

		/// <summary>
		/// Public profile of all Legal Identities associated with account, as dictionaries.
		/// </summary>
		public static Dictionary<string, object>[] AllIdentitiesJSON
		{
			get
			{
				List<Dictionary<string, object>> Result = new List<Dictionary<string, object>>();

				if (!(allIdentities is null))
				{
					foreach (LegalIdentity ID in allIdentities)
					{
						Dictionary<string, object> ID2 = new Dictionary<string, object>()
						{
							{ "Id", ID.Id },
							{ "Created", ID.Created },
							{ "Properties", ID.Properties },
							{ "Attachments", ID.Attachments },
							{ "From", ID.From },
							{ "To", ID.To },
							{ "State", ID.State },
							{ "ADDR", string.Empty },
							{ "ADDR2", string.Empty },
							{ "ZIP", string.Empty },
							{ "AREA", string.Empty },
							{ "CITY", string.Empty },
							{ "REGION", string.Empty },
							{ "COUNTRY", string.Empty },
							{ "NATIONALITY", string.Empty },
							{ "GENDER", string.Empty },
							{ "BDAY", string.Empty },
							{ "BMONTH", string.Empty },
							{ "BYEAR", string.Empty },
							{ "PNR", string.Empty },
							{ "ORGADDR", string.Empty },
							{ "ORGADDR2", string.Empty },
							{ "ORGZIP", string.Empty },
							{ "ORGAREA", string.Empty },
							{ "ORGCITY", string.Empty },
							{ "ORGREGION", string.Empty },
							{ "ORGCOUNTRY", string.Empty },
							{ "ORGNAME", string.Empty },
							{ "ORGDEPT", string.Empty },
							{ "ORGROLE", string.Empty },
							{ "ORGNR", string.Empty },
							{ "", string.Empty }
						};

						foreach (Property P in ID.Properties)
							ID2[P.Name] = P.Value;

						ID2["FIRST"] = ID["FIRST"];
						ID2["MIDDLE"] = ID["MIDDLE"];
						ID2["LAST"] = ID["LAST"];
						ID2["FULLNAME"] = ID["FULLNAME"];

						Result.Add(ID2);
					}
				}

				return Result.ToArray();
			}
		}

		/// <summary>
		/// Approved Legal Identities associated with account.
		/// </summary>
		public static LegalIdentity[] ApprovedIdentities
		{
			get
			{
				Assert.CallFromSource(approvedSources);
				return approvedIdentities;
			}
		}

		/// <summary>
		/// Checks if a Legal Identity refers to the gateway.
		/// </summary>
		/// <param name="LegalId">Legal ID</param>
		/// <returns>If Legal ID is referring to the gateway.</returns>
		public static bool IsMe(CaseInsensitiveString LegalId)
		{
			return TryGetMyIdentity(LegalId, out _);
		}

		/// <summary>
		/// Tries to get one of the legal identities belonging to the current instance.
		/// </summary>
		/// <param name="LegalId">ID of Legal Identity</param>
		/// <param name="Identity">Legal Identity, if found.</param>
		/// <returns>If a legal identity was found for the current instance, matching the ID.</returns>
		public static bool TryGetMyIdentity(CaseInsensitiveString LegalId, out LegalIdentity Identity)
		{
			Identity = null;

			if (allIdentities is null)
				return false;

			foreach (LegalIdentity ID in allIdentities)
			{
				if (LegalId == ID.Id)
				{
					Identity = ID;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if a Legal Identity refers to an approved ID of the gateway.
		/// </summary>
		/// <param name="LegalId">Legal ID</param>
		/// <returns>If Legal ID is referring to an approved ID of the gateway.</returns>
		public static bool IsMeApproved(CaseInsensitiveString LegalId)
		{
			if (approvedIdentities is null)
				return false;

			foreach (LegalIdentity ID in approvedIdentities)
			{
				if (LegalId == ID.Id)
					return true;
			}

			return false;
		}

		private async Task ContractsClient_IdentityUpdated(object Sender, LegalIdentityEventArgs e)
		{
			LegalIdentity ID = e.Identity;

			List<KeyValuePair<string, object>> Tags = new List<KeyValuePair<string, object>>()
			{
				new KeyValuePair<string, object>("State", ID.State),
				new KeyValuePair<string, object>("Provider", ID.Provider),
				new KeyValuePair<string, object>("Created", ID.Created),
				new KeyValuePair<string, object>("Updated", ID.Updated),
				new KeyValuePair<string, object>("From", ID.From),
				new KeyValuePair<string, object>("KeyAlgorithm", ID.ClientKeyName),
				new KeyValuePair<string, object>("PublicKey", Convert.ToBase64String(ID.ClientPubKey))
			};

			foreach (Property P in ID.Properties)
				Tags.Add(new KeyValuePair<string, object>(P.Name, P.Value));

			Log.Notice("Legal Identity updated.", e.Identity.Id, Tags.ToArray());

			await this.UpdateClients(ID);
		}

		private Task UpdateClients(LegalIdentity ID)
		{
			allIdentities = this.SetLegalIdentities(allIdentities, ID, true);
			approvedIdentities = this.SetLegalIdentities(approvedIdentities, ID, false);

			return this.UpdateClients();
		}

		private async Task UpdateClients()
		{
			string[] TabIDs;

			if (Gateway.Configuring)
				TabIDs = ClientEvents.GetTabIDs();
			else
				TabIDs = ClientEvents.GetTabIDsForLocation("/Settings/LegalIdentity.md");

			if (TabIDs.Length > 0)
			{
				string FileName = Path.Combine(Gateway.RootFolder, "Settings", "LegalIdentities.md");
				if (File.Exists(FileName))
				{
					string Markdown = await Files.ReadAllTextAsync(FileName);
					Variables v = HttpServer.CreateSessionVariables();
					v["Config"] = this;

					MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Markdown, new MarkdownSettings(Gateway.Emoji1_24x24, true, v));
					string HTML = await Doc.GenerateHTML();
					HTML = HtmlDocument.GetBody(HTML);

					await ClientEvents.PushEvent(TabIDs, "UpdateIdentityTable", HTML);
				}
			}
		}

		private Task ContractsClient_ContractSigned(object Sender, ContractSignedEventArgs e)
		{
			Log.Notice("Smart contract signed.", e.ContractId, e.LegalId, new KeyValuePair<string, object>("Role", e.Role));
			return Task.CompletedTask;
		}

		private Task ContractsClient_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			Log.Notice("Smart contract updated.", e.ContractId);
			return Task.CompletedTask;
		}

		private Task ContractsClient_ContractDeleted(object Sender, ContractReferenceEventArgs e)
		{
			Log.Notice("Smart contract deleted.", e.ContractId);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.applyLegalIdentity = WebServer.Register("/Settings/ApplyLegalIdentity", null, this.ApplyLegalIdentity, true, false, true);
			this.contractAction = WebServer.Register("/Settings/ContractAction", null, this.ContractAction, true, false, true);

			this.checkApprovedExpiry = Gateway.ScheduleEvent(this.CheckApprovedExpiry, DateTime.Now.AddMinutes(5), true);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.applyLegalIdentity);
			WebServer.Unregister(this.contractAction);

			Gateway.CancelScheduledEvent(this.checkApprovedExpiry);
			this.checkApprovedExpiry = DateTime.MinValue;

			return base.UnregisterSetup(WebServer);
		}

		private async Task CheckApprovedExpiry(object State)
		{
			if (!(State is bool Reschedule))
				Reschedule = false;

			DateTime Now = DateTime.Now;
			DateTime Today = Now.Date;

			if (Reschedule)
				this.checkApprovedExpiry = Gateway.ScheduleEvent(this.CheckApprovedExpiry, Today.AddDays(1).AddMinutes(5), true);

			if (!this.useLegalIdentity)
				return;

			if (Gateway.XmppClient.State != XmppState.Connected)
			{
				Gateway.ScheduleEvent(this.CheckApprovedExpiry, Now.AddMinutes(5), false);
				return;
			}

			DateTime Expires = LatestApprovedLegalIdentityExpires;
			if (Expires < Now)
			{
				await Gateway.SendNotification("No approved Legal Identity registered for the gateway.", string.Empty);
				return;
			}

			int Days = (int)(Expires - Today).TotalDays;

			if (Days < 60)
			{
				switch (Days)
				{
					case 0:
						await Gateway.SendNotification("The approved Legal Identity for the gateway **expires today**.", string.Empty);
						return;

					case 1:
						await Gateway.SendNotification("The approved Legal Identity for the gateway **expires tomorrow**.", string.Empty);
						return;

					default:
						await Gateway.SendNotification("The approved Legal Identity for the gateway **expires in " + Days.ToString() + " days**.", string.Empty);
						return;
				}
			}
		}

		/// <summary>
		/// Waits for the user to provide configuration.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		/// <returns>If all system configuration objects must be reloaded from the database.</returns>
		public override async Task<bool> SetupConfiguration(HttpServer WebServer)
		{
			if (!this.Complete &&
				Gateway.XmppClient.State == XmppState.Offline &&
				!(Gateway.XmppClient is null))
			{
				await Gateway.XmppClient.Connect();
			}

			await this.AddHandlers();

			return await base.SetupConfiguration(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Legal.ID";

		private async Task ApplyLegalIdentity(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, object> Parameters))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("protectWithPassword", out object Obj) || !(Obj is bool ProtectWithPassword) ||
				!Parameters.TryGetValue("password", out Obj) || !(Obj is string Password) ||
				!Parameters.TryGetValue("password2", out Obj) || !(Obj is string Password2) ||
				!Parameters.TryGetValue("firstName", out Obj) || !(Obj is string FirstName) ||
				!Parameters.TryGetValue("middleName", out Obj) || !(Obj is string MiddleName) ||
				!Parameters.TryGetValue("lastName", out Obj) || !(Obj is string LastName) ||
				!Parameters.TryGetValue("pNr", out Obj) || !(Obj is string PNr) ||
				!Parameters.TryGetValue("address", out Obj) || !(Obj is string Address) ||
				!Parameters.TryGetValue("address2", out Obj) || !(Obj is string Address2) ||
				!Parameters.TryGetValue("postalCode", out Obj) || !(Obj is string PostalCode) ||
				!Parameters.TryGetValue("area", out Obj) || !(Obj is string Area) ||
				!Parameters.TryGetValue("city", out Obj) || !(Obj is string City) ||
				!Parameters.TryGetValue("region", out Obj) || !(Obj is string Region) ||
				!Parameters.TryGetValue("country", out Obj) || !(Obj is string Country) ||
				!Parameters.TryGetValue("nationality", out Obj) || !(Obj is string Nationality) ||
				!Parameters.TryGetValue("gender", out Obj) || !(Obj is string Gender) ||
				!Parameters.TryGetValue("birthDate", out Obj) || !(Obj is string BirthDateStr) ||
				!Parameters.TryGetValue("orgName", out Obj) || !(Obj is string OrgName) ||
				!Parameters.TryGetValue("orgDepartment", out Obj) || !(Obj is string OrgDepartment) ||
				!Parameters.TryGetValue("orgRole", out Obj) || !(Obj is string OrgRole) ||
				!Parameters.TryGetValue("orgNr", out Obj) || !(Obj is string OrgNr) ||
				!Parameters.TryGetValue("orgAddress", out Obj) || !(Obj is string OrgAddress) ||
				!Parameters.TryGetValue("orgAddress2", out Obj) || !(Obj is string OrgAddress2) ||
				!Parameters.TryGetValue("orgPostalCode", out Obj) || !(Obj is string OrgPostalCode) ||
				!Parameters.TryGetValue("orgArea", out Obj) || !(Obj is string OrgArea) ||
				!Parameters.TryGetValue("orgCity", out Obj) || !(Obj is string OrgCity) ||
				!Parameters.TryGetValue("orgRegion", out Obj) || !(Obj is string OrgRegion) ||
				!Parameters.TryGetValue("orgCountry", out Obj) || !(Obj is string OrgCountry))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (ProtectWithPassword)
			{
				if (string.IsNullOrEmpty(Password))
				{
					await Response.SendResponse(new BadRequestException("Enter a password and try again."));
					return;
				}

				if (Password != Password2)
				{
					await Response.SendResponse(new BadRequestException("Passwords do not match. Retype, and try again."));
					return;
				}
			}

			List<AlternativeField> AlternativeFields = new List<AlternativeField>();

			if (Parameters.TryGetValue("alternative", out Obj) && Obj is Dictionary<string, object> Alternative)
			{
				foreach (KeyValuePair<string, object> P in Alternative)
				{
					switch (P.Key.ToUpper())
					{
						case "FIRST":
						case "MIDDLE":
						case "LAST":
						case "PNR":
						case "ADDR":
						case "ADDR2":
						case "ZIP":
						case "AREA":
						case "CITY":
						case "REGION":
						case "COUNTRY":
						case "NATIONALITY":
						case "GENDER":
						case "BDAY":
						case "BMONTH":
						case "BYEAR":
						case "ORGNAME":
						case "ORGDEPT":
						case "ORGROLE":
						case "ORGNR":
						case "ORGADDR":
						case "ORGADDR2":
						case "ORGZIP":
						case "ORGAREA":
						case "ORGCITY":
						case "ORGREGION":
						case "ORGCOUNTRY":
							await Response.SendResponse(new BadRequestException("The following alternative field name is not allowed: " + P.Key));
							return;

						default:
							AlternativeFields.Add(new AlternativeField(P.Key, P.Value.ToString()));
							break;
					}
				}
			}

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!string.IsNullOrEmpty(BirthDateStr))
			{
				if (!DateTime.TryParse(BirthDateStr, out DateTime BirthDate))
				{
					await Response.SendResponse(new BadRequestException("Invalid birth date."));
					return;
				}

				this.birthDate = BirthDate;
			}
			else
				this.birthDate = null;

			this.useLegalIdentity = true;
			this.firstName = FirstName;
			this.middleName = MiddleName;
			this.lastName = LastName;
			this.personalNumber = PNr;
			this.address = Address;
			this.address2 = Address2;
			this.postalCode = PostalCode;
			this.area = Area;
			this.city = City;
			this.region = Region;
			this.country = Country;
			this.nationality = Nationality;
			this.gender = Gender;
			this.orgName = OrgName;
			this.orgDepartment = OrgDepartment;
			this.orgRole = OrgRole;
			this.orgNumber = OrgNr;
			this.orgAddress = OrgAddress;
			this.orgAddress2 = OrgAddress2;
			this.orgPostalCode = OrgPostalCode;
			this.orgArea = OrgArea;
			this.orgCity = OrgCity;
			this.orgRegion = OrgRegion;
			this.orgCountry = OrgCountry;
			this.altFields = AlternativeFields.ToArray();

			await Database.Update(this);

			Response.StatusCode = 200;

			await this.ApplyId(Password, TabID, ProtectWithPassword, false, null);
		}

		/// <summary>
		/// Resends an identity application with the same properties as last time.
		/// </summary>
		/// <param name="Response">Task completion source where response will be returned.</param>
		public Task Reapply(TaskCompletionSource<bool> Response)
		{
			return this.ApplyId(null, null, false, true, Response);
		}

		internal async Task ApplyId(string Password, string TabID, bool ProtectWithPassword, 
			bool Reapplication, TaskCompletionSource<bool> Response)
		{
			await Gateway.ContractsClient.GenerateNewKeys();
			await Gateway.ContractsClient.Apply(this.GetProperties(), this.ApplyResponse,
				new object[] { Password, TabID, ProtectWithPassword, Reapplication, Response });
		}

		private async Task ApplyResponse(object Sender, LegalIdentityEventArgs e)
		{
			object[] P = (object[])e.State;
			string Password = (string)P[0];
			string TabID = (string)P[1];
			bool ProtectWithPassword = (bool)P[2];
			bool Reapplication = (bool)P[3];
			TaskCompletionSource<bool> Response = (TaskCompletionSource<bool>)P[4];

			if (e.Ok)
			{
				if (!Reapplication)
				{
					this.protectWithPassword = ProtectWithPassword;

					if (ProtectWithPassword)
					{
						Dictionary<string, AlternativeField> ById = new Dictionary<string, AlternativeField>();

						if (!(this.passwordHashes is null))
						{
							foreach (AlternativeField H in this.passwordHashes)
								ById[H.Key] = H;
						}

						ById[e.Identity.Id] = new AlternativeField(e.Identity.Id, this.CalcPasswordHash(e.Identity, Password));

						AlternativeField[] Hashes = new AlternativeField[ById.Count];
						ById.Values.CopyTo(Hashes, 0);

						this.passwordHashes = Hashes;
					}

					this.Step = 1;
					await Database.Update(this);

					if (!string.IsNullOrEmpty(TabID))
						await ClientEvents.PushEvent(new string[] { TabID }, "ApplicationOK", string.Empty);
				}

				await this.UpdateClients(e.Identity);

				Response?.TrySetResult(true);
			}
			else
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "ApplicationError", e.ErrorText);

				Response?.TrySetResult(false);
			}
		}

		private string CalcPasswordHash(LegalIdentity ID, string Password)
		{
			StringBuilder sb = new StringBuilder();
			SortedDictionary<string, string> Sorted = new SortedDictionary<string, string>();

			foreach (Property P in ID.Properties)
			{
				if (Sorted.TryGetValue(P.Name, out string s))
				{
					s += ";" + P.Value;
					Sorted[P.Name] = s;
				}
				else
					Sorted[P.Name] = P.Value;
			}

			bool First = true;

			foreach (KeyValuePair<string, string> P in Sorted)
			{
				if (First)
					First = false;
				else
					sb.Append(',');

				sb.Append(P.Key);
				sb.Append('=');
				sb.Append(P.Value);
			}

			byte[] Digest = Hashes.ComputeHMACSHA256Hash(System.Text.Encoding.UTF8.GetBytes(Password), System.Text.Encoding.UTF8.GetBytes(sb.ToString()));

			return Convert.ToBase64String(Digest);
		}

		private Property[] GetProperties()
		{
			List<Property> Properties = new List<Property>();

			if (!string.IsNullOrEmpty(this.firstName))
				Properties.Add(new Property("FIRST", this.firstName));

			if (!string.IsNullOrEmpty(this.middleName))
				Properties.Add(new Property("MIDDLE", this.middleName));

			if (!string.IsNullOrEmpty(this.lastName))
				Properties.Add(new Property("LAST", this.lastName));

			if (!string.IsNullOrEmpty(this.personalNumber))
				Properties.Add(new Property("PNR", this.personalNumber));

			if (!string.IsNullOrEmpty(this.address))
				Properties.Add(new Property("ADDR", this.address));

			if (!string.IsNullOrEmpty(this.address2))
				Properties.Add(new Property("ADDR2", this.address2));

			if (!string.IsNullOrEmpty(this.postalCode))
				Properties.Add(new Property("ZIP", this.postalCode));

			if (!string.IsNullOrEmpty(this.area))
				Properties.Add(new Property("AREA", this.area));

			if (!string.IsNullOrEmpty(this.city))
				Properties.Add(new Property("CITY", this.city));

			if (!string.IsNullOrEmpty(this.region))
				Properties.Add(new Property("REGION", this.region));

			if (!string.IsNullOrEmpty(this.country))
				Properties.Add(new Property("COUNTRY", this.country));

			if (!string.IsNullOrEmpty(this.nationality))
				Properties.Add(new Property("NATIONALITY", this.nationality));

			if (!string.IsNullOrEmpty(this.gender))
				Properties.Add(new Property("GENDER", this.gender));

			if (!(this.birthDate is null))
			{
				Properties.Add(new Property("BDAY", this.birthDate.Value.Day.ToString()));
				Properties.Add(new Property("BMONTH", this.birthDate.Value.Month.ToString()));
				Properties.Add(new Property("BYEAR", this.birthDate.Value.Year.ToString()));
			}

			if (!string.IsNullOrEmpty(this.orgName))
				Properties.Add(new Property("ORGNAME", this.orgName));

			if (!string.IsNullOrEmpty(this.orgDepartment))
				Properties.Add(new Property("ORGDEPT", this.orgDepartment));

			if (!string.IsNullOrEmpty(this.orgRole))
				Properties.Add(new Property("ORGROLE", this.orgRole));

			if (!string.IsNullOrEmpty(this.orgNumber))
				Properties.Add(new Property("ORGNR", this.orgNumber));

			if (!string.IsNullOrEmpty(this.orgAddress))
				Properties.Add(new Property("ORGADDR", this.orgAddress));

			if (!string.IsNullOrEmpty(this.orgAddress2))
				Properties.Add(new Property("ORGADDR2", this.orgAddress2));

			if (!string.IsNullOrEmpty(this.orgPostalCode))
				Properties.Add(new Property("ORGZIP", this.orgPostalCode));

			if (!string.IsNullOrEmpty(this.orgArea))
				Properties.Add(new Property("ORGAREA", this.orgArea));

			if (!string.IsNullOrEmpty(this.orgCity))
				Properties.Add(new Property("ORGCITY", this.orgCity));

			if (!string.IsNullOrEmpty(this.orgRegion))
				Properties.Add(new Property("ORGREGION", this.orgRegion));

			if (!string.IsNullOrEmpty(this.orgCountry))
				Properties.Add(new Property("ORGCOUNTRY", this.orgCountry));

			if (!(this.altFields is null))
			{
				foreach (AlternativeField F in this.altFields)
					Properties.Add(new Property(F.Key, F.Value));
			}

			return Properties.ToArray();
		}

		private async Task ContractAction(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException("No content."));
				return;
			}

			string Password;
			ContentResponse Content = await Request.DecodeDataAsync();
			if (Content.HasError || !(Content.Decoded is Dictionary<string, object> Parameters))
			{
				await Response.SendResponse(new UnsupportedMediaTypeException("Invalid content."));
				return;
			}

			if (!Parameters.TryGetValue("requestId", out object Obj) || !(Obj is string RequestId) ||
				!Parameters.TryGetValue("sign", out Obj) || !(Obj is bool Sign) ||
				!Parameters.TryGetValue("protect", out Obj) || !(Obj is bool Protect))
			{
				await Response.SendResponse(new BadRequestException("Invalid request."));
				return;
			}

			if (Protect)
			{
				if (!Parameters.TryGetValue("password", out Obj) || !(Obj is string s))
				{
					await Response.SendResponse(new BadRequestException("No password."));
					return;
				}

				Password = s;
			}
			else
				Password = string.Empty;

			ContractSignatureRequest SignatureRequest = await Database.TryLoadObject<ContractSignatureRequest>(RequestId);
			if (SignatureRequest is null)
			{
				await Response.SendResponse(new NotFoundException("Content Signature Request not found."));
				return;
			}

			if (SignatureRequest.Signed.HasValue)
			{
				await Response.SendResponse(new BadRequestException("Contract has already been signed."));
				return;
			}

			if (Protect)
			{
				if (!HasApprovedLegalIdentities)
				{
					await Response.SendResponse(new BadRequestException("No approved legal identity found with which to sign the contract."));
					return;
				}

				string Id = this.GetPasswordLegalId(Password);
				if (string.IsNullOrEmpty(Id))
				{
					await Response.SendResponse(new BadRequestException("Invalid password."));
					return;
				}
			}
			else if (this.protectWithPassword)
			{
				await Response.SendResponse(new BadRequestException("Legal identities protected with password."));
				return;
			}

			if (Sign)
			{
				Contract Contract = await SignatureRequest.GetContract();
				Contract = await Gateway.ContractsClient.SignContractAsync(Contract, SignatureRequest.Role, false);
				SignatureRequest.SetContract(Contract);
				SignatureRequest.Signed = DateTime.Now;
				await Database.Update(SignatureRequest);
			}
			else
				await Database.Delete(SignatureRequest);

			Response.StatusCode = 200;
			Response.ContentType = JsonCodec.DefaultContentType;
			await Response.Write(JSON.Encode(Sign, false));
			await Response.SendResponse();
		}

		/// <summary>
		/// If there are approved legal identities configured.
		/// </summary>
		public static bool HasApprovedLegalIdentities
		{
			get => !(approvedIdentities is null) && approvedIdentities.Length > 0;
		}

		/// <summary>
		/// Latest approved Legal Identity.
		/// </summary>
		public static LegalIdentity LatestApprovedLegalIdentity
		{
			get
			{
				if (!HasApprovedLegalIdentities)
					throw new NotFoundException("Gateway has no approved legal identity.");

				LegalIdentity Latest = null;

				foreach (LegalIdentity Identity in approvedIdentities)
				{
					if (Latest is null || Identity.Created > Latest.Created)
						Latest = Identity;
				}

				return Latest;
			}
		}

		/// <summary>
		/// Latest approved Legal Identity ID.
		/// </summary>
		public static string LatestApprovedLegalIdentityId
		{
			get
			{
				return LatestApprovedLegalIdentity?.Id;
			}
		}

		/// <summary>
		/// Expiry date of latest approved legal identity.
		/// </summary>
		public static DateTime LatestApprovedLegalIdentityExpires
		{
			get
			{
				if (!HasApprovedLegalIdentities)
					return DateTime.MinValue;

				return LatestApprovedLegalIdentity?.To ?? DateTime.MinValue;
			}
		}

		/// <summary>
		/// Gets the legal identity that corresponds to a given password, from the corresponding hash digests.
		/// </summary>
		/// <param name="Password">Password</param>
		/// <returns>Legal Identity ID, if found, or null otherwise</returns>
		public string GetPasswordLegalId(string Password)
		{
			if (approvedIdentities is null || approvedIdentities.Length == 0)
				return null;

			foreach (LegalIdentity ID in approvedIdentities)
			{
				string H = this.CalcPasswordHash(ID, Password);

				foreach (AlternativeField F in this.passwordHashes)
				{
					if (F.Key == ID.Id)
					{
						if (F.Value == H)
							return F.Key;

						break;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Petitions information about a legal identity from its owner.
		/// </summary>
		/// <param name="LegalId">ID of petitioned legal identity.</param>
		/// <param name="PetitionId">A petition ID string used to identity request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the legal identity of the current machine.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <param name="Request">Optional HTTP Request object.</param>
		/// <returns>If a legal identity was found that could be used to sign the petition.</returns>
		public Task<bool> PetitionLegalIdentity(string LegalId, string PetitionId, string Purpose,
			EventHandlerAsync<LegalIdentityPetitionResponseEventArgs> Callback, TimeSpan Timeout,
			HttpRequest Request)
		{
			if (this.protectWithPassword)
				throw new ForbiddenException(Request, "Petitioning legal identities is protected using passwords on this machine.");

			return this.PetitionLegalIdentity(LegalId, PetitionId, Purpose, string.Empty, Callback, Timeout);
		}

		/// <summary>
		/// Petitions information about a legal identity from its owner.
		/// </summary>
		/// <param name="LegalId">ID of petitioned legal identity.</param>
		/// <param name="PetitionId">A petition ID string used to identity request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the legal identity of the current machine.</param>
		/// <param name="Password">Password of legal identity on the current machine used to sign the petition.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a legal identity was found that could be used to sign the petition, and the password matched (if protected by password).</returns>
		public async Task<bool> PetitionLegalIdentity(string LegalId, string PetitionId, string Purpose, string Password,
			EventHandlerAsync<LegalIdentityPetitionResponseEventArgs> Callback, TimeSpan Timeout)
		{
			if (!HasApprovedLegalIdentities)
				return false;

			if (this.protectWithPassword)
			{
				string Id = this.GetPasswordLegalId(Password);
				if (string.IsNullOrEmpty(Id))
					return false;
			}

			lock (this.identityPetitionCallbackMethods)
			{
				if (this.identityPetitionCallbackMethods.ContainsKey(PetitionId))
					throw new ArgumentException("Petition ID already used.", nameof(PetitionId));

				this.identityPetitionCallbackMethods[PetitionId] = Callback;
			}

			try
			{
				await Gateway.ContractsClient.PetitionIdentityAsync(LegalId, PetitionId, Purpose);

				Gateway.ScheduleEvent((P) =>
				{
					LegalIdentityPetitionResponseEventArgs e = new LegalIdentityPetitionResponseEventArgs(null, null, (string)P, false, string.Empty, null);
					this.ContractsClient_PetitionedIdentityResponseReceived(Gateway.ContractsClient, e);
				}, DateTime.Now.Add(Timeout), PetitionId);
			}
			catch (Exception ex)
			{
				lock (this.identityPetitionCallbackMethods)
				{
					this.identityPetitionCallbackMethods.Remove(PetitionId);
				}

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return true;
		}

		/// <summary>
		/// Petitions information about a smart contract from its owner.
		/// </summary>
		/// 
		/// <param name="ContractId">ID of petitioned smart contract.</param>
		/// <param name="PetitionId">A petition ID string used to contract request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the smart contract of the current machine.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <param name="Request">Optional HTTP Request object.</param>
		/// <returns>If a smart contract was found that could be used to sign the petition.</returns>
		public Task<bool> PetitionContract(string ContractId, string PetitionId, string Purpose,
			EventHandlerAsync<ContractPetitionResponseEventArgs> Callback, TimeSpan Timeout,
			HttpRequest Request)
		{
			if (this.protectWithPassword)
				throw new ForbiddenException(Request, "Petitioning legal identities is protected using passwords on this machine.");

			return this.PetitionContract(ContractId, PetitionId, Purpose, string.Empty, Callback, Timeout);
		}

		/// <summary>
		/// Petitions information about a smart contract from its owner.
		/// </summary>
		/// <param name="ContractId">ID of petitioned smart contract.</param>
		/// <param name="PetitionId">A petition ID string used to contract request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the smart contract of the current machine.</param>
		/// <param name="Password">Password of smart contract on the current machine used to sign the petition.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a smart contract was found that could be used to sign the petition, and the password matched (if protected by password).</returns>
		public async Task<bool> PetitionContract(string ContractId, string PetitionId, string Purpose, string Password,
			EventHandlerAsync<ContractPetitionResponseEventArgs> Callback, TimeSpan Timeout)
		{
			if (!HasApprovedLegalIdentities)
				return false;

			if (this.protectWithPassword)
			{
				string Id = this.GetPasswordLegalId(Password);
				if (string.IsNullOrEmpty(Id))
					return false;
			}

			lock (this.contractPetitionCallbackMethods)
			{
				if (this.contractPetitionCallbackMethods.ContainsKey(PetitionId))
					throw new ArgumentException("Petition ID already used.", nameof(PetitionId));

				this.contractPetitionCallbackMethods[PetitionId] = Callback;
			}

			try
			{
				await Gateway.ContractsClient.PetitionContractAsync(ContractId, PetitionId, Purpose);

				Gateway.ScheduleEvent((P) =>
				{
					ContractPetitionResponseEventArgs e = new ContractPetitionResponseEventArgs(null, null, (string)P, false, string.Empty, null);
					this.ContractsClient_PetitionedContractResponseReceived(Gateway.ContractsClient, e);
				}, DateTime.Now.Add(Timeout), PetitionId);
			}
			catch (Exception ex)
			{
				lock (this.contractPetitionCallbackMethods)
				{
					this.contractPetitionCallbackMethods.Remove(PetitionId);
				}

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return true;
		}

		private readonly Dictionary<string, EventHandlerAsync<LegalIdentityPetitionResponseEventArgs>> identityPetitionCallbackMethods = new Dictionary<string, EventHandlerAsync<LegalIdentityPetitionResponseEventArgs>>();
		private readonly Dictionary<string, EventHandlerAsync<ContractPetitionResponseEventArgs>> contractPetitionCallbackMethods = new Dictionary<string, EventHandlerAsync<ContractPetitionResponseEventArgs>>();

		private Task ContractsClient_PetitionedIdentityResponseReceived(object Sender, LegalIdentityPetitionResponseEventArgs e)
		{
			EventHandlerAsync<LegalIdentityPetitionResponseEventArgs> Callback;
			string PetitionId = e.PetitionId;

			lock (this.identityPetitionCallbackMethods)
			{
				if (!this.identityPetitionCallbackMethods.TryGetValue(PetitionId, out Callback))
					return Task.CompletedTask;
				else
					this.identityPetitionCallbackMethods.Remove(PetitionId);
			}

			return Callback.Raise(Sender, e);
		}

		private Task ContractsClient_PetitionedContractResponseReceived(object Sender, ContractPetitionResponseEventArgs e)
		{
			EventHandlerAsync<ContractPetitionResponseEventArgs> Callback;
			string PetitionId = e.PetitionId;

			lock (this.contractPetitionCallbackMethods)
			{
				if (!this.contractPetitionCallbackMethods.TryGetValue(PetitionId, out Callback))
					return Task.CompletedTask;
				else
					this.contractPetitionCallbackMethods.Remove(PetitionId);
			}

			return Callback.Raise(Sender, e);
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult(true);
		}

		/// <summary>
		/// If a legal identity is to be used by the gateway.If used, the folllowing optional variables can be used to provide information 
		/// going into the application.
		/// </summary>
		public const string GATEWAY_ID_USE = nameof(GATEWAY_ID_USE);

		/// <summary>
		/// First name of legal identity.
		/// </summary>
		public const string GATEWAY_ID_FIRST = nameof(GATEWAY_ID_FIRST);

		/// <summary>
		/// Middle name of legal identity.
		/// </summary>
		public const string GATEWAY_ID_MIDDLE = nameof(GATEWAY_ID_MIDDLE);

		/// <summary>
		/// Last name of legal identity.
		/// </summary>
		public const string GATEWAY_ID_LAST = nameof(GATEWAY_ID_LAST);

		/// <summary>
		/// Personal number of legal identity.
		/// </summary>
		public const string GATEWAY_ID_PNR = nameof(GATEWAY_ID_PNR);

		/// <summary>
		/// Address (line 1) of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ADDR = nameof(GATEWAY_ID_ADDR);

		/// <summary>
		/// Address(line 2) of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ADDR2 = nameof(GATEWAY_ID_ADDR2);

		/// <summary>
		/// Postal code of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ZIP = nameof(GATEWAY_ID_ZIP);

		/// <summary>
		/// Area of legal identity.
		/// </summary>
		public const string GATEWAY_ID_AREA = nameof(GATEWAY_ID_AREA);

		/// <summary>
		/// City of legal identity.
		/// </summary>
		public const string GATEWAY_ID_CITY = nameof(GATEWAY_ID_CITY);

		/// <summary>
		/// Region of legal identity.
		/// </summary>
		public const string GATEWAY_ID_REGION = nameof(GATEWAY_ID_REGION);

		/// <summary>
		/// Country of legal identity.
		/// </summary>
		public const string GATEWAY_ID_COUNTRY = nameof(GATEWAY_ID_COUNTRY);

		/// <summary>
		/// Nationality of legal identity.
		/// </summary>
		public const string GATEWAY_ID_NATIONALITY = nameof(GATEWAY_ID_NATIONALITY);

		/// <summary>
		/// Gender of legal identity.
		/// </summary>
		public const string GATEWAY_ID_GENDER = nameof(GATEWAY_ID_GENDER);

		/// <summary>
		/// Birth Date of legal identity.
		/// </summary>
		public const string GATEWAY_ID_BDATE = nameof(GATEWAY_ID_BDATE);

		/// <summary>
		/// Organization name of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGNAME = nameof(GATEWAY_ID_ORGNAME);

		/// <summary>
		/// Organization department of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGDEPT = nameof(GATEWAY_ID_ORGDEPT);

		/// <summary>
		/// Organization role of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGROLE = nameof(GATEWAY_ID_ORGROLE);

		/// <summary>
		/// Organization number of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGNR = nameof(GATEWAY_ID_ORGNR);

		/// <summary>
		/// Organization address (line 1) of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGADDR = nameof(GATEWAY_ID_ORGADDR);

		/// <summary>
		/// Organization address(line 2) of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGADDR2 = nameof(GATEWAY_ID_ORGADDR2);

		/// <summary>
		/// Organization postal code of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGZIP = nameof(GATEWAY_ID_ORGZIP);

		/// <summary>
		/// Organization area of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGAREA = nameof(GATEWAY_ID_ORGAREA);

		/// <summary>
		/// Organization city of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGCITY = nameof(GATEWAY_ID_ORGCITY);

		/// <summary>
		/// Organization region of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGREGION = nameof(GATEWAY_ID_ORGREGION);

		/// <summary>
		/// Organization country of legal identity.
		/// </summary>
		public const string GATEWAY_ID_ORGCOUNTRY = nameof(GATEWAY_ID_ORGCOUNTRY);

		/// <summary>
		/// Comma-separated list of alternative fields to send in identity application.
		/// </summary>
		public const string GATEWAY_ID_ALT = nameof(GATEWAY_ID_ALT);

		/// <summary>
		/// Value for alternative field `field` to send in the identity application.
		/// </summary>
		public const string GATEWAY_ID_ALT_ = nameof(GATEWAY_ID_ALT_);

		/// <summary>
		/// Protect legal identity with this password.
		/// </summary>
		public const string GATEWAY_ID_PASSWORD = nameof(GATEWAY_ID_PASSWORD);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override async Task<bool> EnvironmentConfiguration()
		{
			if (!this.TryGetEnvironmentVariable(GATEWAY_ID_USE, false, out this.useLegalIdentity))
				return false;

			if (!this.useLegalIdentity)
				return true;

			this.firstName = Environment.GetEnvironmentVariable(GATEWAY_ID_FIRST) ?? string.Empty;
			this.middleName = Environment.GetEnvironmentVariable(GATEWAY_ID_MIDDLE) ?? string.Empty;
			this.lastName = Environment.GetEnvironmentVariable(GATEWAY_ID_LAST) ?? string.Empty;
			this.personalNumber = Environment.GetEnvironmentVariable(GATEWAY_ID_PNR) ?? string.Empty;
			this.address = Environment.GetEnvironmentVariable(GATEWAY_ID_ADDR) ?? string.Empty;
			this.address2 = Environment.GetEnvironmentVariable(GATEWAY_ID_ADDR2) ?? string.Empty;
			this.postalCode = Environment.GetEnvironmentVariable(GATEWAY_ID_ZIP) ?? string.Empty;
			this.area = Environment.GetEnvironmentVariable(GATEWAY_ID_AREA) ?? string.Empty;
			this.city = Environment.GetEnvironmentVariable(GATEWAY_ID_CITY) ?? string.Empty;
			this.region = Environment.GetEnvironmentVariable(GATEWAY_ID_REGION) ?? string.Empty;
			this.country = Environment.GetEnvironmentVariable(GATEWAY_ID_COUNTRY) ?? string.Empty;
			this.nationality = Environment.GetEnvironmentVariable(GATEWAY_ID_NATIONALITY) ?? string.Empty;
			this.gender = Environment.GetEnvironmentVariable(GATEWAY_ID_GENDER) ?? string.Empty;
			this.orgName = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGNAME) ?? string.Empty;
			this.orgDepartment = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGDEPT) ?? string.Empty;
			this.orgRole = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGROLE) ?? string.Empty;
			this.orgNumber = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGNR) ?? string.Empty;
			this.orgAddress = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGADDR) ?? string.Empty;
			this.orgAddress2 = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGADDR2) ?? string.Empty;
			this.orgPostalCode = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGZIP) ?? string.Empty;
			this.orgArea = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGAREA) ?? string.Empty;
			this.orgCity = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGCITY) ?? string.Empty;
			this.orgRegion = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGREGION) ?? string.Empty;
			this.orgCountry = Environment.GetEnvironmentVariable(GATEWAY_ID_ORGCOUNTRY) ?? string.Empty;

			if (!this.TryGetEnvironmentVariable(GATEWAY_ID_BDATE, false, out this.birthDate))
				return false;

			List<AlternativeField> Fields = new List<AlternativeField>();

			if (this.TryGetEnvironmentVariable(GATEWAY_ID_ALT,false,out string Value))
			{
				string[] Parts = Value.Split(',');

				foreach (string Part in Parts)
				{
					string Name = GATEWAY_ID_ALT_ + Part;

					if (!this.TryGetEnvironmentVariable(Name, true, out Value))
						return false;

					Fields.Add(new AlternativeField(Part, Value));
				}
			}

			this.altFields = Fields.ToArray();

			Value = Environment.GetEnvironmentVariable(GATEWAY_ID_PASSWORD);

			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
			try
			{
				Task _ = Task.Delay(30000).ContinueWith(Prev => Result?.TrySetException(new TimeoutException()));

				await this.ApplyId(Value, null, !string.IsNullOrEmpty(Value), false, Result);

				if (await Result.Task)
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return false;
			}
		}

	}
}
