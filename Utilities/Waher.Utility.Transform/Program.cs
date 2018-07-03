using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using Waher.Content.Xsl;

namespace Waher.Utility.Transform
{
    class Program
    {
		/// <summary>
		/// Transforms an XML file using an XSL Transform (XSLT) file.
		/// 
		/// Command line switches:
		/// 
		/// -i INPUT_FILE         File name of XML file.
		/// -t TRANSFORM_FILE     XSLT transform to use.
		/// -o OUTPUT_FILE        File name of output file.
		/// -enc ENCODING         Text encoding. Default=UTF-8
		/// -?                    Help.
		/// </summary>
		static int Main(string[] args)
		{
			try
			{
				Encoding Encoding = Encoding.UTF8;
				string InputFileName = null;
				string OutputFileName = null;
				string XsltPath = null;
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

						case "-i":
							if (i >= c)
								throw new Exception("Missing input file name.");

							if (string.IsNullOrEmpty(InputFileName))
								InputFileName = args[i++];
							else
								throw new Exception("Only one input file name allowed.");
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

						case "-?":
							Help = true;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					Console.Out.WriteLine("Transforms an XML file using an XSL Transform (XSLT) file.");
					Console.Out.WriteLine();
					Console.Out.WriteLine("Command line switches:");
					Console.Out.WriteLine();
					Console.Out.WriteLine("-i INPUT_FILE         File name of XML file.");
					Console.Out.WriteLine("-t TRANSFORM_FILE     XSLT transform to use.");
					Console.Out.WriteLine("-o OUTPUT_FILE        File name of output file.");
					Console.Out.WriteLine("-enc ENCODING         Text encoding. Default=UTF-8");
					Console.Out.WriteLine("-?                    Help.");
					return 0;
				}

				if (string.IsNullOrEmpty(InputFileName))
					throw new Exception("No input filename specified.");

				if (string.IsNullOrEmpty(XsltPath))
					throw new Exception("No transform filename specified.");

				if (string.IsNullOrEmpty(OutputFileName))
					throw new Exception("No output filename specified.");

				string Xml = File.ReadAllText(InputFileName);

				using (Stream f = File.OpenRead(XsltPath))
				{
					using (XmlReader r = XmlReader.Create(f))
					{
						XslCompiledTransform Xslt = new XslCompiledTransform();
						Xslt.Load(r);

						Xml = XSL.Transform(Xml, Xslt);

						File.WriteAllText(OutputFileName, Xml);
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
