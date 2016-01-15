using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using Waher.Content;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls.Chat
{
	public enum ChatItemType
	{
		Received,
		Transmitted
	}

	/// <summary>
	/// Represents one item in a chat.
	/// </summary>
	public class ChatItem : ColorableItem
	{
		internal static readonly Emoji1LocalFiles Emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Png64, 24, 24, "pack://siteoforigin:,,,/Graphics/Emoji1/png/64x64/%FILENAME%");

		private ChatItemType type;
		private DateTime timestamp;
		private string message;
		private object formattedMessage;

		/// <summary>
		/// Represents one item in a chat output.
		/// </summary>
		/// <param name="Type">Type of chat record.</param>
		/// <param name="Message">Message</param>
		/// <param name="FormattedMessage">Formatted message.</param>
		/// <param name="Data">Optional binary data.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public ChatItem(ChatItemType Type, string Message, Color ForegroundColor, Color BackgroundColor)
			: base(ForegroundColor, BackgroundColor)
		{
			this.type = Type;
			this.timestamp = DateTime.Now;
			this.message = Message;

			try
			{
				MarkdownDocument Markdown = new MarkdownDocument(Message, new MarkdownSettings(Emoji1_24x24, false));
				string XAML = Markdown.GenerateXAML();
				this.formattedMessage = XamlReader.Parse(XAML);
			}
			catch (Exception)
			{
				this.formattedMessage = Message;
			}
		}

		/// <summary>
		/// Timestamp of event.
		/// </summary>
		public DateTime Timestamp { get { return this.timestamp; } }

		/// <summary>
		/// Chat item type.
		/// </summary>
		public ChatItemType Type { get { return this.type; } }

		/// <summary>
		/// Time of day of reception, as a string.
		/// </summary>
		public string Received 
		{
			get 
			{
				if (this.type == ChatItemType.Received)
					return this.timestamp.ToLongTimeString();
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Time of day of transmission, as a string.
		/// </summary>
		public string Sent
		{
			get
			{
				if (this.type == ChatItemType.Transmitted)
					return this.timestamp.ToLongTimeString();
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Message
		/// </summary>
		public string Message { get { return this.message; } }

		/// <summary>
		/// Formatted Message
		/// </summary>
		public object FormattedMessage { get { return this.formattedMessage; } }

	}
}
