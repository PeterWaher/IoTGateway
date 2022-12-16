using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Script;
using Waher.Security;
using Waher.Security.ACME;
using Waher.Security.PKCS;

namespace Waher.IoTGateway.Setup
{
	/// <summary>
	/// Domain Configuration
	/// </summary>
	public class DomainConfiguration : SystemMultiStepConfiguration
	{
		private static DomainConfiguration instance = null;

		private HttpResource testDomainNames = null;
		private HttpResource testDomainName = null;
		private HttpResource testCA = null;
		private HttpResource acmeChallenge = null;

		private string[] alternativeDomains = null;
		private byte[] certificate = null;
		private byte[] privateKey = null;
		private byte[] pfx = null;
		private string domain = string.Empty;
		private string acmeDirectory = string.Empty;
		private string contactEMail = string.Empty;
		private string urlToS = string.Empty;
		private string password = string.Empty;
		private string openSslPath = string.Empty;
		private string dynDnsTemplate = string.Empty;
		private string checkIpScript = string.Empty;
		private string updateIpScript = string.Empty;
		private string dynDnsAccount = string.Empty;
		private string dynDnsPassword = string.Empty;
		private int dynDnsInterval = 300;
		private bool useDomainName = false;
		private bool dynamicDns = false;
		private bool useEncryption = true;
		private bool customCA = false;
		private bool acceptToS = false;

		private string challenge = string.Empty;
		private string token = string.Empty;

		/// <summary>
		/// Current instance of configuration.
		/// </summary>
		public static DomainConfiguration Instance => instance;

		/// <summary>
		/// Principal domain name
		/// </summary>
		[DefaultValueStringEmpty]
		public string Domain
		{
			get => this.domain;
			set => this.domain = value;
		}

		/// <summary>
		/// Alternative domain names
		/// </summary>
		[DefaultValueNull]
		public string[] AlternativeDomains
		{
			get => this.alternativeDomains;
			set => this.alternativeDomains = value;
		}

		/// <summary>
		/// If the server uses a domain name.
		/// </summary>
		[DefaultValue(false)]
		public bool UseDomainName
		{
			get => this.useDomainName;
			set => this.useDomainName = value;
		}

		/// <summary>
		/// If the server uses a dynamic DNS service.
		/// </summary>
		[DefaultValue(false)]
		public bool DynamicDns
		{
			get => this.dynamicDns;
			set => this.dynamicDns = value;
		}

		/// <summary>
		/// If the server uses server-side encryption.
		/// </summary>
		[DefaultValue(true)]
		public bool UseEncryption
		{
			get => this.useEncryption;
			set => this.useEncryption = value;
		}

		/// <summary>
		/// If a custom Certificate Authority is to be used
		/// </summary>
		[DefaultValue(false)]
		public bool CustomCA
		{
			get => this.customCA;
			set => this.customCA = value;
		}

		/// <summary>
		/// If a custom Certificate Authority is to be used, this property holds the URL to their ACME directory.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AcmeDirectory
		{
			get => this.acmeDirectory;
			set => this.acmeDirectory = value;
		}

		/// <summary>
		/// Contact e-mail address
		/// </summary>
		[DefaultValueStringEmpty]
		public string ContactEMail
		{
			get => this.contactEMail;
			set => this.contactEMail = value;
		}

		/// <summary>
		/// CA Terms of Service
		/// </summary>
		[DefaultValueStringEmpty]
		public string UrlToS
		{
			get => this.urlToS;
			set => this.urlToS = value;
		}

		/// <summary>
		/// If the CA Terms of Service has been accepted.
		/// </summary>
		[DefaultValue(false)]
		public bool AcceptToS
		{
			get => this.acceptToS;
			set => this.acceptToS = value;
		}

		/// <summary>
		/// Certificate
		/// </summary>
		[DefaultValueNull]
		public byte[] Certificate
		{
			get => this.certificate;
			set => this.certificate = value;
		}

		/// <summary>
		/// Private Key
		/// </summary>
		[DefaultValueNull]
		public byte[] PrivateKey
		{
			get => this.privateKey;
			set => this.privateKey = value;
		}

		/// <summary>
		/// PFX container for certificate and private key, if available.
		/// </summary>
		[DefaultValueNull]
		public byte[] PFX
		{
			get => this.pfx;
			set => this.pfx = value;
		}

		/// <summary>
		/// Password for PFX file, if any.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Password
		{
			get => this.password;
			set => this.password = value;
		}

		/// <summary>
		/// Path to OpenSSL
		/// </summary>
		[DefaultValueStringEmpty]
		public string OpenSslPath
		{
			get => this.openSslPath;
			set => this.openSslPath = value;
		}

		/// <summary>
		/// Dynamic DNS Template
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsTemplate
		{
			get => this.dynDnsTemplate;
			set => this.dynDnsTemplate = value;
		}

		/// <summary>
		/// Script to use to evaluate the current IP Address.
		/// </summary>
		[DefaultValueStringEmpty]
		public string CheckIpScript
		{
			get => this.checkIpScript;
			set => this.checkIpScript = value;
		}

		/// <summary>
		/// Script to use to update the current IP Address.
		/// </summary>
		[DefaultValueStringEmpty]
		public string UpdateIpScript
		{
			get => this.updateIpScript;
			set => this.updateIpScript = value;
		}

		/// <summary>
		/// Account Name for the Dynamic DNS service
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsAccount
		{
			get => this.dynDnsAccount;
			set => this.dynDnsAccount = value;
		}

		/// <summary>
		/// Interval (in seconds) for checking if the IP address has changed.
		/// </summary>
		[DefaultValue(300)]
		public int DynDnsInterval
		{
			get => this.dynDnsInterval;
			set => this.dynDnsInterval = value;
		}

		/// <summary>
		/// Password for the Dynamic DNS service
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsPassword
		{
			get => this.dynDnsPassword;
			set => this.dynDnsPassword = value;
		}

		/// <summary>
		/// If the CA has a Terms of Service.
		/// </summary>
		public bool HasToS => !string.IsNullOrEmpty(this.urlToS);

		/// <summary>
		/// Resource to be redirected to, to perform the configuration.
		/// </summary>
		public override string Resource => "/Settings/Domain.md";

		/// <summary>
		/// Priority of the setting. Configurations are sorted in ascending order.
		/// </summary>
		public override int Priority => 200;

		/// <summary>
		/// Gets a title for the system configuration.
		/// </summary>
		/// <param name="Language">Current language.</param>
		/// <returns>Title string</returns>
		public override Task<string> Title(Language Language)
		{
			return Language.GetStringAsync(typeof(Gateway), 3, "Domain");
		}

		/// <summary>
		/// Is called during startup to configure the system.
		/// </summary>
		public override Task ConfigureSystem()
		{
			return Gateway.ConfigureDomain(this);
		}

		/// <summary>
		/// Sets the static instance of the configuration.
		/// </summary>
		/// <param name="Configuration">Configuration object</param>
		public override void SetStaticInstance(ISystemConfiguration Configuration)
		{
			instance = Configuration as DomainConfiguration;
		}

		/// <summary>
		/// Initializes the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task InitSetup(HttpServer WebServer)
		{
			this.testDomainNames = WebServer.Register("/Settings/TestDomainNames", null, this.TestDomainNames, true, false, true);
			this.testDomainName = WebServer.Register("/Settings/TestDomainName", this.TestDomainName, true, false, true);
			this.testCA = WebServer.Register("/Settings/TestCA", null, this.TestCA, true, false, true);
			this.acmeChallenge = WebServer.Register("/.well-known/acme-challenge", this.AcmeChallenge, true, true, true);

			return base.InitSetup(WebServer);
		}

		/// <summary>
		/// Unregisters the setup object.
		/// </summary>
		/// <param name="WebServer">Current Web Server object.</param>
		public override Task UnregisterSetup(HttpServer WebServer)
		{
			WebServer.Unregister(this.testDomainNames);
			WebServer.Unregister(this.testDomainName);
			WebServer.Unregister(this.testCA);
			WebServer.Unregister(this.acmeChallenge);

			return base.UnregisterSetup(WebServer);
		}

		/// <summary>
		/// Minimum required privilege for a user to be allowed to change the configuration defined by the class.
		/// </summary>
		protected override string ConfigPrivilege => "Admin.Communication.Domain";

		private async Task TestDomainNames(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("domainName", out Obj) || !(Obj is string DomainName) ||
				!Parameters.TryGetValue("dynamicDns", out Obj) || !(Obj is bool DynamicDns) ||
				!Parameters.TryGetValue("dynDnsTemplate", out Obj) || !(Obj is string DynDnsTemplate) ||
				!Parameters.TryGetValue("checkIpScript", out Obj) || !(Obj is string CheckIpScript) ||
				!Parameters.TryGetValue("updateIpScript", out Obj) || !(Obj is string UpdateIpScript) ||
				!Parameters.TryGetValue("dynDnsAccount", out Obj) || !(Obj is string DynDnsAccount) ||
				!Parameters.TryGetValue("dynDnsPassword", out Obj) || !(Obj is string DynDnsPassword) ||
				!Parameters.TryGetValue("dynDnsInterval", out Obj) || !(Obj is int DynDnsInterval))
			{
				throw new BadRequestException();
			}

			List<string> AlternativeNames = new List<string>();
			int Index = 0;

			while (Parameters.TryGetValue("altDomainName" + Index.ToString(), out Obj) && Obj is string AltDomainName && !string.IsNullOrEmpty(AltDomainName))
			{
				AlternativeNames.Add(AltDomainName);
				Index++;
			}

			if (Parameters.TryGetValue("altDomainName", out Obj) && Obj is string AltDomainName2 && !string.IsNullOrEmpty(AltDomainName2))
				AlternativeNames.Add(AltDomainName2);

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			this.dynamicDns = DynamicDns;
			this.dynDnsTemplate = DynDnsTemplate;
			this.checkIpScript = CheckIpScript;
			this.updateIpScript = UpdateIpScript;
			this.dynDnsAccount = DynDnsAccount;
			this.dynDnsPassword = DynDnsPassword;
			this.dynDnsInterval = DynDnsInterval;
			this.domain = DomainName;
			this.alternativeDomains = AlternativeNames.Count == 0 ? null : AlternativeNames.ToArray();
			this.useDomainName = true;

			Response.StatusCode = 200;

			this.Test(TabID);
		}

		private Task TestDomainName(HttpRequest Request, HttpResponse Response)
		{
			Response.StatusCode = 200;
			Response.ContentType = "text/plain";
			return Response.Write(this.token);
		}

		private async void Test(string TabID)
		{
			try
			{
				if (!string.IsNullOrEmpty(this.domain))
				{
					if (!await this.Test(TabID, this.domain))
					{
						await ClientEvents.PushEvent(new string[] { TabID }, "NameNotValid", this.domain, false, "User");
						return;
					}
				}

				if (!(this.alternativeDomains is null))
				{
					foreach (string AltDomainName in this.alternativeDomains)
					{
						if (!await this.Test(TabID, AltDomainName))
						{
							await ClientEvents.PushEvent(new string[] { TabID }, "NameNotValid", AltDomainName, false, "User");
							return;
						}
					}
				}

				if (this.Step < 1)
					this.Step = 1;

				this.Updated = DateTime.Now;
				await Database.Update(this);

				await ClientEvents.PushEvent(new string[] { TabID }, "NamesOK", string.Empty, false, "User");
			}
			catch (Exception ex)
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", ex.Message, false, "User");
			}
		}

		internal async Task<bool> CheckDynamicIp()
		{
			try
			{
				if (!this.useDomainName || !this.dynamicDns)
					return true;

				await this.CheckDynamicIp(null, this.domain);

				foreach (string AlternativeDomain in this.alternativeDomains)
					await this.CheckDynamicIp(null, AlternativeDomain);

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}

		internal async Task<bool> CheckDynamicIp(string TabID, string DomainName)
		{
			string Msg = await this.CheckDynamicIp(DomainName, async (_, Status) =>
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
			});

			if (!string.IsNullOrEmpty(Msg))
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", Msg, false, "User");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if Dynamic IP configuration is correct
		/// </summary>
		/// <param name="DomainName">Domain Name</param>
		/// <param name="Status">Status callback method.</param>
		/// <returns>null if successful, error message if failed.</returns>
		public async Task<string> CheckDynamicIp(string DomainName, EventHandlerAsync<string> Status)
		{
			if (!this.dynamicDns)
				return null;

			Expression CheckIpScript;
			Expression UpdateIpScript;

			try
			{
				CheckIpScript = new Expression(this.checkIpScript);
			}
			catch (Exception ex)
			{
				return "Unable to parse script checking current IP Address: " + ex.Message;
			}

			try
			{
				UpdateIpScript = new Expression(this.updateIpScript);
			}
			catch (Exception ex)
			{
				return "Unable to parse script updating the dynamic DNS server: " + ex.Message;
			}

			await Status.Invoke(this, "Checking current IP Address.");

			Variables Variables = new Variables();
			object Result;

			try
			{
				Result = await CheckIpScript.EvaluateAsync(Variables);
			}
			catch (Exception ex)
			{
				return "Unable to get current IP Address: " + ex.Message;
			}

			if (!(Result is string CurrentIP) || !IPAddress.TryParse(CurrentIP, out IPAddress _))
				return "Unable to get current IP Address. Unexpected response.";

			await Status.Invoke(this, "Current IP Address: " + CurrentIP);

			string LastIP = await RuntimeSettings.GetAsync("Last.IP." + DomainName, string.Empty);

			if (LastIP == CurrentIP)
				await Status.Invoke(this, "IP Address has not changed for " + DomainName + ".");
			else
			{
				try
				{
					await Status.Invoke(this, "Updating IP address for " + DomainName + " to " + CurrentIP + ".");

					Variables["Account"] = this.dynDnsAccount;
					Variables["Password"] = this.dynDnsPassword;
					Variables["IP"] = CurrentIP;
					Variables["Domain"] = DomainName;

					Result = await UpdateIpScript.EvaluateAsync(Variables);

					await RuntimeSettings.SetAsync("Last.IP." + DomainName, CurrentIP);
				}
				catch (Exception ex)
				{
					return "Unable to register new dynamic IP Address: " + ex.Message;
				}
			}

			return null;
		}

		private async Task<bool> Test(string TabID, string DomainName)
		{
			string Msg = await this.Test(DomainName, async (_, Status) =>
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
			});

			if (!string.IsNullOrEmpty(Msg))
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", Msg, false, "User");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the domain points to the server.
		/// </summary>
		/// <param name="DomainName">Domain Name</param>
		/// <param name="Status">Status callback method.</param>
		/// <returns>null if successful, error message if failed.</returns>
		public async Task<string> Test(string DomainName, EventHandlerAsync<string> Status)
		{
			await Status.Invoke(this, "Testing " + DomainName + "...");

			string Msg = await this.CheckDynamicIp(DomainName, Status);
			if (!string.IsNullOrEmpty(Msg))
				return Msg;

			this.token = Hashes.BinaryToString(Gateway.NextBytes(32));

			using (HttpClient HttpClient = new HttpClient()
			{
				Timeout = TimeSpan.FromMilliseconds(10000)
			})
			{
				try
				{
					StringBuilder Url = new StringBuilder();
					int[] HttpPorts = Gateway.GetConfigPorts("HTTP");

					Url.Append("http://");
					Url.Append(DomainName);

					if (Array.IndexOf<int>(HttpPorts, 80) < 0 && HttpPorts.Length > 0)
					{
						Url.Append(':');
						Url.Append(HttpPorts[0].ToString());
					}

					Url.Append("/Settings/TestDomainName");

					HttpResponseMessage Response = await HttpClient.GetAsync("http://" + DomainName + "/Settings/TestDomainName");
					if (!Response.IsSuccessStatusCode)
					{
						return "Domain name does not point to this machine.";
					}

					byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
					string Token = Encoding.ASCII.GetString(Bin);

					if (Token != this.token)
						return "Unexpected response returned. Domain name does not point to this machine.";
				}
				catch (TimeoutException)
				{
					return "Time-out. Check that the domain name points to this machine.";
				}
				catch (Exception ex)
				{
					return "Unable to validate domain name: " + ex.Message;
				}
			}

			await Status.Invoke(this, "Domain name valid.");

			return null;
		}

		private async Task TestCA(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = await Request.DecodeDataAsync();
			if (!(Obj is Dictionary<string, object> Parameters))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("useEncryption", out Obj) || !(Obj is bool UseEncryption))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("customCA", out Obj) || !(Obj is bool CustomCA))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("acmeDirectory", out Obj) || !(Obj is string AcmeDirectory))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("contactEMail", out Obj) || !(Obj is string ContactEMail))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("acceptToS", out Obj) || !(Obj is bool AcceptToS))
				throw new BadRequestException();

			if (!Parameters.TryGetValue("domainName", out Obj) || !(Obj is string DomainName) ||
				!Parameters.TryGetValue("dynamicDns", out Obj) || !(Obj is bool DynamicDns) ||
				!Parameters.TryGetValue("dynDnsTemplate", out Obj) || !(Obj is string DynDnsTemplate) ||
				!Parameters.TryGetValue("checkIpScript", out Obj) || !(Obj is string CheckIpScript) ||
				!Parameters.TryGetValue("updateIpScript", out Obj) || !(Obj is string UpdateIpScript) ||
				!Parameters.TryGetValue("dynDnsAccount", out Obj) || !(Obj is string DynDnsAccount) ||
				!Parameters.TryGetValue("dynDnsPassword", out Obj) || !(Obj is string DynDnsPassword) ||
				!Parameters.TryGetValue("dynDnsInterval", out Obj) || !(Obj is int DynDnsInterval))
				throw new BadRequestException();

			List<string> AlternativeNames = new List<string>();
			int Index = 0;

			while (Parameters.TryGetValue("altDomainName" + Index.ToString(), out Obj) && Obj is string AltDomainName && !string.IsNullOrEmpty(AltDomainName))
			{
				AlternativeNames.Add(AltDomainName);
				Index++;
			}

			if (Parameters.TryGetValue("altDomainName", out Obj) && Obj is string AltDomainName2 && !string.IsNullOrEmpty(AltDomainName2))
				AlternativeNames.Add(AltDomainName2);

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
				throw new BadRequestException();

			this.dynamicDns = DynamicDns;
			this.dynDnsTemplate = DynDnsTemplate;
			this.checkIpScript = CheckIpScript;
			this.updateIpScript = UpdateIpScript;
			this.dynDnsAccount = DynDnsAccount;
			this.dynDnsPassword = DynDnsPassword;
			this.dynDnsInterval = DynDnsInterval;
			this.domain = DomainName;
			this.alternativeDomains = AlternativeNames.Count == 0 ? null : AlternativeNames.ToArray();
			this.useDomainName = true;
			this.useEncryption = UseEncryption;
			this.customCA = CustomCA;
			this.acmeDirectory = AcmeDirectory;
			this.contactEMail = ContactEMail;
			this.acceptToS = AcceptToS;

			Response.StatusCode = 200;

			if (!this.inProgress)
			{
				this.inProgress = true;
				Task _ = Task.Run(async () =>
				{
					try
					{
						await this.CreateCertificate(TabID);
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
					}
				});
			}
		}

		private bool inProgress = false;

		/// <summary>
		/// Adds a domain to the list of domains supported by the gateway.
		/// </summary>
		/// <param name="Name">Domain name.</param>
		/// <param name="Status">Status is reported to this event handler.</param>
		/// <returns>Null, if domain could be added, otherwise contains an error message.</returns>
		public async Task<string> AddDomain(string Name, EventHandlerAsync<string> Status)
		{
			if (this.IsDomainRegistered(Name))
				return "Domain already registered.";

			string Msg = await this.Test(Name, Status);
			if (!string.IsNullOrEmpty(Msg))
				return Msg;

			string DomainBak = this.domain;
			bool UseDomainBak = this.useDomainName;
			string[] AlternativeBak = this.alternativeDomains;

			if (string.IsNullOrEmpty(this.domain))
			{
				this.domain = Name;
				this.useDomainName = true;
			}
			else if (this.alternativeDomains is null)
				this.alternativeDomains = new string[] { Name };
			else
			{
				int c = this.alternativeDomains.Length;
				string[] NewArray = new string[c + 1];
				this.alternativeDomains.CopyTo(NewArray, 0);
				NewArray[c] = Name;
				this.alternativeDomains = NewArray;
			}

			Msg = await this.CreateCertificate(Status, (_, __) => Task.CompletedTask);
			if (!string.IsNullOrEmpty(Msg))
			{
				this.domain = DomainBak;
				this.useDomainName = UseDomainBak;
				this.alternativeDomains = AlternativeBak;

				return Msg;
			}

			await Database.Update(this);

			return null;
		}

		/// <summary>
		/// Removes a domain from the list of domains supported by the gateway.
		/// </summary>
		/// <param name="Name">Domain name.</param>
		/// <param name="Status">Status is reported to this event handler.</param>
		/// <returns>Null, if domain could be removed, otherwise contains an error message.</returns>
		public async Task<string> RemoveDomain(string Name, EventHandlerAsync<string> Status)
		{
			if (!this.IsDomainRegistered(Name))
				return "Domain not registered.";

			string DomainBak = this.domain;
			bool UseDomainBak = this.useDomainName;
			string[] AlternativeBak = this.alternativeDomains;
			string[] NewArray;

			if (string.Compare(this.domain, Name, true) == 0)
			{
				if ((this.alternativeDomains?.Length ?? 0) == 0)
				{
					this.domain = string.Empty;
					this.useDomainName = false;
				}
				else
				{
					this.domain = this.alternativeDomains[0];

					int c = this.alternativeDomains.Length;

					if (c == 1)
						this.alternativeDomains = null;
					else
					{
						NewArray = new string[c - 1];
						Array.Copy(this.alternativeDomains, 1, NewArray, 0, c - 1);
						this.alternativeDomains = NewArray;
					}
				}
			}
			else if (!(this.alternativeDomains is null))
			{
				int c = this.alternativeDomains.Length;
				if (c == 1)
					this.alternativeDomains = null;
				else
				{
					int i = Array.IndexOf(this.alternativeDomains, Name);

					NewArray = new string[c - 1];

					if (i > 0)
						Array.Copy(this.alternativeDomains, 0, NewArray, 0, i);

					if (i < c - 1)
						Array.Copy(this.alternativeDomains, i + 1, NewArray, i, c - i - 1);

					this.alternativeDomains = NewArray;
				}
			}

			if (this.useDomainName)
			{
				string Msg = await this.CreateCertificate(Status, (_, __) => Task.CompletedTask);
				if (!string.IsNullOrEmpty(Msg))
				{
					this.domain = DomainBak;
					this.useDomainName = UseDomainBak;
					this.alternativeDomains = AlternativeBak;

					return Msg;
				}
			}

			await Database.Update(this);

			return null;
		}

		/// <summary>
		/// Checks if a domain name is registered.
		/// </summary>
		/// <param name="Name">Domain name to check.</param>
		/// <returns>If the name is already registered.</returns>
		public bool IsDomainRegistered(string Name)
		{
			if (string.Compare(Name, this.domain, true) == 0)
				return true;

			if (this.alternativeDomains is null)
				return false;

			foreach (string Alternative in this.alternativeDomains)
			{
				if (string.Compare(Name, Alternative, true) == 0)
					return true;
			}

			return false;
		}

		internal Task<bool> CreateCertificate()
		{
			return CreateCertificate((string)null);
		}

		internal async Task<bool> CreateCertificate(string TabID)
		{
			try
			{
				string Msg = await this.CreateCertificate(
					async (_, Status) =>
					{
						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
					},
					async (_, URL) =>
					{
						await ClientEvents.PushEvent(new string[] { TabID }, "TermsOfService", URL, false, "User");
					});

				if (string.IsNullOrEmpty(Msg))
				{
					if (!string.IsNullOrEmpty(TabID))
						await ClientEvents.PushEvent(new string[] { TabID }, "CertificateOk", string.Empty, false, "User");

					return true;
				}
				else
				{
					if (!string.IsNullOrEmpty(TabID))
						await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", Msg, false, "User");

					return false;
				}
			}
			catch (Exception ex)
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", ex.Message, false, "User");

				return false;
			}
		}

		internal async Task<string> CreateCertificate(EventHandlerAsync<string> Status, EventHandlerAsync<string> TermsOfService)
		{
			try
			{
				string URL = this.customCA ? this.acmeDirectory : "https://acme-v02.api.letsencrypt.org/directory";
				RSAParameters Parameters;
				CspParameters CspParams = new CspParameters()
				{
					Flags = CspProviderFlags.UseMachineKeyStore,
					KeyContainerName = "IoTGateway:" + URL
				};

				try
				{
					bool Ok;

					using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
					{
						Parameters = RSA.ExportParameters(true);

						if (RSA.KeySize < 4096)
						{
							RSA.PersistKeyInCsp = false;
							RSA.Clear();
							Ok = false;
						}
						else
							Ok = true;
					}

					if (!Ok)
					{
						using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
						{
							Parameters = RSA.ExportParameters(true);
						}
					}
				}
				catch (CryptographicException ex)
				{
					throw new CryptographicException("Unable to get access to cryptographic key for \"IoTGateway:" + URL +
						"\". Was the database created using another user?", ex);
				}

				using (AcmeClient Client = new AcmeClient(new Uri(URL), Parameters))
				{
					await Status.Invoke(this, "Connecting to directory.");

					AcmeDirectory AcmeDirectory = await Client.GetDirectory();

					if (AcmeDirectory.ExternalAccountRequired)
						await Status.Invoke(this, "An external account is required.");

					if (AcmeDirectory.TermsOfService != null)
					{
						URL = AcmeDirectory.TermsOfService.ToString();
						await Status.Invoke(this, "Terms of service available on: " + URL);
						await TermsOfService.Invoke(this, URL);

						this.urlToS = URL;

						if (!this.acceptToS)
							return "You need to accept the terms of service.";
					}

					if (AcmeDirectory.Website != null)
						await Status.Invoke(this, "Web site available on: " + AcmeDirectory.Website.ToString());

					await Status.Invoke(this, "Getting account.");

					List<string> Names = new List<string>();

					if (!string.IsNullOrEmpty(this.domain))
						Names.Add(this.domain);

					if (!(this.alternativeDomains is null))
					{
						foreach (string Name in this.alternativeDomains)
						{
							if (!Names.Contains(Name))
								Names.Add(Name);
						}
					}
					string[] DomainNames = Names.ToArray();

					AcmeAccount Account;

					try
					{
						Account = await Client.GetAccount();

						await Status.Invoke(this, "Account found.");
						await Status.Invoke(this, "Created: " + Account.CreatedAt.ToString());
						await Status.Invoke(this, "Initial IP: " + Account.InitialIp);
						await Status.Invoke(this, "Status: " + Account.Status.ToString());

						if (string.IsNullOrEmpty(this.contactEMail))
						{
							if (Account.Contact != null && Account.Contact.Length != 0)
							{
								await Status.Invoke(this, "Updating contact URIs in account.");
								Account = await Account.Update(new string[0]);
								await Status.Invoke(this, "Account updated.");
							}
						}
						else
						{
							if (Account.Contact is null || Account.Contact.Length != 1 || Account.Contact[0] != "mailto:" + this.contactEMail)
							{
								await Status.Invoke(this, "Updating contact URIs in account.");
								Account = await Account.Update(new string[] { "mailto:" + this.contactEMail });
								await Status.Invoke(this, "Account updated.");
							}
						}
					}
					catch (AcmeAccountDoesNotExistException)
					{
						await Status.Invoke(this, "Account not found.");
						await Status.Invoke(this, "Creating account.");

						Account = await Client.CreateAccount(string.IsNullOrEmpty(this.contactEMail) ? new string[0] : new string[] { "mailto:" + this.contactEMail },
							this.acceptToS);

						await Status.Invoke(this, "Account created.");
						await Status.Invoke(this, "Status: " + Account.Status.ToString());
					}

					await Status.Invoke(this, "Generating new key.");
					await Account.NewKey();

					using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
					{
						RSA.ImportParameters(Client.ExportAccountKey(true));
					}

					await Status.Invoke(this, "New key generated.");

					await Status.Invoke(this, "Creating order.");
					AcmeOrder Order;

					try
					{
						Order = await Account.OrderCertificate(DomainNames, null, null);
					}
					catch (AcmeMalformedException)  // Not sure why this is necessary. Perhaps because it takes time to propagate the keys correctly on the remote end?
					{
						await Task.Delay(5000);
						await Status.Invoke(this, "Retrying.");
						Order = await Account.OrderCertificate(DomainNames, null, null);
					}

					await Status.Invoke(this, "Order created.");

					AcmeAuthorization[] Authorizations;

					await Status.Invoke(this, "Getting authorizations.");
					try
					{
						Authorizations = await Order.GetAuthorizations();
					}
					catch (AcmeMalformedException)  // Not sure why this is necessary. Perhaps because it takes time to propagate the keys correctly on the remote end?
					{
						await Task.Delay(5000);
						await Status.Invoke(this, "Retrying.");
						Authorizations = await Order.GetAuthorizations();
					}

					foreach (AcmeAuthorization Authorization in Authorizations)
					{
						await Status.Invoke(this, "Processing authorization for " + Authorization.Value);

						AcmeChallenge Challenge;
						bool Acknowledged = false;
						int Index = 1;
						int NrChallenges = Authorization.Challenges.Length;

						for (Index = 1; Index <= NrChallenges; Index++)
						{
							Challenge = Authorization.Challenges[Index - 1];

							if (Challenge is AcmeHttpChallenge HttpChallenge)
							{
								this.challenge = "/" + HttpChallenge.Token;
								this.token = HttpChallenge.KeyAuthorization;

								await Status.Invoke(this, "Acknowleding challenge.");

								string Msg = await this.CheckDynamicIp(Authorization.Value, Status);
								if (string.IsNullOrEmpty(Msg))
								{
									Challenge = await HttpChallenge.AcknowledgeChallenge();
									await Status.Invoke(this, "Challenge acknowledged: " + Challenge.Status.ToString());

									Acknowledged = true;
								}
								else
									return Msg;
							}
						}

						if (!Acknowledged)
							return "No automated method found to respond to any of the authorization challenges.";

						AcmeAuthorization Authorization2 = Authorization;

						do
						{
							await Status.Invoke(this, "Waiting to poll authorization status.");
							await Task.Delay(5000);

							await Status.Invoke(this, "Polling authorization.");
							Authorization2 = await Authorization2.Poll();

							await Status.Invoke(this, "Authorization polled: " + Authorization2.Status.ToString());
						}
						while (Authorization2.Status == AcmeAuthorizationStatus.pending);

						if (Authorization2.Status != AcmeAuthorizationStatus.valid)
						{
							switch (Authorization2.Status)
							{
								case AcmeAuthorizationStatus.deactivated:
									throw new Exception("Authorization deactivated.");

								case AcmeAuthorizationStatus.expired:
									throw new Exception("Authorization expired.");

								case AcmeAuthorizationStatus.invalid:
									throw new Exception("Authorization invalid.");

								case AcmeAuthorizationStatus.revoked:
									throw new Exception("Authorization revoked.");

								default:
									throw new Exception("Authorization not validated.");
							}
						}
					}

					using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))   // TODO: Make configurable
					{
						await Status.Invoke(this, "Finalizing order.");

						SignatureAlgorithm SignAlg = new RsaSha256(RSA);

						Order = await Order.FinalizeOrder(new CertificateRequest(SignAlg)
						{
							CommonName = this.domain,
							SubjectAlternativeNames = DomainNames,
							EMailAddress = this.contactEMail
						});

						await Status.Invoke(this, "Order finalized: " + Order.Status.ToString());

						if (Order.Status != AcmeOrderStatus.valid)
						{
							switch (Order.Status)
							{
								case AcmeOrderStatus.invalid:
									throw new Exception("Order invalid.");

								default:
									throw new Exception("Unable to validate oder.");
							}
						}

						if (Order.Certificate is null)
							throw new Exception("No certificate URI provided.");

						await Status.Invoke(this, "Downloading certificate.");

						X509Certificate2[] Certificates = await Order.DownloadCertificate();
						X509Certificate2 Certificate = Certificates[0];

						await Status.Invoke(this, "Exporting certificate.");

						this.certificate = Certificate.Export(X509ContentType.Cert);
						this.privateKey = RSA.ExportCspBlob(true);
						this.pfx = null;
						this.password = string.Empty;

						await Status.Invoke(this, "Adding private key.");

						try
						{
							Certificate.PrivateKey = RSA;
						}
						catch (PlatformNotSupportedException)
						{
							await Status.Invoke(this, "Platform does not support adding of private key.");
							await Status.Invoke(this, "Searching for OpenSSL on machine.");

							string[] Files;
							string Password = Hashes.BinaryToString(Gateway.NextBytes(32));
							string CertFileName = null;
							string CertFileName2 = null;
							string KeyFileName = null;

							if (string.IsNullOrEmpty(this.openSslPath) || !File.Exists(this.openSslPath))
							{
								string[] Folders = Gateway.GetFolders(new Environment.SpecialFolder[]
									{
										Environment.SpecialFolder.ProgramFiles,
										Environment.SpecialFolder.ProgramFilesX86
									},
									Path.DirectorySeparatorChar + "OpenSSL-Win32",
									Path.DirectorySeparatorChar + "OpenSSL-Win64");

								Files = Gateway.FindFiles(Folders, "openssl.exe", 2, int.MaxValue);
							}
							else
								Files = new string[] { this.openSslPath };

							try
							{
								if (Files.Length == 0)
									return "Unable to join certificate with private key. Try installing <a target=\"_blank\" href=\"https://wiki.openssl.org/index.php/Binaries\">OpenSSL</a> and try again.";
								else
								{
									foreach (string OpenSslFile in Files)
									{
										if (CertFileName is null)
										{
											await Status.Invoke(this, "Generating temporary certificate file.");

											StringBuilder PemOutput = new StringBuilder();
											byte[] Bin = Certificate.Export(X509ContentType.Cert);

											PemOutput.AppendLine("-----BEGIN CERTIFICATE-----");
											PemOutput.AppendLine(Convert.ToBase64String(Bin, Base64FormattingOptions.InsertLineBreaks));
											PemOutput.AppendLine("-----END CERTIFICATE-----");

											CertFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.pem");
											await Resources.WriteAllTextAsync(CertFileName, PemOutput.ToString(), Encoding.ASCII);

											await Status.Invoke(this, "Generating temporary key file.");

											DerEncoder KeyOutput = new DerEncoder();
											SignAlg.ExportPrivateKey(KeyOutput);

											PemOutput.Clear();
											PemOutput.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
											PemOutput.AppendLine(Convert.ToBase64String(KeyOutput.ToArray(), Base64FormattingOptions.InsertLineBreaks));
											PemOutput.AppendLine("-----END RSA PRIVATE KEY-----");

											KeyFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.key");

											await Resources.WriteAllTextAsync(KeyFileName, PemOutput.ToString(), Encoding.ASCII);
										}

										await Status.Invoke(this, "Converting to PFX using " + OpenSslFile);

										Process P = new Process()
										{
											StartInfo = new ProcessStartInfo()
											{
												FileName = OpenSslFile,
												Arguments = "pkcs12 -nodes -export -out Certificate.pfx -inkey Certificate.key -in Certificate.pem -password pass:" + Password,
												UseShellExecute = false,
												RedirectStandardError = true,
												RedirectStandardOutput = true,
												WorkingDirectory = Gateway.AppDataFolder,
												CreateNoWindow = true,
												WindowStyle = ProcessWindowStyle.Hidden
											}
										};

										P.Start();

										if (!P.WaitForExit(60000) || P.ExitCode != 0)
										{
											if (!P.StandardOutput.EndOfStream)
												await Status.Invoke(this, "Output: " + P.StandardOutput.ReadToEnd());

											if (!P.StandardError.EndOfStream)
												await Status.Invoke(this, "Error: " + P.StandardError.ReadToEnd());

											continue;
										}

										await Status.Invoke(this, "Loading PFX.");

										CertFileName2 = Path.Combine(Gateway.AppDataFolder, "Certificate.pfx");
										this.pfx = await Resources.ReadAllBytesAsync(CertFileName2);
										this.password = Password;
										this.openSslPath = OpenSslFile;

										await Status.Invoke(this, "PFX successfully generated using OpenSSL.");
										break;
									}

									if (this.pfx is null)
									{
										this.openSslPath = string.Empty;
										return "Unable to convert to PFX using OpenSSL.";
									}
								}
							}
							finally
							{
								if (CertFileName != null && File.Exists(CertFileName))
								{
									await Status.Invoke(this, "Deleting temporary certificate file.");
									File.Delete(CertFileName);
								}

								if (KeyFileName != null && File.Exists(KeyFileName))
								{
									await Status.Invoke(this, "Deleting temporary key file.");
									File.Delete(KeyFileName);
								}

								if (CertFileName2 != null && File.Exists(CertFileName2))
								{
									await Status.Invoke(this, "Deleting temporary pfx file.");
									File.Delete(CertFileName2);
								}
							}
						}


						if (this.Step < 2)
							this.Step = 2;

						this.Updated = DateTime.Now;
						await Database.Update(this);

						Gateway.UpdateCertificate(this);

						return null;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return "Unable to create certificate: " + XML.HtmlValueEncode(ex.Message);
			}
			finally
			{
				this.inProgress = false;
			}
		}

		private Task AcmeChallenge(HttpRequest Request, HttpResponse Response)
		{
			if (Request.SubPath != this.challenge)
				throw new NotFoundException("ACME Challenge not found.");

			Response.StatusCode = 200;
			Response.ContentType = "application/octet-stream";
			return Response.Write(Encoding.ASCII.GetBytes(this.token));
		}

		/// <summary>
		/// Simplified configuration by configuring simple default values.
		/// </summary>
		/// <returns>If the configuration was changed.</returns>
		public override Task<bool> SimplifiedConfiguration()
		{
			return Task.FromResult<bool>(true);
		}

	}
}
