using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Security;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Class managing GraphViz integration into Markdown documents.
	/// </summary>
	public class GraphViz : ICodeContent
	{
		private static string installationFolder = null;
		private static string graphVizFolder = null;
		private static bool supportsDot = false;
		private static bool supportsNeato = false;
		private static bool supportsFdp = false;
		private static bool supportsSfdp = false;
		private static bool supportsTwopi = false;
		private static bool supportsCirco = false;

		/// <summary>
		/// Class managing GraphViz integration into Markdown documents.
		/// </summary>
		public GraphViz()
		{
		}

		/// <summary>
		/// Initializes the GraphViz-Markdown integration.
		/// </summary>
		public static void Init()
		{
			try
			{
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

			graphVizFolder = Path.Combine(Gateway.RootFolder, "GraphViz");

			if (!Directory.Exists(graphVizFolder))
				Directory.CreateDirectory(graphVizFolder);

			DeleteOldFiles(null);
		}

		private static void DeleteOldFiles(object P)
		{
			DateTime Old = DateTime.Now.AddDays(-7);
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(graphVizFolder, "*.*"))
			{
				if (File.GetLastAccessTime(FileName) < Old)
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

			Gateway.ScheduleEvent(DeleteOldFiles, DateTime.Now.AddDays(Gateway.NextDouble() * 2), null);
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

			if (String.IsNullOrEmpty(Folder))
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
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public bool GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string FileName = this.GetFileName(Language, Rows, out string Title, out string MapFileName, out string Hash);
			if (FileName is null)
				return false;

			FileName = FileName.Substring(Gateway.RootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');
			if (!FileName.StartsWith("/"))
				FileName = "/" + FileName;

			Output.Append("<figure>");
			Output.Append("<img src=\"");
			Output.Append(XML.HtmlAttributeEncode(FileName));

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Title));

				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Title));
			}

			if (!string.IsNullOrEmpty(MapFileName))
			{
				Output.Append("\" usemap=\"#Map");
				Output.Append(Hash);
			}

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");

			if (!string.IsNullOrEmpty(MapFileName))
			{
				Output.Append("<map id=\"Map");
				Output.Append(Hash);
				Output.Append("\" name=\"Map");
				Output.Append(Hash);
				Output.AppendLine("\">");

				string Map = File.ReadAllText(MapFileName);
				string[] MapRows = Map.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);
				int i, c;

				for (i = 1, c = MapRows.Length - 1; i < c; i++)
					Output.AppendLine(MapRows[i]);

				Output.AppendLine("</map>");
			}

			return true;
		}

		private string GetFileName(string Language, string[] Rows, out string Title, out string MapFileName, out string Hash)
		{
			StringBuilder sb = new StringBuilder();

			foreach (string Row in Rows)
				sb.AppendLine(Row);

			string Graph = sb.ToString();
			int i = Language.IndexOf(':');

			if (i > 0)
			{
				Title = Language.Substring(i + 1).Trim();
				Language = Language.Substring(0, i).TrimEnd();
			}
			else
				Title = string.Empty;

			sb.Append(Language);

			Hash = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(sb.ToString()));

			string GraphVizFolder = Path.Combine(Gateway.RootFolder, "GraphViz");
			string FileName = Path.Combine(GraphVizFolder, Hash);
			string SvgFileName = FileName + ".svg";

			MapFileName = FileName + ".map";

			if (File.Exists(SvgFileName))
			{
				if (!File.Exists(MapFileName))
					MapFileName = null;
			}
			else
			{
				string TxtFileName = FileName + ".txt";
				File.WriteAllText(TxtFileName, Graph, Encoding.Default);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = Path.Combine(installationFolder, "bin", Language.ToLower() + ".exe"),
					Arguments = "-Tcmapx -o\"" + MapFileName + "\" -Tsvg -q -o\"" + SvgFileName + "\" \"" + TxtFileName + "\"",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = GraphVizFolder,
					CreateNoWindow = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};

				Process P = new Process();
				bool Error = false;

				P.ErrorDataReceived += (sender, e) =>
				{
					Error = true;
					Log.Error(e.Data);
				};

				P.StartInfo = ProcessInformation;
				P.Start();

				if (!P.WaitForExit(60000) || Error)
				{
					Log.Error("Unable to generate graph.");
					return null;
				}
				else if (P.ExitCode != 0)
				{
					Log.Error("Unable to generate graph. Exit code: " + P.ExitCode.ToString());
					return null;
				}

				string Map = File.ReadAllText(MapFileName);
				string[] MapRows = Map.Split(CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries);
				if (MapRows.Length <= 2)
				{
					File.Delete(MapFileName);
					MapFileName = null;
				}
			}

			return SvgFileName;
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
		public bool GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			this.GetFileName(Language, Rows, out string Title, out string _, out string _);
			Output.AppendLine(Title);

			return true;
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
		public bool GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string FileName = this.GetFileName(Language, Rows, out string Title, out string _, out string _);
			if (FileName is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", FileName);

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();

			return true;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public bool GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string FileName = this.GetFileName(Language, Rows, out string _, out string _, out string _);
			if (FileName is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", FileName);
			Output.WriteEndElement();

			return true;
		}
	}
}
