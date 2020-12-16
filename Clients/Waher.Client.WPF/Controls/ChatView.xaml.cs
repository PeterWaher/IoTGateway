using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Waher.Content;
using Waher.Content.Emoji.Emoji1;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Content.Xsl;
using Waher.Events;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for ChatView.xaml
	/// </summary>
	public partial class ChatView : UserControl, ITabView
	{
		private static Emoji1LocalFiles emoji1_24x24 = null;
		private readonly TreeNode node;
		private bool muc;

		public ChatView(TreeNode Node, bool Muc)
		{
			this.node = Node;
			this.muc = Muc;

			InitializeComponent();

			if (Muc)
			{
				this.FromColumn.Width = this.ReceivedColumn.Width;
				this.ContentColumn.Width -= this.ReceivedColumn.Width;
			}
		}

		internal static void InitEmojis()
		{
			if (emoji1_24x24 is null)
			{
				string Folder = Assembly.GetExecutingAssembly().Location;
				if (string.IsNullOrEmpty(Folder))
					Folder = AppDomain.CurrentDomain.BaseDirectory;

				emoji1_24x24 = new Emoji1LocalFiles(Emoji1SourceFileType.Png64, 24, 24,
					Path.Combine(MainWindow.AppDataFolder, "Graphics", "Emoji1", "png", "64x64", "%FILENAME%"),
					Path.Combine(Path.GetDirectoryName(Folder), "Graphics", "Emoji1.zip"),
					Path.Combine(MainWindow.AppDataFolder, "Graphics"));
			}
		}

		public static Emoji1LocalFiles Emoji1_24x24
		{
			get { return emoji1_24x24; }
		}

		public void Dispose()
		{
			this.Node?.ViewClosed();
		}

		public TreeNode Node
		{
			get { return this.node; }
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (this.ChatListView.View is GridView GridView)
			{
				GridView.Columns[2].Width = Math.Max(this.ActualWidth - GridView.Columns[0].ActualWidth -
					GridView.Columns[1].ActualWidth - GridView.Columns[3].ActualWidth -
					SystemParameters.VerticalScrollBarWidth - 8, 10);
			}

			this.Input.Width = Math.Max(this.ActualWidth - this.SendButton.ActualWidth - 16, 10);
		}

		public static MarkdownSettings GetMarkdownSettings()
		{
			return new MarkdownSettings(Emoji1_24x24, false)
			{
				XamlSettings = new XamlSettings()
				{
					TableCellRowBackgroundColor1 = "#20404040",
					TableCellRowBackgroundColor2 = "#10808080"
				}
			};
		}

		private void Send_Click(object sender, RoutedEventArgs e)
		{
			string Msg = this.Input.Text;
			this.Input.Text = string.Empty;
			this.Input.Focus();

			this.ChatMessageTransmitted(Msg, out MarkdownDocument Markdown);

			this.node.SendChatMessage(Msg, Markdown);
		}

		public void ChatMessageTransmitted(string Message, out MarkdownDocument Markdown)
		{
			ChatItem Item;

			if (Message.IndexOf('|') >= 0)
			{
				string s;
				int c = this.ChatListView.Items.Count;

				if (c > 0 &&
					(Item = this.ChatListView.Items[c - 1] as ChatItem) != null &&
					Item.Type == ChatItemType.Transmitted &&
					string.IsNullOrWhiteSpace(Item.From) &&
					(DateTime.Now - Item.LastUpdated).TotalSeconds < 10 &&
					(s = Item.Message).IndexOf('|') >= 0)
				{
					try
					{
						if (!s.EndsWith("\n"))
							s += Environment.NewLine;

						s += Message;
						Markdown = new MarkdownDocument(s, GetMarkdownSettings());
						Item.Update(s, Markdown);
						this.ChatListView.Items.Refresh();
						this.ChatListView.ScrollIntoView(Item);
						return;
					}
					catch (Exception)
					{
						// Ignore.
					}
				}
			}

			try
			{
				Markdown = new MarkdownDocument(Message, GetMarkdownSettings());
			}
			catch (Exception)
			{
				Markdown = null;
			}

			Item = new ChatItem(ChatItemType.Transmitted, DateTime.Now, Message, string.Empty, Markdown, Colors.Black, Colors.Honeydew);
			ListViewItem ListViewItem = new ListViewItem()
			{
				Content = Item,
				Foreground = new SolidColorBrush(Colors.Black),
				Background = new SolidColorBrush(Colors.Honeydew),
				Margin = new Thickness(0)
			};
			this.ChatListView.Items.Add(ListViewItem);
			this.ChatListView.ScrollIntoView(ListViewItem);
		}

		public void ChatMessageReceived(string Message, string From, bool IsMarkdown, DateTime Timestamp, MainWindow MainWindow)
		{
			MarkdownDocument Markdown;
			ChatItem Item;

			if (IsMarkdown)
			{
				if (Message.IndexOf('|') >= 0)
				{
					int c = this.ChatListView.Items.Count;

					if (c > 0 &&
						(Item = this.ChatListView.Items[c - 1] as ChatItem) != null &&
						Item.Type == ChatItemType.Received &&
						Item.From == From &&
						(DateTime.Now - Item.LastUpdated).TotalSeconds < 10 &&
						Item.LastIsTable)
					{
						Item.Append(Message, this.ChatListView, MainWindow);
						return;
					}
				}

				try
				{
					Markdown = new MarkdownDocument(Message, GetMarkdownSettings());
				}
				catch (Exception)
				{
					Markdown = null;
				}
			}
			else
				Markdown = null;

			Item = new ChatItem(ChatItemType.Received, Timestamp, Message, From, Markdown, Colors.Black, Colors.AliceBlue);
			ListViewItem ListViewItem = new ListViewItem()
			{
				Content = Item,
				Foreground = new SolidColorBrush(Colors.Black),
				Background = new SolidColorBrush(Colors.AliceBlue),
				Margin = new Thickness(0)
			};
			this.ChatListView.Items.Add(ListViewItem);
			this.ChatListView.ScrollIntoView(ListViewItem);
		}

		private void UserControl_GotFocus(object sender, RoutedEventArgs e)
		{
			this.Input.Focus();
		}

		public void NewButton_Click(object sender, RoutedEventArgs e)
		{
			this.ChatListView.Items.Clear();
		}

		public void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			this.SaveAsButton_Click(sender, e);
		}

		public void SaveAsButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog Dialog = new SaveFileDialog()
			{
				AddExtension = true,
				CheckPathExists = true,
				CreatePrompt = false,
				DefaultExt = "html",
				Filter = "XML Files (*.xml)|*.xml|HTML Files (*.html,*.htm)|*.html,*.htm|All Files (*.*)|*.*",
				Title = "Save chat session"
			};

			bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

			if (Result.HasValue && Result.Value)
			{
				try
				{
					if (Dialog.FilterIndex == 2)
					{
						StringBuilder Xml = new StringBuilder();
						using (XmlWriter w = XmlWriter.Create(Xml, XML.WriterSettings(true, true)))
						{
							this.SaveAsXml(w);
						}

						string Html = XSL.Transform(Xml.ToString(), chatToHtml);

						File.WriteAllText(Dialog.FileName, Html, System.Text.Encoding.UTF8);
					}
					else
					{
						using (FileStream f = File.Create(Dialog.FileName))
						{
							using (XmlWriter w = XmlWriter.Create(f, XML.WriterSettings(true, false)))
							{
								this.SaveAsXml(w);
							}
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(MainWindow.FindWindow(this), ex.Message, "Unable to save file.", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private static readonly XslCompiledTransform chatToHtml = XSL.LoadTransform("Waher.Client.WPF.Transforms.ChatToHTML.xslt");
		private static readonly XmlSchema schema = XSL.LoadSchema("Waher.Client.WPF.Schema.Chat.xsd");
		private const string chatNamespace = "http://waher.se/Schema/Chat.xsd";
		private const string chatRoot = "Chat";

		private void SaveAsXml(XmlWriter w)
		{
			w.WriteStartElement(chatRoot, chatNamespace);
			w.WriteAttributeString("muc", CommonTypes.Encode(this.muc));

			foreach (ListViewItem Item in this.ChatListView.Items)
			{
				if (Item.Content is ChatItem ChatItem)
				{
					w.WriteStartElement(ChatItem.Type.ToString());
					w.WriteAttributeString("timestamp", XML.Encode(ChatItem.Timestamp));

					if (!string.IsNullOrEmpty(ChatItem.From))
						w.WriteAttributeString("from", ChatItem.FromStr);

					w.WriteValue(ChatItem.Message);
					w.WriteEndElement();
				}
			}

			w.WriteEndElement();
			w.Flush();
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog Dialog = new OpenFileDialog()
				{
					AddExtension = true,
					CheckFileExists = true,
					CheckPathExists = true,
					DefaultExt = "xml",
					Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
					Multiselect = false,
					ShowReadOnly = true,
					Title = "Open chat session"
				};

				bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

				if (Result.HasValue && Result.Value)
				{
					XmlDocument Xml = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Xml.Load(Dialog.FileName);

					this.Load(Xml, Dialog.FileName);
				}
			}
			catch (Exception ex)
			{
				ex = Log.UnnestException(ex);
				MessageBox.Show(ex.Message, "Unable to load file.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Load(XmlDocument Xml, string FileName)
		{
			MarkdownDocument Markdown;
			XmlElement E;
			DateTime Timestamp;
			Color ForegroundColor;
			Color BackgroundColor;
			string From;

			XSL.Validate(FileName, Xml, chatRoot, chatNamespace, schema);

			this.ChatListView.Items.Clear();

			bool PrevMuc = this.muc;
			this.muc = XML.Attribute(Xml.DocumentElement, "muc", this.muc);

			if (this.muc != PrevMuc)
			{
				if (this.muc)
				{
					this.FromColumn.Width = this.ReceivedColumn.Width;
					this.ContentColumn.Width -= this.ReceivedColumn.Width;
				}
				else
				{
					this.ContentColumn.Width += this.FromColumn.Width;
					this.FromColumn.Width = 0;
				}
			}

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				E = N as XmlElement;
				if (E is null)
					continue;

				if (!Enum.TryParse<ChatItemType>(E.LocalName, out ChatItemType Type))
					continue;

				Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue);
				From = XML.Attribute(E, "from");

				switch (Type)
				{
					case ChatItemType.Received:
						ForegroundColor = Colors.Black;
						BackgroundColor = Colors.AliceBlue;
						break;

					case ChatItemType.Transmitted:
						ForegroundColor = Colors.Black;
						BackgroundColor = Colors.Honeydew;
						break;

					case ChatItemType.Event:
						ForegroundColor = EventFgColor;
						BackgroundColor = EventBgColor;
						break;

					default:
						continue;
				}

				try
				{
					Markdown = new MarkdownDocument(E.InnerText, GetMarkdownSettings());
				}
				catch (Exception)
				{
					Markdown = null;
				}

				ChatItem Item = new ChatItem(Type, Timestamp, E.InnerText, From, Markdown, ForegroundColor, BackgroundColor);
				ListViewItem ListViewItem = new ListViewItem()
				{
					Content = Item,
					Foreground = new SolidColorBrush(ForegroundColor),
					Background = new SolidColorBrush(BackgroundColor),
					Margin = new Thickness(0)
				};
				this.ChatListView.Items.Add(ListViewItem);
				this.ChatListView.ScrollIntoView(ListViewItem);
			}
		}

		private void Input_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Control) && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
				{
					this.Send_Click(sender, e);
					e.Handled = true;
				}
			}
		}

		private void ChatListView_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (this.ChatListView.SelectedItem is ListViewItem ListViewItem &&
				ListViewItem.Content is ChatItem Item)
			{
				this.Input.Text = Item.Message;
				e.Handled = true;
			}
			else if (e.OriginalSource is Run Run)
			{
				this.Input.Text = Run.Text;
				e.Handled = true;
			}
			else if (e.OriginalSource is TextBlock TextBlock)
			{
				this.Input.Text = TextBlock.Text;
				e.Handled = true;
			}
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			string Uri = ((Hyperlink)sender).NavigateUri.ToString();
			System.Diagnostics.Process.Start(Uri);
		}

		public void Event(string Message, string From)
		{
			MainWindow.UpdateGui(() =>
			{
				ChatItem Item = new ChatItem(ChatItemType.Event, DateTime.Now, Message, From, null, EventFgColor, EventBgColor);
				ListViewItem ListViewItem = new ListViewItem()
				{
					Content = Item,
					Foreground = new SolidColorBrush(EventFgColor),
					Background = new SolidColorBrush(EventBgColor),
					Margin = new Thickness(0)
				};
				this.ChatListView.Items.Add(ListViewItem);
				this.ChatListView.ScrollIntoView(ListViewItem);
			});
		}

		public static readonly Color EventFgColor = Color.FromRgb(32, 32, 32);
		public static readonly Color EventBgColor = Color.FromRgb(240, 240, 240);
	}
}
