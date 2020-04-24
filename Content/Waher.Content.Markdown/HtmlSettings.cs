using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown
{
	/// <summary>
	/// Contains settings that the HTML export uses to customize HTML output.
	/// </summary>
	public class HtmlSettings
	{
		private string hashtagClass = string.Empty;
		private string hashtagClickScript = string.Empty;

		/// <summary>
		/// Contains settings that the HTML export uses to customize HTML output.
		/// </summary>
		public HtmlSettings()
		{
		}

		/// <summary>
		/// Class name used on hashtag mark elements.
		/// </summary>
		public string HashtagClass
		{
			get => this.hashtagClass;
			set => this.hashtagClass = value;
		}

		/// <summary>
		/// Javascript to execute when hashtag mark element is clicked.
		/// </summary>
		public string HashtagClickScript
		{
			get => this.hashtagClickScript;
			set => this.hashtagClickScript = value;
		}

	}
}
