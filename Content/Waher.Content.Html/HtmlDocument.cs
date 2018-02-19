using System;
using Waher.Content;

namespace Waher.Content.Html
{
	/// <summary>
	/// HTML document.
	/// </summary>
    public class HtmlDocument
    {
		private string html;

		/// <summary>
		/// HTML document.
		/// </summary>
		/// <param name="Html">HTML text.</param>
		public HtmlDocument(string Html)
		{
			this.html = Html;
		}

		/// <summary>
		/// HTML text.
		/// </summary>
		public string Html
		{
			get { return this.html; }
		}

	}
}
