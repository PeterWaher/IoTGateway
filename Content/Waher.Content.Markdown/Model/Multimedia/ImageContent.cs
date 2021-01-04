using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;

namespace Waher.Content.Markdown.Model.Multimedia
{
	/// <summary>
	/// Image content.
	/// </summary>
	public class ImageContent : MultimediaContent
	{
		/// <summary>
		/// Image content.
		/// </summary>
		public ImageContent()
		{
		}

		/// <summary>
		/// Checks how well the handler supports multimedia content of a given type.
		/// </summary>
		/// <param name="Item">Multimedia item.</param>
		/// <returns>How well the handler supports the content.</returns>
		public override Grade Supports(MultimediaItem Item)
		{
			if (Item.ContentType.StartsWith("image/"))
				return Grade.Ok;
			else
				return Grade.Barely;
		}

		/// <summary>
		/// If the link provided should be embedded in a multi-media construct automatically.
		/// </summary>
		/// <param name="Url">Inline link.</param>
		public override bool EmbedInlineLink(string Url)
		{
			string Extension = Path.GetExtension(Url);
			string ContentType = InternetContent.GetContentType(Extension);

			return ContentType.StartsWith("image/");
		}

		/// <summary>
		/// Generates HTML for the markdown element.
		/// </summary>
		/// <param name="Output">HTML will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Alt = new StringBuilder();
			bool SizeSet;

			foreach (MarkdownElement E in ChildNodes)
				E.GeneratePlainText(Alt);

			string AltStr = Alt.ToString();
			bool SameWidth = true;
			bool SameHeight = true;

			if (string.IsNullOrEmpty(AltStr))
				AloneInParagraph = false;

			if (AloneInParagraph)
				Output.Append("<figure>");

			if (Items.Length > 1)
			{
				Output.AppendLine("<picture>");

				foreach (MultimediaItem Item in Items)
				{
					if (Item.Width != Items[0].Width)
						SameWidth = false;

					if (Item.Height != Items[0].Height)
						SameHeight = false;

					Output.Append("<source srcset=\"");
					Output.Append(XML.HtmlAttributeEncode(Item.Url));
					Output.Append("\" type=\"");
					Output.Append(XML.HtmlAttributeEncode(Item.ContentType));

					if (Item.Width.HasValue)
					{
						Output.Append("\" media=\"(min-width:");
						Output.Append(Item.Width.Value.ToString());
						Output.Append("px)");
					}

					Output.AppendLine("\"/>");
				}
			}

			Output.Append("<img src=\"");
			Output.Append(XML.HtmlAttributeEncode(Document.CheckURL(Items[0].Url, null)));

			Output.Append("\" alt=\"");
			Output.Append(XML.HtmlAttributeEncode(AltStr));

			if (!string.IsNullOrEmpty(Items[0].Title))
			{
				Output.Append("\" title=\"");
				Output.Append(XML.HtmlAttributeEncode(Items[0].Title));
			}

			SizeSet = false;

			if (SameWidth && Items[0].Width.HasValue)
			{
				Output.Append("\" width=\"");
				Output.Append(Items[0].Width.Value.ToString());
				SizeSet = true;
			}

			if (SameHeight && Items[0].Height.HasValue)
			{
				Output.Append("\" height=\"");
				Output.Append(Items[0].Height.Value.ToString());
				SizeSet = true;
			}

			if (AloneInParagraph && !SizeSet && Items.Length == 1)
				Output.Append("\" class=\"aloneUnsized");

			if (Items.Length > 1)
			{
				Output.Append("\" srcset=\"");

				bool First = true;

				foreach (MultimediaItem Item in Items)
				{
					if (First)
						First = false;
					else
						Output.Append(", ");

					Output.Append(XML.HtmlAttributeEncode(Item.Url));

					if (Item.Width.HasValue)
					{
						Output.Append(" ");
						Output.Append(Item.Width.Value.ToString());
						Output.Append("w");
					}
				}

				Output.Append("\" sizes=\"100vw");
			}

			Output.Append("\"/>");

			if (Items.Length > 1)
				Output.AppendLine("</picture>");

			if (AloneInParagraph)
			{
				Output.Append("<figcaption>");
				Output.Append(XML.HtmlValueEncode(AltStr));
				Output.AppendLine("</figcaption></figure>");
			}
		}

		/// <summary>
		/// Generates WPF XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				OutputWpf(Output, Document.CheckURL(Item.Url, null), 
					Item.Width, Item.Height, Item.Title);

				break;
			}
		}

		internal static void OutputWpf(XmlWriter Output, string Source, int? Width, int? Height, string Title)
		{
			Source = CheckDataUri(Source, ref Width, ref Height);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source);

			if (Width.HasValue)
				Output.WriteAttributeString("Width", Width.Value.ToString());

			if (Height.HasValue)
				Output.WriteAttributeString("Height", Height.Value.ToString());

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();
		}

		internal static string CheckDataUri(string Source, ref int? Width, ref int? Height)
		{
			int i;
			if (Source.StartsWith("data:", StringComparison.CurrentCultureIgnoreCase) && 
				(i = Source.IndexOf("base64,")) > 0)
			{
				byte[] Data = Convert.FromBase64String(Source.Substring(i + 7));
				using (SKBitmap Bitmap = SKBitmap.Decode(Data))
				{
					Width = Bitmap.Width;
					Height = Bitmap.Height;
				}

				Source = GetTemporaryFile(Data);
			}

			return Source;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateXamarinForms(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				OutputXamarinForms(Output, Document.CheckURL(Item.Url, null), 
					Item.Width, Item.Height);

				break;
			}
		}

		internal static void OutputXamarinForms(XmlWriter Output, string Source, int? Width, int? Height)
		{
			Source = CheckDataUri(Source, ref Width, ref Height);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source);

			if (Width.HasValue)
				Output.WriteAttributeString("WidthRequest", Width.Value.ToString());

			if (Height.HasValue)
				Output.WriteAttributeString("HeightRequest", Height.Value.ToString());

			// TODO: Tooltip

			Output.WriteEndElement();
		}

		/// <summary>
		/// Stores an image in binary form as a temporary file. Files will be deleted when application closes.
		/// </summary>
		/// <param name="BinaryImage">Binary image.</param>
		/// <returns>Temporary file name.</returns>
		public static string GetTemporaryFile(byte[] BinaryImage)
		{
			string FileName;

			using (SHA256 H = SHA256.Create())
			{
				byte[] Digest = H.ComputeHash(BinaryImage);
				FileName = Path.Combine(Path.GetTempPath(), "tmp" + Base64Url.Encode(Digest) + ".tmp");
			}

			if (!File.Exists(FileName))
			{
				File.WriteAllBytes(FileName, BinaryImage);

				lock (synchObject)
				{
					if (temporaryFiles is null)
					{
						temporaryFiles = new Dictionary<string, bool>();
						Log.Terminating += CurrentDomain_ProcessExit;
					}

					temporaryFiles[FileName] = true;
				}
			}

			return FileName;
		}

		private static Dictionary<string, bool> temporaryFiles = null;
		private readonly static object synchObject = new object();

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			lock (synchObject)
			{
				if (!(temporaryFiles is null))
				{
					foreach (string FileName in temporaryFiles.Keys)
					{
						try
						{
							File.Delete(FileName);
						}
						catch (Exception)
						{
							// Ignore
						}
					}

					temporaryFiles.Clear();
				}
			}
		}
	}
}
