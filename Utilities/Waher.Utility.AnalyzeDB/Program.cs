using System;
using System.IO;
using System.Text;
using System.Xml;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Files.Statistics;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Utility.AnalyzeDB
{
	/// <summary>
	/// Analyzes an object database created by the <see cref="Waher.Persistence.Files"/> or 
	/// <see cref="Waher.Persistence.FilesLW"/> libraries, such as the IoT Gateway database.
	/// 
	/// Command line switches:
	/// 
	/// -d APP_DATA_FOLDER    Points to the application data folder.
	/// -o OUTPUT_FILE        File name of report file.
	/// -e                    If encryption is used by the database.
	/// -bs BLOCK_SIZE        Block size, in bytes. Default=8192.
	/// -bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.
	/// -enc ENCODING         Text encoding. Default=UTF-8
	/// -t TRANSFORM_FILE     XSLT transform to use.
	/// -x                    Export contents of each collection.
	/// -?                    Help.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				Encoding Encoding = Encoding.UTF8;
				string ProgramDataFolder = null;
				string OutputFileName = null;
				string XsltPath = null;
				string s;
				int BlockSize = 8192;
				int BlobBlockSize = 8192;
				int i = 0;
				int c = args.Length;
				bool Help = false;
				bool Encryption = false;
				bool Export = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-d":
							if (i >= c)
								throw new Exception("Missing program data folder.");

							if (string.IsNullOrEmpty(ProgramDataFolder))
								ProgramDataFolder = args[i++];
							else
								throw new Exception("Only one program data folder allowed.");
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing output file name.");

							if (string.IsNullOrEmpty(OutputFileName))
								OutputFileName = args[i++];
							else
								throw new Exception("Only one output file name allowed.");
							break;

						case "-bs":
							if (i >= c)
								throw new Exception("Block size missing.");

							if (!int.TryParse(args[i++], out BlockSize))
								throw new Exception("Invalid block size");

							break;

						case "-bbs":
							if (i >= c)
								throw new Exception("Blob Block size missing.");

							if (!int.TryParse(args[i++], out BlobBlockSize))
								throw new Exception("Invalid blob block size");

							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
							break;

						case "-t":
							if (i >= c)
								throw new Exception("XSLT transform missing.");

							XsltPath = args[i++];
							break;

						case "-e":
							Encryption = true;
							break;

						case "-x":
							Export = true;
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
					Console.Out.WriteLine("Analyzes an object database created by the Waher.Persistence.Files or");
					Console.Out.WriteLine("Waher.Persistence.FilesLW libraries, such as the IoT Gateway database.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-d APP_DATA_FOLDER    Points to the application data folder.");
					Console.Out.WriteLine("-o OUTPUT_FILE        File name of report file.");
					Console.Out.WriteLine("-e                    If encryption is used by the database.");
					Console.Out.WriteLine("-bs BLOCK_SIZE        Block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-enc ENCODING         Text encoding. Default=UTF-8");
					Console.Out.WriteLine("-t TRANSFORM_FILE     XSLT transform to use.");
					Console.Out.WriteLine("-x                    Export contents of each collection.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(ProgramDataFolder))
					throw new Exception("No program data folder set");

				if (!Directory.Exists(ProgramDataFolder))
					throw new Exception("Program data folder does not exist.");

				if (string.IsNullOrEmpty(OutputFileName))
					throw new Exception("No output filename specified.");

				Types.Initialize(
					typeof(Database).Assembly,
					typeof(FilesProvider).Assembly);

				using (FilesProvider FilesProvider = new FilesProvider(ProgramDataFolder, "Default", BlockSize, 10000, BlobBlockSize, Encoding, 10000, Encryption, false))
				{
					Database.Register(FilesProvider);

					using (StreamWriter f = File.CreateText(OutputFileName))
					{
						XmlWriterSettings Settings = new XmlWriterSettings()
						{
							Encoding = Encoding,
							Indent = true,
							IndentChars = "\t",
							NewLineChars = Console.Out.NewLine,
							OmitXmlDeclaration = false,
							WriteEndDocumentOnClose = true
						};

						using (XmlWriter w = XmlWriter.Create(f, Settings))
						{
							if (string.IsNullOrEmpty(XsltPath))
							{
								i = ProgramDataFolder.LastIndexOf(Path.DirectorySeparatorChar);
								if (i > 0)
								{
									s = Path.Combine(ProgramDataFolder.Substring(0, i), "Transforms", "DbStatXmlToHtml.xslt");
									if (File.Exists(s))
										XsltPath = s;
								}
							}

							Database.Analyze(w, XsltPath, ProgramDataFolder, Export);
						}
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
