using SkiaSharp;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Waher.Content.Html;
using Waher.Content.Images;
using Waher.Content.Xml;
using Waher.Content.Xml.Text;
using Waher.Content.Xsl;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;

namespace Waher.Content.Markdown.Layout2D
{
	/// <summary>
	/// Converts GraphViz documents to images.
	/// </summary>
	public class Layout2DXmlToImageConverter : IContentConverter
	{
		private static readonly Regex processingInstructionData= new Regex(
			@"(\s*((type=['""](?'Type'[^'""]*)['""])|(href=['""](?'HRef'[^'""]*)['""])|([^ =]+=['""]([^'""]*)['""])))*",
			RegexOptions.Singleline | RegexOptions.Compiled);

		/// <summary>
		/// Converts GraphViz documents to images.
		/// </summary>
		public Layout2DXmlToImageConverter()
		{
		}

		/// <summary>
		/// Converts content from these content types.
		/// </summary>
		public string[] FromContentTypes => XmlCodec.XmlContentTypes;

		/// <summary>
		/// Converts content to these content types. 
		/// </summary>
		public virtual string[] ToContentTypes => ImageCodec.ImageContentTypes;

		/// <summary>
		/// How well the content is converted.
		/// </summary>
		public virtual Grade ConversionGrade => Grade.Barely;

		/// <summary>
		/// Performs the actual conversion.
		/// </summary>
		/// <param name="State">State of the current conversion.</param>
		/// <returns>If the result is dynamic (true), or only depends on the source (false).</returns>
		public async Task<bool> ConvertAsync(ConversionState State)
		{
			byte[] Bin = await State.From.ReadAllAsync();
			string Xml = Strings.GetString(Bin, Encoding.UTF8);
			string s = State.ToContentType;
			int i;

			i = s.IndexOf(';');
			if (i > 0)
				s = s.Substring(0, i);

			string Extension;
			SKEncodedImageFormat Format;
			int Quality = 100;

			switch (s.ToLower())
			{
				case ImageCodec.ContentTypePng:
					Format = SKEncodedImageFormat.Png;
					Extension = ImageCodec.FileExtensionPng;
					break;

				case ImageCodec.ContentTypeBmp:
					Format = SKEncodedImageFormat.Bmp;
					Extension = ImageCodec.FileExtensionBmp;
					break;

				case ImageCodec.ContentTypeGif:
					Format = SKEncodedImageFormat.Gif;
					Extension = ImageCodec.FileExtensionGif;
					break;

				case ImageCodec.ContentTypeJpg:
					Format = SKEncodedImageFormat.Jpeg;
					Extension = ImageCodec.FileExtensionJpg;
					Quality = 90;
					break;

				case ImageCodec.ContentTypeWebP:
					Format = SKEncodedImageFormat.Webp;
					Extension = ImageCodec.FileExtensionWebP;
					break;

				case ImageCodec.ContentTypeIcon:
					Format = SKEncodedImageFormat.Ico;
					Extension = ImageCodec.FileExtensionIcon;
					break;

				case ImageCodec.ContentTypeTiff:
				case ImageCodec.ContentTypeWmf:
				case ImageCodec.ContentTypeEmf:
				case ImageCodec.ContentTypeSvg:
				default:
					Format = XmlLayout.DefaultFormat;
					Extension = XmlLayout.DefaultFileExtension;
					State.ToContentType = XmlLayout.DefaultContentType;
					break;
			}

			string ContentType = State.FromContentType;
			Variables Variables;
			GraphInfo Graph;
			bool Transformed;
			bool ValidXml = true;

			do
			{
				Transformed = false;
				Variables = new Variables();
				Graph = await XmlLayout.GetFileName("layout", Xml, Variables, Format, Quality, Extension);

				if (!Graph.Converted && !(Graph.Xml is null))
				{
					foreach (XmlNode N in Graph.Xml)
					{
						if (N is XmlProcessingInstruction PI && 
							PI.Name == "xml-stylesheet")
						{
							Match M = processingInstructionData.Match(PI.Data);
							if (M.Success && M.Groups["Type"].Value == "text/xsl")
							{
								string HRef = M.Groups["HRef"].Value;

								if (State.TryGetLocalResourceFileName(HRef, null, out string FileName) &&
									File.Exists(FileName))
								{
									using (FileStream fs = File.OpenRead(FileName))
									{
										XslCompiledTransform Transform = XSL.LoadTransform(fs);
										Xml = XSL.Transform(Xml, Transform);
										ValidXml = XML.IsValidXml(Xml);
										Bin = null;

										if (!ValidXml)
										{
											bool IsHtml;

											try
											{
												HtmlDocument Doc = new HtmlDocument(Xml);
												IsHtml = !(Doc.Html is null) && !(Doc.Body is null);
											}
											catch (Exception)
											{
												IsHtml = false;
											}

											if (!IsHtml)
											{
												MarkdownSettings Settings = new MarkdownSettings(null,
													MarkdownDocument.HeaderEndPosition(Xml).HasValue,
													State.Session)
												{
													ResourceMap = State.ResourceMap
												};

												MarkdownDocument Doc = await MarkdownDocument.CreateAsync(Xml, Settings, State.FromFileName, State.LocalResourceName, State.URL);
												Xml = await Doc.GenerateHTML();
											}

											ContentType = HtmlCodec.DefaultContentType;
										}
									}
								}
							}
						}
					}
				}
			}
			while (!Graph.Converted && Transformed && ValidXml);

			if (!Graph.Converted)
			{
				if (Bin is null)
					Bin = Encoding.UTF8.GetBytes(Xml);

				await State.To.WriteAsync(Bin, 0, Bin.Length);
				State.ToContentType = ContentType;
				return true;
			}
			else if (Graph.Dynamic)
			{
				await State.To.WriteAsync(Graph.DynamicContent, 0, Graph.DynamicContent.Length);
				return true;
			}
			else
			{
				byte[] Data = await Files.ReadAllBytesAsync(Graph.FileName);

				await State.To.WriteAsync(Data, 0, Data.Length);

				return false;
			}
		}
	}
}