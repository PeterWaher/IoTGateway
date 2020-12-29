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
using Waher.Content.Xml;
using Waher.Events;
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
			get { return this.domain; }
			set { this.domain = value; }
		}

		/// <summary>
		/// Alternative domain names
		/// </summary>
		[DefaultValueNull]
		public string[] AlternativeDomains
		{
			get { return this.alternativeDomains; }
			set { this.alternativeDomains = value; }
		}

		/// <summary>
		/// If the server uses a domain name.
		/// </summary>
		[DefaultValue(false)]
		public bool UseDomainName
		{
			get { return this.useDomainName; }
			set { this.useDomainName = value; }
		}

		/// <summary>
		/// If the server uses a dynamic DNS service.
		/// </summary>
		[DefaultValue(false)]
		public bool DynamicDns
		{
			get { return this.dynamicDns; }
			set { this.dynamicDns = value; }
		}

		/// <summary>
		/// If the server uses server-side encryption.
		/// </summary>
		[DefaultValue(true)]
		public bool UseEncryption
		{
			get { return this.useEncryption; }
			set { this.useEncryption = value; }
		}

		/// <summary>
		/// If a custom Certificate Authority is to be used
		/// </summary>
		[DefaultValue(false)]
		public bool CustomCA
		{
			get { return this.customCA; }
			set { this.customCA = value; }
		}

		/// <summary>
		/// If a custom Certificate Authority is to be used, this property holds the URL to their ACME directory.
		/// </summary>
		[DefaultValueStringEmpty]
		public string AcmeDirectory
		{
			get { return this.acmeDirectory; }
			set { this.acmeDirectory = value; }
		}

		/// <summary>
		/// Contact e-mail address
		/// </summary>
		[DefaultValueStringEmpty]
		public string ContactEMail
		{
			get { return this.contactEMail; }
			set { this.contactEMail = value; }
		}

		/// <summary>
		/// CA Terms of Service
		/// </summary>
		[DefaultValueStringEmpty]
		public string UrlToS
		{
			get { return this.urlToS; }
			set { this.urlToS = value; }
		}

		/// <summary>
		/// If the CA Terms of Service has been accepted.
		/// </summary>
		[DefaultValue(false)]
		public bool AcceptToS
		{
			get { return this.acceptToS; }
			set { this.acceptToS = value; }
		}

		/// <summary>
		/// Certificate
		/// </summary>
		[DefaultValueNull]
		public byte[] Certificate
		{
			get { return this.certificate; }
			set { this.certificate = value; }
		}

		/// <summary>
		/// Private Key
		/// </summary>
		[DefaultValueNull]
		public byte[] PrivateKey
		{
			get { return this.privateKey; }
			set { this.privateKey = value; }
		}

		/// <summary>
		/// PFX container for certificate and private key, if available.
		/// </summary>
		[DefaultValueNull]
		public byte[] PFX
		{
			get { return this.pfx; }
			set { this.pfx = value; }
		}

		/// <summary>
		/// Password for PFX file, if any.
		/// </summary>
		[DefaultValueStringEmpty]
		public string Password
		{
			get { return this.password; }
			set { this.password = value; }
		}

		/// <summary>
		/// Path to OpenSSL
		/// </summary>
		[DefaultValueStringEmpty]
		public string OpenSslPath
		{
			get { return this.openSslPath; }
			set { this.openSslPath = value; }
		}

		/// <summary>
		/// Dynamic DNS Template
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsTemplate
		{
			get { return this.dynDnsTemplate; }
			set { this.dynDnsTemplate = value; }
		}

		/// <summary>
		/// Script to use to evaluate the current IP Address.
		/// </summary>
		[DefaultValueStringEmpty]
		public string CheckIpScript
		{
			get { return this.checkIpScript; }
			set { this.checkIpScript = value; }
		}

		/// <summary>
		/// Script to use to update the current IP Address.
		/// </summary>
		[DefaultValueStringEmpty]
		public string UpdateIpScript
		{
			get { return this.updateIpScript; }
			set { this.updateIpScript = value; }
		}

		/// <summary>
		/// Account Name for the Dynamic DNS service
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsAccount
		{
			get { return this.dynDnsAccount; }
			set { this.dynDnsAccount = value; }
		}

		/// <summary>
		/// Interval (in seconds) for checking if the IP address has changed.
		/// </summary>
		[DefaultValue(300)]
		public int DynDnsInterval
		{
			get { return this.dynDnsInterval; }
			set { this.dynDnsInterval = value; }
		}

		/// <summary>
		/// Password for the Dynamic DNS service
		/// </summary>
		[DefaultValueStringEmpty]
		public string DynDnsPassword
		{
			get { return this.dynDnsPassword; }
			set { this.dynDnsPassword = value; }
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

		private Task TestDomainNames(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
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

			return Task.CompletedTask;
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
			if (!this.dynamicDns)
				return true;

			Expression CheckIpScript;
			Expression UpdateIpScript;

			try
			{
				CheckIpScript = new Expression(this.checkIpScript);
			}
			catch (Exception ex)
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to parse script checking current IP Address: " + ex.Message, false, "User");
				return false;
			}

			try
			{
				UpdateIpScript = new Expression(this.updateIpScript);
			}
			catch (Exception ex)
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to parse script updating the dynamic DNS server: " + ex.Message, false, "User");
				return false;
			}

			await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Checking current IP Address.", false, "User");

			Variables Variables = new Variables();
			object Result;

			try
			{
				Result = CheckIpScript.Evaluate(Variables);
			}
			catch (Exception ex)
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to get current IP Address: " + ex.Message, false, "User");
				return false;
			}

			if (!(Result is string CurrentIP) || !IPAddress.TryParse(CurrentIP, out IPAddress _))
			{
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to get current IP Address. Unexpected response.", false, "User");
				return false;
			}

			await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Current IP Address: " + CurrentIP, false, "User");

			string LastIP = await RuntimeSettings.GetAsync("Last.IP." + DomainName, string.Empty);

			if (LastIP == CurrentIP)
				await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "IP Address has not changed for " + DomainName + ".", false, "User");
			else
			{
				try
				{
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Updating IP address for " + DomainName + " to " + CurrentIP + ".", false, "User");

					Variables["Account"] = this.dynDnsAccount;
					Variables["Password"] = this.dynDnsPassword;
					Variables["IP"] = CurrentIP;
					Variables["Domain"] = DomainName;

					Result = UpdateIpScript.Evaluate(Variables);

					await RuntimeSettings.SetAsync("Last.IP." + DomainName, CurrentIP);
				}
				catch (Exception ex)
				{
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to register new dynamic IP Address: " + ex.Message, false, "User");
					return false;
				}
			}

			return true;
		}

		private async Task<bool> Test(string TabID, string DomainName)
		{
			await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Testing " + DomainName + "...", false, "User");

			if (!await this.CheckDynamicIp(TabID, DomainName))
				return false;

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
						await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Domain name does not point to this machine.", false, "User");
						return false;
					}

					byte[] Bin = await Response.Content.ReadAsByteArrayAsync();
					string Token = Encoding.ASCII.GetString(Bin);

					if (Token != this.token)
					{
						await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unexpected response returned. Domain name does not point to this machine.", false, "User");
						return false;
					}
				}
				catch (TimeoutException)
				{
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Time-out. Check that the domain name points to this machine.", false, "User");
					return false;
				}
				catch (Exception ex)
				{
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to validate domain name: " + ex.Message, false, "User");
					return false;
				}
			}

			await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Domain name valid.", false, "User");

			return true;
		}

		private Task TestCA(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request);

			if (!Request.HasData)
				throw new BadRequestException();

			object Obj = Request.DecodeData();
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
				Task _ = this.CreateCertificate(TabID);
			}
		
			return Task.CompletedTask;
		}

		private bool inProgress = false;

		internal Task<bool> CreateCertificate()
		{
			return CreateCertificate(null);
		}

		internal async Task<bool> CreateCertificate(string TabID)
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
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Connecting to directory.", false, "User");

					AcmeDirectory AcmeDirectory = await Client.GetDirectory();

					if (AcmeDirectory.ExternalAccountRequired)
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "An external account is required.", false, "User");

					if (AcmeDirectory.TermsOfService != null)
					{
						URL = AcmeDirectory.TermsOfService.ToString();
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Terms of service available on: " + URL, false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "TermsOfService", URL, false, "User");

						this.urlToS = URL;

						if (!this.acceptToS)
						{
							await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "You need to accept the terms of service.", false, "User");
							return false;
						}
					}

					if (AcmeDirectory.Website != null)
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Web site available on: " + AcmeDirectory.Website.ToString(), false, "User");

					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Getting account.", false, "User");

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

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Account found.", false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Created: " + Account.CreatedAt.ToString(), false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Initial IP: " + Account.InitialIp, false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Status: " + Account.Status.ToString(), false, "User");

						if (string.IsNullOrEmpty(this.contactEMail))
						{
							if (Account.Contact != null && Account.Contact.Length != 0)
							{
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Updating contact URIs in account.", false, "User");
								Account = await Account.Update(new string[0]);
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Account updated.", false, "User");
							}
						}
						else
						{
							if (Account.Contact is null || Account.Contact.Length != 1 || Account.Contact[0] != "mailto:" + this.contactEMail)
							{
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Updating contact URIs in account.", false, "User");
								Account = await Account.Update(new string[] { "mailto:" + this.contactEMail });
								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Account updated.", false, "User");
							}
						}
					}
					catch (AcmeAccountDoesNotExistException)
					{
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Account not found.", false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Creating account.", false, "User");

						Account = await Client.CreateAccount(string.IsNullOrEmpty(this.contactEMail) ? new string[0] : new string[] { "mailto:" + this.contactEMail },
							this.acceptToS);

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Account created.", false, "User");
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Status: " + Account.Status.ToString(), false, "User");
					}

					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Generating new key.", false, "User");
					await Account.NewKey();

					using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
					{
						RSA.ImportParameters(Client.ExportAccountKey(true));
					}

					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "New key generated.", false, "User");

					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Creating order.", false, "User");
					AcmeOrder Order = await Account.OrderCertificate(DomainNames, null, null);
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Order created.", false, "User");

					foreach (AcmeAuthorization Authorization in await Order.GetAuthorizations())
					{
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Processing authorization for " + Authorization.Value, false, "User");

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

								await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Acknowleding challenge.", false, "User");
								if (await this.CheckDynamicIp(TabID, Authorization.Value))
								{
									Challenge = await HttpChallenge.AcknowledgeChallenge();
									await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Challenge acknowledged: " + Challenge.Status.ToString(), false, "User");

									Acknowledged = true;
								}
							}
						}

						if (!Acknowledged)
						{
							await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "No automated method found to respond to any of the authorization challenges.", false, "User");
							return false;
						}

						AcmeAuthorization Authorization2 = Authorization;

						do
						{
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Waiting to poll authorization status.", false, "User");
							await Task.Delay(5000);

							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Polling authorization.", false, "User");
							Authorization2 = await Authorization2.Poll();

							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Authorization polled: " + Authorization2.Status.ToString(), false, "User");
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
						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Finalizing order.", false, "User");

						SignatureAlgorithm SignAlg = new RsaSha256(RSA);

						Order = await Order.FinalizeOrder(new CertificateRequest(SignAlg)
						{
							CommonName = this.domain,
							SubjectAlternativeNames = DomainNames,
							EMailAddress = this.contactEMail
						});

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Order finalized: " + Order.Status.ToString(), false, "User");

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

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Downloading certificate.", false, "User");

						X509Certificate2[] Certificates = await Order.DownloadCertificate();
						X509Certificate2 Certificate = Certificates[0];

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Exporting certificate.", false, "User");

						this.certificate = Certificate.Export(X509ContentType.Cert);
						this.privateKey = RSA.ExportCspBlob(true);
						this.pfx = null;
						this.password = string.Empty;

						await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Adding private key.", false, "User");

						try
						{
							Certificate.PrivateKey = RSA;
						}
						catch (PlatformNotSupportedException)
						{
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Platform does not support adding of private key.", false, "User");
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Searching for OpenSSL on machine.", false, "User");

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
								{
									await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to join certificate with private key. Try installing <a target=\"_blank\" href=\"https://wiki.openssl.org/index.php/Binaries\">OpenSSL</a> and try again.", false, "User");
									return false;
								}
								else
								{
									foreach (string OpenSslFile in Files)
									{
										if (CertFileName is null)
										{
											await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Generating temporary certificate file.", false, "User");

											StringBuilder PemOutput = new StringBuilder();
											byte[] Bin = Certificate.Export(X509ContentType.Cert);

											PemOutput.AppendLine("-----BEGIN CERTIFICATE-----");
											PemOutput.AppendLine(Convert.ToBase64String(Bin, Base64FormattingOptions.InsertLineBreaks));
											PemOutput.AppendLine("-----END CERTIFICATE-----");

											CertFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.pem");
											File.WriteAllText(CertFileName, PemOutput.ToString(), Encoding.ASCII);

											await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Generating temporary key file.", false, "User");

											DerEncoder KeyOutput = new DerEncoder();
											SignAlg.ExportPrivateKey(KeyOutput);

											PemOutput.Clear();
											PemOutput.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
											PemOutput.AppendLine(Convert.ToBase64String(KeyOutput.ToArray(), Base64FormattingOptions.InsertLineBreaks));
											PemOutput.AppendLine("-----END RSA PRIVATE KEY-----");

											KeyFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.key");

											File.WriteAllText(KeyFileName, PemOutput.ToString(), Encoding.ASCII);
										}

										await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Converting to PFX using " + OpenSslFile, false, "User");

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
												await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Output: " + P.StandardOutput.ReadToEnd(), false, "User");

											if (!P.StandardError.EndOfStream)
												await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Error: " + P.StandardError.ReadToEnd(), false, "User");

											continue;
										}

										await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Loading PFX.", false, "User");

										CertFileName2 = Path.Combine(Gateway.AppDataFolder, "Certificate.pfx");
										this.pfx = File.ReadAllBytes(CertFileName2);
										this.password = Password;
										this.openSslPath = OpenSslFile;

										await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "PFX successfully generated using OpenSSL.", false, "User");
										break;
									}

									if (this.pfx is null)
									{
										this.openSslPath = string.Empty;
										await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to convert to PFX using OpenSSL.", false, "User");
										return false;
									}
								}
							}
							finally
							{
								if (CertFileName != null && File.Exists(CertFileName))
								{
									await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Deleting temporary certificate file.", false, "User");
									File.Delete(CertFileName);
								}

								if (KeyFileName != null && File.Exists(KeyFileName))
								{
									await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Deleting temporary key file.", false, "User");
									File.Delete(KeyFileName);
								}

								if (CertFileName2 != null && File.Exists(CertFileName2))
								{
									await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", "Deleting temporary pfx file.", false, "User");
									File.Delete(CertFileName2);
								}
							}
						}


						if (this.Step < 2)
							this.Step = 2;

						this.Updated = DateTime.Now;
						await Database.Update(this);

						await ClientEvents.PushEvent(new string[] { TabID }, "CertificateOk", string.Empty, false, "User");

						Gateway.UpdateCertificate(this);

						return true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", "Unable to create certificate: " + XML.HtmlValueEncode(ex.Message), false, "User");
				return false;
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
