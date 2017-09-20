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
					return 0;
				}

				if (string.IsNullOrEmpty(ManifestFile))
					throw new Exception("Missing manifest file.");

				if (string.IsNullOrEmpty(ServerApplication))
					throw new Exception("Missing server application.");

				if (string.IsNullOrEmpty(ProgramDataFolder))
				{
					ProgramDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "IoT Gateway");
					Console.Out.WriteLine("Using default program data folder: " + ProgramDataFolder);
				}

				if (!File.Exists(ServerApplication))
					throw new Exception("Server application not found: " + ServerApplication);

				AssemblyName ServerName = AssemblyName.GetAssemblyName(ServerApplication);

				string DepsJsonFileName;

				i = ServerApplication.LastIndexOf('.');
				if (i < 0)
					DepsJsonFileName = ServerApplication;
				else
					DepsJsonFileName = ServerApplication.Substring(0, i);

				DepsJsonFileName += ".deps.json";

				if (!File.Exists(DepsJsonFileName))
					throw new Exception("Invalid server executable. No corresponding deps.json file found.");

				s = File.ReadAllText(DepsJsonFileName);
				Dictionary<string, object> Deps = JSON.Parse(s) as Dictionary<string, object>;
				if (Deps == null)
					throw new Exception("Invalid deps.json file. Unable to install.");

				XmlDocument Manifest = new XmlDocument();
				Manifest.Load(ManifestFile);

				XmlSchema Schema = XSL.LoadSchema(typeof(Program).Namespace + ".Schema.Manifest.xsd", Assembly.GetExecutingAssembly());
				XSL.Validate(ManifestFile, Manifest, "Module", "http://waher.se/Schema/ModuleManifest.xsd", Schema);

				XmlElement Module = Manifest["Module"];
				string SourceFolder = Path.GetDirectoryName(ManifestFile);
				string AppFolder = Path.GetDirectoryName(ServerApplication);

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

				CopyContent(SourceFolder, AppFolder, ProgramDataFolder, Module);

				s = JSON.Encode(Deps, true);
				File.WriteAllText(DepsJsonFileName, s, Encoding.UTF8);

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}

		private static bool CopyFileIfNewer(string From, string To, bool OnlyIfNewer)
		{
			if (!File.Exists(From))
				throw new Exception("File not found: " + From);

			if (OnlyIfNewer && File.Exists(To))
			{
				DateTime ToTP = File.GetCreationTimeUtc(To);
				DateTime FromTP = File.GetCreationTimeUtc(From);

				if (ToTP >= FromTP)
					return false;
			}

			File.Copy(From, To, true);

			return true;
		}

		private static void CopyContent(string SourceFolder, string AppFolder, string DataFolder, XmlElement Parent)
		{
			if (!Directory.Exists(DataFolder))
				Directory.CreateDirectory(DataFolder);

			if (!Directory.Exists(AppFolder))
				Directory.CreateDirectory(AppFolder);

			foreach (XmlNode N in Parent.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "Content":
							string FileName = XML.Attribute(E, "fileName");
							CopyFileIfNewer(Path.Combine(SourceFolder, FileName), Path.Combine(AppFolder, FileName), true);
							CopyFileIfNewer(Path.Combine(SourceFolder, FileName), Path.Combine(DataFolder, FileName), true);
							break;

						case "Folder":
							string Name = XML.Attribute(E, "name");
							CopyContent(Path.Combine(SourceFolder, Name), Path.Combine(AppFolder, Name), Path.Combine(DataFolder, Name), E);
							break;
					}
				}
			}
		}

	}
}
