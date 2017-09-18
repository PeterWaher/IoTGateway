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
			else
				DeleteOldFiles(null);

			Gateway.ScheduleEvent(DeleteOldFiles, DateTime.Now.AddDays(Gateway.NextDouble() * 2), null);
		}

		private static void DeleteOldFiles(object P)
		{
			DateTime Old = DateTime.Now.AddDays(-7);

			foreach (string FileName in Directory.GetFiles(graphVizFolder, "*.*"))
			{
				if (File.GetLastAccessTime(FileName) < Old)
				{
					try
					{
						File.Delete(FileName);
					}
					catch (Exception ex)
					{
						Log.Error("Unable to delete old file: " + ex.Message, FileName);
					}
				}
			}

			Gateway.ScheduleEvent(DeleteOldFiles, DateTime.Now.AddDays(Gateway.NextDouble() * 2), null);
		}

		/// <summary>
		/// Searches for the installation folder on the local machine.
		/// </summary>
		/// <returns>Installation folder, if found, null otherwise.</returns>
		public static string SearchForInstallationFolder()
		{
			string InstallationFolder;

			InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));
			if (string.IsNullOrEmpty(InstallationFolder))
			{
				InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
				if (string.IsNullOrEmpty(InstallationFolder))
				{
					InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.Programs));
					if (string.IsNullOrEmpty(InstallationFolder))
					{
						InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86));
						if (string.IsNullOrEmpty(InstallationFolder))
						{
							InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles));
							if (string.IsNullOrEmpty(InstallationFolder))
								InstallationFolder = SearchForInstallationFolder(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms));
						}
					}
				}
			}

			return InstallationFolder;
		}

		private static string SearchForInstallationFolder(string Folder)
		{
			if (String.IsNullOrEmpty(Folder))
				return null;

			if (!Directory.Exists(Folder))
				return null;

			string FolderName;
			string BestFolder = null;
			double BestVersion = 0;

			foreach (string SubFolder in Directory.GetDirectories(Folder))
			{
				FolderName = Path.GetFileName(SubFolder);
				if (!FolderName.StartsWith("Graphviz", StringComparison.CurrentCultureIgnoreCase))
					continue;

				if (!CommonTypes.TryParse(FolderName.Substring(8), out double Version))
					Version = 1.0;

				if (BestFolder == null || Version > BestVersion)
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
		public void GenerateHTML(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string FileName = this.GetFileName(Language, Rows, out string Title);
			FileName = FileName.Substring(Gateway.RootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');

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

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");
		}

		private string GetFileName(string Language, string[] Rows, out string Title)
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

			string FileName = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(sb.ToString()));
			string GraphVizFolder = Path.Combine(Gateway.RootFolder, "GraphViz");

			FileName = Path.Combine(GraphVizFolder, FileName);

			string SvgFileName = FileName + ".svg";
			if (!File.Exists(SvgFileName))
			{
				string TxtFileName = FileName + ".txt";
				File.WriteAllText(TxtFileName, Graph, Encoding.Default);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = Path.Combine(installationFolder, "bin", Language.ToLower() + ".exe"),
					Arguments = "-Tsvg -q -o\"" + SvgFileName + "\" \"" + TxtFileName + "\"",
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
		public void GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			this.GetFileName(Language, Rows, out string Title);
			Output.AppendLine(Title);
		}

		/// <summary>
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Rows">Code rows.</param>
		/// <param name="Language">Language used.</param>
		/// <param name="Indent">Additional indenting.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			string FileName = this.GetFileName(Language, Rows, out string Title);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", FileName);

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();
		}
	}
}
