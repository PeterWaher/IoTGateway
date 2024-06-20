using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Xml;

namespace Waher.Networking.XMPP.Contracts.HumanReadable.BlockElements
{
	/// <summary>
	/// Standalone image
	/// </summary>
	public class Image : InlineBlock
	{
		private byte[] data;
		private string contentType;
		private string alt;
		private int width;
		private int height;

		/// <summary>
		/// Binary data of image.
		/// </summary>
		public byte[] Data
		{
			get => this.data;
			set => this.data = value;
		}

		/// <summary>
		/// Internet Content-Type of image.
		/// </summary>
		public string ContentType
		{
			get => this.contentType;
			set => this.contentType = value;
		}

		/// <summary>
		/// Image alternative text.
		/// </summary>
		public string AlternativeText
		{
			get => this.alt;
			set => this.alt = value;
		}

		/// <summary>
		/// Width of image, in pixels.
		/// </summary>
		public int Width
		{
			get => this.width;
			set => this.width = value;
		}

		/// <summary>
		/// Height of image, in pixels.
		/// </summary>
		public int Height
		{
			get => this.height;
			set => this.height = value;
		}

		/// <summary>
		/// Checks if the element is well-defined.
		/// </summary>
		/// <returns>Returns first failing element, if found.</returns>
		public override async Task<HumanReadableElement> IsWellDefined()
		{
			if (this.data is null || this.data.Length == 0)
				return this;

			if (string.IsNullOrEmpty(this.contentType) || !this.contentType.StartsWith("image/"))
				return this;

			if (string.IsNullOrEmpty(this.alt))
				return this;

			if (this.width <= 0 || this.width > InlineElements.Image.MaxSize)
				return this;

			if (this.height <= 0 || this.height > InlineElements.Image.MaxSize)
				return this;

			HumanReadableElement E = await base.IsWellDefined();
			if (!(E is null))
				return E;

			try
			{
				object Obj = await InternetContent.DecodeAsync(this.contentType, this.Data, null);
				if (Obj is null)
					return this;

				if (Obj is IDisposable Disposable)
					Disposable.Dispose();

				return null;
			}
			catch (Exception)
			{
				return this;
			}
		}

		/// <summary>
		/// Serializes the element in normalized form.
		/// </summary>
		/// <param name="Xml">XML Output.</param>
		public override void Serialize(StringBuilder Xml)
		{
			Xml.Append("<imageStandalone contentType='");
			Xml.Append(XML.Encode(this.contentType));
			Xml.Append("' width='");
			Xml.Append(this.width.ToString());
			Xml.Append("' height='");
			Xml.Append(this.height.ToString());
			Xml.Append("'><binary>");

			if (!(this.data is null))
				Xml.Append(Convert.ToBase64String(this.data));

			Xml.Append("</binary><caption>");

			Serialize(Xml, this.Elements);

			Xml.Append("</caption></imageStandalone>");
		}

		/// <summary>
		/// Generates markdown for the human-readable text.
		/// </summary>
		/// <param name="Markdown">Markdown output.</param>
		/// <param name="SectionLevel">Current section level.</param>
		/// <param name="Indentation">Current indentation.</param>
		/// <param name="Settings">Settings used for Markdown generation of human-readable text.</param>
		public override void GenerateMarkdown(MarkdownOutput Markdown, int SectionLevel, int Indentation, MarkdownSettings Settings)
		{
			Markdown.Indent(Indentation);
			Markdown.Append("![");
			base.GenerateMarkdown(Markdown, SectionLevel, Indentation, Settings);
			Markdown.Append("](data:");
			Markdown.Append(this.contentType);
			Markdown.Append(";base64,");

			if (!(this.data is null))
				Markdown.Append(Convert.ToBase64String(this.data));

			Markdown.Append(' ');
			Markdown.Append(this.width.ToString());
			Markdown.Append(' ');
			Markdown.Append(this.height.ToString());
			Markdown.Append(')');
			Markdown.AppendLine();

			Markdown.Indent(Indentation);
			Markdown.AppendLine();
		}
	}
}