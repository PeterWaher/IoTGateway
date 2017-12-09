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
using Microsoft.Win32;
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
			if (currentInstance == null)
				currentInstance = this;

			Types.Initialize(typeof(MainWindow).Assembly,
				typeof(Waher.Content.InternetContent).Assembly,
				typeof(Waher.Content.Images.ImageCodec).Assembly,
				typeof(Waher.Content.Markdown.MarkdownDocument).Assembly,
				typeof(Waher.Content.Xml.XML).Assembly,
				typeof(Waher.Content.Xsl.XSL).Assembly,
				typeof(Waher.Persistence.Database).Assembly,
				typeof(Waher.Persistence.Files.FilesProvider).Assembly,
				typeof(Waher.Script.Expression).Assembly,
				typeof(Waher.Script.Graphs.Graph).Assembly);

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
					databaseProvider = new FilesProvider(appDataFolder + "Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
					Database.Register(databaseProvider);

					this.LoadQuestions(null, null);	// To prepare indices, etc.
				}
				catch (Exception ex)
				{
					Dispatcher.Invoke(() => MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
				}
			});
			
			InitializeComponent();

			this.MainView.Load(this);
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
				if (Value != null && Value is int)
					this.Left = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value != null && Value is int)
					this.Top = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value != null && Value is int)
					this.Width = (int)Value;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value != null && Value is int)
					this.Height = (int)Value;

				Value = Registry.GetValue(registryKey, "ConnectionTreeWidth", (int)this.MainView.ConnectionTree.Width);
				if (Value != null && Value is int)
					this.MainView.ConnectionsGrid.ColumnDefinitions[0].Width = new GridLength((int)Value);

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value != null && Value is string)
					this.WindowState = (WindowState)Enum.Parse(typeof(WindowState), (string)Value);

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
				MessageBox.Show(this, ex.Message, "Unable to load values from registry.", MessageBoxButton.OK, MessageBoxImage.Error);
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
		}
		private void ConnectTo_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			this.MainView.ConnectTo_Executed(sender, e);
		}

		public ITabView CurrentTab
		{
			get
			{
				TabItem TabItem = this.Tabs.SelectedItem as TabItem;
				if (TabItem == null)
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

			if (Node == null)
			{
				this.AddButton.IsEnabled = false;
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
				this.AddButton.IsEnabled = Node.CanAddChildren;
				this.DeleteButton.IsEnabled = true;
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
				if (this.Tabs == null)
					return null;

				if (this.Tabs.SelectedIndex != 0)
					return null;

				if (this.MainView == null || this.MainView.ConnectionTree == null)
					return null;

				return this.MainView.SelectedNode;
			}
		}

		public static MainWindow FindWindow(FrameworkElement Element)
		{
			MainWindow MainWindow = Element as MainWindow;

			while (MainWindow == null && Element != null)
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
			if (Node == null || !Node.CanAddChildren)
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
			if (Node == null || !Node.CanRecycle)
				return;

			Node.Recycle(this);
		}

		private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null);
		}

		private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node == null)
				return;

			if (MessageBox.Show(this, "Are you sure you want to remove " + Node.Header + "?", "Are you sure?", MessageBoxButton.YesNo,
				MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				this.MainView.NodeRemoved(Node.Parent, Node);
			}
		}

		private void Sniff_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.IsSniffable);
		}

		private void Sniff_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node == null || !Node.IsSniffable)
				return;

			SnifferView View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as SnifferView;
				if (View == null)
					continue;

				if (View.Node == Node)
				{
					Tab.Focus();
					return;
				}
			}

			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			View = new SnifferView(Node);

			TabItem.Header = Node.Header;
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
					if (Content != null && Content is IDisposable)
						((IDisposable)Content).Dispose();
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
			if (Node == null || !Node.CanChat)
				return;

			ChatView View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as ChatView;
				if (View == null)
					continue;

				if (View.Node == Node)
				{
					Tab.Focus();
					return;
				}
			}

			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			View = new ChatView(Node);

			TabItem.Header = Node.Header;
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
			ChatView ChatView;

			string Message = e.Body;
			bool IsMarkdown = false;

			foreach (XmlNode N in e.Message.ChildNodes)
			{
				if (N.LocalName == "content" && N.NamespaceURI == "urn:xmpp:content")
				{
					string Type = XML.Attribute((XmlElement)N, "type");
					if (Type == "text/markdown")
					{
						IsMarkdown = true;

						Type = N.InnerText;
						if (!string.IsNullOrEmpty(Type))
							Message = Type;

						break;
					}
				}
			}

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				ChatView = TabItem.Content as ChatView;
				if (ChatView == null)
					continue;

				XmppContact XmppContact = ChatView.Node as XmppContact;
				if (XmppContact == null)
					continue;

				if (XmppContact.BareJID != e.FromBareJID)
					continue;

				XmppAccountNode XmppAccountNode = XmppContact.XmppAccountNode;
				if (XmppAccountNode == null)
					continue;

				if (XmppAccountNode.BareJID != XmppClient.GetBareJID(e.To))
					continue;

				ChatView.ChatMessageReceived(Message, IsMarkdown, this);
				return;
			}

			foreach (TreeNode Node in this.MainView.ConnectionTree.Items)
			{
				XmppAccountNode XmppAccountNode = Node as XmppAccountNode;
				if (XmppAccountNode == null)
					continue;

				if (XmppAccountNode.BareJID != XmppClient.GetBareJID(e.To))
					continue;

				if (XmppAccountNode.TryGetChild(e.FromBareJID, out TreeNode ContactNode))
				{
					TabItem TabItem2 = new TabItem();
					this.Tabs.Items.Add(TabItem2);

					ChatView = new ChatView(ContactNode);

					TabItem2.Header = e.FromBareJID;
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
			if (Node == null || !Node.CanReadSensorData)
				return;

			SensorDataClientRequest Request = Node.StartSensorDataMomentaryReadout();
			if (Request == null)
				return;

			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, false);

			TabItem.Header = Node.Header;
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
			if (Node == null || !Node.CanReadSensorData)
				return;

			SensorDataClientRequest Request = Node.StartSensorDataFullReadout();
			if (Request == null)
				return;

			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, false);

			TabItem.Header = Node.Header;
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
			if (Node == null || !Node.CanSubscribeToSensorData)
				return;

			SensorDataClientRequest Request;

			if (Node.CanReadSensorData)
				Request = Node.StartSensorDataMomentaryReadout();
			else
				Request = Node.SubscribeSensorDataMomentaryReadout(new FieldSubscriptionRule[0]);

			if (Request == null)
				return;

			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			SensorDataView View = new SensorDataView(Request, Node, true);

			TabItem.Header = Node.Header;
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
			if (Node == null || !Node.CanConfigure)
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
				MessageBox.Show(this, e.ErrorText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				MessageBox.Show(this, P.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		private void Search_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			e.CanExecute = (Node != null && Node.CanSearch);
		}

		private void Search_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TreeNode Node = this.SelectedNode;
			if (Node == null || !Node.CanSearch)
				return;

			try
			{
				Node.Search();
			}
			catch (Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Script_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Script_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			TabItem TabItem = new TabItem();
			this.Tabs.Items.Add(TabItem);

			ScriptView ScriptView = new ScriptView();

			TabItem.Header = "Script";
			TabItem.Content = ScriptView;

			this.Tabs.SelectedItem = TabItem;
		}

		internal Task LoadQuestions(XmppAccountNode Owner, ProvisioningClient ProvisioningClient)
		{
			return this.NewQuestion(Owner, ProvisioningClient, null);
		}

		internal Task NewQuestion(XmppAccountNode Owner, Question Question, ProvisioningClient ProvisioningClient)
		{
			return this.NewQuestion(Owner, ProvisioningClient, Question);
		}

		private async Task NewQuestion(XmppAccountNode Owner, ProvisioningClient ProvisioningClient, Question Question)
		{
			QuestionView QuestionView;
			bool DoSearch;

			if (Question == null || Owner == null)
			{
				QuestionView = null;
				DoSearch = true;
			}
			else
				QuestionView = this.OpenQuestionTab(Owner, ProvisioningClient, out DoSearch);

			if (DoSearch)
			{
				bool Found = false;

				foreach (Question Question2 in await Database.Find<Question>(new FilterAnd(new FilterFieldEqualTo("OwnerJID", Owner?.BareJID), 
					new FilterFieldEqualTo("ProvisioningJID", ProvisioningClient?.ProvisioningServerAddress)), "Created"))
				{
					if (QuestionView == null)
						QuestionView = this.OpenQuestionTab(Owner, ProvisioningClient, out DoSearch);

					QuestionView.NewQuestion(Question2);

					if (Question != null)
						Found |= Question2.ObjectId == Question.ObjectId;
				}

				if (Found)
					return;
			}

			if (Question != null)
				QuestionView.NewQuestion(Question);
		}

		private QuestionView OpenQuestionTab(XmppAccountNode Owner, ProvisioningClient ProvisioningClient, out bool DoSearch)
		{
			QuestionView QuestionView = null;

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				QuestionView = TabItem.Content as QuestionView;
				if (QuestionView != null &&
					QuestionView.Owner == Owner && 
					QuestionView.ProvisioningJid == ProvisioningClient.ProvisioningServerAddress)
				{
					DoSearch = false;
					return QuestionView;
				}
			}

			TabItem TabItem2 = new TabItem();
			this.Tabs.Items.Add(TabItem2);

			QuestionView = new QuestionView(Owner, ProvisioningClient);

			TabItem2.Header = "Questions (" + Owner.BareJID + ")";
			TabItem2.Content = QuestionView;

			DoSearch = true;

			return QuestionView;
		}

	}
}
