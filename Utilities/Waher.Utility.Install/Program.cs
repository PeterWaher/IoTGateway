using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
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
				LinkedList<KeyValuePair<string, string>> Packages = new();
				List<string> ManifestFiles = new();
				string ProgramDataFolder = null;
				string ServerApplication = null;
				string PackageFile = null;
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
					Console.Out.WriteLine("-m MANIFEST_FILE     Points to a manifest file describing the files in the module.");
					Console.Out.WriteLine("                     If multiple manifest files are referened, they are processed");
					Console.Out.WriteLine("                     in the order they are listed.");
					Console.Out.WriteLine("-p PACKAGE_FILE      If provided together with a manifest file, files will be");
					Console.Out.WriteLine("                     packed into a package file that can be easily distributed,");
					Console.Out.WriteLine("                     instead of being installed on the local machine.");
					Console.Out.WriteLine("                     If a manifest file is not specified, the package file will");
					Console.Out.WriteLine("                     be used instead.");
					Console.Out.WriteLine("-d APP_DATA_FOLDER   Points to the application data folder. Required if");
					Console.Out.WriteLine("                     installing a module.");
					Console.Out.WriteLine("-s SERVER_EXE        Points to the executable file of the IoT Gateway. Required");
					Console.Out.WriteLine("                     if installing a module.");
					Console.Out.WriteLine("-k KEY               Encryption key used for the package file. Secret used in");
					Console.Out.WriteLine("                     encryption is based on the local package file name and the");
					Console.Out.WriteLine("                     KEY parameter, if provided. You cannot rename a package file.");
					Console.Out.WriteLine("-v                   Verbose mode.");
					Console.Out.WriteLine("-i                   Install. This the default. Switch not required.");
					Console.Out.WriteLine("-u                   Uninstall. Add this switch if the module is being uninstalled.");
					Console.Out.WriteLine("-r                   Remove files. Add this switch if you want files removed during");
					Console.Out.WriteLine("                     uninstallation. Default is to not remove files.");
					Console.Out.WriteLine("-n INSTANCE          Name of instance. Default is the empty string. Parallel instances");
					Console.Out.WriteLine("                     of the IoT Gateway can execute, provided they are given separate");
					Console.Out.WriteLine("                     instance names. This property is used in conjunction with the -w");
					Console.Out.WriteLine("                     property.");
					Console.Out.WriteLine("-w MILLISECONDS      Waits for the Gateway to stop executing before performing the");
					Console.Out.WriteLine("                     operation. If the gateway does not stop within this period of");
					Console.Out.WriteLine("                     time, the operation fails. (Default=60000)");
					Console.Out.WriteLine("-co                  If only content (content only) should be installed.");
					Console.Out.WriteLine("-?                   Help.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Note: Alternating -p and -k attributes can be used to process multiple packages in");
					Console.Out.WriteLine("      one operation.");
					return 0;
				}

				if (Verbose)
					Log.Register(new Events.Console.ConsoleEventSink());

				Types.Initialize(typeof(Program).Assembly,
					typeof(JSON).Assembly);

				Semaphore GatewayRunning = null;
				Semaphore StartingServer = null;
				bool GatewayRunningLocked = false;
				bool StartingServerLocked = false;

				try
				{
					if (Timeout.HasValue && !ContentOnly)
					{
						if (Verbose)
							Console.Out.WriteLine("Making sure server is closed...");

						GatewayRunning = new Semaphore(1, 1, "Waher.IoTGateway.Running" + Suffix);
						if (!GatewayRunning.WaitOne(Timeout.Value))
							throw new Exception("The IoT Gateway did not stop within the given time period.");

						GatewayRunningLocked = true;

						StartingServer = new Semaphore(1, 1, "Waher.IoTGateway.Starting" + Suffix);
						if (!StartingServer.WaitOne(Timeout.Value))
							throw new Exception("The IoT Gateway is starting in another process, and is unable to stop within the given time period.");

						StartingServerLocked = true;

						if (Verbose)
							Console.Out.WriteLine("Server is closed. Proceeding...");
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
							GeneratePackage(ManifestFiles.ToArray(), Packages.First.Value.Key, Packages.First.Value.Value);
						else
							throw new Exception("Only one package file name can be referenced, when generating a package.");
					}
					else
					{
						foreach (string ManifestFile in ManifestFiles)
						{
							if (UninstallService)
								Uninstall(ManifestFile, ServerApplication, ProgramDataFolder, RemoveFiles, ContentOnly);
							else
								Install(ManifestFile, ServerApplication, ProgramDataFolder, ContentOnly);
						}
					}
				}
				finally
				{
					if (GatewayRunning is not null)
					{
						if (GatewayRunningLocked)
							GatewayRunning.Release();

						GatewayRunning.Dispose();
						GatewayRunning = null;
					}

					if (StartingServer is not null)
					{
						if (StartingServerLocked)
							StartingServer.Release();

						StartingServer.Dispose();
						StartingServer = null;
					}
				}

				return 0;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				Console.Out.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				Log.Terminate();
			}
		}

		public static AssemblyName GetAssemblyName(string ServerApplication)
		{
			if (ServerApplication.EndsWith(".exe", StringComparison.CurrentCultureIgnoreCase))
				ServerApplication = ServerApplication[0..^4] + ".dll";

			return AssemblyName.GetAssemblyName(ServerApplication);
		}

		public static void Install(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool ContentOnly)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(SpecialFolder.CommonApplicationData), "IoT Gateway");
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
				if (N is XmlElement E && E.LocalName == "Assembly")
				{
					if (!ContentOnly)
					{
						(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

						if (CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), null, true))
						{
							if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
							{
								string PdbFileName = FileName[0..^4] + ".pdb";
								if (File.Exists(PdbFileName))
									CopyFileIfNewer(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), null, true);
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

			CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module);

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

		private static void CopyContent(string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is XmlElement E)
				{
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

							CopyContent(SourceFolder2, AppFolder2, DataFolder2, E);
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

		public static void Uninstall(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool Remove, bool ContentOnly)
		{
			// Same code as for custom action UninstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ManifestFile))
				throw new Exception("Missing manifest file.");

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
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
				if (N is XmlElement E && E.LocalName == "Assembly")
				{
					if (!ContentOnly)
					{
						(string FileName, string AppFileName) = GetFileName(E, AppFolder);

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

		public static void GeneratePackage(string[] ManifestFiles, string PackageFile, string Key)
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
						if (N is XmlElement E && E.LocalName == "Assembly")
						{
							(string FileName, string SourceFileName) = GetFileName(E, SourceFolder);

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

					CopyContent(SourceFolder, Compressed, string.Empty, Module);
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

		private static void CopyContent(string SourceFolder, Stream Output, string RelativeFolder, XmlElement Parent)
		{
			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is XmlElement E)
				{
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

							CopyContent(SourceFolder2, Output, RelativeFolder2, E);
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

							CopyContent(SourceFolder2, Output, string.Empty, E);
							break;
					}
				}
			}
		}

		private static (string, string) GetFileName(XmlElement E, string ReferenceFolder)
		{
			string FileName = XML.Attribute(E, "fileName");
			string AbsFileName = Path.Combine(ReferenceFolder, FileName);
			if (File.Exists(AbsFileName))
				return (FileName, Path.GetFullPath(AbsFileName));

			string AltFolder = XML.Attribute(E, "altFolder");
			if (string.IsNullOrEmpty(AltFolder))
				throw new FileNotFoundException("File not found: " + AbsFileName);

			AltFolder = Path.Combine(ReferenceFolder, AltFolder);
			if (!Directory.Exists(AltFolder))
				throw new Exception("Folder not found: " + AltFolder);

			AbsFileName = Path.Combine(AltFolder, FileName);
			if (File.Exists(AbsFileName))
				return (FileName, Path.GetFullPath(AbsFileName));

			throw new FileNotFoundException("File not found: " + AbsFileName);
		}

		private static void InstallPackage(LinkedList<KeyValuePair<string, string>> Packages, string ServerApplication,
			string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			foreach (KeyValuePair<string, string> Package in Packages)
				InstallPackage(Package.Key, Package.Value, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
		}

		public static void InstallPackage(string PackageFile, string Key, string ServerApplication, string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			if (string.IsNullOrEmpty(PackageFile))
				throw new Exception("Missing package file.");

			using FileStream fs = File.OpenRead(PackageFile);

			InstallPackage(PackageFile, fs, Key, ServerApplication, ProgramDataFolder, ContentOnly, ServerExists);
		}

		public static void InstallPackage(string FileName, Stream Encrypted, string Key, string ServerApplication, string ProgramDataFolder, bool ContentOnly, bool ServerExists)
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

		public static void InstallPackage(Stream Decrypted, string ServerApplication, string ProgramDataFolder, bool ContentOnly, bool ServerExists)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

			if (string.IsNullOrEmpty(ServerApplication))
				throw new Exception("Missing server application.");

			if (string.IsNullOrEmpty(ProgramDataFolder))
			{
				ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
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

	}
}
