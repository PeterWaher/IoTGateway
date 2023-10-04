using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using Waher.Content.Emoji;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Runtime.Inventory;
using Waher.Runtime.Temporary;
using Waher.Script;
using Waher.Content.Markdown.Model.SpanElements;

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
		public override async Task GenerateHTML(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			StringBuilder Alt = new StringBuilder();
			bool SizeSet;

			foreach (MarkdownElement E in ChildNodes)
				await E.GeneratePlainText(Alt);

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
						Output.Append(' ');
						Output.Append(Item.Width.Value.ToString());
						Output.Append('w');
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
		public override Task GenerateXAML(XmlWriter Output, TextAlignment TextAlignment, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				return OutputWpf(Output, new ImageSource()
				{
					Url = Document.CheckURL(Item.Url, null),
					Width = Item.Width,
					Height = Item.Height
				}, Item.Title);
			}

			return Task.CompletedTask;
		}

		internal static async Task OutputWpf(XmlWriter Output, IImageSource Source, string Title)
		{
			Source = await CheckDataUri(Source);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source.Url);

			bool HasSize = false;

			if (Source.Width.HasValue)
			{
				Output.WriteAttributeString("Width", Source.Width.Value.ToString());
				HasSize = true;
			}

			if (Source.Height.HasValue)
			{
				Output.WriteAttributeString("Height", Source.Height.Value.ToString());
				HasSize = true;
			}

			if (!HasSize)
				Output.WriteAttributeString("Stretch", "None");

			if (!string.IsNullOrEmpty(Title))
				Output.WriteAttributeString("ToolTip", Title);

			Output.WriteEndElement();
		}

		internal static async Task<IImageSource> CheckDataUri(IImageSource Source)
		{
			string Url = Source.Url;
			int i;

			if (Url.StartsWith("data:", StringComparison.CurrentCultureIgnoreCase) && (i = Url.IndexOf("base64,")) > 0)
			{
				int? Width = Source.Width;
				int? Height = Source.Height;
				byte[] Data = Convert.FromBase64String(Url.Substring(i + 7));
				using (SKBitmap Bitmap = SKBitmap.Decode(Data))
				{
					Width = Bitmap.Width;
					Height = Bitmap.Height;
				}

				Url = await GetTemporaryFile(Data);

				return new ImageSource()
				{
					Url = Url,
					Width = Width,
					Height = Height
				};
			}
			else
				return Source;
		}

		/// <summary>
		/// Generates Xamarin.Forms XAML for the markdown element.
		/// </summary>
		/// <param name="Output">XAML will be output here.</param>
		/// <param name="State">Xamarin Forms XAML Rendering State.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override Task GenerateXamarinForms(XmlWriter Output, XamarinRenderingState State, MultimediaItem[] Items,
			IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				return OutputXamarinForms(Output, new ImageSource()
				{
					Url = Document.CheckURL(Item.Url, null),
					Width = Item.Width,
					Height = Item.Height
				});
			}

			return Task.CompletedTask;
		}

		internal static async Task OutputXamarinForms(XmlWriter Output, IImageSource Source)
		{
			Source = await CheckDataUri(Source);

			Output.WriteStartElement("Image");
			Output.WriteAttributeString("Source", Source.Url);

			if (Source.Width.HasValue)
				Output.WriteAttributeString("WidthRequest", Source.Width.Value.ToString());

			if (Source.Height.HasValue)
				Output.WriteAttributeString("HeightRequest", Source.Height.Value.ToString());

			// TODO: Tooltip

			Output.WriteEndElement();
		}

		/// <summary>
		/// Generates LaTeX text for the markdown element.
		/// </summary>
		/// <param name="Output">LaTeX will be output here.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateLaTeX(StringBuilder Output, MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes,
			bool AloneInParagraph, MarkdownDocument Document)
		{
			foreach (MultimediaItem Item in Items)
			{
				string Url = Document.CheckURL(Item.Url, Document.URL);

				if (Uri.TryCreate(Url, UriKind.RelativeOrAbsolute, out Uri ParsedUri))
				{
					KeyValuePair<string, TemporaryStream> P;

					if (ParsedUri.IsAbsoluteUri)
					{
						P = await InternetContent.GetTempStreamAsync(new Uri(Item.Url), 60000);
					}
					else
					{
						string FileName = Document.Settings.GetFileName(Document.FileName, Url);
						if (!File.Exists(FileName))
							continue;

					}

					using (TemporaryStream f = P.Value)
					{
						int c = (int)Math.Min(int.MaxValue, f.Length);
						byte[] Bin = new byte[c];

						f.Position = 0;
						await f.ReadAsync(Bin, 0, c);

						string FileName = await GetTemporaryFile(Bin);

						if (AloneInParagraph)
						{
							Output.AppendLine("\\begin{figure}[h]");
							Output.AppendLine("\\centering");
						}

						Output.Append("\\fbox{\\includegraphics");

						if (Item.Width.HasValue || Item.Height.HasValue)
						{
							Output.Append('[');

							if (Item.Width.HasValue)
							{
								Output.Append("width=");
								Output.Append(((Item.Width.Value * 3) / 4).ToString());
								Output.Append("pt");
							}

							if (Item.Height.HasValue)
							{
								if (Item.Width.HasValue)
									Output.Append(", ");

								Output.Append("height=");
								Output.Append(((Item.Height.Value * 3) / 4).ToString());
								Output.Append("pt");
							}

							Output.Append(']');
						}

						Output.Append('{');
						Output.Append(FileName.Replace('\\', '/'));
						Output.Append("}}");

						if (AloneInParagraph)
						{
							Output.AppendLine("\\end{figure}");
							Output.AppendLine();
						}
					}
				}
			}
		}

		/// <summary>
		/// Stores an image in binary form as a temporary file. Files will be deleted when application closes.
		/// </summary>
		/// <param name="BinaryImage">Binary image.</param>
		/// <returns>Temporary file name.</returns>
		public static Task<string> GetTemporaryFile(byte[] BinaryImage)
		{
			return GetTemporaryFile(BinaryImage, "tmp");
		}

		/// <summary>
		/// Stores an image in binary form as a temporary file. Files will be deleted when application closes.
		/// </summary>
		/// <param name="BinaryImage">Binary image.</param>
		/// <param name="FileExtension">File extension.</param>
		/// <returns>Temporary file name.</returns>
		public static async Task<string> GetTemporaryFile(byte[] BinaryImage, string FileExtension)
		{
			string FileName;

			using (SHA256 H = SHA256.Create())
			{
				byte[] Digest = H.ComputeHash(BinaryImage);
				FileName = Path.Combine(Path.GetTempPath(), "tmp" + Base64Url.Encode(Digest) + "." + FileExtension);
			}

			if (!File.Exists(FileName))
			{
				await Resources.WriteAllBytesAsync(FileName, BinaryImage);

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

		/// <summary>
		/// Generates Human-Readable XML for Smart Contracts from the markdown text.
		/// Ref: https://gitlab.com/IEEE-SA/XMPPI/IoT/-/blob/master/SmartContracts.md#human-readable-text
		/// </summary>
		/// <param name="Output">Smart Contract XML will be output here.</param>
		/// <param name="State">Current rendering state.</param>
		/// <param name="Items">Multimedia items.</param>
		/// <param name="ChildNodes">Child nodes.</param>
		/// <param name="AloneInParagraph">If the element is alone in a paragraph.</param>
		/// <param name="Document">Markdown document containing element.</param>
		public override async Task GenerateSmartContractXml(XmlWriter Output, SmartContractRenderState State,
			MultimediaItem[] Items, IEnumerable<MarkdownElement> ChildNodes, bool AloneInParagraph, MarkdownDocument Document)
		{
			try
			{
				string ContentType;
				string Title;
				byte[] Bin;
				int Width;
				int Height;

				foreach (MultimediaItem Item in Items)
				{
					try
					{
						Uri Uri = new Uri(Item.Url);
						KeyValuePair<string, TemporaryStream> P = await InternetContent.GetTempStreamAsync(
							new Uri(Item.Url), 10000, new KeyValuePair<string, string>("Accept", "image/*"));

						using (TemporaryStream f = P.Value)
						{
							MemoryStream ms = new MemoryStream();
							f.Position = 0;

							await f.CopyToAsync(ms);

							Bin = ms.ToArray();
							ContentType = P.Key;

							if (!(await InternetContent.DecodeAsync(ContentType, Bin, Uri) is SKImage Image))
								continue;

							Width = Item.Width ?? Image.Width;
							Height = Item.Height ?? Image.Height;
							Title = Item.Title;

							if (AloneInParagraph)
								Output.WriteStartElement("imageStandalone");
							else
								Output.WriteStartElement("imageInline");

							Output.WriteAttributeString("contentType", ContentType);
							Output.WriteAttributeString("width", Image.Width.ToString());
							Output.WriteAttributeString("height", Image.Height.ToString());

							Output.WriteStartElement("binary");
							Output.WriteValue(Convert.ToBase64String(Bin));
							Output.WriteEndElement();

							Output.WriteStartElement("caption");
							if (string.IsNullOrEmpty(Title))
								Output.WriteElementString("text", "Image");
							else
								Output.WriteElementString("text", Title);

							Output.WriteEndElement();
							Output.WriteEndElement();

							return;
						}
					}
					catch (Exception)
					{
						continue;
					}
				}
			}
			catch (Exception)
			{
			}

			Variables Variables = Document.Settings.Variables;
			Variables?.Push();

			try
			{
				foreach (MultimediaItem Item in Items)
					await InlineScript.GenerateSmartContractXml(Item.Url, Output, AloneInParagraph, Variables, State);
			}
			finally
			{
				Variables?.Pop();
			}

		}
	}
}
