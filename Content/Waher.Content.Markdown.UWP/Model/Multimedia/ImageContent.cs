using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using SkiaSharp;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Cache;
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
		/// Generates XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="Settings">XAML settings.</param>
		/// <param name="TextAlignment">Alignment of text in element.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override void GenerateXAML(XmlWriter Output, XamlSettings Settings, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			string Source;
			int i;
			int? Width;
			int? Height;

			foreach (MultimediaItem Item in Items)
			{
				Width = Item.Width;
				Height = Item.Height;

				if ((Source = Item.Url).StartsWith("data:", StringComparison.CurrentCultureIgnoreCase) && (i = Item.Url.IndexOf("base64,")) > 0)
				{
					byte[] Data = System.Convert.FromBase64String(Item.Url.Substring(i + 7));
					MemoryStream ms = new MemoryStream(Data);

					using (SKManagedStream Stream = new SKManagedStream(ms))
					{
						using (SKCodec Codec = SKCodec.Create(Stream))
						{
							SKImageInfo ImgInfo = Codec.Info;
							SKBitmap Bitmap = new SKBitmap(ImgInfo.Width, ImgInfo.Height, ImgInfo.ColorType,
								ImgInfo.IsOpaque ? SKAlphaType.Opaque : SKAlphaType.Premul);

							Width = Bitmap.Width;
							Height = Bitmap.Height;
						}
					}
				}

				Output.WriteStartElement("Image");
				Output.WriteAttributeString("Source", Document.CheckURL(Source, null));

				if (Width.HasValue)
					Output.WriteAttributeString("Width", Width.Value.ToString());

				if (Height.HasValue)
					Output.WriteAttributeString("Height", Height.Value.ToString());

				if (!string.IsNullOrEmpty(Item.Title))
					Output.WriteAttributeString("ToolTip", Item.Title);

				Output.WriteEndElement();

				break;
			}
		}

		private void TemporaryFiles_Removed(object Sender, CacheItemEventArgs<string, bool> e)
		{
			try
			{
				File.Delete(e.Key);
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static Cache<string, bool> temporaryFiles = null;
		private static object synchObject = new object();

		static ImageContent()
		{
			Log.Terminating += CurrentDomain_ProcessExit;
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			lock(synchObject)
			{
				if (temporaryFiles != null)
				{
					temporaryFiles.Dispose();
					temporaryFiles = null;
				}
			}
		}
	}
}
