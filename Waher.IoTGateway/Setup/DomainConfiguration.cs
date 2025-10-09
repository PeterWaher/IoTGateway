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
using Waher.Content.Binary;
using Waher.Content.Text;
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
		private HttpResource saveNames = null;
		private HttpResource saveDescriptions = null;

		private AlternativeField[] localizedNames = null;
		private AlternativeField[] localizedDescriptions = null;
		private string[] alternativeDomains = null;
		private byte[] certificate = null;
		private byte[] privateKey = null;
		private byte[] pfx = null;
		private string humanReadableName = string.Empty;
		private string humanReadableNameLanguage = string.Empty;
		private string humanReadableDescription = string.Empty;
		private string humanReadableDescriptionLanguage = string.Empty;
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
			set
			{
				this.useDomainName = value;
				if (!this.useDomainName)
				{
					this.domain = string.Empty;
					this.alternativeDomains = null;
					this.dynamicDns = false;
					this.useEncryption = false;
					this.customCA = false;
				}
			}
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
		[Encrypted(32)]
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
		/// Human-readable name of domain
		/// </summary>
		[DefaultValueStringEmpty]
		public string HumanReadableName
		{
			get => this.humanReadableName;
			set => this.humanReadableName = value;
		}

		/// <summary>
		/// Language of <see cref="HumanReadableName"/>.
		/// </summary>
		[DefaultValueStringEmpty]
		public string HumanReadableNameLanguage
		{
			get => this.humanReadableNameLanguage;
			set => this.humanReadableNameLanguage = value;
		}

		/// <summary>
		/// Human-readable description of domain
		/// </summary>
		[DefaultValueStringEmpty]
		public string HumanReadableDescription
		{
			get => this.humanReadableDescription;
			set => this.humanReadableDescription = value;
		}

		/// <summary>
		/// Language of <see cref="HumanReadableDescription"/>.
		/// </summary>
		[DefaultValueStringEmpty]
		public string HumanReadableDescriptionLanguage
		{
			get => this.humanReadableDescriptionLanguage;
			set => this.humanReadableDescriptionLanguage = value;
		}

		/// <summary>
		/// Localized names of domain
		/// </summary>
		[DefaultValueNull]
		public AlternativeField[] LocalizedNames
		{
			get => this.localizedNames;
			set => this.localizedNames = value;
		}

		/// <summary>
		/// Localized descriptions of domain
		/// </summary>
		[DefaultValueNull]
		public AlternativeField[] LocalizedDescriptions
		{
			get => this.localizedDescriptions;
			set => this.localizedDescriptions = value;
		}

		/// <summary>
		/// If the CA has a Terms of Service.
		/// </summary>
		public bool HasToS => !string.IsNullOrEmpty(this.urlToS);

		/// <summary>
		/// If the configuration has a certificate.
		/// </summary>
		public bool HasCertificate =>
			(!(this.certificate is null) && !(this.privateKey is null)) ||
			(!(this.pfx is null) && !(this.password is null));

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
			this.saveNames = WebServer.Register("/Settings/SaveNames", null, this.SaveNames, true, false, true);
			this.saveDescriptions = WebServer.Register("/Settings/SaveDescriptions", null, this.SaveDescriptions, true, false, true);

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
			WebServer.Unregister(this.saveNames);
			WebServer.Unregister(this.saveDescriptions);

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

			if (!Parameters.TryGetValue("domainName", out object Obj) || !(Obj is string DomainName) ||
				!Parameters.TryGetValue("dynamicDns", out Obj) || !(Obj is bool DynamicDns) ||
				!Parameters.TryGetValue("dynDnsTemplate", out Obj) || !(Obj is string DynDnsTemplate) ||
				!Parameters.TryGetValue("checkIpScript", out Obj) || !(Obj is string CheckIpScript) ||
				!Parameters.TryGetValue("updateIpScript", out Obj) || !(Obj is string UpdateIpScript) ||
				!Parameters.TryGetValue("dynDnsAccount", out Obj) || !(Obj is string DynDnsAccount) ||
				!Parameters.TryGetValue("dynDnsPassword", out Obj) || !(Obj is string DynDnsPassword) ||
				!Parameters.TryGetValue("dynDnsInterval", out Obj) || !(Obj is int DynDnsInterval))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (string.Compare(DomainName, "localhost", true) == 0)
			{
				await Response.SendResponse(new BadRequestException("localhost is not a valid domain name."));
				return;
			}

			List<string> AlternativeNames = new List<string>();
			int Index = 0;

			while (Parameters.TryGetValue("altDomainName" + Index.ToString(), out Obj) && Obj is string AltDomainName && !string.IsNullOrEmpty(AltDomainName))
			{
				if (string.Compare(AltDomainName, "localhost", true) == 0)
				{
					await Response.SendResponse(new BadRequestException("localhost is not a valid domain name."));
					return;
				}

				AlternativeNames.Add(AltDomainName);
				Index++;
			}

			if (Parameters.TryGetValue("altDomainName", out Obj) && Obj is string AltDomainName2 && !string.IsNullOrEmpty(AltDomainName2))
			{
				if (string.Compare(AltDomainName2, "localhost", true) == 0)
				{
					await Response.SendResponse(new BadRequestException("localhost is not a valid domain name."));
					return;
				}

				AlternativeNames.Add(AltDomainName2);
			}

			string TabID = Request.Header["X-TabID"];
			if (string.IsNullOrEmpty(TabID))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

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

			Task _ = Task.Run(async () => await this.Test(TabID, false));
		}

		private Task TestDomainName(HttpRequest Request, HttpResponse Response)
		{
			Response.StatusCode = 200;
			Response.ContentType = PlainTextCodec.DefaultContentType;
			return Response.Write(this.token);
		}

		private async Task<bool> Test(string TabID, bool EnvironmentSetup)
		{
			try
			{
				if (!string.IsNullOrEmpty(this.domain))
				{
					if (!await this.Test(TabID, EnvironmentSetup, this.domain))
					{
						if (string.IsNullOrEmpty(TabID))
							this.LogEnvironmentError("Domain name not valid.", GATEWAY_DOMAIN_NAME, this.domain);
						else
							await ClientEvents.PushEvent(new string[] { TabID }, "NameNotValid", this.domain, false, "User");
						return false;
					}
				}

				if (!(this.alternativeDomains is null))
				{
					foreach (string AltDomainName in this.alternativeDomains)
					{
						if (!await this.Test(TabID, EnvironmentSetup, AltDomainName))
						{
							if (string.IsNullOrEmpty(TabID))
								this.LogEnvironmentError("Alternative domain name not valid.", GATEWAY_DOMAIN_ALT, AltDomainName);
							else
								await ClientEvents.PushEvent(new string[] { TabID }, "NameNotValid", AltDomainName, false, "User");
							return false;
						}
					}
				}

				if (this.Step < 1)
					this.Step = 1;

				this.Updated = DateTime.Now;
				await Database.Update(this);

				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "NamesOK", string.Empty, false, "User");

				return true;
			}
			catch (Exception ex)
			{
				if (string.IsNullOrEmpty(TabID))
					Log.Exception(ex);
				else
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", ex.Message, false, "User");

				return false;
			}
		}

		internal Task<bool> CheckDynamicIp()
		{
			return this.CheckDynamicIp(null, false);
		}

		internal Task<bool> CheckDynamicIp(bool EnvironmentSetup)
		{
			return this.CheckDynamicIp(null, EnvironmentSetup);
		}

		private async Task<bool> CheckDynamicIp(string TabID, bool EnvironmentSetup)
		{
			try
			{
				if (!this.useDomainName || !this.dynamicDns)
					return true;

				bool Result = true;

				if (!await this.CheckDynamicIp(TabID, this.domain, EnvironmentSetup))
					Result = false;

				foreach (string AlternativeDomain in this.alternativeDomains)
				{
					if (!await this.CheckDynamicIp(TabID, AlternativeDomain, EnvironmentSetup))
						Result = false;
				}

				return Result;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				return false;
			}
		}

		internal async Task<bool> CheckDynamicIp(string TabID, string DomainName, bool EnvironmentSetup)
		{
			string Msg = await this.CheckDynamicIp(DomainName, EnvironmentSetup, async (_, Status) =>
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
			});

			if (!string.IsNullOrEmpty(Msg))
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", Msg, false, "User");

				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if Dynamic IP configuration is correct
		/// </summary>
		/// <param name="DomainName">Domain Name</param>
		/// <param name="EnvironmentSetup">If check is done as part of environment variable configuration.</param>
		/// <param name="Status">Status callback method.</param>
		/// <returns>null if successful, error message if failed.</returns>
		public async Task<string> CheckDynamicIp(string DomainName, bool EnvironmentSetup, EventHandlerAsync<string> Status)
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
				string Msg = "Unable to parse script checking current IP Address: " + ex.Message;

				if (EnvironmentSetup)
					this.LogEnvironmentError(Msg, GATEWAY_DYNDNS_CHECK, this.checkIpScript);

				return Msg;
			}

			try
			{
				UpdateIpScript = new Expression(this.updateIpScript);
			}
			catch (Exception ex)
			{
				string Msg = "Unable to parse script updating the dynamic DNS server: " + ex.Message;

				if (EnvironmentSetup)
					this.LogEnvironmentError(Msg, GATEWAY_DYNDNS_UPDATE, this.updateIpScript);

				return Msg;
			}

			await Status.Raise(this, "Checking current IP Address.");

			Variables Variables = HttpServer.CreateSessionVariables();
			object Result;

			try
			{
				Result = await CheckIpScript.EvaluateAsync(Variables);
			}
			catch (Exception ex)
			{
				string Msg = "Unable to get current IP Address: " + ex.Message;

				if (EnvironmentSetup)
					this.LogEnvironmentError(Msg, GATEWAY_DYNDNS_CHECK, this.checkIpScript);

				return Msg;
			}

			if (!(Result is string CurrentIP) || !IPAddress.TryParse(CurrentIP, out IPAddress _))
			{
				string Msg = "Unable to get current IP Address. Unexpected response.";

				if (EnvironmentSetup)
					this.LogEnvironmentError(Msg, GATEWAY_DYNDNS_CHECK, this.checkIpScript);

				return Msg;
			}

			await Status.Raise(this, "Current IP Address: " + CurrentIP);

			string LastIP = await RuntimeSettings.GetAsync("Last.IP." + DomainName, string.Empty);

			if (LastIP == CurrentIP)
				await Status.Raise(this, "IP Address has not changed for " + DomainName + ".");
			else
			{
				try
				{
					await Status.Raise(this, "Updating IP address for " + DomainName + " to " + CurrentIP + ".");

					Variables["Account"] = this.dynDnsAccount;
					Variables["Password"] = this.dynDnsPassword;
					Variables["IP"] = CurrentIP;
					Variables["Domain"] = DomainName;

					Result = await UpdateIpScript.EvaluateAsync(Variables);

					await RuntimeSettings.SetAsync("Last.IP." + DomainName, CurrentIP);
				}
				catch (Exception ex)
				{
					string Msg = "Unable to register new dynamic IP Address: " + ex.Message;

					if (EnvironmentSetup)
						this.LogEnvironmentError(Msg, GATEWAY_DYNDNS_UPDATE, this.updateIpScript);

					return Msg;
				}
			}

			return null;
		}

		private async Task<bool> Test(string TabID, bool EnvironmentSetup, string DomainName)
		{
			string Msg = await this.Test(DomainName, EnvironmentSetup, async (_, Status) =>
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
			});

			if (!string.IsNullOrEmpty(Msg))
			{
				if (!string.IsNullOrEmpty(TabID))
					await ClientEvents.PushEvent(new string[] { TabID }, "CertificateError", Msg, false, "User");

				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks if the domain points to the server.
		/// </summary>
		/// <param name="DomainName">Domain Name</param>
		/// <param name="EnvironmentSetup">If test is part of an environment setup.</param>
		/// <param name="Status">Status callback method.</param>
		/// <returns>null if successful, error message if failed.</returns>
		public async Task<string> Test(string DomainName, bool EnvironmentSetup, EventHandlerAsync<string> Status)
		{
			await Status.Raise(this, "Testing " + DomainName + "...");

			string Msg = await this.CheckDynamicIp(DomainName, EnvironmentSetup, Status);
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

					if (Array.IndexOf(HttpPorts, 80) < 0 && HttpPorts.Length > 0)
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
					string Token = System.Text.Encoding.ASCII.GetString(Bin);

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

			await Status.Raise(this, "Domain name valid.");

			return null;
		}

		private async Task TestCA(HttpRequest Request, HttpResponse Response)
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

			if (!Parameters.TryGetValue("useEncryption", out object Obj) || !(Obj is bool UseEncryption))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("customCA", out Obj) || !(Obj is bool CustomCA))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("acmeDirectory", out Obj) || !(Obj is string AcmeDirectory))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("contactEMail", out Obj) || !(Obj is string ContactEMail))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("acceptToS", out Obj) || !(Obj is bool AcceptToS))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("domainName", out Obj) || !(Obj is string DomainName) ||
				!Parameters.TryGetValue("dynamicDns", out Obj) || !(Obj is bool DynamicDns) ||
				!Parameters.TryGetValue("dynDnsTemplate", out Obj) || !(Obj is string DynDnsTemplate) ||
				!Parameters.TryGetValue("checkIpScript", out Obj) || !(Obj is string CheckIpScript) ||
				!Parameters.TryGetValue("updateIpScript", out Obj) || !(Obj is string UpdateIpScript) ||
				!Parameters.TryGetValue("dynDnsAccount", out Obj) || !(Obj is string DynDnsAccount) ||
				!Parameters.TryGetValue("dynDnsPassword", out Obj) || !(Obj is string DynDnsPassword) ||
				!Parameters.TryGetValue("dynDnsInterval", out Obj) || !(Obj is int DynDnsInterval))
			{
				await Response.SendResponse(new BadRequestException());
				return;

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
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

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
						await this.CreateCertificate(TabID, false);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
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

			string Msg = await this.Test(Name, false, Status);
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

			Msg = await this.CreateCertificate(false, Status, (_, __) => Task.CompletedTask);
			if (!string.IsNullOrEmpty(Msg))
			{
				this.domain = DomainBak;
				this.useDomainName = UseDomainBak;
				this.alternativeDomains = AlternativeBak;

				return Msg;
			}

			this.Updated = DateTime.Now;
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
				string Msg = await this.CreateCertificate(false, Status, (_, __) => Task.CompletedTask);
				if (!string.IsNullOrEmpty(Msg))
				{
					this.domain = DomainBak;
					this.useDomainName = UseDomainBak;
					this.alternativeDomains = AlternativeBak;

					return Msg;
				}
			}

			this.Updated = DateTime.Now;
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
			return this.CreateCertificate(null, false);
		}

		private async Task<bool> CreateCertificate(string TabID, bool EnvironmentSetup)
		{
			try
			{
				string Msg = await this.CreateCertificate(EnvironmentSetup,
					async (_, Status) =>
					{
						if (!string.IsNullOrEmpty(TabID))
							await ClientEvents.PushEvent(new string[] { TabID }, "ShowStatus", Status, false, "User");
					},
					async (_, URL) =>
					{
						if (!string.IsNullOrEmpty(TabID))
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

				if (EnvironmentSetup)
					this.LogEnvironmentError(ex.Message, GATEWAY_ENCRYPTION, this.useEncryption);

				return false;
			}
		}

		internal async Task<string> CreateCertificate(bool EnvironmentSetup, EventHandlerAsync<string> Status,
			EventHandlerAsync<string> TermsOfService)
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
						using RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams);
						Parameters = RSA.ExportParameters(true);
					}
				}
				catch (CryptographicException ex)
				{
					throw new CryptographicException("Unable to get access to cryptographic key for \"IoTGateway:" + URL +
						"\". Was the database created using another user?", ex);
				}

				using AcmeClient Client = new AcmeClient(new Uri(URL), Parameters);
				await Status.Raise(this, "Connecting to directory.");

				AcmeDirectory AcmeDirectory = await Client.GetDirectory();

				if (AcmeDirectory.ExternalAccountRequired)
					await Status.Raise(this, "An external account is required.");

				if (!(AcmeDirectory.TermsOfService is null))
				{
					URL = AcmeDirectory.TermsOfService.ToString();
					await Status.Raise(this, "Terms of service available on: " + URL);
					await TermsOfService.Raise(this, URL);

					this.urlToS = URL;

					if (!this.acceptToS)
					{
						string Msg = "You need to accept the terms of service.";

						if (EnvironmentSetup)
							this.LogEnvironmentError(Msg, GATEWAY_ACME_ACCEPT_TOS, this.acceptToS);

						return Msg;
					}
				}

				if (!(AcmeDirectory.Website is null))
					await Status.Raise(this, "Web site available on: " + AcmeDirectory.Website.ToString());

				await Status.Raise(this, "Getting account.");

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

					await Status.Raise(this, "Account found.");
					await Status.Raise(this, "Created: " + Account.CreatedAt.ToString());
					await Status.Raise(this, "Initial IP: " + Account.InitialIp);
					await Status.Raise(this, "Status: " + Account.Status.ToString());

					if (string.IsNullOrEmpty(this.contactEMail))
					{
						if (!(Account.Contact is null) && Account.Contact.Length != 0)
						{
							await Status.Raise(this, "Updating contact URIs in account.");
							Account = await Account.Update(Array.Empty<string>());
							await Status.Raise(this, "Account updated.");
						}
					}
					else
					{
						if (Account.Contact is null || Account.Contact.Length != 1 || Account.Contact[0] != "mailto:" + this.contactEMail)
						{
							await Status.Raise(this, "Updating contact URIs in account.");
							Account = await Account.Update(new string[] { "mailto:" + this.contactEMail });
							await Status.Raise(this, "Account updated.");
						}
					}
				}
				catch (AcmeAccountDoesNotExistException)
				{
					await Status.Raise(this, "Account not found.");
					await Status.Raise(this, "Creating account.");

					Account = await Client.CreateAccount(string.IsNullOrEmpty(this.contactEMail) ? Array.Empty<string>() : new string[] { "mailto:" + this.contactEMail },
						this.acceptToS);

					await Status.Raise(this, "Account created.");
					await Status.Raise(this, "Status: " + Account.Status.ToString());
				}

				await Status.Raise(this, "Generating new key.");
				await Account.NewKey();

				using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096, CspParams))
				{
					RSA.ImportParameters(Client.ExportAccountKey(true));
				}

				await Status.Raise(this, "New key generated.");

				await Status.Raise(this, "Creating order.");
				AcmeOrder Order;

				try
				{
					Order = await Account.OrderCertificate(DomainNames, null, null);
				}
				catch (AcmeMalformedException)  // Not sure why this is necessary. Perhaps because it takes time to propagate the keys correctly on the remote end?
				{
					await Task.Delay(5000);
					await Status.Raise(this, "Retrying.");
					Order = await Account.OrderCertificate(DomainNames, null, null);
				}

				await Status.Raise(this, "Order created.");

				AcmeAuthorization[] Authorizations;

				await Status.Raise(this, "Getting authorizations.");
				try
				{
					Authorizations = await Order.GetAuthorizations();
				}
				catch (AcmeMalformedException)  // Not sure why this is necessary. Perhaps because it takes time to propagate the keys correctly on the remote end?
				{
					await Task.Delay(5000);
					await Status.Raise(this, "Retrying.");
					Authorizations = await Order.GetAuthorizations();
				}

				foreach (AcmeAuthorization Authorization in Authorizations)
				{
					await Status.Raise(this, "Processing authorization for " + Authorization.Value);

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

							await Status.Raise(this, "Acknowleding challenge.");

							string Msg = await this.CheckDynamicIp(Authorization.Value, EnvironmentSetup, Status);
							if (string.IsNullOrEmpty(Msg))
							{
								Challenge = await HttpChallenge.AcknowledgeChallenge();
								await Status.Raise(this, "Challenge acknowledged: " + Challenge.Status.ToString());

								Acknowledged = true;
							}
							else
								return Msg;
						}
					}

					if (!Acknowledged)
					{
						string Msg = "No automated method found to respond to any of the authorization challenges.";

						if (EnvironmentSetup)
							this.LogEnvironmentError(Msg, GATEWAY_ENCRYPTION, this.useEncryption);

						return Msg;
					}

					AcmeAuthorization Authorization2 = Authorization;

					do
					{
						await Status.Raise(this, "Waiting to poll authorization status.");
						await Task.Delay(5000);

						await Status.Raise(this, "Polling authorization.");
						Authorization2 = await Authorization2.Poll();

						await Status.Raise(this, "Authorization polled: " + Authorization2.Status.ToString());
					}
					while (Authorization2.Status == AcmeAuthorizationStatus.pending);

					if (Authorization2.Status != AcmeAuthorizationStatus.valid)
					{
						throw Authorization2.Status switch
						{
							AcmeAuthorizationStatus.deactivated => new Exception("Authorization deactivated."),
							AcmeAuthorizationStatus.expired => new Exception("Authorization expired."),
							AcmeAuthorizationStatus.invalid => new Exception("Authorization invalid."),
							AcmeAuthorizationStatus.revoked => new Exception("Authorization revoked."),
							_ => new Exception("Authorization not validated."),
						};
					}
				}

				using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(4096))   // TODO: Make configurable
				{
					await Status.Raise(this, "Finalizing order.");

					SignatureAlgorithm SignAlg = new RsaSha256(RSA);

					Order = await Order.FinalizeOrder(new Security.PKCS.CertificateRequest(SignAlg)
					{
						CommonName = this.domain,
						SubjectAlternativeNames = DomainNames,
						EMailAddress = this.contactEMail
					});

					await Status.Raise(this, "Order finalized: " + Order.Status.ToString());

					if (Order.Status != AcmeOrderStatus.valid)
					{
						throw Order.Status switch
						{
							AcmeOrderStatus.invalid => new Exception("Order invalid."),
							_ => new Exception("Unable to validate order."),
						};
					}

					if (Order.Certificate is null)
						throw new Exception("No certificate URI provided.");

					await Status.Raise(this, "Downloading certificate.");

					X509Certificate2[] Certificates = await Order.DownloadCertificate();
					X509Certificate2 Certificate = Certificates[0];

					await Status.Raise(this, "Exporting certificate.");

					this.certificate = Certificate.Export(X509ContentType.Cert);
					this.privateKey = RSA.ExportCspBlob(true);
					this.pfx = null;
					this.password = string.Empty;

					await Status.Raise(this, "Adding private key.");

					try
					{
						Certificate.PrivateKey = RSA;
					}
					catch (PlatformNotSupportedException)
					{
						await Status.Raise(this, "Platform does not support adding of private key.");
						await Status.Raise(this, "Searching for OpenSSL on machine.");

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
								string Msg = "Unable to join certificate with private key. Try installing <a target=\"_blank\" href=\"https://wiki.openssl.org/index.php/Binaries\">OpenSSL</a> and try again.";

								if (EnvironmentSetup)
									this.LogEnvironmentError(Msg, GATEWAY_ENCRYPTION, this.useEncryption);

								return Msg;
							}
							else
							{
								foreach (string OpenSslFile in Files)
								{
									if (CertFileName is null)
									{
										await Status.Raise(this, "Generating temporary certificate file.");

										StringBuilder PemOutput = new StringBuilder();
										byte[] Bin = Certificate.Export(X509ContentType.Cert);

										PemOutput.AppendLine("-----BEGIN CERTIFICATE-----");
										PemOutput.AppendLine(Convert.ToBase64String(Bin, Base64FormattingOptions.InsertLineBreaks));
										PemOutput.AppendLine("-----END CERTIFICATE-----");

										CertFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.pem");
										await Runtime.IO.Files.WriteAllTextAsync(CertFileName, PemOutput.ToString(), System.Text.Encoding.ASCII);

										await Status.Raise(this, "Generating temporary key file.");

										DerEncoder KeyOutput = new DerEncoder();
										SignAlg.ExportPrivateKey(KeyOutput);

										PemOutput.Clear();
										PemOutput.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
										PemOutput.AppendLine(Convert.ToBase64String(KeyOutput.ToArray(), Base64FormattingOptions.InsertLineBreaks));
										PemOutput.AppendLine("-----END RSA PRIVATE KEY-----");

										KeyFileName = Path.Combine(Gateway.AppDataFolder, "Certificate.key");

										await Runtime.IO.Files.WriteAllTextAsync(KeyFileName, PemOutput.ToString(), System.Text.Encoding.ASCII);
									}

									await Status.Raise(this, "Converting to PFX using " + OpenSslFile);

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
											await Status.Raise(this, "Output: " + P.StandardOutput.ReadToEnd());

										if (!P.StandardError.EndOfStream)
											await Status.Raise(this, "Error: " + P.StandardError.ReadToEnd());

										continue;
									}

									await Status.Raise(this, "Loading PFX.");

									CertFileName2 = Path.Combine(Gateway.AppDataFolder, "Certificate.pfx");
									this.pfx = await Runtime.IO.Files.ReadAllBytesAsync(CertFileName2);
									this.password = Password;
									this.openSslPath = OpenSslFile;

									await Status.Raise(this, "PFX successfully generated using OpenSSL.");
									break;
								}

								if (this.pfx is null)
								{
									this.openSslPath = string.Empty;

									string Msg = "Unable to convert to PFX using OpenSSL.";

									if (EnvironmentSetup)
										this.LogEnvironmentError(Msg, GATEWAY_ENCRYPTION, this.useEncryption);

									return Msg;
								}
							}
						}
						finally
						{
							if (!(CertFileName is null) && File.Exists(CertFileName))
							{
								await Status.Raise(this, "Deleting temporary certificate file.");
								File.Delete(CertFileName);
							}

							if (!(KeyFileName is null) && File.Exists(KeyFileName))
							{
								await Status.Raise(this, "Deleting temporary key file.");
								File.Delete(KeyFileName);
							}

							if (!(CertFileName2 is null) && File.Exists(CertFileName2))
							{
								await Status.Raise(this, "Deleting temporary pfx file.");
								File.Delete(CertFileName2);
							}
						}
					}


					if (this.Step < 2)
						this.Step = 2;

					this.Updated = DateTime.Now;
					await Database.Update(this);

					await Gateway.UpdateCertificate(this);

					return null;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				string Msg = "Unable to create certificate: " + XML.HtmlValueEncode(ex.Message);

				if (EnvironmentSetup)
					this.LogEnvironmentError(Msg, GATEWAY_ENCRYPTION, this.useEncryption);

				return Msg;
			}
			finally
			{
				this.inProgress = false;
			}
		}

		private async Task AcmeChallenge(HttpRequest Request, HttpResponse Response)
		{
			if (Request.SubPath != this.challenge)
			{
				await Response.SendResponse(new NotFoundException("ACME Challenge not found."));
				return;
			}

			Response.StatusCode = 200;
			Response.ContentType = BinaryCodec.DefaultContentType;
			
			await Response.Write(true, System.Text.Encoding.ASCII.GetBytes(this.token));
		}

		private async Task SaveNames(HttpRequest Request, HttpResponse Response)
		{
			Gateway.AssertUserAuthenticated(Request, this.ConfigPrivilege);

			if (!Request.HasData)
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			ContentResponse Content = await Request.DecodeDataAsync();
			if (!(Content.Decoded is Dictionary<string, object> Parameters))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			if (!Parameters.TryGetValue("humanReadableName", out object Obj) ||
				!(Obj is string HumanReadableName) ||
				!Parameters.TryGetValue("humanReadableNameLanguage", out Obj) ||
				!(Obj is string HumanReadableNameLanguage))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			List<AlternativeField> LocalizedNames = new List<AlternativeField>();
			int Index = 1;

			while (Parameters.TryGetValue("nameLanguage" + Index.ToString(), out Obj) &&
				Obj is string NameLanguage &&
				Parameters.TryGetValue("nameLocalized" + Index.ToString(), out Obj) &&
				Obj is string NameLocalized)
			{
				if (string.IsNullOrEmpty(NameLanguage))
				{
					await Response.SendResponse(new BadRequestException("Language cannot be empty."));
					return;
				}

				if (string.IsNullOrEmpty(NameLocalized))
				{
					await Response.SendResponse(new BadRequestException("Localized name cannot be empty."));
					return;
				}

				LocalizedNames.Add(new AlternativeField(NameLanguage, NameLocalized));
				Index++;
			}

			this.HumanReadableName = HumanReadableName;
			this.HumanReadableNameLanguage = HumanReadableNameLanguage;
			this.LocalizedNames = LocalizedNames.ToArray();

			this.Updated = DateTime.Now;
			await Database.Update(this);
		}

		private async Task SaveDescriptions(HttpRequest Request, HttpResponse Response)
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

			if (!Parameters.TryGetValue("humanReadableDescription", out object Obj) ||
				!(Obj is string HumanReadableDescription) ||
				!Parameters.TryGetValue("humanReadableDescriptionLanguage", out Obj) ||
				!(Obj is string HumanReadableDescriptionLanguage))
			{
				await Response.SendResponse(new BadRequestException());
				return;
			}

			List<AlternativeField> LocalizedDescriptions = new List<AlternativeField>();
			int Index = 1;

			while (Parameters.TryGetValue("descriptionLanguage" + Index.ToString(), out Obj) &&
				Obj is string DescriptionLanguage &&
				Parameters.TryGetValue("descriptionLocalized" + Index.ToString(), out Obj) &&
				Obj is string DescriptionLocalized)
			{
				if (string.IsNullOrEmpty(DescriptionLanguage))
				{
					await Response.SendResponse(new BadRequestException("Language cannot be empty."));
					return;
				}

				if (string.IsNullOrEmpty(DescriptionLocalized))
				{
					await Response.SendResponse(new BadRequestException("Localized description cannot be empty."));
					return;
				}

				LocalizedDescriptions.Add(new AlternativeField(DescriptionLanguage, DescriptionLocalized));
				Index++;
			}

			this.HumanReadableDescription = HumanReadableDescription;
			this.HumanReadableDescriptionLanguage = HumanReadableDescriptionLanguage;
			this.LocalizedDescriptions = LocalizedDescriptions.ToArray();

			this.Updated = DateTime.Now;
			await Database.Update(this);
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
		/// If the gateway uses a domain name.
		/// </summary>
		public const string GATEWAY_DOMAIN_USE = nameof(GATEWAY_DOMAIN_USE);

		/// <summary>
		/// Main Domain Name of the gateway, if defined. If not provided, the gateway will not use a domain name.
		/// </summary>
		public const string GATEWAY_DOMAIN_NAME = nameof(GATEWAY_DOMAIN_NAME);

		// If a Domain Name is configured(<see cref="GATEWAY_DOMAIN"/> variable), the following variables define its operation:

		/// <summary>
		/// Comma-separated list of alternative domain names for the gateway, if defined.
		/// </summary>
		public const string GATEWAY_DOMAIN_ALT = nameof(GATEWAY_DOMAIN_ALT);

		/// <summary>
		/// true or 1 if gateway should use a Dynamic DNS-service, false or 0 if IP-address of Gateway is static.
		/// </summary>
		public const string GATEWAY_DYNDNS = nameof(GATEWAY_DYNDNS);

		/// <summary>
		/// true or 1 if gateway should use X.509-based encryption (for example TLS over HTTP, HTTPS), false or 0 if encryption is disabled.
		/// </summary>
		public const string GATEWAY_ENCRYPTION = nameof(GATEWAY_ENCRYPTION);

		/// <summary>
		/// true or 1 if gateway should use a custom Certificate Authority (must support ACME), false or 0 if [Let's Encrypt](https://letsencrypt.org/) should be used to generate certificates for the gateway.
		/// </summary>
		public const string GATEWAY_CA_CUSTOM = nameof(GATEWAY_CA_CUSTOM);

		/// <summary>
		/// E-mail address for contact person associated with generated certificates.
		/// </summary>
		public const string GATEWAY_ACME_EMAIL = nameof(GATEWAY_ACME_EMAIL);

		/// <summary>
		/// If Certificate Authority Terms of Services are accepted.
		/// </summary>
		public const string GATEWAY_ACME_ACCEPT_TOS = nameof(GATEWAY_ACME_ACCEPT_TOS);

		/// <summary>
		/// Default Human-readable name for gateway.
		/// </summary>
		public const string GATEWAY_HR_NAME = nameof(GATEWAY_HR_NAME);

		/// <summary>
		/// Language code (ISO-639-1) of <see cref="GATEWAY_HR_NAME"/>.
		/// </summary>
		public const string GATEWAY_HR_NAME_LANG = nameof(GATEWAY_HR_NAME_LANG);

		/// <summary>
		/// Default Human-readable description of gateway.
		/// </summary>
		public const string GATEWAY_HR_DESC = nameof(GATEWAY_HR_DESC);

		/// <summary>
		/// Language code (ISO-639-1) of <see cref="GATEWAY_HR_DESC"/>.
		/// </summary>
		public const string GATEWAY_HR_DESC_LANG = nameof(GATEWAY_HR_DESC_LANG);

		/// <summary>
		/// Comma-separated list of Language Codes (ISO-639-1) for available localizations of the human-readable name for the gateway.
		/// </summary>
		public const string GATEWAY_HR_NAME_LOC = nameof(GATEWAY_HR_NAME_LOC);

		/// <summary>
		/// Localized Human-readable name for the gateway, where "lang" is replaced by any of the ISO-639-1 language codes available in <see cref="GATEWAY_HR_NAME_LOC"/>.
		/// </summary>
		public const string GATEWAY_HR_NAME_ = nameof(GATEWAY_HR_NAME_);

		/// <summary>
		/// Comma-separated list of Language Codes (ISO-639-1) for available localizations of the human-readable description of the gateway.
		/// </summary>
		public const string GATEWAY_HR_DESC_LOC = nameof(GATEWAY_HR_DESC_LOC);

		/// <summary>
		/// Localized Human-readable description of the gateway, where "lang" is replaced by any of the ISO-639-1 language codes available in <see cref="GATEWAY_HR_DESC_LOC"/>.
		/// </summary>
		public const string GATEWAY_HR_DESC_ = nameof(GATEWAY_HR_DESC_);

		// If Dynamic DNS is configured (<see cref="GATEWAY_DYNDNS"/> variable), the following variables define its operation:

		/// <summary>
		/// Name of template to use for reporting IP address changes to the Dynamic DNS-service.
		/// </summary>
		public const string GATEWAY_DYNDNS_TEMPLATE = nameof(GATEWAY_DYNDNS_TEMPLATE);

		/// <summary>
		/// Script to use to check the current public IP address of the gateway.
		/// </summary>
		public const string GATEWAY_DYNDNS_CHECK = nameof(GATEWAY_DYNDNS_CHECK);

		/// <summary>
		/// Script to use to update the current public IP address of the gateway in the Dynamic DNS service.
		/// </summary>
		public const string GATEWAY_DYNDNS_UPDATE = nameof(GATEWAY_DYNDNS_UPDATE);

		/// <summary>
		/// Account of the gateway in the Dynamic DNS service.
		/// </summary>
		public const string GATEWAY_DYNDNS_ACCOUNT = nameof(GATEWAY_DYNDNS_ACCOUNT);

		/// <summary>
		/// Password of the Dynamic DNS service account.
		/// </summary>
		public const string GATEWAY_DYNDNS_PASSWORD = nameof(GATEWAY_DYNDNS_PASSWORD);

		/// <summary>
		/// Interval (in seconds) for checking if the IP address has changed.
		/// </summary>
		public const string GATEWAY_DYNDNS_INTERVAL = nameof(GATEWAY_DYNDNS_INTERVAL);

		// If a Custom Certificate Authority is configured(<see cref="GATEWAY_CA_CUSTOM"/> variable), the following variables define its operation:

		/// <summary>
		/// URL to the custom ACME directory to use to generate certificates for the gateway if a custom CA has been selected.
		/// </summary>
		public const string GATEWAY_ACME_DIRECTORY = nameof(GATEWAY_ACME_DIRECTORY);

		/// <summary>
		/// Environment configuration by configuring values available in environment variables.
		/// </summary>
		/// <returns>If the configuration was changed, and can be considered completed.</returns>
		public override async Task<bool> EnvironmentConfiguration()
		{
			if (!this.TryGetEnvironmentVariable(GATEWAY_DOMAIN_USE, false, out this.useDomainName))
				return false;

			if (!this.useDomainName)
			{
				this.Domain = string.Empty;
				this.DynamicDns = false;
				this.UseEncryption = false;
				this.CustomCA = false;
				this.ContactEMail = string.Empty;
				this.AcceptToS = false;
			}
			else
			{
				if (!this.TryGetEnvironmentVariable(GATEWAY_DOMAIN_NAME, true, out string Value) ||
					string.IsNullOrEmpty(Value))
				{
					return !this.useDomainName;
				}

				if (string.Compare(Value, "localhost", true) == 0)
				{
					this.LogEnvironmentError("localhost is not a valid domain name.", GATEWAY_DOMAIN_NAME, Value);
					return false;
				}

				this.Domain = Value;

				Value = Environment.GetEnvironmentVariable(GATEWAY_DOMAIN_ALT);
				if (string.IsNullOrEmpty(Value))
					this.AlternativeDomains = null;
				else
				{
					string[] Parts = Value.Split(',');

					foreach (string Part in Parts)
					{
						if (string.Compare(Part, "localhost", true) == 0)
						{
							this.LogEnvironmentError("localhost is not a valid alternative domain name.", GATEWAY_DOMAIN_ALT, Value);
							return false;
						}
					}

					this.AlternativeDomains = Parts;
				}

				if (!this.TryGetEnvironmentVariable(GATEWAY_DYNDNS, out this.dynamicDns, false))
					return false;

				if (this.dynamicDns)
				{
					if (!this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_TEMPLATE, true, out Value))
						return false;

					this.DynDnsTemplate = Value;

					if (!this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_CHECK, true, out Value))
						return false;

					this.CheckIpScript = Value;

					if (!this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_UPDATE, true, out Value))
						return false;

					this.UpdateIpScript = Value;

					this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_ACCOUNT, false, out this.dynDnsAccount);
					this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_PASSWORD, false, out this.dynDnsPassword);

					if (!this.TryGetEnvironmentVariable(GATEWAY_DYNDNS_UPDATE, 60, 86400, true, ref this.dynDnsInterval))
						return false;
				}

				if (!this.TryGetEnvironmentVariable(GATEWAY_ENCRYPTION, true, out this.useEncryption))
					return false;

				if (this.useEncryption)
				{
					if (!this.TryGetEnvironmentVariable(GATEWAY_CA_CUSTOM, out this.customCA, false))
						return false;

					if (this.customCA)
					{
						if (!this.TryGetEnvironmentVariable(GATEWAY_ACME_DIRECTORY, true, out this.acmeDirectory))
							return false;
					}

					if (!this.TryGetEnvironmentVariable(GATEWAY_ACME_EMAIL, true, out this.contactEMail))
						return false;

					if (!this.TryGetEnvironmentVariable(GATEWAY_ACME_ACCEPT_TOS, true, out this.acceptToS))
						return false;
				}
			}

			AlternativeField LocalizedString = this.GetLocalizedEnvironmentVariable(GATEWAY_HR_NAME, GATEWAY_HR_NAME_LANG);
			if (LocalizedString is null)
				return false;

			this.HumanReadableName = LocalizedString.Value;
			this.HumanReadableNameLanguage = LocalizedString.Key;

			LocalizedString = this.GetLocalizedEnvironmentVariable(GATEWAY_HR_DESC, GATEWAY_HR_DESC_LANG);
			if (LocalizedString is null)
				return false;

			this.HumanReadableDescription = LocalizedString.Value;
			this.HumanReadableDescriptionLanguage = LocalizedString.Key;

			this.localizedNames = this.GetLocalizedEnvironmentVariables(GATEWAY_HR_NAME_LOC, GATEWAY_HR_NAME_);
			if (this.localizedNames is null)
				return false;

			this.localizedDescriptions = this.GetLocalizedEnvironmentVariables(GATEWAY_HR_DESC_LOC, GATEWAY_HR_DESC_);
			if (this.localizedDescriptions is null)
				return false;

			if (!await this.Test(null, true))                                                           // Tests domain names.
				return false;

			if (this.dynamicDns && !await this.CheckDynamicIp(true))                                    // Tests dynamic DNS settings.
				return false;

			if (this.useEncryption && this.useDomainName && !await this.CreateCertificate(null, true))  // Creates certificate.
				return false;

			return true;
		}

		private AlternativeField GetLocalizedEnvironmentVariable(string VariableName, string LanguageVariableName)
		{
			if (!this.TryGetEnvironmentVariable(VariableName, true, out string Value) ||
				!this.TryGetEnvironmentVariable(LanguageVariableName, true, out string Language))
			{
				return null;
			}

			return new AlternativeField(Value, Language);
		}

		private AlternativeField[] GetLocalizedEnvironmentVariables(string CollectionVariableName, string VariableName)
		{
			List<AlternativeField> Result = new List<AlternativeField>();

			string Value = Environment.GetEnvironmentVariable(CollectionVariableName);
			if (!string.IsNullOrEmpty(Value))
			{
				string[] Languages = Value.Split(',');
				string Name;

				foreach (string Language in Languages)
				{
					Name = VariableName + Language;

					if (!this.TryGetEnvironmentVariable(Name, true, out Value))
						return null;

					Result.Add(new AlternativeField(Language, Value));
				}
			}

			return Result.ToArray();
		}

	}
}
