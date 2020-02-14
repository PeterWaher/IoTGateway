using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.Contracts.HumanReadable;
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
		internal static readonly Regex FromSaveUnsaved = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]<SaveUnsaved>\w*[.]\w*",
			RegexOptions.Compiled | RegexOptions.Singleline);
		internal static readonly Regex FromUpdateObject = new Regex(@"Waher[.]Persistence[.]Files[.]ObjectBTreeFile[.+]<UpdateObject>\w*[.]\w*",
			RegexOptions.Compiled | RegexOptions.Singleline);
		private static readonly object[] approvedSources = new object[]
		{
			"Waher.Persistence.NeuroLedger.NeuroLedgerProvider",
			typeof(LegalIdentityConfiguration),
			typeof(Content.Markdown.Web.MarkdownToHtmlConverter),
			FromSaveUnsaved,
			FromUpdateObject
		};

		private static LegalIdentityConfiguration instance = null;
		private static LegalIdentity[] allIdentities = null;
		private static LegalIdentity[] approvedIdentities = null;

		private HttpResource applyLegalIdentity = null;

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
		private AlternativeField[] altFields = null;
		private AlternativeField[] passwordHashes = null;

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
				Assert.CallFromSource(approvedSources);
				return this.protectWithPassword;
			}

			set
			{
				if (this.protectWithPassword)
					Assert.CallFromSource(approvedSources);

				this.protectWithPassword = value;
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
				Assert.CallFromSource(approvedSources);
				return this.passwordHashes;
			}

			set
			{
				if (this.passwordHashes != null)
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
			if (!this.handlersAdded)
			{
				this.handlersAdded = true;

				Gateway.XmppClient.OnStateChanged += XmppClient_OnStateChanged;

				Gateway.ContractsClient.ContractDeleted += ContractsClient_ContractDeleted;
				Gateway.ContractsClient.ContractSigned += ContractsClient_ContractSigned;
				Gateway.ContractsClient.ContractUpdated += ContractsClient_ContractUpdated;
				Gateway.ContractsClient.IdentityUpdated += ContractsClient_IdentityUpdated;
				Gateway.ContractsClient.SetAllowedSources(approvedSources);

				if (Gateway.XmppClient.State == XmppState.Connected)
					this.GetLegalIdentities();
			}
		}

		private bool handlersAdded = false;

		private void XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			if (NewState == XmppState.Connected)
				this.GetLegalIdentities();
		}

		private void GetLegalIdentities()
		{
			Gateway.ContractsClient.GetLegalIdentities((sender, e) =>
			{
				if (e.Ok)
				{
					approvedIdentities = this.SetLegalIdentities(e.Identities, null, false);
					allIdentities = this.SetLegalIdentities(e.Identities, null, true);
				}
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

		private void ContractsClient_IdentityUpdated(object Sender, LegalIdentityEventArgs e)
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

			this.UpdateClients(ID);
		}

		private void UpdateClients(LegalIdentity ID)
		{
			allIdentities = this.SetLegalIdentities(allIdentities, ID, true);
			approvedIdentities = this.SetLegalIdentities(approvedIdentities, ID, false);

			this.UpdateClients();
		}

		private void UpdateClients()
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
					string Markdown = File.ReadAllText(FileName);
					Variables v = new Variables(new Variable("Config", this));
					MarkdownDocument Doc = new MarkdownDocument(Markdown, new MarkdownSettings(Gateway.Emoji1_24x24, true, v));
					string HTML = Doc.GenerateHTML();
					HTML = HtmlDocument.GetBody(HTML);

					ClientEvents.PushEvent(TabIDs, "UpdateIdentityTable", HTML);
				}
			}
		}

		private void ContractsClient_ContractSigned(object Sender, ContractSignedEventArgs e)
		{
			Log.Notice("Smart contract signed.", e.ContractId, e.LegalId);
		}

		private void ContractsClient_ContractUpdated(object Sender, ContractReferenceEventArgs e)
		{
			Log.Notice("Smart contract updated.", e.ContractId);
		}

		private void ContractsClient_ContractDeleted(object Sender, ContractReferenceEventArgs e)
		{
			Log.Notice("Smart contract deleted.", e.ContractId);
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.applyLegalIdentity = WebServer.Register("/Settings/ApplyLegalIdentity", null, this.ApplyLegalIdentity, true, false, true);

			return base.InitSetup(WebServer);
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
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.applyLegalIdentity);

			return base.UnregisterSetup(WebServer);
		}

		private void ApplyLegalIdentity(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
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
				!Parameters.TryGetValue("country", out Obj) || !(Obj is string Country))
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
			this.altFields = AlternativeFields.ToArray();

			Response.StatusCode = 200;

			Gateway.ContractsClient.Apply(this.GetProperties(), (sender, e) =>
			{
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
					Task _ = Database.Update(this);

					ClientEvents.PushEvent(new string[] { TabID }, "ApplicationOK", string.Empty);

					this.UpdateClients(e.Identity);
				}
				else
					ClientEvents.PushEvent(new string[] { TabID }, "ApplicationError", e.ErrorText);
			}, null);
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

			if (!string.IsNullOrEmpty(this.firstName))
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

			if (!(this.altFields is null))
			{
				foreach (AlternativeField F in this.altFields)
					Properties.Add(new Property(F.Key, F.Value));
			}

			return Properties.ToArray();
		}

		public void Sign(string Content, string LegalId, string Password)
		{
			LegalIdentity ID = GetApproved(LegalId);
			if (ID is null)
				throw new BadRequestException("Invalid ID or password.");

			if (this.protectWithPassword)
			{
				bool? Match = null;

				foreach (AlternativeField F in this.passwordHashes)
				{
					if (F.Key == LegalId)
					{
						Match = (this.CalcPasswordhash(ID, Password) == F.Value);
						break;
					}
				}

				if (!Match.HasValue || !Match.Value)
					throw new BadRequestException("Invalid ID or password.");
			}

		}

		private static LegalIdentity GetApproved(string LegalId)
		{
			if (approvedIdentities is null)
				return null;

			foreach (LegalIdentity ID in approvedIdentities)
			{
				if (ID.Id == LegalId)
				{
					if (ID.State == IdentityState.Approved)
						return ID;
					else
						return null;
				}
			}

			return null;
		}


		#region Create Contract

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public static void CreateContract(XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			SmartContractEventHandler Callback, object State)
		{
			Gateway.ContractsClient.CreateContract(ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Callback, State);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public static void CreateContract(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate,
			SmartContractEventHandler Callback, object State)
		{
			Gateway.ContractsClient.CreateContract(Address, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Callback, State);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public static Task<Contract> CreateContractAsync(XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			return Gateway.ContractsClient.CreateContractAsync(ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		/// <summary>
		/// Creates a new contract.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="ForMachines">Machine-readable content.</param>
		/// <param name="ForHumans">Human-readable localized content. Provide one object for each language supported by the contract.</param>
		/// <param name="Roles">Roles defined in contract.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public static Task<Contract> CreateContractAsync(string Address, XmlElement ForMachines, HumanReadableText[] ForHumans, Role[] Roles,
			Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility, ContractParts PartsMode, Duration Duration,
			Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			return Gateway.ContractsClient.CreateContractAsync(Address, ForMachines, ForHumans, Roles, Parts, Parameters, Visibility, PartsMode, 
				Duration, ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		#endregion

		#region Create Contract From Template

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public static void CreateContract(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			Gateway.ContractsClient.CreateContract(TemplateId, Parts, Parameters, Visibility, PartsMode, Duration, ArchiveRequired, ArchiveOptional, 
				SignAfter, SignBefore, CanActAsTemplate, Callback, State);
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <param name="Callback">Method to call when registration response is returned.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public static void CreateContract(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate, SmartContractEventHandler Callback, object State)
		{
			Gateway.ContractsClient.CreateContract(Address, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate, Callback, State);
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public static Task<Contract> CreateContractAsync(string TemplateId, Part[] Parts, Parameter[] Parameters, ContractVisibility Visibility,
			ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional, DateTime? SignAfter,
			DateTime? SignBefore, bool CanActAsTemplate)
		{
			return Gateway.ContractsClient.CreateContractAsync(TemplateId, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		/// <summary>
		/// Creates a new contract from a template.
		/// </summary>
		/// <param name="Address">Address of server (component).</param>
		/// <param name="TemplateId">ID of contract to be used as a template.</param>
		/// <param name="Parts">Parts defined in contract. Can be empty or null, if creating an open contract or a template.</param>
		/// <param name="Parameters">Any contractual parameters defined for the contract.</param>
		/// <param name="Visibility">Visibility of the contract.</param>
		/// <param name="PartsMode">How parts are defined in the contract. If equal to <see cref="ContractParts.ExplicitlyDefined"/>,
		/// then the explicitly defined parts must be provided in <paramref name="Parts"/>.</param>
		/// <param name="Duration">Duration of the contract, once signed.</param>
		/// <param name="ArchiveRequired">Required archivation duration, after signed contract has become obsolete.</param>
		/// <param name="ArchiveOptional">Optional archivation duration, after required archivation duration has elapsed.</param>
		/// <param name="SignAfter">Signatures will only be accepted after this point in time, if provided.</param>
		/// <param name="SignBefore">Signatures will only be accepted until this point in time, if provided.</param>
		/// <param name="CanActAsTemplate">If the contract can act as a template.</param>
		/// <returns>Contract.</returns>
		public static Task<Contract> CreateContractAsync(string Address, string TemplateId, Part[] Parts, Parameter[] Parameters,
			ContractVisibility Visibility, ContractParts PartsMode, Duration Duration, Duration ArchiveRequired, Duration ArchiveOptional,
			DateTime? SignAfter, DateTime? SignBefore, bool CanActAsTemplate)
		{
			return Gateway.ContractsClient.CreateContractAsync(Address, TemplateId, Parts, Parameters, Visibility, PartsMode, Duration, 
				ArchiveRequired, ArchiveOptional, SignAfter, SignBefore, CanActAsTemplate);
		}

		#endregion


	}
}
