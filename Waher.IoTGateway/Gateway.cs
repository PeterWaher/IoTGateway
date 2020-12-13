using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
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
using SkiaSharp;
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
using Waher.IoTGateway.Events;
using Waher.IoTGateway.Exceptions;
using Waher.IoTGateway.Setup;
using Waher.IoTGateway.Setup.Legal;
using Waher.IoTGateway.WebResources;
using Waher.IoTGateway.WebResources.ExportFormats;
using Waher.Networking.CoAP;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Contracts;
using Waher.Networking.XMPP.HTTPX;
using Waher.Networking.XMPP.Mail;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.PEP;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.Software;
using Waher.Networking.XMPP.Synchronization;
using Waher.Runtime.Language;
using Waher.Runtime.Inventory;
using Waher.Runtime.Inventory.Loader;
using Waher.Runtime.Settings;
using Waher.Runtime.Timing;
using Waher.Persistence;
using Waher.Persistence.Filters;
using Waher.Script;
using Waher.Security;
using Waher.Security.CallStack;
using Waher.Security.LoginMonitor;
using Waher.Things;
using Waher.Things.Metering;
using Waher.Things.SensorData;
using Waher.Script.Graphs;

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
		private const int MaxChunkSize = 4096;

		private static readonly LinkedList<KeyValuePair<string, int>> ports = new LinkedList<KeyValuePair<string, int>>();
		private static readonly Dictionary<int, EventHandler> serviceCommandByNr = new Dictionary<int, EventHandler>();
		private static readonly Dictionary<EventHandler, int> serviceCommandNrByCallback = new Dictionary<EventHandler, int>();
		private static readonly Dictionary<string, DateTime> lastUnauthorizedAccess = new Dictionary<string, DateTime>();
		private static IDatabaseProvider internalProvider = null;
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
		private static MultiUserChatClient mucClient = null;
		private static ContractsClient contractsClient = null;
		private static SoftwareUpdateClient softwareUpdateClient = null;
		private static MailClient mailClient = null;
		private static X509Certificate2 certificate = null;
		private static DateTime checkCertificate = DateTime.MinValue;
		private static DateTime checkIp = DateTime.MinValue;
		private static HttpServer webServer = null;
		private static HttpFolderResource root = null;
		private static HttpxProxy httpxProxy = null;
		private static HttpxServer httpxServer = null;
		private static CoapEndpoint coapEndpoint = null;
		private static SystemConfiguration[] configurations;
		private static LoggedIn loggedIn = null;
		private static LoginAuditor loginAuditor = null;
		private static Scheduler scheduler = null;
		private readonly static RandomNumberGenerator rnd = RandomNumberGenerator.Create();
		private static Semaphore gatewayRunning = null;
		private static Semaphore startingServer = null;
		private static Emoji1LocalFiles emoji1_24x24 = null;
		private static StreamWriter exceptionFile = null;
		private static CaseInsensitiveString domain = null;
		private static CaseInsensitiveString ownerJid = null;
		private static string instance;
		private static string appDataFolder;
		private static string runtimeFolder;
		private static string rootFolder;
		private static string defaultPage;
		private static string applicationName;
		private static string exceptionFolder = null;
		private static string exceptionFileName = null;
		private static int nextServiceCommandNr = 128;
		private static int beforeUninstallCommandNr = 0;
		private static bool firstStart = true;
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
			return Start(ConsoleOutput, true, string.Empty);
		}

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		/// <param name="LoopbackIntefaceAvailable">If the loopback interface is available.</param>
		/// <returns>If the gateway was successfully started.</returns>
		public static Task<bool> Start(bool ConsoleOutput, bool LoopbackIntefaceAvailable)
		{
			return Start(ConsoleOutput, LoopbackIntefaceAvailable, string.Empty);
		}

		/// <summary>
		/// Starts the gateway.
		/// </summary>
		/// <param name="ConsoleOutput">If console output is permitted.</param>
		/// <param name="LoopbackIntefaceAvailable">If the loopback interface is available.</param>
		/// <param name="InstanceName">Name of instance. Default=<see cref="string.Empty"/>.</param>
		/// <returns>If the gateway was successfully started.</returns>
		public static async Task<bool> Start(bool ConsoleOutput, bool LoopbackIntefaceAvailable, string InstanceName)
		{
			bool FirstStart = firstStart;
			
			firstStart = false;
			instance = InstanceName;

			string Suffix = string.IsNullOrEmpty(InstanceName) ? string.Empty : "." + InstanceName;
			gatewayRunning = new Semaphore(1, 1, "Waher.IoTGateway.Running" + Suffix);
			if (!gatewayRunning.WaitOne(1000))
				return false; // Is running in another process.

			startingServer = new Semaphore(1, 1, "Waher.IoTGateway.Starting" + Suffix);
			if (!startingServer.WaitOne(1000))
			{
				gatewayRunning.Release();
				gatewayRunning.Dispose();
				gatewayRunning = null;

				startingServer.Dispose();
				startingServer = null;
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

				appDataFolder += "IoT Gateway";

				if (!string.IsNullOrEmpty(InstanceName))
					appDataFolder += " " + InstanceName;

				appDataFolder += Path.DirectorySeparatorChar;
				rootFolder = appDataFolder + "Root" + Path.DirectorySeparatorChar;

				Log.Register(new AlertNotifier("Alert Notifier"));
				Log.Register(new XmlFileEventSink("XML File Event Sink",
					appDataFolder + "Events" + Path.DirectorySeparatorChar + "Event Log %YEAR%-%MONTH%-%DAY%T%HOUR%.xml",
					appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "EventXmlToHtml.xslt", 7));

				if (FirstStart)
					Assert.UnauthorizedAccess += Assert_UnauthorizedAccess;

				Log.Informational("Server starting up.");

				if (FirstStart)
				{
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
					Dictionary<string, CopyOptions> ContentOptions = new Dictionary<string, CopyOptions>();
					int i;

					for (i = 0; i < 2; i++)
					{
						foreach (string ManifestFile in ManifestFiles)
						{
							string FileName = Path.GetFileName(ManifestFile);
							bool GatewayFile = FileName.StartsWith("Waher.IoTGateway", StringComparison.CurrentCultureIgnoreCase);

							if ((i == 0 && GatewayFile) || (i == 1 && !GatewayFile))
							{
								CheckContentFiles(ManifestFile, ContentOptions);

								if (ManifestFile.EndsWith("Waher.Utility.Install.manifest"))
									CheckInstallUtilityFiles(ManifestFile);
							}
						}
					}
				}

				Types.SetModuleParameter("AppData", appDataFolder);
				Types.SetModuleParameter("Root", rootFolder);

				scheduler = new Scheduler();

				if (FirstStart)
				{
					Task T = Task.Run(() =>
					{
						CodeContent.GraphViz.Init();
						CodeContent.PlantUml.Init();
					});
				}

				XmlDocument Config = new XmlDocument()
				{
					PreserveWhitespace = true
				};

				string GatewayConfigFileName = appDataFolder + "Gateway.config";
				if (!File.Exists(GatewayConfigFileName))
					GatewayConfigFileName = "Gateway.config";

				Config.Load(GatewayConfigFileName);
				XSL.Validate("Gateway.config", Config, "GatewayConfiguration", "http://waher.se/Schema/GatewayConfiguration.xsd",
					XSL.LoadSchema(typeof(Gateway).Namespace + ".Schema.GatewayConfiguration.xsd", typeof(Gateway).Assembly));

				IDatabaseProvider DatabaseProvider = null;

				foreach (XmlNode N in Config.DocumentElement.ChildNodes)
				{
					if (N is XmlElement E)
					{
						switch (E.LocalName)
						{
							case "ApplicationName":
								applicationName = E.InnerText;
								break;

							case "DefaultPage":
								defaultPage = E.InnerText;
								break;

							case "ExportExceptions":
								exceptionFolder = Path.Combine(appDataFolder, XML.Attribute(E, "folder", "Exceptions"));

								if (!Directory.Exists(exceptionFolder))
									Directory.CreateDirectory(exceptionFolder);

								DateTime Now = DateTime.Now;
								string[] ExceptionFiles = Directory.GetFiles(exceptionFolder, "*.txt", SearchOption.TopDirectoryOnly);
								foreach (string ExceptionFile in ExceptionFiles)
								{
									try
									{
										DateTime TP = File.GetLastWriteTime(ExceptionFile);
										if ((TP - Now).TotalDays > 90)
											File.Delete(ExceptionFile);
										else
										{
											string XmlFile = Path.ChangeExtension(ExceptionFile, "xml");
											if (!File.Exists(XmlFile))
											{
												Log.Informational("Processing " + ExceptionFile);
												Analyze.Process(ExceptionFile, XmlFile);
											}
										}
									}
									catch (Exception ex)
									{
										Log.Critical(ex, ExceptionFile);
									}
								}

								exceptionFileName = Path.Combine(exceptionFolder, Now.Year.ToString("D4") + "-" + Now.Month.ToString("D2") + "-" + Now.Day.ToString("D2") +
									" " + Now.Hour.ToString("D2") + "." + Now.Minute.ToString("D2") + "." + Now.Second.ToString("D2") + ".txt");
								exceptionFile = File.CreateText(exceptionFileName);
								exportExceptions = true;

								exceptionFile.Write("Start of export: ");
								exceptionFile.WriteLine(DateTime.Now.ToString());

								if (FirstStart)
								{
									AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
									{
										if (exceptionFile is null)
											return;

										lock (exceptionFile)
										{
											if (!exportExceptions || e.Exception.StackTrace.Contains("FirstChanceExceptionEventArgs"))
												return;

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
													exceptionFile.WriteLine(Log.CleanStackTrace(ex.StackTrace));
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
								break;

							case "Database":
								if (FirstStart)
								{
									if (!(DatabaseProvider is null))
										throw new Exception("Database provider already initiated.");

									if (!(GetDatabaseProvider is null))
										DatabaseProvider = await GetDatabaseProvider(E);
									else
										DatabaseProvider = null;

									if (DatabaseProvider is null)
										throw new Exception("Database provider not defined. Make sure the GetDatabaseProvider event has an appropriate event handler.");

									internalProvider = DatabaseProvider;
									Database.Register(DatabaseProvider, false);
								}
								else
								{
									DatabaseProvider = Database.Provider;
									await DatabaseProvider.Start();

									if (Ledger.HasProvider)
										await Ledger.Provider.Start();
								}
								break;

							case "Ports":
								foreach (XmlNode N2 in E.ChildNodes)
								{
									if (N2.LocalName == "Port")
									{
										XmlElement E2 = (XmlElement)N2;
										string Protocol = XML.Attribute(E2, "protocol");
										if (!string.IsNullOrEmpty(Protocol) && int.TryParse(E2.InnerText, out int Port2))
											ports.AddLast(new KeyValuePair<string, int>(Protocol, Port2));
									}
								}
								break;
						}
					}
				}

				if (DatabaseProvider is null)
					throw new Exception("Database provider not defined in Gateway.config.");

				Database.CollectionRepaired += Database_CollectionRepaired;

				await RepairIfInproperShutdown();

				PersistedEventLog PersistedEventLog = new PersistedEventLog(90, new TimeSpan(4, 15, 0));
				Log.Register(PersistedEventLog);
				try
				{
					await PersistedEventLog.Queue(new Event(EventType.Informational, "Server starting up.", string.Empty, string.Empty, string.Empty, EventLevel.Minor, string.Empty, string.Empty, string.Empty));
				}
				catch (Exception ex)
				{
					Event Event = new Event(DateTime.Now, EventType.Critical, ex.Message, PersistedEventLog.ObjectID, string.Empty, string.Empty,
						EventLevel.Major, string.Empty, ex.Source, Log.CleanStackTrace(ex.StackTrace));

					Event.Avoid(PersistedEventLog);

					Log.Event(Event);
				}

				loginAuditor = new LoginAuditor("Login Auditor",
					new LoginInterval(5, TimeSpan.FromHours(1)),    // Maximum 5 failed login attempts in an hour
					new LoginInterval(2, TimeSpan.FromDays(1)),     // Maximum 2x5 failed login attempts in a day
					new LoginInterval(2, TimeSpan.FromDays(7)),     // Maximum 2x2x5 failed login attempts in a week
					new LoginInterval(2, TimeSpan.MaxValue));       // Maximum 2x2x2x5 failed login attempts in total, then blocked.
				Log.Register(loginAuditor);

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

				configurations = new SystemConfiguration[SystemConfigurations.Count];
				SystemConfigurations.Values.CopyTo(configurations, 0);
				Array.Sort<SystemConfiguration>(configurations, (c1, c2) => c1.Priority - c2.Priority);

				ISystemConfiguration CurrentConfiguration = null;
				LinkedList<HttpResource> SetupResources = null;

				if (!Configured)
				{
					configuring = true;

					if (loopbackIntefaceAvailable)
						Log.Notice("System needs to be configured. This is done by navigating to the loopback interface using a browser on this machine.");
					else
						Log.Notice("System needs to be configured. This is done by navigating to the machine using a browser on another machine in the same network.");

					webServer = new HttpServer(new int[] { HttpServer.DefaultHttpPort, 8080, 8081, 8082 }, null, null)
					{
						ResourceOverrideFilter = "(?<!Login)[.]md(\\?[.]*)?$",
						LoginAuditor = loginAuditor
					};

					webServer.CustomError += WebServer_CustomError;

					loggedIn = new LoggedIn(webServer);

					SetupResources = new LinkedList<HttpResource>();

					SetupResources.AddLast(webServer.Register(new HttpFolderResource("/Graphics", Path.Combine(appDataFolder, "Graphics"), false, false, true, false))); // TODO: Add authentication mechanisms for PUT & DELETE.
					SetupResources.AddLast(webServer.Register(new HttpFolderResource("/Transforms", Path.Combine(appDataFolder, "Transforms"), false, false, true, false))); // TODO: Add authentication mechanisms for PUT & DELETE.
					SetupResources.AddLast(webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false)));   // Syntax highlighting library, provided by http://highlightjs.org
					SetupResources.AddLast(webServer.Register(root = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true)));    // TODO: Add authentication mechanisms for PUT & DELETE.
					SetupResources.AddLast(webServer.Register("/", (req, resp) => throw new TemporaryRedirectException(defaultPage)));
					SetupResources.AddLast(webServer.Register(new ClientEvents()));
					SetupResources.AddLast(webServer.Register(new ClientEventsWebSocket()));
					SetupResources.AddLast(webServer.Register(new Login()));
					SetupResources.AddLast(webServer.Register(new Logout()));

					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					root.AllowTypeConversion();

					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
					MarkdownToHtmlConverter.RootFolder = rootFolder;
				}

				foreach (SystemConfiguration Configuration in configurations)
				{
					Configuration.SetStaticInstance(Configuration);

					if (!(webServer is null))
						await Configuration.InitSetup(webServer);
				}

				bool ReloadConfigurations;

				do
				{
					ReloadConfigurations = false;

					foreach (SystemConfiguration Configuration in configurations)
					{
						bool NeedsCleanup = false;

						if (!Configuration.Complete)
						{
							CurrentConfiguration = Configuration;
							webServer.ResourceOverride = Configuration.Resource;
							Configuration.SetStaticInstance(Configuration);

							startingServer?.Release();
							startingServer?.Dispose();
							startingServer = null;

							ClientEvents.PushEvent(ClientEvents.GetTabIDs(), "Reload", string.Empty);

							if (await Configuration.SetupConfiguration(webServer))
								ReloadConfigurations = true;

							NeedsCleanup = true;
						}

						try
						{
							await Configuration.ConfigureSystem();
						}
						catch (Exception)
						{
							await RepairIfInproperShutdown();

							try
							{
								await Configuration.ConfigureSystem();
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}

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

								if (!(webServer is null))
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

							configurations = new SystemConfiguration[SystemConfigurations.Count];
							SystemConfigurations.Values.CopyTo(configurations, 0);
							Array.Sort<SystemConfiguration>(configurations, (c1, c2) => c1.Priority - c2.Priority);

							break;
						}
					}
				}
				while (ReloadConfigurations);

				configuring = false;

				if (!(webServer is null))
				{
					webServer.ResourceOverride = null;
					webServer.ResourceOverrideFilter = null;

					if (!(SetupResources is null))
					{
						foreach (HttpResource Resource in SetupResources)
							webServer.Unregister(Resource);
					}

					webServer.AddHttpPorts(GetConfigPorts("HTTP"));

					if (!(certificate is null))
					{
						webServer.AddHttpsPorts(GetConfigPorts("HTTPS"));
						webServer.UpdateCertificate(certificate);
					}
				}
				else
				{
					if (!(certificate is null))
						webServer = new HttpServer(GetConfigPorts("HTTP"), GetConfigPorts("HTTPS"), certificate);
					else
						webServer = new HttpServer(GetConfigPorts("HTTP"), null, null);

					webServer.CustomError += WebServer_CustomError;

					webServer.LoginAuditor = loginAuditor;
					loggedIn = new LoggedIn(webServer);

					foreach (SystemConfiguration Configuration in configurations)
					{
						try
						{
							await Configuration.InitSetup(webServer);
						}
						catch (Exception)
						{
							await RepairIfInproperShutdown();

							try
							{
								await Configuration.InitSetup(webServer);
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
					}
				}

				Types.SetModuleParameter("HTTP", webServer);

				loggedIn = new LoggedIn(webServer);

				WriteWebServerOpenPorts();
				webServer.OnNetworkChanged += (sender, e) => WriteWebServerOpenPorts();

				webServer.Register(new HttpFolderResource("/Graphics", Path.Combine(appDataFolder, "Graphics"), false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(new HttpFolderResource("/Transforms", Path.Combine(appDataFolder, "Transforms"), false, false, true, false)); // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(new HttpFolderResource("/highlight", "Highlight", false, false, true, false));   // Syntax highlighting library, provided by http://highlightjs.org
				webServer.Register(root = new HttpFolderResource(string.Empty, rootFolder, false, false, true, true));    // TODO: Add authentication mechanisms for PUT & DELETE.
				webServer.Register(httpxProxy = new HttpxProxy("/HttpxProxy", xmppClient, MaxChunkSize));
				webServer.Register("/", (req, resp) => throw new TemporaryRedirectException(defaultPage));
				webServer.Register(new ClientEvents());
				webServer.Register(new ClientEventsWebSocket());
				webServer.Register(new Login());
				webServer.Register(new Logout());
				webServer.Register(new ProposeContract());

				if (emoji1_24x24 is null)
				{
					emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Svg, 24, 24, "/Graphics/Emoji1/svg/%FILENAME%",
						Path.Combine(runtimeFolder, "Graphics", "Emoji1.zip"), Path.Combine(appDataFolder, "Graphics"));

					MarkdownToHtmlConverter.EmojiSource = emoji1_24x24;
					MarkdownToHtmlConverter.RootFolder = rootFolder;
				}

				root.AllowTypeConversion();

				XmlElement FileFolders = Config.DocumentElement["FileFolders"];
				if (!(FileFolders is null))
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

				XmlElement VanityResources = Config.DocumentElement["VanityResources"];
				if (!(VanityResources is null))
				{
					foreach (XmlNode N in VanityResources.ChildNodes)
					{
						if (N is XmlElement E && E.LocalName == "VanityResource")
						{
							string RegEx = XML.Attribute(E, "regex");
							string Url = XML.Attribute(E, "url");

							try
							{
								webServer.RegisterVanityResource(RegEx, Url);
							}
							catch (Exception ex)
							{
								Log.Error("Unable to register vanity resource: " + ex.Message,
									new KeyValuePair<string, object>("RegEx", RegEx),
									new KeyValuePair<string, object>("Url", Url));
							}
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

				IDataSource[] Sources;

				try
				{
					Sources = new IDataSource[] { new MeteringTopology() };
				}
				catch (Exception)
				{
					await RepairIfInproperShutdown();

					try
					{
						Sources = new IDataSource[] { new MeteringTopology() };
					}
					catch (Exception ex)
					{
						Log.Critical(ex);
						Sources = new IDataSource[0];
					}
				}

				if (!(GetDataSources is null))
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

				if (FirstStart)
					MeteringTopology.OnNewMomentaryValues += NewMomentaryValues;

				DeleteOldDataSourceEvents(null);

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
									XmlDocument Doc = new XmlDocument()
									{
										PreserveWhitespace = true
									};
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

					foreach (string UnhandledException in Directory.GetFiles(appDataFolder, "UnhandledException*.txt", SearchOption.TopDirectoryOnly))
					{
						try
						{
							string Msg = File.ReadAllText(UnhandledException);
							File.Delete(UnhandledException);
							Log.Alert("Unhandled Exception\r\n=======================\r\n\r\n```\r\n" + Msg + "\r\n```");
						}
						catch (Exception ex)
						{
							Log.Critical(ex, UnhandledException);
						}
					}

					if (await Types.StartAllModules(int.MaxValue, new ModuleStartOrder()))
						Log.Informational("Server started.");
					else
						Log.Critical("Unable to start all modules.");
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
				finally
				{
					startingServer?.Release();
					startingServer?.Dispose();
					startingServer = null;

					if (xmppClient.State != XmppState.Connected)
						xmppClient.Connect();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				startingServer?.Release();
				startingServer?.Dispose();
				startingServer = null;

				gatewayRunning?.Release();
				gatewayRunning?.Dispose();
				gatewayRunning = null;

				ExceptionDispatchInfo.Capture(ex).Throw();
			}

			return true;
		}

		private class ModuleStartOrder : IComparer<IModule>
		{
			public int Compare(IModule x, IModule y)
			{
				bool dm1 = x is Persistence.LifeCycle.DatabaseModule;
				bool dm2 = y is Persistence.LifeCycle.DatabaseModule;

				if (dm1 && dm2)
					return 0;
				else if (dm1)
					return -1;
				else if (dm2)
					return 1;
				else
					return string.Compare(x.GetType().FullName, y.GetType().FullName);
			}
		}

		private static async Task RepairIfInproperShutdown()
		{
			IDatabaseProvider DatabaseProvider = Database.Provider;
			Type ProviderType = DatabaseProvider.GetType();
			PropertyInfo AutoRepairReportFolder = ProviderType.GetProperty("AutoRepairReportFolder");
			if (!(AutoRepairReportFolder is null))
				AutoRepairReportFolder.SetValue(DatabaseProvider, Path.Combine(AppDataFolder, "Backup"));

			MethodInfo MI = ProviderType.GetMethod("RepairIfInproperShutdown", new Type[] { typeof(string) });

			if (!(MI is null))
			{
				Task T = MI.Invoke(DatabaseProvider, new object[] { Gateway.AppDataFolder + "Transforms" + Path.DirectorySeparatorChar + "DbStatXmlToHtml.xslt" }) as Task;

				if (T is Task<string[]> StringArrayTask)
					DatabaseConfiguration.RepairedCollections = await StringArrayTask;
				else if (!(T is null))
					await T;
			}
		}

		private static void WriteWebServerOpenPorts()
		{
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
		}

		private static void Assert_UnauthorizedAccess(object Sender, UnauthorizedAccessEventArgs e)
		{
			DateTime Now = DateTime.Now;
			string Key = e.Trace.ToString();

			lock (lastUnauthorizedAccess)
			{
				if (lastUnauthorizedAccess.TryGetValue(Key, out DateTime TP) && (Now - TP).TotalHours < 1)
					return;

				lastUnauthorizedAccess[Key] = Now;
			}

			StringBuilder Markdown = new StringBuilder();

			Markdown.AppendLine("Unauthorized access detected and prevented.");
			Markdown.AppendLine("===============================================");
			Markdown.AppendLine();
			Markdown.AppendLine("| Details ||");
			Markdown.AppendLine("|:---|:---|");
			Markdown.Append("| Method | `");
			Markdown.Append(e.Method.Name);
			Markdown.AppendLine("` |");
			Markdown.Append("| Type | `");
			Markdown.Append(e.Type.FullName);
			Markdown.AppendLine("` |");
			Markdown.Append("| Assembly | `");
			Markdown.Append(e.Assembly.GetName().Name);
			Markdown.AppendLine("` |");
			Markdown.Append("| Date | ");
			Markdown.Append(MarkdownDocument.Encode(Now.ToShortDateString()));
			Markdown.AppendLine(" |");
			Markdown.Append("| Time | ");
			Markdown.Append(MarkdownDocument.Encode(Now.ToLongTimeString()));
			Markdown.AppendLine(" |");
			Markdown.AppendLine();
			Markdown.AppendLine("Stack Trace:");
			Markdown.AppendLine();
			Markdown.AppendLine("```");
			Markdown.AppendLine(Log.CleanStackTrace(e.Trace.ToString()));
			Markdown.AppendLine("```");

			Gateway.SendNotification(Markdown.ToString());
		}

		private static void CheckContentFiles(string ManifestFileName, Dictionary<string, CopyOptions> ContentOptions)
		{
			try
			{
				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.Load(ManifestFileName);

				if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "Module" && Doc.DocumentElement.NamespaceURI == "http://waher.se/Schema/ModuleManifest.xsd")
					CheckContentFiles(Doc.DocumentElement, runtimeFolder, runtimeFolder, appDataFolder, ContentOptions);
			}
			catch (Exception ex)
			{
				Log.Critical(ex, ManifestFileName);
			}
		}

		private enum CopyOptions
		{
			IfNewer,
			Always
		}

		private static void CheckContentFiles(XmlElement Element, string RuntimeFolder, string RuntimeSubfolder, string AppDataSubFolder,
			Dictionary<string, CopyOptions> ContentOptions)
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
							CheckContentFiles(E, RuntimeFolder, Path.Combine(RuntimeSubfolder, Name), Path.Combine(AppDataSubFolder, Name),
								ContentOptions);
							break;

						case "Content":
							Name = XML.Attribute(E, "fileName");
							CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

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

							if (CopyOptions == CopyOptions.Always || !File.Exists(s2))
							{
								File.Copy(s, s2, true);
								ContentOptions[s2] = CopyOptions;
							}
							else
							{
								DateTime TP = File.GetLastWriteTime(s);
								DateTime TP2 = File.GetLastWriteTime(s2);

								if (TP > TP2 &&
									(!ContentOptions.TryGetValue(s2, out CopyOptions CopyOptions2) ||
									CopyOptions2 != CopyOptions.Always))
								{
									File.Copy(s, s2, true);
									ContentOptions[s2] = CopyOptions;
								}
							}
							break;
					}
				}
			}
		}

		private static void CheckInstallUtilityFiles(string ManifestFileName)
		{
			try
			{
				XmlDocument Doc = new XmlDocument()
				{
					PreserveWhitespace = true
				};
				Doc.Load(ManifestFileName);

				if (Doc.DocumentElement != null && Doc.DocumentElement.LocalName == "Module" && Doc.DocumentElement.NamespaceURI == "http://waher.se/Schema/ModuleManifest.xsd")
				{
					string InstallUtilityFolder = Path.Combine(runtimeFolder, "InstallUtility");
					bool NoticeLogged = false;

					if (!Directory.Exists(InstallUtilityFolder))
						Directory.CreateDirectory(InstallUtilityFolder);

					foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
					{
						if (N is XmlElement E)
						{
							switch (E.LocalName)
							{
								case "Assembly":
									string Name = XML.Attribute(E, "fileName");
									CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

									string s = Path.Combine(runtimeFolder, Name);
									if (!File.Exists(s))
										break;

									string s2 = Path.Combine(InstallUtilityFolder, Name);

									if (CopyOptions == CopyOptions.IfNewer && File.Exists(s2))
									{
										DateTime TP = File.GetLastWriteTime(s);
										DateTime TP2 = File.GetLastWriteTime(s2);

										if (TP <= TP2)
											break;
									}

									if (!NoticeLogged)
									{
										NoticeLogged = true;
										Log.Notice("Copying Installation Utility executable files to InstallUtility subfolder.");
									}

									File.Copy(s, s2, true);
									break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex, ManifestFileName);
			}
		}

		internal static bool ConsoleOutput => consoleOutput;

		internal static async Task ConfigureXmpp(XmppConfiguration Configuration)
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
				provisioningClient = null;

			scheduler.Add(DateTime.Now.AddMinutes(1), CheckConnection, null);

			xmppClient.OnStateChanged += XmppClient_OnStateChanged;

			ibbClient = new Networking.XMPP.InBandBytestreams.IbbClient(xmppClient, MaxChunkSize);
			Types.SetModuleParameter("IBB", ibbClient);

			socksProxy = new Networking.XMPP.P2P.SOCKS5.Socks5Proxy(xmppClient);
			Types.SetModuleParameter("SOCKS5", socksProxy);

			synchronizationClient = new SynchronizationClient(xmppClient);
			pepClient = new PepClient(xmppClient, XmppConfiguration.Instance.PubSub);

			if (!string.IsNullOrEmpty(XmppConfiguration.Instance.LegalIdentities))
				contractsClient = await ContractsClient.Create(xmppClient, XmppConfiguration.Instance.LegalIdentities);
			else
				contractsClient = null;

			if (!string.IsNullOrEmpty(XmppConfiguration.Instance.MultiUserChat))
				mucClient = new MultiUserChatClient(xmppClient, XmppConfiguration.Instance.MultiUserChat);
			else
				mucClient = null;

			if (!string.IsNullOrEmpty(XmppConfiguration.Instance.SoftwareUpdates))
			{
				string PackagesFolder = Path.Combine(Gateway.appDataFolder, "Packages");
				if (!Directory.Exists(PackagesFolder))
					Directory.CreateDirectory(PackagesFolder);

				softwareUpdateClient = new SoftwareUpdateClient(xmppClient, XmppConfiguration.Instance.SoftwareUpdates, PackagesFolder);
			}
			else
				softwareUpdateClient = null;

			mailClient = new MailClient(xmppClient);
			mailClient.MailReceived += MailClient_MailReceived;
		}

		internal static async Task ConfigureDomain(DomainConfiguration Configuration)
		{
			domain = Configuration.Domain;

			if (Configuration.UseDomainName)
			{
				if (Configuration.DynamicDns)
				{
					await Configuration.CheckDynamicIp();

					if (Configuration.DynDnsInterval > 0)
					{
						if (checkIp > DateTime.MinValue)
						{
							scheduler.Remove(checkIp);
							checkIp = DateTime.MinValue;
						}

						checkIp = scheduler.Add(DateTime.Now.AddSeconds(Configuration.DynDnsInterval), CheckIp, Configuration);
					}
				}

				if (Configuration.UseEncryption &&
					Configuration.Certificate != null &&
					Configuration.PrivateKey != null)
				{
					UpdateCertificate(Configuration);

					if (checkCertificate > DateTime.MinValue)
					{
						scheduler.Remove(checkCertificate);
						checkCertificate = DateTime.MinValue;
					}

					checkCertificate = scheduler.Add(DateTime.Now.AddHours(0.5 + NextDouble()), CheckCertificate, Configuration);
				}
				else
					certificate = null;
			}
			else
				certificate = null;
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
					OnNewCertificate?.Invoke(typeof(Gateway), new Events.CertificateEventArgs(certificate));
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
				checkCertificate = scheduler.Add(DateTime.Now.AddDays(0.5 + NextDouble()), CheckCertificate, Configuration);
			}
		}

		/// <summary>
		/// Event raised when a new server certificate has been generated.
		/// </summary>
		public static event CertificateEventHandler OnNewCertificate = null;

		private static async void CheckIp(object P)
		{
			DomainConfiguration Configuration = (DomainConfiguration)P;

			try
			{
				await Configuration.CheckDynamicIp();
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
			finally
			{
				checkIp = scheduler.Add(DateTime.Now.AddSeconds(Configuration.DynDnsInterval), CheckIp, Configuration);
			}
		}

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
					case "libzstd.dll":
					case "mscordaccore.dll":
					case "mscordbi.dll":
					case "mscorlib.dll":
					case "mscorrc.debug.dll":
					case "mscorrc.dll":
					case "netstandard.dll":
					case "snappy32.dll":
					case "snappy64.dll":
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
		public static async Task Stop()
		{
			if (stopped)
			{
				Log.Notice("Request to stop Gateway, but Gateway already stopped.");
				return;
			}

			Log.Informational("Server shutting down.");

			bool StopInternalProvider = !(internalProvider is null) && Database.Provider != internalProvider;

			stopped = true;
			try
			{
				await Types.StopAllModules();

				Database.CollectionRepaired -= Database_CollectionRepaired;

				if (StopInternalProvider)
					await internalProvider.Stop();

				scheduler?.Dispose();
				scheduler = null;

				startingServer?.Release();
				startingServer?.Dispose();
				startingServer = null;

				gatewayRunning?.Release();
				gatewayRunning?.Dispose();
				gatewayRunning = null;

				if (!(configurations is null))
				{
					foreach (SystemConfiguration Configuration in configurations)
					{
						if (Configuration is IDisposable D)
						{
							try
							{
								D.Dispose();
							}
							catch (Exception ex)
							{
								Log.Critical(ex);
							}
						}
					}

					configurations = null;
				}

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

				mucClient?.Dispose();
				mucClient = null;

				mailClient?.Dispose();
				mailClient = null;

				contractsClient?.Dispose();
				contractsClient = null;

				softwareUpdateClient?.Dispose();
				softwareUpdateClient = null;

				if (!(xmppClient is null))
				{
					using (ManualResetEvent OfflineSent = new ManualResetEvent(false))
					{
						xmppClient.SetPresence(Availability.Offline, (sender, e) => OfflineSent.Set());
						OfflineSent.WaitOne(1000);
					}

					foreach (ISniffer Sniffer in xmppClient.Sniffers)
					{
						XmppClient.Remove(Sniffer);

						if (Sniffer is IDisposable Disposable)
							Disposable.Dispose();
					}

					xmppClient.Dispose();
					xmppClient = null;
				}

				coapEndpoint?.Dispose();
				coapEndpoint = null;

				if (!(webServer is null))
				{
					foreach (ISniffer Sniffer in webServer.Sniffers)
					{
						webServer.Remove(Sniffer);

						if (Sniffer is IDisposable Disposable)
							Disposable.Dispose();
					}

					webServer.Dispose();
					webServer = null;
				}

				root = null;

				if (exportExceptions)
				{
					lock (exceptionFile)
					{
						exportExceptions = false;

						exceptionFile.WriteLine(new string('-', 80));
						exceptionFile.Write("End of export: ");
						exceptionFile.WriteLine(DateTime.Now.ToString());

						exceptionFile.Flush();
						exceptionFile.Close();
					}

					exceptionFile = null;
				}
			}
			finally
			{
				Persistence.LifeCycle.DatabaseModule.Flush().Wait(60000);

				if (StopInternalProvider)
					internalProvider.Flush().Wait(60000);
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
		/// Name of the current instance. Default instance=<see cref="string.Empty"/>
		/// </summary>
		public static string InstanceName
		{
			get { return instance; }
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
		/// Root folder resource.
		/// </summary>
		public static HttpFolderResource Root
		{
			get { return root; }
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
		/// Internal Database provider.
		/// </summary>
		public static IDatabaseProvider InternalDatabase
		{
			get { return internalProvider; }
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

		/// <summary>
		/// Gets the protocol names defined in the configuration file.
		/// </summary>
		/// <returns>Defined protocol names.</returns>
		public static string[] GetProtocols()
		{
			SortedDictionary<string, bool> Protocols = new SortedDictionary<string, bool>();

			foreach (KeyValuePair<string, int> P in ports)
				Protocols[P.Key] = true;

			string[] Result = new string[Protocols.Count];
			Protocols.Keys.CopyTo(Result, 0);

			return Result;
		}

		/// <summary>
		/// Raises the <see cref="OnTerminate"/> event handler, letting the container executable know the application
		/// needs to close.
		/// </summary>
		/// <exception cref="InvalidOperationException">If no <see cref="OnTerminate"/> event handler has been set.</exception>
		public static void Terminate()
		{
			EventHandler h = OnTerminate;
			if (h is null)
				throw new InvalidOperationException("No OnTerminate event handler set.");

			try
			{
				h(instance, new EventArgs());
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Event raised when the <see cref="Terminate"/> method has been called, letting the container exceutable know
		/// the application needs to close.
		/// </summary>
		public static event EventHandler OnTerminate = null;

		#endregion

		#region XMPP

		private static Task XmppClient_OnValidateSender(object Sender, ValidateSenderEventArgs e)
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
						return Task.CompletedTask;
				}

				e.Reject();
			}

			return Task.CompletedTask;
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
					if (!(h is null))
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

		private static Task XmppClient_OnStateChanged(object _, XmppState NewState)
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

			return Task.CompletedTask;
		}

		/// <summary>
		/// Performs a login operation on the main XMPP account, on the main XMPP account domain.
		/// </summary>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="RemoteEndPoint">Remote End-Point.</param>
		/// <param name="Protocol">Protocol used to log in.</param>
		/// <returns>If the login-operation was successful or not.</returns>
		public static async Task<LoginResult> DoMainXmppLogin(string UserName, string Password, string RemoteEndPoint, string Protocol)
		{
			if (xmppClient is null || xmppClient.UserName != UserName)
			{
				LoginAuditor.Fail("Invalid login.", UserName, RemoteEndPoint, Protocol);
				return LoginResult.InvalidLogin;
			}

			TaskCompletionSource<bool> Result = new TaskCompletionSource<bool>();
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
				Client.AllowScramSHA256 = xmppClient.AllowScramSHA256;
				Client.AllowEncryption = xmppClient.AllowEncryption;

				Client.OnStateChanged += (sender, NewState) =>
				{
					switch (NewState)
					{
						case XmppState.StreamOpened:
							Connected = true;
							break;

						case XmppState.Binding:
							Result.TrySetResult(true);
							break;

						case XmppState.Error:
							Result.TrySetResult(false);
							break;
					}

					return Task.CompletedTask;
				};

				scheduler.Add(DateTime.Now.AddSeconds(10), (P) => Result.TrySetResult(false), null);

				Client.Connect();

				if (await Result.Task)
				{
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

					LoginAuditor.Success("Successful login.", UserName, RemoteEndPoint, Protocol);
				}
				else
				{
					if (Connected)
					{
						LoginAuditor.Fail("Invalid login.", UserName, RemoteEndPoint, Protocol);
						return LoginResult.InvalidLogin;
					}
					else
					{
						if ((RemoteEndPoint.StartsWith("[::1]:") || RemoteEndPoint.StartsWith("127.0.0.1:")) &&
							UserName == xmppCredentials.Account && Password == xmppCredentials.Password &&
							string.IsNullOrEmpty(xmppCredentials.PasswordType))
						{
							LoginAuditor.Success("Successful login. Connection to XMPP broker down. Credentials matched configuration and connection made from same machine.",
								UserName, RemoteEndPoint, Protocol);

							return LoginResult.Successful;
						}
						else
						{
							LoginAuditor.Fail("Unable to connect to XMPP broker.", UserName, RemoteEndPoint, Protocol);
							return LoginResult.UnableToConnect;
						}
					}
				}
			}

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
			int i;
			bool DoLog = false;

			if (Request.Header.TryGetQueryParameter("debug", out string s) && CommonTypes.TryParse(s, out bool b))
				DoLog = b;

			if (Request.Session is null)
			{
				if (DoLog)
					Log.Debug("No local login: No session.");

				return;
			}

			if (!Request.Session.TryGetVariable("from", out Variable v) || string.IsNullOrEmpty(From = v.ValueObject as string))
				From = string.Empty;

			if (!loopbackIntefaceAvailable && (XmppConfiguration.Instance is null || !XmppConfiguration.Instance.Complete || configuring))
			{
				LoginAuditor.Success("User logged in by default, since XMPP not configued and loopback interface not available.",
					string.Empty, Request.RemoteEndPoint, "Web");

				Login.DoLogin(Request, From, true);
				return;
			}

			if (DoLog)
				Log.Debug("Checking for local login from: " + RemoteEndpoint);

			i = RemoteEndpoint.LastIndexOf(':');
			if (i < 0 || !int.TryParse(RemoteEndpoint.Substring(i + 1), out int Port))
			{
				if (DoLog)
					Log.Debug("Invalid port number: " + RemoteEndpoint);

				return;
			}

			if (!IPAddress.TryParse(RemoteEndpoint.Substring(0, i), out IPAddress Address))
			{
				if (DoLog)
					Log.Debug("Invalid IP Address: " + RemoteEndpoint);

				return;
			}

			if (!IsLocalCall(Address, Request, DoLog))
			{
				if (DoLog)
					Log.Debug("Request not local: " + RemoteEndpoint);

				return;
			}

#if !MONO
			if (XmppConfiguration.Instance is null || !XmppConfiguration.Instance.Complete || configuring)
#endif
			{
				LoginAuditor.Success("Local user logged in.", string.Empty, Request.RemoteEndPoint, "Web");
				Login.DoLogin(Request, From, true);
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
					if (DoLog)
						Log.Debug("Netstat output:\r\n\r\n" + Output);

					if (Proc.ExitCode != 0)
					{
						if (DoLog)
							Log.Debug("Netstat exit code: " + Proc.ExitCode.ToString());

						return;
					}

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
							LoginAuditor.Success("Local user logged in.", string.Empty, Request.RemoteEndPoint, "Web");
							Login.DoLogin(Request, From, true);
							break;
						}
					}
				}
			}
			catch (HttpException ex)
			{
				if (DoLog)
					Log.Critical(ex);

				ExceptionDispatchInfo.Capture(ex).Throw();
			}
			catch (Exception ex)
			{
				if (DoLog)
					Log.Critical(ex);

				return;
			}
#endif
		}

		private static readonly IPAddress ipv6Local = IPAddress.Parse("[::1]");
		private static readonly IPAddress ipv4Local = IPAddress.Parse("127.0.0.1");

		private static bool IsLocalCall(IPAddress Address, HttpRequest Request, bool DoLog)
		{
			if (Address.Equals(ipv4Local) || Address.Equals(ipv6Local))
				return true;

			string s = Request.Header.Host.Value;
			int i = s.IndexOf(':');
			if (i > 0)
				s = s.Substring(0, i);

			if (string.Compare(s, "localhost", true) != 0)
			{
				if (!IPAddress.TryParse(s, out IPAddress IP) || !Address.Equals(IP))
				{
					if (DoLog)
						Log.Debug("Host not localhost or IP Address: " + Request.Header.Host.Value);

					return false;
				}
			}

			try
			{
				foreach (NetworkInterface Interface in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (Interface.OperationalStatus != OperationalStatus.Up)
						continue;

					IPInterfaceProperties Properties = Interface.GetIPProperties();

					foreach (UnicastIPAddressInformation UnicastAddress in Properties.UnicastAddresses)
					{
						if (Address.Equals(UnicastAddress.Address))
							return true;
					}
				}

				if (DoLog)
					Log.Debug("IP Address not found among network adapters: " + Address.ToString());

				return false;
			}
			catch (Exception ex)
			{
				if (DoLog)
					Log.Debug(ex.Message);

				return false;
			}
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
		/// Current Login Auditor. Should be used by modules accepting user logins, to protect the system from
		/// unauthorized access by malicious users.
		/// </summary>
		public static LoginAuditor LoginAuditor
		{
			get { return loginAuditor; }
		}

		/// <summary>
		/// Makes sure a request is being made from a session with a successful user login.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		public static void AssertUserAuthenticated(HttpRequest Request)
		{
			if (Request.Session is null || !Request.Session.TryGetVariable("User", out Variable v) || !(v.ValueObject is IUser))
				throw new ForbiddenException("Access denied.");
		}

		#endregion

		#region Thing Registry

		private static Task ThingRegistryClient_Claimed(object Sender, ClaimedEventArgs e)
		{
			if (e.Node.IsEmpty)
			{
				ownerJid = e.JID;
				Log.Informational("Gateway has been claimed.", ownerJid, new KeyValuePair<string, object>("Public", e.IsPublic));
			}

			return Task.CompletedTask;
		}

		private static Task ThingRegistryClient_Disowned(object Sender, Networking.XMPP.Provisioning.NodeEventArgs e)
		{
			if (e.Node.IsEmpty)
			{
				Log.Informational("Gateway has been disowned.", ownerJid);
				ownerJid = string.Empty;
				Task.Run(Register);
			}

			return Task.CompletedTask;
		}

		private static Task ThingRegistryClient_Removed(object Sender, Networking.XMPP.Provisioning.NodeEventArgs e)
		{
			if (e.Node.IsEmpty)
				Log.Informational("Gateway has been removed from the public registry.", ownerJid);

			return Task.CompletedTask;
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

			if (!(GetMetaData is null))
				MetaData = await GetMetaData(MetaData);

			thingRegistryClient.RegisterThing(MetaData, async (sender2, e2) =>
			{
				if (e2.Ok)
				{
					registered = true;

					if (e2.IsClaimed)
						ownerJid = e2.OwnerJid;
					else
						ownerJid = string.Empty;

					RegistrationEventHandler h = RegistrationSuccessful;
					if (!(h is null))
						await h(MetaData, e2);
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
		/// XMPP Multi-User Chat Protocol (MUC) Client.
		/// </summary>
		public static MultiUserChatClient MucClient
		{
			get { return mucClient; }
		}

		/// <summary>
		/// XMPP Publish/Subscribe (PubSub) Client, if such a component is available on the XMPP broker.
		/// </summary>
		public static PubSubClient PubSubClient
		{
			get { return pepClient.PubSubClient; }
		}

		/// <summary>
		/// XMPP Software Updates Client, if such a compoent is available on the XMPP broker.
		/// </summary>
		public static SoftwareUpdateClient SoftwareUpdateClient
		{
			get { return softwareUpdateClient; }
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
		/// Returns a non-negative random integer that is less than the specified maximum.
		/// </summary>
		/// <param name="Max">The exclusive upper bound of the random number to be generated. maxValue must
		/// be greater than or equal to 0.</param>
		/// <returns>A 32-bit signed integer that is greater than or equal to 0, and less than <paramref name="Max"/>;
		/// that is, the range of return values ordinarily includes 0 but not <paramref name="Max"/>. However,
		/// if <paramref name="Max"/> equals 0, <paramref name="Max"/> is returned.</returns>
		public static int NextInteger(int Max)
		{
			if (Max < 0)
				throw new ArgumentOutOfRangeException("Must be non-negative.", nameof(Max));

			if (Max == 0)
				return 0;

			int Result;

			do
			{
				Result = (int)(NextDouble() * Max);
			}
			while (Result >= Max);

			return Result;
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

		#region Backups

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
						await DoBackup();

						Result = true;
					}
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}

			return Result;
		}

		/// <summary>
		/// Performs a backup of the system.
		/// </summary>
		/// <returns></returns>
		public static async Task DoBackup()
		{
			DateTime Now = DateTime.Now;

			await Export.SetLastBackupAsync(Now);

			KeyValuePair<string, IExportFormat> Exporter = StartExport.GetExporter("Encrypted", false, new string[0]);
			string FileName = Exporter.Key;

			ExportFormat.UpdateClientsFileUpdated(FileName, -1, Now);

			List<string> Folders = new List<string>();

			foreach (Export.FolderCategory FolderCategory in Export.GetRegisteredFolders())
				Folders.AddRange(FolderCategory.Folders);

			await StartExport.DoExport(Exporter.Value, true, false, true, Folders.ToArray());

			long KeepDays = await Export.GetKeepDaysAsync();
			long KeepMonths = await Export.GetKeepMonthsAsync();
			long KeepYears = await Export.GetKeepYearsAsync();
			string ExportFolder = Export.FullExportFolder;
			string KeyFolder = Export.FullKeyExportFolder;

			DeleteOldFiles(ExportFolder, KeepDays, KeepMonths, KeepYears, Now);
			if (ExportFolder != KeyFolder)
				DeleteOldFiles(KeyFolder, KeepDays, KeepMonths, KeepYears, Now);

			OnAfterBackup?.Invoke(typeof(Gateway), new EventArgs());
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

		private static void Database_CollectionRepaired(object Sender, CollectionRepairedEventArgs e)
		{
			StringBuilder Msg = new StringBuilder();

			Msg.Append("Collection repaired: ");
			Msg.AppendLine(e.Collection);

			if (!(e.Flagged is null))
			{
				foreach (FlagSource Source in e.Flagged)
				{
					Msg.AppendLine();
					Msg.Append("Reason: ");
					Msg.Append(MarkdownDocument.Encode(Source.Reason));

					if (Source.Count > 1)
					{
						Msg.Append(" (");
						Msg.Append(Source.Count.ToString());
						Msg.Append(" times)");
					}

					Msg.AppendLine();
					Msg.AppendLine();
					Msg.AppendLine("StackTrace:");
					Msg.AppendLine();
					Msg.AppendLine("```");
					Msg.AppendLine(Source.StackTrace);
					Msg.AppendLine("```");
				}
			}

			Log.Alert(Msg.ToString(), e.Collection);
		}

		#endregion

		#region Notifications

		private static async Task MailClient_MailReceived(object Sender, MailEventArgs e)
		{
			MailEventHandler h = MailReceived;
			if (!(h is null))
				await h(Sender, e);
		}

		/// <summary>
		/// Event raised when a mail has been received.
		/// </summary>
		public static event MailEventHandler MailReceived = null;

		/// <summary>
		/// Sends a graph as a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Graph">Graph to send.</param>
		/// <param name="Settings">Graph settings.</param>
		public static void SendNotification(Graph Graph, GraphSettings Settings)
		{
			using (SKImage Image = Graph.CreateBitmap(Settings))
			{
				Gateway.SendNotification(Image);
			}
		}

		/// <summary>
		/// Sends an image as a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Image">Image to send.</param>
		public static void SendNotification(SKImage Image)
		{
			byte[] Data = InternetContent.Encode(Image, null, out string ContentType);
			StringBuilder sb = new StringBuilder();

			sb.Append("<figure>");
			sb.Append("<img border=\"2\" width=\"");
			sb.Append(Image.Width.ToString());
			sb.Append("\" height=\"");
			sb.Append(Image.Height.ToString());
			sb.Append("\" src=\"data:");
			sb.Append(ContentType);
			sb.Append(";base64,");
			sb.Append(Convert.ToBase64String(Data, 0, Data.Length));
			sb.Append("\" />");
			sb.Append("</figure>");

			SendNotification(sb.ToString());
		}

		/// <summary>
		/// Sends a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Markdown">Markdown of message.</param>
		public static void SendNotification(string Markdown)
		{
			SendNotification(Markdown, string.Empty, false);
		}

		/// <summary>
		/// Sends a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Markdown">Markdown of message.</param>
		/// <param name="MessageId">Message ID</param>
		public static void SendNotification(string Markdown, string MessageId)
		{
			SendNotification(Markdown, MessageId, false);
		}

		/// <summary>
		/// Sends a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Markdown">Markdown of message.</param>
		/// <param name="MessageId">Message ID</param>
		public static void SendNotificationUpdate(string Markdown, string MessageId)
		{
			SendNotification(Markdown, MessageId, true);
		}

		/// <summary>
		/// Sends a notification message to configured notification recipients.
		/// </summary>
		/// <param name="Markdown">Markdown of message.</param>
		/// <param name="MessageId">Message ID</param>
		/// <param name="Update">If its an update notification</param>
		private static void SendNotification(string Markdown, string MessageId, bool Update)
		{
			try
			{
				CaseInsensitiveString[] Addresses = GetNotificationAddresses();
				(string Text, string Html) = ConvertMarkdown(Markdown);

				foreach (CaseInsensitiveString Admin in Addresses)
					SendNotification(Admin, Markdown, Text, Html, MessageId, Update);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static (string, string) ConvertMarkdown(string Markdown)
		{
			MarkdownSettings Settings = new MarkdownSettings()
			{
				ParseMetaData = false
			};
			MarkdownDocument Doc = new MarkdownDocument(Markdown, Settings);
			string Text = Doc.GeneratePlainText();
			string Html = HtmlDocument.GetBody(Doc.GenerateHTML());

			return (Text, Html);
		}

		/// <summary>
		/// Returns configured notification addresses.
		/// </summary>
		/// <returns>Array of addresses</returns>
		public static CaseInsensitiveString[] GetNotificationAddresses()
		{
			return NotificationConfiguration.Instance?.Addresses ?? new CaseInsensitiveString[0];
		}

		private static void SendNotification(string To, string Markdown, string Text, string Html, string MessageId, bool Update)
		{
			if (Gateway.XmppClient != null && Gateway.XmppClient.State == XmppState.Connected)
			{
				RosterItem Item = Gateway.XmppClient.GetRosterItem(To);
				if (Item is null || (Item.State != SubscriptionState.To && Item.State != SubscriptionState.Both))
				{
					xmppClient.RequestPresenceSubscription(To);
					ScheduleEvent(Resend, DateTime.Now.AddMinutes(15), new object[] { To, Markdown, Text, Html, MessageId, Update });
				}
				else
					SendChatMessage(Markdown, Text, Html, To, MessageId, Update);
			}
			else
				ScheduleEvent(Resend, DateTime.Now.AddSeconds(30), new object[] { To, Markdown, Text, Html, MessageId, Update });
		}

		/// <summary>
		/// Sends a chat message to a recipient.
		/// </summary>
		/// <param name="Markdown">Markdown containing text to send.</param>
		/// <param name="To">Recipient of chat message.</param>
		public static void SendChatMessage(string Markdown, string To)
		{
			SendChatMessage(Markdown, To, string.Empty);
		}

		/// <summary>
		/// Sends a chat message to a recipient.
		/// </summary>
		/// <param name="Markdown">Markdown containing text to send.</param>
		/// <param name="To">Recipient of chat message.</param>
		/// <param name="MessageId">Message ID to use.</param>
		public static void SendChatMessage(string Markdown, string To, string MessageId)
		{
			(string Text, string Html) = ConvertMarkdown(Markdown);
			SendChatMessage(Markdown, Text, Html, To, MessageId, false);
		}

		/// <summary>
		/// Sends a chat message update to a recipient.
		/// </summary>
		/// <param name="Markdown">Markdown containing text to send.</param>
		/// <param name="To">Recipient of chat message.</param>
		/// <param name="MessageId">Message ID of message to update.</param>
		public static void SendChatMessageUpdate(string Markdown, string To, string MessageId)
		{
			(string Text, string Html) = ConvertMarkdown(Markdown);
			SendChatMessage(Markdown, Text, Html, To, MessageId, true);
		}

		private static void SendChatMessage(string Markdown, string Text, string Html, string To, string MessageId, bool Update)
		{
			if (Gateway.XmppClient != null && Gateway.XmppClient.State == XmppState.Connected)
			{
				StringBuilder Xml = new StringBuilder();

				Xml.Append("<content xmlns=\"urn:xmpp:content\" type=\"text/markdown\">");
				Xml.Append(XML.Encode(Markdown));
				Xml.Append("</content><html xmlns='http://jabber.org/protocol/xhtml-im'><body xmlns='http://www.w3.org/1999/xhtml'>");

				HtmlDocument Doc = new HtmlDocument("<root>" + Html + "</root>");

				foreach (HtmlNode N in (Doc.Body ?? Doc.Root).Children)
					N.Export(Xml);

				Xml.Append("</body></html>");

				if (Update && !string.IsNullOrEmpty(MessageId))
				{
					Xml.Append("<replace id='");
					Xml.Append(MessageId);
					Xml.Append("' xmlns='urn:xmpp:message-correct:0'/>");

					MessageId = string.Empty;
				}

				xmppClient.SendMessage(QoSLevel.Unacknowledged, MessageType.Chat, MessageId, To, Xml.ToString(), Text,
					string.Empty, string.Empty, string.Empty, string.Empty, null, null);
			}
		}

		/// <summary>
		/// Gets a URL for a resource.
		/// </summary>
		/// <param name="LocalResource">Local resource.</param>
		/// <returns>URL</returns>
		public static string GetUrl(string LocalResource)
		{
			return GetUrl(LocalResource, webServer);
		}

		/// <summary>
		/// Gets a URL for a resource.
		/// </summary>
		/// <param name="LocalResource">Local resource.</param>
		/// <param name="Server">HTTP Server to get URL to. (By default is equal to the HTTP Server hosted by the gateway.)</param>
		/// <returns>URL</returns>
		public static string GetUrl(string LocalResource, HttpServer Server)
		{
			StringBuilder sb = new StringBuilder();
			int DefaultPort;
			int[] Ports;

			sb.Append("http");

			if (certificate is null)
			{
				Ports = Server?.OpenHttpPorts ?? GetConfigPorts("HTTP");
				DefaultPort = 80;
			}
			else
			{
				sb.Append('s');
				Ports = Server?.OpenHttpsPorts ?? GetConfigPorts("HTTPS");
				DefaultPort = 443;
			}

			sb.Append("://");
			if (DomainConfiguration.Instance?.UseDomainName ?? false)
				sb.Append(domain);
			/*else if (httpxProxy?.ServerlessMessaging?.Network.State == PeerToPeerNetworkState.Ready)
				sb.Append(httpxProxy.ServerlessMessaging.Network.ExternalAddress.ToString());
			
				TODO: P2P & Serverless messaging: Recognize HTTP request, and redirect to local HTTP Server, and return response.
			 */
			else
			{
				IPAddress IP4 = null;
				IPAddress IP6 = null;

				if (!(Server is null))
				{
					foreach (IPAddress Addr in Server.LocalIpAddresses)
					{
						if (IPAddress.IsLoopback(Addr))
							continue;

						switch (Addr.AddressFamily)
						{
							case System.Net.Sockets.AddressFamily.InterNetwork:
								if (IP4 is null)
									IP4 = Addr;
								break;

							case System.Net.Sockets.AddressFamily.InterNetworkV6:
								if (IP6 is null)
									IP6 = Addr;
								break;
						}
					}
				}

				if (!(IP4 is null))
					sb.Append(IP4.ToString());
				else if (!(IP6 is null))
					sb.Append(IP6.ToString());
				else
					sb.Append(Dns.GetHostName());
			}

			if (Array.IndexOf<int>(Ports, DefaultPort) < 0 && Ports.Length > 0)
			{
				sb.Append(":");
				sb.Append(Ports[0].ToString());
			}

			sb.Append(LocalResource);

			return sb.ToString();
		}

		private static void Resend(object P)
		{
			object[] P2 = (object[])P;
			SendNotification((string)P2[0], (string)P2[1], (string)P2[2], (string)P2[3], (string)P2[4], (bool)P2[5]);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Simplified configuration.
		/// </summary>
		internal static async Task SimplifiedConfiguration()
		{
			foreach (SystemConfiguration Configuration in configurations)
			{
				if (!Configuration.Complete)
				{
					if (await Configuration.SimplifiedConfiguration())
					{
						await Configuration.MakeCompleted();
						await Database.Update(Configuration);
					}
				}
			}
		}

		/// <summary>
		/// Gets the settings menu.
		/// </summary>
		/// <param name="Request">Current HTTP Request</param>
		/// <param name="UserVariable">Name of user variable</param>
		/// <returns></returns>
		public static WebMenuItem[] GetSettingsMenu(HttpRequest Request, string UserVariable)
		{
			List<WebMenuItem> Result = new List<WebMenuItem>();
			Variables Session = Request.Session;
			if (Session is null)
				return new WebMenuItem[0];

			Language Language = ScriptExtensions.Language.GetLanguageAsync(Session).Result;

			if (Session is null ||
				!Session.TryGetVariable(UserVariable, out Variable v) ||
				!(v.ValueObject is IUser User))
			{
				Result.Add(new WebMenuItem("Login", "/Login.md"));
			}
			else
			{
				if (!(configurations is null))
				{
					foreach (SystemConfiguration Configuration in configurations)
					{
						if (User.HasPrivilege(Configuration.GetType().FullName))
							Result.Add(new WebMenuItem(Configuration.Title(Language).Result, Configuration.Resource));
					}
				}

				if (!Session.TryGetVariable(" AutoLogin ", out v) || !(v.ValueObject is bool AutoLogin) || !AutoLogin)
					Result.Add(new WebMenuItem("Logout", "/Logout"));
			}

			return Result.ToArray();
		}

		#endregion

		#region Smart Contracts

		/// <summary>
		/// XMPP Contracts Client, if such a compoent is available on the XMPP broker.
		/// </summary>
		public static ContractsClient ContractsClient
		{
			get { return contractsClient; }
		}

		/// <summary>
		/// Requests the operator to sign a smart contract.
		/// </summary>
		/// <param name="Contract">Contract to sign. Must be ready to sign.</param>
		/// <param name="Role">Role to sign for. Must be available in contract.</param>
		/// <param name="Purpose">Purpose of contract. Must be one row.</param>
		/// <exception cref="ArgumentException">Any of the arguments are invalid.</exception>
		public static async Task RequestContractSignature(Contract Contract, string Role, string Purpose)
		{
			bool RoleFound = false;

			if (Contract is null)
				throw new ArgumentException("Contract cannot be null.", nameof(Contract));

			// TODO: Check contract server signature is valid.

			foreach (Waher.Networking.XMPP.Contracts.Role R in Contract.Roles)
			{
				if (R.Name == Role)
				{
					RoleFound = true;
					break;
				}
			}

			if (!RoleFound)
				throw new ArgumentException("Invalid role.", nameof(Role));

			if (string.IsNullOrEmpty(Purpose) || Purpose.IndexOfAny(CommonTypes.CRLF) >= 0)
				throw new ArgumentException("Invalid purpose.", nameof(Purpose));

			try
			{
				string Module = string.Empty;
				int Skip = 1;

				while (true)
				{
					StackFrame Frame = new StackFrame(Skip);
					MethodBase Method = Frame.GetMethod();
					if (Method is null)
						break;

					Type Type = Method.DeclaringType;
					Assembly Assembly = Type.Assembly;
					Module = Assembly.GetName().Name;

					if (Type != typeof(Gateway) && !Module.StartsWith("System."))
						break;

					Skip++;
				}

				string Markdown;
				ContractSignatureRequest Request = await Database.FindFirstIgnoreRest<ContractSignatureRequest>(new FilterAnd(
					new FilterFieldEqualTo("ContractId", Contract.ContractId),
					new FilterFieldEqualTo("Role", Role),
					new FilterFieldEqualTo("Module", Module),
					new FilterFieldEqualTo("Provider", Contract.Provider),
					new FilterFieldEqualTo("Purpose", Purpose)));


				if (Request is null)
				{
					Request = new ContractSignatureRequest()
					{
						Contract = Contract,
						Received = DateTime.Now,
						Signed = null,
						ContractId = Contract.ContractId,
						Role = Role,
						Module = Module,
						Provider = Contract.Provider,
						Purpose = Purpose
					};

					await Database.Insert(Request);

					Markdown = File.ReadAllText(Path.Combine(rootFolder, "Settings", "SignatureRequest.md"));

					int i = Markdown.IndexOf("~~~~~~");
					int c = Markdown.Length;

					if (i >= 0)
					{
						i += 6;
						while (i < c && Markdown[i] == '~')
							i++;

						Markdown = Markdown.Substring(i).TrimStart();
					}

					i = Markdown.IndexOf("~~~~~~");
					if (i > 0)
						Markdown = Markdown.Substring(0, i).TrimEnd();

					Variables Variables = new Variables(
						new Variable("RequestId", Request.ObjectId),
						new Variable("Request", Request));
					MarkdownSettings Settings = new MarkdownSettings(emoji1_24x24, false, Variables);
					Markdown = MarkdownDocument.Preprocess(Markdown, Settings);

					StringBuilder sb = new StringBuilder(Markdown);

					sb.AppendLine();
					sb.Append("Link: [`");
					sb.Append(Request.ContractId);
					sb.Append("](");
					sb.Append(GetUrl("/SignatureRequest.md?RequestId=" + Request.ObjectId));
					sb.AppendLine(")");

					Markdown = sb.ToString();
				}
				else
				{
					Markdown = "**Reminder**: Smart Contract [" + Request.ContractId + "](" +
						GetUrl("/SignatureRequest.md?RequestId=" + Request.ObjectId) + ") is waiting for your signature.";
				}

				SendNotification(Markdown);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Petitions information about a legal identity from its owner.
		/// </summary>
		/// <param name="LegalId">ID of petitioned legal identity.</param>
		/// <param name="PetitionId">A petition ID string used to identity request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the legal identity of the current machine.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a legal identity was found that could be used to sign the petition.</returns>
		public static Task<bool> PetitionLegalIdentity(string LegalId, string PetitionId, string Purpose,
			LegalIdentityPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			return LegalIdentityConfiguration.Instance.PetitionLegalIdentity(LegalId, PetitionId, Purpose, Callback, Timeout);
		}

		/// <summary>
		/// Petitions information about a legal identity from its owner.
		/// </summary>
		/// <param name="LegalId">ID of petitioned legal identity.</param>
		/// <param name="PetitionId">A petition ID string used to identity request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the legal identity of the current machine.</param>
		/// <param name="Password">Password of legal identity on the current machine used to sign the petition.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a legal identity was found that could be used to sign the petition, and the password matched (if protected by password).</returns>
		public static Task<bool> PetitionLegalIdentity(string LegalId, string PetitionId, string Purpose, string Password,
			LegalIdentityPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			return LegalIdentityConfiguration.Instance.PetitionLegalIdentity(LegalId, PetitionId, Purpose, Password, Callback, Timeout);
		}

		/// <summary>
		/// Petitions information about a smart contract from its owner.
		/// </summary>
		/// <param name="ContractId">ID of petitioned smart contract.</param>
		/// <param name="PetitionId">A petition ID string used to contract request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the smart contract of the current machine.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a smart contract was found that could be used to sign the petition.</returns>
		public static Task<bool> PetitionContract(string ContractId, string PetitionId, string Purpose,
			ContractPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			return LegalIdentityConfiguration.Instance.PetitionContract(ContractId, PetitionId, Purpose, Callback, Timeout);
		}

		/// <summary>
		/// Petitions information about a smart contract from its owner.
		/// </summary>
		/// <param name="ContractId">ID of petitioned smart contract.</param>
		/// <param name="PetitionId">A petition ID string used to contract request when response is returned.</param>
		/// <param name="Purpose">String containing purpose of petition. Can be seen by owner, as well as the smart contract of the current machine.</param>
		/// <param name="Password">Password of smart contract on the current machine used to sign the petition.</param>
		/// <param name="Callback">Method to call when response is returned. If timed out, or declined, identity will be null.</param>
		/// <param name="Timeout">Maximum time to wait for a response.</param>
		/// <returns>If a smart contract was found that could be used to sign the petition, and the password matched (if protected by password).</returns>
		public static Task<bool> PetitionContract(string ContractId, string PetitionId, string Purpose, string Password,
			ContractPetitionResponseEventHandler Callback, TimeSpan Timeout)
		{
			return LegalIdentityConfiguration.Instance.PetitionContract(ContractId, PetitionId, Purpose, Password, Callback, Timeout);
		}

		#endregion

		#region Finding Files

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="BreakOnFirst">If the search should break when it finds the first file.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(Environment.SpecialFolder[] Folders, string Pattern, bool IncludeSubfolders, bool BreakOnFirst)
		{
			return FindFiles(GetFolders(Folders), Pattern, IncludeSubfolders, BreakOnFirst ? 1 : int.MaxValue);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="BreakOnFirst">If the search should break when it finds the first file.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, bool IncludeSubfolders, bool BreakOnFirst)
		{
			return FindFiles(Folders, Pattern, IncludeSubfolders, BreakOnFirst ? 1 : int.MaxValue);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <param name="MaxCount">Maximum number of files to return.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, bool IncludeSubfolders, int MaxCount)
		{
			return FindFiles(Folders, Pattern, IncludeSubfolders ? int.MaxValue : 0, MaxCount);
		}

		/// <summary>
		/// Finds files in a set of folders, and optionally, their subfolders. This method only finds files in folders
		/// the application as access rights to.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="SubfolderDepth">Maximum folder depth to search.</param>
		/// <param name="MaxCount">Maximum number of files to return.</param>
		/// <returns>Files matching the pattern found in the corresponding folders.</returns>
		public static string[] FindFiles(string[] Folders, string Pattern, int SubfolderDepth, int MaxCount)
		{
			if (MaxCount <= 0)
				throw new ArgumentException("Must be positive.", nameof(MaxCount));

			LinkedList<KeyValuePair<string, int>> ToProcess = new LinkedList<KeyValuePair<string, int>>();
			Dictionary<string, bool> Processed = new Dictionary<string, bool>(StringComparer.CurrentCultureIgnoreCase);
			List<string> Result = new List<string>();
			int Count = 0;

			foreach (string Folder in Folders)
				ToProcess.AddLast(new KeyValuePair<string, int>(Folder, SubfolderDepth));

			while (!(ToProcess.First is null))
			{
				KeyValuePair<string, int> Processing = ToProcess.First.Value;
				string Folder = Processing.Key;
				int Depth = Processing.Value;

				ToProcess.RemoveFirst();
				if (Processed.ContainsKey(Folder))
					continue;

				Processed[Folder] = true;

				try
				{
					string[] Names = Directory.GetFiles(Folder, Pattern, SearchOption.TopDirectoryOnly);

					foreach (string FileName in Names)
					{
						Result.Add(FileName);
						if (++Count >= MaxCount)
							return Result.ToArray();
					}

					if (Depth-- > 0)
					{
						Names = Directory.GetDirectories(Folder);

						foreach (string SubFolder in Names)
							ToProcess.AddLast(new KeyValuePair<string, int>(SubFolder, Depth));
					}
				}
				catch (Exception)
				{
					// Ignore
				}
			}

			return Result.ToArray();
		}

		/// <summary>
		/// Gets the physical locations of special folders.
		/// </summary>
		/// <param name="Folders">Special folders.</param>
		/// <param name="AppendWith">Append result with this array of folders.</param>
		/// <returns>Physical locations. Only the physical locations of defined special folders are returned.</returns>
		public static string[] GetFolders(Environment.SpecialFolder[] Folders, params string[] AppendWith)
		{
			List<string> Result = new List<string>();

			foreach (Environment.SpecialFolder Folder in Folders)
			{
				string Path = Environment.GetFolderPath(Folder, Environment.SpecialFolderOption.None);
				if (!string.IsNullOrEmpty(Path))
				{
					// In 64-bit operating systems, the 32-bit folder can be returned anyway, if the process is running in 32 bit.

					if (Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess)
					{
						switch (Folder)
						{
							case Environment.SpecialFolder.CommonProgramFiles:
							case Environment.SpecialFolder.ProgramFiles:
							case Environment.SpecialFolder.System:
								if (Path.EndsWith(" (x86)"))
								{
									Path = Path.Substring(0, Path.Length - 6);
									if (!Directory.Exists(Path))
										continue;
								}
								break;
						}
					}

					Result.Add(Path);
				}
			}

			foreach (string Path in AppendWith)
				Result.Add(Path);

			return Result.ToArray();
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(Environment.SpecialFolder[] Folders, string Pattern, bool IncludeSubfolders)
		{
			return FindLatestFile(GetFolders(Folders), Pattern, IncludeSubfolders);
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="IncludeSubfolders">If subfolders are to be included.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(string[] Folders, string Pattern, bool IncludeSubfolders)
		{
			return FindLatestFile(Folders, Pattern, IncludeSubfolders ? int.MaxValue : 0);
		}

		/// <summary>
		/// Finds the latest file matching a search pattern, by searching in a set of folders, and optionally, their subfolders. 
		/// This method only finds files in folders the application as access rights to. If no file is found, the empty string
		/// is returned.
		/// </summary>
		/// <param name="Folders">Folders to search in.</param>
		/// <param name="Pattern">Pattern to search for.</param>
		/// <param name="SubfolderDepth">Maximum folder depth to search.</param>
		/// <returns>Latest file if found, empty string if not.</returns>
		public static string FindLatestFile(string[] Folders, string Pattern, int SubfolderDepth)
		{
			string[] Files = FindFiles(Folders, Pattern, SubfolderDepth, int.MaxValue);
			string Result = string.Empty;
			DateTime BestTP = DateTime.MinValue;
			DateTime TP;

			foreach (string FilePath in Files)
			{
				TP = File.GetCreationTimeUtc(FilePath);
				if (TP > BestTP)
				{
					BestTP = TP;
					Result = FilePath;
				}
			}

			return Result;
		}

		#endregion

		#region Custom Errors

		private static readonly Dictionary<string, KeyValuePair<DateTime, MarkdownDocument>> defaultDocuments = new Dictionary<string, KeyValuePair<DateTime, MarkdownDocument>>();

		private static void WebServer_CustomError(object Sender, CustomErrorEventArgs e)
		{
			Networking.HTTP.HeaderFields.HttpFieldAccept Accept = e.Request?.Header?.Accept;
			if (Accept is null || Accept.Value == "*/*")
				return;

			if (Accept.IsAcceptable("text/html"))
			{
				string Html = GetCustomErrorHtml(e.Request, e.StatusCode.ToString() + ".md", e.ContentType, e.Content);

				if (!string.IsNullOrEmpty(Html))
					e.SetContent("text/html; charset=utf-8", Encoding.UTF8.GetBytes(Html));
			}
		}

		/// <summary>
		/// Gets a custom error HTML document.
		/// </summary>
		/// <param name="Request">Request for which the error document is to be prepared.</param>
		/// <param name="LocalFileName">Local file name of the custom error markdown document.</param>
		/// <param name="ContentType">Content-Type of embedded content.</param>
		/// <param name="Content">Binary encoding of embedded content.</param>
		/// <returns>HTML document, if it could be found and generated, or null otherwise.</returns>
		public static string GetCustomErrorHtml(HttpRequest Request, string LocalFileName, string ContentType, byte[] Content)
		{
			bool IsText;
			bool IsMarkdown;
			bool IsEmpty;

			if (string.IsNullOrEmpty(ContentType))
			{
				IsText = IsMarkdown = false;
				IsEmpty = true;
			}
			else
			{
				IsText = ContentType.StartsWith("text/plain");
				IsMarkdown = ContentType.StartsWith("text/markdown");
				IsEmpty = false;
			}

			if (IsEmpty || IsText || IsMarkdown)
			{
				MarkdownDocument Doc;
				DateTime TP;

				lock (defaultDocuments)
				{
					if (defaultDocuments.TryGetValue(LocalFileName, out KeyValuePair<DateTime, MarkdownDocument> P))
					{
						TP = P.Key;
						Doc = P.Value;
					}
					else
					{
						TP = DateTime.MinValue;
						Doc = null;
					}
				}

				string FullFileName = Path.Combine(appDataFolder, "Default", LocalFileName);

				if (File.Exists(FullFileName))
				{
					DateTime TP2 = File.GetLastWriteTime(FullFileName);
					MarkdownSettings Settings;
					MarkdownDocument Detail;
					string Markdown;

					if (Doc is null || TP2 > TP)
					{
						Markdown = File.ReadAllText(FullFileName);
						Settings = new MarkdownSettings(emoji1_24x24, true)
						{
							RootFolder = rootFolder,
							Variables = Request.Session ?? new Variables()
						};

						if (!Settings.Variables.ContainsVariable("Request"))
							Settings.Variables["Request"] = Request;

						Doc = new MarkdownDocument(Markdown, Settings, RootFolder, string.Empty, string.Empty);

						lock (defaultDocuments)
						{
							defaultDocuments[LocalFileName] = new KeyValuePair<DateTime, MarkdownDocument>(TP2, Doc);
						}
					}

					if (IsEmpty || Content is null)
						Detail = null;
					else
					{
						Encoding Encoding = null;
						int i = ContentType.IndexOf(';');

						if (i > 0)
						{
							KeyValuePair<string, string>[] Fields = CommonTypes.ParseFieldValues(ContentType.Substring(i + 1).TrimStart());

							foreach (KeyValuePair<string, string> Field in Fields)
							{
								if (Field.Key.ToUpper() == "CHARSET")
									Encoding = InternetContent.GetEncoding(Field.Value);
							}
						}

						if (Encoding is null)
							Encoding = System.Text.Encoding.UTF8;

						Markdown = CommonTypes.GetString(Content, Encoding);
						if (IsText)
						{
							MarkdownSettings Settings2 = new MarkdownSettings(null, false);
							Detail = new MarkdownDocument("```\r\n" + Markdown + "\r\n```", Settings2);
						}
						else
							Detail = new MarkdownDocument(Markdown, Doc.Settings);
					}

					lock (Doc)
					{
						Doc.Detail = Detail;
						return Doc.GenerateHTML();
					}
				}
				else
				{
					if (!(Doc is null))
					{
						lock (defaultDocuments)
						{
							defaultDocuments.Remove(LocalFileName);
						}
					}
				}
			}

			return null;
		}

		#endregion
	}
}
