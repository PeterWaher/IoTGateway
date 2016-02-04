using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Multimedia item.
	/// </summary>
	public class MultimediaItem
	{
		private string url;
		private string title;
		private int? width;
		private int? height;

		/// <summary>
		/// Multimedia item.
		/// </summary>
		/// <param name="Url">URL</param>
		/// <param name="Title">Title</param>
		/// <param name="Width">Width of media item, if available.</param>
		/// <param name="Height">Height of media item, if available.</param>
		public MultimediaItem(string Url, string Title, int? Width, int? Height)
		{
			this.url = Url;
			this.title = Title;
			this.width = Width;
			this.height = Height;
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

	}
}
