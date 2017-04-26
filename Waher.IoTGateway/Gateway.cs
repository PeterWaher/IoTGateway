using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.WindowsEventLog;
using Waher.Events.XMPP;
using Waher.Mock;
using Waher.Networking.CoAP;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.Metering;
using Waher.WebService.Script;

namespace Waher.IoTGateway
{
	/// <summary>
	/// Login result.
	/// </summary>
	public enum LoginResult
	{
		/// <summary>
		/// Unable to connect to server.
		/// </summary>
		UnableToConnect,

		/// <summary>
		/// Login credentials invalid.
		/// </summary>
		InvalidLogin,

		/// <summary>
		/// Login successful
		/// </summary>
		Successful
	}

	public static class Gateway
	{
		private const string FormSignatureKey = "";     // Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";  // Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;
		private const int MaxChunkSize = 4096;

		private static LinkedList<KeyValuePair<string, int>> ports = new LinkedList<KeyValuePair<string, int>>();
		private static Dictionary<int, EventHandler> serviceCommandByNr = new Dictionary<int, EventHandler>();
		private static Dictionary<EventHandler, int> serviceCommandNrByCallback = new Dictionary<EventHandler, int>();
		private static SimpleXmppConfiguration xmppConfiguration;
		private static ThingRegistryClient thingRegistryClient = null;
		private static ProvisioningClient provisioningClient = null;
		private static XmppClient xmppClient = null;
		private static Networking.XMPP.InBandBytestreams.IbbClient ibbClient = null;
		private static Networking.XMPP.P2P.SOCKS5.Socks5Proxy socksProxy = null;
		private static SensorServer sensorServer = null;
		private static ControlServer controlServer = null;
		private static ConcentratorServer concentratorServer = null;
		private static Timer connectionTimer = null;
		private static X509Certificate2 certificate = null;
		private static HttpServer webServer = null;
		private static HttpxServer httpxServer = null;
		private static CoapClient coapEndpoint = null;
		private static FilesProvider databaseProvider;
		private static ClientEvents clientEvents = null;
		private static string domain = null;
		private static string ownerJid = null;
		private static string appDataFolder;
		private static string xmppConfigFileName;
		private static int nextServiceCommandNr = 128;
		private static int beforeUninstallCommandNr = 0;
		private static bool registered = false;
		private static bool connected = false;
		private static bool immediateReconnect;

		#region Life Cycle

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		public static bool Start(bool ConsoleOutput)
		{
			Semaphore StartingServer = new Semaphore(1, 1, "Waher.IoTGateway");
			if (!StartingServer.WaitOne(1000))
				return false; // Being started in another process.

			try
			{
				if (!ConsoleOutput)
					Log.Register(new WindowsEventLog("IoTGateway", "IoTGateway", 512));

				appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					appDataFolder += Path.DirectorySeparatorChar;

				appDataFolder += "IoT Gateway" + Path.DirectorySeparatorChar;

				Log.Register(new XmlFileEventSink("XML File Event Sink",
					appDataFolder + "Events" + Path.DirectorySeparatorChar + "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "EventXmlToHtml.xslt", 7));

				Log.Informational("Server starting up.");

				beforeUninstallCommandNr = Gateway.RegisterServiceCommand(BeforeUninstall);

				string RootFolder = appDataFolder + "Root" + Path.DirectorySeparatorChar;
				if (!Directory.Exists(RootFolder))
				{
					appDataFolder = string.Empty;
					RootFolder = "Root" + Path.DirectorySeparatorChar;
				}

				Script.Types.SetModuleParameter("AppData", appDataFolder);
				Script.Types.SetModuleParameter("Root", RootFolder);

				XmlDocument Config = new XmlDocument();

				string GatewayConfigFileName = appDataFolder + "Gateway.config";
				if (!File.Exists(GatewayConfigFileName))
					GatewayConfigFileName = "Gateway.config";

				Config.Load(GatewayConfigFileName);
				XML.Validate("Gateway.config", Config, "GatewayConfiguration", "http://waher.se/Schema/GatewayConfiguration.xsd",
					Resources.LoadSchema(typeof(Gateway).Namespace + ".Schema.GatewayConfiguration.xsd", typeof(Gateway).Assembly));

				domain = Config.DocumentElement["Domain"].InnerText;

				XmlElement DatabaseConfig = Config.DocumentElement["Database"];
				if (!CommonTypes.TryParse(DatabaseConfig.Attributes["encrypted"].Value, out bool Encrypted))
					Encrypted = true;

				databaseProvider = new FilesProvider(appDataFolder + DatabaseConfig.Attributes["folder"].Value,
					DatabaseConfig.Attributes["defaultCollectionName"].Value,
					int.Parse(DatabaseConfig.Attributes["blockSize"].Value),
					int.Parse(DatabaseConfig.Attributes["blocksInCache"].Value),
					int.Parse(DatabaseConfig.Attributes["blobBlockSize"].Value), Encoding.UTF8,
					int.Parse(DatabaseConfig.Attributes["timeoutMs"].Value),
					Encrypted, false);
				Database.Register(databaseProvider);

				xmppConfigFileName = Config.DocumentElement["XmppClient"].Attributes["configFileName"].Value;
				if (!File.Exists(xmppConfigFileName))
					xmppConfigFileName = appDataFolder + xmppConfigFileName;

				if (ConsoleOutput)
				{
					xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog(xmppConfigFileName,
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
						FormSignatureKey, FormSignatureSecret);
				}
				else if (File.Exists(xmppConfigFileName))
				{
					xmppConfiguration = new SimpleXmppConfiguration(xmppConfigFileName);
					RuntimeSettings.Set("XMPP.CONFIG", xmppConfiguration.ExportSimpleXmppConfiguration());
				}
				else
				{
					string XmppConfig = RuntimeSettings.Get("XMPP.CONFIG", string.Empty);
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(XmppConfig);
					xmppConfiguration = new SimpleXmppConfiguration(Doc);
				}

				xmppClient = xmppConfiguration.GetClient("en", false);
				xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);
				xmppClient.OnValidateSender += XmppClient_OnValidateSender;
				Script.Types.SetModuleParameter("XMPP", xmppClient);

				if (xmppConfiguration.Sniffer)
				{
					ISniffer Sniffer;

					if (ConsoleOutput)
					{
						Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount);
						xmppClient.Add(Sniffer);
					}

					Sniffer = new XmlFileSniffer(appDataFolder + "XMPP" + Path.DirectorySeparatorChar +
						"XMPP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
						appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
						7, BinaryPresentationMethod.ByteCount);
					xmppClient.Add(Sniffer);
				}

				if (!string.IsNullOrEmpty(xmppConfiguration.Events))
					Log.Register(new XmppEventSink("XMPP Event Sink", xmppClient, xmppConfiguration.Events, false));

				if (!string.IsNullOrEmpty(xmppConfiguration.ThingRegistry))
				{
					thingRegistryClient = new ThingRegistryClient(xmppClient, xmppConfiguration.ThingRegistry);
					thingRegistryClient.Claimed += ThingRegistryClient_Claimed;
					thingRegistryClient.Disowned += ThingRegistryClient_Disowned;
					thingRegistryClient.Removed += ThingRegistryClient_Removed;
				}

				if (!string.IsNullOrEmpty(xmppConfiguration.Provisioning))
					provisioningClient = new ProvisioningClient(xmppClient, xmppConfiguration.Provisioning);

				DateTime Now = DateTime.Now;
				int MsToNext = 60000 - (Now.Second * 1000 + Now.Millisecond);

				connectionTimer = new Timer(CheckConnection, null, MsToNext, 60000);
				xmppClient.OnStateChanged += XmppClient_OnStateChanged;
				xmppClient.OnPresenceSubscribe += XmppClient_OnPresenceSubscribe;
				xmppClient.OnPresenceUnsubscribe += XmppClient_OnPresenceUnsubscribe;
				xmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;

				ibbClient = new Networking.XMPP.InBandBytestreams.IbbClient(xmppClient, MaxChunkSize);
				Script.Types.SetModuleParameter("IBB", ibbClient);

				socksProxy = new Networking.XMPP.P2P.SOCKS5.Socks5Proxy(xmppClient);
				Script.Types.SetModuleParameter("SOCKS5", socksProxy);

				string CertificateLocalFileName = Config.DocumentElement["Certificate"].Attributes["configFileName"].Value;
				string CertificateFileName;
				string CertificateXml;
				string CertificatePassword;
				byte[] CertificateRaw;

				try
				{
					CertificateRaw = Convert.FromBase64String(RuntimeSettings.Get("CERTIFICATE.BASE64", string.Empty));
					CertificatePassword = RuntimeSettings.Get("CERTIFICATE.PWD", string.Empty);

					certificate = new X509Certificate2(CertificateRaw, CertificatePassword);
				}
				catch (Exception)
				{
					certificate = null;
				}

				if (File.Exists(CertificateFileName = appDataFolder + CertificateLocalFileName))
					CertificateXml = File.ReadAllText(CertificateFileName);
				else if (File.Exists(CertificateFileName = CertificateLocalFileName) && certificate == null)
					CertificateXml = File.ReadAllText(CertificateFileName);
				else
				{
					CertificateFileName = null;
					CertificateXml = null;
				}

				if (CertificateXml != null)
				{
					XmlDocument CertificateConfig = new XmlDocument();
					CertificateConfig.LoadXml(CertificateXml);

					XML.Validate(CertificateLocalFileName, CertificateConfig, "CertificateConfiguration", "http://waher.se/Schema/CertificateConfiguration.xsd",
						Resources.LoadSchema(typeof(Gateway).Namespace + ".Schema.CertificateConfiguration.xsd", typeof(Gateway).Assembly));

					CertificateLocalFileName = CertificateConfig.DocumentElement["FileName"].InnerText;

					if (File.Exists(appDataFolder + CertificateLocalFileName))
						CertificateLocalFileName = appDataFolder + CertificateLocalFileName;

					CertificateRaw = File.ReadAllBytes(CertificateLocalFileName);
					CertificatePassword = CertificateConfig.DocumentElement["Password"].InnerText;

					certificate = new X509Certificate2(CertificateRaw, CertificatePassword);

					RuntimeSettings.Set("CERTIFICATE.BASE64", Convert.ToBase64String(CertificateRaw, Base64FormattingOptions.None));
					RuntimeSettings.Set("CERTIFICATE.PWD", CertificatePassword);

					if (CertificateLocalFileName != "certificate.pfx" || CertificatePassword != "testexamplecom")
					{
						try
						{
							File.Delete(CertificateLocalFileName);
						}
						catch (Exception)
						{
							Log.Warning("Unable to delete " + CertificateLocalFileName + " after importing it into the encrypted database.");
						}

						try
						{
							File.Delete(CertificateFileName);
						}
						catch (Exception)
						{
							Log.Warning("Unable to delete " + CertificateFileName + " after importing it into the encrypted database.");
						}
					}
				}

				foreach (XmlNode N in Config.DocumentElement["Ports"].ChildNodes)
				{
					if (N.LocalName == "Port")
					{
						XmlElement E = (XmlElement)N;
						string Protocol = XML.Attribute(E, "protocol");
						if (!string.IsNullOrEmpty(Protocol) && int.TryParse(E.InnerText, out int Port))
							ports.AddLast(new KeyValuePair<string, int>(Protocol, Port));
					}
				}

				webServer = new HttpServer(GetConfigPorts("HTTP"), GetConfigPorts("HTTPS"), certificate);
				Script.Types.SetModuleParameter("HTTP", webServer);

				StringBuilder sb = new StringBuilder();

				foreach (int Port in webServer.OpenPorts)
					sb.AppendLine(Port.ToString());

				try
				{
					File.WriteAllText(appDataFolder + "Ports.txt", sb.ToString());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				HttpFolderResource HttpFolderResource;
				HttpxProxy HttpxProxy;

				webServer.Register(new HttpFolderResource("/Graphics", "Graphics", false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
				webServer.Register(new ScriptService("/Evaluate"));  // TODO: Add authentication mechanisms. Make service availability pluggable.
				webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, RootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(HttpxProxy = new HttpxProxy("/HttpxProxy", xmppClient, MaxChunkSize));
				webServer.Register("/", (req, resp) =>
				{
					throw new TemporaryRedirectException(Config.DocumentElement["DefaultPage"].InnerText);
				});
				webServer.Register(clientEvents = new ClientEvents());

				HttpFolderResource.AllowTypeConversion();

				httpxServer = new HttpxServer(xmppClient, webServer, MaxChunkSize);
				Script.Types.SetModuleParameter("HTTPX", HttpxProxy);
				Script.Types.SetModuleParameter("HTTPXS", httpxServer);

				HttpxProxy.IbbClient = ibbClient;
				httpxServer.IbbClient = ibbClient;

				HttpxProxy.Socks5Proxy = socksProxy;
				httpxServer.Socks5Proxy = socksProxy;

				if (xmppConfiguration.Sniffer)
				{
					ISniffer Sniffer;

					Sniffer = new XmlFileSniffer(appDataFolder + "HTTP" + Path.DirectorySeparatorChar +
						"HTTP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
						appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
						7, BinaryPresentationMethod.ByteCount);
					webServer.Add(Sniffer);
				}

				coapEndpoint = new CoapClient();
				Script.Types.SetModuleParameter("CoAP", coapEndpoint);

				sensorServer = new SensorServer(xmppClient, provisioningClient, true);
				sensorServer.OnExecuteReadoutRequest += SensorServer_OnExecuteReadoutRequest;
				Script.Types.SetModuleParameter("Sensor", sensorServer);

				controlServer = new ControlServer(xmppClient, provisioningClient);
				controlServer.OnGetControlParameters += ControlServer_OnGetControlParameters;
				Script.Types.SetModuleParameter("Control", controlServer);

				concentratorServer = new ConcentratorServer(xmppClient, new MeteringTopology());
				Script.Types.SetModuleParameter("Concentrator", concentratorServer);

				new Waher.Script.Statistics.Functions.Uniform(0, 0, null);	// Loads assembly.
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				StartingServer.Release();
				StartingServer.Dispose();

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			Task.Run(async () =>
			{
				try
				{
					try
					{
						string BinaryFolder = AppDomain.CurrentDomain.BaseDirectory;
						string[] LanguageFiles = Directory.GetFiles(BinaryFolder, "*.lng", SearchOption.AllDirectories);
						string FileName;

						if (LanguageFiles.Length > 0)
						{
							XmlSchema Schema = Resources.LoadSchema(Translator.SchemaResource, typeof(Translator).Assembly);

							foreach (string LanguageFile in LanguageFiles)
							{
								try
								{
									FileName = LanguageFile;
									if (FileName.StartsWith(BinaryFolder))
										FileName = FileName.Substring(BinaryFolder.Length);

									DateTime LastWriteTime = File.GetLastWriteTime(LanguageFile);
									DateTime LastImportedTime = await RuntimeSettings.GetAsync(FileName, DateTime.MinValue);

									if (LastWriteTime > LastImportedTime)
									{
										Log.Informational("Importing language file.", FileName);

										string Xml = File.ReadAllText(LanguageFile);
										XmlDocument Doc = new XmlDocument();
										Doc.LoadXml(Xml);

										XML.Validate(FileName, Doc, Translator.SchemaRoot, Translator.SchemaNamespace, Schema);

										using (XmlReader r = new XmlNodeReader(Doc))
										{
											await Translator.ImportAsync(r);
										}

										RuntimeSettings.Set(FileName, LastWriteTime);
									}
								}
								catch (Exception ex)
								{
									Log.Critical(ex, LanguageFile);
								}
							}
						}

						Script.Types.StartAllModules(int.MaxValue);
					}
					finally
					{
						StartingServer.Release();
						StartingServer.Dispose();
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
				finally
				{
					xmppClient.Connect();
				}
			});

			return true;
		}

		/// <summary>
		/// Stops the gateway.
		/// </summary>
		public static void Stop()
		{
			IDisposable Disposable;

			Log.Informational("Server shutting down.");

			/*
			if (databaseProvider != null)
			{
				Task<string> Task = databaseProvider.ExportXml(true);
				Task.Wait();
				File.WriteAllText(appDataFolder + "Stop.xml", Task.Result);
			}
			*/

			if (ibbClient != null)
			{
				ibbClient.Dispose();
				ibbClient = null;
			}

			if (httpxServer != null)
			{
				httpxServer.Dispose();
				httpxServer = null;
			}

			if (provisioningClient != null)
			{
				provisioningClient.Dispose();
				provisioningClient = null;
			}

			if (thingRegistryClient != null)
			{
				thingRegistryClient.Dispose();
				thingRegistryClient = null;
			}

			if (sensorServer != null)
			{
				sensorServer.Dispose();
				sensorServer = null;
			}

			if (controlServer != null)
			{
				controlServer.Dispose();
				controlServer = null;
			}

			if (concentratorServer != null)
			{
				concentratorServer.Dispose();
				concentratorServer = null;
			}

			if (xmppClient != null)
			{
				using (ManualResetEvent OfflineSent = new ManualResetEvent(false))
				{
					xmppClient.SetPresence(Availability.Offline, string.Empty, (sender, e) => OfflineSent.Set());
					OfflineSent.WaitOne(1000);
				}

				foreach (ISniffer Sniffer in xmppClient.Sniffers)
				{
					XmppClient.Remove(Sniffer);

					Disposable = Sniffer as IDisposable;
					if (Disposable != null)
						Disposable.Dispose();
				}

				xmppClient.Dispose();
				xmppClient = null;
			}

			if (connectionTimer != null)
			{
				connectionTimer.Dispose();
				connectionTimer = null;
			}

			if (coapEndpoint != null)
			{
				coapEndpoint.Dispose();
				coapEndpoint = null;
			}

			if (webServer != null)
			{
				foreach (ISniffer Sniffer in webServer.Sniffers)
				{
					webServer.Remove(Sniffer);

					Disposable = Sniffer as IDisposable;
					if (Disposable != null)
						Disposable.Dispose();
				}

				webServer.Dispose();
				webServer = null;
			}

			clientEvents = null;
		}

		/// <summary>
		/// Domain certificate.
		/// </summary>
		public static X509Certificate2 Certificate
		{
			get { return certificate; }
		}

		/// <summary>
		/// Domain name.
		/// </summary>
		public static string Domain
		{
			get { return domain; }
		}

		/// <summary>
		/// Gets the port numbers defined for a given protocol in the configuration file.
		/// </summary>
		/// <param name="Protocol">Protocol.</param>
		/// <returns>Defined port numbers.</returns>
		public static int[] GetConfigPorts(string Protocol)
		{
			List<int> Result = new List<int>();

			foreach (KeyValuePair<string, int> P in ports)
			{
				if (P.Key == Protocol)
					Result.Add(P.Value);
			}

			return Result.ToArray();
		}

		#endregion

		#region XMPP

		private static void XmppClient_OnValidateSender(object Sender, ValidateSenderEventArgs e)
		{
			RosterItem Item;
			string BareJid = e.FromBareJID.ToLower();

			if (string.IsNullOrEmpty(BareJid) || (xmppClient != null && (BareJid == xmppClient.Domain.ToLower() || BareJid == xmppClient.BareJID.ToLower())))
				e.Accept();

			else if (BareJid.IndexOf('@') > 0 && (xmppClient == null || (Item = xmppClient.GetRosterItem(BareJid)) == null ||
				(Item.State != SubscriptionState.Both && Item.State != SubscriptionState.From)))
			{
				foreach (XmlNode N in e.Stanza.ChildNodes)
				{
					if (N.LocalName == "query" && N.NamespaceURI == XmppClient.NamespaceServiceDiscoveryInfo)
						return;
				}

				e.Reject();
			}
		}

		private static void CheckConnection(object State)
		{
			if (xmppClient.State == XmppState.Offline || xmppClient.State == XmppState.Error || xmppClient.State == XmppState.Authenticating)
			{
				try
				{
					xmppClient.Reconnect();
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}

			EventHandler h = MinuteTick;
			if (h != null)
			{
				try
				{
					h(null, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			}
		}

		/// <summary>
		/// Event raised every minute.
		/// </summary>
		public static event EventHandler MinuteTick = null;

		private static void XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			switch (NewState)
			{
				case XmppState.Connected:
					connected = true;

					if (!registered && thingRegistryClient != null)
						Register();

					if (!socksProxy.HasProxy)
						socksProxy.StartSearch(null);
					break;

				case XmppState.Offline:
					immediateReconnect = connected;
					connected = false;

					if (immediateReconnect && xmppClient != null)
						xmppClient.Reconnect();
					break;
			}
		}

		private static void XmppClient_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();     // TODO: Provisioning

			RosterItem Item = xmppClient.GetRosterItem(e.FromBareJID);
			if (Item == null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
				xmppClient.RequestPresenceSubscription(e.FromBareJID);

			xmppClient.SetPresence(Availability.Chat);
		}

		private static void XmppClient_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			e.Accept();
		}

		private static void XmppClient_OnRosterItemUpdated(object Sender, RosterItem Item)
		{
			//if (Item.State == SubscriptionState.None && Item.PendingSubscription != PendingSubscription.Subscribe)
			//	xmppClient.RemoveRosterItem(Item.BareJid);
		}

		/// <summary>
		/// Performs a login operation on the main XMPP account, on the main XMPP account domain.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="RemoteEndPoint">Remote End-Point.</param>
		/// <returns>If the login-operation was successful or not.</returns>
		public static async Task<LoginResult> DoMainXmppLogin(string UserName, string Password, string RemoteEndPoint)
		{
			if (xmppClient == null || xmppClient.UserName != UserName)
			{
				Log.Notice("Invalid login.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
				return LoginResult.InvalidLogin;
			}

			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			int Result = -1;
			string PasswordHash;
			string PasswordHashMethod;
			bool Connected = false;

			using (XmppClient Client = new XmppClient(xmppClient.Host, xmppClient.Port, UserName, Password, "en"))
			{
				Client.AllowCramMD5 = xmppClient.AllowCramMD5;
				Client.AllowDigestMD5 = xmppClient.AllowDigestMD5;
				Client.AllowPlain = xmppClient.AllowPlain;
				Client.AllowScramSHA1 = xmppClient.AllowScramSHA1;
				Client.AllowEncryption = xmppClient.AllowEncryption;

				Client.OnStateChanged += (sender, NewState) =>
				{
					switch (NewState)
					{
						case XmppState.StreamOpened:
							Connected = true;
							break;

						case XmppState.Binding:
							Done.Set();
							break;

						case XmppState.Error:
							Error.Set();
							break;
					}
				};

				Client.Connect();

				await Task.Run(() => Result = WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));

				PasswordHash = Client.PasswordHash;
				PasswordHashMethod = Client.PasswordHashMethod;
			}

			if (Result != 0)
			{
				if (Connected)
				{
					Log.Notice("Invalid login.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
					return LoginResult.InvalidLogin;
				}
				else
				{
					if ((RemoteEndPoint.StartsWith("[::1]:") || RemoteEndPoint.StartsWith("127.0.0.1:")) &&
						UserName == xmppConfiguration.Account && Password == xmppConfiguration.Password &&
						string.IsNullOrEmpty(xmppConfiguration.PasswordType))
					{
						Log.Notice("Successful login. Connection to XMPP broker down. Credentials matched configuration and connection made from same machine.", UserName, RemoteEndPoint, "Login", EventLevel.Minor);
						return LoginResult.Successful;
					}
					else
					{
						Log.Notice("Unable to connect to XMPP broker.", UserName, RemoteEndPoint, "LoginFailure", EventLevel.Minor);
						return LoginResult.UnableToConnect;
					}
				}
			}

			Log.Informational("Successful login.", UserName, RemoteEndPoint, "Login", EventLevel.Minor);

			if (xmppClient.State != XmppState.Connected &&
				(xmppClient.PasswordHash != PasswordHash || xmppClient.PasswordHashMethod != PasswordHashMethod))
			{
				Log.Notice("XMPP credentials updated.", UserName, RemoteEndPoint, "CredentialsUpdated", EventLevel.Minor);

				xmppClient.Reconnect(UserName, PasswordHash, PasswordHashMethod);

				xmppConfiguration.Account = UserName;
				xmppConfiguration.Password = PasswordHash;
				xmppConfiguration.PasswordType = PasswordHashMethod;

				xmppConfiguration.SaveSimpleXmppConfiguration(xmppConfigFileName);
			}

			return LoginResult.Successful;
		}

		#endregion

		#region Thing Registry

		private static void ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			ownerJid = e.JID;
			Log.Informational("Thing has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
		}

		private static void ThingRegistryClient_Disowned(object Sender, NodeEventArgs e)
		{
			Log.Informational("Thing has been disowned.", ownerJid);
			ownerJid = string.Empty;
			Register();
		}

		private static void ThingRegistryClient_Removed(object Sender, NodeEventArgs e)
		{
			Log.Informational("Thing has been removed from the public registry.", ownerJid);
		}

		private static void Register()
		{
			string Key = Guid.NewGuid().ToString().Replace("-", string.Empty);

			// For info on tag names, see: http://xmpp.org/extensions/xep-0347.html#tags
			MetaDataTag[] MetaData = new MetaDataTag[]
			{
				new MetaDataStringTag("KEY", Key),
				new MetaDataStringTag("CLASS", "Gateway"),
				new MetaDataStringTag("MAN", "waher.se"),
				new MetaDataStringTag("MODEL", "Waher.IoTGateway"),
				new MetaDataStringTag("PURL", "https://github.com/PeterWaher/IoTGateway#iotgateway"),
				new MetaDataNumericTag("V",1.0)
			};

			thingRegistryClient.RegisterThing(MetaData, (sender2, e2) =>
			{
				if (e2.Ok)
				{
					registered = true;

					if (e2.IsClaimed)
						ownerJid = e2.OwnerJid;
					else
					{
						ownerJid = string.Empty;
						SimpleXmppConfiguration.PrintQRCode(thingRegistryClient.EncodeAsIoTDiscoURI(MetaData));
					}
				}
			}, null);
		}

		internal static XmppClient XmppClient
		{
			get { return xmppClient; }
		}

		// TODO: Teman: http://mmistakes.github.io/skinny-bones-jekyll/, http://jekyllrb.com/

		#endregion

		#region Sensors & Controllers

		private static async void SensorServer_OnExecuteReadoutRequest(object Sender, SensorDataServerRequest Request)
		{
			try
			{
				DateTime TP = DateTime.Now;
				INode Node;
				ISensor Sensor;

				foreach (ThingReference NodeRef in Request.Nodes)
				{
					if (!concentratorServer.TryGetDataSource(NodeRef.SourceId, out IDataSource Source))
					{
						Request.ReportErrors(false, new ThingError(NodeRef, TP, "Data source not found."));
						continue;
					}

					Node = await Source.GetNodeAsync(NodeRef);
					if (Node == null)
					{
						Request.ReportErrors(false, new ThingError(NodeRef, TP, "Node not found."));
						continue;
					}

					Sensor = Node as ISensor;
					if (Sensor == null)
					{
						Request.ReportErrors(false, new ThingError(NodeRef, TP, "Node not a sensor."));
						continue;
					}

					Sensor.StartReadout(Request);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static ControlParameter[] ControlServer_OnGetControlParameters(ThingReference NodeRef)
		{
			try
			{
				DateTime TP = DateTime.Now;
				INode Node;
				IActuator Actuator;

				if (!concentratorServer.TryGetDataSource(NodeRef.SourceId, out IDataSource Source))
					return null;

				Task<INode> T = Source.GetNodeAsync(NodeRef);
				T.Wait();
				Node = T.Result;

				if (Node == null)
					return null;

				Actuator = Node as IActuator;
				if (Actuator == null)
					return null;

				return Actuator.GetControlParameters();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}
		}

		#endregion

		#region Service Commands

		/// <summary>
		/// Executes a service command.
		/// 
		/// Command must have been registered with <see cref="RegisterServiceCommand(EventHandler)"/> before being executed.
		/// </summary>
		/// <param name="CommandNr">Command number.</param>
		/// <returns>If a service command with the given number was found and executed.</returns>
		public static bool ExecuteServiceCommand(int CommandNr)
		{
			EventHandler h;

			lock (serviceCommandByNr)
			{
				if (!serviceCommandByNr.TryGetValue(CommandNr, out h))
					h = null;
			}

			if (h == null)
			{
				Log.Warning("Service command lacking command handler invoked.", CommandNr.ToString());
				return false;
			}
			else
			{
				try
				{
					h(null, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return true;
			}
		}

		/// <summary>
		/// Registers an administrative service command.
		/// </summary>
		/// <param name="Callback">Method to call when service command is invoked.</param>
		/// <returns>Command number assigned to the command.</returns>
		public static int RegisterServiceCommand(EventHandler Callback)
		{
			int i;

			lock (serviceCommandByNr)
			{
				if (serviceCommandNrByCallback.TryGetValue(Callback, out i))
					return i;

				i = nextServiceCommandNr++;

				serviceCommandNrByCallback[Callback] = i;
				serviceCommandByNr[i] = Callback;
			}

			return i;
		}

		/// <summary>
		/// Unregisters an administrative service command.
		/// </summary>
		/// <param name="Callback">Method serving the service command.</param>
		/// <returns>If the command was found and unregistered.</returns>
		public static bool UnregisterServiceCommand(EventHandler Callback)
		{
			lock (serviceCommandByNr)
			{
				if (serviceCommandNrByCallback.TryGetValue(Callback, out int i))
				{
					serviceCommandByNr.Remove(i);
					serviceCommandNrByCallback.Remove(Callback);

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Service command number related to the BeforeUninstall Service command registered by the gateway.
		/// </summary>
		public static int BeforeUninstallCommandNr
		{
			get { return beforeUninstallCommandNr; }
		}

		/// <summary>
		/// Event raised before the application is uninstalled.
		/// </summary>
		public static event EventHandler OnBeforeUninstall = null;

		private static void BeforeUninstall(object Sender, EventArgs e)
		{
			try
			{
				OnBeforeUninstall?.Invoke(Sender, e);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		#endregion

	}
}
