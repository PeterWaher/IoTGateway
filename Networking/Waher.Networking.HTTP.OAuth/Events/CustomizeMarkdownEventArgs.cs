using System;

namespace Waher.Networking.HTTP.OAuth.Events
{
	/// <summary>
	/// Event arguments for markdown customization events.
	/// </summary>
	public class CustomizeMarkdownEventArgs : EventArgs
	{
		/// <summary>
		/// Event arguments for markdown customization events.
		/// </summary>
		/// <param name="Markdown">Markdown text that can be customized.</param>
		public CustomizeMarkdownEventArgs(string Markdown)
		{
			this.Markdown = Markdown;
		}

		/// <summary>
		/// Markdown text that can be customized.
		/// </summary>
		public string Markdown { get; set; }
	}
}
