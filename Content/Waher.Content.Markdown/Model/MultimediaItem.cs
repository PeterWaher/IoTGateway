using System;
using System.IO;
using System.Xml;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Multimedia item.
	/// </summary>
	public class MultimediaItem
	{
		private readonly MarkdownDocument doc;
		private readonly string url;
		private readonly string title;
		private string extension;
		private string contentType;
		private readonly int? width;
		private readonly int? height;

		/// <summary>
		/// Multimedia item.
		/// </summary>
		/// <param name="Doc">Markdown document.</param>
		/// <param name="Url">URL</param>
		/// <param name="Title">Title</param>
		/// <param name="Width">Width of media item, if available.</param>
		/// <param name="Height">Height of media item, if available.</param>
		public MultimediaItem(MarkdownDocument Doc, string Url, string Title, int? Width, int? Height)
		{
			this.doc = Doc;
			this.url = Url;
			this.title = Title;
			this.width = Width;
			this.height = Height;
		}

		/// <summary>
		/// Markdown document
		/// </summary>
		public MarkdownDocument Document
		{
			get { return this.doc; }
		}

		/// <summary>
		/// URL
		/// </summary>
		public string Url
		{
			get { return this.url; }
		}

		/// <summary>
		/// Optional title.
		/// </summary>
		public string Title
		{
			get { return this.title; }
		}

		/// <summary>
		/// Width of media item, if available.
		/// </summary>
		public int? Width
		{
			get { return this.width; }
		}

		/// <summary>
		/// Height of media item, if available.
		/// </summary>
		public int? Height
		{
			get { return this.height; }
		}

		/// <summary>
		/// Resource extension.
		/// </summary>
		public string Extension
		{
			get
			{
				if (this.extension is null)
				{
					int i = this.url.IndexOfAny(QuestionOrHash);
					if (i > 0)
					{
						this.extension = Path.GetExtension(this.url.Substring(0, i));

						if (string.IsNullOrEmpty(this.extension))
						{
							int j = this.url.IndexOf('?', i + 1);
							if (j > i)
								this.extension = Path.GetExtension(this.url.Substring(0, j));
							else
								this.extension = Path.GetExtension(this.url);
						}
					}
					else
						this.extension = Path.GetExtension(this.url);
				}

				return this.extension;
			}
		}

		private static readonly char[] QuestionOrHash = new char[] { '?', '#' };

		/// <summary>
		/// Content Type
		/// </summary>
		public string ContentType
		{
			get
			{
				if (this.contentType is null)
					this.contentType = InternetContent.GetContentType(this.Extension);

				return this.contentType;
			}
		}

		/// <summary>
		/// Exports the element to XML.
		/// </summary>
		/// <param name="Output">XML Output.</param>
		public void Export(XmlWriter Output)
		{
			Output.WriteStartElement("MultimediaItem");
			Output.WriteAttributeString("url", this.url);
			Output.WriteAttributeString("title", this.title);
			Output.WriteAttributeString("extension", this.extension);
			Output.WriteAttributeString("contentType", this.contentType);

			if (this.width.HasValue)
				Output.WriteAttributeString("width", this.width.Value.ToString());

			if (this.height.HasValue)
				Output.WriteAttributeString("height", this.height.Value.ToString());

			Output.WriteEndElement();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return obj is MultimediaItem Obj &&
				this.url == Obj.url &&
				this.title == Obj.title &&
				this.extension == Obj.extension &&
				this.contentType == Obj.contentType &&
				this.width == Obj.width &&
				this.height == Obj.height;
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			int h1 = this.url?.GetHashCode() ?? 0;
			int h2 = this.title?.GetHashCode() ?? 0;

			h1 = ((h1 << 5) + h1) ^ h2;

			h2 = this.extension?.GetHashCode() ?? 0;
			h1 = ((h1 << 5) + h1) ^ h2;

			h2 = this.contentType?.GetHashCode() ?? 0;
			h1 = ((h1 << 5) + h1) ^ h2;

			h2 = this.width?.GetHashCode() ?? 0;
			h1 = ((h1 << 5) + h1) ^ h2;

			h2 = this.height?.GetHashCode() ?? 0;
			h1 = ((h1 << 5) + h1) ^ h2;

			return h1;
		}

	}
}
