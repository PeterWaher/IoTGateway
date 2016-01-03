using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Waher.Networking;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF.Controls
{
	/// <summary>
	/// Interaction logic for ChatView.xaml
	/// </summary>
	public partial class ChatView : UserControl, ITabView
	{
		private TreeNode node;

		public ChatView(TreeNode Node)
		{
			this.node = Node;

			InitializeComponent();
		}

		public void Dispose()
		{
		}

		public TreeNode Node
		{
			get { return this.node; }
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			GridView GridView = this.ChatListView.View as GridView;
			if (GridView != null)
			{
				GridView.Columns[1].Width = this.ActualWidth - GridView.Columns[0].ActualWidth - GridView.Columns[2].ActualWidth -
					SystemParameters.VerticalScrollBarWidth - 8;
			}

			this.Input.Width = this.ActualWidth - EnterTextLabel.ActualWidth - this.SendButton.ActualWidth - 5;
		}

		private void Send_Click(object sender, RoutedEventArgs e)
		{
			string Msg = this.Input.Text;
			this.Input.Text = string.Empty;
			this.Input.Focus();

			this.ChatListView.Items.Add(new ChatItem(ChatItemType.Transmitted, Msg, Colors.Black, Colors.Honeydew));

			this.node.SendChatMessage(Msg);
		}

		public void ChatMessageReceived(string Message)
		{
			this.ChatListView.Items.Add(new ChatItem(ChatItemType.Received, Message, Colors.Black, Colors.AliceBlue));
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
			SaveFileDialog Dialog = new SaveFileDialog();
			Dialog.AddExtension = true;
			Dialog.CheckPathExists = true;
			Dialog.CreatePrompt = false;
			Dialog.DefaultExt = "html";
			Dialog.Filter = "XML Files (*.xml)|*.xml|HTML Files (*.html;*.htm)|*.html;*.htm|All Files (*.*)|*.*";
			Dialog.Title = "Save chat session";

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

						string Html = XML.Transform(Xml.ToString(), chatToHtml);

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

		private static readonly XslCompiledTransform chatToHtml = XML.LoadTransform("Waher.Client.WPF.Transforms.ChatToHTML.xslt", typeof(ChatView).Assembly);
		private static readonly XmlSchema schema = XML.LoadSchema("Waher.Client.WPF.Schema.Chat.xsd", typeof(ChatView).Assembly);
		private const string chatNamespace = "http://waher.se/Chat.xsd";
		private const string chatRoot = "Chat";

		private void SaveAsXml(XmlWriter w)
		{
			w.WriteStartElement(chatRoot, chatNamespace);

			foreach (ChatItem Item in this.ChatListView.Items)
			{
				w.WriteStartElement(Item.Type.ToString());
				w.WriteAttributeString("timestamp", XML.Encode(Item.Timestamp));
				w.WriteValue(Item.Message);
				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.Flush();
		}

		public void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				OpenFileDialog Dialog = new OpenFileDialog();
				Dialog.AddExtension = true;
				Dialog.CheckFileExists = true;
				Dialog.CheckPathExists = true;
				Dialog.DefaultExt = "xml";
				Dialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
				Dialog.Multiselect = false;
				Dialog.ShowReadOnly = true;
				Dialog.Title = "Open chat session";

				bool? Result = Dialog.ShowDialog(MainWindow.FindWindow(this));

				if (Result.HasValue && Result.Value)
				{
					XmlDocument Xml = new XmlDocument();
					Xml.Load(Dialog.FileName);

					this.Load(Xml, Dialog.FileName);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Unable to load file.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		public void Load(XmlDocument Xml, string FileName)
		{
			XmlElement E;
			DateTime Timestamp;
			ChatItemType Type;
			Color ForegroundColor;
			Color BackgroundColor;

			XML.Validate(FileName, Xml, chatRoot, chatNamespace, schema);

			this.ChatListView.Items.Clear();

			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				E = N as XmlElement;
				if (E == null)
					continue;

				if (!Enum.TryParse<ChatItemType>(E.LocalName, out Type))
					continue;

				Timestamp = XML.Attribute(E, "timestamp", DateTime.MinValue);

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

					default:
						continue;
				}

				this.ChatListView.Items.Add(new ChatItem(Type, E.InnerText, ForegroundColor, BackgroundColor));
			}
		}

	}
}
