using System;
using System.IO;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Layout.Layout2D;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security;

namespace Waher.IoTGateway.CodeContent
{
	/// <summary>
	/// Class managing 2D XML Layout integration into Markdown documents.
	/// </summary>
	public class XmlLayout : ICodeContent
	{
		private static string layoutFolder = null;

		/// <summary>
		/// Class managing 2D XML Layout integration into Markdown documents.
		/// </summary>
		public XmlLayout()
		{
			layoutFolder = Path.Combine(Gateway.RootFolder, "Layout");

			if (!Directory.Exists(layoutFolder))
				Directory.CreateDirectory(layoutFolder);

			DeleteOldFiles(null);
		}

		private static void DeleteOldFiles(object P)
		{
			DateTime Old = DateTime.Now.AddDays(-7);
			int Count = 0;

			foreach (string FileName in Directory.GetFiles(layoutFolder, "*.*"))
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
				Log.Informational(Count.ToString() + " old file(s) deleted.", layoutFolder);

			Gateway.ScheduleEvent(DeleteOldFiles, DateTime.Now.AddDays(Gateway.NextDouble() * 2), null);
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
				case "layout":
					return Grade.Excellent;
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

			string Xml = sb.ToString();
			int i = Language.IndexOf(':');

			if (i > 0)
			{
				Title = Language.Substring(i + 1).Trim();
				Language = Language.Substring(0, i).TrimEnd();
			}
			else
				Title = string.Empty;

			sb.Append(Language);

			string Hash = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(sb.ToString()));

			string LayoutFolder = Path.Combine(Gateway.RootFolder, "Layout");
			string FileName = Path.Combine(LayoutFolder, Hash);
			string PngFileName = FileName + ".png";

			if (!File.Exists(PngFileName))
			{
				try
				{
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(Xml);

					Layout2DDocument LayoutDoc = new Layout2DDocument(Doc);
					RenderSettings Settings = new RenderSettings()
					{
						ImageSize = RenderedImageSize.ResizeImage   // TODO: Theme colors, font, etc.
					};

					using (SKImage Img = LayoutDoc.Render(Settings, out Map[] _))   // TODO: Maps
					{
						using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
						{
							using (FileStream fs = File.Create(PngFileName))
							{
								Data.SaveTo(fs);
							}
						}
					}
				}
				catch (Exception)
				{
					return null;
				}
			}

			return PngFileName;
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
