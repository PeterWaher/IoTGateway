using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
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
		private ChatItemType type;
		private DateTime timestamp;
		private DateTime lastUpdated;
		private string message;
		private object formattedMessage;

		/// <summary>
		/// Represents one item in a chat output.
		/// </summary>
		/// <param name="Type">Type of chat record.</param>
		/// <param name="Message">Message</param>
		/// <param name="Markdown">Markdown, if available, or null if plain text.</param>
		/// <param name="FormattedMessage">Formatted message.</param>
		/// <param name="Data">Optional binary data.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		public ChatItem(ChatItemType Type, string Message, MarkdownDocument Markdown, Color ForegroundColor, Color BackgroundColor)
			: base(ForegroundColor, BackgroundColor)
		{
			this.type = Type;
			this.timestamp = this.lastUpdated = DateTime.Now;
			this.message = Message;

			this.ParseMarkdown(Markdown);
		}

		private void ParseMarkdown(MarkdownDocument Markdown)
		{
			try
			{
				if (Markdown != null)
				{
					XamlSettings Settings = new XamlSettings()
					{
						TableCellRowBackgroundColor1 = "#20404040",
						TableCellRowBackgroundColor2 = "#10808080"
					};

					string XAML = Markdown.GenerateXAML(Settings);
					this.formattedMessage = XamlReader.Parse(XAML);

					if (this.formattedMessage is DependencyObject Root)
						this.AddEventHandlers(Root);
				}
				else
					this.formattedMessage = Message;
			}
			catch (Exception)
			{
				this.formattedMessage = Message;
			}
		}

		private void AddEventHandlers(DependencyObject Element)
		{
			int i, c = VisualTreeHelper.GetChildrenCount(Element);
			DependencyObject Child;
			Hyperlink Hyperlink;
			TextBlock TextBlock;

			if ((TextBlock = Element as TextBlock) != null)
			{
				foreach (Inline Inline in TextBlock.Inlines)
				{
					if ((Hyperlink = Inline as Hyperlink) != null)
						Hyperlink.Click += Hyperlink_Click;
				}
			}

			for (i = 0; i < c; i++)
			{
				Child = VisualTreeHelper.GetChild(Element, i);
				this.AddEventHandlers(Child);
			}
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			Hyperlink Link = e.Source as Hyperlink;
			if (Link == null)
				return;

			string Uri = Link.NavigateUri.ToString();
			System.Diagnostics.Process.Start(Uri);
		}

		public void Update(string Message, MarkdownDocument Markdown)
		{
			this.message = Message;
			this.lastUpdated = DateTime.Now;

			this.ParseMarkdown(Markdown);
		}

		/// <summary>
		/// Timestamp of item.
		/// </summary>
		public DateTime Timestamp { get { return this.timestamp; } }

		/// <summary>
		/// Timestamp when item was last updated.
		/// </summary>
		public DateTime LastUpdated { get { return this.lastUpdated; } }

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
