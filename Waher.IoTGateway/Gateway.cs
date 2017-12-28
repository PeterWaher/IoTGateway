using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown.Web;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
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
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;
using Waher.Runtime.Settings;
using Waher.Runtime.Timing;
using Waher.Persistence;
using Waher.Things;
using Waher.Things.ControlParameters;
using Waher.Things.Metering;

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

	/// <summary>
	/// Delegate for callback methods used for the creation of database providers.
	/// </summary>
	/// <param name="Definition">XML definition of provider.</param>
	/// <returns>Database provider.</returns>
	public delegate IDatabaseProvider GetDatabaseProviderCallback(XmlElement Definition);

	/// <summary>
	/// Static class managing the runtime environment of the IoT Gateway.
	/// </summary>
	public static class Gateway
	{
		private const string FormSignatureKey = "";     // Form signature key, if form signatures (XEP-0348) is to be used during registration.
		private const string FormSignatureSecret = "";  // Form signature secret, if form signatures (XEP-0348) is to be used during registration.
		private const int MaxRecordsPerPeriod = 500;
		private const int MaxChunkSize = 4096;

		internal static readonly Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24,
			"/Graphics/Emoji1/svg/%FILENAME%", File.Exists, File.ReadAllBytes);

		private static LinkedList<KeyValuePair<string, int>> ports = new LinkedList<KeyValuePair<string, int>>();
		private static Dictionary<int, EventHandler> serviceCommandByNr = new Dictionary<int, EventHandler>();
		private static Dictionary<EventHandler, int> serviceCommandNrByCallback = new Dictionary<EventHandler, int>();
		private static SimpleXmppConfiguration xmppConfiguration;
		private static ThingRegistryClient thingRegistryClient = null;
		private static ProvisioningClient provisioningClient = null;
		private static XmppClient xmppClient = null;
		private static Networking.XMPP.InBandBytestreams.IbbClient ibbClient = null;
		private static Networking.XMPP.P2P.SOCKS5.Socks5Proxy socksProxy = null;
		private static ConcentratorServer concentratorServer = null;
		private static Timer connectionTimer = null;
		private static X509Certificate2 certificate = null;
		private static HttpServer webServer = null;
		private static HttpxServer httpxServer = null;
		private static CoapEndpoint coapEndpoint = null;
		private static ClientEvents clientEvents = null;
		private static Scheduler scheduler = null;
		private static RandomNumberGenerator rnd = null;
		private static Semaphore gatewayRunning = null;
		private static string domain = null;
		private static string ownerJid = null;
		private static string appDataFolder;
		private static string runtimeFolder;
		private static string rootFolder;
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
		/// <param name="GetDatabaseProvider">Callback method for the creation of database provider.</param>
		public static bool Start(bool ConsoleOutput, GetDatabaseProviderCallback GetDatabaseProvider)
		{
			gatewayRunning = new Semaphore(1, 1, "Waher.IoTGateway.Running");
			if (!gatewayRunning.WaitOne(1000))
				return false; // Is running in another process.

			Semaphore StartingServer = new Semaphore(1, 1, "Waher.IoTGateway.Starting");
			if (!StartingServer.WaitOne(1000))
			{
				gatewayRunning.Release();
				gatewayRunning.Dispose();
				gatewayRunning = null;

				StartingServer.Dispose();
				return false; // Being started in another process.
			}

			try
			{
				appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					appDataFolder += Path.DirectorySeparatorChar;

				appDataFolder += "IoT Gateway" + Path.DirectorySeparatorChar;

				Log.Register(new XmlFileEventSink("XML File Event Sink",
					appDataFolder + "Events" + Path.DirectorySeparatorChar + "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "EventXmlToHtml.xslt", 7));

				Log.Informational("Server starting up.");

				Initialize();

				beforeUninstallCommandNr = Gateway.RegisterServiceCommand(BeforeUninstall);

				rootFolder = appDataFolder + "Root" + Path.DirectorySeparatorChar;
				if (!Directory.Exists(rootFolder))
				{
					string s = Path.Combine(runtimeFolder, "Root");
					if (Directory.Exists(s))
					{
						CopyFolder(runtimeFolder, appDataFolder, "*.config", true);
						CopyFolder(runtimeFolder, appDataFolder, "*.pfx", true);
						CopyFolders(s, rootFolder, true);
						CopyFolders(Path.Combine(runtimeFolder, "Graphics"), Path.Combine(appDataFolder, "Graphics"), true);
						CopyFolders(Path.Combine(runtimeFolder, "Transforms"), Path.Combine(appDataFolder, "Transforms"), true);
					}
					else
					{
						appDataFolder = string.Empty;
						rootFolder = "Root" + Path.DirectorySeparatorChar;
					}
				}

				Types.SetModuleParameter("AppData", appDataFolder);
				Types.SetModuleParameter("Root", rootFolder);

				scheduler = new Scheduler();
				rnd = RandomNumberGenerator.Create();

				Task.Run(() => CodeContent.GraphViz.Init());

				XmlDocument Config = new XmlDocument();

				string GatewayConfigFileName = appDataFolder + "Gateway.config";
				if (!File.Exists(GatewayConfigFileName))
					GatewayConfigFileName = "Gateway.config";

				Config.Load(GatewayConfigFileName);
				XSL.Validate("Gateway.config", Config, "GatewayConfiguration", "http://waher.se/Schema/GatewayConfiguration.xsd",
					XSL.LoadSchema(typeof(Gateway).Namespace + ".Schema.GatewayConfiguration.xsd", typeof(Gateway).Assembly));

				domain = Config.DocumentElement["Domain"].InnerText;

				XmlElement DatabaseConfig = Config.DocumentElement["Database"];
				IDatabaseProvider DatabaseProvider = GetDatabaseProvider(DatabaseConfig);
				Database.Register(DatabaseProvider);

				PersistedEventLog PersistedEventLog = new PersistedEventLog(7, new TimeSpan(4, 15, 0));
				Log.Register(PersistedEventLog);
				PersistedEventLog.Queue(new Event(EventType.Informational, "Server starting up.", string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty));

				xmppConfigFileName = Config.DocumentElement["XmppClient"].Attributes["configFileName"].Value;
				if (!File.Exists(xmppConfigFileName))
					xmppConfigFileName = appDataFolder + xmppConfigFileName;

				if (ConsoleOutput)
				{
					xmppConfiguration = SimpleXmppConfiguration.GetConfigUsingSimpleConsoleDialog(xmppConfigFileName,
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default user name.
						Guid.NewGuid().ToString().Replace("-", string.Empty),   // Default password.
						FormSignatureKey, FormSignatureSecret, typeof(Gateway).Assembly);
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

				xmppClient = xmppConfiguration.GetClient("en", typeof(Gateway).Assembly, false);
				xmppClient.AllowRegistration(FormSignatureKey, FormSignatureSecret);
				xmppClient.OnValidateSender += XmppClient_OnValidateSender;
				Types.SetModuleParameter("XMPP", xmppClient);

				if (xmppConfiguration.Sniffer)
				{
					ISniffer Sniffer;

					if (ConsoleOutput)
					{
						Sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.PadWithSpaces);
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
				Types.SetModuleParameter("IBB", ibbClient);

				socksProxy = new Networking.XMPP.P2P.SOCKS5.Socks5Proxy(xmppClient);
				Types.SetModuleParameter("SOCKS5", socksProxy);

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

					XSL.Validate(CertificateLocalFileName, CertificateConfig, "CertificateConfiguration", "http://waher.se/Schema/CertificateConfiguration.xsd",
						XSL.LoadSchema(typeof(Gateway).Namespace + ".Schema.CertificateConfiguration.xsd", typeof(Gateway).Assembly));

					CertificateLocalFileName = CertificateConfig.DocumentElement["FileName"].InnerText;

					if (File.Exists(appDataFolder + CertificateLocalFileName))
						CertificateLocalFileName = appDataFolder + CertificateLocalFileName;

					CertificateRaw = File.ReadAllBytes(CertificateLocalFileName);
					CertificatePassword = CertificateConfig.DocumentElement["Password"].InnerText;

					certificate = new X509Certificate2(CertificateRaw, CertificatePassword);

					RuntimeSettings.Set("CERTIFICATE.BASE64", Convert.ToBase64String(CertificateRaw));
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
				Types.SetModuleParameter("HTTP", webServer);

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
				webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(HttpxProxy = new HttpxProxy("/HttpxProxy", xmppClient, MaxChunkSize));
				webServer.Register("/", (req, resp) =>
				{
					throw new TemporaryRedirectException(Config.DocumentElement["DefaultPage"].InnerText);
				});
				webServer.Register(clientEvents = new ClientEvents());

				HttpFolderResource.AllowTypeConversion();
				MarkdownToHtmlConverter.EmojiSource = Emoji1_24x24;

				XmlElement FileFolders = Config.DocumentElement["FileFolders"];
				if (FileFolders != null)
				{
					foreach (XmlNode N in FileFolders.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "FileFolder")
						{
							string WebFolder = XML.Attribute(E, "webFolder");
							string FolderPath = XML.Attribute(E, "folderPath");

							webServer.Register(new HttpFolderResource(WebFolder, FolderPath, false, false, true, true));
						}
					}
				}

				httpxServer = new HttpxServer(xmppClient, webServer, MaxChunkSize);
				Types.SetModuleParameter("HTTPX", HttpxProxy);
				Types.SetModuleParameter("HTTPXS", httpxServer);

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

				coapEndpoint = new CoapEndpoint();
				Types.SetModuleParameter("CoAP", coapEndpoint);

				concentratorServer = new ConcentratorServer(xmppClient, provisioningClient, new MeteringTopology());
				Types.SetModuleParameter("Concentrator", concentratorServer);
				Types.SetModuleParameter("Sensor", concentratorServer.SensorServer);
				Types.SetModuleParameter("Control", concentratorServer.ControlServer);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				gatewayRunning.Release();
				gatewayRunning.Dispose();
				gatewayRunning = null;

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
							XmlSchema Schema = XSL.LoadSchema(Translator.SchemaResource, typeof(Translator).Assembly);

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

										XSL.Validate(FileName, Doc, Translator.SchemaRoot, Translator.SchemaNamespace, Schema);

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

						Types.StartAllModules(int.MaxValue);
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
		/// Initializes the inventory engine by loading available assemblies in the installation folder (top directory only).
		/// </summary>
		private static void Initialize()
		{
			runtimeFolder = Path.GetDirectoryName(typeof(Gateway).GetTypeInfo().Assembly.Location);
			Directory.SetCurrentDirectory(runtimeFolder);

			TypesLoader.Initialize(runtimeFolder, (FileName) =>
			{
				FileName = Path.GetFileName(FileName).ToLower();
				if (FileName.StartsWith("api-ms-win-") ||
					FileName.StartsWith("system.") ||
					FileName.StartsWith("microsoft.") ||
					FileName.StartsWith("windows.") ||
					FileName.StartsWith("waher.client.") ||
					FileName.StartsWith("waher.utility."))
				{
					return false;
				}

				switch (FileName)
				{
					case "clrcompression.dll":
					case "clretwrc.dll":
					case "clrjit.dll":
					case "coreclr.dll":
					case "dbgshim.dll":
					case "hostpolicy.dll":
					case "hostfxr.dll":
					case "libegl.dll":
					case "libglesv2.dll":
					case "libskiasharp.dll":
					case "mscordaccore.dll":
					case "mscordaccore_x86_x86_4.6.00001.0.dll":
					case "mscordbi.dll":
					case "mscorlib.dll":
					case "mscorrc.debug.dll":
					case "mscorrc.dll":
					case "netstandard.dll":
					case "sos.dll":
					case "sos.netcore.dll":
					case "sos_x86_x86_4.6.00001.0.dll":
					case "ucrtbase.dll":
					case "windowsbase.dll":
						return false;
				}

				return true;
			});
		}

		private static bool CopyFile(string From, string To, bool OnlyIfNewer)
		{
			if (From == To)
				return false;

			if (!File.Exists(From))
				return false;

			if (OnlyIfNewer && File.Exists(To))
			{
				DateTime ToTP = File.GetLastWriteTimeUtc(To);
				DateTime FromTP = File.GetLastWriteTimeUtc(From);

				if (ToTP >= FromTP)
					return false;
			}

			File.Copy(From, To, true);

			return true;
		}

		private static void CopyFolder(string From, string To, string Mask, bool OnlyIfNewer)
		{
			if (Directory.Exists(From))
			{
				if (!Directory.Exists(To))
					Directory.CreateDirectory(To);

				string[] Files = Directory.GetFiles(From, Mask, SearchOption.TopDirectoryOnly);

				foreach (string File in Files)
				{
					string FileName = Path.GetFileName(File);
					CopyFile(File, Path.Combine(To, FileName), OnlyIfNewer);
				}
			}
		}

		private static void CopyFolders(string From, string To, bool OnlyIfNewer)
		{
			CopyFolder(From, To, "*.*", OnlyIfNewer);

			string[] Folders = Directory.GetDirectories(From, "*.*", SearchOption.TopDirectoryOnly);

			foreach (string Folder in Folders)
			{
				string FolderName = Path.GetFileName(Folder);
				CopyFolders(Folder, Path.Combine(To, FolderName), OnlyIfNewer);
			}
		}

		/// <summary>
		/// Stops the gateway.
		/// </summary>
		public static void Stop()
		{
			IDisposable Disposable;

			Log.Informational("Server shutting down.");

			if (gatewayRunning != null)
			{
				gatewayRunning.Release();
				gatewayRunning.Dispose();
				gatewayRunning = null;
			}

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
		/// Application data folder.
		/// </summary>
		public static string AppDataFolder
		{
			get { return appDataFolder; }
		}

		/// <summary>
		/// Runtime folder.
		/// </summary>
		public static string RuntimeFolder
		{
			get { return runtimeFolder; }
		}

		/// <summary>
		/// Web root folder.
		/// </summary>
		public static string RootFolder
		{
			get { return rootFolder; }
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

					MarkdownToHtmlConverter.BareJID = xmppClient.BareJID;

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

			using (XmppClient Client = new XmppClient(xmppClient.Host, xmppClient.Port, UserName, Password, "en", typeof(Gateway).Assembly))
			{
				Client.TrustServer = xmppClient.TrustServer;
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
						SimpleXmppConfiguration.PrintQRCode(ThingRegistryClient.EncodeAsIoTDiscoURI(MetaData));
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

		#region Scheduling

		/// <summary>
		/// Schedules a one-time event.
		/// </summary>
		/// <param name="Callback">Method to call when event is due.</param>
		/// <param name="When">When the event is to be executed.</param>
		/// <param name="State">State object</param>
		/// <returns>Timepoint of when event was scheduled.</returns>
		public static DateTime ScheduleEvent(ScheduledEventCallback Callback, DateTime When, object State)
		{
			if (scheduler != null)
				return scheduler.Add(When, Callback, State);
			else
				return DateTime.MinValue;
		}

		/// <summary>
		/// Cancels a scheduled event.
		/// </summary>
		/// <param name="When">When event is scheduled</param>
		/// <returns>If event was found and removed.</returns>
		public static bool CancelScheduledEvent(DateTime When)
		{
			return scheduler?.Remove(When) ?? false;
		}

		#endregion

		#region Random number generation

		/// <summary>
		/// Generates a new floating-point value between 0 and 1, using a cryptographic random number generator.
		/// </summary>
		/// <returns>Random number.</returns>
		public static double NextDouble()
		{
			byte[] b = new byte[8];

			lock (rnd)
			{
				rnd.GetBytes(b);
			}

			double d = BitConverter.ToUInt64(b, 0);
			d /= ulong.MaxValue;

			return d;
		}

		#endregion

	}
}
