using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Web;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
using Waher.Events.XMPP;
using Waher.IoTGateway.WebResources;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.IoTGateway.Setup;
using Waher.Networking.CoAP;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Mail;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.Synchronization;
using Waher.Runtime.Language;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;
using Waher.Runtime.Settings;
using Waher.Runtime.Timing;
using Waher.Persistence;
using Waher.Script;
using Waher.Security;
using Waher.Things;
using Waher.Things.Metering;
using Waher.Things.SensorData;

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
	public delegate Task<IDatabaseProvider> GetDatabaseProviderEventHandler(XmlElement Definition);

	/// <summary>
	/// Delegate for registration callback methods.
	/// </summary>
	/// <param name="MetaData">Meta data used in registration.</param>
	/// <param name="e">Event arguments.</param>
	public delegate Task RegistrationEventHandler(MetaDataTag[] MetaData, RegistrationEventArgs e);

	/// <summary>
	/// Delegate for events requesting meta data for registration.
	/// </summary>
	/// <param name="MetaData">Defult meta data.</param>
	/// <returns>Meta data to register.</returns>
	public delegate Task<MetaDataTag[]> GetRegistryMetaDataEventHandler(MetaDataTag[] MetaData);

	/// <summary>
	/// Delegate for events requesting an array of data sources.
	/// </summary>
	/// <param name="DataSources">Default set of data sources.</param>
	/// <returns>Data sources to publish.</returns>
	public delegate Task<IDataSource[]> GetDataSources(params IDataSource[] DataSources);

	/// <summary>
	/// Static class managing the runtime environment of the IoT Gateway.
	/// </summary>
	public static class Gateway
	{
		private const int MaxRecordsPerPeriod = 500;
		private const int MaxChunkSize = 4096;

		private static LinkedList<KeyValuePair<string, int>> ports = new LinkedList<KeyValuePair<string, int>>();
		private static Dictionary<int, EventHandler> serviceCommandByNr = new Dictionary<int, EventHandler>();
		private static Dictionary<EventHandler, int> serviceCommandNrByCallback = new Dictionary<EventHandler, int>();
		private static ThingRegistryClient thingRegistryClient = null;
		private static ProvisioningClient provisioningClient = null;
		private static XmppCredentials xmppCredentials = null;
		private static XmppClient xmppClient = null;
		private static Networking.XMPP.Avatar.AvatarClient avatarClient = null;
		private static Networking.XMPP.InBandBytestreams.IbbClient ibbClient = null;
		private static Networking.XMPP.P2P.SOCKS5.Socks5Proxy socksProxy = null;
		private static ConcentratorServer concentratorServer = null;
		private static SynchronizationClient synchronizationClient = null;
		private static PepClient pepClient = null;
		private static ContractsClient contractsClient = null;
		private static MailClient mailClient = null;
		private static X509Certificate2 certificate = null;
		private static HttpServer webServer = null;
		private static HttpxProxy httpxProxy = null;
		private static HttpxServer httpxServer = null;
		private static CoapEndpoint coapEndpoint = null;
		private static ClientEvents clientEvents = null;
		private static ClientEventsWebSocket clientEventsWs = null;
		private static Login login = null;
		private static Logout logout = null;
		private static LoggedIn loggedIn = null;
		private static HttpFolderResource exportFolder = null;
		private static HttpFolderResource keyFolder = null;
		private static StartExport startExport = null;
		private static StartAnalyze startAnalyze = null;
		private static DeleteExport deleteExport = null;
		private static Scheduler scheduler = null;
		private static RandomNumberGenerator rnd = null;
		private static Semaphore gatewayRunning = null;
		private static Emoji1LocalFiles emoji1_24x24 = null;
		private static StreamWriter exceptionFile = null;
		private static CaseInsensitiveString domain = null;
		private static CaseInsensitiveString ownerJid = null;
		private static string appDataFolder;
		private static string runtimeFolder;
		private static string rootFolder;
		private static string defaultPage;
		private static string applicationName;
		private static string exceptionFolder = null;
		private static string exceptionFileName = null;
		private static int nextServiceCommandNr = 128;
		private static int beforeUninstallCommandNr = 0;
		private static bool registered = false;
		private static bool connected = false;
		private static bool immediateReconnect;
		private static bool consoleOutput;
		private static bool loopbackIntefaceAvailable;
		private static bool configuring = false;
		private static bool exportExceptions = false;
		private static bool stopped = false;

		#region Life Cycle

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		/// <returns>If the gateway was successfully started.</returns>
		public static Task<bool> Start(bool ConsoleOutput)
		{
			return Start(ConsoleOutput, true);
		}

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		/// <param name="LoopbackIntefaceAvailable">If the loopback interface is available.</param>
		/// <returns>If the gateway was successfully started.</returns>
		public static async Task<bool> Start(bool ConsoleOutput, bool LoopbackIntefaceAvailable)
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
				StartingServer = null;
				return false; // Being started in another process.
			}

			try
			{
				stopped = false;
				consoleOutput = ConsoleOutput;
				loopbackIntefaceAvailable = LoopbackIntefaceAvailable;

				appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
					appDataFolder += Path.DirectorySeparatorChar;

				appDataFolder += "IoT Gateway" + Path.DirectorySeparatorChar;
				rootFolder = appDataFolder + "Root" + Path.DirectorySeparatorChar;

				Log.Register(new XmlFileEventSink("XML File Event Sink",
					appDataFolder + "Events" + Path.DirectorySeparatorChar + "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "EventXmlToHtml.xslt", 7));

				Log.Informational("Server starting up.");

				Initialize();

				beforeUninstallCommandNr = Gateway.RegisterServiceCommand(BeforeUninstall);

				if (!Directory.Exists(rootFolder))
				{
					string s = Path.Combine(runtimeFolder, "Root");
					if (Directory.Exists(s))
					{
						CopyFolder(runtimeFolder, appDataFolder, "*.config", true);
						CopyFolders(s, rootFolder, true);
						CopyFolders(Path.Combine(runtimeFolder, "Graphics"), Path.Combine(appDataFolder, "Graphics"), true);
						CopyFolders(Path.Combine(runtimeFolder, "Transforms"), Path.Combine(appDataFolder, "Transforms"), true);
					}
				}

				string[] ManifestFiles = Directory.GetFiles(runtimeFolder, "*.manifest", SearchOption.TopDirectoryOnly);

				foreach (string ManifestFile in ManifestFiles)
					CheckContentFiles(ManifestFile);


				Types.SetModuleParameter("AppData", appDataFolder);
				Types.SetModuleParameter("Root", rootFolder);

				scheduler = new Scheduler();
				rnd = RandomNumberGenerator.Create();

				Task T = Task.Run(() =>
				{
					CodeContent.GraphViz.Init();
					CodeContent.PlantUml.Init();
				});

				XmlDocument Config = new XmlDocument();

				string GatewayConfigFileName = appDataFolder + "Gateway.config";
				if (!File.Exists(GatewayConfigFileName))
					GatewayConfigFileName = "Gateway.config";

				Config.Load(GatewayConfigFileName);
				XSL.Validate("Gateway.config", Config, "GatewayConfiguration", "http://waher.se/Schema/GatewayConfiguration.xsd",
					XSL.LoadSchema(typeof(Gateway).Namespace + ".Schema.GatewayConfiguration.xsd", typeof(Gateway).Assembly));

				applicationName = Config.DocumentElement["ApplicationName"].InnerText;
				defaultPage = Config.DocumentElement["DefaultPage"].InnerText;

				XmlElement ExportExceptions = Config.DocumentElement["ExportExceptions"];
				if (ExportExceptions != null)
				{
					exceptionFolder = Path.Combine(appDataFolder, XML.Attribute(ExportExceptions, "folder", "Exceptions"));

					if (!Directory.Exists(exceptionFolder))
						Directory.CreateDirectory(exceptionFolder);

					DateTime Now = DateTime.Now;

					exceptionFileName = Path.Combine(exceptionFolder, Now.Year.ToString("D4") + "-" + Now.Month.ToString("D2") + "-" + Now.Day.ToString("D2") +
						" " + Now.Hour.ToString("D2") + "." + Now.Minute.ToString("D2") + "." + Now.Second.ToString("D2") + ".txt");
					exceptionFile = File.CreateText(exceptionFileName);
					exportExceptions = true;

					exceptionFile.Write("Start of export: ");
					exceptionFile.WriteLine(DateTime.Now.ToString());

					AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
					{
						if (!exportExceptions || e.Exception.StackTrace.Contains("FirstChanceExceptionEventArgs"))
							return;

						lock (exceptionFile)
						{
							exceptionFile.WriteLine(new string('-', 80));
							exceptionFile.Write("Type: ");

							if (e.Exception != null)
								exceptionFile.WriteLine(e.Exception.GetType().FullName);
							else
								exceptionFile.WriteLine("null");

							exceptionFile.Write("Time: ");
							exceptionFile.WriteLine(DateTime.Now.ToString());

							if (e.Exception != null)
							{
								LinkedList<Exception> Exceptions = new LinkedList<Exception>();
								Exceptions.AddLast(e.Exception);

								while (Exceptions.First != null)
								{
									Exception ex = Exceptions.First.Value;
									Exceptions.RemoveFirst();

									exceptionFile.WriteLine();

									exceptionFile.WriteLine(ex.Message);
									exceptionFile.WriteLine();
									exceptionFile.WriteLine(ex.StackTrace);
									exceptionFile.WriteLine();

									if (ex is AggregateException ex2)
									{
										foreach (Exception ex3 in ex2.InnerExceptions)
											Exceptions.AddLast(ex3);
									}
									else if (ex.InnerException != null)
										Exceptions.AddLast(ex.InnerException);
								}
							}

							exceptionFile.Flush();
						}
					};
				}

				XmlElement DatabaseConfig = Config.DocumentElement["Database"];
				IDatabaseProvider DatabaseProvider;

				if (GetDatabaseProvider != null)
					DatabaseProvider = await GetDatabaseProvider(DatabaseConfig);
				else
					DatabaseProvider = null;

				if (DatabaseProvider is null)
					throw new Exception("Database provider not defined. Make sure the GetDatabaseProvider event has an appropriate event handler.");

				Database.Register(DatabaseProvider);

				PersistedEventLog PersistedEventLog = new PersistedEventLog(7, new TimeSpan(4, 15, 0));
				Log.Register(PersistedEventLog);
				try
				{
					await PersistedEventLog.Queue(new Event(EventType.Informational, "Server starting up.", string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty));
				}
				catch (Exception ex)
				{
					Event Event = new Event(DateTime.Now, EventType.Critical, ex.Message, PersistedEventLog.ObjectID, string.Empty, string.Empty,
						EventLevel.Major, string.Empty, ex.Source, ex.StackTrace);

					Event.Avoid(PersistedEventLog);

					Log.Event(Event);
				}

				Dictionary<string, Type> SystemConfigurationTypes = new Dictionary<string, Type>();
				Dictionary<string, SystemConfiguration> SystemConfigurations = new Dictionary<string, SystemConfiguration>();
				bool Configured = true;

				foreach (Type SystemConfigurationType in Types.GetTypesImplementingInterface(typeof(ISystemConfiguration)))
				{
					if (SystemConfigurationType.IsAbstract)
						continue;

					SystemConfigurationTypes[SystemConfigurationType.FullName] = SystemConfigurationType;
				}

				foreach (SystemConfiguration SystemConfiguration in await Database.Find<SystemConfiguration>())
				{
					string s = SystemConfiguration.GetType().FullName;

					if (SystemConfigurations.ContainsKey(s))
						await Database.Delete(SystemConfiguration);
					else
					{
						SystemConfigurations[s] = SystemConfiguration;
						SystemConfigurationTypes.Remove(s);

						if (!SystemConfiguration.Complete)
							Configured = false;
					}
				}

				foreach (KeyValuePair<string, Type> P in SystemConfigurationTypes)
				{
					try
					{
						SystemConfiguration SystemConfiguration = (SystemConfiguration)Activator.CreateInstance(P.Value);
						SystemConfiguration.Complete = false;
						SystemConfiguration.Created = DateTime.Now;

						await Database.Insert(SystemConfiguration);

						SystemConfigurations[P.Key] = SystemConfiguration;
						Configured = false;
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						continue;
					}
				}

				SystemConfiguration[] Configurations = new SystemConfiguration[SystemConfigurations.Count];
				SystemConfigurations.Values.CopyTo(Configurations, 0);
				Array.Sort<SystemConfiguration>(Configurations, (c1, c2) => c1.Priority - c2.Priority);

				HttpFolderResource HttpFolderResource;
				ISystemConfiguration CurrentConfiguration = null;
				LinkedList<HttpResource> SetupResources = null;
				StringBuilder sb;

				if (!Configured)
				{
					configuring = true;

					if (loopbackIntefaceAvailable)
						Log.Notice("System needs to be configured. This is done by navigating to the loopback interface using a browser on this machine.");
					else
						Log.Notice("System needs to be configured. This is done by navigating to the machine using a browser on another machine in the same network.");

					webServer = new HttpServer(new int[] { HttpServer.DefaultHttpPort, 8080, 8081, 8082 }, null, null)
					{
						ResourceOverrideFilter = "(?<!Login)[.]md(\\?[.]*)?$"
					};

					loggedIn = new LoggedIn(webServer);

					SetupResources = new LinkedList<HttpResource>();

					SetupResources.AddLast(webServer.Register(new HttpFolderResource("/Graphics", Path.Combine(appDataFolder, "Graphics"), false, false, true, false))); // TODO: Add authentication mechanisms for PUT & DELETE.
					SetupResources.AddLast(webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false)));   // Syntax highlighting library, provided by http://highlightjs.org
					SetupResources.AddLast(webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true)));    // TODO: Add authentication mechanisms for PUT & DELETE.
					SetupResources.AddLast(webServer.Register("/", (req, resp) => throw new TemporaryRedirectException(defaultPage)));
					SetupResources.AddLast(webServer.Register(clientEvents = new ClientEvents()));
					SetupResources.AddLast(webServer.Register(clientEventsWs = new ClientEventsWebSocket()));
					SetupResources.AddLast(webServer.Register(login = new Login()));
					SetupResources.AddLast(webServer.Register(logout = new Logout()));
					SetupResources.AddLast(webServer.Register(exportFolder = new HttpFolderResource("/Export", Export.FullExportFolder, false, false, false, true, loggedIn)));
					SetupResources.AddLast(webServer.Register(keyFolder = new HttpFolderResource("/Key", Export.FullKeyExportFolder, false, false, false, true, loggedIn)));
					SetupResources.AddLast(webServer.Register(startExport = new StartExport()));
					SetupResources.AddLast(webServer.Register(startAnalyze = new StartAnalyze()));
					SetupResources.AddLast(webServer.Register(deleteExport = new DeleteExport()));

					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					HttpFolderResource.AllowTypeConversion();
					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
					MarkdownToHtmlConverter.RootFolder = rootFolder;
				}

				foreach (SystemConfiguration Configuration in Configurations)
				{
					Configuration.SetStaticInstance(Configuration);

					if (webServer != null)
						await Configuration.InitSetup(webServer);
				}

				bool ReloadConfigurations;

				do
				{
					ReloadConfigurations = false;

					foreach (SystemConfiguration Configuration in Configurations)
					{
						bool NeedsCleanup = false;

						if (!Configuration.Complete)
						{
							CurrentConfiguration = Configuration;
							webServer.ResourceOverride = Configuration.Resource;
							Configuration.SetStaticInstance(Configuration);

							if (StartingServer != null)
							{
								StartingServer.Release();
								StartingServer.Dispose();
								StartingServer = null;
							}

							ClientEvents.PushEvent(ClientEvents.GetTabIDs(), "Reload", string.Empty);

							if (await Configuration.SetupConfiguration(webServer))
								ReloadConfigurations = true;

							NeedsCleanup = true;
						}

						await Configuration.ConfigureSystem();

						if (NeedsCleanup)
							await Configuration.CleanupAfterConfiguration(webServer);

						if (ReloadConfigurations)
						{
							Configured = true;

							foreach (SystemConfiguration SystemConfiguration in await Database.Find<SystemConfiguration>())
							{
								string s = SystemConfiguration.GetType().FullName;

								if (webServer != null && SystemConfigurations.TryGetValue(s, out SystemConfiguration OldConfiguration))
									await OldConfiguration.UnregisterSetup(webServer);

								SystemConfigurations[s] = SystemConfiguration;
								SystemConfiguration.SetStaticInstance(SystemConfiguration);

								if (webServer != null)
									await SystemConfiguration.InitSetup(webServer);
							}

							foreach (SystemConfiguration SystemConfiguration in SystemConfigurations.Values)
							{
								if (!SystemConfiguration.Complete)
								{
									Configured = false;
									break;
								}
							}

							Configurations = new SystemConfiguration[SystemConfigurations.Count];
							SystemConfigurations.Values.CopyTo(Configurations, 0);
							Array.Sort<SystemConfiguration>(Configurations, (c1, c2) => c1.Priority - c2.Priority);

							break;
						}
					}
				}
				while (ReloadConfigurations);

				configuring = false;

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

				if (webServer != null)
				{
					webServer.ResourceOverride = null;
					webServer.ResourceOverrideFilter = null;

					if (SetupResources != null)
					{
						foreach (HttpResource Resource in SetupResources)
							webServer.Unregister(Resource);
					}

					webServer.AddHttpPorts(GetConfigPorts("HTTP"));

					if (certificate != null)
					{
						webServer.AddHttpsPorts(GetConfigPorts("HTTPS"));
						webServer.UpdateCertificate(certificate);
					}
				}
				else
				{
					if (certificate != null)
						webServer = new HttpServer(GetConfigPorts("HTTP"), GetConfigPorts("HTTPS"), certificate);
					else
						webServer = new HttpServer(GetConfigPorts("HTTP"), null, null);

					foreach (SystemConfiguration Configuration in Configurations)
						await Configuration.InitSetup(webServer);
				}

				Types.SetModuleParameter("HTTP", webServer);

				loggedIn = new LoggedIn(webServer);

				sb = new StringBuilder();

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

				webServer.Register(new HttpFolderResource("/Graphics", Path.Combine(appDataFolder, "Graphics"), false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
				webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(httpxProxy = new HttpxProxy("/HttpxProxy", xmppClient, MaxChunkSize));
				webServer.Register("/", (req, resp) => throw new TemporaryRedirectException(defaultPage));
				webServer.Register(clientEvents = new ClientEvents());
				webServer.Register(clientEventsWs = new ClientEventsWebSocket());
				webServer.Register(login = new Login());
				webServer.Register(logout = new Logout());
				webServer.Register(exportFolder = new HttpFolderResource("/Export", Export.FullExportFolder, false, false, false, true, loggedIn));
				webServer.Register(keyFolder = new HttpFolderResource("/Key", Export.FullKeyExportFolder, false, false, false, true, loggedIn));
				webServer.Register(startExport = new StartExport());
				webServer.Register(startAnalyze = new StartAnalyze());
				webServer.Register(deleteExport = new DeleteExport());

				if (emoji1_24x24 is null)
				{
					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
					MarkdownToHtmlConverter.RootFolder = rootFolder;
				}

				HttpFolderResource.AllowTypeConversion();

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
				Types.SetModuleParameter("HTTPX", httpxProxy);
				Types.SetModuleParameter("HTTPXS", httpxServer);

				httpxProxy.IbbClient = ibbClient;
				httpxServer.IbbClient = ibbClient;

				httpxProxy.Socks5Proxy = socksProxy;
				httpxServer.Socks5Proxy = socksProxy;

				if (xmppCredentials.Sniffer)
				{
					ISniffer Sniffer;

					Sniffer = new XmlFileSniffer(appDataFolder + "HTTP" + Path.DirectorySeparatorChar +
						"HTTP Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
						appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "SnifferXmlToHtml.xslt",
						7, BinaryPresentationMethod.ByteCount);
					webServer.Add(Sniffer);
				}

				ClientEvents.PushEvent(ClientEvents.GetTabIDs(), "Reload", string.Empty);

				coapEndpoint = new CoapEndpoint();
				Types.SetModuleParameter("CoAP", coapEndpoint);

				IDataSource[] Sources = new IDataSource[] { new MeteringTopology() };

				if (GetDataSources != null)
					Sources = await GetDataSources(Sources);

				concentratorServer = new ConcentratorServer(xmppClient, thingRegistryClient, provisioningClient, Sources);
				avatarClient = new Networking.XMPP.Avatar.AvatarClient(xmppClient, pepClient);

				Types.SetModuleParameter("Concentrator", concentratorServer);
				Types.SetModuleParameter("Sensor", concentratorServer.SensorServer);
				Types.SetModuleParameter("Control", concentratorServer.ControlServer);
				Types.SetModuleParameter("Registry", thingRegistryClient);
				Types.SetModuleParameter("Provisioning", provisioningClient);
				Types.SetModuleParameter("Avatar", avatarClient);
				Types.SetModuleParameter("Scheduler", scheduler);

				MeteringTopology.OnNewMomentaryValues += NewMomentaryValues;

				DeleteOldDataSourceEvents(null);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				if (gatewayRunning != null)
				{
					gatewayRunning.Release();
					gatewayRunning.Dispose();
					gatewayRunning = null;
				}

				if (StartingServer != null)
				{
					StartingServer.Release();
					StartingServer.Dispose();
					StartingServer = null;
				}

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			Task T2 = Task.Run(async () =>
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
						if (StartingServer != null)
						{
							StartingServer.Release();
							StartingServer.Dispose();
							StartingServer = null;
						}
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

		private static void CheckContentFiles(string ManifestFileName)
		{
			try
			{
				XmlDocument Doc = new XmlDocument();
				Doc.Load(ManifestFileName);

				if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "Module" && Doc.DocumentElement.NamespaceURI == "http://waher.se/Schema/ModuleManifest.xsd")
					CheckContentFiles(Doc.DocumentElement, runtimeFolder, runtimeFolder, appDataFolder);
			}
			catch (Exception ex)
			{
				Log.Critical(ex, ManifestFileName);
			}
		}

		private static void CheckContentFiles(XmlElement Element, string RuntimeFolder, string RuntimeSubfolder, string AppDataSubFolder)
		{
			bool AppDataFolderChecked = false;

			foreach (XmlNode N in Element.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "Folder":
							string Name = XML.Attribute(E, "name");
							CheckContentFiles(E, RuntimeFolder, Path.Combine(RuntimeSubfolder, Name), Path.Combine(AppDataSubFolder, Name));
							break;

						case "Content":
							Name = XML.Attribute(E, "fileName");

							string s = Path.Combine(RuntimeSubfolder, Name);
							if (!File.Exists(s))
							{
								s = Path.Combine(RuntimeFolder, Name);
								if (!File.Exists(s))
									break;
							}

							if (!AppDataFolderChecked)
							{
								AppDataFolderChecked = true;

								if (!Directory.Exists(AppDataSubFolder))
									Directory.CreateDirectory(AppDataSubFolder);
							}

							string s2 = Path.Combine(AppDataSubFolder, Name);

							if (!File.Exists(s2))
								File.Copy(s, s2);
							else
							{
								DateTime TP = File.GetLastWriteTime(s);
								DateTime TP2 = File.GetLastWriteTime(s2);

								if (TP > TP2)
									File.Copy(s, s2, true);
							}
							break;
					}
				}
			}
		}

		internal static bool ConsoleOutput => consoleOutput;

		internal static Task ConfigureXmpp(XmppConfiguration Configuration)
		{
			xmppCredentials = Configuration.GetCredentials();
			xmppClient = new XmppClient(xmppCredentials, "en", typeof(Gateway).Assembly);
			xmppClient.OnValidateSender += XmppClient_OnValidateSender;
			Types.SetModuleParameter("XMPP", xmppClient);

			if (xmppCredentials.Sniffer)
			{
				ISniffer Sniffer;

				if (consoleOutput)
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

			if (!string.IsNullOrEmpty(xmppCredentials.Events))
				Log.Register(new XmppEventSink("XMPP Event Sink", xmppClient, xmppCredentials.Events, false));

			if (!string.IsNullOrEmpty(xmppCredentials.ThingRegistry))
			{
				thingRegistryClient = new ThingRegistryClient(xmppClient, xmppCredentials.ThingRegistry);
				thingRegistryClient.Claimed += ThingRegistryClient_Claimed;
				thingRegistryClient.Disowned += ThingRegistryClient_Disowned;
				thingRegistryClient.Removed += ThingRegistryClient_Removed;
			}

			if (!string.IsNullOrEmpty(xmppCredentials.Provisioning))
				provisioningClient = new ProvisioningClient(xmppClient, xmppCredentials.Provisioning);
			else
			{
				provisioningClient = null;
				xmppClient.OnPresenceSubscribe += XmppClient_OnPresenceSubscribe;
				xmppClient.OnPresenceUnsubscribe += XmppClient_OnPresenceUnsubscribe;
			}

			scheduler.Add(DateTime.Now.AddMinutes(1), CheckConnection, null);

			xmppClient.OnStateChanged += XmppClient_OnStateChanged;
			xmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;

			ibbClient = new Networking.XMPP.InBandBytestreams.IbbClient(xmppClient, MaxChunkSize);
			Types.SetModuleParameter("IBB", ibbClient);

			socksProxy = new Networking.XMPP.P2P.SOCKS5.Socks5Proxy(xmppClient);
			Types.SetModuleParameter("SOCKS5", socksProxy);

			synchronizationClient = new SynchronizationClient(xmppClient);
			pepClient = new PepClient(xmppClient, XmppConfiguration.Instance.PubSub);

			if (!string.IsNullOrEmpty(XmppConfiguration.Instance.LegalIdentities))
				contractsClient = new ContractsClient(xmppClient, XmppConfiguration.Instance.LegalIdentities);
			else
				contractsClient = null;

			if (XmppConfiguration.Instance.Mail)
			{
				mailClient = new MailClient(xmppClient);
				mailClient.MailReceived += MailClient_MailReceived;
			}
			else
				mailClient = null;

			return Task.CompletedTask;
		}

		internal static Task ConfigureDomain(DomainConfiguration Configuration)
		{
			domain = Configuration.Domain;

			if (Configuration.UseEncryption && Configuration.Certificate != null && Configuration.PrivateKey != null)
			{
				UpdateCertificate(Configuration);
				scheduler.Add(DateTime.Now.AddHours(0.5 + NextDouble()), CheckCertificate, Configuration);
			}
			else
				certificate = null;

			return Task.CompletedTask;
		}

		internal static bool UpdateCertificate(DomainConfiguration Configuration)
		{
			try
			{
				if (Configuration.PFX != null)
					certificate = new X509Certificate2(Configuration.PFX, Configuration.Password);
				else
				{
					RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
					RSA.ImportCspBlob(Configuration.PrivateKey);

					certificate = new X509Certificate2(Configuration.Certificate)
					{
						PrivateKey = RSA
					};
				}

				try
				{
					webServer?.UpdateCertificate(Certificate);
					OnNewCertificate?.Invoke(certificate, new EventArgs());
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return false;
		}

		private static async void CheckCertificate(object P)
		{
			DomainConfiguration Configuration = (DomainConfiguration)P;
			DateTime Now = DateTime.Now;

			try
			{
				if (Now.AddDays(50) >= certificate.NotAfter)
				{
					Log.Notice("Updating certificate");

					if (await Configuration.CreateCertificate())
					{
						Log.Notice("Certificate created.");
						if (!UpdateCertificate(Configuration))
							Log.Error("Unable to update gatetway with new certificate.");
					}
					else
					{
						int DaysLeft = (int)Math.Round((certificate.NotAfter - Now.Date).TotalDays);

						if (DaysLeft < 2)
							Log.Emergency("Unable to generate new certificate.", domain);
						else if (DaysLeft < 5)
							Log.Alert("Unable to generate new certificate.", domain);
						else if (DaysLeft < 10)
							Log.Critical("Unable to generate new certificate.", domain);
						else if (DaysLeft < 20)
							Log.Error("Unable to generate new certificate.", domain);
						else
							Log.Warning("Unable to generate new certificate.", domain);
					}
				}
			}
			catch (Exception ex)
			{
				int DaysLeft = (int)Math.Round((certificate.NotAfter - Now.Date).TotalDays);

				if (DaysLeft < 2)
					Log.Emergency(ex, domain);
				else if (DaysLeft < 5)
					Log.Alert(ex, domain);
				else
					Log.Critical(ex);
			}
			finally
			{
				scheduler.Add(DateTime.Now.AddDays(0.5 + NextDouble()), CheckCertificate, Configuration);
			}
		}

		/// <summary>
		/// Event raised when a new server certificate has been generated.
		/// </summary>
		public static EventHandler OnNewCertificate = null;

		private static async void DeleteOldDataSourceEvents(object P)
		{
			try
			{
				await MeteringTopology.DeleteOldEvents(TimeSpan.FromDays(7));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				ScheduleEvent(DeleteOldDataSourceEvents, DateTime.Today.AddDays(1).AddHours(4), null);
			}
		}

		/// <summary>
		/// Event raised when the Gateway requires its database provider from the host.
		/// </summary>
		public static event GetDatabaseProviderEventHandler GetDatabaseProvider = null;

		/// <summary>
		/// Event raised when the Gateway requires a set of data sources to publish.
		/// </summary>
		public static event GetDataSources GetDataSources = null;

		/// <summary>
		/// Initializes the inventory engine by loading available assemblies in the installation folder (top directory only).
		/// </summary>
		private static void Initialize()
		{
			string Folder = Assembly.GetExecutingAssembly().Location;
			if (string.IsNullOrEmpty(Folder))
				Folder = AppDomain.CurrentDomain.BaseDirectory;

			runtimeFolder = Path.GetDirectoryName(Folder);

			Directory.SetCurrentDirectory(runtimeFolder);

			TypesLoader.Initialize(runtimeFolder, (FileName) =>
			{
				FileName = Path.GetFileName(FileName).ToLower();
				if (FileName.StartsWith("api-ms-win-") ||
					FileName.StartsWith("system.") ||
					FileName.StartsWith("microsoft.") ||
					FileName.StartsWith("windows.") ||
					FileName.StartsWith("waher.client.") ||
					FileName.StartsWith("waher.utility.") ||
					FileName.StartsWith("mscordaccore_x86_x86_") ||
					FileName.StartsWith("sos_x86_x86_"))
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
					case "mscordbi.dll":
					case "mscorlib.dll":
					case "mscorrc.debug.dll":
					case "mscorrc.dll":
					case "netstandard.dll":
					case "sos.dll":
					case "sos.netcore.dll":
					case "ucrtbase.dll":
					case "windowsbase.dll":
					case "waher.persistence.fileslw.dll":
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
			if (Directory.Exists(From))
			{
				CopyFolder(From, To, "*.*", OnlyIfNewer);

				string[] Folders = Directory.GetDirectories(From, "*.*", SearchOption.TopDirectoryOnly);

				foreach (string Folder in Folders)
				{
					string FolderName = Path.GetFileName(Folder);
					CopyFolders(Folder, Path.Combine(To, FolderName), OnlyIfNewer);
				}
			}
		}

		/// <summary>
		/// Stops the gateway.
		/// </summary>
		public static void Stop()
		{
			IDisposable Disposable;

			Log.Informational("Server shutting down.");

			stopped = true;

			scheduler?.Dispose();
			scheduler = null;

			gatewayRunning?.Release();
			gatewayRunning?.Dispose();
			gatewayRunning = null;

			ibbClient?.Dispose();
			ibbClient = null;

			httpxProxy?.Dispose();
			httpxProxy = null;

			httpxServer?.Dispose();
			httpxServer = null;

			provisioningClient?.Dispose();
			provisioningClient = null;

			thingRegistryClient?.Dispose();
			thingRegistryClient = null;

			concentratorServer?.Dispose();
			concentratorServer = null;

			avatarClient?.Dispose();
			avatarClient = null;

			synchronizationClient?.Dispose();
			synchronizationClient = null;

			pepClient?.Dispose();
			pepClient = null;

			mailClient?.Dispose();
			mailClient = null;

			if (xmppClient != null)
			{
				using (ManualResetEvent OfflineSent = new ManualResetEvent(false))
				{
					xmppClient.SetPresence(Availability.Offline, (sender, e) => OfflineSent.Set());
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

			coapEndpoint?.Dispose();
			coapEndpoint = null;

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
			login = null;
			logout = null;
			exportFolder = null;
			keyFolder = null;
			startExport = null;
			startAnalyze = null;
			deleteExport = null;

			if (exportExceptions)
			{
				exportExceptions = false;

				lock (exceptionFile)
				{
					exceptionFile.WriteLine(new string('-', 80));
					exceptionFile.Write("End of export: ");
					exceptionFile.WriteLine(DateTime.Now.ToString());

					exceptionFile.Flush();
					exceptionFile.Close();
				}
			}
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
		public static CaseInsensitiveString Domain
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
		/// Default page of gateway.
		/// </summary>
		public static string DefaultPage
		{
			get { return defaultPage; }
			internal set { defaultPage = value; }
		}

		/// <summary>
		/// Application Name.
		/// </summary>
		public static string ApplicationName
		{
			get { return applicationName; }
			internal set { applicationName = value; }
		}

		/// <summary>
		/// Emojis.
		/// </summary>
		public static Emoji1LocalFiles Emoji1_24x24
		{
			get { return emoji1_24x24; }
		}

		/// <summary>
		/// If the gateway is being configured.
		/// </summary>
		public static bool Configuring
		{
			get { return configuring; }
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

			if (string.IsNullOrEmpty(BareJid) ||
				(xmppClient != null && (BareJid == xmppClient.Domain.ToLower() || BareJid == xmppClient.BareJID.ToLower())))
			{
				e.Accept();
			}
			else if (BareJid.IndexOf('@') > 0 &&
				(xmppClient is null ||
				(Item = xmppClient.GetRosterItem(BareJid)) is null ||
				Item.State == SubscriptionState.None ||
				Item.State == SubscriptionState.Remove ||
				Item.State == SubscriptionState.Unknown))
			{
				foreach (XmlNode N in e.Stanza.ChildNodes)
				{
					if (N.LocalName == "query" && N.NamespaceURI == XmppClient.NamespaceServiceDiscoveryInfo)
						return;
				}

				e.Reject();
			}
		}

		private static async void CheckConnection(object State)
		{
			try
			{
				if (!stopped)
				{
					scheduler.Add(DateTime.Now.AddMinutes(1), CheckConnection, null);

					XmppState? State2 = xmppClient?.State;
					if (State2.HasValue && (State2 == XmppState.Offline || State2 == XmppState.Error || State2 == XmppState.Authenticating))
					{
						try
						{
							xmppClient?.Reconnect();
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}

					await CheckBackup();

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
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
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
						Task.Run(Register);

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
			e.Accept();

			RosterItem Item = xmppClient.GetRosterItem(e.FromBareJID);
			if (Item is null || Item.State == SubscriptionState.None || Item.State == SubscriptionState.From)
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
			if (xmppClient is null || xmppClient.UserName != UserName)
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

				if (XmppConfiguration.Instance.StorePasswordInsteadOfHash)
				{
					PasswordHash = Password;
					PasswordHashMethod = string.Empty;
				}
				else
				{
					PasswordHash = Client.PasswordHash;
					PasswordHashMethod = Client.PasswordHashMethod;
				}
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
						UserName == xmppCredentials.Account && Password == xmppCredentials.Password &&
						string.IsNullOrEmpty(xmppCredentials.PasswordType))
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

				xmppCredentials.Account = UserName;
				xmppCredentials.Password = PasswordHash;
				xmppCredentials.PasswordType = PasswordHashMethod;
				xmppCredentials.AllowRegistration = false;
				xmppCredentials.FormSignatureKey = string.Empty;
				xmppCredentials.FormSignatureSecret = string.Empty;

				XmppConfiguration.Instance.Account = UserName;
				XmppConfiguration.Instance.Password = PasswordHash;
				XmppConfiguration.Instance.PasswordType = PasswordHashMethod;

				XmppConfiguration.Instance.Updated = DateTime.Now;
				await Database.Update(XmppConfiguration.Instance);
			}

			return LoginResult.Successful;
		}

		/// <summary>
		/// Checks if a web request comes from the local host in the current session. If so, the user is automatically logged in.
		/// </summary>
		/// <param name="Request">Web request</param>
		public static void CheckLocalLogin(HttpRequest Request)
		{
			string RemoteEndpoint = Request.RemoteEndPoint;
			string From;
			int Port;
			int i;

			if (Request.Session is null)
				return;

			if (!Request.Session.TryGetVariable("from", out Variable v) || string.IsNullOrEmpty(From = v.ValueObject as string))
				From = string.Empty;

			if (!loopbackIntefaceAvailable && (XmppConfiguration.Instance is null || !XmppConfiguration.Instance.Complete || configuring))
			{
				Log.Informational("User logged in by default, since XMPP not configued and loopback interface not available.", string.Empty, Request.RemoteEndPoint, "LoginSuccessful", EventLevel.Minor);
				Login.DoLogin(Request, From);
				return;
			}

			if (RemoteEndpoint.StartsWith("[::1]:"))
			{
				if (!int.TryParse(RemoteEndpoint.Substring(6), out Port))
					return;
			}
			else if (RemoteEndpoint.StartsWith("127."))
			{
				i = RemoteEndpoint.IndexOf(':');
				if (i < 0 || !int.TryParse(RemoteEndpoint.Substring(i + 1), out Port))
					return;
			}
			else
				return;

#if !MONO
			if (XmppConfiguration.Instance is null || !XmppConfiguration.Instance.Complete || configuring)
#endif
			{
				Log.Informational("Local user logged in.", string.Empty, Request.RemoteEndPoint, "LoginSuccessful", EventLevel.Minor);
				Login.DoLogin(Request, From);
				return;
			}

#if !MONO
			try
			{
				using (Process Proc = new Process())
				{
					ProcessStartInfo StartInfo = new ProcessStartInfo()
					{
						FileName = "netstat.exe",
						Arguments = "-a -n -o",
						WindowStyle = ProcessWindowStyle.Hidden,
						UseShellExecute = false,
						RedirectStandardInput = true,
						RedirectStandardOutput = true,
						RedirectStandardError = true
					};

					Proc.StartInfo = StartInfo;
					Proc.Start();

					string Output = Proc.StandardOutput.ReadToEnd();
					if (Proc.ExitCode != 0)
						return;

					string[] Rows = Output.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);

					foreach (string Row in Rows)
					{
						string[] Tokens = Regex.Split(Row, @"\s+");

						if (Tokens.Length < 6)
							continue;

						if (Tokens[1] != "TCP")
							continue;

						if (Tokens[2] != RemoteEndpoint)
							continue;

						if (Tokens[4] != "ESTABLISHED")
							continue;

						if (!int.TryParse(Tokens[5], out int PID))
							continue;

						Process P = Process.GetProcessById(PID);
						int CurrentSession = WTSGetActiveConsoleSessionId();

						if (P.SessionId == CurrentSession)
						{
							Log.Informational("Local user logged in.", string.Empty, Request.RemoteEndPoint, "LoginSuccessful", EventLevel.Minor);
							Login.DoLogin(Request, From);
							break;
						}
					}
				}
			}
			catch (HttpException)
			{
				throw;
			}
			catch (Exception)
			{
				return;
			}
#endif
		}

#if !MONO
		[DllImport("kernel32.dll")]
		static extern int WTSGetActiveConsoleSessionId();
#endif

		/// <summary>
		/// Authentication mechanism that makes sure the call is made from a session with a valid authenticated user.
		/// </summary>
		public static LoggedIn LoggedIn
		{
			get { return loggedIn; }
		}

		/// <summary>
		/// Makes sure a request is being made from a session with a successful user login.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		public static void AssertUserAuthenticated(HttpRequest Request)
		{
			IUser User;

			if (Request.Session is null || !Request.Session.TryGetVariable("User", out Variable v) || (User = v.ValueObject as IUser) is null)
				throw new ForbiddenException();
		}

		#endregion

		#region Thing Registry

		private static void ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			if (e.Node.IsEmpty)
			{
				ownerJid = e.JID;
				Log.Informational("Gateway has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
			}
		}

		private static void ThingRegistryClient_Disowned(object Sender, Networking.XMPP.Provisioning.NodeEventArgs e)
		{
			if (e.Node.IsEmpty)
			{
				Log.Informational("Gateway has been disowned.", ownerJid);
				ownerJid = string.Empty;
				Task.Run(Register);
			}
		}

		private static void ThingRegistryClient_Removed(object Sender, Networking.XMPP.Provisioning.NodeEventArgs e)
		{
			if (e.Node.IsEmpty)
				Log.Informational("Gateway has been removed from the public registry.", ownerJid);
		}

		private static async Task Register()
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
				new MetaDataNumericTag("V", 1.0)
			};

			if (GetMetaData != null)
				MetaData = await GetMetaData(MetaData);

			thingRegistryClient.RegisterThing(MetaData, (sender2, e2) =>
			{
				if (e2.Ok)
				{
					registered = true;

					if (e2.IsClaimed)
						ownerJid = e2.OwnerJid;
					else
						ownerJid = string.Empty;

					RegistrationSuccessful?.Invoke(MetaData, e2);
				}
			}, null);
		}

		/// <summary>
		/// Event raised when meta data is compiled for registration.
		/// </summary>
		public static event GetRegistryMetaDataEventHandler GetMetaData = null;

		/// <summary>
		/// Event raised when the gateway has performed a successful registration in a thing registry.
		/// </summary>
		public static event RegistrationEventHandler RegistrationSuccessful = null;

		/// <summary>
		/// XMPP Client connection of gateway.
		/// </summary>
		public static XmppClient XmppClient
		{
			get { return xmppClient; }
		}

		/// <summary>
		/// XMPP Thing Registry Client.
		/// </summary>
		public static ThingRegistryClient ThingRegistryClient
		{
			get { return thingRegistryClient; }
		}

		/// <summary>
		/// XMPP Provisioning Client.
		/// </summary>
		public static ProvisioningClient ProvisioningClient
		{
			get { return provisioningClient; }
		}

		/// <summary>
		/// XMPP Concentrator Server.
		/// </summary>
		public static ConcentratorServer ConcentratorServer
		{
			get { return concentratorServer; }
		}

		/// <summary>
		/// XMPP Concentrator Server.
		/// </summary>
		public static Networking.XMPP.Avatar.AvatarClient AvatarClient
		{
			get { return avatarClient; }
		}

		/// <summary>
		/// XMPP Synchronization Client.
		/// </summary>
		public static SynchronizationClient SynchronizationClient
		{
			get { return synchronizationClient; }
		}

		/// <summary>
		/// XMPP Personal Eventing Protocol (PEP) Client.
		/// </summary>
		public static PepClient PepClient
		{
			get { return pepClient; }
		}

		/// <summary>
		/// XMPP Publish/Subscribe (PubSub) Client, if such a component is available on the XMPP broker.
		/// </summary>
		public static PubSubClient PubSubClient
		{
			get { return pepClient.PubSubClient; }
		}

		/// <summary>
		/// XMPP Contracts Client, if such a compoent is available on the XMPP broker.
		/// </summary>
		public static ContractsClient ContractsClient
		{
			get { return contractsClient; }
		}

		/// <summary>
		/// XMPP Mail Client, if support for mail-extensions is available on the XMPP broker.
		/// </summary>
		public static MailClient MailClient
		{
			get { return mailClient; }
		}

		/// <summary>
		/// HTTP Server
		/// </summary>
		public static HttpServer HttpServer
		{
			get { return webServer; }
		}

		/// <summary>
		/// HTTPX Server
		/// </summary>
		public static HttpxServer HttpxServer
		{
			get { return httpxServer; }
		}

		/// <summary>
		/// HTTPX Proxy resource
		/// </summary>
		public static HttpxProxy HttpxProxy
		{
			get { return httpxProxy; }
		}

		/// <summary>
		/// CoAP Endpoint
		/// </summary>
		public static CoapEndpoint CoapEndpoint
		{
			get { return coapEndpoint; }
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

			if (h is null)
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
			return scheduler?.Add(When, Callback, State) ?? DateTime.MinValue;
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

		/// <summary>
		/// Generates an array of random bytes.
		/// </summary>
		/// <param name="NrBytes">Number of random bytes to generate.</param>
		/// <returns>Random bytes.</returns>
		public static byte[] NextBytes(int NrBytes)
		{
			if (NrBytes < 0)
				throw new ArgumentException("Number of bytes must be non-negative.", nameof(NrBytes));

			byte[] Result = new byte[NrBytes];

			lock (rnd)
			{
				rnd.GetBytes(Result);
			}

			return Result;
		}

		#endregion

		#region Momentary values

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Values">New momentary values.</param>
		public static void NewMomentaryValues(params Field[] Values)
		{
			concentratorServer?.SensorServer?.NewMomentaryValues(Values);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public static void NewMomentaryValues(IThingReference Reference, params Field[] Values)
		{
			concentratorServer?.SensorServer?.NewMomentaryValues(Reference, Values);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Values">New momentary values.</param>
		public static void NewMomentaryValues(IEnumerable<Field> Values)
		{
			concentratorServer?.SensorServer?.NewMomentaryValues(Values);
		}

		/// <summary>
		/// Reports newly measured values.
		/// </summary>
		/// <param name="Reference">Optional node reference</param>
		/// <param name="Values">New momentary values.</param>
		public static void NewMomentaryValues(IThingReference Reference, IEnumerable<Field> Values)
		{
			concentratorServer?.SensorServer?.NewMomentaryValues(Reference, Values);
		}

		#endregion

		#region Personal Eventing Protocol

		/// <summary>
		/// Publishes a personal event on the XMPP network.
		/// </summary>
		/// <param name="PersonalEvent">Personal event to publish.</param>
		public static void PublishPersonalEvent(IPersonalEvent PersonalEvent)
		{
			pepClient?.Publish(PersonalEvent, null, null);
		}

		/// <summary>
		/// Registers an event handler of a specific type of personal events.
		/// </summary>
		/// <param name="PersonalEventType">Type of personal event.</param>
		/// <param name="Handler">Event handler.</param>
		public static void RegisterHandler(Type PersonalEventType, PersonalEventNotificationEventHandler Handler)
		{
			pepClient?.RegisterHandler(PersonalEventType, Handler);
		}

		/// <summary>
		/// Unregisters an event handler of a specific type of personal events.
		/// </summary>
		/// <param name="PersonalEventType">Type of personal event.</param>
		/// <param name="Handler">Event handler.</param>
		/// <returns>If the event handler was found and removed.</returns>
		public static bool UnregisterHandler(Type PersonalEventType, PersonalEventNotificationEventHandler Handler)
		{
			if (pepClient is null)
				return false;
			else
				return pepClient.UnregisterHandler(PersonalEventType, Handler);
		}

		/// <summary>
		/// Event raisen when an item notification has been received, that is not a personal event, but received from the
		/// Publish/Subscribe component.
		/// </summary>
		public static event ItemNotificationEventHandler PubSubItemNotification
		{
			add => pepClient.NonPepItemNotification += value;
			remove => pepClient.NonPepItemNotification -= value;
		}

		#endregion

		#region Export

		private static async Task<bool> CheckBackup()
		{
			bool Result = false;

			try
			{
				if (await Export.GetAutomaticBackupsAsync())
				{
					DateTime Now = DateTime.Now;
					TimeSpan Timepoint = await Export.GetBackupTimeAsync();
					DateTime EstimatedTime = Now.Date + Timepoint;
					DateTime LastBackup = await Export.GetLastBackupAsync();

					if ((Timepoint.Hours == Now.Hour && Timepoint.Minutes == Now.Minute) ||
						(lastBackupTimeCheck.HasValue && lastBackupTimeCheck.Value < EstimatedTime && Now >= EstimatedTime) ||
						LastBackup.AddDays(1) < EstimatedTime)
					{
						lastBackupTimeCheck = Now;
						await Export.SetLastBackupAsync(Now);

						KeyValuePair<string, IExportFormat> Exporter = StartExport.GetExporter("Encrypted");
						string FileName = Exporter.Key;

						ExportFormat.UpdateClientsFileUpdated(FileName, -1, Now);

						List<string> Folders = new List<string>();

						foreach (Export.FolderCategory FolderCategory in Export.GetRegisteredFolders())
							Folders.AddRange(FolderCategory.Folders);

						await StartExport.DoExport(Exporter.Value, true, true, Folders.ToArray());

						long KeepDays = await Export.GetKeepDaysAsync();
						long KeepMonths = await Export.GetKeepMonthsAsync();
						long KeepYears = await Export.GetKeepYearsAsync();
						string ExportFolder = Export.FullExportFolder;
						string KeyFolder = Export.FullKeyExportFolder;

						DeleteOldFiles(ExportFolder, KeepDays, KeepMonths, KeepYears, Now);
						if (ExportFolder != KeyFolder)
							DeleteOldFiles(KeyFolder, KeepDays, KeepMonths, KeepYears, Now);

						Result = true;

						OnAfterBackup?.Invoke(typeof(Gateway), new EventArgs());
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Result;
		}

		private static DateTime? lastBackupTimeCheck = null;

		/// <summary>
		/// Event raised after a backup has been performed. This event can be used by services to purge the system of old data, without
		/// affecting any ongoing backup.
		/// </summary>
		public static event EventHandler OnAfterBackup = null;

		private static void DeleteOldFiles(string Path, long KeepDays, long KeepMonths, long KeepYears, DateTime Now)
		{
			try
			{
				string[] Files = Directory.GetFiles(Path, "*.*", SearchOption.TopDirectoryOnly);
				DateTime CreationTime;

				foreach (string FileName in Files)
				{
					try
					{
						CreationTime = File.GetCreationTime(FileName);

						if (CreationTime.Day == 1)
						{
							if (CreationTime.Month == 1)    // Yearly
							{
								if (Now.Year - CreationTime.Year <= KeepYears)
									continue;
							}
							else    // Monthly
							{
								if (((Now.Year * 12 + Now.Month) - (CreationTime.Year * 12 + Now.Month)) <= KeepMonths)
									continue;
							}
						}
						else    // Daily
						{
							if ((Now.Date - CreationTime.Date).TotalDays <= KeepDays)
								continue;
						}

						File.Delete(FileName);
						Log.Informational("Backup file deleted.", FileName);

						ExportFormat.UpdateClientsFileDeleted(System.IO.Path.GetFileName(FileName));
					}
					catch (Exception ex)
					{
						Log.Critical(ex, FileName);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		internal static void UpdateExportFolder(string Folder)
		{
			if (exportFolder != null)
				exportFolder.FolderPath = Folder;
		}

		internal static void UpdateExportKeyFolder(string Folder)
		{
			if (keyFolder != null)
				keyFolder.FolderPath = Folder;
		}

		#endregion

		#region Notifications

		private static void MailClient_MailReceived(object Sender, MailEventArgs e)
		{
			MailReceived?.Invoke(Sender, e);
		}

		/// <summary>
		/// Event raised when a mail has been received.
		/// </summary>
		public static MailEventHandler MailReceived = null;

		/// <summary>
		/// Sends a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Markdown">Markdown of message.</param>
		public static void SendNotification(string Markdown)
		{
			try
			{
				CaseInsensitiveString[] Addresses = GetNotificationAddresses();
				MarkdownSettings Settings = new MarkdownSettings()
				{
					ParseMetaData = false
				};
				MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings);
				string Text = Doc.GeneratePlainText();
				string Html = HtmlDocument.GetBody(Doc.GenerateHTML());

				foreach (CaseInsensitiveString Admin in Addresses)
					SendNotification(Admin, Markdown, Text, Html);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Returns configured notification addresses.
		/// </summary>
		/// <returns>Array of addresses</returns>
		public static CaseInsensitiveString[] GetNotificationAddresses()
		{
			return NotificationConfiguration.Instance.Addresses;
		}

		private static void SendNotification(string To, string Markdown, string Text, string Html)
		{
			if (Gateway.XmppClient != null && Gateway.XmppClient.State == Networking.XMPP.XmppState.Connected)
			{
				RosterItem Item = Gateway.XmppClient.GetRosterItem(To);
				if (Item is null || (Item.State != SubscriptionState.To && Item.State != SubscriptionState.Both))
				{
					xmppClient.RequestPresenceSubscription(To);
					ScheduleEvent(Resend, DateTime.Now.AddMinutes(15), new string[] { To, Markdown, Text, Html });
				}
				else
				{
					StringBuilder Xml = new StringBuilder();

					Xml.Append("<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">");
					Xml.Append(XML.Encode(Markdown));
					Xml.Append("</content><html xmlns='http://jabber.org/protocol/xhtml-im'><body xmlns='http://www.w3.org/1999/xhtml'>");
					Xml.Append(Html);
					Xml.Append("</body></html>");

					xmppClient.SendMessage(MessageType.Chat, To, Xml.ToString(), Text, string.Empty, string.Empty, string.Empty, string.Empty);
				}
			}
			else
				ScheduleEvent(Resend, DateTime.Now.AddSeconds(30), new string[] { To, Markdown, Text, Html });
		}

		private static void Resend(object P)
		{
			string[] P2 = (string[])P;
			SendNotification(P2[0], P2[1], P2[2], P2[3]);
		}


		#endregion

	}
}
