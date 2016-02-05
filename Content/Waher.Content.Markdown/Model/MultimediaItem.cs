using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Multimedia item.
	/// </summary>
	public class MultimediaItem
	{
		private MarkdownDocument doc;
		private string url;
		private string title;
		private string extension;
		private string contentType;
		private int? width;
		private int? height;

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
				if (this.extension == null)
				{
					int i = this.url.IndexOfAny(QuestionOrHash);
					this.extension = Path.GetExtension(i > 0 ? Url.Substring(0, i) : Url);
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
				if (this.contentType == null)
					this.contentType = InternetContent.GetContentType(this.Extension);

				return this.contentType;
			}
		}

	}
}
