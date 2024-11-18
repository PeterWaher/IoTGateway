using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
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
	///                      the Docker image.
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
				string ServerApplication = null;
				string PackageFile = null;
				string DockerFile = null;
				string Key = string.Empty;
				string Suffix = string.Empty;
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

						case "-s":
							if (i >= c)
								throw new Exception("Missing server application.");

							if (string.IsNullOrEmpty(ServerApplication))
								ServerApplication = args[i++];
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
					ConsoleOut.WriteLine("                     the Docker image.");
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


					using StreamWriter DockerOutput = File.CreateText(DockerFile);

					foreach (string ManifestFile in ManifestFiles)
					{
						GenerateDockerInstructions(ManifestFile, DockerOutput, ProgramDataFolder, ServerApplication, ContentOnly,
							ExcludeCategories);
					}

					DockerOutput.Flush();
				}

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
				Log.Terminate();
			}
		}

		public static AssemblyName GetAssemblyName(string ServerApplication)
		{
			if (ServerApplication.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
				ServerApplication = ServerApplication[0..^4] + ".dll";

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

				int i = ServerApplication.LastIndexOf('.');
				if (i < 0)
					DepsJsonFileName = ServerApplication;
				else
					DepsJsonFileName = ServerApplication[..i];

				DepsJsonFileName += ".deps.json";

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

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = new()
			{
				PreserveWhitespace = true
			};
			Manifest.Load(ManifestFile);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.ModuleManifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			XmlElement Module = Manifest["Module"];
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);
			string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

			CopyFileIfNewer(ManifestFile, DestManifestFileName, null, false);

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

						if (CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), null, true))
						{
							if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								string PdbFileName = FileName[0..^4] + ".pdb";
								if (File.Exists(PdbFileName))
								{
									Log.Informational("Symbol file: " + PdbFileName, string.Empty, string.Empty, "FileCopy");

									CopyFileIfNewer(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, true);
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

		private static bool CopyFileIfNewer(string From, string To, string To2, bool OnlyIfNewer)
		{
			if (!File.Exists(From))
				throw new FileNotFoundException("File not found: " + From);

			bool Copy1 = From != To;

			if (Copy1 && OnlyIfNewer && File.Exists(To))
			{
				DateTime ToTP = File.GetLastWriteTimeUtc(To);
				DateTime FromTP = File.GetLastWriteTimeUtc(From);

				if (ToTP >= FromTP)
				{
					Log.Warning("Skipping file. Destination folder contains newer version: " + From,
						new KeyValuePair<string, object>("FromTP", FromTP),
						new KeyValuePair<string, object>("ToTP", ToTP),
						new KeyValuePair<string, object>("From", From),
						new KeyValuePair<string, object>("To", To));
					Copy1 = false;
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

				if (Copy2 && OnlyIfNewer && File.Exists(To2))
				{
					DateTime ToTP = File.GetLastWriteTimeUtc(To2);
					DateTime FromTP = File.GetLastWriteTimeUtc(From);

					if (ToTP >= FromTP)
					{
						Log.Warning("Skipping file. Destination folder contains newer version: " + From,
							string.Empty, string.Empty, "FileSkip",
							new KeyValuePair<string, object>("FromTP", FromTP),
							new KeyValuePair<string, object>("ToTP", ToTP),
							new KeyValuePair<string, object>("From", From),
							new KeyValuePair<string, object>("To", To2));
						Copy2 = false;
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
			Always = 4
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

						CopyFileIfNewer(SourceFileName,
							Path.Combine(DataFolder, FileName),
							Path.Combine(AppFolder, FileName),
							CopyOptions == CopyOptions.IfNewer);
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
				Log.Informational("Getting assembly name of server.");
				ServerName = GetAssemblyName(ServerApplication);
				Log.Informational("Server assembly name: " + ServerName.ToString());

				int i = ServerApplication.LastIndexOf('.');
				if (i < 0)
					DepsJsonFileName = ServerApplication;
				else
					DepsJsonFileName = ServerApplication[..i];

				DepsJsonFileName += ".deps.json";

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

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = new()
			{
				PreserveWhitespace = true
			};
			Manifest.Load(ManifestFile);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.ModuleManifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			XmlElement Module = Manifest["Module"];
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

			Array.Copy(Digest, 0, AesKey, 0, 32);
			Array.Copy(Digest, 32, IV, 0, 16);

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
					Log.Informational("Loading manifest file.");

					XmlDocument Manifest = new()
					{
						PreserveWhitespace = true
					};
					Manifest.Load(ManifestFile);

					Log.Informational("Validating manifest file.");

					XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.ModuleManifest.xsd", Assembly.GetExecutingAssembly());
					XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

					XmlElement Module = Manifest["Module"];
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
			if (File.Exists(AbsFileName))
				return (FileName, Path.GetFullPath(AbsFileName));

			string AltFolder = XML.Attribute(E, "altFolder");
			if (string.IsNullOrEmpty(AltFolder))
			{
				if (!CheckFileExists)
					return (FileName, Path.GetFullPath(AbsFileName));

				throw new FileNotFoundException("File not found: " + AbsFileName);
			}

			AltFolder = Path.Combine(ReferenceFolder, AltFolder);
			if (!Directory.Exists(AltFolder))
			{
				if (!CheckFileExists)
					return (FileName, Path.GetFullPath(AbsFileName));

				throw new Exception("Folder not found: " + AltFolder);
			}

			string AbsFileName2 = Path.Combine(AltFolder, FileName);
			if (File.Exists(AbsFileName2))
				return (FileName, Path.GetFullPath(AbsFileName2));

			if (!CheckFileExists)
				return (FileName, Path.GetFullPath(AbsFileName));

			throw new FileNotFoundException("File not found: " + AbsFileName);
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

			Array.Copy(Digest, 0, AesKey, 0, 32);
			Array.Copy(Digest, 32, IV, 0, 16);

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

				int i = ServerApplication.LastIndexOf('.');
				if (i < 0)
					DepsJsonFileName = ServerApplication;
				else
					DepsJsonFileName = ServerApplication[..i];

				DepsJsonFileName += ".deps.json";

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

									CopyFile(Decompressed, FileName, false, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);

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
								bool OnlyIfNewer = b == 3;

								FileName = Path.Combine(AppFolder, RelativeName);
								Log.Informational("Content file: " + FileName, string.Empty, string.Empty, "FileCopy");

								CopyFile(Decompressed, FileName, false, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);

								using (FileStream TempFile = File.OpenRead(FileName))
								{
									FileName = Path.Combine(ProgramDataFolder, RelativeName);
									Log.Informational("Content file: " + FileName, string.Empty, string.Empty, "FileCopy");

									CopyFile(TempFile, FileName, OnlyIfNewer, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);
								}
								break;

							case 5: // External application file
								FileName = Path.Combine(ExternalFolder, RelativeName);
								Log.Informational("External file: " + FileName, string.Empty, string.Empty, "FileCopy");

								CopyFile(Decompressed, FileName, false, Bytes, Attr, CreationTimeUtc, LastAccessTimeUtc, LastWriteTimeUtc, Buffer);
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

		private static void CopyFile(Stream Input, string OutputFileName, bool OnlyIfNewer, ulong Bytes, FileAttributes Attr,
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

				if (OnlyIfNewer && File.Exists(OutputFileName))
				{
					DateTime ExistingLastWriteTime = File.GetLastWriteTimeUtc(OutputFileName);
					if (ExistingLastWriteTime >= LastWriteTimeUtc)
					{
						SkipBytes(Input, Bytes, Buffer);
						return;
					}
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

				int i = ServerApplication.LastIndexOf('.');
				if (i < 0)
					DepsJsonFileName = ServerApplication;
				else
					DepsJsonFileName = ServerApplication[..i];

				DepsJsonFileName += ".deps.json";

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

			Array.Copy(Digest, 0, AesKey, 0, 32);
			Array.Copy(Digest, 32, IV, 0, 16);

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

		/// <summary>
		/// Generates Docker Instructions from information available in a manifest file.
		/// </summary>
		/// <param name="ManifestFile">File name of manifest file.</param>
		/// <param name="DockerOutput">Docker output</param>
		/// <param name="ProgramDataFolder">Data folder inside Docker container.</param>
		/// <param name="ServerApplication">Path to server application inside Docker container.</param>
		/// <param name="ContentOnly">If only content files should be copied.</param>
		/// <param name="ExcludeCategories">Any categories that should be exluded.</param>
		public static void GenerateDockerInstructions(string ManifestFile, StreamWriter DockerOutput, 
			string ProgramDataFolder, string ServerApplication, bool ContentOnly, Dictionary<string, bool> ExcludeCategories)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
				throw new Exception("Program Data folder for Docker image not specified.");

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = new()
			{
				PreserveWhitespace = true
			};
			Manifest.Load(ManifestFile);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.ModuleManifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			XmlElement Module = Manifest["Module"];
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);
			string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));

			CopyFile(ManifestFile, DestManifestFileName, DockerOutput);

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

						CopyFile(SourceFileName, Path.Combine(AppFolder, FileName), DockerOutput);
						if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							string PdbFileName = FileName[0..^4] + ".pdb";
							if (File.Exists(PdbFileName))
								CopyFile(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), DockerOutput);
						}
					}
				}
			}

			CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module, DockerOutput, ExcludeCategories);
		}

		private static void CopyFile(string SourceFileName, string DestFileName, StreamWriter DockerOutput)
		{
			DockerOutput.Write("COPY \"");
			DockerOutput.Write(SourceFileName);
			DockerOutput.Write("\" \"");
			DockerOutput.Write(DestFileName.Replace(Path.DirectorySeparatorChar, '/'));
			DockerOutput.WriteLine("\"");
		}

		private static void CopyContent(string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent, 
			StreamWriter DockerOutput, Dictionary<string, bool> ExcludeCategories)
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

						CopyFile(SourceFileName, Path.Combine(DataFolder, FileName), DockerOutput);
						CopyFile(SourceFileName, Path.Combine(AppFolder, FileName), DockerOutput);
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

						CopyContent(SourceFolder2, AppFolder2, DataFolder2, E, DockerOutput, ExcludeCategories);
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
				}
			}
		}

	}
}
