using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Waher.Content.Markdown;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Inventory;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Filters;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Questions;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Controls.Sniffers;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model;

namespace Waher.Client.WPF
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const string WindowTitle = "Simple XMPP IoT Client";

		public static RoutedUICommand Add = new RoutedUICommand("Add", "Add", typeof(MainWindow));
		public static RoutedUICommand Edit = new RoutedUICommand("Edit", "Edit", typeof(MainWindow));
		public static RoutedUICommand Delete = new RoutedUICommand("Delete", "Delete", typeof(MainWindow));
		public static RoutedUICommand ConnectTo = new RoutedUICommand("Connect To", "ConnectTo", typeof(MainWindow));
		public static RoutedUICommand Refresh = new RoutedUICommand("Refresh", "Refresh", typeof(MainWindow));
		public static RoutedUICommand Sniff = new RoutedUICommand("Sniff", "Sniff", typeof(MainWindow));
		public static RoutedUICommand CloseTab = new RoutedUICommand("Close Tab", "CloseTab", typeof(MainWindow));
		public static RoutedUICommand Chat = new RoutedUICommand("Chat", "Chat", typeof(MainWindow));
		public static RoutedUICommand ReadMomentary = new RoutedUICommand("Read Momentary", "ReadMomentary", typeof(MainWindow));
		public static RoutedUICommand ReadDetailed = new RoutedUICommand("Read Detailed", "ReadDetailed", typeof(MainWindow));
		public static RoutedUICommand SubscribeToMomentary = new RoutedUICommand("Subscribe to Momentary", "SubscribeToMomentary", typeof(MainWindow));
		public static RoutedUICommand Configure = new RoutedUICommand("Configure", "Configure", typeof(MainWindow));
		public static RoutedUICommand Search = new RoutedUICommand("Search", "Search", typeof(MainWindow));
		public static RoutedUICommand Script = new RoutedUICommand("Script", "Script", typeof(MainWindow));

		internal static MainWindow currentInstance = null;
		private static string appDataFolder = null;
		private static FilesProvider databaseProvider = null;

		public MainWindow()
		{
			if (currentInstance is null)
				currentInstance = this;

			Types.Initialize(typeof(MainWindow).Assembly,
				typeof(Content.InternetContent).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(Content.Markdown.MarkdownDocument).Assembly,
				typeof(XML).Assembly,
				typeof(Content.Xsl.XSL).Assembly,
				typeof(SensorData).Assembly,
				typeof(Networking.XMPP.BOSH.HttpBinding).Assembly,
				typeof(Networking.XMPP.P2P.EndpointSecurity).Assembly,
				typeof(Networking.XMPP.Provisioning.ProvisioningClient).Assembly,
				typeof(Networking.XMPP.WebSocket.WebSocketBinding).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Script.Content.Functions.Encoding.Decode).Assembly,
				typeof(Script.Graphs.Graph).Assembly,
				typeof(Script.Fractals.FractalGraph).Assembly,
				typeof(Script.Persistence.Functions.FindObjects).Assembly,
				typeof(Script.Statistics.Functions.Beta).Assembly,
				typeof(Security.IUser).Assembly,
				typeof(Security.EllipticCurves.PrimeFieldCurve).Assembly);

			appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
				appDataFolder += Path.DirectorySeparatorChar;

			appDataFolder += "IoT Client" + Path.DirectorySeparatorChar;

			if (!Directory.Exists(appDataFolder))
				Directory.CreateDirectory(appDataFolder);

			Task.Run(() =>
			{
				try
				{
					databaseProvider = new FilesProvider(appDataFolder + "Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 3600000);
					databaseProvider.RepairIfInproperShutdown(appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "DbStatXmlToHtml.xslt").Wait();
					databaseProvider.Start().Wait();
					Database.Register(databaseProvider);

					Database.Find<Question>(new FilterAnd(new FilterFieldEqualTo("OwnerJID", string.Empty),
						new FilterFieldEqualTo("ProvisioningJID", string.Empty)));  // To prepare indices, etc.

					ChatView.InitEmojis();
				}
				catch (Exception ex)
				{
					ex = Log.UnnestException(ex);
					ErrorBox(ex.Message);
				}
			});

			InitializeComponent();

			this.MainView.Load(this);
		}

		public static string AppDataFolder
		{
			get { return appDataFolder; }
		}

		internal static readonly string registryKey = Registry.CurrentUser + @"\Software\Waher Data AB\Waher.Client.WPF";

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			object Value;

			try
			{
				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				Value = Registry.GetValue(registryKey, "WindowLeft", (int)this.Left);
				if (Value != null && Value is int WindowLeft)
					this.Left = WindowLeft;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value != null && Value is int WindowTop)
					this.Top = WindowTop;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value != null && Value is int WindowWidth && WindowWidth > 0)
					this.Width = WindowWidth;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value != null && Value is int WindowHeight && WindowHeight > 0)
					this.Height = WindowHeight;

				Value = Registry.GetValue(registryKey, "ConnectionTreeWidth", (int)this.MainView.ConnectionTree.Width);
				if (!(Value is int ConnectionTreeWidth) || ConnectionTreeWidth <= 0)
					ConnectionTreeWidth = 150;

				this.MainView.ConnectionsGrid.ColumnDefinitions[0].Width = new GridLength(ConnectionTreeWidth);

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value != null && Value is string s && Enum.TryParse<WindowState>(s, out WindowState WindowState))
					this.WindowState = WindowState;

				Value = Registry.GetValue(registryKey, "FileName", string.Empty);
				if (Value != null && Value is string)
				{
					this.MainView.FileName = (string)Value;
					if (!string.IsNullOrEmpty(this.MainView.FileName))
						this.MainView.Load(this.MainView.FileName);
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(this, ex.Message, "Unable to load values from registry.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.MainView.CheckSaved())
			{
				e.Cancel = true;
				return;
			}

			Registry.SetValue(registryKey, "WindowLeft", (int)this.Left, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowTop", (int)this.Top, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowWidth", (int)this.Width, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowHeight", (int)this.Height, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "ConnectionTreeWidth", (int)this.MainView.ConnectionsGrid.ColumnDefinitions[0].Width.Value, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowState", this.WindowState.ToString(), RegistryValueKind.String);
			Registry.SetValue(registryKey, "FileName", this.MainView.FileName, RegistryValueKind.String);

			Log.Terminate();

			databaseProvider.Stop();
			databaseProvider.Flush();
		}
		private void ConnectTo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.MainView.ConnectTo_Executed(sender, e);
		}

		public ITabView CurrentTab
		{
			get
			{
				if (!(this.Tabs.SelectedItem is TabItem TabItem))
					return null;
				else
					return TabItem.Content as ITabView;
			}
		}

		private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ITabView TabView = this.CurrentTab;
			if (TabView != null)
				TabView.SaveButton_Click(sender, e);
		}

		private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ITabView TabView = this.CurrentTab;
			if (TabView != null)
				TabView.SaveAsButton_Click(sender, e);
		}

		private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ITabView TabView = this.CurrentTab;
			if (TabView != null)
				TabView.OpenButton_Click(sender, e);
		}

		private void New_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			ITabView TabView = this.CurrentTab;
			if (TabView != null)
				TabView.NewButton_Click(sender, e);
		}

		internal void SelectionChanged()
		{
			TreeNode Node = this.SelectedNode;

			if (Node is null)
			{
				this.AddButton.IsEnabled = false;
				this.EditButton.IsEnabled = false;
				this.DeleteButton.IsEnabled = false;
				this.RefreshButton.IsEnabled = false;
				this.SniffButton.IsEnabled = false;
				this.ChatButton.IsEnabled = false;
				this.ReadMomentaryButton.IsEnabled = false;
				this.ReadDetailedButton.IsEnabled = false;
				this.ConfigureButton.IsEnabled = false;
				this.SubscribeMomentaryButton.IsEnabled = false;
				this.SearchButton.IsEnabled = false;
			}
			else
			{
				Node.SelectionChanged();

				this.AddButton.IsEnabled = Node.CanAddChildren;
				this.EditButton.IsEnabled = Node.CanEdit;
				this.DeleteButton.IsEnabled = Node.CanDelete;
				this.RefreshButton.IsEnabled = Node.CanRecycle;
				this.SniffButton.IsEnabled = Node.IsSniffable;
				this.ChatButton.IsEnabled = Node.CanChat;
				this.ReadMomentaryButton.IsEnabled = Node.CanReadSensorData;
				this.ReadDetailedButton.IsEnabled = Node.CanReadSensorData;
				this.ConfigureButton.IsEnabled = Node.CanConfigure;
				this.SubscribeMomentaryButton.IsEnabled = Node.CanSubscribeToSensorData;
				this.SearchButton.IsEnabled = Node.CanSearch;
			}
		}

		private TreeNode SelectedNode
		{
			get
			{
				if (this.Tabs is null)
					return null;

				if (this.Tabs.SelectedIndex != 0)
					return null;

				if (this.MainView is null || this.MainView.ConnectionTree is null)
					return null;

				return this.MainView.SelectedNode;
			}
		}

		public static MainWindow FindWindow(FrameworkElement Element)
		{
			MainWindow MainWindow = Element as MainWindow;

			while (MainWindow is null && Element != null)
			{
				Element = Element.Parent as FrameworkElement;
				MainWindow = Element as MainWindow;
			}

			return MainWindow;
		}

		private void Add_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanAddChildren);
		}

		private void Add_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanAddChildren)
				return;

			Node.Add();
		}

		private void Refresh_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanRecycle);
		}

		private void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanRecycle)
				return;

			Node.Recycle(this);
		}

		private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanDelete);
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanDelete)
				return;

			if (System.Windows.MessageBox.Show(this, "Are you sure you want to remove " + Node.Header + "?", "Are you sure?", MessageBoxButton.YesNo,
				MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				try
				{
					Node.Delete(Node.Parent, (sender2, e2) => this.MainView.NodeRemoved(Node.Parent, Node));
				}
				catch (Exception ex)
				{
					ErrorBox(ex.Message);
				}
			}
		}

		private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanEdit);
		}

		private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanEdit)
				return;

			Node.Edit();
		}

		private void Sniff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.IsSniffable);
		}

		private void Sniff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.IsSniffable)
				return;

			SnifferView View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as SnifferView;
				if (View is null)
					continue;

				if (View.Node == Node)
				{
					Tab.Focus();
					return;
				}
			}

			TabItem TabItem = MainWindow.NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			View = new SnifferView(Node);
			TabItem.Content = View;

			View.Sniffer = new TabSniffer(TabItem, View);
			Node.AddSniffer(View.Sniffer);

			this.Tabs.SelectedItem = TabItem;
		}

		private void CloseTab_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.Tabs.SelectedIndex > 0;
		}

		internal void CloseTab_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			int i = this.Tabs.SelectedIndex;
			if (i > 0)
			{
				if (this.Tabs.Items[i] is TabItem TabItem)
				{
					object Content = TabItem.Content;
					if (Content != null && Content is IDisposable Disposable)
						Disposable.Dispose();
				}

				this.Tabs.Items.RemoveAt(i);
			}
		}

		private void Chat_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanChat);
		}

		private void Chat_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanChat)
				return;

			ChatView View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as ChatView;
				if (View is null)
					continue;

				if (View.Node == Node)
				{
					Tab.Focus();
					return;
				}
			}

			TabItem TabItem = MainWindow.NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			View = new ChatView(Node);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;

			Thread T = new Thread(this.FocusChatInput);
			T.Start(View);
		}

		private void FocusChatInput(object P)
		{
			Thread.Sleep(50);
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.FocusChatInput2), P);
		}

		private void FocusChatInput2(object P)
		{
			ChatView View = (ChatView)P;
			View.Input.Focus();
		}

		public void OnChatMessage(object Sender, MessageEventArgs e)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.ChatMessageReceived), e);
		}

		public void OnStateChange(object Sender, XmppState State)
		{
			this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.UpdateStateStatus), State);
		}

		private void UpdateStateStatus(object State)
		{
			try
			{
				SortedDictionary<int, int> ByState = new SortedDictionary<int, int>(reverseInt32);
				int i = 0;
				int c = 0;

				foreach (TreeNode N in this.MainView.Connections.RootNodes)
				{
					if (N is XmppAccountNode Account)
					{
						i = (int)Account.Client.State;

						if (ByState.TryGetValue(i, out int j))
							j++;
						else
							j = 1;

						ByState[i] = j;
						c++;
					}
				}

				if (c == 0)
					this.MainView.ConnectionStatus.Content = string.Empty;
				else if (c == 1)
					this.MainView.ConnectionStatus.Content = StateToString((XmppState)i);
				else
				{
					StringBuilder sb = new StringBuilder();
					bool First = true;

					foreach (KeyValuePair<int, int> P in ByState)
					{
						if (First)
							First = false;
						else
							sb.Append(", ");

						sb.Append(P.Value.ToString());
						sb.Append(' ');
						sb.Append(StateToString((XmppState)P.Key));
					}

					this.MainView.ConnectionStatus.Content = sb.ToString();
				}
			}
			catch (Exception ex)
			{
				Log.Critical(ex);
			}
		}

		private static string StateToString(XmppState State)
		{
			switch (State)
			{
				case XmppState.Offline: return "Offline";
				case XmppState.Connecting: return "Connecting";
				case XmppState.StreamNegotiation: return "Negotiating stream";
				case XmppState.StreamOpened: return "Opened stream";
				case XmppState.StartingEncryption: return "Starting encryption";
				case XmppState.Authenticating: return "Authenticating";
				case XmppState.Registering: return "Registering";
				case XmppState.Binding: return "Binding";
				case XmppState.RequestingSession: return "Requesting session";
				case XmppState.FetchingRoster: return "Fetching roster";
				case XmppState.SettingPresence: return "Setting presence";
				case XmppState.Connected: return "Connected";
				case XmppState.Error: return "In error";
				default: return "Unknown";
			}
		}

		private class ReverseInt32 : IComparer<int>
		{
			public int Compare(int x, int y)
			{
				return y - x;
			}
		}

		private static readonly ReverseInt32 reverseInt32 = new ReverseInt32();

		private void ChatMessageReceived(object P)
		{
			MessageEventArgs e = (MessageEventArgs)P;

			string Message = e.Body;
			bool IsMarkdown = false;

			foreach (XmlNode N in e.Message.ChildNodes)
			{
				if (N.LocalName == "content" && N.NamespaceURI == "urn:xmpp:content")
				{
					string Type = XML.Attribute((XmlElement)N, "type");
					if (Type == MarkdownCodec.ContentType)
					{
						IsMarkdown = true;

						Type = N.InnerText;
						if (!string.IsNullOrEmpty(Type))
							Message = Type;

						break;
					}
				}
			}

			this.ChatMessage(e.FromBareJID, XmppClient.GetBareJID(e.To), Message, IsMarkdown);
		}

		public void ChatMessage(string FromBareJid, string ToBareJid, string Message, bool IsMarkdown)
		{
			XmppAccountNode XmppAccountNode;
			ChatView ChatView;

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				ChatView = TabItem.Content as ChatView;
				if (ChatView is null)
					continue;

				if (ChatView.Node is XmppContact XmppContact)
				{
					if (XmppContact.BareJID != FromBareJid)
						continue;

					XmppAccountNode = XmppContact.XmppAccountNode;
				}
				else if (ChatView.Node is Model.XmppComponent XmppComponent)
				{
					if (XmppComponent.JID != FromBareJid)
						continue;

					XmppAccountNode = XmppComponent.Account;
				}
				else
					continue;

				if (XmppAccountNode is null)
					continue;

				if (XmppAccountNode.BareJID != ToBareJid)
					continue;

				ChatView.ChatMessageReceived(Message, IsMarkdown, this);
				return;
			}

			foreach (TreeNode Node in this.MainView.ConnectionTree.Items)
			{
				if (!(Node is XmppAccountNode XmppAccountNode2))
					continue;

				if (XmppAccountNode2.BareJID != ToBareJid)
					continue;

				if (XmppAccountNode2.TryGetChild(FromBareJid, out TreeNode ContactNode))
				{
					TabItem TabItem2 = MainWindow.NewTab(FromBareJid);
					this.Tabs.Items.Add(TabItem2);

					ChatView = new ChatView(ContactNode);
					TabItem2.Content = ChatView;

					ChatView.ChatMessageReceived(Message, IsMarkdown, this);
					return;
				}
			}
		}

		private void Tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (this.Tabs.SelectedItem is TabItem Item)
			{
				if (Item.Content is ChatView View)
				{
					Thread T = new Thread(this.FocusChatInput);
					T.Start(View);
				}
			}
		}

		private void ReadMomentary_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanReadSensorData);
		}

		private void ReadMomentary_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanReadSensorData)
				return;

			SensorDataClientRequest Request = Node.StartSensorDataMomentaryReadout();
			if (Request is null)
				return;

			TabItem TabItem = MainWindow.NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, false);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;
		}

		private void ReadDetailed_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanReadSensorData);
		}

		private void ReadDetailed_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanReadSensorData)
				return;

			SensorDataClientRequest Request = Node.StartSensorDataFullReadout();
			if (Request is null)
				return;

			TabItem TabItem = MainWindow.NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, false);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;
		}

		private void SubscribeToMomentary_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanSubscribeToSensorData);
		}

		private void SubscribeToMomentary_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanSubscribeToSensorData)
				return;

			SensorDataClientRequest Request;

			if (Node.CanReadSensorData)
				Request = Node.StartSensorDataMomentaryReadout();
			else
				Request = Node.SubscribeSensorDataMomentaryReadout(new FieldSubscriptionRule[0]);

			if (Request is null)
				return;

			TabItem TabItem = MainWindow.NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, true);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;
		}

		private void Configure_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanConfigure);
		}

		private void Configure_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanConfigure)
				return;

			Mouse.OverrideCursor = Cursors.Wait;
			Node.GetConfigurationForm((Sender, e2) =>
			{
				if (e2.Ok && e2.Form != null)
					this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.ShowForm), e2.Form);
				else
					this.Dispatcher.BeginInvoke(new ParameterizedThreadStart(this.ShowError), e2);
			}, null);
		}

		private void ShowForm(object P)
		{
			Mouse.OverrideCursor = null;

			DataForm Form = (DataForm)P;

			/*string Xml = File.ReadAllText("../../../../Networking/Waher.Networking.XMPP.Test/Data/TestForm.xml");
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Xml);
			Form = new DataForm(Form.Client, Doc.DocumentElement, null, null, Form.From, Form.To);*/

			ParameterDialog Dialog = new ParameterDialog(Form)
			{
				Owner = this
			};

			Dialog.ShowDialog();
		}

		private void ShowError(object P)
		{
			Mouse.OverrideCursor = null;

			if (P is IqResultEventArgs e)
				System.Windows.MessageBox.Show(this, e.ErrorText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				System.Windows.MessageBox.Show(this, P.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void Search_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanSearch);
		}

		private void Search_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node is null || !Node.CanSearch)
				return;

			try
			{
				Node.Search();
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Script_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Script_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TabItem TabItem = MainWindow.NewTab("Script");
			this.Tabs.Items.Add(TabItem);

			ScriptView ScriptView = new ScriptView();
			TabItem.Content = ScriptView;

			this.Tabs.SelectedItem = TabItem;
		}

		internal void NewQuestion(XmppAccountNode Owner, ProvisioningClient ProvisioningClient, Question Question)
		{
			QuestionView QuestionView = this.FindQuestionTab(Owner, ProvisioningClient);

			if (QuestionView != null && Question != null)
			{
				QuestionView.NewQuestion(Question);
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					LinkedList<Question> Questions = new LinkedList<Question>();
					bool Found = Question is null;

					foreach (Question Question2 in await Database.Find<Question>(new FilterAnd(new FilterFieldEqualTo("OwnerJID", Owner?.BareJID),
						new FilterFieldEqualTo("ProvisioningJID", ProvisioningClient?.ProvisioningServerAddress)), "Created"))
					{
						Questions.AddLast(Question2);

						if (!Found)
							Found = Question2.ObjectId == Question.ObjectId;
					}

					if (!Found)
						Questions.AddLast(Question);

					if (Questions.First != null)
					{
						DispatcherOperation Op = MainWindow.currentInstance.Dispatcher.BeginInvoke(new ThreadStart(() =>
						{
							if (QuestionView is null)
								QuestionView = this.CreateQuestionTab(Owner, ProvisioningClient);

							foreach (Question Question2 in Questions)
								QuestionView.NewQuestion(Question2);
						}));
					}
				}
				catch (Exception ex)
				{
					Log.Critical(ex);
				}
			});
		}

		private QuestionView FindQuestionTab(XmppAccountNode Owner, ProvisioningClient ProvisioningClient)
		{
			QuestionView QuestionView = null;

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				QuestionView = TabItem.Content as QuestionView;
				if (QuestionView != null &&
					QuestionView.Owner == Owner &&
					QuestionView.ProvisioningJid == ProvisioningClient.ProvisioningServerAddress)
				{
					return QuestionView;
				}
			}

			return null;
		}

		private QuestionView CreateQuestionTab(XmppAccountNode Owner, ProvisioningClient ProvisioningClient)
		{
			TabItem TabItem = MainWindow.NewTab("Questions (" + Owner.BareJID + ")");
			this.Tabs.Items.Add(TabItem);

			QuestionView QuestionView = new QuestionView(Owner, ProvisioningClient);
			TabItem.Content = QuestionView;

			return QuestionView;
		}

		internal static TabItem NewTab(string HeaderText)
		{
			return NewTab(HeaderText, out TextBlock HeaderLabel);
		}

		internal static TabItem NewTab(string HeaderText, out TextBlock HeaderLabel)
		{
			StackPanel Header = new StackPanel()
			{
				Orientation = Orientation.Horizontal
			};

			Image CloseImage = new Image()
			{
				Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative)),
				Width = 16,
				Height = 16,
				ToolTip = "Close tab"
			};

			HeaderLabel = new TextBlock()
			{
				Text = HeaderText,
				Margin = new Thickness(0, 0, 5, 0)
			};

			Header.Children.Add(HeaderLabel);
			Header.Children.Add(CloseImage);

			TabItem Result = new TabItem()
			{
				Header = Header
			};

			CloseImage.MouseLeftButtonDown += CloseImage_MouseLeftButtonDown;
			CloseImage.MouseEnter += CloseImage_MouseEnter;
			CloseImage.MouseLeave += CloseImage_MouseLeave;
			CloseImage.Tag = Result;

			return Result;
		}

		private static void CloseImage_MouseLeave(object sender, MouseEventArgs e)
		{
			if (sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative));
		}

		private static void CloseImage_MouseEnter(object sender, MouseEventArgs e)
		{
			if (sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon.png", UriKind.Relative));
		}

		private static void CloseImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if ((sender as Image)?.Tag is TabItem Item)
			{
				MainWindow.currentInstance?.Tabs?.Items.Remove(Item);
				if (Item.Content != null && Item.Content is IDisposable Disposable)
					Disposable.Dispose();
			}
		}

		public static void ErrorBox(string ErrorMessage)
		{
			MessageBox(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void SuccessBox(string ErrorMessage)
		{
			MessageBox(ErrorMessage, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void MessageBox(string Text, string Caption, MessageBoxButton Button, MessageBoxImage Icon)
		{
			currentInstance.Dispatcher.BeginInvoke(new ThreadStart(() =>
			{
				Mouse.OverrideCursor = null;
				System.Windows.MessageBox.Show(currentInstance, Text, Caption, Button, Icon);
			}));
		}

		public static void MouseDefault()
		{
			MainWindow.currentInstance.Dispatcher.BeginInvoke(new ThreadStart(() => Mouse.OverrideCursor = null));
		}
	}
}
