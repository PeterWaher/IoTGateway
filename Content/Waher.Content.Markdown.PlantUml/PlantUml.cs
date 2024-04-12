using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.CodeContent;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Markdown.Xamarin;
using Waher.Content.SystemFiles;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Security;
using static System.Environment;

namespace Waher.Content.Markdown.PlantUml
{
	/// <summary>
	/// Class managing PlantUML integration into Markdown documents.
	/// </summary>
	public class PlantUml : IImageCodeContent, ICodeContentHtmlRenderer, ICodeContentTextRenderer, ICodeContentMarkdownRenderer,
		ICodeContentContractsRenderer, ICodeContentLatexRenderer, ICodeContentWpfXamlRenderer, ICodeContentXamarinFormsXamlRenderer
	{
		private static readonly Random rnd = new Random();
		private static Scheduler scheduler = null;
		private static string jarPath = null;
		private static string javaPath = null;
		private static string plantUmlFolder = null;
		private static string contentRootFolder = null;
		private static string defaultBgColor = null;
		private static string defaultFgColor = null;
		private static IMarkdownAsynchronousOutput asyncHtmlOutput = null;

		/// <summary>
		/// Class managing PlantUML integration into Markdown documents.
		/// </summary>
		public PlantUml()
		{
		}

		/// <summary>
		/// Initializes the PlantUML-Markdown integration.
		/// </summary>
		/// <param name="ContentRootFolder">Content root folder. If hosting markdown under a web server, this would correspond
		/// to the roof folder for the web content.</param>
		public static void Init(string ContentRootFolder)
		{
			try
			{
				contentRootFolder = ContentRootFolder;

				if (scheduler is null)
				{
					if (Types.TryGetModuleParameter("Scheduler", out object Obj) && Obj is Scheduler Scheduler)
						scheduler = Scheduler;
					else
					{
						scheduler = new Scheduler();

						Log.Terminating += (sender, e) =>
						{
							scheduler?.Dispose();
							scheduler = null;
						};
					}
				}

				SearchForInstallationFolder(out string JarPath, out string JavaPath);

				if (string.IsNullOrEmpty(JarPath))
					Log.Warning("PlantUML not found. PlantUML support will not be available in Markdown.");
				else if (string.IsNullOrEmpty(JavaPath))
					Log.Warning("Java not found. PlantUML support will not be available in Markdown.");
				else
				{
					SetPath(JarPath, JavaPath);

					Log.Informational("PlantUML found. Integration with Markdown added.",
						new KeyValuePair<string, object>("Path", jarPath),
						new KeyValuePair<string, object>("Java", javaPath));


					asyncHtmlOutput = Types.FindBest<IMarkdownAsynchronousOutput, MarkdownOutputType>(MarkdownOutputType.Html);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Sets the full path of PlantUML.
		/// </summary>
		/// <param name="JarPath">Full path of plantuml.jar</param>
		/// <param name="JavaPath">Full path of java.exe</param>
		/// <exception cref="Exception">If trying to set the path to a different path than the one set previously.
		/// The path can only be set once, for security reasons.</exception>
		public static void SetPath(string JarPath, string JavaPath)
		{
			if (!string.IsNullOrEmpty(jarPath) && JarPath != jarPath)
				throw new Exception("PlantUML an Java paths have already been set.");

			jarPath = JarPath;
			javaPath = JavaPath;
			plantUmlFolder = Path.Combine(contentRootFolder, "PlantUML");

			if (!Directory.Exists(plantUmlFolder))
				Directory.CreateDirectory(plantUmlFolder);

			DeleteOldFiles(TimeSpan.FromDays(7));
		}

		private static void DeleteOldFiles(object P)
		{
			if (P is TimeSpan MaxAge)
				DeleteOldFiles(MaxAge, true);
		}

		/// <summary>
		/// Deletes generated files older than <paramref name="MaxAge"/>.
		/// </summary>
		/// <param name="MaxAge">Age limit.</param>
		/// <param name="Reschedule">If rescheduling should be done.</param>
		public static void DeleteOldFiles(TimeSpan MaxAge, bool Reschedule)
		{
			if (string.IsNullOrEmpty(plantUmlFolder))
				return;

			DateTime Limit = DateTime.Now - MaxAge;
			int Count = 0;

			DirectoryInfo PlantUmlFolder = new DirectoryInfo(plantUmlFolder);
			FileInfo[] Files = PlantUmlFolder.GetFiles("*.*");

			foreach (FileInfo FileInfo in Files)
			{
				if (FileInfo.LastAccessTime < Limit)
				{
					try
					{
						File.Delete(FileInfo.FullName);
						Count++;
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileInfo.FullName);
					}
				}
			}

			if (Count > 0)
				Log.Informational(Count.ToString() + " old file(s) deleted.", plantUmlFolder);

			if (Reschedule)
			{
				lock (rnd)
				{
					scheduler.Add(DateTime.Now.AddDays(rnd.NextDouble() * 2), DeleteOldFiles, MaxAge);
				}
			}
		}

		/// <summary>
		/// Searches for the installation folder on the local machine.
		/// </summary>
		/// <param name="JarPath">Path to PlantUML Jar file.</param>
		/// <param name="JavaPath">Path to Java VM.</param>
		public static void SearchForInstallationFolder(out string JarPath, out string JavaPath)
		{
			List<string> Folders = new List<string>();

			Folders.AddRange(FileSystem.GetFolders(new Environment.SpecialFolder[]
			{
				SpecialFolder.ProgramFiles,
				SpecialFolder.ProgramFilesX86
			}));


			if (Types.TryGetModuleParameter("Runtime", out object Obj) && Obj is string RuntimeFolder)
			{
				Folders.Add(Path.Combine(RuntimeFolder, SpecialFolder.ProgramFiles.ToString()));
				Folders.Add(Path.Combine(RuntimeFolder, SpecialFolder.ProgramFilesX86.ToString()));
			}

			string[] Folders2 = Folders.ToArray();

			JarPath = FileSystem.FindLatestFile(Folders2, "plantuml.jar", 1);
			JavaPath = FileSystem.FindLatestFile(Folders2, "java.exe", 3);
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			if (!string.IsNullOrEmpty(jarPath) && !string.IsNullOrEmpty(javaPath))
			{
				int i = Language.IndexOf(':');
				if (i > 0)
					Language = Language.Substring(0, i).TrimEnd();

				switch (Language.ToLower())
				{
					case "uml": return Grade.Excellent;
					case "plantuml": return Grade.Perfect;
				}
			}

			return Grade.NotAtAll;
		}

		/// <summary>
		/// Is called on the object when an instance of the element has been created in a document.
		/// </summary>
		/// <param name="Document">Document containing the instance.</param>
		public void Register(MarkdownDocument Document)
		{
			// Do nothing.
		}

		/// <summary>
		/// Generates HTML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderHtml(HtmlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			StringBuilder Output = Renderer.Output;
			bool GenerateIfNotExists = asyncHtmlOutput is null;
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Svg, GenerateIfNotExists);
			if (GenerateIfNotExists || File.Exists(Info.ImageFileName))
			{
				this.GenerateHTML(Output, Info);
				return true;
			}

			Info.AsyncId = await asyncHtmlOutput.GenerateStub(MarkdownOutputType.Html, Output, Info.Title);

			foreach (KeyValuePair<AsyncMarkdownProcessing, object> P in Document.AsyncTasks)
			{
				if (P.Value is AsyncState AsyncState && AsyncState.Type == ResultType.Svg && AsyncState.GraphInfos.Count < 10)
				{
					AsyncState.GraphInfos.Add(Info);
					return true;
				}
			}

			Document.QueueAsyncTask(this.ExecutePlantUml, new AsyncState(ResultType.Svg, Info));

			return true;
		}

		private class AsyncState
		{
			public readonly List<GraphInfo> GraphInfos;
			public readonly ResultType Type;

			public AsyncState(ResultType Type, GraphInfo Info)
			{
				this.Type = Type;
				this.GraphInfos = new List<GraphInfo>()
				{
					Info
				};
			}
		}

		private async Task ExecutePlantUml(object State)
		{
			using (HtmlRenderer Renderer = new HtmlRenderer(new HtmlSettings()
			{
				XmlEntitiesOnly = true
			}))
			{
				AsyncState AsyncState = (AsyncState)State;

				try
				{
					await this.ExecutePlantUml(AsyncState.Type, AsyncState.GraphInfos.ToArray());

					foreach (GraphInfo Info in AsyncState.GraphInfos)
					{
						Renderer.Clear();

						this.GenerateHTML(Renderer.Output, Info);
						await asyncHtmlOutput.ReportResult(MarkdownOutputType.Html, Info.AsyncId, Renderer.ToString());
						Info.Sent = true;
					}
				}
				catch (Exception ex)
				{
					Renderer.Clear();
					await Renderer.RenderObject(ex, true, new Variables());

					string s = Renderer.ToString();

					foreach (GraphInfo Info in AsyncState.GraphInfos)
					{
						if (!Info.Sent)
							await asyncHtmlOutput.ReportResult(MarkdownOutputType.Html, Info.AsyncId, s);
					}
				}
			}
		}

		private async Task ExecutePlantUml(ResultType Type, params GraphInfo[] Files)
		{
			StringBuilder Arguments = new StringBuilder();
			Arguments.Append("-jar \"");
			Arguments.Append(jarPath);
			Arguments.Append("\" -quiet -charset UTF-8 -t");
			Arguments.Append(Type.ToString().ToLower());

			foreach (GraphInfo Info in Files)
			{
				Arguments.Append(" \"");
				Arguments.Append(Info.TxtFileName);
				Arguments.Append('"');
			}

			ProcessStartInfo ProcessInformation = new ProcessStartInfo()
			{
				FileName = javaPath,
				Arguments = Arguments.ToString(),
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = false,
				WorkingDirectory = Files[0].PlantUmlFolder,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			Process P = new Process();
			TaskCompletionSource<int> ExitSource = new TaskCompletionSource<int>();
			StringBuilder StandardOutput = new StringBuilder();
			StringBuilder ErrorOutput = new StringBuilder();

			P.ErrorDataReceived += (sender, e) =>
			{
				ErrorOutput.AppendLine(e.Data);
			};

			P.OutputDataReceived += (sender, e) =>
			{
				StandardOutput.AppendLine(e.Data);
			};

			P.Exited += (sender, e) =>
			{
				ExitSource.TrySetResult(P.ExitCode);
			};

			Task _ = Task.Delay(10000).ContinueWith(Prev =>
			{
				ExitSource.TrySetException(new TimeoutException("PlantUML process did not terminate properly."));
			});

			P.StartInfo = ProcessInformation;
			P.EnableRaisingEvents = true;
			P.Start();

			int ExitCode = await ExitSource.Task;

			if (ExitCode != 0)
			{
				string Error = P.StandardError.ReadToEnd();
				throw new Exception(Error);
			}
		}

		private void GenerateHTML(StringBuilder Output, GraphInfo Info)
		{
			Info.ImageFileName = Info.ImageFileName.Substring(contentRootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');
			if (!Info.ImageFileName.StartsWith("/"))
				Info.ImageFileName = "/" + Info.ImageFileName;

			Output.Append("<figure>");
			Output.Append("<img src=\"");
			Output.Append(XML.HtmlAttributeEncode(Info.ImageFileName));

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));

				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));
			}
			else
				Output.Append("\" alt=\"PlantUML graph");

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Info.Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");
		}

		private enum ResultType
		{
			Svg,
			Png
		}

		private class GraphInfo
		{
			public string BaseFileName;
			public string TxtFileName;
			public string ImageFileName;
			public string PlantUmlFolder;
			public string Title;
			public string AsyncId;
			public bool Sent;
		}

		private async Task<GraphInfo> GetGraphInfo(string Language, string[] Rows, ResultType Type)
		{
			string Graph = MarkdownDocument.AppendRows(Rows);
			string FileName = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(Graph + Language));
			string PlantUmlFolder = Path.Combine(contentRootFolder, "PlantUML");

			GraphInfo Result = new GraphInfo()
			{
				BaseFileName = Path.Combine(PlantUmlFolder, FileName),
				PlantUmlFolder = PlantUmlFolder
			};
			int i = Language.IndexOf(':');

			if (i > 0)
				Result.Title = Language.Substring(i + 1).Trim();
			else
				Result.Title = string.Empty;

			Result.TxtFileName = Result.BaseFileName + ".txt";
			if (!File.Exists(Result.TxtFileName))
				await Resources.WriteAllTextAsync(Result.TxtFileName, Graph, Encoding.UTF8);

			switch (Type)
			{
				case ResultType.Svg:
				default:
					Result.ImageFileName = Result.BaseFileName + ".svg";
					break;

				case ResultType.Png:
					Result.ImageFileName = Result.BaseFileName + ".png";
					break;
			}

			return Result;
		}

		private async Task<GraphInfo> GetGraphInfo(string Language, string[] Rows, ResultType Type, bool GenerateIfNotExists)
		{
			GraphInfo Result = await this.GetGraphInfo(Language, Rows, Type);

			if (GenerateIfNotExists && !File.Exists(Result.ImageFileName))
				await this.ExecutePlantUml(Type, Result);
			return Result;
		}

		/// <summary>
		/// Generates Markdown for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderMarkdown(MarkdownRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			return await ImageContent.GenerateMarkdownFromFile(Renderer.Output, Info.ImageFileName, Info.Title);
		}


		/// <summary>
		/// Generates plain text for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderText(TextRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Svg, true);
			if (Info is null)
				return false;

			Renderer.Output.AppendLine(Info.Title);

			return true;
		}

		/// <summary>
		/// Generates WPF XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderWpfXaml(WpfXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			XmlWriter Output = Renderer.XmlOutput;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.ImageFileName);
			Output.WriteAttributeString("Stretch", "None");

			if (!string.IsNullOrEmpty(Info.Title))
				Output.WriteAttributeString("ToolTip", Info.Title);

			Output.WriteEndElement();

			return true;
		}
		/// <summary>
		/// Generates Xamarin.Forms XAML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderXamarinFormsXaml(XamarinFormsXamlRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			XmlWriter Output = Renderer.XmlOutput;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.ImageFileName);
			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates LaTeX for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderLatex(LatexRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
			StringBuilder Output = Renderer.Output;

			Output.AppendLine("\\begin{figure}[h]");
			Output.AppendLine("\\centering");

			Output.Append("\\fbox{\\includegraphics{");
			Output.Append(Info.ImageFileName.Replace('\\', '/'));
			Output.AppendLine("}}");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\\caption{");
				Output.Append(LatexRenderer.EscapeLaTeX(Info.Title));
				Output.AppendLine("}");
			}

			Output.AppendLine("\\end{figure}");
			Output.AppendLine();

			return true;
		}

		/// <summary>
		/// Generates an image of the contents.
		/// </summary>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>Image, if successful, null otherwise.</returns>
		public async Task<PixelInformation> GenerateImage(string[] Rows, string Language, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return null;

			byte[] Data = await Resources.ReadAllBytesAsync(Info.ImageFileName);

			using (SKBitmap Bitmap = SKBitmap.Decode(Data))
			{
				return new PixelInformationPng(Data, Bitmap.Width, Bitmap.Height);
			}
		}

		/// <summary>
		/// Default Background color
		/// </summary>
		public static string DefaultBgColor
		{
			get => defaultBgColor;
			set => defaultBgColor = value;
		}

		/// <summary>
		/// Default Foreground color
		/// </summary>
		public static string DefaultFgColor
		{
			get => defaultFgColor;
			set => defaultFgColor = value;
		}

		/// <summary>
		/// Generates smart contract XML for the code content.
		/// </summary>
		/// <param name="Renderer">Renderer.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language.</param>
		/// <param name="Indent">Code block indentation.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If renderer was able to generate output.</returns>
		public async Task<bool> RenderContractXml(ContractsRenderer Renderer, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			try
			{
				GraphInfo Info = await this.GetGraphInfo(Language, Rows, ResultType.Png, true);
				if (Info is null)
					return false;

				byte[] Data = await Resources.ReadAllBytesAsync(Info.ImageFileName);
				string ContentType = "image/png";

				if (!(await InternetContent.DecodeAsync(ContentType, Data, null) is SKImage Image))
					return false;

				XmlWriter Output = Renderer.XmlOutput;
				int Width = Image.Width;
				int Height = Image.Height;

				Output.WriteStartElement("imageStandalone");

				Output.WriteAttributeString("contentType", ContentType);
				Output.WriteAttributeString("width", Width.ToString());
				Output.WriteAttributeString("height", Height.ToString());

				Output.WriteStartElement("binary");
				Output.WriteValue(Convert.ToBase64String(Data));
				Output.WriteEndElement();

				Output.WriteStartElement("caption");
				if (string.IsNullOrEmpty(Info.Title))
					Output.WriteElementString("text", "Graph");
				else
					Output.WriteElementString("text", Info.Title);

				Output.WriteEndElement();
				Output.WriteEndElement();

				return true;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return false;
			}
		}
	}
}
