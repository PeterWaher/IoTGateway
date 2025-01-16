using SkiaSharp;
using System;
using System.Threading.Tasks;
using System.Xml;

namespace Waher.Content.Markdown.Contracts.CodeContent
{
	/// <summary>
	/// Base64-encoded image content.
	/// </summary>
	public class ImageContent : Model.CodeContent.ImageContent, ICodeContentContractsRenderer
	{
		/// <summary>
		/// Base64-encoded image content.
		/// </summary>
		public ImageContent()
		{
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
				XmlWriter Output = Renderer.XmlOutput;
				int i = Language.IndexOf(':');
				string ContentType = i < 0 ? Language : Language.Substring(0, i);
				string Title = i < 0 ? string.Empty : Language.Substring(i + 1);
				string Base64 = GetImageBase64(Rows);
				byte[] Bin = Convert.FromBase64String(Base64);

				ContentResponse Decoded = await InternetContent.DecodeAsync(ContentType, Bin, null);

				if (Decoded.HasError || !(Decoded.Decoded is SKImage Image))
					return false;

				Output.WriteStartElement("imageStandalone");
				Output.WriteAttributeString("contentType", ContentType);
				Output.WriteAttributeString("width", Image.Width.ToString());
				Output.WriteAttributeString("height", Image.Height.ToString());

				Output.WriteStartElement("binary");
				Output.WriteValue(Base64);
				Output.WriteEndElement();

				Output.WriteStartElement("caption");
				if (string.IsNullOrEmpty(Title))
					Output.WriteElementString("text", "Image");
				else
					Output.WriteElementString("text", Title);

				Output.WriteEndElement();
				Output.WriteEndElement();

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
