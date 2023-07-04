using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.IoTGateway.Setup.Legal;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Security;
using Waher.Security.CallStack;

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
		internal static readonly Regex GatewayStartupRegex = new Regex(@"Waher[.]IoTGateway[.]Gateway([.]Start|[.+]<Start>\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex ApplyLegalIdentityRegex = new Regex(@"Waher[.]IoTGateway[.]Setup[.]LegalIdentityConfiguration([.]ApplyLegalIdentity|[.+]<ApplyLegalIdentity>\w*[.]\w*)",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex GetAttachmentRegex = new Regex(@"Waher[.]Networking[.]XMPP[.]Contracts[.]ContractsClient[.+]<GetAttachmentAsync>\w*[.]\w*",
			RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly object[] approvedSources = new object[]
		{
			"Waher.Persistence.NeuroLedger.NeuroLedgerProvider",
			typeof(Content.Markdown.Web.MarkdownToHtmlConverter),
			"Waher.IoTGateway.Setup.LegalIdentityConfiguration.UpdateClients",
			FromSaveUnsavedRegex,
			FromUpdateObjectRegex,
			GatewayStartupRegex
		};
		private static readonly object[] approvedContractClientSources = new object[]
		{
			"Waher.Service.IoTBroker.Legal.MFA.QuickLogin",
			"Waher.Service.IoTBroker.Marketplace.MarketplaceProcessor",
			"Waher.Service.Abc4Io.Model.Actions.Contract.SignContract",
			ApplyLegalIdentityRegex,
			typeof(LegalIdentityConfiguration),
			GetAttachmentRegex
		};

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
			get
			{
				return this.protectWithPassword;
			}

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
		public override Task ConfigureSystem()
		{
			if (this.useLegalIdentity && !(Gateway.ContractsClient is null))
				this.AddHandlers();

			return Task.CompletedTask;
		}

		private void AddHandlers()
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

				if (Gateway.XmppClient.State == XmppState.Connected)
					this.GetLegalIdentities();
			}
		}

		private bool handlersAdded = false;
		private ContractsClient prevClient = null;

		private Task XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.Connected)
				this.GetLegalIdentities();

			return Task.CompletedTask;
		}

		private void GetLegalIdentities()
		{
			Gateway.ContractsClient?.GetLegalIdentities((sender, e) =>
			{
				if (e.Ok)
				{
					approvedIdentities = this.SetLegalIdentities(e.Identities, null, false);
					allIdentities = this.SetLegalIdentities(e.Identities, null, true);
				}

				return Task.CompletedTask;

			}, null);
		}

		private LegalIdentity[] SetLegalIdentities(LegalIdentity[] Identities, LegalIdentity Changed, bool All)
		{
			List<LegalIdentity> Result = new List<LegalIdentity>();
			LegalIdentity ID2;
			bool Added = false;

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
							{ "FIRST", string.Empty },
							{ "MIDDLE", string.Empty },
							{ "LAST", string.Empty },
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
					string Markdown = await Resources.ReadAllTextAsync(FileName);
					Variables v = new Variables(new Variable("Config", this));
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
		public override Task<bool> SetupConfiguration(HttpServer WebServer)
		{
			if (!this.Complete && Gateway.XmppClient.State == XmppState.Offline)
				Gateway.XmppClient?.Connect();

			this.AddHandlers();

			return base.SetupConfiguration(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Legal.ID";

		private async Task ApplyLegalIdentity(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("protectWithPassword", out Obj) || !(Obj is bool ProtectWithPassword) ||
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
				throw new BadRequestException();
			}

			if (ProtectWithPassword)
			{
				if (string.IsNullOrEmpty(Password))
					throw new BadRequestException("Enter a password and try again.");

				if (Password != Password2)
					throw new BadRequestException("Passwords do not match. Retype, and try again.");
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
							throw new BadRequestException("The following alternative field name is not allowed: " + P.Key);

						default:
							AlternativeFields.Add(new AlternativeField(P.Key, P.Value.ToString()));
							break;
					}
				}
			}

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

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

			await Gateway.ContractsClient.GenerateNewKeys();
			await Gateway.ContractsClient.Apply(this.GetProperties(), this.ApplyResponse, new object[] { Password, TabID, ProtectWithPassword });
		}

		private async Task ApplyResponse(object Sender, LegalIdentityEventArgs e)
		{
			object[] P = (object[])e.State;
			string Password = (string)P[0];
			string TabID = (string)P[1];
			bool ProtectWithPassword = (bool)P[2];

			if (e.Ok)
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

					ById[e.Identity.Id] = new AlternativeField(e.Identity.Id, this.CalcPasswordhash(e.Identity, Password));

					AlternativeField[] Hashes = new AlternativeField[ById.Count];
					ById.Values.CopyTo(Hashes, 0);

					this.passwordHashes = Hashes;
				}

				this.Step = 1;
				await Database.Update(this);

				await ClientEvents.PushEvent(new string[] { TabID }, "ApplicationOK", string.Empty);

				await this.UpdateClients(e.Identity);
			}
			else
				await ClientEvents.PushEvent(new string[] { TabID }, "ApplicationError", e.ErrorText);
		}

		private string CalcPasswordhash(LegalIdentity ID, string Password)
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

			byte[] Digest = Hashes.ComputeHMACSHA256Hash(Encoding.UTF8.GetBytes(Password), Encoding.UTF8.GetBytes(sb.ToString()));

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
				throw new BadRequestException("No content.");

			string Password;
			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new UnsupportedMediaTypeException("Invalid content.");

			if (!Parameters.TryGetValue("requestId", out Obj) || !(Obj is string RequestId) ||
				!Parameters.TryGetValue("sign", out Obj) || !(Obj is bool Sign) ||
				!Parameters.TryGetValue("protect", out Obj) || !(Obj is bool Protect))
			{
				throw new BadRequestException("Invalid request.");
			}

			if (Protect)
			{
				if (!Parameters.TryGetValue("password", out Obj) || !(Obj is string s))
					throw new BadRequestException("No password.");

				Password = s;
			}
			else
				Password = string.Empty;

			ContractSignatureRequest SignatureRequest = await Database.TryLoadObject<ContractSignatureRequest>(RequestId) ?? throw new NotFoundException("Content Signature Request not found.");
			if (SignatureRequest.Signed.HasValue)
				throw new BadRequestException("Contract has already been signed.");

			if (Protect)
			{
				if (!HasApprovedLegalIdentities)
					throw new BadRequestException("No approved legal identity found with which to sign the contract.");

				string Id = this.GetPasswordLegalId(Password);
				if (string.IsNullOrEmpty(Id))
					throw new BadRequestException("Invalid password.");
			}
			else if (this.protectWithPassword)
				throw new BadRequestException("Legal identities protected with password.");

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
			Response.ContentType = "application/json";
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
		/// Latest approved Legal Identity ID.
		/// </summary>
		public static string LatestApprovedLegalIdentityId
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

				return Latest?.Id;
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

				LegalIdentity Latest = null;

				foreach (LegalIdentity Identity in approvedIdentities)
				{
					if (Latest is null || Identity.Created > Latest.Created)
						Latest = Identity;
				}

				return Latest?.To ?? DateTime.MinValue;
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
				string H = this.CalcPasswordhash(ID, Password);

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
		/// <returns>If a legal identity was found that could be used to sign the petition.</returns>
		public Task<bool> PetitionLegalIdentity(string LegalId, string PetitionId, string Purpose,
			LegalIdentityPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			if (this.protectWithPassword)
				throw new ForbiddenException("Petitioning legal identities is protected using passwords on this machine.");

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
			LegalIdentityPetitionResponseEventHandler Callback, TimeSpan Timeout)
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
					LegalIdentityPetitionResponseEventArgs e = new LegalIdentityPetitionResponseEventArgs(null, null, (string)P, false, string.Empty);
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
		/// <param name="ContractId">ID of petitioned smart contract.</param>
		/// <param name="PetitionId">A petition ID string used to contract request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the smart contract of the current machine.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a smart contract was found that could be used to sign the petition.</returns>
		public Task<bool> PetitionContract(string ContractId, string PetitionId, string Purpose,
			ContractPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			if (this.protectWithPassword)
				throw new ForbiddenException("Petitioning legal identities is protected using passwords on this machine.");

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
			ContractPetitionResponseEventHandler Callback, TimeSpan Timeout)
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
					ContractPetitionResponseEventArgs e = new ContractPetitionResponseEventArgs(null, null, (string)P, false, string.Empty);
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

		private readonly Dictionary<string, LegalIdentityPetitionResponseEventHandler> identityPetitionCallbackMethods = new Dictionary<string, LegalIdentityPetitionResponseEventHandler>();
		private readonly Dictionary<string, ContractPetitionResponseEventHandler> contractPetitionCallbackMethods = new Dictionary<string, ContractPetitionResponseEventHandler>();

		private Task ContractsClient_PetitionedIdentityResponseReceived(object Sender, LegalIdentityPetitionResponseEventArgs e)
		{
			LegalIdentityPetitionResponseEventHandler Callback;
			string PetitionId = e.PetitionId;

			lock (this.identityPetitionCallbackMethods)
			{
				if (!this.identityPetitionCallbackMethods.TryGetValue(PetitionId, out Callback))
					return Task.CompletedTask;
				else
					this.identityPetitionCallbackMethods.Remove(PetitionId);
			}

			try
			{
				Callback(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.CompletedTask;
		}

		private Task ContractsClient_PetitionedContractResponseReceived(object Sender, ContractPetitionResponseEventArgs e)
		{
			ContractPetitionResponseEventHandler Callback;
			string PetitionId = e.PetitionId;

			lock (this.contractPetitionCallbackMethods)
			{
				if (!this.contractPetitionCallbackMethods.TryGetValue(PetitionId, out Callback))
					return Task.CompletedTask;
				else
					this.contractPetitionCallbackMethods.Remove(PetitionId);
			}

			try
			{
				Callback(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Task.CompletedTask;
		}

	}
}
