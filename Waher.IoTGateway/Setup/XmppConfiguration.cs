using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Security;

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
	public partial class XmppConfiguration : SystemMultiStepConfiguration
	{
		private static XmppConfiguration instance = null;

		private XmppTransportMethod transportMethod = XmppTransportMethod.C2S;
		private string host = string.Empty;
		private int port = XmppCredentials.DefaultPort;
		private string boshUrl = string.Empty;
		private string wsUrl = string.Empty;
		private string account = string.Empty;
		private string accountHumanReadableName = string.Empty;
		private string password = string.Empty;
		private string passwordType = string.Empty;
		private string thingRegistry = string.Empty;
		private string provisioning = string.Empty;
		private string events = string.Empty;
		private string pubSub = string.Empty;
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
			get { return this.transportMethod; }
			set { this.transportMethod = value; }
		}

		/// <summary>
		/// Host to connect to
		/// </summary>
		[DefaultValueStringEmpty]
		public string Host
		{
			get { return this.host; }
			set { this.host = value; }
		}

		/// <summary>
		/// Port number to use
		/// </summary>
		[DefaultValue(XmppCredentials.DefaultPort)]
		public int Port
		{
			get { return this.port; }
			set { this.port = value; }
		}

		/// <summary>
		/// BOSH URL to bind to.
		/// </summary>
		[DefaultValueStringEmpty]
		public string BoshUrl
		{
			get { return this.boshUrl; }
			set { this.boshUrl = value; }
		}

		/// <summary>
		/// Web-socket URL to bind to.
		/// </summary>
		[DefaultValueStringEmpty]
		public string WebSocketUrl
		{
			get { return this.wsUrl; }
			set { this.wsUrl = value; }
		}

		/// <summary>
		/// XMPP Account
		/// </summary>
		[DefaultValueStringEmpty]
		public string Account
		{
			get { return this.account; }
			set { this.account = value; }
		}

		/// <summary>
		/// Password
		/// </summary>
		[DefaultValueStringEmpty]
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		/// <summary>
		/// Type of password. Empty string = clear text, otherwise, type of hash.
		/// </summary>
		[DefaultValueStringEmpty]
		public string PasswordType
		{
			get { return this.passwordType; }
			set { this.passwordType = value; }
		}

		/// <summary>
		/// Human readable account name, if any.
		/// </summary>
		public string AccountHumanReadableName
		{
			get { return this.accountHumanReadableName; }
			set { this.accountHumanReadableName = value; }
		}

		/// <summary>
		/// JID of Thing Registry.
		/// </summary>
		[DefaultValueStringEmpty]
		public string ThingRegistry
		{
			get { return this.thingRegistry; }
			set { this.thingRegistry = value; }
		}

		/// <summary>
		/// JID of provisioning server.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Provisioning
		{
			get { return this.provisioning; }
			set { this.provisioning = value; }
		}

		/// <summary>
		/// JID of event log.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Events
		{
			get { return this.events; }
			set { this.events = value; }
		}

		/// <summary>
		/// JID of publish/subscribe component.
		/// </summary>
		[DefaultValueStringEmpty]
		public string PubSub
		{
			get { return this.pubSub; }
			set { this.pubSub = value; }
		}

		/// <summary>
		/// Bare JID
		/// </summary>
		[DefaultValueStringEmpty]
		public string BareJid
		{
			get { return this.bareJid; }
			set { this.bareJid = value; }
		}

		/// <summary>
		/// If communication should be sniffed.
		/// </summary>
		[DefaultValue(false)]
		public bool Sniffer
		{
			get { return this.sniffer; }
			set { this.sniffer = value; }
		}

		/// <summary>
		/// If server is to be trusted (true), or if certificate should be validated before connection is accepted (false).
		/// </summary>
		[DefaultValue(false)]
		public bool TrustServer
		{
			get { return this.trustServer; }
			set { this.trustServer = value; }
		}

		/// <summary>
		/// If insecure authentication methods should be allowed.
		/// </summary>
		[DefaultValue(false)]
		public bool AllowInsecureMechanisms
		{
			get { return this.allowInsecureMechanisms; }
			set { this.allowInsecureMechanisms = value; }
		}

		/// <summary>
		/// If password should be stored instead of hash.
		/// </summary>
		[DefaultValue(false)]
		public bool StorePasswordInsteadOfHash
		{
			get { return this.storePasswordInsteadOfHash; }
			set { this.storePasswordInsteadOfHash = value; }
		}

		/// <summary>
		/// If a custom binding is used.
		/// </summary>
		[DefaultValue(false)]
		public bool CustomBinding
		{
			get { return this.customBinding; }
			set { this.customBinding = value; }
		}

		/// <summary>
		/// If offline messages are supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool OfflineMessages
		{
			get { return this.offlineMessages; }
			set { this.offlineMessages = value; }
		}

		/// <summary>
		/// If blocking accounts is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Blocking
		{
			get { return this.blocking; }
			set { this.blocking = value; }
		}

		/// <summary>
		/// If reporting is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Reporting
		{
			get { return this.reporting; }
			set { this.reporting = value; }
		}

		/// <summary>
		/// If reporting abuse is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Abuse
		{
			get { return this.abuse; }
			set { this.abuse = value; }
		}

		/// <summary>
		/// If reporting spam is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool Spam
		{
			get { return this.spam; }
			set { this.spam = value; }
		}

		/// <summary>
		/// If the Personal Eventing Protocol (PEP) is supported by the server.
		/// </summary>
		[DefaultValue(false)]
		public bool PersonalEventing
		{
			get { return this.pep; }
			set { this.pep = value; }
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
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			return Gateway.ConfigureXmpp(this);
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
			WebServer.Register("/Settings/ConnectToHost", null, this.ConnectToHost, true, false, true);
			WebServer.Register("/Settings/RandomizePassword", null, this.RandomizePassword, true, false, true);

			return Task.CompletedTask;
		}

		private void ConnectToHost(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("host", out Obj) || !(Obj is string HostName))
				throw new BadRequestException();

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("port", out Obj) || !(Obj is int Port) || Port < 1 || Port > 65535)
				throw new BadRequestException();

			if (!Parameters.TryGetValue("boshUrl", out Obj) || !(Obj is string BoshUrl))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("customBinding", out Obj) || !(Obj is bool CustomBinding))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("trustServer", out Obj) || !(Obj is bool TrustServer))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("insecureMechanisms", out Obj) || !(Obj is bool InsecureMechanisms))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("storePassword", out Obj) || !(Obj is bool StorePasswordInsteadOfHash))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("sniffer", out Obj) || !(Obj is bool Sniffer))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("transport", out Obj) || !(Obj is string s2) || !Enum.TryParse<XmppTransportMethod>(s2, out XmppTransportMethod Method))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("account", out Obj) || !(Obj is string Account))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("password", out Obj) || !(Obj is string Password))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("createAccount", out Obj) || !(Obj is bool CreateAccount))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("accountName", out Obj) || !(Obj is string AccountName))
				throw new BadRequestException();

			this.host = HostName;
			this.port = Port;
			this.boshUrl = BoshUrl.Trim();
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
				this.password = Password;
				this.passwordType = string.Empty;
			}

			if (this.client != null)
			{
				this.client.Dispose();
				this.client = null;
			}

			Response.StatusCode = 200;

			this.Connect(TabID);
		}

		private void RandomizePassword(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			Response.StatusCode = 200;
			Response.ContentType = "text/plain";
			Response.Write(Hashes.BinaryToString(Gateway.NextBytes(32)));
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
				TrustServer = this.trustServer,
				Sniffer = this.sniffer,
				AllowEncryption = true,
				AllowCramMD5 = this.allowInsecureMechanisms,
				AllowDigestMD5 = this.allowInsecureMechanisms,
				AllowPlain = this.allowInsecureMechanisms,
				AllowRegistration = this.createAccount,
				AllowScramSHA1 = true,
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

			this.client.OnStateChanged += Client_OnStateChanged;
			this.client.Connect();
		}

		private async void Client_OnStateChanged(object Sender, XmppState NewState)
		{
			if (!(Sender is XmppClient Client))
				return;

			if (!Client.TryGetTag("TabID", out object Obj) || !(Obj is string TabID))
				return;

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
							ClientEvents.PushEvent(new string[] { TabID }, "ConnectionOK0", "Connection established.", false);

							this.client.Dispose();
							this.client = null;

							this.Step = 1;
							this.Updated = DateTime.Now;
							await Database.Update(this);
							return;
						}
						else
							Msg = "Authenticating user.";
						break;

					case XmppState.Binding:
						Msg = "Binding to resource.";
						break;

					case XmppState.Connected:
						this.bareJid = Client.BareJID;

						if (this.createAccount && !string.IsNullOrEmpty(this.accountHumanReadableName))
						{
							ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Setting vCard.", false);

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

						ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking server features.", false);
						ServiceDiscoveryEventArgs e = await Client.ServiceDiscoveryAsync(null, string.Empty, string.Empty);

						if (e.Ok)
						{
							this.offlineMessages = e.HasFeature("msgoffline");
							this.blocking = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceBlocking);
							this.reporting = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceReporting);
							this.abuse = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceAbuseReason);
							this.spam = e.HasFeature(Networking.XMPP.Abuse.AbuseClient.NamespaceSpamReason);
						}
						else
						{
							this.offlineMessages = false;
							this.blocking = false;
							this.reporting = false;
							this.abuse = false;
							this.spam = false;
						}

						ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking account features.", false);
						e = await Client.ServiceDiscoveryAsync(null, Client.BareJID, string.Empty);

						this.pep = e.Ok && this.ContainsIdentity("pep", "pubsub", e);

						ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking server components.", false);
						ServiceItemsDiscoveryEventArgs e2 = await Client.ServiceItemsDiscoveryAsync(null, string.Empty, string.Empty);

						this.thingRegistry = string.Empty;
						this.provisioning = string.Empty;
						this.events = string.Empty;
						this.pubSub = string.Empty;

						if (e2.Ok)
						{
							foreach (Item Item in e2.Items)
							{
								ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking component features for " + Item.JID, false);

								e = await Client.ServiceDiscoveryAsync(null, Item.JID, string.Empty);

								if (e.HasFeature(Networking.XMPP.Provisioning.ThingRegistryClient.NamespaceDiscovery))
									this.thingRegistry = Item.JID;

								if (e.HasFeature(Networking.XMPP.Provisioning.ProvisioningClient.NamespaceProvisioningDevice))
									this.provisioning = Item.JID;

								if (e.HasFeature(Networking.XMPP.PubSub.PubSubClient.NamespacePubSub) && this.ContainsIdentity("service", "pubsub", e))
									this.pubSub = Item.JID;

								if (e.HasFeature(Waher.Events.XMPP.XmppEventSink.NamespaceEventLogging))
									this.events = Item.JID;
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
							{ "pep", this.pep ? this.bareJid : string.Empty },
							{ "thingRegistry", this.thingRegistry },
							{ "provisioning", this.provisioning },
							{ "eventLog", this.events },
							{ "pubSub", this.pubSub }
						};

						ClientEvents.PushEvent(new string[] { TabID }, "ConnectionOK1", JSON.Encode(ConnectionInfo, false), true);

						this.client.Dispose();
						this.client = null;

						this.Step = 2;
						this.Updated = DateTime.Now;
						await Database.Update(this);
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

							ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Unable to connect properly. Looking for alternative ways to connect.", false);
							ClientEvents.PushEvent(new string[] { TabID }, "ShowCustomProperties", "{\"visible\":true}", true);

							using (HttpClient HttpClient = new HttpClient(new HttpClientHandler()
							{
#if !NETFW
								ServerCertificateCustomValidationCallback = this.RemoteCertificateValidationCallback,
#endif
								UseCookies = false
							})
							{
								Timeout = TimeSpan.FromMilliseconds(60000)
							})
							{
								try
								{
									HttpResponseMessage Response = await HttpClient.GetAsync("http://" + this.host + "/.well-known/host-meta");
									Response.EnsureSuccessStatusCode();

									Stream Stream = await Response.Content.ReadAsStreamAsync(); // Regardless of status code, we check for XML content.
									byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
									string CharSet = Response.Content.Headers.ContentType.CharSet;
									Encoding Encoding;

									if (string.IsNullOrEmpty(CharSet))
										Encoding = Encoding.UTF8;
									else
										Encoding = System.Text.Encoding.GetEncoding(CharSet);

									string XmlResponse = Encoding.GetString(Bin);
									XmlDocument Doc = new XmlDocument();
									Doc.LoadXml(XmlResponse);

									if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "XRD")
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

											ClientEvents.PushEvent(new string[] { TabID }, "ShowTransport", "{\"method\":\"WS\"}", true);

											this.Connect(TabID);

											return;
										}
										else if (!string.IsNullOrEmpty(BoshUrl))
										{
											this.boshUrl = BoshUrl;
											this.transportMethod = XmppTransportMethod.BOSH;

											ClientEvents.PushEvent(new string[] { TabID }, "ShowTransport", "{\"method\":\"BOSH\"}", true);

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
						}
						else
						{
							Msg = "Unable to connect properly.";
							Error = true;

							if (Client.TryGetTag("StartedAuthentication", out Obj) && Obj is bool b && b)
							{
								if (this.createAccount)
									ClientEvents.PushEvent(new string[] { TabID }, "ShowFail2", Msg, false);
								else
									ClientEvents.PushEvent(new string[] { TabID }, "ShowFail1", Msg, false);
								return;
							}
						}

						if (Error)
						{
							ClientEvents.PushEvent(new string[] { TabID }, "ConnectionError", Msg, false);

							this.client.Dispose();
							this.client = null;

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

				ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Msg, false);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", ex.Message, false);
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

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
				if (featuredServers == null)
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
	}
}
