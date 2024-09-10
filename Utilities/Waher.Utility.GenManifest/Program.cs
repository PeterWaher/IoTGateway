using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Waher.Content.Xml;
using Waher.Runtime.Console;
using static System.Environment;

namespace Waher.Utility.GenManifest
{
	class Program
	{
		public const string Namespace = "http://waher.se/Schema/ModuleManifest.xsd";

		/// <summary>
		/// Generates a manifest file from contents in one or more file
		/// folders.
		/// 
		/// Command line switches:
		/// 
		/// -a FOLDER             Adding folder of assembly files. This switch
		///                       can be used multiple times. Folder is not
		///                       processed recursively.
		/// -c FOLDER             Adding folder of content files. This switch
		///                       can be used multiple times. Folder is processed
		///                       recursively.
		/// -p SPECIALFOLDER      Adding folder of external program data files.
		///    NAME FOLDER        The program data files will be copied to a
		///                       folder named NAME under the operating system
		///                       special folder SPECIALFOLDER. The folder FOLDER
		///                       is processed recursively. Possible values of
		///                       SPECIALFOLDER are: Desktop, Programs, MyDocuments, 
		///                       Personal, Favorites, Startup, Recent, SendTo, 
		///                       StartMenu, MyMusic, MyVideos, DesktopDirectory, 
		///                       MyComputer, NetworkShortcuts, Fonts, Templates, 
		///                       CommonStartMenu, CommonPrograms, CommonStartup, 
		///                       CommonDesktopDirectory, ApplicationData, 
		///                       PrinterShortcuts, LocalApplicationData, 
		///                       InternetCache, Cookies, History, 
		///                       CommonApplicationData, Windows, System, 
		///                       ProgramFiles, MyPictures, UserProfile, SystemX86, 
		///                       ProgramFilesX86, CommonProgramFiles, 
		///                       CommonProgramFilesX86, CommonTemplates, 
		///                       CommonDocuments, CommonAdminTools, AdminTools, 
		///                       CommonMusic, CommonPictures, CommonVideos, 
		///                       Resources, LocalizedResources, CommonOemLinks, 
		///                       CDBurning.
		/// -o FILENAME           File name of output manifest file.
		/// -ef FOLDER            Exclude folders with the given name.
		/// -ex EXTENSION         Exclude files with the given extension.
		/// -l                    Lists available special folders, and to what
		///                       folders on the computer they are mapped to.
		/// -?                    Help.
		/// </summary>
		static int Main(string[] args)
		{
			SortedDictionary<string, bool> AssemblyFiles = new(StringComparer.InvariantCultureIgnoreCase);
			FolderRec ContentFiles = new()
			{
				Folders = new SortedDictionary<string, FolderRec>(StringComparer.InvariantCultureIgnoreCase)
			};
			List<string> AssemblyFolderNames = new();
			List<string> ContentFolderNames = new();
			List<ProgramRec> ProgramFolders = new();
			Dictionary<string, bool> ExtensionsToIgnore = null;
			Dictionary<string, bool> FoldersToIgnore = null;
			string OutputFileName = null;
			string s;
			int i = 0;
			int c = args.Length;
			bool Help = false;
			bool ListSpecialFolders = false;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-a":
							if (i >= c)
								throw new Exception("Missing assembly folder name.");

							s = args[i++];
							AssemblyFolderNames.Add(s);

							if (!Directory.Exists(s))
								throw new Exception("Assembly Folder not found: " + s);
							break;

						case "-c":
							if (i >= c)
								throw new Exception("Missing content folder name.");

							s = args[i++];
							ContentFolderNames.Add(s);

							if (!Directory.Exists(s))
								throw new Exception("Content Folder not found: " + s);
							break;

						case "-p":
							if (i >= c)
								throw new Exception("Missing special folder.");

							s = args[i++];
							if (!Enum.TryParse(s, out SpecialFolder SpecialFolder))
								throw new Exception("Invalid special folder: " + s);

							if (i >= c)
								throw new Exception("Missing program folder name.");

							string Name = args[i++];

							if (i >= c)
								throw new Exception("Missing program folder.");

							s = args[i++];

							if (!Directory.Exists(s))
								throw new Exception("Program Folder not found: " + s);

							ProgramFolders.Add(new ProgramRec()
							{
								SpecialFolder = SpecialFolder,
								Name = Name,
								Folder = s
							});
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing file name.");

							if (!string.IsNullOrEmpty(OutputFileName))
								throw new Exception("Output file name already provided.");

							OutputFileName = args[i++];
							break;

						case "-ef":
							if (i >= c)
								throw new Exception("Missing folder.");

							FoldersToIgnore ??= new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

							s = args[i++];
							FoldersToIgnore[s] = true;
							break;

						case "-ex":
							if (i >= c)
								throw new Exception("Missing extension.");

							ExtensionsToIgnore ??= new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);

							s = args[i++];
							ExtensionsToIgnore[s] = true;

							if (s.StartsWith("."))
								ExtensionsToIgnore[s[1..]] = true;
							else
								ExtensionsToIgnore["." + s] = true;
							break;

						case "-l":
							ListSpecialFolders = true;
							break;

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (ListSpecialFolders)
				{
					SortedDictionary<string, string> Mappings = new();
					int MaxLen = 0;
					int Len;

					foreach (SpecialFolder SpecialFolder in Enum.GetValues(typeof(SpecialFolder)))
					{
						string Folder = Environment.GetFolderPath(SpecialFolder);
						string Name = SpecialFolder.ToString();

						Len = Name.Length;
						if (Len > MaxLen)
							MaxLen = Len;

						Mappings[Name] = Folder;
					}

					MaxLen += 2;

					foreach (KeyValuePair<string, string> P in Mappings)
					{
						ConsoleOut.Write(P.Key);
						ConsoleOut.Write(':');
						ConsoleOut.Write(new string(' ', MaxLen - P.Key.Length));
						ConsoleOut.WriteLine(P.Value);
					}
				}

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("Generates a manifest file from contents in one or more file");
					ConsoleOut.WriteLine("folders.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Command line switches:");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("-a FOLDER             Adding folder of assembly files. This switch");
					ConsoleOut.WriteLine("                      can be used multiple times. Folder is not");
					ConsoleOut.WriteLine("                      processed recursively.");
					ConsoleOut.WriteLine("-c FOLDER             Adding folder of content files. This switch");
					ConsoleOut.WriteLine("                      can be used multiple times. Folder is processed");
					ConsoleOut.WriteLine("                      recursively.");
					ConsoleOut.WriteLine("-p SPECIALFOLDER      Adding folder of external program data files.");
					ConsoleOut.WriteLine("   NAME FOLDER        The program data files will be copied to a");
					ConsoleOut.WriteLine("                      folder named NAME under the operating system");
					ConsoleOut.WriteLine("                      special folder SPECIALFOLDER. The folder FOLDER");
					ConsoleOut.WriteLine("                      is processed recursively. Possible values of");
					ConsoleOut.WriteLine("                      SPECIALFOLDER are: Desktop, Programs, MyDocuments, ");
					ConsoleOut.WriteLine("                      Personal, Favorites, Startup, Recent, SendTo, ");
					ConsoleOut.WriteLine("                      StartMenu, MyMusic, MyVideos, DesktopDirectory, ");
					ConsoleOut.WriteLine("                      MyComputer, NetworkShortcuts, Fonts, Templates, ");
					ConsoleOut.WriteLine("                      CommonStartMenu, CommonPrograms, CommonStartup, ");
					ConsoleOut.WriteLine("                      CommonDesktopDirectory, ApplicationData, ");
					ConsoleOut.WriteLine("                      PrinterShortcuts, LocalApplicationData, ");
					ConsoleOut.WriteLine("                      InternetCache, Cookies, History, ");
					ConsoleOut.WriteLine("                      CommonApplicationData, Windows, System, ");
					ConsoleOut.WriteLine("                      ProgramFiles, MyPictures, UserProfile, SystemX86, ");
					ConsoleOut.WriteLine("                      ProgramFilesX86, CommonProgramFiles, ");
					ConsoleOut.WriteLine("                      CommonProgramFilesX86, CommonTemplates, ");
					ConsoleOut.WriteLine("                      CommonDocuments, CommonAdminTools, AdminTools, ");
					ConsoleOut.WriteLine("                      CommonMusic, CommonPictures, CommonVideos, ");
					ConsoleOut.WriteLine("                      Resources, LocalizedResources, CommonOemLinks, ");
					ConsoleOut.WriteLine("                      CDBurning.");
					ConsoleOut.WriteLine("-o FILENAME           File name of output manifest file.");
					ConsoleOut.WriteLine("-ef FOLDER            Exclude folders with the given name.");
					ConsoleOut.WriteLine("-ex EXTENSION         Exclude files with the given extension.");
					ConsoleOut.WriteLine("-l                    Lists available special folders, and to what");
					ConsoleOut.WriteLine("                      folders on the computer they are mapped to.");
					ConsoleOut.WriteLine("-?                    Help.");
					return 0;
				}

				if (AssemblyFolderNames.Count + ContentFolderNames.Count + ProgramFolders.Count == 0)
					throw new Exception("No file folders provided.");

				if (string.IsNullOrEmpty(OutputFileName))
					throw new Exception("No output file name provided.");

				foreach (string FolderName in AssemblyFolderNames)
				{
					string AbsolutePath = Path.GetFullPath(FolderName);
					string[] FileNames = Directory.GetFiles(AbsolutePath, "*.*", SearchOption.TopDirectoryOnly);

					foreach (string FileName in FileNames)
					{
						if (IgnoreFile(ExtensionsToIgnore, FoldersToIgnore, FileName))
							continue;

						string RelativeFileName = Path.GetRelativePath(AbsolutePath, FileName);
						if (RelativeFileName.Contains(Path.DirectorySeparatorChar))
							throw new Exception("Directory separators not permitted in assembly files.");

						AssemblyFiles[RelativeFileName] = true;
					}
				}

				foreach (string FolderName in ContentFolderNames)
					OrderFiles(ExtensionsToIgnore, FoldersToIgnore, FolderName, ContentFiles.Folders);

				XmlWriterSettings Settings = XML.WriterSettings(true, false);
				using XmlWriter Output = XmlWriter.Create(OutputFileName, Settings);

				Output.WriteStartDocument();
				Output.WriteStartElement("Module", Namespace);

				foreach (string AssemblyFile in AssemblyFiles.Keys)
				{
					Output.WriteStartElement("Assembly", Namespace);
					Output.WriteAttributeString("fileName", AssemblyFile);
					Output.WriteEndElement();
				}

				ExportFolder(Output, ContentFiles.Folders, "Content");

				foreach (ProgramRec Rec in ProgramFolders)
				{
					Output.WriteStartElement("External", Namespace);
					Output.WriteAttributeString("folder", Rec.SpecialFolder.ToString());
					Output.WriteAttributeString("name", Rec.Name);

					FolderRec ExternalFiles = new()
					{
						Folders = new SortedDictionary<string, FolderRec>(StringComparer.InvariantCultureIgnoreCase)
					};

					OrderFiles(ExtensionsToIgnore, FoldersToIgnore, Rec.Folder, ExternalFiles.Folders);
					ExportFolder(Output, ExternalFiles.Folders, "File");

					Output.WriteEndElement();
				}

				Output.WriteEndElement();
				Output.WriteEndDocument();

				return 0;
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
				return -1;
			}
		}

		private static bool IgnoreFile(Dictionary<string, bool> ExtensionsToIgnore, Dictionary<string, bool> FoldersToIgnore,
			string FileName)
		{
			if (ExtensionsToIgnore is not null)
			{
				string Extension = Path.GetExtension(FileName);

				if (ExtensionsToIgnore.ContainsKey(Extension))
					return true;
			}

			if (FoldersToIgnore is not null)
			{
				string Directory = Path.GetDirectoryName(FileName);

				if (FoldersToIgnore.ContainsKey(Directory))
					return true;

				if (Directory.EndsWith(Path.DirectorySeparatorChar))
				{
					Directory = Directory[..^1];

					if (FoldersToIgnore.ContainsKey(Directory))
						return true;
				}

				string[] Parts = Directory.Split(Path.DirectorySeparatorChar);

				foreach (string Part in Parts)
				{
					if (FoldersToIgnore.ContainsKey(Part))
						return true;
				}
			}

			return false;
		}

		private static void OrderFiles(Dictionary<string, bool> ExtensionsToIgnore,
			Dictionary<string, bool> FoldersToIgnore, string FolderName,
			SortedDictionary<string, FolderRec> Files)
		{
			string AbsolutePath = Path.GetFullPath(FolderName);
			string[] FileNames = Directory.GetFiles(AbsolutePath, "*.*", SearchOption.AllDirectories);

			foreach (string FileName in FileNames)
			{
				if (IgnoreFile(ExtensionsToIgnore, FoldersToIgnore, FileName))
					continue;

				string RelativeFileName = Path.GetRelativePath(AbsolutePath, FileName);
				string[] Parts = RelativeFileName.Split(Path.DirectorySeparatorChar);
				FolderRec Rec = null;

				foreach (string Part in Parts)
				{
					if (Rec is null)
					{
						if (!Files.TryGetValue(Part, out Rec))
						{
							Files[Part] = Rec = new FolderRec()
							{
								Name = Part
							};
						}
					}
					else
					{
						Rec.Folders ??= new SortedDictionary<string, FolderRec>(StringComparer.InvariantCultureIgnoreCase);

						if (!Rec.Folders.TryGetValue(Part, out FolderRec Rec2))
						{
							Rec.Folders[Part] = Rec2 = new FolderRec()
							{
								Name = Part
							};
						}

						Rec = Rec2;
					}
				}
			}
		}

		private static void ExportFolder(XmlWriter Output, SortedDictionary<string, FolderRec> Rec, string FileElementName)
		{
			foreach (KeyValuePair<string, FolderRec> P in Rec)
			{
				if (P.Value?.Folders is null)
				{
					Output.WriteStartElement(FileElementName, Namespace);
					Output.WriteAttributeString("fileName", P.Key);
					Output.WriteEndElement();
				}
			}

			foreach (KeyValuePair<string, FolderRec> P in Rec)
			{
				if (P.Value?.Folders is not null)
				{
					Output.WriteStartElement("Folder", Namespace);
					Output.WriteAttributeString("name", P.Key);

					ExportFolder(Output, P.Value.Folders, FileElementName);

					Output.WriteEndElement();
				}
			}
		}

		private class ProgramRec
		{
			public SpecialFolder SpecialFolder;
			public string Name;
			public string Folder;
		}

		private class FolderRec
		{
			public string Name { get; set; }
			public SortedDictionary<string, FolderRec> Folders { get; set; }
		}
	}
}
