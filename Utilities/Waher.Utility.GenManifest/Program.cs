using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Xml;
using Waher.Content.Xml;
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
		/// -l                    Lists available special folders, and to what
		///                       folders on the computer they are mapped to.
		/// -?                    Help.
		/// </summary>
		static int Main(string[] args)
		{
			SortedDictionary<string, bool> AssemblyFiles = new SortedDictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
			FolderRec ContentFiles = new FolderRec()
			{
				Folders = new SortedDictionary<string, FolderRec>(StringComparer.InvariantCultureIgnoreCase)
			};
			List<string> AssemblyFolderNames = new List<string>();
			List<string> ContentFolderNames = new List<string>();
			List<ProgramRec> ProgramFolders = new List<ProgramRec>();
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
					SortedDictionary<string, string> Mappings = new SortedDictionary<string, string>();
					int MaxLen = 0;
					int Len;

					foreach (Environment.SpecialFolder SpecialFolder in Enum.GetValues(typeof(Environment.SpecialFolder)))
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
						Console.Out.Write(P.Key);
						Console.Out.Write(':');
						Console.Out.Write(new string(' ', MaxLen - P.Key.Length));
						Console.Out.WriteLine(P.Value);
					}
				}

				if (Help || c == 0)
				{
					Console.Out.WriteLine("Generates a manifest file from contents in one or more file");
					Console.Out.WriteLine("folders.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-a FOLDER             Adding folder of assembly files. This switch");
					Console.Out.WriteLine("                      can be used multiple times. Folder is not");
					Console.Out.WriteLine("                      processed recursively.");
					Console.Out.WriteLine("-c FOLDER             Adding folder of content files. This switch");
					Console.Out.WriteLine("                      can be used multiple times. Folder is processed");
					Console.Out.WriteLine("                      recursively.");
					Console.Out.WriteLine("-p SPECIALFOLDER      Adding folder of external program data files.");
					Console.Out.WriteLine("   NAME FOLDER        The program data files will be copied to a");
					Console.Out.WriteLine("                      folder named NAME under the operating system");
					Console.Out.WriteLine("                      special folder SPECIALFOLDER. The folder FOLDER");
					Console.Out.WriteLine("                      is processed recursively. Possible values of");
					Console.Out.WriteLine("                      SPECIALFOLDER are: Desktop, Programs, MyDocuments, ");
					Console.Out.WriteLine("                      Personal, Favorites, Startup, Recent, SendTo, ");
					Console.Out.WriteLine("                      StartMenu, MyMusic, MyVideos, DesktopDirectory, ");
					Console.Out.WriteLine("                      MyComputer, NetworkShortcuts, Fonts, Templates, ");
					Console.Out.WriteLine("                      CommonStartMenu, CommonPrograms, CommonStartup, ");
					Console.Out.WriteLine("                      CommonDesktopDirectory, ApplicationData, ");
					Console.Out.WriteLine("                      PrinterShortcuts, LocalApplicationData, ");
					Console.Out.WriteLine("                      InternetCache, Cookies, History, ");
					Console.Out.WriteLine("                      CommonApplicationData, Windows, System, ");
					Console.Out.WriteLine("                      ProgramFiles, MyPictures, UserProfile, SystemX86, ");
					Console.Out.WriteLine("                      ProgramFilesX86, CommonProgramFiles, ");
					Console.Out.WriteLine("                      CommonProgramFilesX86, CommonTemplates, ");
					Console.Out.WriteLine("                      CommonDocuments, CommonAdminTools, AdminTools, ");
					Console.Out.WriteLine("                      CommonMusic, CommonPictures, CommonVideos, ");
					Console.Out.WriteLine("                      Resources, LocalizedResources, CommonOemLinks, ");
					Console.Out.WriteLine("                      CDBurning.");
					Console.Out.WriteLine("-o FILENAME           File name of output manifest file.");
					Console.Out.WriteLine("-l                    Lists available special folders, and to what");
					Console.Out.WriteLine("                      folders on the computer they are mapped to.");
					Console.Out.WriteLine("-?                    Help.");
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
						string RelativeFileName = Path.GetRelativePath(AbsolutePath, FileName);
						if (RelativeFileName.Contains(Path.DirectorySeparatorChar))
							throw new Exception("Directory separators not permitted in assembly files.");

						AssemblyFiles[RelativeFileName] = true;
					}
				}

				foreach (string FolderName in ContentFolderNames)
					OrderFiles(FolderName, ContentFiles.Folders);

				XmlWriterSettings Settings = XML.WriterSettings(true, false);

				using (XmlWriter Output = XmlWriter.Create(OutputFileName, Settings))
				{
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

						FolderRec ExternalFiles = new FolderRec()
						{
							Folders = new SortedDictionary<string, FolderRec>(StringComparer.InvariantCultureIgnoreCase)
						};

						OrderFiles(Rec.Folder, ExternalFiles.Folders);
						ExportFolder(Output, ExternalFiles.Folders, "File");

						Output.WriteEndElement();
					}

					Output.WriteEndElement();
					Output.WriteEndDocument();
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}

		private static void OrderFiles(string FolderName, SortedDictionary<string, FolderRec> Files)
		{
			string AbsolutePath = Path.GetFullPath(FolderName);
			string[] FileNames = Directory.GetFiles(AbsolutePath, "*.*", SearchOption.AllDirectories);

			foreach (string FileName in FileNames)
			{
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
				if (!(P.Value?.Folders is null))
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
