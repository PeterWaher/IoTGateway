using System;
using System.IO;
using System.Text;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Content.Html;
using Waher.Content.Html.JavaScript;
using Waher.Content.Images;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.JavaScript;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Markdown.Xamarin;
using Waher.Content.Markdown.Xml;
using Waher.Content.Text;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Networking.XMPP.Contracts;
using Waher.Runtime.Console;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Script.Graphs;

namespace Waher.Utility.Markdown
{
	class Program
	{
		/// <summary>
		/// Markdown conversion tool.
		/// 
		/// Command line switches:
		/// 
		/// -i FILENAME           Filename of the input file.
		/// -o FILENAME           Filename of the output file. The extension
		///                       determines the output format.
		/// -r FOLDER             Defines a root content folder.
		/// -enc ENCODING         Text encoding if Byte-order-marks not available.
		///                       Default=UTF-8
		/// -s                    Allow script
		/// -h                    Parse Markdown headers.
		/// -?                    Help.
		/// 
		/// LaTeX settings:
		/// 
		/// -ldc CLASS            LaTeX document class. Default=Article
		///                       Possible values: Article, Report, Book, Standalone
		/// -lpf PAPER            LaTeX paper format. Default=A4
		///                       Possible values: Letter, A4
		/// -lfs SIZE             Default font size. Default=10
		/// </summary>
		static int Main(string[] args)
		{
			MarkdownSettings Settings = new();
			HtmlSettings HtmlSettings = new();
			LaTeXSettings LatexSettings = new();
			Content.Markdown.Wpf.XamlSettings XamlSettings = new();
			Content.Markdown.Xamarin.XamlSettings XamarinSettings = new();
			Encoding Encoding = Encoding.UTF8;
			string InputFileName = null;
			string OutputFileName = null;
			bool Help = false;
			int i = 0;
			int c = args.Length;
			string s;

			try
			{
				while (i < c)
				{
					s = args[i++].ToLower();

					switch (s)
					{
						case "-i":
							if (i >= c)
								throw new Exception("Missing input filename.");

							if (string.IsNullOrEmpty(InputFileName))
								InputFileName = args[i++];
							else
								throw new Exception("Only one input file name allowed.");
							break;

						case "-o":
							if (i >= c)
								throw new Exception("Missing output filename.");

							if (string.IsNullOrEmpty(OutputFileName))
								OutputFileName = args[i++];
							else
								throw new Exception("Only one output file name allowed.");
							break;

						case "-r":
							if (i >= c)
								throw new Exception("Missing root folder.");

							if (string.IsNullOrEmpty(Settings.RootFolder))
								Settings.RootFolder = args[i++];
							else
								throw new Exception("Only one root folder allowed.");
							break;

						case "-enc":
							if (i >= c)
								throw new Exception("Text encoding missing.");

							Encoding = Encoding.GetEncoding(args[i++]);
							break;

						case "-h":
							Settings.ParseMetaData = true;
							break;

						case "-s":
							Settings.Variables = [];
							Settings.AllowInlineScript = true;
							break;

						case "-?":
							Help = true;
							break;

						case "-ldc":
							if (i >= c)
								throw new Exception("LaTeX Document Class missing.");

							if (!Enum.TryParse(args[i++], out LaTeXDocumentClass DocumentClass))
								throw new Exception("Invalid LaTeX Document Class.");

							LatexSettings.DocumentClass = DocumentClass;
							break;

						case "-lpf":
							if (i >= c)
								throw new Exception("LaTeX Paper Format missing.");

							if (!Enum.TryParse(args[i++], out LaTeXPaper PaperFormat))
								throw new Exception("Invalid LaTeX Paper Format.");

							LatexSettings.PaperFormat = PaperFormat;
							break;

						case "-lfc":
							if (i >= c)
								throw new Exception("LaTeX Font Size missing.");

							if (!int.TryParse(args[i++], out int FontSize))
								throw new Exception("Invalid LaTeX Font Size.");

							LatexSettings.DefaultFontSize = FontSize;
							break;

						default:
							throw new Exception("Unrecognized switch: " + s);
					}
				}

				if (Help || c == 0)
				{
					ConsoleOut.WriteLine("Markdown conversion tool.");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("Command line switches:");
					ConsoleOut.WriteLine();
					ConsoleOut.WriteLine("-i FILENAME           Filename of the input file.");
					ConsoleOut.WriteLine("-o FILENAME           Filename of the output file. The extension");
					ConsoleOut.WriteLine("                      determines the output format.");
					ConsoleOut.WriteLine("-r FOLDER             Defines a root content folder.");
					ConsoleOut.WriteLine("-enc ENCODING         Text encoding if Byte-order-marks not available.");
					ConsoleOut.WriteLine("                      Default=UTF-8");
					ConsoleOut.WriteLine("-s                    Allow script");
					ConsoleOut.WriteLine("-h                    Parse Markdown headers.");
					ConsoleOut.WriteLine("-?                    Help.");
					ConsoleOut.WriteLine(string.Empty);
					ConsoleOut.WriteLine("LaTeX settings:");
					ConsoleOut.WriteLine(string.Empty);
					ConsoleOut.WriteLine("-ldc CLASS            LaTeX document class. Default=Article");
					ConsoleOut.WriteLine("                      Possible values: Article, Report, Book, Standalone");
					ConsoleOut.WriteLine("-lpf PAPER            LaTeX paper format. Default=A4");
					ConsoleOut.WriteLine("                      Possible values: Letter, A4");
					ConsoleOut.WriteLine("-lfs SIZE             Default font size. Default=10");
					return 0;
				}

				if (string.IsNullOrEmpty(InputFileName))
					throw new Exception("No intput file name specified.");

				if (string.IsNullOrEmpty(OutputFileName))
					throw new Exception("No output file name specified.");

				Types.Initialize(
					typeof(Program).Assembly,
					typeof(Expression).Assembly,
					typeof(Graph).Assembly,
					typeof(InternetContent).Assembly,
					typeof(HtmlCodec).Assembly,
					typeof(ImageCodec).Assembly,
					typeof(XmlCodec).Assembly,
					typeof(MarkdownDocument).Assembly,
					typeof(ContractsRenderer).Assembly,
					typeof(JavaScriptRenderer).Assembly,
					typeof(LatexRenderer).Assembly,
					typeof(WpfXamlRenderer).Assembly,
					typeof(XamarinFormsXamlRenderer).Assembly,
					typeof(XmlRenderer).Assembly);

				string InputContentType = InternetContent.GetContentType(Path.GetExtension(InputFileName));
				if (string.IsNullOrEmpty(InputContentType))
					throw new Exception("Unable to determine content type of input file.");

				if (InputContentType != MarkdownCodec.ContentType)
					throw new Exception("Input file is not a Markdown file.");

				string OutputExtension = Path.GetExtension(OutputFileName);
				string OutputContentType = InternetContent.GetContentType(OutputExtension);
				IRenderer Renderer = null;

				if (string.IsNullOrEmpty(OutputContentType) || OutputContentType == BinaryCodec.DefaultContentType)
				{
					if (OutputExtension.StartsWith('.'))
						OutputExtension = OutputExtension[1..];

					Renderer = OutputExtension switch
					{
						"tex" => new LatexRenderer(LatexSettings),
						"xaml" => new WpfXamlRenderer(XML.WriterSettings(true, true), XamlSettings),
						"xf" => new XamarinFormsXamlRenderer(XML.WriterSettings(true, true), XamarinSettings),
						_ => throw new Exception("Unable to determine content type of output file."),
					};
				}

				InputFileName = Path.GetFullPath(InputFileName);

				if (string.IsNullOrEmpty(Settings.RootFolder))
					Settings.RootFolder = Path.GetDirectoryName(InputFileName);

				string Markdown = File.ReadAllText(InputFileName, Encoding);
				MarkdownDocument Doc = MarkdownDocument.CreateAsync(Markdown, Settings, InputFileName, null, null).Result;

				if (Renderer is null)
				{
					if (Array.IndexOf(HtmlCodec.HtmlContentTypes, OutputContentType) >= 0)
						Renderer = new HtmlRenderer(HtmlSettings);
					else if (string.Compare(MarkdownCodec.ContentType, OutputContentType, true) == 0)
						Renderer = new MarkdownRenderer();
					else if (Array.IndexOf(PlainTextCodec.PlainTextContentTypes, OutputContentType) >= 0)
						Renderer = new TextRenderer();
					else if (Array.IndexOf(XmlCodec.XmlContentTypes, OutputContentType) >= 0)
						Renderer = new ContractsRenderer(XML.WriterSettings(true, true), "Root", ContractsClient.NamespaceSmartContractsCurrent);
					else if (Array.IndexOf(JavaScriptCodec.JavaScriptContentTypes, OutputContentType) >= 0)
						Renderer = new JavaScriptRenderer(HtmlSettings);
					else
						throw new Exception("Unable to convert Markdown file to " + OutputContentType);
				}

				Doc.RenderDocument(Renderer).Wait();

				Files.WriteAllTextAsync(OutputFileName, Renderer.ToString(), Encoding).Wait();

				return 0;
			}
			catch (Exception ex)
			{
				ConsoleOut.WriteLine(ex.Message);
				return -1;
			}
			finally
			{
				ConsoleOut.Flush(true);
			}
		}
	}
}
