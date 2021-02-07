using System;
using System.IO;
using System.Text;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script;

namespace Waher.Utility.RunScript
{
	/// <summary>
	/// Allows you to execute script.
	/// 
	/// Command line switches:
	/// 
	/// -i SCRIPT_FILE        Points to the script file to execute.
	/// -d APP_DATA_FOLDER    Points to the application data folder.
	///                       If specified, a connection to a files
	///                       object database (Waher.Persistence.Files)
	///                       will be established.
	/// -e                    If encryption is used by the database.
	/// -bs BLOCK_SIZE        Block size, in bytes. Default=8192.
	/// -bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.
	/// -?                    Help.
	/// </summary>
	class Program
	{
		static int Main(string[] args)
		{
			FilesProvider FilesProvider = null;
			Encoding Encoding = Encoding.UTF8;
			string ProgramDataFolder = null;
			string ScriptFile = null;
			string s;
			int BlockSize = 8192;
			int BlobBlockSize = 8192;
			int i = 0;
			int c = args.Length;
			bool Help = false;
			bool Encryption = false;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-i":
							if (i >= c)
								throw new Exception("Missing script file.");

							if (string.IsNullOrEmpty(ScriptFile))
								ScriptFile = args[i++];
							else
								throw new Exception("Only one script file allowed.");
							break;

						case "-d":
							if (i >= c)
								throw new Exception("Missing program data folder.");

							if (string.IsNullOrEmpty(ProgramDataFolder))
								ProgramDataFolder = args[i++];
							else
								throw new Exception("Only one program data folder allowed.");
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

						case "-e":
							Encryption = true;
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
					Console.Out.WriteLine("Allows you to execute script.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-i SCRIPT_FILE        Points to the script file to execute.");
					Console.Out.WriteLine("-d APP_DATA_FOLDER    Points to the application data folder.");
					Console.Out.WriteLine("                      If specified, a connection to a files");
					Console.Out.WriteLine("                      object database (Waher.Persistence.Files)");
					Console.Out.WriteLine("                      will be established.");
					Console.Out.WriteLine("-e                    If encryption is used by the database.");
					Console.Out.WriteLine("-bs BLOCK_SIZE        Block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-bbs BLOB_BLOCK_SIZE  BLOB block size, in bytes. Default=8192.");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(ScriptFile))
					throw new Exception("No script file provided.");

				Types.Initialize(
					typeof(Log).Assembly,
					typeof(Expression).Assembly,
					typeof(Database).Assembly,
					typeof(FilesProvider).Assembly,
					typeof(ObjectSerializer).Assembly,
					typeof(Content.InternetContent).Assembly,
					typeof(Content.Html.HtmlDocument).Assembly,
					typeof(Content.Images.ImageCodec).Assembly,
					typeof(Content.Xml.XML).Assembly,
					typeof(Content.Xsl.XSL).Assembly,
					typeof(Events.Console.ConsoleEventSink).Assembly,
					typeof(Script.Content.Functions.Encoding.Base64Decode).Assembly,
					typeof(Script.Cryptography.Functions.RandomBytes).Assembly,
					typeof(Script.Fractals.FractalGraph).Assembly,
					typeof(Script.Graphs.Graph).Assembly,
					typeof(Script.Graphs3D.Canvas3D).Assembly,
					typeof(Script.Networking.Functions.Dns).Assembly,
					typeof(Script.Persistence.Functions.DeleteObject).Assembly,
					typeof(Script.Statistics.StatMath).Assembly,
					typeof(Script.Xml.XmlOutput).Assembly);

				string Script = File.ReadAllText(ScriptFile);
				Expression Parsed = new Expression(Script);
				
				Log.Register(new Events.Console.ConsoleEventSink());

				if (!string.IsNullOrEmpty(ProgramDataFolder))
				{
					if (!Directory.Exists(ProgramDataFolder))
						throw new Exception("Program data folder does not exist.");

					FilesProvider = FilesProvider.CreateAsync(ProgramDataFolder, "Default", BlockSize, 10000, BlobBlockSize, Encoding, 3600000, Encryption, false).Result;
					Database.Register(FilesProvider);

					FilesProvider.RepairIfInproperShutdown(string.Empty).Wait();
					FilesProvider.Start().Wait();
				}

				Variables Variables = new Variables();
				Variables.ConsoleOut = Console.Out;

				object Result = Parsed.Evaluate(Variables);

				Console.Out.WriteLine(Expression.ToString(Result));

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				Log.Terminate();

				if (!(FilesProvider is null))
				{
					FilesProvider.Stop().Wait();
					FilesProvider?.Dispose();
				}
			}
		}

	}
}
