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
	/// -t                    XSLT transform to use.
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
					Console.Out.WriteLine("<see Waher.Persistence.FilesLW libraries, such as the IoT Gateway database.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-d APP_DATA_FOLDER    Points to the application data folder.");
					Console.Out.WriteLine("-o OUTPUT_FILE        File name of report file.");
					Console.Out.WriteLine("-e                    If encryption is used by the database.");
					Console.Out.WriteLine("-bs BLOCK_SIZE        Block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-enc ENCODING         Text encoding. Default=UTF-8");
					Console.Out.WriteLine("-t                    XSLT transform to use.");
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
							w.WriteStartDocument();

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

							if (!string.IsNullOrEmpty(XsltPath))
								w.WriteProcessingInstruction("xml-stylesheet", "type=\"text/xsl\" href=\"" + XML.Encode(XsltPath) + "\"");

							w.WriteStartElement("DatabaseStatistics", "http://waher.se/Schema/Persistence/Statistics.xsd");

							foreach (ObjectBTreeFile File in FilesProvider.Files)
							{
								w.WriteStartElement("File");
								w.WriteAttributeString("id", File.Id.ToString());
								w.WriteAttributeString("collectionName", File.CollectionName);
								w.WriteAttributeString("fileName", Path.GetRelativePath(ProgramDataFolder, File.FileName));
								w.WriteAttributeString("blockSize", File.BlockSize.ToString());
								w.WriteAttributeString("blobFileName", Path.GetRelativePath(ProgramDataFolder, File.BlobFileName));
								w.WriteAttributeString("blobBlockSize", File.BlobBlockSize.ToString());
								w.WriteAttributeString("count", File.Count.ToString());
								w.WriteAttributeString("encoding", File.Encoding.WebName);
								w.WriteAttributeString("encrypted", CommonTypes.Encode(File.Encrypted));
								w.WriteAttributeString("inlineObjectSizeLimit", File.InlineObjectSizeLimit.ToString());
								w.WriteAttributeString("isReadOnly", CommonTypes.Encode(File.IsReadOnly));
								w.WriteAttributeString("timeoutMs", File.TimeoutMilliseconds.ToString());

								FileStatistics Stat = File.ComputeStatistics().Result;
								WriteStat(w, File, Stat);

								foreach (IndexBTreeFile Index in File.Indices)
								{
									w.WriteStartElement("Index");
									w.WriteAttributeString("id", Index.IndexFile.Id.ToString());
									w.WriteAttributeString("fileName", Path.GetRelativePath(ProgramDataFolder, Index.IndexFile.FileName));
									w.WriteAttributeString("blockSize", Index.IndexFile.BlockSize.ToString());
									w.WriteAttributeString("blobFileName", Index.IndexFile.BlobFileName);
									w.WriteAttributeString("blobBlockSize", Index.IndexFile.BlobBlockSize.ToString());
									w.WriteAttributeString("count", Index.IndexFile.Count.ToString());
									w.WriteAttributeString("encoding", Index.IndexFile.Encoding.WebName);
									w.WriteAttributeString("encrypted", CommonTypes.Encode(Index.IndexFile.Encrypted));
									w.WriteAttributeString("inlineObjectSizeLimit", Index.IndexFile.InlineObjectSizeLimit.ToString());
									w.WriteAttributeString("isReadOnly", CommonTypes.Encode(Index.IndexFile.IsReadOnly));
									w.WriteAttributeString("timeoutMs", Index.IndexFile.TimeoutMilliseconds.ToString());

									foreach (string Field in Index.FieldNames)
										w.WriteElementString("Field", Field);

									Stat = Index.IndexFile.ComputeStatistics().Result;
									WriteStat(w, Index.IndexFile, Stat);

									w.WriteEndElement();
								}

								if (Export)
									File.ExportGraphXML(w, true).Wait();

								w.WriteEndElement();
							}

							w.WriteEndElement();
							w.WriteEndDocument();
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

		private static void WriteStat(XmlWriter w, ObjectBTreeFile File, FileStatistics Stat)
		{
			w.WriteStartElement("Stat");

			if (!double.IsNaN(Stat.AverageBytesUsedPerBlock))
				w.WriteAttributeString("avgBytesPerBlock", CommonTypes.Encode(Stat.AverageBytesUsedPerBlock));

			if (!double.IsNaN(Stat.AverageObjectSize))
				w.WriteAttributeString("avgObjSize", CommonTypes.Encode(Stat.AverageObjectSize));

			if (!double.IsNaN(Stat.AverageObjectsPerBlock))
				w.WriteAttributeString("avgObjPerBlock", CommonTypes.Encode(Stat.AverageObjectsPerBlock));

			w.WriteAttributeString("hasComments", CommonTypes.Encode(Stat.HasComments));
			w.WriteAttributeString("isBalanced", CommonTypes.Encode(Stat.IsBalanced));
			w.WriteAttributeString("isCorrupt", CommonTypes.Encode(Stat.IsCorrupt));
			w.WriteAttributeString("maxBytesPerBlock", Stat.MaxBytesUsedPerBlock.ToString());
			w.WriteAttributeString("maxDepth", Stat.MaxDepth.ToString());
			w.WriteAttributeString("maxObjSize", Stat.MaxObjectSize.ToString());
			w.WriteAttributeString("maxObjPerBlock", Stat.MaxObjectsPerBlock.ToString());
			w.WriteAttributeString("minBytesPerBlock", Stat.MinBytesUsedPerBlock.ToString());
			w.WriteAttributeString("minDepth", Stat.MinDepth.ToString());
			w.WriteAttributeString("minObjSize", Stat.MinObjectSize.ToString());
			w.WriteAttributeString("minObjPerBlock", Stat.MinObjectsPerBlock.ToString());
			w.WriteAttributeString("nrBlobBlocks", Stat.NrBlobBlocks.ToString());
			w.WriteAttributeString("nrBlobBytes", Stat.NrBlobBytesTotal.ToString());
			w.WriteAttributeString("nrBlobBytesUnused", Stat.NrBlobBytesUnused.ToString());
			w.WriteAttributeString("nrBlobBytesUsed", Stat.NrBlobBytesUsed.ToString());
			w.WriteAttributeString("nrBlocks", Stat.NrBlocks.ToString());
			w.WriteAttributeString("nrBytes", Stat.NrBytesTotal.ToString());
			w.WriteAttributeString("nrBytesUnused", Stat.NrBytesUnused.ToString());
			w.WriteAttributeString("nrBytesUsed", Stat.NrBytesUsed.ToString());
			w.WriteAttributeString("nrObjects", Stat.NrObjects.ToString());
			w.WriteAttributeString("usage", CommonTypes.Encode(Stat.Usage));

			if (Stat.NrBlobBytesTotal > 0)
				w.WriteAttributeString("blobUsage", CommonTypes.Encode((100.0 * Stat.NrBlobBytesUsed) / Stat.NrBlobBytesTotal));

			if (Stat.HasComments)
			{
				foreach (string Comment in Stat.Comments)
					w.WriteElementString("Comment", Comment);
			}

			w.WriteEndElement();
		}

	}
}
