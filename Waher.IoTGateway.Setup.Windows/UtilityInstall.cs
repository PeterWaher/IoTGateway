using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Runtime.Collections;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Security.SHA3;
using static System.Environment;

namespace Waher.Utility.Install
{
	/// <summary>
	/// Installs a module in an IoT Gateway, or generates a module package file.
	/// 
	/// Command line switches:
	/// 
	/// -m MANIFEST_FILE     Points to a manifest file describing the files in the module.
	///                      If multiple manifest files are referened, they are processed
	///                      in the order they are listed.
	/// -p PACKAGE_FILE      If provided together with a manifest file, files will be
	///                      packed into a package file that can be easily distributed,
	///                      instead of being installed on the local machine.
	///                      If a manifest file is not specified, the package file will
	///                      be used instead.
	/// -d APP_DATA_FOLDER   Points to the application data folder. Required if
	///                      installing a module.
	/// -a ALT_DATA_FOLDER   Points to an alternative data folder where files will be copied.
	///                      If not provided, it will default to the application folder
	///                      where the server executable is stored.
	/// -s SERVER_EXE        Points to the executable file of the IoT Gateway. Required
	///                      if installing a module.
	/// -k KEY               Encryption key used for the package file. Secret used in
	///                      encryption is based on the local package file name and the
	///                      KEY parameter, if provided. You cannot rename a package file.
	/// -x CATEGORY          If working with manifest files, excludes a category from
	///                      processing. Categories are case-insensitive.
	/// -dk DOCKER_FILE      If provided together with one or more manifest files, a Docker
	///                      file will be generated. the -d switch specifies a folder within
	///                      the Docker image where content files will be copied. The -s
	///                      switch specifies a full path of the server executable within
	///                      the Docker image. The -a switch must be used to specify an
	///                      alternative data folder to which files will be copied. From
	///                      that folder, only the newer files will be copied to the data
	///                      folder.
	/// -de ENVIRONMENT_FILE If generating a Docker file, this option also generates an
	///                      environment file that allows the user to provide environment
	///                      variables to Docker in a simpler manner.
	/// -v                   Verbose mode.
	/// -i                   Install. This is the default. Switch not required.
	/// -u                   Uninstall. Add this switch if the module is being uninstalled.
	/// -r                   Remove files. Add this switch if you want files removed during
	///                      uninstallation. Default is to not remove files.
	/// -n INSTANCE          Name of instance. Default is the empty string. Parallel instances
	///                      of the IoT Gateway can execute, provided they are given separate
	///                      instance names. This property is used in conjunction with the -w
	///                      property.
	/// -w MILLISECONDS      Waits for the Gateway to stop executing before performing the
	///                      operation. If the gateway does not stop within this period of
	///                      time, the operation fails. (Default=60000)
	/// -co                  If only content (content only) should be installed.
	/// -sn SERVICE_NAME     If provided, the utility will attempt to start the service with
	///                      the given service name before exiting.
	/// -?                   Help.
	/// 
	/// Note: Alternating -p and -k attributes can be used to process multiple packages in
	///       one operation.
	/// </summary>
	/// <example>
	/// Packs assembly and content files defined in a manifest file into an encrypted package file:
	/// 
	/// Waher.Utility.Install.exe -m Waher.IoTGateway.Svc.manifest -p Waher.IoTGateway.Svc.package -k Testing -v
	/// </example>
	/// <example>
	/// Unpacks and installs files from an encrypted package file:
	/// 
	/// Waher.Utility.Install.exe -i -p Waher.IoTGateway.Svc.package -k Testing -v -d "\ProgramData\IoT Gateway Dev" -s Waher.IoTGateway.Svc.exe
	/// </example>
	public class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				Dictionary<string, bool> ExcludeCategories = new(StringComparer.InvariantCultureIgnoreCase);
				LinkedList<KeyValuePair<string, string>> Packages = new();
				List<string> ManifestFiles = new();
				string ProgramDataFolder = null;
				string AlternativeDataFolder = null;
				string ServerApplication = null;
				string AppFolder = null;
				string PackageFile = null;
				string DockerFile = null;
				string EnvironmentFile = null;
				string Key = string.Empty;
				string Suffix = string.Empty;
				string ServiceName = string.Empty;
				int i = 0;
				int c = args.Length;
				int? Timeout = null;
				string s;
				bool Help = false;
				bool Verbose = false;
				bool UninstallService = false;
				bool RemoveFiles = false;
				bool ContentOnly = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-m":
							if (i >= c)
								throw new Exception("Missing manifest file.");

							ManifestFiles.Add(args[i++]);
							break;

						case "-d":
							if (i >= c)
								throw new Exception("Missing program data folder.");

							if (string.IsNullOrEmpty(ProgramDataFolder))
								ProgramDataFolder = args[i++];
							else
								throw new Exception("Only one program data folder allowed.");
							break;

						case "-a":
							if (i >= c)
								throw new Exception("Missing alternative data folder.");

							if (string.IsNullOrEmpty(AlternativeDataFolder))
								AlternativeDataFolder = args[i++];
							else
								throw new Exception("Only one alternative data folder allowed.");
							break;

						case "-s":
							if (i >= c)
								throw new Exception("Missing server application.");

							if (string.IsNullOrEmpty(ServerApplication))
							{
								ServerApplication = args[i++];
								AppFolder = Path.GetDirectoryName(ServerApplication);
							}
							else
								throw new Exception("Only one server application allowed.");
							break;

						case "-p":
							if (i >= c)
								throw new Exception("Missing package file name.");

							if (!string.IsNullOrEmpty(PackageFile))
							{
								Packages.AddLast(new KeyValuePair<string, string>(PackageFile, Key));
								Key = string.Empty;
							}

							PackageFile = args[i++];
							break;

						case "-k":
							if (i >= c)
								throw new Exception("Missing key.");

							if (string.IsNullOrEmpty(Key))
							{
								Key = args[i++];
								if (!string.IsNullOrEmpty(PackageFile))
								{
									Packages.AddLast(new KeyValuePair<string, string>(PackageFile, Key));
									PackageFile = null;
									Key = string.Empty;
								}
							}
							else
								throw new Exception("Only one key per package allowed.");
							break;

						case "-dk":
							if (i >= c)
								throw new Exception("Missing Docker file name.");

							if (!string.IsNullOrEmpty(DockerFile))
								throw new Exception("A Docker file name has already been specified.");

							DockerFile = args[i++];
							break;

						case "-de":
							if (i >= c)
								throw new Exception("Missing Docker environment file name.");

							if (!string.IsNullOrEmpty(EnvironmentFile))
								throw new Exception("A Docker environment file name has already been specified.");

							EnvironmentFile = args[i++];
							break;

						case "-n":
							if (i >= c)
								throw new Exception("Missing instance.");

							if (string.IsNullOrEmpty(Suffix))
							{
								Suffix = args[i++];
								if (!string.IsNullOrEmpty(Suffix))
									Suffix = "." + Suffix;
							}
							else
								throw new Exception("Only one instance allowed.");
							break;

						case "-sn":
							if (i >= c)
								throw new Exception("Missing service name.");

							if (string.IsNullOrEmpty(ServiceName))
								ServiceName = args[i++];
							else
								throw new Exception("Only one service name allowed.");
							break;

						case "-w":
							if (i >= c)
								throw new Exception("Missing wait time.");

							if (Timeout.HasValue)
								throw new Exception("Wait time already specified.");
							else if (!int.TryParse(args[i++], out int j) || j < 0)
								throw new Exception("Invalid wait time.");
							else
								Timeout = j;
							break;

						case "-x":
							if (i >= c)
								throw new Exception("Missing category.");

							ExcludeCategories[args[i++]] = true;
							break;

						case "-co":
							ContentOnly = true;
							break;

						case "-i":
							UninstallService = false;
							break;

						case "-u":
							UninstallService = true;
							break;

						case "-r":
							RemoveFiles = true;
							break;

						case "-v":
							Verbose = true;
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (!string.IsNullOrEmpty(PackageFile))
					Packages.AddLast(new KeyValuePair<string, string>(PackageFile, Key));

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("-m MANIFEST_FILE     Points to a manifest file describing the files in the module.");
					ConsoleOut.WriteLine("                     If multiple manifest files are referened, they are processed");
					ConsoleOut.WriteLine("                     in the order they are listed.");
					ConsoleOut.WriteLine("-p PACKAGE_FILE      If provided together with a manifest file, files will be");
					ConsoleOut.WriteLine("                     packed into a package file that can be easily distributed,");
					ConsoleOut.WriteLine("                     instead of being installed on the local machine.");
					ConsoleOut.WriteLine("                     If a manifest file is not specified, the package file will");
					ConsoleOut.WriteLine("                     be used instead.");
					ConsoleOut.WriteLine("-d APP_DATA_FOLDER   Points to the application data folder. Required if");
					ConsoleOut.WriteLine("                     installing a module.");
					ConsoleOut.WriteLine("-a ALT_DATA_FOLDER   Points to an alternative data folder where files will be copied.");
					ConsoleOut.WriteLine("                     If not provided, it will default to the application folder");
					ConsoleOut.WriteLine("                     where the server executable is stored.");
					ConsoleOut.WriteLine("-s SERVER_EXE        Points to the executable file of the IoT Gateway. Required");
					ConsoleOut.WriteLine("                     if installing a module.");
					ConsoleOut.WriteLine("-k KEY               Encryption key used for the package file. Secret used in");
					ConsoleOut.WriteLine("                     encryption is based on the local package file name and the");
					ConsoleOut.WriteLine("                     KEY parameter, if provided. You cannot rename a package file.");
					ConsoleOut.WriteLine("-x CATEGORY          If working with manifest files, excludes a category from");
					ConsoleOut.WriteLine("                     processing. Categories are case-insensitive.");
					ConsoleOut.WriteLine("-dk DOCKER_FILE      If provided together with one or more manifest files, a Docker");
					ConsoleOut.WriteLine("                     file will be generated. the -d switch specifies a folder within");
					ConsoleOut.WriteLine("                     the Docker image where content files will be copied. The -s");
					ConsoleOut.WriteLine("                     switch specifies a full path of the server executable within");
					ConsoleOut.WriteLine("                     the Docker image. The -a switch must be used to specify an");
					ConsoleOut.WriteLine("                     alternative data folder to which files will be copied. From");
					ConsoleOut.WriteLine("                     that folder, only the newer files will be copied to the data");
					ConsoleOut.WriteLine("                     folder.");
					ConsoleOut.WriteLine("-de ENVIRONMENT_FILE If generating a Docker file, this option also generates an");
					ConsoleOut.WriteLine("                     environment file that allows the user to provide environment");
					ConsoleOut.WriteLine("                     variables to Docker in a simpler manner.");
					ConsoleOut.WriteLine("-v                   Verbose mode.");
					ConsoleOut.WriteLine("-i                   Install. This the default. Switch not required.");
					ConsoleOut.WriteLine("-u                   Uninstall. Add this switch if the module is being uninstalled.");
					ConsoleOut.WriteLine("-r                   Remove files. Add this switch if you want files removed during");
					ConsoleOut.WriteLine("                     uninstallation. Default is to not remove files.");
					ConsoleOut.WriteLine("-n INSTANCE          Name of instance. Default is the empty string. Parallel instances");
					ConsoleOut.WriteLine("                     of the IoT Gateway can execute, provided they are given separate");
					ConsoleOut.WriteLine("                     instance names. This property is used in conjunction with the -w");
					ConsoleOut.WriteLine("                     property.");
					ConsoleOut.WriteLine("-w MILLISECONDS      Waits for the Gateway to stop executing before performing the");
					ConsoleOut.WriteLine("                     operation. If the gateway does not stop within this period of");
					ConsoleOut.WriteLine("                     time, the operation fails. (Default=60000)");
					ConsoleOut.WriteLine("-co                  If only content (content only) should be installed.");
					ConsoleOut.WriteLine("-sn SERVICE_NAME     If provided, the utility will attempt to start the service with");
					ConsoleOut.WriteLine("                     the given service name before exiting.");
					ConsoleOut.WriteLine("-?                   Help.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Note: Alternating -p and -k attributes can be used to process multiple packages in");
					ConsoleOut.WriteLine("      one operation.");
					return 0;
				}

				if (Verbose)
					Log.Register(new Events.Console.ConsoleEventSink());

				Types.Initialize(typeof(Program).Assembly,
					typeof(JSON).Assembly);

				if (string.IsNullOrEmpty(AlternativeDataFolder))
				{
					if (!string.IsNullOrEmpty(DockerFile))
						throw new Exception("Alternative Data Folder must be specified when generating docker files.");

					AlternativeDataFolder = Path.GetDirectoryName(ServerApplication);
				}

				if (string.IsNullOrEmpty(DockerFile))
				{
					Mutex GatewayRunning = null;
					Mutex StartingServer = null;
					bool GatewayRunningLocked = false;
					bool StartingServerLocked = false;

					try
					{
						if (Timeout.HasValue && !ContentOnly)
						{
							if (Verbose)
								ConsoleOut.WriteLine("Making sure server is closed...");

							GatewayRunning = new Mutex(false, "Waher.IoTGateway.Running" + Suffix);
							if (!GatewayRunning.WaitOne(Timeout.Value))
								throw new Exception("The IoT Gateway did not stop within the given time period.");

							GatewayRunningLocked = true;

							StartingServer = new Mutex(false, "Waher.IoTGateway.Starting" + Suffix);
							if (!StartingServer.WaitOne(Timeout.Value))
								throw new Exception("The IoT Gateway is starting in another process, and is unable to stop within the given time period.");

							StartingServerLocked = true;

							if (Verbose)
								ConsoleOut.WriteLine("Server is closed. Proceeding...");
						}

						if (Packages.First is not null)
						{
							if (ManifestFiles.Count == 0)
							{
								if (UninstallService)
									UninstallPackage(Packages, ServerApplication, ProgramDataFolder, RemoveFiles, ContentOnly);
								else
									InstallPackage(Packages, ServerApplication, ProgramDataFolder, ContentOnly, true);
							}
							else if (Packages.Count == 1)
							{
								GeneratePackage(ManifestFiles.ToArray(), Packages.First.Value.Key, Packages.First.Value.Value,
									ExcludeCategories);
							}
							else
								throw new Exception("Only one package file name can be referenced, when generating a package.");
						}
						else
						{
							foreach (string ManifestFile in ManifestFiles)
							{
								if (UninstallService)
									Uninstall(ManifestFile, ServerApplication, ProgramDataFolder, RemoveFiles, ContentOnly, ExcludeCategories);
								else
									Install(ManifestFile, ServerApplication, ProgramDataFolder, ContentOnly, ExcludeCategories);
							}
						}
					}
					finally
					{
						if (GatewayRunning is not null)
						{
							if (GatewayRunningLocked)
								GatewayRunning.ReleaseMutex();

							GatewayRunning.Dispose();
							GatewayRunning = null;
						}

						if (StartingServer is not null)
						{
							if (StartingServerLocked)
								StartingServer.ReleaseMutex();

							StartingServer.Dispose();
							StartingServer = null;
						}
					}
				}
				else
				{
					if (UninstallService)
						throw new Exception("Uninstallation not supported when creating Docker files.");

					if (Packages.First is not null)
						throw new Exception("Docker files are generated from manifest files, not package files.");

					if (string.IsNullOrEmpty(ServerApplication))
						throw new Exception("Missing server application.");

					AppFolder = AppFolder.Replace(Path.DirectorySeparatorChar, '/');
					if (!AppFolder.EndsWith('/'))
						AppFolder += "/";

					if (string.IsNullOrEmpty(AlternativeDataFolder))
						throw new Exception("Alternative data folder not specified.");

					if (!AlternativeDataFolder.EndsWith('/'))
						AlternativeDataFolder += "/";

					Dictionary<string, bool> Froms = [];
					Dictionary<string, bool> Volumes = [];
					Dictionary<int, string> TcpPorts = [];
					Dictionary<int, string> UdpPorts = [];
					Dictionary<string, KeyValuePair<string, string>> Variables = [];
					ChunkedList<KeyValuePair<string, string[]>> Commands = [];
					KeyValuePair<string, string[]>? EntryPoint = null;

					foreach (string ManifestFile in ManifestFiles)
					{
						XmlElement Module = LoadManifestFile(ManifestFile);
						foreach (XmlNode N in Module.ChildNodes)
						{
							if (N is not XmlElement E)
								continue;

							switch (E.LocalName)
							{
								case "Content":
								case "Folder":
								case "File":
								case "External":
									continue;   // Handled elsewhere

								case "From":
									string Image = XML.Attribute(E, "image");
									Froms[Image] = true;
									break;

								case "Volume":
									string Path = XML.Attribute(E, "path");
									Volumes[Path] = true;
									break;

								case "Port":
									int PortNumber = XML.Attribute(E, "portNumber", 0);
									string PortType = XML.Attribute(E, "type");
									string Protocol = XML.Attribute(E, "protocol");

									switch (PortType)
									{
										case "TCP":
											TcpPorts[PortNumber] = Protocol;
											break;

										case "UDP":
											UdpPorts[PortNumber] = Protocol;
											break;
									}
									break;

								case "Variable":
									string Name = XML.Attribute(E, "name");
									string Default = XML.Attribute(E, "default");
									string Label = XML.Attribute(E, "label");

									Variables[Name] = new KeyValuePair<string, string>(Label, Default);
									break;

								case "Command":
									string Executable = XML.Attribute(E, "executable");
									ChunkedList<string> Arguments = [];

									foreach (XmlNode N2 in E.ChildNodes)
									{
										if (N2 is XmlElement E2 && E2.LocalName == "Argument")
											Arguments.Add(E2.InnerText);
									}

									Commands.Add(new KeyValuePair<string, string[]>(Executable, Arguments.ToArray()));
									break;

								case "EntryPoint":
									Executable = XML.Attribute(E, "executable");
									Arguments = [];

									foreach (XmlNode N2 in E.ChildNodes)
									{
										if (N2 is XmlElement E2 && E2.LocalName == "Argument")
											Arguments.Add(E2.InnerText);
									}

									EntryPoint = new KeyValuePair<string, string[]>(Executable, Arguments.ToArray());
									break;
							}
						}
					}

					using StreamWriter DockerOutput = File.CreateText(DockerFile);

					foreach (string Image in Froms.Keys)
						DockerOutput.WriteLine("FROM " + Image);

					if (Froms.Count > 0)
						DockerOutput.WriteLine();

					foreach (string Path in Volumes.Keys)
						DockerOutput.WriteLine("VOLUME [\"" + Path + "\"]");

					DockerOutput.WriteLine("WORKDIR \"" + AppFolder + "\"");

					if (Volumes.Count > 0)
						DockerOutput.WriteLine();

					foreach (KeyValuePair<int, string> P in TcpPorts)
					{
						DockerOutput.WriteLine("# " + P.Value);
						DockerOutput.WriteLine("EXPOSE " + P.Key.ToString() + "/tcp");
						DockerOutput.WriteLine();
					}

					foreach (KeyValuePair<int, string> P in UdpPorts)
					{
						DockerOutput.WriteLine("# " + P.Value);
						DockerOutput.WriteLine("EXPOSE " + P.Key.ToString() + "/udp");
						DockerOutput.WriteLine();
					}

					foreach (KeyValuePair<string, KeyValuePair<string, string>> P in Variables)
					{
						DockerOutput.WriteLine("ARG " + P.Key);
						DockerOutput.WriteLine("ENV " + P.Key + "=\"" + P.Value.Value.Replace("\"", "\\\"") + "\"");
						DockerOutput.WriteLine("LABEL env." + P.Key + "=\"" + P.Value.Key.Replace("\"", "\\\"") + "\"");
						DockerOutput.WriteLine();
					}

					string DockerFileFolder = Path.GetDirectoryName(Path.GetFullPath(DockerFile));
					Dictionary<string, Dictionary<string, string>> FilesPerDestinationFolder = [];
					bool HasContentFiles = false;

					foreach (string ManifestFile in ManifestFiles)
					{
						PrepareDockerCopyInstructions(ManifestFile, FilesPerDestinationFolder,
							AlternativeDataFolder, AppFolder, DockerFileFolder, ContentOnly,
							ExcludeCategories, ref HasContentFiles);
					}

					foreach (KeyValuePair<string, Dictionary<string, string>> CopyInstructions in FilesPerDestinationFolder)
					{
						string DestinationPath = CopyInstructions.Key;

						DockerOutput.WriteLine("COPY [ \\");

						foreach (string RelativeSourcePath in CopyInstructions.Value.Values)
						{
							DockerOutput.Write("\t\"");
							DockerOutput.Write(RelativeSourcePath.Replace("\"", "\\\""));
							DockerOutput.WriteLine("\", \\");
						}

						DockerOutput.Write("\t\"");
						DockerOutput.Write(DestinationPath.Replace("\"", "\\\""));
						DockerOutput.WriteLine("\"]");
						DockerOutput.WriteLine();
					}

					if (HasContentFiles && AlternativeDataFolder != ProgramDataFolder)
					{
						WriteCommand(DockerOutput, "RUN", "cp", new string[]
						{
							"-ru", AlternativeDataFolder + ".", ProgramDataFolder
						}, AppFolder, ServerApplication, ProgramDataFolder, AlternativeDataFolder);

						WriteCommand(DockerOutput, "RUN", "rm", new string[]
						{
							"-rf", AlternativeDataFolder
						}, AppFolder, ServerApplication, ProgramDataFolder, AlternativeDataFolder);
					}

					foreach (KeyValuePair<string, string[]> Command in Commands)
					{
						WriteCommand(DockerOutput, "RUN", Command.Key, Command.Value,
							AppFolder, ServerApplication, ProgramDataFolder, AlternativeDataFolder);
					}

					if (EntryPoint.HasValue)
					{
						WriteCommand(DockerOutput, "ENTRYPOINT", EntryPoint.Value.Key,
							EntryPoint.Value.Value, AppFolder, ServerApplication,
							ProgramDataFolder, AlternativeDataFolder);
					}

					DockerOutput.Flush();

					if (!string.IsNullOrEmpty(EnvironmentFile))
					{
						using StreamWriter EnvironmentOutput = File.CreateText(EnvironmentFile);
						int MaxLen = 0;
						int Len;

						foreach (KeyValuePair<string, KeyValuePair<string, string>> P in Variables)
						{
							s = P.Value.Value.Replace("\"", "\\\"");
							Len = P.Key.Length + 3 + s.Length;
							if (Len > MaxLen)
								MaxLen = Len;
						}

						MaxLen += 2;

						foreach (KeyValuePair<string, KeyValuePair<string, string>> P in Variables)
						{
							s = P.Value.Value.Replace("\"", "\\\"");
							Len = P.Key.Length + 3 + s.Length;

							EnvironmentOutput.Write(P.Key);
							EnvironmentOutput.Write("=\"");
							EnvironmentOutput.Write(s);
							EnvironmentOutput.Write("\"");
							EnvironmentOutput.Write(new string(' ', MaxLen - Len));
							EnvironmentOutput.Write("# ");
							EnvironmentOutput.Write(P.Value.Key);
							EnvironmentOutput.WriteLine();
						}

						EnvironmentOutput.Flush();
					}
				}

				if (!string.IsNullOrEmpty(ServiceName))
					StartService(ServiceName);

				return 0;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				ConsoleOut.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				ConsoleOut.Flush(true);
				Log.TerminateAsync().Wait();
			}
		}

		private static void WriteCommand(StreamWriter DockerOutput, string DockerCommand,
			string Command, string[] Arguments, string AppFolder, string ServerApplication,
			string ProgramDataFolder, string AlternativeDataFolder)
		{
			DockerOutput.Write(DockerCommand);
			DockerOutput.Write(" [\"");
			DockerOutput.Write(Command.
				Replace("%AppFolder%", AppFolder).
				Replace("%Executable%", ServerApplication).
				Replace("%DataFolder%", ProgramDataFolder).
				Replace("%AlternativeFolder%", AlternativeDataFolder).
				Replace("\"", "\\\""));
			DockerOutput.Write('"');

			foreach (string Argument in Arguments)
			{
				DockerOutput.Write(", \"");
				DockerOutput.Write(Argument.
					Replace("%AppFolder%", AppFolder).
					Replace("%Executable%", ServerApplication).
					Replace("%DataFolder%", ProgramDataFolder).
					Replace("%AlternativeFolder%", AlternativeDataFolder).
					Replace("\"", "\\\""));
				DockerOutput.Write('"');
			}

			DockerOutput.WriteLine("]");
			DockerOutput.WriteLine();
		}

		public static void StartService(string ServiceName)
		{
#pragma warning disable CA1416 // Validate platform compatibility
			ServiceController service = new(ServiceName);
			service.Start();
#pragma warning restore CA1416 // Validate platform compatibility
		}

		public static AssemblyName GetAssemblyName(string ServerApplication)
		{
			if (ServerApplication.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
				ServerApplication = ServerApplication[0..^4];

			if (!ServerApplication.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
				ServerApplication += ".dll";

			return AssemblyName.GetAssemblyName(ServerApplication);
		}

		public static bool InExcludedCategory(XmlElement E, Dictionary<string, bool> ExcludeCategories)
		{
			if (ExcludeCategories is null)
				return false;

			if (!E.HasAttribute("category"))
				return false;

			string Category = E.GetAttribute("category");

			return ExcludeCategories.ContainsKey(Category);
		}

		private static string GetDepsJsonFileName(string ServerApplication)
		{
			string DepsJsonFileName = ServerApplication;

			if (DepsJsonFileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
				DepsJsonFileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
			{
				DepsJsonFileName = DepsJsonFileName[0..^4];
			}

			return DepsJsonFileName + ".deps.json";
		}

		public static void Install(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool ContentOnly,
			Dictionary<string, bool> ExcludeCategories)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "IoT Gateway");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/usr/local/share");
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/var/lib");

				Log.Informational("Using default program data folder: " + ProgramDataFolder);
			}

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (!File.Exists(ServerApplication))
				throw new Exception("Server application not found: " + ServerApplication);

			Dictionary<string, object> Deps;
			AssemblyName ServerName;
			string DepsJsonFileName;

			if (ContentOnly)
			{
				Deps = null;
				ServerName = null;
				DepsJsonFileName = null;
			}
			else
			{
				Log.Informational("Getting assembly name of server.");
				ServerName = GetAssemblyName(ServerApplication);
				Log.Informational("Server assembly name: " + ServerName.ToString());

				DepsJsonFileName = GetDepsJsonFileName(ServerApplication);
				Log.Informational("deps.json file name: " + DepsJsonFileName);

				if (!File.Exists(DepsJsonFileName))
					throw new Exception("Invalid server executable. No corresponding deps.json file found.");

				Log.Informational("Opening " + DepsJsonFileName);

				string s = File.ReadAllText(DepsJsonFileName);

				Log.Informational("Parsing " + DepsJsonFileName);

				Deps = JSON.Parse(s) as Dictionary<string, object>;
				if (Deps is null)
					throw new Exception("Invalid deps.json file. Unable to install.");
			}

			XmlElement Module = LoadManifestFile(ManifestFile);
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);
			string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

			CopyFileWithOptions(ManifestFile, DestManifestFileName, null, CopyOptions.Always);

			Log.Informational("Source folder: " + SourceFolder);
			Log.Informational("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				if (E.LocalName == "Assembly")
				{
					if (!ContentOnly)
					{
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

						if (FileName.EndsWith(".dll"))
							Log.Informational("Assembly file: " + FileName, string.Empty, string.Empty, "FileCopy");
						else
							Log.Informational("Application file: " + FileName, string.Empty, string.Empty, "FileCopy");

						if (CopyFileWithOptions(SourceFileName, Path.Combine(AppFolder, FileName), null, CopyOptions.IfNewer))
						{
							if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								string PdbFileName = FileName[0..^4] + ".pdb";
								if (File.Exists(PdbFileName))
								{
									Log.Informational("Symbol file: " + PdbFileName, string.Empty, string.Empty, "FileCopy");

									CopyFileWithOptions(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, CopyOptions.IfNewer);
								}
							}
						}

						if (SourceFileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
							SourceFileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							Assembly A = Assembly.LoadFrom(SourceFileName);
							AssemblyName AN = A.GetName();

							if (Deps is not null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
							{
								foreach (KeyValuePair<string, object> P in Targets)
								{
									if (P.Value is Dictionary<string, object> Target)
									{
										foreach (KeyValuePair<string, object> P2 in Target)
										{
											if (P2.Key.StartsWith(ServerName.Name + "/") &&
												P2.Value is Dictionary<string, object> App &&
												App.TryGetValue("dependencies", out object Obj2) &&
												Obj2 is Dictionary<string, object> Dependencies)
											{
												Dependencies[AN.Name] = AN.Version.ToString();
												break;
											}
										}

										Dictionary<string, object> Dependencies2 = new();

										foreach (AssemblyName Dependency in A.GetReferencedAssemblies())
											Dependencies2[Dependency.Name] = Dependency.Version.ToString();

										Dictionary<string, object> Runtime = new()
										{
											{ Path.GetFileName(SourceFileName), new Dictionary<string,object>() }
										};

										Target[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
										{
											{ "dependencies", Dependencies2 },
											{ "runtime", Runtime }
										};
									}
								}
							}

							if (Deps is not null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
							{
								foreach (KeyValuePair<string, object> P in Libraries)
								{
									if (P.Key.StartsWith(AN.Name + "/"))
									{
										Libraries.Remove(P.Key);
										break;
									}
								}

								Libraries[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
								{
									{ "type", "project" },
									{ "serviceable", false },
									{ "sha512", string.Empty }
								};
							}
						}
					}
				}
			}

			CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module, ExcludeCategories);

			if (!ContentOnly)
			{
				Log.Informational("Encoding JSON");
				string s = JSON.Encode(Deps, true);

				Log.Informational("Writing " + DepsJsonFileName);
				File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
			}
		}

		private static bool CopyFileWithOptions(string From, string To, string To2, CopyOptions CopyOptions)
		{
			if (!File.Exists(From))
				throw new FileNotFoundException("File not found: " + From);

			bool Copy1 = From != To;

			if (Copy1 && CopyOptions != CopyOptions.Always && File.Exists(To))
			{
				if (CopyOptions == CopyOptions.IfNotExists)
					Copy1 = false;
				else if (CopyOptions == CopyOptions.IfNewer)
				{
					DateTime ToTP = File.GetLastWriteTimeUtc(To);
					DateTime FromTP = File.GetLastWriteTimeUtc(From);

					if (ToTP >= FromTP)
					{
						if (ToTP > FromTP)
						{
							Log.Warning("Skipping file. Destination folder contains newer version: " + From,
								new KeyValuePair<string, object>("FromTP", FromTP),
								new KeyValuePair<string, object>("ToTP", ToTP),
								new KeyValuePair<string, object>("From", From),
								new KeyValuePair<string, object>("To", To));
						}
						else
						{
							Log.Notice("Skipping file. Destination folder contains same version: " + From,
								new KeyValuePair<string, object>("FromTP", FromTP),
								new KeyValuePair<string, object>("ToTP", ToTP),
								new KeyValuePair<string, object>("From", From),
								new KeyValuePair<string, object>("To", To));
						}

						Copy1 = false;
					}
				}
			}

			if (Copy1)
			{
				Log.Informational("Copying " + From + " to " + To, string.Empty, string.Empty, "FileCopy");
				FileCopyWithRetries(From, To);
			}

			if (!string.IsNullOrEmpty(To2))
			{
				bool Copy2 = From != To2;

				if (Copy2 && CopyOptions != CopyOptions.Always && File.Exists(To2))
				{
					if (CopyOptions == CopyOptions.IfNotExists)
						Copy2 = false;
					else if (CopyOptions == CopyOptions.IfNewer)
					{
						DateTime ToTP = File.GetLastWriteTimeUtc(To2);
						DateTime FromTP = File.GetLastWriteTimeUtc(From);

						if (ToTP >= FromTP)
						{
							if (ToTP > FromTP)
							{
								Log.Warning("Skipping file. Destination folder contains newer version: " + From,
									string.Empty, string.Empty, "FileSkip",
									new KeyValuePair<string, object>("FromTP", FromTP),
									new KeyValuePair<string, object>("ToTP", ToTP),
									new KeyValuePair<string, object>("From", From),
									new KeyValuePair<string, object>("To", To2));
							}
							else
							{
								Log.Notice("Skipping file. Destination folder contains same version: " + From,
									string.Empty, string.Empty, "FileSkip",
									new KeyValuePair<string, object>("FromTP", FromTP),
									new KeyValuePair<string, object>("ToTP", ToTP),
									new KeyValuePair<string, object>("From", From),
									new KeyValuePair<string, object>("To", To2));
							}

							Copy2 = false;
						}
					}
				}

				if (Copy2)
				{
					Log.Informational("Copying " + From + " to " + To2, string.Empty, string.Empty, "FileCopy");
					FileCopyWithRetries(From, To2);
				}
			}

			return true;
		}

		private static void FileCopyWithRetries(string FromFileName, string ToFileName)
		{
			DateTime Start = DateTime.Now;
			IOException LastException = null;

			do
			{
				if (LastException is not null)
					Thread.Sleep(1000);

				try
				{
					File.Copy(FromFileName, ToFileName, true);
					return;
				}
				catch (IOException ex)  // File might be temporarily locked
				{
					LastException = ex;
				}
			}
			while (DateTime.Now.Subtract(Start).TotalSeconds < 30);

			if (LastException is not null)
				ExceptionDispatchInfo.Capture(LastException).Throw();

			throw new IOException("Unable to file file " + FromFileName + " to " + ToFileName);
		}

		private enum CopyOptions
		{
			IfNewer = 3,
			Always = 4,
			IfNotExists = 6
		}

		private static void CopyContent(string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent,
			Dictionary<string, bool> ExcludeCategories)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				switch (E.LocalName)
				{
					case "Content":
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);
						CopyOptions CopyOptions = XML.Attribute(E, "copy", CopyOptions.IfNewer);

						Log.Informational("Content file: " + FileName);

						if (!string.IsNullOrEmpty(DataFolder) && !Directory.Exists(DataFolder))
						{
							Log.Informational("Creating folder " + DataFolder + ".");
							Directory.CreateDirectory(DataFolder);
						}

						if (!string.IsNullOrEmpty(AppFolder) && !Directory.Exists(AppFolder))
						{
							Log.Informational("Creating folder " + AppFolder + ".");
							Directory.CreateDirectory(AppFolder);
						}

						CopyFileWithOptions(SourceFileName,
							Path.Combine(DataFolder, FileName),
							Path.Combine(AppFolder, FileName),
							CopyOptions);
						break;

					case "Folder":
						string Name = XML.Attribute(E, "name");

						string SourceFolder2 = Path.Combine(SourceFolder, Name);
						string AppFolder2 = Path.Combine(AppFolder, Name);
						string DataFolder2 = Path.Combine(DataFolder, Name);

						Log.Informational("Folder: " + Name,
							new KeyValuePair<string, object>("Source", SourceFolder2),
							new KeyValuePair<string, object>("App", AppFolder2),
							new KeyValuePair<string, object>("Data", DataFolder2));

						CopyContent(SourceFolder2, AppFolder2, DataFolder2, E, ExcludeCategories);
						break;

					case "File":
						//(FileName, SourceFileName) = GetFileName(E, SourceFolder);
						//
						//Log.Informational("External program file: " + FileName);
						//
						//if (!string.IsNullOrEmpty(AppFolder) && !Directory.Exists(AppFolder))
						//{
						//	Log.Informational("Creating folder " + AppFolder + ".");
						//	Directory.CreateDirectory(AppFolder);
						//}
						//
						//CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), null, false);
						break;

					case "External":
						//SpecialFolder SpecialFolder = XML.Attribute(E, "folder", SpecialFolder.ProgramFiles);
						//Name = XML.Attribute(E, "name");
						//
						//SourceFolder2 = GetFolderPath(SpecialFolder, Name);
						//AppFolder2 = SourceFolder2;
						//DataFolder2 = Path.Combine(DataFolder, Name);
						//
						//Log.Informational("External Folder: " + Name,
						//	new KeyValuePair<string, object>("Source", SourceFolder2),
						//	new KeyValuePair<string, object>("App", AppFolder2),
						//	new KeyValuePair<string, object>("Data", DataFolder2),
						//	new KeyValuePair<string, object>("SpecialFolder", SpecialFolder));
						//
						//CopyContent(SourceFolder2, AppFolder2, DataFolder2, E);
						break;
				}
			}
		}

		private static string GetFolderPath(SpecialFolder SpecialFolder, string Name)
		{
			string s = Environment.GetFolderPath(SpecialFolder);
			string Result = Path.Combine(s, Name);

			if (Directory.Exists(Result))
				return Result;

			if (s.EndsWith("(x86)"))
			{
				s = s[..^5].TrimEnd();
				string Result2 = Path.Combine(s, Name);

				if (Directory.Exists(Result2))
					return Result2;
			}

			return Result;
		}

		public static void Uninstall(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool Remove, bool ContentOnly,
			Dictionary<string, bool> ExcludeCategories)
		{
			// Same code as for custom action UninstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "IoT Gateway");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/usr/local/share");
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/var/lib");

				Log.Informational("Using default program data folder: " + ProgramDataFolder);
			}

			if (!File.Exists(ServerApplication))
				throw new Exception("Server application not found: " + ServerApplication);

			Dictionary<string, object> Deps;
			AssemblyName ServerName;
			string DepsJsonFileName;

			if (ContentOnly)
			{
				Deps = null;
				DepsJsonFileName = null;
				ServerName = null;
			}
			else
			{
				Log.Informational("Getting assembly name of server");
				ServerName = GetAssemblyName(ServerApplication);
				Log.Informational("Server assembly name: " + ServerName.ToString());

				DepsJsonFileName = GetDepsJsonFileName(ServerApplication);
				Log.Informational("deps.json file name: " + DepsJsonFileName);

				if (!File.Exists(DepsJsonFileName))
					throw new Exception("Invalid server executable. No corresponding deps.json file found.");

				Log.Informational("Opening " + DepsJsonFileName);

				string s = File.ReadAllText(DepsJsonFileName);

				Log.Informational("Parsing " + DepsJsonFileName);

				Deps = JSON.Parse(s) as Dictionary<string, object>;
				if (Deps is null)
					throw new Exception("Invalid deps.json file. Unable to install.");
			}

			XmlElement Module = LoadManifestFile(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);

			Log.Informational("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				if (E.LocalName == "Assembly")
				{
					if (!ContentOnly)
					{
						(string FileName, string AppFileName) = GetFileName(E, AppFolder);

						if (FileName.EndsWith(".dll"))
							Log.Informational("Assembly file: " + FileName, string.Empty, string.Empty, "FileCopy");
						else
							Log.Informational("Application file: " + FileName, string.Empty, string.Empty, "FileCopy");

						if (AppFileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
							AppFileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							Assembly A = Assembly.LoadFrom(AppFileName);
							AssemblyName AN = A.GetName();
							string Key = AN.Name + "/" + AN.Version.ToString();

							if (Deps is not null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
							{
								Targets.Remove(Key);

								foreach (KeyValuePair<string, object> P in Targets)
								{
									if (P.Value is Dictionary<string, object> Target)
									{
										foreach (KeyValuePair<string, object> P2 in Target)
										{
											if (P2.Key.StartsWith(ServerName.Name + "/") &&
												P2.Value is Dictionary<string, object> App &&
												App.TryGetValue("dependencies", out object Obj2) &&
												Obj2 is Dictionary<string, object> Dependencies)
											{
												Dependencies.Remove(AN.Name);
												break;
											}
										}
									}
								}
							}

							if (Deps is not null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
							{
								foreach (KeyValuePair<string, object> P in Libraries)
								{
									if (P.Key.StartsWith(AN.Name + "/"))
									{
										Libraries.Remove(P.Key);
										break;
									}
								}
							}
						}

						if (Remove)
						{
							RemoveFile(AppFileName);
							if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								string PdbFileName = FileName[0..^4] + ".pdb";
								Log.Informational("Symbol file: " + PdbFileName, string.Empty, string.Empty, "FileCopy");
								RemoveFile(PdbFileName);
							}
						}
					}
				}
			}

			if (!ContentOnly)
			{
				Log.Informational("Encoding JSON");
				string s = JSON.Encode(Deps, true);

				Log.Informational("Writing " + DepsJsonFileName);
				File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);

				if (Path.GetDirectoryName(ManifestFile) == AppFolder)
					RemoveFile(ManifestFile);
			}
		}

		private static bool RemoveFile(string FileName)
		{
			if (!File.Exists(FileName))
				return false;

			Log.Informational("Deleting " + FileName, string.Empty, string.Empty, "FileDelete");
			File.Delete(FileName);

			return true;
		}

		public static void GeneratePackage(string[] ManifestFiles, string PackageFile, string Key, Dictionary<string, bool> ExcludeCategories)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(PackageFile))
				throw new Exception("Missing package file.");

			string LocalName = Path.GetFileName(PackageFile);
			SHAKE256 H = new(384);
			byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
			byte[] AesKey = new byte[32];
			byte[] IV = new byte[16];
			Aes Aes = null;
			FileStream fs = null;
			ICryptoTransform AesTransform = null;
			CryptoStream Encrypted = null;
			GZipStream Compressed = null;

			Buffer.BlockCopy(Digest, 0, AesKey, 0, 32);
			Buffer.BlockCopy(Digest, 32, IV, 0, 16);

			try
			{
				Aes = Aes.Create();
				Aes.BlockSize = 128;
				Aes.KeySize = 256;
				Aes.Mode = CipherMode.CBC;
				Aes.Padding = PaddingMode.Zeros;

				fs = File.Create(PackageFile);
				AesTransform = Aes.CreateEncryptor(AesKey, IV);
				Encrypted = new CryptoStream(fs, AesTransform, CryptoStreamMode.Write);
				Compressed = new GZipStream(Encrypted, CompressionLevel.Optimal, false);

				using (RandomNumberGenerator rnd = RandomNumberGenerator.Create())
				{
					byte[] Bin = new byte[1];

					rnd.GetBytes(Bin);
					Compressed.Write(Bin, 0, 1);

					Bin = new byte[Bin[0]];

					rnd.GetBytes(Bin);
					Compressed.Write(Bin, 0, Bin.Length);

					WriteBin(Encoding.ASCII.GetBytes("IoTGatewayPackage"), Compressed);
				}

				foreach (string ManifestFile in ManifestFiles)
				{
					XmlElement Module = LoadManifestFile(ManifestFile);
					string SourceFolder = Path.GetDirectoryName(ManifestFile);

					Log.Informational("Source folder: " + SourceFolder);

					CopyFile(1, ManifestFile, Path.GetFileName(ManifestFile), Compressed);

					foreach (XmlNode N in Module.ChildNodes)
					{
						if (N is not XmlElement E)
							continue;

						if (InExcludedCategory(E, ExcludeCategories))
							continue;

						if (E.LocalName == "Assembly")
						{
							(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

							if (FileName.EndsWith(".dll"))
								Log.Informational("Assembly file: " + FileName, string.Empty, string.Empty, "FileCopy");
							else
								Log.Informational("Application file: " + FileName, string.Empty, string.Empty, "FileCopy");

							if (SourceFileName.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase) ||
								SourceFileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								CopyFile(2, SourceFileName, FileName, Compressed);
							}
							else
								CopyFile(1, SourceFileName, FileName, Compressed);

							if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								string PdbSourceFileName = SourceFileName[0..^4] + ".pdb";
								string PdbFileName = FileName[0..^4] + ".pdb";
								if (File.Exists(PdbSourceFileName))
									CopyFile(1, PdbSourceFileName, PdbFileName, Compressed);
							}
						}
					}

					CopyContent(SourceFolder, Compressed, string.Empty, Module, ExcludeCategories);
				}

				Compressed.WriteByte(0);
			}
			finally
			{
				Compressed?.Flush();
				Encrypted?.Flush();
				fs?.Flush();

				Compressed?.Dispose();
				Encrypted?.Dispose();
				AesTransform?.Dispose();
				Aes?.Dispose();
				fs?.Dispose();
			}
		}

		private static void CopyFile(byte Type, string SourceFileName, string RelativeName, Stream Output)
		{
			Output.WriteByte(Type);

			WriteBin(Encoding.UTF8.GetBytes(RelativeName), Output);

			WriteVarLenUInt((uint)File.GetAttributes(SourceFileName), Output);
			WriteVarLenUInt((ulong)File.GetCreationTimeUtc(SourceFileName).Ticks, Output);
			WriteVarLenUInt((ulong)File.GetLastAccessTimeUtc(SourceFileName).Ticks, Output);
			WriteVarLenUInt((ulong)File.GetLastWriteTimeUtc(SourceFileName).Ticks, Output);

			using FileStream f = File.OpenRead(SourceFileName);

			WriteVarLenUInt((ulong)f.Length, Output);
			f.CopyTo(Output);
		}

		private static void WriteBin(byte[] Bin, Stream Output)
		{
			WriteVarLenUInt((uint)Bin.Length, Output);
			Output.Write(Bin, 0, Bin.Length);
		}

		private static void WriteVarLenUInt(ulong Len, Stream Output)
		{
			byte b;

			do
			{
				b = (byte)(Len & 0x7f);
				Len >>= 7;
				if (Len > 0)
					b |= 0x80;

				Output.WriteByte(b);
			}
			while (Len > 0);
		}

		private static byte[] ReadBin(Stream Input)
		{
			ulong Len = ReadVarLenUInt(Input);
			return ReadBin(Input, Len);
		}

		public static byte[] ReadBin(Stream Input, ulong Len)
		{
			if (Len > int.MaxValue)
				throw new Exception("Invalid package.");

			int c = (int)Len;
			byte[] Result = new byte[c];
			int Pos = 0;

			while (Pos < c)
			{
				int d = Input.Read(Result, Pos, c - Pos);
				if (d <= 0)
					throw new EndOfStreamException("Reading past end-of-file.");

				Pos += d;
			}

			return Result;
		}

		private static ulong ReadVarLenUInt(Stream Input)
		{
			ulong Len = 0;
			int Offset = 0;
			byte b;

			do
			{
				b = ReadByte(Input);

				Len |= ((ulong)(b & 127)) << Offset;
				Offset += 7;
			}
			while ((b & 0x80) != 0);

			return Len;
		}

		private static byte ReadByte(Stream Input)
		{
			int i = Input.ReadByte();
			if (i < 0)
				throw new EndOfStreamException("Reading past end-of-file.");

			return (byte)i;
		}

		private static void CopyContent(string SourceFolder, Stream Output, string RelativeFolder, XmlElement Parent,
			Dictionary<string, bool> ExcludeCategories)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				switch (E.LocalName)
				{
					case "Content":
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);
						CopyOptions CopyOptions = XML.Attribute(E, "copy", CopyOptions.IfNewer);

						Log.Informational("Content file: " + FileName);

						string RelativePath = Path.Combine(RelativeFolder, FileName);
						CopyFile((byte)CopyOptions, SourceFileName, RelativePath, Output);
						break;

					case "Folder":
						string Name = XML.Attribute(E, "name");

						string SourceFolder2 = Path.Combine(SourceFolder, Name);
						string RelativeFolder2 = string.IsNullOrEmpty(RelativeFolder) ? Name : RelativeFolder + Path.DirectorySeparatorChar + Name;

						Log.Informational("Folder: " + Name,
							new KeyValuePair<string, object>("Source", SourceFolder2),
							new KeyValuePair<string, object>("Relative", RelativeFolder2));

						CopyContent(SourceFolder2, Output, RelativeFolder2, E, ExcludeCategories);
						break;

					case "File":
						(FileName, SourceFileName) = GetFileName(E, SourceFolder);

						Log.Informational("External program file: " + FileName);

						RelativePath = Path.Combine(RelativeFolder, FileName);
						CopyFile(5, SourceFileName, RelativePath, Output);
						break;

					case "External":
						SpecialFolder SpecialFolder = XML.Attribute(E, "folder", SpecialFolder.ProgramFiles);
						Name = XML.Attribute(E, "name");

						Output.WriteByte(6);
						WriteBin(Encoding.UTF8.GetBytes(SpecialFolder.ToString()), Output);
						WriteBin(Encoding.UTF8.GetBytes(Name), Output);

						SourceFolder2 = GetFolderPath(SpecialFolder, Name);

						Log.Informational("External Folder: " + Name,
							new KeyValuePair<string, object>("Source", SourceFolder2),
							new KeyValuePair<string, object>("SpecialFolder", SpecialFolder));

						CopyContent(SourceFolder2, Output, string.Empty, E, ExcludeCategories);
						break;
				}
			}
		}

		private static (string, string) GetFileName(XmlElement E, string ReferenceFolder)
		{
			return GetFileName(E, ReferenceFolder, true);
		}

		private static (string, string) GetFileName(XmlElement E, string ReferenceFolder, bool CheckFileExists)
		{
			string FileName = XML.Attribute(E, "fileName");
			string AbsFileName = Path.Combine(ReferenceFolder, FileName);
			string AltFolder = XML.Attribute(E, "altFolder");
			string AltAbsFileName;
			bool AltFolderOk;

			if (string.IsNullOrEmpty(AltFolder))
				AltFolderOk = true;
			else
			{
				if (AltFolderOk = Directory.Exists(AltFolder))
				{
					AltAbsFileName = Path.Combine(AltFolder, FileName);
					if (File.Exists(AltAbsFileName))
						return (FileName, Path.GetFullPath(AltAbsFileName));
				}

				string AltFolder2 = Path.Combine(ReferenceFolder, AltFolder);
				if (Directory.Exists(AltFolder2))
				{
					AltFolderOk = true;
					AltAbsFileName = Path.Combine(AltFolder2, FileName);

					if (File.Exists(AltAbsFileName))
						return (FileName, Path.GetFullPath(AltAbsFileName));
				}
			}

			if (!CheckFileExists || File.Exists(AbsFileName))
				return (FileName, Path.GetFullPath(AbsFileName));

			if (AltFolderOk)
				throw new FileNotFoundException("File not found: " + AbsFileName);
			else
				throw new Exception("Folder not found: " + AltFolder);
		}

		private static void InstallPackage(LinkedList<KeyValuePair<string, string>> Packages, string ServerApplication,
			string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			foreach (KeyValuePair<string, string> Package in Packages)
				InstallPackage(Package.Key, Package.Value, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
		}

		public static void InstallPackage(string PackageFile, string Key, string ServerApplication, string ProgramDataFolder,
			bool ContentOnly, bool ServerExists)
		{
			if (string.IsNullOrEmpty(PackageFile))
				throw new Exception("Missing package file.");

			using FileStream fs = File.OpenRead(PackageFile);

			InstallPackage(PackageFile, fs, Key, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
		}

		public static void InstallPackage(string FileName, Stream Encrypted, string Key, string ServerApplication,
			string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			string LocalName = Path.GetFileName(FileName);
			SHAKE256 H = new(384);
			byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
			byte[] AesKey = new byte[32];
			byte[] IV = new byte[16];

			Buffer.BlockCopy(Digest, 0, AesKey, 0, 32);
			Buffer.BlockCopy(Digest, 32, IV, 0, 16);

			using Aes Aes = Aes.Create();
			Aes.BlockSize = 128;
			Aes.KeySize = 256;
			Aes.Mode = CipherMode.CBC;
			Aes.Padding = PaddingMode.Zeros;

			using ICryptoTransform AesTransform = Aes.CreateDecryptor(AesKey, IV);
			using CryptoStream Decrypted = new(Encrypted, AesTransform, CryptoStreamMode.Read);

			InstallPackage(Decrypted, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
		}

		public static void InstallPackage(Stream Decrypted, string ServerApplication, string ProgramDataFolder,
			bool ContentOnly, bool ServerExists)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "IoT Gateway");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/usr/local/share");
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/var/lib");

				Log.Informational("Using default program data folder: " + ProgramDataFolder);
			}

			if (ServerExists && !File.Exists(ServerApplication))
				throw new Exception("Server application not found: " + ServerApplication);

			Dictionary<string, object> Deps;
			string DepsJsonFileName;
			AssemblyName ServerName;

			if (ContentOnly || !ServerExists)
			{
				ServerName = null;
				Deps = null;
				DepsJsonFileName = null;
			}
			else
			{
				Log.Informational("Getting assembly name of server.");
				ServerName = GetAssemblyName(ServerApplication);
				Log.Informational("Server assembly name: " + ServerName.ToString());

				DepsJsonFileName = GetDepsJsonFileName(ServerApplication);
				Log.Informational("deps.json file name: " + DepsJsonFileName);

				if (!File.Exists(DepsJsonFileName))
					throw new Exception("Invalid server executable. No corresponding deps.json file found.");

				Log.Informational("Opening " + DepsJsonFileName);

				string s = File.ReadAllText(DepsJsonFileName);

				Log.Informational("Parsing " + DepsJsonFileName);

				Deps = JSON.Parse(s) as Dictionary<string, object>;
				if (Deps is null)
					throw new Exception("Invalid deps.json file. Unable to install.");
			}

			Log.Informational("Loading package file.");

			GZipStream Decompressed = null;
			byte[] Buffer = new byte[65536];

			try
			{
				Decompressed = new GZipStream(Decrypted, CompressionMode.Decompress);

				byte b = ReadByte(Decompressed);
				byte[] Bin;

				if (b > 0)
					ReadBin(Decompressed, b);

				Bin = ReadBin(Decompressed);
				if (Encoding.ASCII.GetString(Bin) != "IoTGatewayPackage")
					throw new Exception("Invalid package file.");

				string AppFolder = Path.GetDirectoryName(ServerApplication);
				string ExternalFolder = AppFolder;

				Log.Informational("App folder: " + AppFolder);

				while ((b = ReadByte(Decompressed)) != 0)
				{
					if (b == 6) // External application
					{
						string SpecialFolderName = Encoding.UTF8.GetString(ReadBin(Decompressed));
						string FolderName = Encoding.UTF8.GetString(ReadBin(Decompressed));

						SpecialFolder SpecialFolder = Enum.Parse<SpecialFolder>(SpecialFolderName);
						ExternalFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder), FolderName);

						if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Directory.Exists(ExternalFolder))
							ExternalFolder = ExternalFolder.Replace("/usr/share", "/usr/local/share");
						else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists(ExternalFolder))
							ExternalFolder = ExternalFolder.Replace("/usr/share", "/var/lib");

						if (!Directory.Exists(ExternalFolder))
						{
							try
							{
								Directory.CreateDirectory(ExternalFolder);
							}
							catch (UnauthorizedAccessException)
							{
								ExternalFolder = Path.Combine(AppFolder, SpecialFolderName.ToString(), FolderName);

								if (!Directory.Exists(ExternalFolder))
									Directory.CreateDirectory(ExternalFolder);
							}
						}
					}
					else
					{
						string RelativeName = Encoding.UTF8.GetString(ReadBin(Decompressed));
						FileAttributes Attr = (FileAttributes)ReadVarLenUInt(Decompressed);
						DateTime CreationTimeUtc = new((long)ReadVarLenUInt(Decompressed));
						DateTime LastAccessTimeUtc = new((long)ReadVarLenUInt(Decompressed));
						DateTime LastWriteTimeUtc = new((long)ReadVarLenUInt(Decompressed));
						ulong Bytes = ReadVarLenUInt(Decompressed);
						string FileName;

						switch (b)
						{
							case 1: // Program file in installation folder, not assembly file
							case 2: // Assembly file
								FileName = Path.Combine(AppFolder, RelativeName);

								if (ContentOnly)
								{
									Log.Notice("Skipping file: " + FileName, string.Empty, string.Empty, "FileSkip");
									SkipBytes(Decompressed, Bytes, Buffer);
								}
								else
								{
									if (b == 1)
										Log.Informational("Application file: " + FileName, string.Empty, string.Empty, "FileCopy");
									else
										Log.Informational("Assembly file: " + FileName, string.Empty, string.Empty, "FileCopy");

									CopyFile(Decompressed, FileName, Program.CopyOptions.Always, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);

									if (b == 2)
									{
										Assembly A;

										try
										{
											A = Assembly.LoadFrom(FileName);
										}
										catch (Exception)
										{
											break;  // Ignore. Not a valid assembly that needs to be registered in the deps.json file.
										}

										AssemblyName AN = A.GetName();

										if (Deps is not null &&
											Deps.TryGetValue("targets", out object Obj) &&
											Obj is Dictionary<string, object> Targets)
										{
											foreach (KeyValuePair<string, object> P in Targets)
											{
												if (P.Value is Dictionary<string, object> Target)
												{
													foreach (KeyValuePair<string, object> P2 in Target)
													{
														if (P2.Key.StartsWith(ServerName.Name + "/") &&
															P2.Value is Dictionary<string, object> App &&
															App.TryGetValue("dependencies", out object Obj2) &&
															Obj2 is Dictionary<string, object> Dependencies)
														{
															Dependencies[AN.Name] = AN.Version.ToString();
															break;
														}
													}

													Dictionary<string, object> Dependencies2 = new();

													foreach (AssemblyName Dependency in A.GetReferencedAssemblies())
														Dependencies2[Dependency.Name] = Dependency.Version.ToString();

													Dictionary<string, object> Runtime = new()
													{
														{ Path.GetFileName(FileName), new Dictionary<string,object>() }
													};

													Target[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
													{
														{ "dependencies", Dependencies2 },
														{ "runtime", Runtime }
													};
												}
											}
										}

										if (Deps is not null &&
											Deps.TryGetValue("libraries", out object Obj3) &&
											Obj3 is Dictionary<string, object> Libraries)
										{
											foreach (KeyValuePair<string, object> P in Libraries)
											{
												if (P.Key.StartsWith(AN.Name + "/"))
												{
													Libraries.Remove(P.Key);
													break;
												}
											}

											Libraries[AN.Name + "/" + AN.Version.ToString()] = new Dictionary<string, object>()
											{
												{ "type", "project" },
												{ "serviceable", false },
												{ "sha512", string.Empty }
											};
										}
									}
								}
								break;

							case 3: // Content file (copy if newer)
							case 4: // Content file (always copy)
							case 6: // Content file (if not exists)
								CopyOptions CopyOptions = (CopyOptions)b;

								FileName = Path.Combine(AppFolder, RelativeName);
								Log.Informational("Content file: " + FileName, string.Empty, string.Empty, "FileCopy");

								CopyFile(Decompressed, FileName, CopyOptions.Always, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);

								using (FileStream TempFile = File.OpenRead(FileName))
								{
									FileName = Path.Combine(ProgramDataFolder, RelativeName);
									Log.Informational("Content file: " + FileName, string.Empty, string.Empty, "FileCopy");

									CopyFile(TempFile, FileName, CopyOptions, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);
								}
								break;

							case 5: // External application file
								FileName = Path.Combine(ExternalFolder, RelativeName);
								Log.Informational("External file: " + FileName, string.Empty, string.Empty, "FileCopy");

								CopyFile(Decompressed, FileName, CopyOptions.Always, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);
								break;

							default:
								throw new Exception("Invalid package file.");
						}
					}
				}

				if (!ContentOnly && !string.IsNullOrEmpty(DepsJsonFileName))
				{
					Log.Informational("Encoding JSON");
					string s = JSON.Encode(Deps, true);

					Log.Informational("Writing " + DepsJsonFileName);
					File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
				}
			}
			finally
			{
				Decompressed?.Dispose();
			}
		}

		private static void CopyFile(Stream Input, string OutputFileName, CopyOptions CopyOptions, ulong Bytes, FileAttributes Attr,
			DateTime CreationTimeUtc, DateTime LastAccessTimeUtc, DateTime LastWriteTimeUtc, byte[] Buffer)
		{
			try
			{
				string Folder = Path.GetDirectoryName(OutputFileName);

				if (!string.IsNullOrEmpty(Folder) && !Directory.Exists(Folder))
				{
					Log.Informational("Creating folder " + Folder + ".");
					Directory.CreateDirectory(Folder);
				}

				bool Skip;

				switch (CopyOptions)
				{
					case CopyOptions.Always:
					default:
						Skip = false;
						break;

					case CopyOptions.IfNewer:
						Skip = false;
						if (File.Exists(OutputFileName))
						{
							DateTime ExistingLastWriteTime = File.GetLastWriteTimeUtc(OutputFileName);
							if (ExistingLastWriteTime >= LastWriteTimeUtc)
								Skip = true;
							break;
						}
						break;

					case CopyOptions.IfNotExists:
						Skip = File.Exists(OutputFileName);
						break;
				}

				if (Skip)
				{
					SkipBytes(Input, Bytes, Buffer);
					return;
				}

				FileStream f;

				try
				{
					f = File.Create(OutputFileName);
				}
				catch (Exception ex)
				{
					Log.Error(ex);
					SkipBytes(Input, Bytes, Buffer);
					return;
				}

				try
				{
					ulong c = (ulong)Buffer.Length;

					while (Bytes > 0)
					{
						int d = (int)Math.Min(Bytes, c);
						int e = Input.Read(Buffer, 0, d);

						if (e <= 0)
							throw new EndOfStreamException("Reading past end-of-file.");

						f.Write(Buffer, 0, e);
						Bytes -= (uint)e;
					}
				}
				finally
				{
					f.Dispose();
				}

				File.SetAttributes(OutputFileName, Attr);
				File.SetCreationTimeUtc(OutputFileName, CreationTimeUtc);
				File.SetLastAccessTimeUtc(OutputFileName, LastAccessTimeUtc);
				File.SetLastWriteTimeUtc(OutputFileName, LastWriteTimeUtc);
			}
			catch (Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static void SkipBytes(Stream Input, ulong Bytes, byte[] Buffer)
		{
			int c = Buffer.Length;

			while (Bytes > 0)
			{
				int d = (int)Math.Min(Bytes, (ulong)c);
				int e = Input.Read(Buffer, 0, d);

				if (e <= 0)
					throw new EndOfStreamException("Reading past end-of-file.");

				Bytes -= (uint)e;
			}
		}

		private static void UninstallPackage(LinkedList<KeyValuePair<string, string>> Packages, string ServerApplication,
			string ProgramDataFolder, bool Remove, bool ContentOnly)
		{
			foreach (KeyValuePair<string, string> Package in Packages)
				UninstallPackage(Package.Key, Package.Value, ServerApplication, ProgramDataFolder, Remove, ContentOnly);
		}

		public static void UninstallPackage(string PackageFile, string Key, string ServerApplication, string ProgramDataFolder,
			bool Remove, bool ContentOnly)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(PackageFile))
				throw new Exception("Missing package file.");

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "IoT Gateway");
				if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/usr/local/share");
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !Directory.Exists(ProgramDataFolder))
					ProgramDataFolder = ProgramDataFolder.Replace("/usr/share", "/var/lib");

				Log.Informational("Using default program data folder: " + ProgramDataFolder);
			}

			if (!File.Exists(ServerApplication))
				throw new Exception("Server application not found: " + ServerApplication);

			Dictionary<string, object> Deps;
			string DepsJsonFileName;
			AssemblyName ServerName;

			if (ContentOnly)
			{
				ServerName = null;
				Deps = null;
				DepsJsonFileName = null;
			}
			else
			{
				Log.Informational("Getting assembly name of server.");
				ServerName = GetAssemblyName(ServerApplication);
				Log.Informational("Server assembly name: " + ServerName.ToString());

				DepsJsonFileName = GetDepsJsonFileName(ServerApplication);
				Log.Informational("deps.json file name: " + DepsJsonFileName);

				if (!File.Exists(DepsJsonFileName))
					throw new Exception("Invalid server executable. No corresponding deps.json file found.");

				Log.Informational("Opening " + DepsJsonFileName);

				string s = File.ReadAllText(DepsJsonFileName);

				Log.Informational("Parsing " + DepsJsonFileName);

				Deps = JSON.Parse(s) as Dictionary<string, object>;
				if (Deps is null)
					throw new Exception("Invalid deps.json file. Unable to install.");
			}

			Log.Informational("Loading package file.");

			string LocalName = Path.GetFileName(PackageFile);
			SHAKE256 H = new(384);
			byte[] Digest = H.ComputeVariable(Encoding.UTF8.GetBytes(LocalName + ":" + Key + ":" + typeof(Program).Namespace));
			byte[] AesKey = new byte[32];
			byte[] IV = new byte[16];
			byte[] Buffer = new byte[65536];
			Aes Aes = null;
			FileStream fs = null;
			ICryptoTransform AesTransform = null;
			CryptoStream Decrypted = null;
			GZipStream Decompressed = null;

			System.Buffer.BlockCopy(Digest, 0, AesKey, 0, 32);
			System.Buffer.BlockCopy(Digest, 32, IV, 0, 16);

			try
			{
				Aes = Aes.Create();
				Aes.BlockSize = 128;
				Aes.KeySize = 256;
				Aes.Mode = CipherMode.CBC;
				Aes.Padding = PaddingMode.Zeros;

				fs = File.OpenRead(PackageFile);
				AesTransform = Aes.CreateDecryptor(AesKey, IV);
				Decrypted = new CryptoStream(fs, AesTransform, CryptoStreamMode.Read);
				Decompressed = new GZipStream(Decrypted, CompressionMode.Decompress);

				byte b = ReadByte(Decompressed);
				byte[] Bin;

				if (b > 0)
					ReadBin(Decompressed, b);

				Bin = ReadBin(Decompressed);
				if (Encoding.ASCII.GetString(Bin) != "IoTGatewayPackage")
					throw new Exception("Invalid package file.");

				string AppFolder = Path.GetDirectoryName(ServerApplication);

				Log.Informational("App folder: " + AppFolder);

				while ((b = ReadByte(Decompressed)) != 0)
				{
					if (b == 6)
					{
						ReadBin(Decompressed);
						ReadBin(Decompressed);
					}
					else
					{
						string RelativeName = Encoding.UTF8.GetString(ReadBin(Decompressed));
						ReadVarLenUInt(Decompressed);
						ReadVarLenUInt(Decompressed);
						ReadVarLenUInt(Decompressed);
						ReadVarLenUInt(Decompressed);
						ulong Bytes = ReadVarLenUInt(Decompressed);
						string FileName;

						SkipBytes(Decompressed, Bytes, Buffer);

						switch (b)
						{
							case 1: // Program file in installation folder, not assembly file
							case 2: // Assembly file
								if (!ContentOnly || b == 1)
								{
									FileName = Path.Combine(AppFolder, RelativeName);

									if (b == 2)
									{
										Assembly A = Assembly.LoadFrom(FileName);
										AssemblyName AN = A.GetName();
										Key = AN.Name + "/" + AN.Version.ToString();

										if (Deps is not null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
										{
											Targets.Remove(Key);

											foreach (KeyValuePair<string, object> P in Targets)
											{
												if (P.Value is Dictionary<string, object> Target)
												{
													foreach (KeyValuePair<string, object> P2 in Target)
													{
														if (P2.Key.StartsWith(ServerName.Name + "/") &&
															P2.Value is Dictionary<string, object> App &&
															App.TryGetValue("dependencies", out object Obj2) &&
															Obj2 is Dictionary<string, object> Dependencies)
														{
															Dependencies.Remove(AN.Name);
															break;
														}
													}
												}
											}
										}

										if (Deps is not null &&
											Deps.TryGetValue("libraries", out object Obj3) &&
											Obj3 is Dictionary<string, object> Libraries)
										{
											foreach (KeyValuePair<string, object> P in Libraries)
											{
												if (P.Key.StartsWith(AN.Name + "/"))
												{
													Libraries.Remove(P.Key);
													break;
												}
											}
										}
									}

									if (Remove)
									{
										RemoveFile(FileName);
										if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
										{
											string PdbFileName = FileName[0..^4] + ".pdb";
											RemoveFile(PdbFileName);
										}
									}
								}
								break;

							case 3: // Content file (copy if newer)
							case 4: // Content file (always copy)
							case 5: // External file
								break;

							default:
								throw new Exception("Invalid package file.");
						}
					}
				}

				if (!ContentOnly && !string.IsNullOrEmpty(DepsJsonFileName))
				{
					Log.Informational("Encoding JSON");
					string s = JSON.Encode(Deps, true);

					Log.Informational("Writing " + DepsJsonFileName);
					File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
				}
			}
			finally
			{
				Decompressed?.Dispose();
				Decrypted?.Dispose();
				AesTransform?.Dispose();
				Aes?.Dispose();
				fs?.Dispose();
			}
		}

		private static XmlElement LoadManifestFile(string ManifestFile)
		{
			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = XML.LoadFromFile(ManifestFile, true);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.ModuleManifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			return Manifest["Module"];
		}

		/// <summary>
		/// Generates Docker Instructions from information available in a manifest file.
		/// </summary>
		/// <param name="ManifestFile">File name of manifest file.</param>
		/// <param name="FilesPerDestinationFolder">Files to copy, sorted by destination
		/// folder.</param>
		/// <param name="ProgramDataFolder">Data folder inside Docker container.</param>
		/// <param name="AppFolder">Path to where execitables are copied.</param>
		/// <param name="DockerFileFolder">Folder containing Dockerfile.</param>
		/// <param name="ContentOnly">If only content files should be copied.</param>
		/// <param name="ExcludeCategories">Any categories that should be exluded.</param>
		/// <param name="HasContentFiles">If content files were found.</param>
		public static void PrepareDockerCopyInstructions(string ManifestFile,
			Dictionary<string, Dictionary<string, string>> FilesPerDestinationFolder,
			string ProgramDataFolder, string AppFolder, string DockerFileFolder,
			bool ContentOnly, Dictionary<string, bool> ExcludeCategories,
			ref bool HasContentFiles)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ProgramDataFolder))
				throw new Exception("Program Data folder for Docker image not specified.");

			XmlElement Module = LoadManifestFile(ManifestFile);
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

			PrepareCopyFile(ManifestFile, Path.GetFileName(ManifestFile), DestManifestFileName, DockerFileFolder, FilesPerDestinationFolder);

			Log.Informational("Source folder: " + SourceFolder);
			Log.Informational("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				if (E.LocalName == "Assembly")
				{
					if (!ContentOnly)
					{
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder, false);

						if (FileName.EndsWith(".dll"))
							Log.Informational("Assembly file: " + FileName, string.Empty, string.Empty, "FileCopy");
						else
							Log.Informational("Application file: " + FileName, string.Empty, string.Empty, "FileCopy");

						PrepareCopyFile(SourceFileName, FileName, Path.Combine(AppFolder, FileName), DockerFileFolder, FilesPerDestinationFolder);
						if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							string PdbFileName = FileName[0..^4] + ".pdb";
							if (File.Exists(PdbFileName))
							{
								PrepareCopyFile(Path.Combine(SourceFolder, PdbFileName),
									PdbFileName, Path.Combine(AppFolder, PdbFileName), DockerFileFolder, FilesPerDestinationFolder);
							}
						}
					}
				}
			}

			PrepareCopyContent(SourceFolder, AppFolder, ProgramDataFolder, DockerFileFolder,
				Module, FilesPerDestinationFolder, ExcludeCategories, ref HasContentFiles);
		}

		private static void PrepareCopyFile(string SourcePath, string FileName, string DestFileName,
			string DockerFileFolder, Dictionary<string, Dictionary<string, string>> FilesPerDestinationFolder)
		{
			string RelativeSourcePath = Path.GetRelativePath(DockerFileFolder, SourcePath).Replace(Path.DirectorySeparatorChar, '/');
			string DestinationPath = Path.GetDirectoryName(DestFileName).Replace(Path.DirectorySeparatorChar, '/');

			if (!DestinationPath.EndsWith('/'))
				DestinationPath += "/";

			if (!FilesPerDestinationFolder.TryGetValue(DestinationPath, out Dictionary<string, string> Files))
			{
				Files = [];
				FilesPerDestinationFolder[DestinationPath] = Files;
			}

			Files[FileName] = RelativeSourcePath;
		}

		private static void PrepareCopyContent(string SourceFolder, string AppFolder, string DataFolder,
			string DockerFileFolder, XmlElement Parent, Dictionary<string, Dictionary<string, string>> FilesPerDestinationFolder,
			Dictionary<string, bool> ExcludeCategories, ref bool HasContentFiles)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is not XmlElement E)
					continue;

				if (InExcludedCategory(E, ExcludeCategories))
					continue;

				switch (E.LocalName)
				{
					case "Content":
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder, false);

						Log.Informational("Content file: " + FileName);

						HasContentFiles = true;
						PrepareCopyFile(SourceFileName, FileName, Path.Combine(DataFolder, FileName), DockerFileFolder, FilesPerDestinationFolder);
						PrepareCopyFile(SourceFileName, FileName, Path.Combine(AppFolder, FileName), DockerFileFolder, FilesPerDestinationFolder);
						break;

					case "Folder":
						string Name = XML.Attribute(E, "name");

						string SourceFolder2 = Path.Combine(SourceFolder, Name);
						string AppFolder2 = Path.Combine(AppFolder, Name);
						string DataFolder2 = Path.Combine(DataFolder, Name);

						Log.Informational("Folder: " + Name,
							new KeyValuePair<string, object>("Source", SourceFolder2),
							new KeyValuePair<string, object>("App", AppFolder2),
							new KeyValuePair<string, object>("Data", DataFolder2));

						PrepareCopyContent(SourceFolder2, AppFolder2, DataFolder2, DockerFileFolder, E, FilesPerDestinationFolder, ExcludeCategories, ref HasContentFiles);
						break;

					case "File":
						//(FileName, SourceFileName) = GetFileName(E, SourceFolder, false);
						//
						//Log.Informational("External program file: " + FileName);
						//
						//if (!string.IsNullOrEmpty(AppFolder) && !Directory.Exists(AppFolder))
						//{
						//	Log.Informational("Creating folder " + AppFolder + ".");
						//	Directory.CreateDirectory(AppFolder);
						//}
						//
						//CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), null, false);
						break;

					case "External":
						//SpecialFolder SpecialFolder = XML.Attribute(E, "folder", SpecialFolder.ProgramFiles);
						//Name = XML.Attribute(E, "name");
						//
						//SourceFolder2 = GetFolderPath(SpecialFolder, Name);
						//AppFolder2 = SourceFolder2;
						//DataFolder2 = Path.Combine(DataFolder, Name);
						//
						//Log.Informational("External Folder: " + Name,
						//	new KeyValuePair<string, object>("Source", SourceFolder2),
						//	new KeyValuePair<string, object>("App", AppFolder2),
						//	new KeyValuePair<string, object>("Data", DataFolder2),
						//	new KeyValuePair<string, object>("SpecialFolder", SpecialFolder));
						//
						//CopyContent(SourceFolder2, AppFolder2, DataFolder2, E);
						break;

					case "From":
					case "Volume":
					case "Port":
					case "Variable":
					case "Command":
					case "EntryPoint":
						break;  // Handled elsewhere
				}
			}
		}

	}
}
