using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.CodeContent;
using Waher.Content.Markdown.Model.SpanElements;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Security;

namespace Waher.Content.Markdown.GraphViz
{
	/// <summary>
	/// Class managing GraphViz integration into Markdown documents.
	/// </summary>
	public class GraphViz : IImageCodeContent
	{
		private static readonly Random rnd = new Random();
		private static Scheduler scheduler = null;
		private static string installationFolder = null;
		private static string graphVizFolder = null;
		private static string contentRootFolder = null;
		private static string defaultBgColor = null;
		private static string defaultFgColor = null;
		private static bool supportsDot = false;
		private static bool supportsNeato = false;
		private static bool supportsFdp = false;
		private static bool supportsSfdp = false;
		private static bool supportsTwopi = false;
		private static bool supportsCirco = false;
		private static IMarkdownAsynchronousOutput asyncHtmlOutput = null;

		/// <summary>
		/// Class managing GraphViz integration into Markdown documents.
		/// </summary>
		public GraphViz()
		{
		}

		/// <summary>
		/// Initializes the GraphViz-Markdown integration.
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

				string Folder = SearchForInstallationFolder();

				if (string.IsNullOrEmpty(Folder))
					Log.Warning("GraphViz not found. GraphViz support will not be available in Markdown.");
				else
				{
					SetInstallationFolder(Folder);

					Log.Informational("GraphViz found. Integration with Markdown added.",
						new KeyValuePair<string, object>("Folder", installationFolder),
						new KeyValuePair<string, object>("dot", supportsDot),
						new KeyValuePair<string, object>("neato", supportsNeato),
						new KeyValuePair<string, object>("fdp", supportsFdp),
						new KeyValuePair<string, object>("sfdp", supportsSfdp),
						new KeyValuePair<string, object>("twopi", supportsTwopi),
						new KeyValuePair<string, object>("circo", supportsCirco));

					asyncHtmlOutput = Types.FindBest<IMarkdownAsynchronousOutput, MarkdownOutputType>(MarkdownOutputType.Html);
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		/// <summary>
		/// Sets the installation folder of GraphViz.
		/// </summary>
		/// <param name="Folder">Installation folder.</param>
		/// <exception cref="Exception">If trying to set the installation folder to a different folder than the one set previously.
		/// The folder can only be set once, for security reasons.</exception>
		public static void SetInstallationFolder(string Folder)
		{
			if (!string.IsNullOrEmpty(installationFolder) && Folder != installationFolder)
				throw new Exception("GraphViz installation folder has already been set.");

			installationFolder = Folder;
			supportsDot = File.Exists(Path.Combine(installationFolder, "bin", "dot.exe"));
			supportsNeato = File.Exists(Path.Combine(installationFolder, "bin", "neato.exe"));
			supportsFdp = File.Exists(Path.Combine(installationFolder, "bin", "fdp.exe"));
			supportsSfdp = File.Exists(Path.Combine(installationFolder, "bin", "sfdp.exe"));
			supportsTwopi = File.Exists(Path.Combine(installationFolder, "bin", "twopi.exe"));
			supportsCirco = File.Exists(Path.Combine(installationFolder, "bin", "circo.exe"));

			graphVizFolder = Path.Combine(contentRootFolder, "GraphViz");

			if (!Directory.Exists(graphVizFolder))
				Directory.CreateDirectory(graphVizFolder);

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
			DateTime Limit = DateTime.Now - MaxAge;
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(graphVizFolder, "*.*"))
			{
				if (File.GetLastAccessTime(FileName) < Limit)
				{
					try
					{
						File.Delete(FileName);
						Count++;
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileName);
					}
				}
			}

			if (Count > 0)
				Log.Informational(Count.ToString() + " old file(s) deleted.", graphVizFolder);

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
		/// <returns>Installation folder, if found, null otherwise.</returns>
		public static string SearchForInstallationFolder()
		{
			string InstallationFolder;

			InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.ProgramFilesX86);
			if (string.IsNullOrEmpty(InstallationFolder))
			{
				InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.ProgramFiles);
				if (string.IsNullOrEmpty(InstallationFolder))
				{
					InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.Programs);
					if (string.IsNullOrEmpty(InstallationFolder))
					{
						InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonProgramFilesX86);
						if (string.IsNullOrEmpty(InstallationFolder))
						{
							InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonProgramFiles);
							if (string.IsNullOrEmpty(InstallationFolder))
								InstallationFolder = SearchForInstallationFolder(Environment.SpecialFolder.CommonPrograms);
						}
					}
				}
			}

			return InstallationFolder;
		}

		private static string SearchForInstallationFolder(Environment.SpecialFolder SpecialFolder)
		{
			string Folder;

			try
			{
				Folder = Environment.GetFolderPath(SpecialFolder);
			}
			catch (Exception)
			{
				return null; // Folder not defined for the operating system.
			}

			string Result = SearchForInstallationFolder(Folder);

			if (string.IsNullOrEmpty(Result) && Types.TryGetModuleParameter("Runtime", out object Obj) && Obj is string RuntimeFolder)
				Result = SearchForInstallationFolder(Path.Combine(RuntimeFolder, SpecialFolder.ToString()));

			return Result;
		}

		private static string SearchForInstallationFolder(string Folder)
		{
			if (string.IsNullOrEmpty(Folder))
				return null;

			if (!Directory.Exists(Folder))
				return null;

			string FolderName;
			string BestFolder = null;
			double BestVersion = 0;
			string[] SubFolders;

			try
			{
				SubFolders = Directory.GetDirectories(Folder);
			}
			catch (UnauthorizedAccessException)
			{
				return null;
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
				return null;
			}

			foreach (string SubFolder in SubFolders)
			{
				FolderName = Path.GetFileName(SubFolder);
				if (!FolderName.StartsWith("Graphviz", StringComparison.CurrentCultureIgnoreCase))
					continue;

				if (!CommonTypes.TryParse(FolderName.Substring(8), out double Version))
					Version = 1.0;

				if (BestFolder is null || Version > BestVersion)
				{
					BestFolder = SubFolder;
					BestVersion = Version;
				}
			}

			return BestFolder;
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Language">Language.</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(string Language)
		{
			int i = Language.IndexOf(':');
			if (i > 0)
				Language = Language.Substring(0, i).TrimEnd();

			switch (Language.ToLower())
			{
				case "dot":
					if (supportsDot)
						return Grade.Excellent;
					break;

				case "neato":
					if (supportsNeato)
						return Grade.Excellent;
					break;

				case "fdp":
					if (supportsFdp)
						return Grade.Excellent;
					break;

				case "sfdp":
					if (supportsSfdp)
						return Grade.Excellent;
					break;

				case "twopi":
					if (supportsTwopi)
						return Grade.Excellent;
					break;

				case "circo":
					if (supportsCirco)
						return Grade.Excellent;
					break;
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
		/// If (transportable) Markdown is handled.
		/// </summary>
		public bool HandlesMarkdown => true;

		/// <summary>
		/// If HTML is handled.
		/// </summary>
		public bool HandlesHTML => true;

		/// <summary>
		/// If Plain Text is handled.
		/// </summary>
		public bool HandlesPlainText => true;

		/// <summary>
		/// If XAML is handled.
		/// </summary>
		public bool HandlesXAML => true;

		/// <summary>
		/// If LaTeX is handled.
		/// </summary>
		public bool HandlesLaTeX => true;

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Svg, asyncHtmlOutput is null);
			if (!(Info is null))
			{
				await this.GenerateHTML(Output, Info);
				return true;
			}

			string Title;
			int i = Language.IndexOf(':');
			if (i > 0)
				Title = Language.Substring(i + 1).Trim();
			else
				Title = null;

			string Id = await asyncHtmlOutput.GenerateStub(MarkdownOutputType.Html, Output, Title);

			Document.QueueAsyncTask(this.ExecuteGraphViz, new AsyncState()
			{
				Id = Id,
				Language = Language,
				Rows = Rows
			});

			return true;
		}

		private class AsyncState
		{
			public string Id;
			public string Language;
			public string[] Rows;
		}

		private async Task ExecuteGraphViz(object State)
		{
			AsyncState AsyncState = (AsyncState)State;
			StringBuilder Output = new StringBuilder();

			try
			{
				GraphInfo Info = await this.GetFileName(AsyncState.Language, AsyncState.Rows, ResultType.Svg, true);
				if (!(Info is null))
					await this.GenerateHTML(Output, Info);
			}
			catch (Exception ex)
			{
				await InlineScript.GenerateHTML(ex, Output, true, new Variables());
			}

			await asyncHtmlOutput.ReportResult(MarkdownOutputType.Html, AsyncState.Id, Output.ToString());
		}

		private async Task GenerateHTML(StringBuilder Output, GraphInfo Info)
		{
			Info.FileName = Info.FileName.Substring(contentRootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');
			if (!Info.FileName.StartsWith("/"))
				Info.FileName = "/" + Info.FileName;

			Output.Append("<figure>");
			Output.Append("<img src=\"");
			Output.Append(XML.HtmlAttributeEncode(Info.FileName));

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));

				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));
			}
			else
				Output.Append("\" alt=\"GraphViz graph");

			if (!string.IsNullOrEmpty(Info.MapFileName))
			{
				Output.Append("\" usemap=\"#Map");
				Output.Append(Info.Hash);
			}

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Info.Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");

			if (!string.IsNullOrEmpty(Info.MapFileName))
			{
				Output.Append("<map id=\"Map");
				Output.Append(Info.Hash);
				Output.Append("\" name=\"Map");
				Output.Append(Info.Hash);
				Output.AppendLine("\">");

				string Map = await Resources.ReadAllTextAsync(Info.MapFileName);
				string[] MapRows = Map.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);
				int i, c;

				for (i = 1, c = MapRows.Length - 1; i < c; i++)
					Output.AppendLine(MapRows[i]);

				Output.AppendLine("</map>");
			}
		}

		private enum ResultType
		{
			Svg,
			Png
		}

		private class GraphInfo
		{
			public string FileName;
			public string Title;
			public string MapFileName;
			public string Hash;
		}

		private async Task<GraphInfo> GetFileName(string Language, string[] Rows, ResultType Type, bool GenerateIfNotExists)
		{
			GraphInfo Result = new GraphInfo();
			string Graph = MarkdownDocument.AppendRows(Rows);
			int i = Language.IndexOf(':');

			if (i > 0)
			{
				Result.Title = Language.Substring(i + 1).Trim();
				Language = Language.Substring(0, i).TrimEnd();
			}
			else
				Result.Title = string.Empty;

			Result.Hash = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(Graph + Language));

			string GraphVizFolder = Path.Combine(contentRootFolder, "GraphViz");
			string FileName = Path.Combine(GraphVizFolder, Result.Hash);

			switch (Type)
			{
				case ResultType.Svg:
				default:
					Result.FileName = FileName + ".svg";
					break;

				case ResultType.Png:
					Result.FileName = FileName + ".png";
					break;
			}

			Result.MapFileName = FileName + ".map";

			if (File.Exists(Result.FileName))
			{
				if (!File.Exists(Result.MapFileName))
					Result.MapFileName = null;

				return Result;
			}

			if (!GenerateIfNotExists)
				return null;

			string TxtFileName = FileName + ".txt";
			await Resources.WriteAllTextAsync(TxtFileName, Graph, Encoding.Default);

			StringBuilder Arguments = new StringBuilder();

			Arguments.Append("-Tcmapx -o\"");
			Arguments.Append(Result.MapFileName);
			Arguments.Append("\" -T");
			Arguments.Append(Type.ToString().ToLower());

			if (!string.IsNullOrEmpty(defaultBgColor))
			{
				Arguments.Append(" -Gbgcolor=\"");
				Arguments.Append(defaultBgColor);
				Arguments.Append('"');
			}

			if (!string.IsNullOrEmpty(defaultFgColor))
			{
				Arguments.Append(" -Gcolor=\"");
				Arguments.Append(defaultFgColor);
				//Arguments.Append("\" -Nfillcolor=\"");
				//Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Nfontcolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Nlabelfontcolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Npencolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Efontcolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Elabelfontcolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\" -Epencolor=\"");
				Arguments.Append(defaultFgColor);
				Arguments.Append("\"");
			}

			Arguments.Append(" -q -o\"");
			Arguments.Append(Result.FileName);
			Arguments.Append("\" \"");
			Arguments.Append(TxtFileName + "\"");

			ProcessStartInfo ProcessInformation = new ProcessStartInfo()
			{
				FileName = Path.Combine(installationFolder, "bin", Language.ToLower() + ".exe"),
				Arguments = Arguments.ToString(),
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				RedirectStandardInput = false,
				WorkingDirectory = GraphVizFolder,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};

			Process P = new Process();
			TaskCompletionSource<GraphInfo> ResultSource = new TaskCompletionSource<GraphInfo>();

			P.ErrorDataReceived += (sender, e) =>
			{
				Log.Error("Unable to generate graph: " + e.Data);
				ResultSource.TrySetResult(null);
			};

			P.Exited += async (sender, e) =>
			{
				try
				{
					if (P.ExitCode != 0)
					{
						string ErrorText = await P.StandardError.ReadToEndAsync();
						Log.Error("Unable to generate graph. Exit code: " + P.ExitCode.ToString() + "\r\n\r\n" + ErrorText);
						ResultSource.TrySetResult(null);
					}
					else
					{
						string Map = await Resources.ReadAllTextAsync(Result.MapFileName);
						string[] MapRows = Map.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);
						if (MapRows.Length <= 2)
						{
							File.Delete(Result.MapFileName);
							Result.MapFileName = null;
						}

						ResultSource.TrySetResult(Result);
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			};

			Task _ = Task.Delay(10000).ContinueWith(Prev => ResultSource.TrySetException(new TimeoutException("GraphViz process did not terminate properly.")));

			P.StartInfo = ProcessInformation;
			P.EnableRaisingEvents = true;
			P.Start();

			return await ResultSource.Task;
		}

		/// <summary>
		/// Generates Plain Text for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Svg, true);
			if (Info is null)
				return false;

			Output.AppendLine(Info.Title);

			return true;
		}

		/// <summary>
		/// Generates (transportanle) Markdown for the markdown element.
		/// </summary>
		/// <param name="Output">Markdown will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateMarkdown(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			return await ImageContent.GenerateMarkdownFromFile(Output, Info.FileName, Info.Title);
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.FileName);
			Output.WriteAttributeString("Stretch", "None");

			if (!string.IsNullOrEmpty(Info.Title))
				Output.WriteAttributeString("ToolTip", Info.Title);

			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Info.FileName);
			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public async Task<bool> GenerateLaTeX(StringBuilder Output, string[] Rows, string Language, int Indent,
			MarkdownDocument Document)
		{
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Png, true);

			Output.AppendLine("\\begin{figure}[h]");
			Output.AppendLine("\\centering");

			Output.Append("\\fbox{\\includegraphics{");
			Output.Append(Info.FileName.Replace('\\', '/'));
			Output.AppendLine("}}");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\\caption{");
				Output.Append(InlineText.EscapeLaTeX(Info.Title));
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
			GraphInfo Info = await this.GetFileName(Language, Rows, ResultType.Png, true);
			if (Info is null)
				return null;

			byte[] Data = await Resources.ReadAllBytesAsync(Info.FileName);

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
	}
}
