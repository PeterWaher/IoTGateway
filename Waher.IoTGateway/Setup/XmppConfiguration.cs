﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Text;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Events.Persistence;
using Waher.IoTGateway.ScriptExtensions.Functions;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Security;
using Waher.Security.Users;
using Waher.Runtime.IO;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// How to bind to the XMPP network.
	/// </summary>
	public enum XmppTransportMethod
	{
		/// <summary>
		/// Traditional XMPP C2S binary socket connection.
		/// </summary>
		C2S = 0,

		/// <summary>
		/// BOSH
		/// </summary>
		BOSH = 1,

		/// <summary>
		/// Web-socket
		/// </summary>
		WS = 2
	}

	/// <summary>
	/// XMPP Configuration
	/// </summary>
	public partial class XmppConfiguration : SystemMultiStepConfiguration, IHostReference
	{
		private static XmppConfiguration instance = null;
		private static string defaultFacility = string.Empty;
		private static string defaultFacilityKey = string.Empty;

		private HttpResource connectToHost = null;
		private HttpResource randomizePassword = null;

		private XmppTransportMethod transportMethod = XmppTransportMethod.C2S;
		private string host = string.Empty;
		private int port = XmppCredentials.DefaultPort;
		private string boshUrl = string.Empty;
		private string wsUrl = string.Empty;
		private string account = string.Empty;
		private string accountHumanReadableName = string.Empty;
		private string password = string.Empty;
		private string password0 = string.Empty;
		private string passwordType = string.Empty;
		private string thingRegistry = string.Empty;
		private string provisioning = string.Empty;
		private string events = string.Empty;
		private string muc = string.Empty;
		private string pubSub = string.Empty;
		private string legal = string.Empty;
		private string software = string.Empty;
		private string geo = string.Empty;
		private string bareJid = string.Empty;
		private bool sniffer = false;
		private bool trustServer = false;
		private bool allowInsecureMechanisms = false;
		private bool storePasswordInsteadOfHash = false;
		private bool customBinding = false;
		private bool offlineMessages = false;
		private bool blocking = false;
		private bool reporting = false;
		private bool abuse = false;
		private bool spam = false;
		private bool pep = false;
		private bool mail = false;

		private TaskCompletionSource<bool> testConnection = null;
		private XmppClient client = null;
		private bool createAccount = false;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static XmppConfiguration Instance => instance;

		/// <summary>
		/// Transport method
		/// </summary>
		[DefaultValue(XmppTransportMethod.C2S)]
		public XmppTransportMethod TransportMethod
		{
			get => this.transportMethod;
			set => this.transportMethod = value;
		}

		/// <summary>
		/// Host to connect to
		/// </summary>
		[DefaultValueStringEmpty]
		public string Host
		{
			get => this.host;
			set => this.host = value;
		}

		/// <summary>
		/// Port number to use
		/// </summary>
		[DefaultValue(XmppCredentials.DefaultPort)]
		public int Port
		{
			get => this.port;
			set => this.port = value;
		}

		/// <summary>
		/// BOSH URL to bind to.
		/// </summary>
		[DefaultValueStringEmpty]
		public string BoshUrl
		{
			get => this.boshUrl;
			set => this.boshUrl = value;
		}

		/// <summary>
		/// Web-socket URL to bind to.
		/// </summary>
		[DefaultValueStringEmpty]
		public string WebSocketUrl
		{
			get => this.wsUrl;
			set => this.wsUrl = value;
		}

		/// <summary>
		/// XMPP Account
		/// </summary>
		[DefaultValueStringEmpty]
		public string Account
		{
			get => this.account;
			set => this.account = value;
		}

		/// <summary>
		/// Password
		/// </summary>
		[Encrypted(32)]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}

		/// <summary>
		/// Type of password. Empty string = clear text, otherwise, type of hash.
		/// </summary>
		[DefaultValueStringEmpty]
		public string PasswordType
		{
			get => this.passwordType;
			set => this.passwordType = value;
		}

		/// <summary>
		/// Human readable account name, if any.
		/// </summary>
		public string AccountHumanReadableName
		{
			get => this.accountHumanReadableName;
			set => this.accountHumanReadableName = value;
		}

		/// <summary>
		/// JID of Thing Registry.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ThingRegistry
		{
			get => this.thingRegistry;
			set => this.thingRegistry = value;
		}

		/// <summary>
		/// JID of provisioning server.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Provisioning
		{
			get => this.provisioning;
			set => this.provisioning = value;
		}

		/// <summary>
		/// JID of event log.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Events
		{
			get => this.events;
			set => this.events = value;
		}

		/// <summary>
		/// JID of Multi-User Chat service.
		/// </summary>
		[DefaultValueStringEmpty]
		public string MultiUserChat
		{
			get => this.muc;
			set => this.muc = value;
		}

		/// <summary>
		/// JID of publish/subscribe component.
		/// </summary>
		[DefaultValueStringEmpty]
		public string PubSub
		{
			get => this.pubSub;
			set => this.pubSub = value;
		}

		/// <summary>
		/// JID of legal identities component.
		/// </summary>
		[DefaultValueStringEmpty]
		public string LegalIdentities
		{
			get => this.legal;
			set => this.legal = value;
		}

		/// <summary>
		/// JID of software updates component.
		/// </summary>
		[DefaultValueStringEmpty]
		public string SoftwareUpdates
		{
			get => this.software;
			set => this.software = value;
		}

		/// <summary>
		/// JID of geo-spatial publish/subscribe component.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Geo
		{
			get => this.geo;
			set => this.geo = value;
		}

		/// <summary>
		/// Bare JID
		/// </summary>
		[DefaultValueStringEmpty]
		public string BareJid
		{
			get => this.bareJid;
			set => this.bareJid = value;
		}

		/// <summary>
		/// If communication should be sniffed.
		/// </summary>
		[DefaultValue(false)]
		public bool Sniffer
		{
			get => this.sniffer;
			set => this.sniffer = value;
		}

		/// <summary>
		/// If server is to be trusted (true), or if certificate should be validated before connection is accepted (false).
		/// </summary>
		[DefaultValue(false)]
		public bool TrustServer
		{
			get => this.trustServer;
			set => this.trustServer = value;
		}

		/// <summary>
		/// If insecure authentication methods should be allowed.
		/// </summary>
		[DefaultValue(false)]
		public bool AllowInsecureMechanisms
		{
			get => this.allowInsecureMechanisms;
			set => this.allowInsecureMechanisms = value;
		}

		/// <summary>
		/// If password should be stored instead of hash.
		/// </summary>
		[DefaultValue(false)]
		public bool StorePasswordInsteadOfHash
		{
			get => this.storePasswordInsteadOfHash;
			set => this.storePasswordInsteadOfHash = value;
		}

		/// <summary>
		/// If a custom binding is used.
		/// </summary>
		[DefaultValue(false)]
		public bool CustomBinding
		{
			get => this.customBinding;
			set => this.customBinding = value;
		}

		/// <summary>
		/// If offline messages are supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool OfflineMessages
		{
			get => this.offlineMessages;
			set => this.offlineMessages = value;
		}

		/// <summary>
		/// If blocking accounts is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Blocking
		{
			get => this.blocking;
			set => this.blocking = value;
		}

		/// <summary>
		/// If reporting is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Reporting
		{
			get => this.reporting;
			set => this.reporting = value;
		}

		/// <summary>
		/// If reporting abuse is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Abuse
		{
			get => this.abuse;
			set => this.abuse = value;
		}

		/// <summary>
		/// If reporting spam is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Spam
		{
			get => this.spam;
			set => this.spam = value;
		}

		/// <summary>
		/// If the Personal Eventing Protocol (PEP) is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool PersonalEventing
		{
			get => this.pep;
			set => this.pep = value;
		}

		/// <summary>
		/// If mail is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Mail
		{
			get => this.mail;
			set => this.mail = value;
		}

		/// <summary>
		/// If an account can be created.
		/// </summary>
		[IgnoreMember]
		public bool CreateAccount => this.createAccount;

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/XMPP.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 300;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 6, "XMPP");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override async Task ConfigureSystem()
		{
			await Gateway.ConfigureXmpp(this);
			await this.CheckAdminAccount();
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as XmppConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.connectToHost = WebServer.Register("/Settings/ConnectToHost", null, this.ConnectToHost, true, false, true);
			this.randomizePassword = WebServer.Register("/Settings/RandomizePassword", null, this.RandomizePassword, true, false, true);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.connectToHost);
			WebServer.Unregister(this.randomizePassword);

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Communication.XMPP";

		private async Task ConnectToHost(HttpRequest Request, HttpResponse Response)
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

			if (!Parameters.TryGetValue("host", out object Obj) || !(Obj is string HostName))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("port", out Obj) || !(Obj is int Port) || Port < 1 || Port > 65535)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("boshUrl", out Obj) || !(Obj is string BoshUrl))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("wsUrl", out Obj) || !(Obj is string WsUrl))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("customBinding", out Obj) || !(Obj is bool CustomBinding))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("trustServer", out Obj) || !(Obj is bool TrustServer))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("insecureMechanisms", out Obj) || !(Obj is bool InsecureMechanisms))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("storePassword", out Obj) || !(Obj is bool StorePasswordInsteadOfHash))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("sniffer", out Obj) || !(Obj is bool Sniffer))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("transport", out Obj) || !(Obj is string s2) || !Enum.TryParse(s2, out XmppTransportMethod Method))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("account", out Obj) || !(Obj is string Account))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("password", out Obj) || !(Obj is string Password))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("createAccount", out Obj) || !(Obj is bool CreateAccount))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("accountName", out Obj) || !(Obj is string AccountName))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			this.host = HostName;
			this.port = Port;
			this.boshUrl = BoshUrl.Trim();
			this.wsUrl = WsUrl.Trim();
			this.customBinding = CustomBinding;
			this.trustServer = TrustServer;
			this.allowInsecureMechanisms = InsecureMechanisms;
			this.storePasswordInsteadOfHash = StorePasswordInsteadOfHash;
			this.sniffer = Sniffer;
			this.transportMethod = Method;
			this.account = Account;
			this.createAccount = CreateAccount;
			this.accountHumanReadableName = AccountName;

			if (this.password != Password)
			{
				this.password = this.password0 = Password;
				this.passwordType = string.Empty;
			}

			if (!(this.client is null))
			{
				await this.client.OfflineAndDisposeAsync();
				this.client = null;
			}

			this.Connect(TabID);

			Response.StatusCode = 200;
		}

		private Task RandomizePassword(HttpRequest Request, HttpResponse Response)
		{
			Response.StatusCode = 200;
			Response.ContentType = PlainTextCodec.DefaultContentType;

			return Response.Write(RandomPassword.CreateRandomPassword());
		}

		/// <summary>
		/// Gets connection credentials.
		/// </summary>
		/// <returns>Connection credentials.</returns>
		public XmppCredentials GetCredentials()
		{
			XmppCredentials Credentials = new XmppCredentials()
			{
				Host = this.host,
				Port = this.port,
				Account = this.account,
				Password = this.password,
				PasswordType = this.passwordType,
				ThingRegistry = this.thingRegistry,
				Provisioning = this.provisioning,
				Events = this.events,
				TrustServer = this.trustServer,
				Sniffer = this.sniffer,
				AllowEncryption = true,
				AllowCramMD5 = this.allowInsecureMechanisms,
				AllowDigestMD5 = this.allowInsecureMechanisms,
				AllowPlain = this.allowInsecureMechanisms,
				AllowRegistration = this.createAccount,
				AllowScramSHA1 = true,
				AllowScramSHA256 = true,
				RequestRosterOnStartup = true
			};

			if (this.createAccount && clp.TryGetValue(this.host, out KeyValuePair<string, string> P))
			{
				Credentials.FormSignatureKey = P.Key;
				Credentials.FormSignatureSecret = P.Value;
			}

			switch (this.transportMethod)
			{
				case XmppTransportMethod.BOSH:
					Credentials.UriEndpoint = this.boshUrl;
					break;

				case XmppTransportMethod.WS:
					Credentials.UriEndpoint = this.wsUrl;
					break;
			}

			return Credentials;
		}

		private void Connect(string TabID)
		{
			this.client = new XmppClient(this.GetCredentials(), "en", typeof(Gateway).Assembly);
			this.client.SetTag("TabID", TabID);
			this.client.SetTag("StartedEncryption", false);
			this.client.SetTag("EncyptionSuccessful", false);
			this.client.SetTag("StartedAuthentication", false);

			if (this.sniffer)
			{
				ISniffer Sniffer;

				if (Gateway.ConsoleOutput)
				{
					Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.PadWithSpaces);
					this.client.Add(Sniffer);
				}

				Sniffer = new XmlFileSniffer(Gateway.AppDataFolder + "XMPP" + Path.DirectorySeparatorChar +
					"XMPP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					Gateway.AppDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
					7, BinaryPresentationMethod.ByteCount);
				this.client.Add(Sniffer);
			}

			this.client.OnStateChanged += this.Client_OnStateChanged;
			this.client.Connect();
		}

		private async Task Client_OnStateChanged(object Sender, XmppState NewState)
		{
			if (!(Sender is XmppClient Client))
				return;

			if (!Client.TryGetTag("TabID", out object Obj) || !(Obj is string TabID))
				TabID = null;

			try
			{
				string Msg;

				switch (NewState)
				{
					case XmppState.Authenticating:
						Client.SetTag("StartedAuthentication", true);
						Client.SetTag("EncyptionSuccessful", true);
						if (this.Step == 0)
						{
							if (!string.IsNullOrEmpty(TabID))
							{
								await ClientEvents.PushEvent(new string[] { TabID }, "ConnectionOK0", "Connection established.", false, "User");

								if (!(this.client is null))
								{
									await this.client.OfflineAndDisposeAsync();
									this.client = null;
								}

								this.Step = 1;
								this.Updated = DateTime.Now;
								await Database.Update(this);

								return;
							}
						}

						Msg = "Authenticating user.";
						break;

					case XmppState.Binding:
						Msg = "Binding to resource.";
						break;

					case XmppState.Connected:
						this.bareJid = Client.BareJID;
						this.password = Client.PasswordHash;
						this.passwordType = Client.PasswordHashMethod;

						if (this.bareJid != defaultFacility)
						{
							defaultFacility = this.bareJid;

							if (string.IsNullOrEmpty(defaultFacilityKey))
								defaultFacilityKey = Hashes.BinaryToString(Gateway.NextBytes(32));

							foreach (IEventSink Sink in Log.Sinks)
							{
								if (Sink is PersistedEventLog PersistedEventLog)
								{
									try
									{
										PersistedEventLog.SetDefaultFacility(defaultFacility, defaultFacilityKey);
									}
									catch (Exception ex)
									{
										Log.Exception(ex);
									}

									break;
								}
							}
						}

						if (this.createAccount && !string.IsNullOrEmpty(this.accountHumanReadableName))
						{
							if (!string.IsNullOrEmpty(TabID))
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Setting vCard.", false, "User");

							StringBuilder Xml = new StringBuilder();

							Xml.Append("<vCard xmlns='vcard-temp'>");
							Xml.Append("<FN>");
							Xml.Append(XML.Encode(this.accountHumanReadableName));
							Xml.Append("</FN>");
							Xml.Append("<JABBERID>");
							Xml.Append(XML.Encode(this.client.BareJID));
							Xml.Append("</JABBERID>");
							Xml.Append("</vCard>");

							await Client.IqSetAsync(this.client.BareJID, Xml.ToString());
						}

						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking server features.", false, "User");

						ServiceDiscoveryEventArgs e = await Client.ServiceDiscoveryAsync(null, string.Empty, string.Empty);

						if (e.Ok)
						{
							this.offlineMessages = e.HasFeature("msgoffline");
							this.blocking = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceBlocking);
							this.reporting = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceReporting);
							this.abuse = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceAbuseReason);
							this.spam = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceSpamReason);
							this.mail = e.HasFeature("urn:xmpp:smtp");
						}
						else
						{
							this.offlineMessages = false;
							this.blocking = false;
							this.reporting = false;
							this.abuse = false;
							this.spam = false;
							this.mail = false;
						}

						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking account features.", false, "User");

						e = await Client.ServiceDiscoveryAsync(null, Client.BareJID, string.Empty);

						this.pep = e.Ok && this.ContainsIdentity("pep", "pubsub", e);

						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking server components.", false, "User");

						ServiceItemsDiscoveryEventArgs e2 = await Client.ServiceItemsDiscoveryAsync(null, string.Empty, string.Empty);

						this.thingRegistry = string.Empty;
						this.provisioning = string.Empty;
						this.events = string.Empty;
						this.muc = string.Empty;
						this.pubSub = string.Empty;
						this.legal = string.Empty;
						this.software = string.Empty;
						this.geo = string.Empty;

						if (e2.Ok)
						{
							foreach (Item Item in e2.Items)
							{
								if (!string.IsNullOrEmpty(TabID))
									await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking component features for " + Item.JID, false, "User");

								e = await Client.ServiceDiscoveryAsync(null, Item.JID, string.Empty);

								if (e.HasAnyFeature(Networking.XMPP.Provisioning.ThingRegistryClient.NamespacesDiscovery))
									this.thingRegistry = Item.JID;

								if (e.HasAnyFeature(Networking.XMPP.Provisioning.ProvisioningClient.NamespacesProvisioningDevice))
									this.provisioning = Item.JID;

								if (e.HasFeature(Networking.XMPP.MUC.MultiUserChatClient.NamespaceMuc))
									this.muc = Item.JID;

								if (e.HasFeature(Networking.XMPP.PubSub.PubSubClient.NamespacePubSub) && this.ContainsIdentity("service", "pubsub", e))
									this.pubSub = Item.JID;

								if (e.HasFeature(Waher.Events.XMPP.XmppEventSink.NamespaceEventLogging))
									this.events = Item.JID;

								if (e.HasAnyFeature(Networking.XMPP.Contracts.ContractsClient.NamespacesLegalIdentities))
									this.legal = Item.JID;

								if (e.HasAnyFeature(Networking.XMPP.Software.SoftwareUpdateClient.NamespacesSoftwareUpdates))
									this.software = Item.JID;

								if (e.HasAnyFeature(Networking.XMPP.Geo.GeoClient.NamespacesGeoSpatial))
									this.geo = Item.JID;
							}
						}

						Dictionary<string, object> ConnectionInfo = new Dictionary<string, object>()
						{
							{ "msg", "Connection successful." },
							{ "offlineMsg", this.offlineMessages },
							{ "blocking", this.blocking },
							{ "reporting", this.reporting },
							{ "abuse", this.abuse },
							{ "spam", this.spam },
							{ "mail", this.mail },
							{ "pep", this.pep ? this.bareJid : string.Empty },
							{ "thingRegistry", this.thingRegistry },
							{ "provisioning", this.provisioning },
							{ "eventLog", this.events },
							{ "pubSub", this.pubSub },
							{ "muc", this.muc },
							{ "legal", this.legal },
							{ "software", this.software },
							{ "geo", this.geo }
						};

						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ConnectionOK1", JSON.Encode(ConnectionInfo, false), true, "User");

						if (!(this.client is null))
						{
							this.client.OnStateChanged -= this.Client_OnStateChanged;
							await this.client.OfflineAndDisposeAsync();
							this.client = null;
						}

						this.Step = 2;
						this.Updated = DateTime.Now;
						await Database.Update(this);

						this.testConnection?.TrySetResult(true);
						return;

					case XmppState.Connecting:
						Msg = "Connecting to server.";
						break;

					case XmppState.Error:
						bool Error = false;
						Msg = string.Empty;

						if (this.Step == 0 && this.transportMethod == XmppTransportMethod.C2S)
						{
							this.customBinding = true;

							if (!string.IsNullOrEmpty(TabID))
							{
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Unable to connect properly. Looking for alternative ways to connect.", false, "User");
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowCustomProperties", "{\"visible\":true}", true, "User");
							}

							using HttpClient HttpClient = new HttpClient(new HttpClientHandler()
							{
#if !NETFW
								ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
								UseCookies = false,
								AutomaticDecompression = (DecompressionMethods)(-1)     // All
							})
							{
								Timeout = TimeSpan.FromMilliseconds(60000)
							};

							try
							{
								Uri Uri = new Uri("http://" + this.host + "/.well-known/host-meta");
								HttpResponseMessage Response = await HttpClient.GetAsync(Uri);
								if (!Response.IsSuccessStatusCode)
								{
									ContentResponse Temp = await Content.Getters.WebGetter.ProcessResponse(Response, Uri);
									Temp.AssertOk();
								}

								Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.
								byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
								string CharSet = Response.Content.Headers.ContentType.CharSet;
								System.Text.Encoding Encoding;

								if (string.IsNullOrEmpty(CharSet))
									Encoding = System.Text.Encoding.UTF8;
								else
									Encoding = InternetContent.GetEncoding(CharSet);

								string XmlResponse = Strings.GetString(Bin, Encoding);
								XmlDocument Doc = new XmlDocument()
								{
									PreserveWhitespace = true
								};
								Doc.LoadXml(XmlResponse);

								if (!(Doc.DocumentElement is null) && Doc.DocumentElement.LocalName == "XRD")
								{
									string BoshUrl = null;
									string WsUrl = null;

									foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
									{
										if (N is XmlElement E && E.LocalName == "Link")
										{
											switch (XML.Attribute(E, "rel"))
											{
												case "urn:xmpp:alt-connections:xbosh":
													BoshUrl = XML.Attribute(E, "href");
													break;

												case "urn:xmpp:alt-connections:websocket":
													WsUrl = XML.Attribute(E, "href");
													break;
											}
										}
									}

									if (!string.IsNullOrEmpty(WsUrl))
									{
										this.wsUrl = WsUrl;
										this.transportMethod = XmppTransportMethod.WS;

										if (!string.IsNullOrEmpty(TabID))
											await ClientEvents.PushEvent(new string[] { TabID }, "ShowTransport", "{\"method\":\"WS\"}", true, "User");

										this.Connect(TabID);

										return;
									}
									else if (!string.IsNullOrEmpty(BoshUrl))
									{
										this.boshUrl = BoshUrl;
										this.transportMethod = XmppTransportMethod.BOSH;

										if (!string.IsNullOrEmpty(TabID))
											await ClientEvents.PushEvent(new string[] { TabID }, "ShowTransport", "{\"method\":\"BOSH\"}", true, "User");

										this.Connect(TabID);

										return;
									}
								}
							}
							catch (Exception)
							{
								// Ignore.
							}

							Msg = "No alternative binding methods found.";
							Error = true;
						}
						else
						{
							Msg = "Unable to connect properly.";
							Error = true;

							if (Client.TryGetTag("StartedAuthentication", out Obj) && Obj is bool b && b)
							{
								if (!string.IsNullOrEmpty(TabID))
								{
									if (this.createAccount)
										await ClientEvents.PushEvent(new string[] { TabID }, "ShowFail2", Msg, false, "User");
									else
										await ClientEvents.PushEvent(new string[] { TabID }, "ShowFail1", Msg, false, "User");
								}
								return;
							}
						}

						if (Error)
						{
							if (string.IsNullOrEmpty(TabID))
								this.LogEnvironmentError(Msg, GATEWAY_XMPP_HOST, this.host);
							else
								await ClientEvents.PushEvent(new string[] { TabID }, "ConnectionError", Msg, false, "User");

							if (!(this.client is null))
							{
								await this.client.OfflineAndDisposeAsync();
								this.client = null;
							}

							this.testConnection?.TrySetResult(false);
							return;
						}
						break;

					case XmppState.FetchingRoster:
						Msg = "Fetching roster from server.";
						break;

					case XmppState.Offline:
						Msg = "Offline.";
						break;

					case XmppState.Registering:
						Msg = "Registering account.";
						break;

					case XmppState.RequestingSession:
						Msg = "Requesting session.";
						break;

					case XmppState.SettingPresence:
						Msg = "Setting presence.";
						break;

					case XmppState.StartingEncryption:
						Msg = "Starting encryption.";
						Client.SetTag("StartedEncryption", true);
						break;

					case XmppState.StreamNegotiation:
						Msg = "Negotiating stream.";
						break;

					case XmppState.StreamOpened:
						Msg = "Stream opened.";
						break;

					default:
						Msg = NewState.ToString();
						break;
				}

				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Msg, false, "User");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", ex.Message, false, "User");
			}
		}

		private async Task CheckAdminAccount()
		{
			if (string.IsNullOrEmpty(this.password0) && (!string.IsNullOrEmpty(this.passwordType) || string.IsNullOrEmpty(this.password)))
				return;

			User User = await Users.GetUser(this.account, true);

			User.PasswordHash = Convert.ToBase64String(Users.ComputeHash(this.account,
				string.IsNullOrEmpty(this.password0) ? this.password : this.password0));

			if (User.RoleIds.Length == 0)
				User.RoleIds = new string[] { "Administrator" };

			await Database.Update(User);

			Role Role = await Roles.GetRole("Administrator");
			if (Role.Privileges.Length == 0)
			{
				Role.Privileges = new PrivilegePattern[]
				{
					new PrivilegePattern(".*", true)
				};

				if (string.IsNullOrEmpty(Role.Description))
					Role.Description = "Administrator role. Has all privileges by default.";

				await Database.Update(Role);
			}
		}

		private bool ContainsIdentity(string Type, string Cateogory, ServiceDiscoveryEventArgs e)
		{
			foreach (Identity Identity in e.Identities)
			{
				if (Identity.Type == Type && Identity.Category == Cateogory)
					return true;
			}

			return false;
		}

		private bool RemoteCertificateValidationCallback(object Sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool Valid;

			if (sslPolicyErrors == SslPolicyErrors.None)
				Valid = true;
			else
				Valid = this.trustServer;

			return Valid;
		}

		/// <summary>
		/// Array of XMPP servers featured by the installation.
		/// </summary>
		public static string[] FeaturedServers
		{
			get
			{
				if (featuredServers is null)
				{
					string[] Result = new string[clp.Count];
					clp.Keys.CopyTo(Result, 0);
					Array.Sort(Result);
					featuredServers = Result;
				}

				return featuredServers;
			}
		}

		private static string[] featuredServers = null;

		/// <summary>
		/// XMPP broker to connect to. (If `C2S` binding has been selected).
		/// </summary>
		public const string GATEWAY_XMPP_HOST = nameof(GATEWAY_XMPP_HOST);

		/// <summary>
		/// XMPP transport method(a.k.a.binding). Can be `C2S` (default if variable not available), `BOSH` (Bidirectional HTTP) or 
		/// `WS` (Web-socket).
		/// </summary>
		public const string GATEWAY_XMPP_TRANSPORT = nameof(GATEWAY_XMPP_TRANSPORT);

		/// <summary>
		/// Optional Port number to use when connecting to host. (If `C2S` binding has been selected.) If not provided, the default port number 
		/// will be used.
		/// </summary>
		public const string GATEWAY_XMPP_PORT = nameof(GATEWAY_XMPP_PORT);

		/// <summary>
		/// URL to use when connecting to host. (If `BOSH` binding has been selected).
		/// </summary>
		public const string GATEWAY_XMPP_BOSHURL = nameof(GATEWAY_XMPP_BOSHURL);

		/// <summary>
		/// URL to use when connecting to host. (If `WS` binding has been selected).
		/// </summary>
		public const string GATEWAY_XMPP_WSURL = nameof(GATEWAY_XMPP_WSURL);

		/// <summary>
		/// If an account is to be created.
		/// </summary>
		public const string GATEWAY_XMPP_CREATE = nameof(GATEWAY_XMPP_CREATE);

		/// <summary>
		/// API-Key to use when creating account, if host is not one of the featured hosts.
		/// </summary>
		public const string GATEWAY_XMPP_CREATE_KEY = nameof(GATEWAY_XMPP_CREATE_KEY);

		/// <summary>
		/// API-Key secret to use when creating account, if host is not one of the featured hosts.
		/// </summary>
		public const string GATEWAY_XMPP_CREATE_SECRET = nameof(GATEWAY_XMPP_CREATE_SECRET);

		/// <summary>
		/// Name of account.
		/// </summary>
		public const string GATEWAY_XMPP_ACCOUNT = nameof(GATEWAY_XMPP_ACCOUNT);

		/// <summary>
		/// Password of account. If creating an account, this variable is optional. If not available, a secure password will be generated.
		/// </summary>
		public const string GATEWAY_XMPP_PASSWORD = nameof(GATEWAY_XMPP_PASSWORD);

		/// <summary>
		/// Optional Human-readable name of account.
		/// </summary>
		public const string GATEWAY_XMPP_ACCOUNT_NAME = nameof(GATEWAY_XMPP_ACCOUNT_NAME);

		/// <summary>
		/// Optional. `true` or `1` if gateway should log communication to program data folder, `false` or `0` if communication should not be 
		/// logged (default).
		/// </summary>
		public const string GATEWAY_XMPP_LOG = nameof(GATEWAY_XMPP_LOG);

		/// <summary>
		/// Optional. `true` or `1` if gateway should trust server certificate, even if it does not validate, `false` or `0` if server should 
		/// be distrusted(default).
		/// </summary>
		public const string GATEWAY_XMPP_TRUST = nameof(GATEWAY_XMPP_TRUST);

		/// <summary>
		/// Optional. `true` or `1` if gateway should be allowed to use obsolete and insecure authentication mechanisms, `false` or `0` if only 
		/// secure mechanisms should be allowed(default).
		/// </summary>
		public const string GATEWAY_XMPP_OBS_AUTH = nameof(GATEWAY_XMPP_OBS_AUTH);

		/// <summary>
		/// Optional. `true` or `1` if gateway should store password as-is in the database, `false` or `0` if only the password hash should be 
		/// stored(default).
		/// </summary>
		public const string GATEWAY_XMPP_CLEAR_PWD = nameof(GATEWAY_XMPP_CLEAR_PWD);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override async Task<bool> EnvironmentConfiguration()
		{
			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_HOST, false, out this.host))
				return false;

			if (this.TryGetEnvironmentVariable(GATEWAY_XMPP_TRANSPORT, false, out string Value))
			{
				if (!Enum.TryParse(Value, out XmppTransportMethod Method))
				{
					this.LogEnvironmentError("Invalid transport method.", GATEWAY_XMPP_TRANSPORT, Value);
					return false;
				}

				this.transportMethod = Method;
			}

			switch (this.transportMethod)
			{
				case XmppTransportMethod.C2S:
					Value = Environment.GetEnvironmentVariable(GATEWAY_XMPP_PORT);
					if (!string.IsNullOrEmpty(Value))
					{
						if (!int.TryParse(Value, out int i) || i <= 0 || i > 65535)
						{
							this.LogEnvironmentVariableInvalidRangeError(1, 65535, GATEWAY_XMPP_PORT, Value);
							return false;
						}

						this.port = i;
						this.customBinding = false;
					}
					break;

				case XmppTransportMethod.BOSH:
					if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_BOSHURL, true, out this.boshUrl))
						return false;

					this.customBinding = true;
					break;

				case XmppTransportMethod.WS:
					if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_WSURL, true, out this.wsUrl))
						return false;

					this.customBinding = true;
					break;

				default:
					this.LogEnvironmentError("Unhandled binding method.", GATEWAY_XMPP_TRANSPORT, this.transportMethod);
					return false;
			}

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_CREATE, out this.createAccount, false))
				return false;

			if (this.createAccount)
			{
				Value = Environment.GetEnvironmentVariable(GATEWAY_XMPP_CREATE_KEY);
				if (string.IsNullOrEmpty(Value))
				{
					if (!clp.ContainsKey(this.host))
					{
						this.LogEnvironmentError("Host is not a featured broker. If an account is to be created, an API Key must be provided.",
							GATEWAY_XMPP_CREATE_KEY, Value);
						return false;
					}
				}
				else
				{
					if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_CREATE_SECRET, true, out string Value2))
						return false;

					clp[this.host] = new KeyValuePair<string, string>(Value, Value2);
				}
			}

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_ACCOUNT, true, out this.account))
				return false;

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_PASSWORD, false, out this.password))
			{
				if (this.createAccount)
					this.password = RandomPassword.CreateRandomPassword();
				else
				{
					this.LogEnvironmentVariableMissingError(GATEWAY_XMPP_PASSWORD, Value);
					return false;
				}
			}

			this.password0 = this.password;
			this.passwordType = string.Empty;

			if (this.TryGetEnvironmentVariable(GATEWAY_XMPP_ACCOUNT_NAME, false, out Value))
				this.accountHumanReadableName = Value;

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_LOG, out this.sniffer, false))
				return false;

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_TRUST, out this.trustServer, false))
				return false;

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_OBS_AUTH, out this.allowInsecureMechanisms, false))
				return false;

			if (!this.TryGetEnvironmentVariable(GATEWAY_XMPP_CLEAR_PWD, out this.storePasswordInsteadOfHash, false))
				return false;

			this.testConnection = new TaskCompletionSource<bool>();
			try
			{
				Task _ = Task.Delay(30000).ContinueWith(Prev => this.testConnection?.TrySetException(new TimeoutException()));

				this.Connect(null);

				if (await this.testConnection.Task)
					return true;
				else
					return false;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return false;
			}
			finally
			{
				this.testConnection = null;
			}
		}
	}
}
