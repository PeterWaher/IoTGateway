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
using Waher.Content.Emoji;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown.Web;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Events.Files;
using Waher.Events.Persistence;
using Waher.Events.XMPP;
using Waher.IoTGateway.WebResources;
using Waher.IoTGateway.Setup;
using Waher.Networking.CoAP;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.InBandBytestreams;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.Sensor;
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
using Waher.Things.ControlParameters;
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
		private static Networking.XMPP.InBandBytestreams.IbbClient ibbClient = null;
		private static Networking.XMPP.P2P.SOCKS5.Socks5Proxy socksProxy = null;
		private static ConcentratorServer concentratorServer = null;
		private static SynchronizationClient synchronizationClient = null;
		private static PepClient pepClient = null;
		private static Timer connectionTimer = null;
		private static X509Certificate2 certificate = null;
		private static HttpServer webServer = null;
		private static HttpxProxy httpxProxy = null;
		private static HttpxServer httpxServer = null;
		private static CoapEndpoint coapEndpoint = null;
		private static ClientEvents clientEvents = null;
		private static Login login = null;
		private static Logout logout = null;
		private static LoggedIn loggedIn = null;
		private static Scheduler scheduler = null;
		private static RandomNumberGenerator rnd = null;
		private static Semaphore gatewayRunning = null;
		private static Emoji1LocalFiles emoji1_24x24 = null;
		private static StreamWriter exceptionFile = null;
		private static string domain = null;
		private static string ownerJid = null;
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
					else
					{
						appDataFolder = string.Empty;
						rootFolder = "Root" + Path.DirectorySeparatorChar;
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

							exceptionFile.WriteLine();

							Exception ex = e.Exception;

							while (ex != null)
							{
								exceptionFile.WriteLine(ex.Message);
								exceptionFile.WriteLine();
								exceptionFile.WriteLine(ex.StackTrace);
								exceptionFile.WriteLine();

								ex = ex.InnerException;
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

				if (DatabaseProvider == null)
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

					webServer.Register(new HttpFolderResource("/Graphics", Path.Combine(appDataFolder, "Graphics"), false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
					webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
					webServer.Register(HttpFolderResource = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
					webServer.Register("/", (req, resp) => throw new TemporaryRedirectException(defaultPage));
					webServer.Register(clientEvents = new ClientEvents());
					webServer.Register(login = new Login());
					webServer.Register(logout = new Logout());

					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					HttpFolderResource.AllowTypeConversion();
					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
				}

				foreach (SystemConfiguration Configuration in Configurations)
				{
					Configuration.SetStaticInstance(Configuration);

					if (webServer != null)
						await Configuration.InitSetup(webServer);
				}

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

						await Configuration.SetupConfiguration(webServer);
						NeedsCleanup = true;
					}

					await Configuration.ConfigureSystem();

					if (NeedsCleanup)
						await Configuration.CleanupAfterConfiguration(webServer);
				}

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
					webServer.Dispose();
					webServer = null;
				}

				if (certificate != null)
					webServer = new HttpServer(GetConfigPorts("HTTP"), GetConfigPorts("HTTPS"), certificate);
				else
					webServer = new HttpServer(GetConfigPorts("HTTP"), null, null);

				foreach (SystemConfiguration Configuration in Configurations)
					await Configuration.InitSetup(webServer);

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
				webServer.Register(login = new Login());
				webServer.Register(logout = new Logout());

				if (emoji1_24x24 != null)
				{
					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
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
				Types.SetModuleParameter("Concentrator", concentratorServer);
				Types.SetModuleParameter("Sensor", concentratorServer.SensorServer);
				Types.SetModuleParameter("Control", concentratorServer.ControlServer);
				Types.SetModuleParameter("Registry", thingRegistryClient);
				Types.SetModuleParameter("Provisioning", provisioningClient);

				ScheduleEvent(DeleteOldDataSourceEvents, DateTime.Today.AddDays(1).AddHours(4), null);
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

			DateTime Now = DateTime.Now;
			int MsToNext = 60000 - (Now.Second * 1000 + Now.Millisecond);

			connectionTimer = new Timer(CheckConnection, null, MsToNext, 60000);
			xmppClient.OnStateChanged += XmppClient_OnStateChanged;
			xmppClient.OnRosterItemUpdated += XmppClient_OnRosterItemUpdated;

			ibbClient = new Networking.XMPP.InBandBytestreams.IbbClient(xmppClient, MaxChunkSize);
			Types.SetModuleParameter("IBB", ibbClient);

			socksProxy = new Networking.XMPP.P2P.SOCKS5.Socks5Proxy(xmppClient);
			Types.SetModuleParameter("SOCKS5", socksProxy);

			synchronizationClient = new SynchronizationClient(xmppClient);
			pepClient = new PepClient(xmppClient);

			return Task.CompletedTask;
		}

		internal static Task ConfigureDomain(DomainConfiguration Configuration)
		{
			domain = Configuration.Domain;

			if (Configuration.UseEncryption && Configuration.Certificate != null && Configuration.PrivateKey != null)
			{
				UpdateCertificate(Configuration);
				scheduler.Add(DateTime.Now.AddDays(0.5 + NextDouble()), CheckCertificate, Configuration);
			}
			else
				certificate = null;

			return Task.CompletedTask;
		}

		private static bool UpdateCertificate(DomainConfiguration Configuration)
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
					if (await Configuration.CreateCertificate())
					{
						if (UpdateCertificate(Configuration))
							webServer.UpdateCertificate(certificate);
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
				await MeteringTopology.DeleteOldEvents(new TimeSpan(7, 0, 0, 0));
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			ScheduleEvent(DeleteOldDataSourceEvents, DateTime.Today.AddDays(1).AddHours(4), null);
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

			if (httpxProxy != null)
			{
				httpxProxy.Dispose();
				httpxProxy = null;
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

			if (synchronizationClient != null)
			{
				synchronizationClient.Dispose();
				synchronizationClient = null;
			}

			if (pepClient != null)
			{
				pepClient.Dispose();
				pepClient = null;
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
			login = null;
			logout = null;

			if (exportExceptions)
			{
				exportExceptions = false;

				lock (exceptionFile)
				{
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
		/// Default page of gateway.
		/// </summary>
		public static string DefaultPage
		{
			get { return defaultPage; }
		}

		/// <summary>
		/// Application Name.
		/// </summary>
		public static string ApplicationName
		{
			get { return applicationName; }
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

			if (Request.Session == null || !Request.Session.TryGetVariable("from", out Variable v) || string.IsNullOrEmpty(From = v.ValueObject as string))
				return;

			if (!loopbackIntefaceAvailable && (XmppConfiguration.Instance == null || !XmppConfiguration.Instance.Complete || configuring))
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
			if (XmppConfiguration.Instance == null || !XmppConfiguration.Instance.Complete || configuring)
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

			if (Request.Session == null || !Request.Session.TryGetVariable("User", out Variable v) || (User = v.ValueObject as IUser) == null)
				throw new Networking.HTTP.ForbiddenException();
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
		public static void NewMomentaryValues(ThingReference Reference, params Field[] Values)
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
		public static void NewMomentaryValues(ThingReference Reference, IEnumerable<Field> Values)
		{
			concentratorServer?.SensorServer?.NewMomentaryValues(Reference, Values);
		}

		#endregion

		#region Personal Eventing Protocll

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
			if (pepClient == null)
				return false;
			else
				return pepClient.UnregisterHandler(PersonalEventType, Handler);
		}

		#endregion

	}
}
