using System.Text;
using Waher.Content;
using Waher.Runtime.Text;

namespace Waher.Utility.TextDiff
{
	class Program
	{
		/// <summary>
		/// Compares two text files and outputs the differences between them.
		/// 
		/// Command line switches:
		/// 
		/// -f FROM_FILE     File name of XML file.
		/// -t TO_FILE       XSLT transform to use.
		/// -o OUTPUT_FILE   File name of output file.
		///                  If no output file is provided, output will be
		///                  made to the console.
		/// -enc ENCODING    Text encoding. Default=UTF-8
		/// -?               Help.
		/// </summary>
		static int Main(string[] args)
		{
			try
			{
				Encoding Encoding = Encoding.UTF8;
				string? FromFileName = null;
				string? ToFileName = null;
				string? OutputFileName = null;
				string s;
				int i = 0;
				int c = args.Length;
				bool Help = false;

				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-o":
							if (i >= c)
								throw new Exception("Missing output file name.");

							if (string.IsNullOrEmpty(OutputFileName))
								OutputFileName = args[i++];
							else
								throw new Exception("Only one output file name allowed.");
							break;

						case "-f":
							if (i >= c)
								throw new Exception("Missing from file name.");

							if (string.IsNullOrEmpty(FromFileName))
								FromFileName = args[i++];
							else
								throw new Exception("Only one from file name allowed.");
							break;

						case "-t":
							if (i >= c)
								throw new Exception("Missing to file name.");

							if (string.IsNullOrEmpty(ToFileName))
								ToFileName = args[i++];
							else
								throw new Exception("Only one to file name allowed.");
							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
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
					Console.Out.WriteLine("Compares two text files and outputs the differences between them.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-f FROM_FILE     File name of XML file.");
					Console.Out.WriteLine("-t TO_FILE       XSLT transform to use.");
					Console.Out.WriteLine("-o OUTPUT_FILE   File name of output file.");
					Console.Out.WriteLine("                 If no output file is provided, output will be");
					Console.Out.WriteLine("                 made to the console.");
					Console.Out.WriteLine("-enc ENCODING    Text encoding. Default=UTF-8");
					Console.Out.WriteLine("-?               Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(FromFileName))
					throw new Exception("No from filename specified.");

				string From = ReadTextFile(FromFileName, Encoding);
				string[] FromRows = Difference.ExtractRows(From);

				if (string.IsNullOrEmpty(ToFileName))
					throw new Exception("No to filename specified.");

				string To = ReadTextFile(ToFileName, Encoding);
				string[] ToRows = Difference.ExtractRows(To);

				EditScript<string> Script = Difference.Analyze(FromRows, ToRows);
				TextWriter Output;
				bool DisposeOutput;

				if (string.IsNullOrEmpty(OutputFileName))
				{
					Output = Console.Out;
					DisposeOutput = false;
				}
				else
				{
					Output = new StreamWriter(OutputFileName, Encoding, new FileStreamOptions()
					{
						Access = FileAccess.Write,
						Mode = FileMode.CreateNew
					});
					DisposeOutput = true;
				}

				try
				{
					int NrAdded = 0;
					int NrRemoved = 0;

					foreach (Step<string> Step in Script)
					{
						switch (Step.Operation)
						{
							case EditOperation.Delete:
								NrRemoved++;
								break;

							case EditOperation.Insert:
								NrAdded++;
								break;
						}
					}

					if (NrAdded == 0 && NrRemoved == 0)
						Output.WriteLine("Files are identical.");
					else
					{
						if (NrAdded == 1)
							Output.WriteLine("1 row added.");
						else if (NrAdded > 1)
							Output.WriteLine(NrAdded.ToString() + " rows added.");

						if (NrRemoved == 1)
							Output.WriteLine("1 row removed.");
						else if (NrRemoved > 1)
							Output.WriteLine(NrRemoved.ToString() + " rows removed.");

						Output.WriteLine();

						foreach (Step<string> Step in Script)
						{
							int i1 = Step.Index1;
							int i2 = Step.Index2;

							switch (Step.Operation)
							{
								case EditOperation.Delete:
									foreach (string Row in Step.Symbols)
									{
										Output.Write((i1++).ToString());
										Output.Write('/');
										Output.Write(i2.ToString());
										Output.Write("\t-\t");
										Output.WriteLine(Row);
									}
									break;

								case EditOperation.Insert:
									foreach (string Row in Step.Symbols)
									{
										Output.Write(i1.ToString());
										Output.Write('/');
										Output.Write((i2++).ToString());
										Output.Write("\t+\t");
										Output.WriteLine(Row);
									}
									break;
							}
						}
					}
				}
				finally
				{
					if (DisposeOutput)
						Output.Dispose();
				}

				return 0;
			}
			catch (Exception ex)
			{
				Console.Out.WriteLine(ex.Message);
				return -1;
			}
		}

		private static string ReadTextFile(string FileName, Encoding DefaultEncoding)
		{
			byte[] Data = File.ReadAllBytes(FileName);
			return CommonTypes.GetString(Data, DefaultEncoding);
		}
	}
}
