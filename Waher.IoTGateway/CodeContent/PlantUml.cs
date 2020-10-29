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
	/// Class managing PlantUML integration into Markdown documents.
	/// </summary>
	public class PlantUml : ICodeContent
	{
		private static string jarPath = null;
		private static string javaPath = null;
		private static string plantUmlFolder = null;

		/// <summary>
		/// Class managing PlantUML integration into Markdown documents.
		/// </summary>
		public PlantUml()
		{
		}

		/// <summary>
		/// Initializes the PlantUML-Markdown integration.
		/// </summary>
		public static void Init()
		{
			try
			{
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
			plantUmlFolder = Path.Combine(Gateway.RootFolder, "PlantUML");

			if (!Directory.Exists(plantUmlFolder))
				Directory.CreateDirectory(plantUmlFolder);

			DeleteOldFiles(null);
		}

		private static void DeleteOldFiles(object P)
		{
			DateTime Old = DateTime.Now.AddDays(-7);
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(plantUmlFolder, "*.*"))
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
				Log.Informational(Count.ToString() + " old file(s) deleted.", plantUmlFolder);

			Gateway.ScheduleEvent(DeleteOldFiles, DateTime.Now.AddDays(Gateway.NextDouble() * 2), null);
		}

		/// <summary>
		/// Searches for the installation folder on the local machine.
		/// </summary>
		/// <param name="JarPath">Path to PlantUML Jar file.</param>
		/// <param name="JavaPath">Path to Java VM.</param>
		public static void SearchForInstallationFolder(out string JarPath, out string JavaPath)
		{
			string[] Folders = Gateway.GetFolders(new Environment.SpecialFolder[]
				{
					Environment.SpecialFolder.ProgramFiles,
					Environment.SpecialFolder.ProgramFilesX86
				});

			JarPath = Gateway.FindLatestFile(Folders, "plantuml.jar", 1);
			JavaPath = Gateway.FindLatestFile(Folders, "java.exe", 3);
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
			string FileName = this.GetFileName(Language, Rows, out string Title);
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

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");

			return true;
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
			string PlantUmlFolder = Path.Combine(Gateway.RootFolder, "PlantUML");

			FileName = Path.Combine(PlantUmlFolder, FileName);

			string SvgFileName = FileName + ".svg";
			if (!File.Exists(SvgFileName))
			{
				string TxtFileName = FileName + ".txt";
				File.WriteAllText(TxtFileName, Graph, Encoding.UTF8);

				ProcessStartInfo ProcessInformation = new ProcessStartInfo()
				{
					FileName = javaPath,
					Arguments = "-jar \"" + jarPath + "\" -charset UTF-8 -tsvg -quiet \"" + TxtFileName + "\" \"" + SvgFileName + "\"",
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					WorkingDirectory = PlantUmlFolder,
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
		/// <returns>If content was rendered. If returning false, the default rendering of the code block will be performed.</returns>
		public bool GeneratePlainText(StringBuilder Output, string[] Rows, string Language, int Indent, MarkdownDocument Document)
		{
			this.GetFileName(Language, Rows, out string Title);
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
			string FileName = this.GetFileName(Language, Rows, out string Title);
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
			string FileName = this.GetFileName(Language, Rows, out string _);
			if (FileName is null)
				return false;

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", FileName);
			Output.WriteEndElement();

			return true;
		}
	}
}
