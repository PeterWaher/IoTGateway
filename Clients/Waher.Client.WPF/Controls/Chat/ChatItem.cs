using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Model;
using Waher.Events;
using Waher.Client.WPF.Model;
using System.Threading.Tasks;
using Waher.Content.Markdown.Wpf;

namespace Waher.Client.WPF.Controls.Chat
{
	public enum ChatItemType
	{
		Received,
		Transmitted,
		Event
	}

	/// <summary>
	/// Represents one item in a chat.
	/// </summary>
	public class ChatItem : ColorableItem
	{
		private readonly ChatItemType type;
		private readonly DateTime timestamp;
		private bool lastIsTable;
		private readonly string threadId;
		private string from;
		private string fromStr;
		private DateTime lastUpdated;
		private string message;
		private object formattedMessage;
		private StringBuilder building = null;
		private DateTime timer = DateTime.MinValue;

		/// <summary>
		/// Represents one item in a chat output.
		/// </summary>
		/// <param name="Type">Type of chat record.</param>
		/// <param name="Message">Message</param>
		/// <param name="From">From where the message came.</param>
		/// <param name="ThreadId">Thread ID</param>
		/// <param name="FormattedMessage">Formatted message.</param>
		/// <param name="Data">Optional binary data.</param>
		/// <param name="ForegroundColor">Foreground Color</param>
		/// <param name="BackgroundColor">Background Color</param>
		private ChatItem(ChatItemType Type, DateTime Timestamp, string Message, string From, string ThreadId,
			Color ForegroundColor, Color BackgroundColor)
			: base(ForegroundColor, BackgroundColor)
		{
			this.type = Type;
			this.timestamp = this.lastUpdated = Timestamp;
			this.message = Message;
			this.from = From;
			this.fromStr = GetShortFrom(From);
			this.threadId = ThreadId;
		}

		public static async Task<ChatItem> CreateAsync(ChatItemType Type, DateTime Timestamp, string Message, string From, MarkdownDocument Markdown, string ThreadId,
			Color ForegroundColor, Color BackgroundColor)
		{
			ChatItem Result = new ChatItem(Type, Timestamp, Message, From, ThreadId, ForegroundColor, BackgroundColor);

			if (Markdown is null)
			{
				XamlSettings Settings = ChatView.GetXamlSettings();

				Result.formattedMessage = new TextBlock()
				{
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(Settings.ParagraphMarginLeft, Settings.ParagraphMarginTop, Settings.ParagraphMarginRight, Settings.ParagraphMarginBottom),
					Text = Message
				};

				if (Result.formattedMessage is DependencyObject Root)
					Result.AddEventHandlers(Root);

				Result.lastIsTable = false;
			}
			else
			{
				await Result.ParseMarkdown(Markdown);

				foreach (MarkdownElement E in Markdown)
					Result.lastIsTable = E is Content.Markdown.Model.BlockElements.Table;
			}

			return Result;
		}

		internal bool LastIsTable => this.lastIsTable;

		internal void Append(string Message, ListView ChatListView, MainWindow MainWindow)
		{
			if (this.building is null)
			{
				this.building = new StringBuilder(this.message);

				if (!this.message.EndsWith("\n"))
					this.building.Append(Environment.NewLine);
			}

			this.building.Append(Message);
			if (!Message.EndsWith("\n"))
				this.building.Append(Environment.NewLine);

			if (this.timer > DateTime.MinValue)
			{
				MainWindow.Scheduler?.Remove(this.timer);
				this.timer = DateTime.MinValue;
			}

			this.timer = MainWindow.Scheduler.Add(DateTime.Now.AddSeconds(1), this.Refresh, new object[] { ChatListView, MainWindow });
		}

		private async void Refresh(object P)
		{
			try
			{
				if (this.timer > DateTime.MinValue)
				{
					WPF.MainWindow.Scheduler?.Remove(this.timer);
					this.timer = DateTime.MinValue;
				}

				object[] P2 = (object[])P;
				ListView ChatListView = (ListView)P2[0];
				MainWindow MainWindow = (MainWindow)P2[1];
				string s = this.building.ToString();
				this.building = null;

				MarkdownDocument Markdown = await MarkdownDocument.CreateAsync(s, ChatView.GetMarkdownSettings());

				MainWindow.UpdateGui(() => this.Refresh2(ChatListView, s, Markdown));
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task Refresh2(ListView ChatListView, string s, MarkdownDocument Markdown)
		{
			try
			{
				await this.Update(s, Markdown);

				ChatListView.Items.Refresh();
				ChatListView.ScrollIntoView(this);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task ParseMarkdown(MarkdownDocument Markdown)
		{
			try
			{
				if (!(Markdown is null))
				{
					string XAML = await Markdown.GenerateXAML(ChatView.GetXamlSettings());
					this.formattedMessage = XamlReader.Parse(XAML);

					if (this.formattedMessage is DependencyObject Root)
						this.AddEventHandlers(Root);
				}
				else
					this.formattedMessage = this.Message;
			}
			catch (Exception)
			{
				this.formattedMessage = this.Message;
			}
		}

		private void AddEventHandlers(DependencyObject Element)
		{
			int i, c = VisualTreeHelper.GetChildrenCount(Element);
			DependencyObject Child;

			if (Element is TextBlock TextBlock)
			{
				foreach (Inline Inline in TextBlock.Inlines)
				{
					if (Inline is Hyperlink Hyperlink)
						Hyperlink.Click += this.Hyperlink_Click;
				}
			}

			for (i = 0; i < c; i++)
			{
				Child = VisualTreeHelper.GetChild(Element, i);
				this.AddEventHandlers(Child);
			}
		}

		private void Hyperlink_Click(object Sender, RoutedEventArgs e)
		{
			if (!(e.Source is Hyperlink Link))
				return;

			string Uri = Link.NavigateUri.ToString();
			System.Diagnostics.Process.Start(Uri);
		}

		public Task Update(string Message, MarkdownDocument Markdown)
		{
			this.message = Message;
			this.lastUpdated = DateTime.Now;

			return this.ParseMarkdown(Markdown);
		}

		/// <summary>
		/// Timestamp of item.
		/// </summary>
		public DateTime Timestamp => this.timestamp;

		/// <summary>
		/// Timestamp when item was last updated.
		/// </summary>
		public DateTime LastUpdated => this.lastUpdated;

		/// <summary>
		/// Chat item type.
		/// </summary>
		public ChatItemType Type => this.type;

		/// <summary>
		/// Who sent the message.
		/// </summary>
		public string From
		{
			get => this.from;
			set
			{
				this.from = value;
				this.fromStr = GetShortFrom(value);
			}
		}

		/// <summary>
		/// Thread ID
		/// </summary>
		public string ThreadId => this.threadId;

		/// <summary>
		/// Nick-name of sender.
		/// </summary>
		public string FromStr => this.fromStr;

		internal static string GetShortFrom(string From)
		{
			int i = From.IndexOfAny(Content.CommonTypes.CRLF);
			if (i >= 0)
			{
				StringBuilder sb = new StringBuilder();
				bool First = true;

				foreach (string Row in From.Split(Content.CommonTypes.CRLF, StringSplitOptions.RemoveEmptyEntries))
				{
					if (First)
						First = false;
					else
						sb.AppendLine();

					sb.Append(GetShortFrom(Row));
				}

				return sb.ToString();
			}
			else
			{
				if (From is null)
					return string.Empty;

				i = From.IndexOf('/');
				if (i < 0)
					return From;

				return From.Substring(i + 1);
			}
		}

		/// <summary>
		/// Resource-name of who sent the message.
		/// </summary>
		public string FromResource
		{
			get
			{
				int i = this.from?.IndexOf('/') ?? -1;
				if (i < 0)
					return string.Empty;
				else
					return this.from.Substring(i + 1);
			}
		}

		/// <summary>
		/// Time of day of reception, as a string.
		/// </summary>
		public string Received
		{
			get
			{
				if (this.type == ChatItemType.Received || this.type == ChatItemType.Event)
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
		public string Message => this.message;

		/// <summary>
		/// Formatted Message
		/// </summary>
		public object FormattedMessage => this.formattedMessage;

	}
}
