using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content.Asn1;

namespace Waher.Utility.Asn1ToCSharp
{
	/// <summary>
	/// Creates C# files from definitions made in ASN.1 files.
	/// 
	/// Command line switches:
	/// 
	/// -BER                  Adds support for BER enconding.
	/// -CER                  Adds support for CER enconding.
	/// -DER                  Adds support for DER enconding.
	/// -n NAMESPACE          Defines a base namespace. Can be
	///                       used multiple times, to place code
	///                       in different namespaces.
	/// -o FOLDER             Path to folder where C# files will be
	///                       stored. Can be used multiple times
	///                       in one call, to place different files
	///                       in different output folders.
	/// -f IMPORT_FOLDER      Path to a folder where import files
	///                       may be stored. Can be used multiple
	///                       times to define different folders.
	/// -t                    Only load files from top input 
	///                       folder(s).
	/// -r                    Load files recursively from input
	///                       folder(s) and their subfolders.
	/// -i PATTERN            Search pattern for ASN.1 files, or 
	///                       single ASN.1 file to convert to C#.
	/// -?                    Help.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				CSharpExportSettings Settings = new CSharpExportSettings(string.Empty, EncodingSchemes.None);
				List<string> ImportFolders = new List<string>();
				Dictionary<string, bool> ExportedModules = new Dictionary<string, bool>();
				SearchOption SearchOption = SearchOption.TopDirectoryOnly;
				string OutputFolder = null;
				string s;
				int i = 0;
				int c = args.Length;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-ber":
							Settings.Codecs |= EncodingSchemes.BER;
							break;

						case "-cer":
							Settings.Codecs |= EncodingSchemes.CER;
							break;

						case "-der":
							Settings.Codecs |= EncodingSchemes.DER;
							break;

						case "-n":
							if (i >= c)
								throw new Exception("Missing namespace.");

							Settings.BaseNamespace = args[i++];
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing output folder.");

							OutputFolder = Path.GetFullPath(args[i++]);

							if (!Directory.Exists(OutputFolder))
								Directory.CreateDirectory(OutputFolder);
							break;

						case "-f":
							if (i >= c)
								throw new Exception("Missing import folder.");

							s = Path.GetFullPath(args[i++]);

							if (!Directory.Exists(s))
								throw new Exception("Import folder does not exist: " + s);

							if (!ImportFolders.Contains(s))
								ImportFolders.Add(s);
							break;

						case "-t":
							SearchOption = SearchOption.TopDirectoryOnly;
							break;

						case "-r":
							SearchOption = SearchOption.AllDirectories;
							break;

						case "-i":

							if (string.IsNullOrWhiteSpace(Settings.BaseNamespace))
								throw new Exception("No base namespace provided.");

							if (string.IsNullOrWhiteSpace(OutputFolder))
								throw new Exception("No output folder provided.");

							if (i >= c)
								throw new Exception("Missing import folder.");

							s = args[i++];

							List<string> InputFiles = new List<string>();

							if (File.Exists(s))
								InputFiles.Add(s);
							else
							{
								string Pattern = Path.GetFileName(s);
								string Folder = Path.GetFullPath(Path.GetDirectoryName(s));

								foreach (string FileName in Directory.GetFiles(Folder, Pattern, SearchOption))
									InputFiles.Add(FileName);
							}

							foreach (string FileName in InputFiles)
							{
								Console.Out.WriteLine("Loading " + FileName + "...");

								Asn1Document Doc = Asn1Document.FromFile(FileName, ImportFolders.ToArray());

								Console.Out.WriteLine("Generating C#...");

								string CSharp = Doc.ExportCSharp(Settings);
								string OutputFileName = Path.Combine(OutputFolder, Path.ChangeExtension(Path.GetFileName(FileName), "cs"));

								Console.Out.WriteLine("Exporting " + OutputFileName + "...");

								File.WriteAllText(OutputFileName, CSharp, Encoding.UTF8);

								foreach (string ImportedModule in Settings.Modules)
								{
									if (ExportedModules.ContainsKey(ImportedModule))
										continue;

									CSharp = Settings.GetCode(ImportedModule);
									OutputFileName = Path.Combine(OutputFolder, Path.ChangeExtension(ImportedModule, "cs"));
									
									Console.Out.WriteLine("Exporting " + OutputFileName + "...");

									File.WriteAllText(OutputFileName, CSharp, Encoding.UTF8);

									ExportedModules[ImportedModule] = true;
								}
							}

							break;

						case "-?":
							Console.Out.WriteLine("Creates C# files from definitions made in ASN.1 files.");
							Console.Out.WriteLine();
							Console.Out.WriteLine("Command line switches:");
							Console.Out.WriteLine();
							Console.Out.WriteLine("-BER                  Adds support for BER enconding.");
							Console.Out.WriteLine("-CER                  Adds support for CER enconding.");
							Console.Out.WriteLine("-DER                  Adds support for DER enconding.");
							Console.Out.WriteLine("-n NAMESPACE          Defines a base namespace. Can be");
							Console.Out.WriteLine("                      used multiple times, to place code");
							Console.Out.WriteLine("                      in different namespaces.");
							Console.Out.WriteLine("-o FOLDER             Path to folder where C# files will be");
							Console.Out.WriteLine("                      stored. Can be used multiple times");
							Console.Out.WriteLine("                      in one call, to place different files");
							Console.Out.WriteLine("                      in different output folders.");
							Console.Out.WriteLine("-f IMPORT_FOLDER      Path to a folder where import files");
							Console.Out.WriteLine("                      may be stored. Can be used multiple");
							Console.Out.WriteLine("                      times to define different folders.");
							Console.Out.WriteLine("-t                    Only load files from top input");
							Console.Out.WriteLine("                      folder(s).");
							Console.Out.WriteLine("-r                    Load files recursively from input");
							Console.Out.WriteLine("                      folder(s) and their subfolders.");
							Console.Out.WriteLine("-i PATTERN            Search pattern for ASN.1 files, or");
							Console.Out.WriteLine("                      single ASN.1 file to convert to C#.");
							Console.Out.WriteLine("-?                    Help.");
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}
	}
}
