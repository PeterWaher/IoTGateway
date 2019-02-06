using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;

namespace Waher.Utility.Install
{
	/// <summary>
	/// Installs a module in an IoT Gateway.
	/// 
	/// Command line switches:
	/// 
	/// -m MANIFEST_FILE     Points to the manifest file describing the files in the module.
	/// -d APP_DATA_FOLDER   Points to the application data folder.
	/// -s SERVER_EXE        Points to the executable file of the IoT Gateway.
	/// -v                   Verbose mode.
	/// -i                   Install. This is the default. Switch not required.
	/// -u                   Uninstall. Add this switch if the module is being uninstalled.
	/// -r                   Remove files. Add this switch if you want files removed during
	///                      uninstallation. Default is to not remove files.
	/// -?                   Help.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				string ManifestFile = null;
				string ProgramDataFolder = null;
				string ServerApplication = null;
				int i = 0;
				int c = args.Length;
				string s;
				bool Help = false;
				bool Verbose = false;
				bool UninstallService = false;
				bool RemoveFiles = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-m":
							if (i >= c)
								throw new Exception("Missing manifest file.");

							if (string.IsNullOrEmpty(ManifestFile))
								ManifestFile = args[i++];
							else
								throw new Exception("Only one manifest file allowed.");
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

				if (Help || c == 0)
				{
					Console.Out.WriteLine("-m MANIFEST_FILE     Points to the manifest file describing the files in the module.");
					Console.Out.WriteLine("-d APP_DATA_FOLDER   Points to the application data folder.");
					Console.Out.WriteLine("-s SERVER_EXE        Points to the executable file of the IoT Gateway.");
					Console.Out.WriteLine("-v                   Verbose mode.");
					Console.Out.WriteLine("-i                   Install. This the default. Switch not required.");
					Console.Out.WriteLine("-u                   Uninstall. Add this switch if the module is being uninstalled.");
					Console.Out.WriteLine("-r                   Remove files. Add this switch if you want files removed during");
					Console.Out.WriteLine("                     uninstallation. Default is to not remove files.");
					Console.Out.WriteLine("-?                   Help.");
					return 0;
				}

				if (Verbose)
					Log.Register(new Waher.Events.Console.ConsoleEventSink());

				if (UninstallService)
					Uninstall(ManifestFile, ServerApplication, ProgramDataFolder, RemoveFiles);
				else
					Install(ManifestFile, ServerApplication, ProgramDataFolder);

				return 0;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);

				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}

		private static void Install(string ManifestFile, string ServerApplication, string ProgramDataFolder)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

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

			Log.Informational("Getting assembly name of server.");
			AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);
			Log.Informational("Server assembly name: " + ServerName.ToString());

			string DepsJsonFileName;

			int i = ServerApplication.LastIndexOf('.');
			if (i < 0)
				DepsJsonFileName = ServerApplication;
			else
				DepsJsonFileName = ServerApplication.Substring(0, i);

			DepsJsonFileName += ".deps.json";

			Log.Informational("deps.json file name: " + DepsJsonFileName);

			if (!File.Exists(DepsJsonFileName))
				throw new Exception("Invalid server executable. No corresponding deps.json file found.");

			Log.Informational("Opening " + DepsJsonFileName);

			string s = File.ReadAllText(DepsJsonFileName);

			Log.Informational("Parsing " + DepsJsonFileName);

			Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
			if (Deps is null)
				throw new Exception("Invalid deps.json file. Unable to install.");

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = new XmlDocument();
			Manifest.Load(ManifestFile);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			XmlElement Module = Manifest["Module"];
			string SourceFolder = Path.GetDirectoryName(ManifestFile);
			string AppFolder = Path.GetDirectoryName(ServerApplication);

			string DestManifestFileName = Path.Combine(AppFolder, Path.GetFileName(ManifestFile));
			CopyFileIfNewer(ManifestFile, DestManifestFileName, false);

			Log.Informational("Source folder: " + SourceFolder);
			Log.Informational("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Assembly")
				{
					string FileName = XML.Attribute(E, "fileName");
					string SourceFileName = Path.Combine(SourceFolder, FileName);

					if (CopyFileIfNewer(SourceFileName, Path.Combine(AppFolder, FileName), true))
					{
						if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
							if (File.Exists(PdbFileName))
								CopyFileIfNewer(Path.Combine(SourceFolder, PdbFileName), Path.Combine(AppFolder, PdbFileName), false);
						}
					}

					Assembly A = Assembly.LoadFrom(SourceFileName);
					AssemblyName AN = A.GetName();

					if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
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

								Dictionary<string, object> Dependencies2 = new Dictionary<string, object>();

								foreach (AssemblyName Dependency in A.GetReferencedAssemblies())
									Dependencies2[Dependency.Name] = Dependency.Version.ToString();

								Dictionary<string, object> Runtime = new Dictionary<string, object>()
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

					if (Deps != null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
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

			if (SourceFolder == AppFolder)
				Log.Warning("Skipping copying of content. Source and application folders the same. Assuming content files are located where they should be.");
			else
				CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module);

			Log.Informational("Encoding JSON");
			s = JSON.Encode(Deps, true);

			Log.Informational("Writing " + DepsJsonFileName);
			File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);
		}

		private static bool CopyFileIfNewer(string From, string To, bool OnlyIfNewer)
		{
			if (From == To)
			{
				Log.Warning("Skipping file. Copying to same location: " + From);
				return false;
			}

			if (!File.Exists(From))
				throw new Exception("File not found: " + From);

			if (OnlyIfNewer && File.Exists(To))
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
					return false;
				}
			}

			Log.Informational("Copying " + From + " to " + To);
			File.Copy(From, To, true);

			return true;
		}

		private enum CopyOptions
		{
			IfNewer,
			Always
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
							string FileName = XML.Attribute(E, "fileName");
							CopyOptions CopyOptions = (CopyOptions)XML.Attribute(E, "copy", CopyOptions.IfNewer);

							Log.Informational("Content file: " + FileName);

							if (!string.IsNullOrEmpty(DataFolder) && !Directory.Exists(DataFolder))
							{
								Log.Informational("Creating folder " + DataFolder + ".");
								Directory.CreateDirectory(DataFolder);
							}

							CopyFileIfNewer(Path.Combine(SourceFolder, FileName), Path.Combine(DataFolder, FileName),
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
					}
				}
			}
		}

		private static void Uninstall(string ManifestFile, string ServerApplication, string ProgramDataFolder, bool Remove)
		{
			// Same code as for custom action InstallManifest in Waher.IoTGateway.Installers

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

			Log.Informational("Getting assembly name of server.");
			AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);
			Log.Informational("Server assembly name: " + ServerName.ToString());

			string DepsJsonFileName;

			int i = ServerApplication.LastIndexOf('.');
			if (i < 0)
				DepsJsonFileName = ServerApplication;
			else
				DepsJsonFileName = ServerApplication.Substring(0, i);

			DepsJsonFileName += ".deps.json";

			Log.Informational("deps.json file name: " + DepsJsonFileName);

			if (!File.Exists(DepsJsonFileName))
				throw new Exception("Invalid server executable. No corresponding deps.json file found.");

			Log.Informational("Opening " + DepsJsonFileName);

			string s = File.ReadAllText(DepsJsonFileName);

			Log.Informational("Parsing " + DepsJsonFileName);

			Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
			if (Deps is null)
				throw new Exception("Invalid deps.json file. Unable to install.");

			Log.Informational("Loading manifest file.");

			XmlDocument Manifest = new XmlDocument();
			Manifest.Load(ManifestFile);

			Log.Informational("Validating manifest file.");

			XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
			XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

			XmlElement Module = Manifest["Module"];
			string AppFolder = Path.GetDirectoryName(ServerApplication);

			Log.Informational("App folder: " + AppFolder);

			foreach (XmlNode N in Module.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Assembly")
				{
					string FileName = XML.Attribute(E, "fileName");
					string AppFileName = Path.Combine(AppFolder, FileName);

					Assembly A = Assembly.LoadFrom(AppFileName);
					AssemblyName AN = A.GetName();
					string Key = AN.Name + "/" + AN.Version.ToString();

					if (Deps != null && Deps.TryGetValue("targets", out object Obj) && Obj is Dictionary<string, object> Targets)
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

					if (Deps != null && Deps.TryGetValue("libraries", out object Obj3) && Obj3 is Dictionary<string, object> Libraries)
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

					if (Remove)
					{
						RemoveFile(AppFileName);
						if (FileName.EndsWith(".dll", StringComparison.CurrentCultureIgnoreCase))
						{
							string PdbFileName = FileName.Substring(0, FileName.Length - 4) + ".pdb";
							RemoveFile(PdbFileName);
						}
					}
				}
			}

			Log.Informational("Encoding JSON");
			s = JSON.Encode(Deps, true);

			Log.Informational("Writing " + DepsJsonFileName);
			File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);

			if (Path.GetDirectoryName(ManifestFile) == AppFolder)
				RemoveFile(ManifestFile);
		}

		private static bool RemoveFile(string FileName)
		{
			if (!File.Exists(FileName))
				return false;

			Log.Informational("Deleting " + FileName);
			File.Delete(FileName);

			return true;
		}

	}
}
