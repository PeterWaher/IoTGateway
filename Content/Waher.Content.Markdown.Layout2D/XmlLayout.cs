using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content.Emoji;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Model;
using Waher.Content.Markdown.Model.CodeContent;
using Waher.Content.Markdown.Rendering;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Markdown.Xamarin;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Layout.Layout2D;
using Waher.Runtime.Inventory;
using Waher.Runtime.Timing;
using Waher.Script;
using Waher.Script.Graphs;
using Waher.Security;

namespace Waher.Content.Markdown.Layout2D
{
	/// <summary>
	/// Class managing 2D XML Layout integration into Markdown documents.
	/// </summary>
	public class XmlLayout : IImageCodeContent, IXmlVisualizer, ICodeContentMarkdownRenderer, ICodeContentHtmlRenderer, ICodeContentTextRenderer,
		ICodeContentContractsRenderer, ICodeContentLatexRenderer, ICodeContentWpfXamlRenderer, ICodeContentXamarinFormsXamlRenderer
	{
		private static readonly Random rnd = new Random();
		private static Scheduler scheduler = null;
		private static string layoutFolder = null;
		private static string contentRootFolder = null;

		/// <summary>
		/// Class managing 2D XML Layout integration into Markdown documents.
		/// </summary>
		public XmlLayout()
		{
		}

		/// <summary>
		/// Initializes the Layout2D-Markdown integration.
		/// </summary>
		/// <param name="ContentRootFolder">Content root folder. If hosting markdown under a web server, this would correspond
		/// to the roof folder for the web content.</param>
		public static void Init(string ContentRootFolder)
		{
			contentRootFolder = ContentRootFolder;
			layoutFolder = Path.Combine(contentRootFolder, "Layout");

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

			if (!Directory.Exists(layoutFolder))
				Directory.CreateDirectory(layoutFolder);

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
			if (string.IsNullOrEmpty(layoutFolder))
				return;

			DateTime Limit = DateTime.Now - MaxAge;
			int Count = 0;

			DirectoryInfo LayoutFolder = new DirectoryInfo(layoutFolder);
			FileInfo[] Files = LayoutFolder.GetFiles("*.*");

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
				Log.Informational(Count.ToString() + " old file(s) deleted.", layoutFolder);

			if (Reschedule)
			{
				lock (rnd)
				{
					scheduler.Add(DateTime.Now.AddDays(rnd.NextDouble() * 2), DeleteOldFiles, MaxAge);
				}
			}
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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			string FileName = Info.FileName.Substring(contentRootFolder.Length).Replace(Path.DirectorySeparatorChar, '/');
			if (!FileName.StartsWith("/"))
				FileName = "/" + FileName;

			StringBuilder Output = Renderer.Output;

			Output.Append("<figure>");
			Output.Append("<img src=\"");
			if (Info.Dynamic)
				Output.Append(ImageContent.GenerateUrl(Info.DynamicContent, "image/png"));
			else
				Output.Append(XML.HtmlAttributeEncode(FileName));

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("\" alt=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));

				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Info.Title));
			}
			else
				Output.Append("\" alt=\"2D-layout");

			Output.Append("\" class=\"aloneUnsized\"/>");

			if (!string.IsNullOrEmpty(Info.Title))
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(Info.Title));
				Output.Append("</figcaption>");
			}

			Output.AppendLine("</figure>");

			return true;
		}

		private class GraphInfo
		{
			public string FileName;
			public string Title;
			public bool Dynamic;
			public byte[] DynamicContent;
		}

		/// <summary>
		/// Generates an image, saves it, and returns the file name of the image file.
		/// </summary>
		/// <param name="Language">Language</param>
		/// <param name="Rows">Code Block rows</param>
		/// <param name="Session">Session variables.</param>
		/// <returns>File name</returns>
		private static async Task<GraphInfo> GetFileName(string Language, string[] Rows, Variables Session)
		{
			GraphInfo Result = new GraphInfo();
			string Xml = MarkdownDocument.AppendRows(Rows);
			int i = Language.IndexOf(':');

			if (i > 0)
			{
				Result.Title = Language.Substring(i + 1).Trim();
				Language = Language.Substring(0, i).TrimEnd();
			}
			else
				Result.Title = string.Empty;

			string Hash = Hashes.ComputeSHA256HashString(Encoding.UTF8.GetBytes(Xml + Language));

			string LayoutFolder = Path.Combine(contentRootFolder, "Layout");
			string FileName = Path.Combine(LayoutFolder, Hash);
			Result.FileName = FileName + ".png";

			if (!File.Exists(Result.FileName))
			{
				try
				{
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(Xml);

					Layout2DDocument LayoutDoc = await Layout2DDocument.FromXml(Doc, Session);
					RenderSettings Settings = await LayoutDoc.GetRenderSettings(Session);

					KeyValuePair<SKImage, Map[]> P = await LayoutDoc.Render(Settings);
					using (SKImage Img = P.Key)   // TODO: Maps
					{
						using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
						{
							Result.DynamicContent = Data.ToArray();
							Result.Dynamic = LayoutDoc.Dynamic;

							if (!LayoutDoc.Dynamic)
								await Resources.WriteAllBytesAsync(Result.FileName, Result.DynamicContent);
						}
					}
				}
				catch (Exception)
				{
					return null;
				}
			}

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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			if (Info.Dynamic)
			{
				ImageContent.GenerateMarkdown(Renderer.Output, Info.DynamicContent, "image/png", Info.Title);
				return true;
			}
			else
				return await ImageContent.GenerateMarkdownFromFile(Renderer.Output, Info.FileName, Info.Title);
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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			XmlWriter Output = Renderer.XmlOutput;

			if (Info.Dynamic)
			{
				await Wpf.Multimedia.ImageContent.OutputWpf(Output, new ImageSource()
				{
					Url = ImageContent.GenerateUrl(Info.DynamicContent, "image/png")
				}, Info.Title);
			}
			else
			{
				Output.WriteStartElement("Image");
				Output.WriteAttributeString("Source", Info.FileName);
				Output.WriteAttributeString("Stretch", "None");

				if (!string.IsNullOrEmpty(Info.Title))
					Output.WriteAttributeString("ToolTip", Info.Title);

				Output.WriteEndElement();
			}

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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			XmlWriter Output = Renderer.XmlOutput;

			if (Info.Dynamic)
			{
				await Xamarin.Multimedia.ImageContent.OutputXamarinForms(Output, new ImageSource()
				{
					Url = ImageContent.GenerateUrl(Info.DynamicContent, "image/png")
				});
			}
			else
			{
				Output.WriteStartElement("Image");
				Output.WriteAttributeString("Source", Info.FileName);
				Output.WriteEndElement();
			}

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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return false;

			if (Info.Dynamic)
				Info.FileName = await Model.Multimedia.ImageContent.GetTemporaryFile(Info.DynamicContent, "png");

			StringBuilder Output = Renderer.Output;

			Output.AppendLine("\\begin{figure}[h]");
			Output.AppendLine("\\centering");

			Output.Append("\\fbox{\\includegraphics{");
			Output.Append(Info.FileName.Replace('\\', '/'));
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
			GraphInfo Info = await GetFileName(Language, Rows, Document.Settings.Variables);
			if (Info?.FileName is null)
				return null;

			byte[] Data = await Resources.ReadAllBytesAsync(Info.FileName);

			using (SKBitmap Bitmap = SKBitmap.Decode(Data))
			{
				return new PixelInformationPng(Data, Bitmap.Width, Bitmap.Height);
			}
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Xml">XML Document</param>
		/// <returns>How well the handler supports the content.</returns>
		public Grade Supports(XmlDocument Xml)
		{
			return Xml.DocumentElement.LocalName == Layout2DDocument.LocalName &&
				Xml.DocumentElement.NamespaceURI == Layout2DDocument.Namespace ? Grade.Excellent : Grade.NotAtAll;
		}

		/// <summary>
		/// Transforms the XML document before visualizing it.
		/// </summary>
		/// <param name="Xml">XML Document.</param>
		/// <param name="Session">Current variables.</param>
		/// <returns>Transformed object.</returns>
		public async Task<object> TransformXml(XmlDocument Xml, Variables Session)
		{
			Layout2DDocument LayoutDoc = await Layout2DDocument.FromXml(Xml, Session);
			RenderSettings Settings = await LayoutDoc.GetRenderSettings(Session);

			KeyValuePair<SKImage, Map[]> P = await LayoutDoc.Render(Settings);
			using (SKImage Img = P.Key)   // TODO: Maps
			{
				return PixelInformation.FromImage(Img);
			}
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
				string Xml = MarkdownDocument.AppendRows(Rows);
				string Title;
				int i = Language.IndexOf(':');

				if (i > 0)
				{
					Title = Language.Substring(i + 1).Trim();
					Language = Language.Substring(0, i).TrimEnd();
				}
				else
					Title = string.Empty;

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml);

				Variables Variables = Document.Settings.Variables;
				Layout2DDocument LayoutDoc = await Layout2DDocument.FromXml(Doc, Variables);
				RenderSettings Settings = await LayoutDoc.GetRenderSettings(Variables);
				XmlWriter Output = Renderer.XmlOutput;

				KeyValuePair<SKImage, Map[]> P = await LayoutDoc.Render(Settings);

				using (SKImage Img = P.Key)
				{
					Output.WriteStartElement("imageStandalone");

					Output.WriteAttributeString("contentType", "image/png");
					Output.WriteAttributeString("width", Img.Width.ToString());
					Output.WriteAttributeString("height", Img.Height.ToString());

					using (SKData Data = Img.Encode(SKEncodedImageFormat.Png, 100))
					{
						byte[] Bin = Data.ToArray();

						Output.WriteStartElement("binary");
						Output.WriteValue(Convert.ToBase64String(Bin));
						Output.WriteEndElement();
					}

					Output.WriteStartElement("caption");
					if (string.IsNullOrEmpty(Title))
						Output.WriteElementString("text", "Layout");
					else
						Output.WriteElementString("text", Title);

					Output.WriteEndElement();
					Output.WriteEndElement();
				}

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
