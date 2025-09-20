﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Chat;
using Waher.Client.WPF.Controls.Questions;
using Waher.Client.WPF.Controls.Sniffers;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Model;
using Waher.Client.WPF.Model.Muc;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Consolidation;
using Waher.Content.Markdown.Contracts;
using Waher.Content.Markdown.GraphViz;
using Waher.Content.Markdown.Latex;
using Waher.Content.Markdown.Layout2D;
using Waher.Content.Markdown.PlantUml;
using Waher.Content.Markdown.Wpf;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Layout.Layout2D;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Events;
using Waher.Networking.XMPP.MUC;
using Waher.Networking.XMPP.Provisioning;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.Sensor;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Filters;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.Language;
using Waher.Runtime.Settings;
using Waher.Runtime.Timing;

namespace Waher.Client.WPF
{
	/// <summary>
	/// Delegate for GUI update methods with parameter.
	/// </summary>
	public delegate Task GuiDelegate();

	/// <summary>
	/// Delegate for GUI update methods with parameter.
	/// </summary>
	/// <param name="Parameter">Parameter</param>
	public delegate Task GuiDelegateWithParameter(object Parameter);

	/// <summary>
	/// Interaction logic for xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public const string WindowTitle = "Simple XMPP IoT Client";

		public static readonly RoutedUICommand Add = new("Add", "Add", typeof(MainWindow));
		public static readonly RoutedUICommand Edit = new("Edit", "Edit", typeof(MainWindow));
		public static readonly RoutedUICommand Delete = new("Delete", "Delete", typeof(MainWindow));
		public static readonly RoutedUICommand Copy = new("Copy", "Copy", typeof(MainWindow));
		public static readonly RoutedUICommand Paste = new("Paste", "Paste", typeof(MainWindow));
		public static readonly RoutedUICommand ConnectTo = new("Connect To", "ConnectTo", typeof(MainWindow));
		public static readonly RoutedUICommand Refresh = new("Refresh", "Refresh", typeof(MainWindow));
		public static readonly RoutedUICommand Sniff = new("Sniff", "Sniff", typeof(MainWindow));
		public static readonly RoutedUICommand EventLog = new("EventLog", "EventLog", typeof(MainWindow));
		public static readonly RoutedUICommand CloseTab = new("Close Tab", "CloseTab", typeof(MainWindow));
		public static readonly RoutedUICommand Chat = new("Chat", "Chat", typeof(MainWindow));
		public static readonly RoutedUICommand ReadMomentary = new("Read Momentary", "ReadMomentary", typeof(MainWindow));
		public static readonly RoutedUICommand ReadDetailed = new("Read Detailed", "ReadDetailed", typeof(MainWindow));
		public static readonly RoutedUICommand SubscribeToMomentary = new("Subscribe to Momentary", "SubscribeToMomentary", typeof(MainWindow));
		public static readonly RoutedUICommand Configure = new("Configure", "Configure", typeof(MainWindow));
		public static readonly RoutedUICommand Search = new("Search", "Search", typeof(MainWindow));
		public static readonly RoutedUICommand Script = new("Script", "Script", typeof(MainWindow));

		internal static MainWindow? currentInstance = null;
		private static string? appDataFolder = null;
		private static FilesProvider? databaseProvider = null;
		private static Scheduler? scheduler = new();
		private static readonly LinkedList<GuiUpdateTask> guiUpdateQueue = new();

		public MainWindow()
		{
			currentInstance ??= this;

			Types.Initialize(typeof(MainWindow).Assembly,
				typeof(Content.InternetContent).Assembly,
				typeof(Content.Images.ImageCodec).Assembly,
				typeof(Content.Rss.RssDocument).Assembly,
				typeof(MarkdownDocument).Assembly,
				typeof(Consolidator).Assembly,
				typeof(ContractsRenderer).Assembly,
				typeof(GraphViz).Assembly,
				typeof(XmlLayout).Assembly,
				typeof(LatexRenderer).Assembly,
				typeof(PlantUml).Assembly,
				typeof(WpfXamlRenderer).Assembly,
				typeof(Layout2DDocument).Assembly,
				typeof(XML).Assembly,
				typeof(Content.Xsl.XSL).Assembly,
				typeof(SensorData).Assembly,
				typeof(Networking.XMPP.BOSH.HttpBinding).Assembly,
				typeof(Networking.XMPP.P2P.EndpointSecurity).Assembly,
				typeof(ProvisioningClient).Assembly,
				typeof(Networking.XMPP.WebSocket.WebSocketBinding).Assembly,
				typeof(Log).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(RuntimeSettings).Assembly,
				typeof(Persistence.FullTextSearch.Search).Assembly,
				typeof(Script.Expression).Assembly,
				typeof(Script.Content.Functions.Encoding.Decode).Assembly,
				typeof(Script.Cryptography.Functions.RandomBytes).Assembly,
				typeof(Script.Graphs.Graph).Assembly,
				typeof(Script.Graphs3D.Canvas3D).Assembly,
				typeof(Script.Fractals.FractalGraph).Assembly,
				typeof(Script.FullTextSearch.Functions.Search).Assembly,
				typeof(Script.Persistence.Functions.FindObjects).Assembly,
				typeof(Script.Statistics.Functions.RandomNumbers.Beta).Assembly,
				typeof(Security.IUser).Assembly,
				typeof(Security.EllipticCurves.PrimeFieldCurve).Assembly);

			appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			if (!appDataFolder.EndsWith(new string(Path.DirectorySeparatorChar, 1)))
				appDataFolder += Path.DirectorySeparatorChar;

			appDataFolder += "IoT Client" + Path.DirectorySeparatorChar;

			if (!Directory.Exists(appDataFolder))
			{
				try
				{
					Directory.CreateDirectory(appDataFolder);
				}
				catch (Exception ex)
				{
					if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
					{
						appDataFolder = appDataFolder.Replace("/usr/share", "/usr/local/share");
						Directory.CreateDirectory(appDataFolder);
					}
					else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
					{
						appDataFolder = appDataFolder.Replace("/usr/share", "/var/lib");
						Directory.CreateDirectory(appDataFolder);
					}
					else
						ExceptionDispatchInfo.Capture(ex).Throw();
				}
			}

			Task T = Task.Run(() =>
			{
				GraphViz.Init(appDataFolder);
				XmlLayout.Init(appDataFolder);
				PlantUml.Init(appDataFolder);
			});

			this.InitializeComponent();
			this.MainView.Load(this);

			Task.Run(() => this.Start());
		}

		private async void Start()
		{
			try
			{
				this.MainView.ShowStatus("Initializing");

				databaseProvider = await FilesProvider.CreateAsync(appDataFolder + "Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 3600000);
				await databaseProvider.RepairIfInproperShutdown(appDataFolder + "Transforms" + Path.DirectorySeparatorChar + "DbStatXmlToHtml.xslt");
				await databaseProvider.Start();
				Database.Register(databaseProvider);

				await Database.Find<Question>(new FilterAnd(new FilterFieldEqualTo("OwnerJID", string.Empty),
					new FilterFieldEqualTo("ProvisioningJID", string.Empty)));  // To prepare indices, etc.

				ChatView.InitEmojis();

				this.MainView.ShowStatus("Initialization complete.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				this.MainView.ShowStatus("Failure to initialize: " + ex.Message);
				ErrorBox(ex.Message);
			}
		}

		public static string? AppDataFolder => appDataFolder;

		internal static Scheduler? Scheduler => scheduler;

		internal static readonly string registryKey = Registry.CurrentUser + @"\Software\Waher Data AB\Waher.Client.WPF";

		private async void Window_Loaded(object Sender, RoutedEventArgs e)
		{
			object Value;

			try
			{
				Log.RegisterAlertExceptionType(true,
					typeof(OutOfMemoryException),
					typeof(StackOverflowException),
					typeof(AccessViolationException),
					typeof(InsufficientMemoryException));

				Log.RegisterExceptionToUnnest(typeof(System.Runtime.InteropServices.ExternalException));
				Log.RegisterExceptionToUnnest(typeof(System.Security.Authentication.AuthenticationException));

				Value = Registry.GetValue(registryKey, "WindowLeft", (int)this.Left);
				if (Value is not null && Value is int WindowLeft)
					this.Left = WindowLeft;

				Value = Registry.GetValue(registryKey, "WindowTop", (int)this.Top);
				if (Value is not null && Value is int WindowTop)
					this.Top = WindowTop;

				Value = Registry.GetValue(registryKey, "WindowWidth", (int)this.Width);
				if (Value is not null && Value is int WindowWidth && WindowWidth > 0)
					this.Width = WindowWidth;

				Value = Registry.GetValue(registryKey, "WindowHeight", (int)this.Height);
				if (Value is not null && Value is int WindowHeight && WindowHeight > 0)
					this.Height = WindowHeight;

				Value = Registry.GetValue(registryKey, "ConnectionTreeWidth", (int)this.MainView.ConnectionTree.Width);
				if (Value is not int ConnectionTreeWidth || ConnectionTreeWidth <= 0)
					ConnectionTreeWidth = 150;

				this.MainView.ConnectionsGrid.ColumnDefinitions[0].Width = new GridLength(ConnectionTreeWidth);

				Value = Registry.GetValue(registryKey, "WindowState", this.WindowState.ToString());
				if (Value is not null && Value is string s && Enum.TryParse(s, out WindowState WindowState))
					this.WindowState = WindowState;

				Value = Registry.GetValue(registryKey, "FileName", string.Empty);
				if (Value is not null && Value is string s2)
				{
					this.MainView.FileName = s2;
					if (!string.IsNullOrEmpty(this.MainView.FileName))
						await this.MainView.Load(this.MainView.FileName);
				}
			}
			catch (Exception ex)
			{
				System.Windows.MessageBox.Show(this, ex.Message, "Unable to load values from registry.", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Window_Closing(object Sender, System.ComponentModel.CancelEventArgs e)
		{
			if (!this.MainView.CheckSaved())
			{
				e.Cancel = true;
				return;
			}

			scheduler?.Dispose();
			scheduler = null;

			Registry.SetValue(registryKey, "WindowLeft", (int)this.Left, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowTop", (int)this.Top, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowWidth", (int)this.Width, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowHeight", (int)this.Height, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "ConnectionTreeWidth", (int)this.MainView.ConnectionsGrid.ColumnDefinitions[0].Width.Value, RegistryValueKind.DWord);
			Registry.SetValue(registryKey, "WindowState", this.WindowState.ToString(), RegistryValueKind.String);
			Registry.SetValue(registryKey, "FileName", this.MainView.FileName, RegistryValueKind.String);

			Log.TerminateAsync().Wait();

			if (databaseProvider is not null)
			{
				databaseProvider.Stop().Wait();
				databaseProvider.Flush().Wait();
				databaseProvider.DisposeAsync().Wait();
				databaseProvider = null;
			}
		}

		private void ConnectTo_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			this.MainView.ConnectTo_Executed(Sender, e);
		}

		public ITabView? CurrentTab
		{
			get
			{
				if (this.Tabs.SelectedItem is not TabItem TabItem)
					return null;
				else
					return TabItem.Content as ITabView;
			}
		}

		private void Save_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			this.CurrentTab?.SaveButton_Click(Sender, e);
		}

		private void SaveAs_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			this.CurrentTab?.SaveAsButton_Click(Sender, e);
		}

		private void Open_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			this.CurrentTab?.OpenButton_Click(Sender, e);
		}

		private void New_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			this.CurrentTab?.NewButton_Click(Sender, e);
		}

		internal void SelectionChanged()
		{
			TreeNode? Node = this.SelectedNode;

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
				this.CopyButton.IsEnabled = false;
				this.PasteButton.IsEnabled = false;
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
				this.CopyButton.IsEnabled = Node.CanCopy;
				this.PasteButton.IsEnabled = Node.CanPaste;
			}
		}

		private TreeNode? SelectedNode
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

		public static MainWindow? FindWindow(FrameworkElement? Element)
		{
			MainWindow? MainWindow = Element as MainWindow;

			while (MainWindow is null && Element is not null)
			{
				Element = Element.Parent as FrameworkElement;
				MainWindow = Element as MainWindow;
			}

			return MainWindow;
		}

		private void Add_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = Node is not null && Node.CanAddChildren;
		}

		private void Add_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanAddChildren)
				return;

			Node.Add();
		}

		private void Refresh_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanRecycle);
		}

		private void Refresh_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanRecycle)
				return;

			Node.Recycle(this);
		}

		private void Delete_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanDelete);
		}

		private void Delete_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanDelete)
				return;

			if (Node.CustomDeleteQuestion ||
				System.Windows.MessageBox.Show(this, "Are you sure you want to remove " + Node.Header + "?", "Are you sure?", MessageBoxButton.YesNo,
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

		private void Edit_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanEdit);
		}

		private void Edit_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanEdit)
				return;

			Node.Edit();
		}

		private void Copy_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanCopy);
		}

		private void Copy_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanCopy)
				return;

			Node.Copy();

			CommandManager.InvalidateRequerySuggested();
		}

		private void Paste_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanPaste);
		}

		private void Paste_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanPaste)
				return;

			Node.Paste();
		}

		private void Sniff_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.IsSniffable);
		}

		private void Sniff_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.IsSniffable)
				return;

			this.GetSnifferView(Node, null, false);
		}

		internal SnifferView GetSnifferView(TreeNode Node, string? Identifier, bool Custom)
		{
			SnifferView? View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as SnifferView;
				if (View is null)
					continue;

				if ((Node is not null && View.Node == Node) || (!string.IsNullOrEmpty(Identifier) && View.Identifier == Identifier))
				{
					Tab.Focus();
					return View;
				}
			}

			TabItem TabItem = NewTab(Node?.Header ?? Identifier);
			this.Tabs.Items.Add(TabItem);

			View = new SnifferView(Node, Identifier, Custom);
			TabItem.Content = View;

			View.Sniffer = new TabSniffer(View);
			Node?.AddSniffer(View.Sniffer);

			this.Tabs.SelectedItem = TabItem;

			return View;
		}

		internal LogView GetLogView(string Identifier)
		{
			LogView? View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as LogView;
				if (View is null)
					continue;

				if (View.Identifier is not null && View.Identifier == Identifier)
				{
					Tab.Focus();
					return View;
				}
			}

			TabItem TabItem = NewTab(Identifier);
			this.Tabs.Items.Add(TabItem);

			View = new LogView(Identifier, false);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;

			return View;
		}

		private void EventLog_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void EventLog_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			LogView View;

			foreach (TabItem Tab in this.Tabs.Items)
			{
				View = Tab.Content as LogView;
				if (View is null || View.Sink is null)
					continue;

				Tab.Focus();
				return;
			}

			TabItem TabItem = NewTab("Event Log");
			this.Tabs.Items.Add(TabItem);

			View = new LogView(null, true);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;
		}

		private void CloseTab_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = this.Tabs.SelectedIndex > 0;
		}

		internal void CloseTab_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			int i = this.Tabs.SelectedIndex;
			if (i > 0)
			{
				if (this.Tabs.Items[i] is TabItem TabItem)
				{
					object Content = TabItem.Content;
					if (Content is not null && Content is IDisposable Disposable)
						Disposable.Dispose();
				}

				this.Tabs.Items.RemoveAt(i);
			}
		}

		private void Chat_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanChat);
		}

		private void Chat_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
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

			TabItem TabItem = NewTab(Node.Header);
			this.Tabs.Items.Add(TabItem);

			View = new(Node, Node is RoomNode);
			TabItem.Content = View;

			this.Tabs.SelectedItem = TabItem;

			Thread T = new(this.FocusChatInput);
			T.Start(View);
		}

		private void FocusChatInput(object P)
		{
			Thread.Sleep(50);
			UpdateGui(this.FocusChatInput2, P);
		}

		private Task FocusChatInput2(object P)
		{
			ChatView View = (ChatView)P;
			View.Input.Focus();
			return Task.CompletedTask;
		}

		public Task OnStateChange(object _, XmppState _2)
		{
			try
			{
				SortedDictionary<int, int> ByState = new(reverseInt32);
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
					this.MainView.ShowStatus(string.Empty);
				else if (c == 1)
					this.MainView.ShowStatus(StateToString((XmppState)i));
				else
				{
					StringBuilder sb = new();
					bool First = true;

					foreach (KeyValuePair<int, int> P in ByState)
					{
						if (First)
							First = false;
						else
							sb.Append(", ");

						sb.Append(P.Value);
						sb.Append(' ');
						sb.Append(StateToString((XmppState)P.Key));
					}

					this.MainView.ShowStatus(sb.ToString());
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}

			return Task.CompletedTask;
		}

		private static string StateToString(XmppState State)
		{
			return State switch
			{
				XmppState.Offline => "Offline",
				XmppState.Connecting => "Connecting",
				XmppState.StreamNegotiation => "Negotiating stream",
				XmppState.StreamOpened => "Opened stream",
				XmppState.StartingEncryption => "Starting encryption",
				XmppState.Authenticating => "Authenticating",
				XmppState.Registering => "Registering",
				XmppState.Binding => "Binding",
				XmppState.RequestingSession => "Requesting session",
				XmppState.FetchingRoster => "Fetching roster",
				XmppState.SettingPresence => "Setting presence",
				XmppState.Connected => "Connected",
				XmppState.Error => "In error",
				_ => "Unknown",
			};
		}

		private class ReverseInt32 : IComparer<int>
		{
			public int Compare(int x, int y)
			{
				return y - x;
			}
		}

		private static readonly ReverseInt32 reverseInt32 = new();

		public Task OnChatMessage(object Sender, MessageEventArgs e)
		{
			UpdateGui(this.ChatMessageReceived, e);
			return Task.CompletedTask;
		}

		public static void ParseChatMessage(MessageEventArgs e, out string Message, out bool IsMarkdown, out DateTime Timestamp)
		{
			Timestamp = DateTime.Now;
			Message = e.Body;
			IsMarkdown = false;

			foreach (XmlNode N in e.Message.ChildNodes)
			{
				if (N is XmlElement E)
				{
					switch (E.LocalName)
					{
						case "content":
							if (E.NamespaceURI == "urn:xmpp:content")
							{
								string Type = XML.Attribute(E, "type");
								if (Type == MarkdownCodec.ContentType)
								{
									IsMarkdown = true;

									Type = E.InnerText;
									if (!string.IsNullOrEmpty(Type))
										Message = Type;
								}
							}
							break;

						case "delay":
							if (E.NamespaceURI == PubSubClient.NamespaceDelayedDelivery &&
								E.HasAttribute("stamp") &&
								XML.TryParse(E.GetAttribute("stamp"), out DateTime Timestamp2))
							{
								Timestamp = Timestamp2.ToLocalTime();
							}
							break;
					}
				}
			}
		}

		private async Task ChatMessageReceived(object P)
		{
			MessageEventArgs e = (MessageEventArgs)P;
			ParseChatMessage(e, out string Message, out bool IsMarkdown, out DateTime Timestamp);
			await this.ChatMessage(e.FromBareJID, XmppClient.GetBareJID(e.To), Message, e.ThreadID, IsMarkdown, Timestamp);
		}

		public async Task ChatMessage(string FromBareJid, string ToBareJid, string Message, string ThreadId, bool IsMarkdown, DateTime Timestamp)
		{
			XmppAccountNode XmppAccountNode;

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				if (TabItem.Content is not ChatView ChatView)
					continue;

				if (ChatView.Node is XmppContact XmppContact)
				{
					if (XmppContact.BareJID != FromBareJid)
						continue;

					XmppAccountNode = XmppContact.XmppAccountNode;
					if (XmppAccountNode.BareJID != ToBareJid)
						continue;
				}
				else if (ChatView.Node is Model.XmppComponent XmppComponent)
				{
					if (XmppComponent.JID != FromBareJid)
						continue;

					XmppAccountNode = XmppComponent.Account;
					if (XmppAccountNode.BareJID != ToBareJid)
						continue;
				}
				else if (ChatView.Node is OccupantNode OccupantNode)
				{
					if (OccupantNode.RoomId + "@" + OccupantNode.Domain + "/" + OccupantNode.NickName != FromBareJid)
						continue;
				}
				else
					continue;

				await ChatView.ChatMessageReceived(Message, FromBareJid, ThreadId, IsMarkdown, Timestamp, this);
				return;
			}

			foreach (TreeNode Node in this.MainView.ConnectionTree.Items)
			{
				if (Node is XmppAccountNode XmppAccountNode2 && XmppAccountNode2.BareJID == ToBareJid)
				{
					if (XmppAccountNode2.TryGetChild(FromBareJid, out TreeNode ContactNode))
					{
						TabItem TabItem2 = NewTab(FromBareJid);
						this.Tabs.Items.Add(TabItem2);

						ChatView ChatView = new(ContactNode, false);
						TabItem2.Content = ChatView;

						await ChatView.ChatMessageReceived(Message, FromBareJid, ThreadId, IsMarkdown, Timestamp, this);
						return;
					}
					else
					{
						string BareJid = XmppClient.GetBareJID(FromBareJid);
						string Account = XmppClient.GetAccount(BareJid);
						string Domain = XmppClient.GetDomain(BareJid);
						string Resource = XmppClient.GetResource(FromBareJid);

						if (XmppAccountNode2.TryGetChild(Domain, out TreeNode ComponentNode) &&
							ComponentNode.TryGetChild(BareJid, out TreeNode RoomNode0) &&
							RoomNode0 is RoomNode RoomNode)
						{
							if (!RoomNode.TryGetChild(Resource, out ContactNode))
							{
								ContactNode = RoomNode.CreateOccupantNode(Account, Domain, Resource,
									Networking.XMPP.MUC.Affiliation.None, Role.None, string.Empty);
							}

							TabItem TabItem2 = NewTab(FromBareJid);
							this.Tabs.Items.Add(TabItem2);

							ChatView ChatView = new(ContactNode, false);
							TabItem2.Content = ChatView;

							await ChatView.ChatMessageReceived(Message, FromBareJid, ThreadId, IsMarkdown, Timestamp, this);
							return;
						}
					}
				}
			}
		}

		public ChatView FindRoomView(string FromFullJid, string ToBareJid)
		{
			string FromBareJid = XmppClient.GetBareJID(FromFullJid);

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				if (TabItem.Content is not ChatView ChatView)
					continue;

				if (ChatView.Node is RoomNode Room)
				{
					if (Room.Jid != FromBareJid)
						continue;

					if (Room.Service.MucClient.Client.BareJID != ToBareJid)
						continue;
				}
				else
					continue;

				return ChatView;
			}

			return null;
		}

		public async Task MucGroupChatMessage(string FromFullJid, string ToBareJid, string Message, string ThreadId, bool IsMarkdown,
			DateTime Timestamp, ChatItemType Type, RoomNode Node, string Title)
		{
			ChatView ChatView = this.FindRoomView(FromFullJid, ToBareJid);

			if (ChatView is null)
			{
				TabItem TabItem2 = NewTab(Title);
				this.Tabs.Items.Add(TabItem2);

				ChatView = new(Node, true);
				TabItem2.Content = ChatView;
			}

			switch (Type)
			{
				case ChatItemType.Transmitted:
					await ChatView.ChatMessageTransmitted(Message, ThreadId);
					break;

				case ChatItemType.Received:
					await ChatView.ChatMessageReceived(Message, FromFullJid, ThreadId, IsMarkdown, Timestamp, this);
					break;

				case ChatItemType.Event:
					ChatView.Event(Message, XmppClient.GetResource(FromFullJid), ThreadId);
					break;
			}
		}

		public async Task MucPrivateChatMessage(string FromFullJid, string ToBareJid, string Message, string ThreadId, bool IsMarkdown, DateTime Timestamp,
			OccupantNode Node, string Title)
		{
			if (!string.IsNullOrEmpty(ThreadId))
			{
				ChatView ChatView = this.FindRoomView(FromFullJid, ToBareJid);
				if (ChatView is not null && ChatView.ContainsThread(ThreadId))
				{
					await ChatView.ChatMessageReceived(Message, FromFullJid, ThreadId, IsMarkdown, Timestamp, this);
					return;
				}
			}

			foreach (TabItem TabItem in this.Tabs.Items)
			{
				if (TabItem.Content is not ChatView ChatView)
					continue;

				if (ChatView.Node is OccupantNode Occupant)
				{
					if (Occupant.RoomId + "@" + Occupant.Domain + "/" + Occupant.NickName != FromFullJid)
						continue;
				}
				else
					continue;

				await ChatView.ChatMessageReceived(Message, FromFullJid, ThreadId, IsMarkdown, Timestamp, this);
				return;
			}

			TabItem TabItem2 = NewTab(Title);
			this.Tabs.Items.Add(TabItem2);

			ChatView ChatView2 = new(Node, true);
			TabItem2.Content = ChatView2;

			await ChatView2.ChatMessageReceived(Message, FromFullJid, ThreadId, IsMarkdown, Timestamp, this);
		}

		public void MucChatSubject(string FromFullJid, string ToBareJid, RoomNode Node, string Title)
		{
			ChatView ChatView = this.FindRoomView(FromFullJid, ToBareJid);

			if (ChatView is not null)
			{
				if (ChatView.Parent is TabItem TabItem)
					NewHeader(Title, TabItem);

				return;
			}

			TabItem TabItem2 = NewTab(Title);
			this.Tabs.Items.Add(TabItem2);

			ChatView ChatView2 = new(Node, true);
			TabItem2.Content = ChatView2;
		}

		private void Tabs_SelectionChanged(object Sender, SelectionChangedEventArgs e)
		{
			if (this.Tabs.SelectedItem is TabItem Item)
			{
				if (Item.Content is ChatView View)
				{
					Thread T = new(this.FocusChatInput);
					T.Start(View);
				}
			}
		}

		private void ReadMomentary_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanReadSensorData);
		}

		private async void ReadMomentary_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				TreeNode? Node = this.SelectedNode;
				if (Node is null || !Node.CanReadSensorData)
					return;

				SensorDataClientRequest Request = await Node.StartSensorDataMomentaryReadout();
				if (Request is null)
					return;

				TabItem TabItem = NewTab(Node.Header);
				this.Tabs.Items.Add(TabItem);

				SensorDataView View = new(Request, Node, false);
				TabItem.Content = View;

				this.Tabs.SelectedItem = TabItem;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void ReadDetailed_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanReadSensorData);
		}

		private async void ReadDetailed_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				TreeNode? Node = this.SelectedNode;
				if (Node is null || !Node.CanReadSensorData)
					return;

				SensorDataClientRequest Request = await Node.StartSensorDataFullReadout();
				if (Request is null)
					return;

				TabItem TabItem = NewTab(Node.Header);
				this.Tabs.Items.Add(TabItem);

				SensorDataView View = new(Request, Node, false);
				TabItem.Content = View;

				this.Tabs.SelectedItem = TabItem;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void SubscribeToMomentary_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanSubscribeToSensorData);
		}

		private async void SubscribeToMomentary_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				TreeNode? Node = this.SelectedNode;
				if (Node is null || !Node.CanSubscribeToSensorData)
					return;

				SensorDataClientRequest Request;

				if (Node.CanReadSensorData)
					Request = await Node.StartSensorDataMomentaryReadout();
				else
					Request = await Node.SubscribeSensorDataMomentaryReadout([]);

				if (Request is null)
					return;

				TabItem TabItem = NewTab(Node.Header);
				this.Tabs.Items.Add(TabItem);

				SensorDataView View = new(Request, Node, true);
				TabItem.Content = View;

				this.Tabs.SelectedItem = TabItem;
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private void Configure_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanConfigure);
		}

		private void Configure_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			if (Node is null || !Node.CanConfigure)
				return;

			Node.Configure();
		}

		public void ShowDataForm(DataForm Form)
		{
			UpdateGui(this.ShowForm, Form);
		}

		internal async Task ShowForm(object P)
		{
			Mouse.OverrideCursor = null;

			DataForm Form = (DataForm)P;

			/*string Xml = File.ReadAllText("../../../../Networking/Waher.Networking.XMPP.Test/Data/TestForm.xml");
			XmlDocument Doc = new XmlDocument()
			{
				PreserveWhitespace = true
			};
			Doc.LoadXml(Xml);
			Form = new DataForm(Form.Client, Doc.DocumentElement, null, null, Form.From, Form.To);*/

			await ShowParameterDialog(Form);
		}

		internal Task ShowError(object P)
		{
			Mouse.OverrideCursor = null;

			if (P is IqResultEventArgs e)
				System.Windows.MessageBox.Show(this, e.ErrorText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				System.Windows.MessageBox.Show(this, P.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

			return Task.CompletedTask;
		}

		private void Search_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
			e.CanExecute = (Node is not null && Node.CanSearch);
		}

		private void Search_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TreeNode? Node = this.SelectedNode;
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

		private void Script_CanExecute(object Sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void Script_Executed(object Sender, ExecutedRoutedEventArgs e)
		{
			TabItem TabItem = NewTab("Script");
			this.Tabs.Items.Add(TabItem);

			ScriptView ScriptView = new();
			TabItem.Content = ScriptView;

			this.Tabs.SelectedItem = TabItem;
		}

		internal void NewQuestion(XmppAccountNode Owner, ProvisioningClient ProvisioningClient, Question Question)
		{
			QuestionView QuestionView = this.FindQuestionTab(Owner, ProvisioningClient);

			if (QuestionView is not null && Question is not null)
			{
				QuestionView.NewQuestion(Question);
				return;
			}

			Task.Run(async () =>
			{
				try
				{
					LinkedList<Question> Questions = new();
					bool Found = Question is null;

					foreach (Question Question2 in await Database.Find<Question>(new FilterAnd(
						new FilterFieldEqualTo("OwnerJID", Owner?.BareJID),
						new FilterFieldEqualTo("ProvisioningJID", ProvisioningClient?.ProvisioningServerAddress)), "Created"))
					{
						if (string.IsNullOrEmpty(Question2.Sender))
						{
							string ThingDomain = XmppClient.GetDomain(Question2.JID);
							KeyValuePair<string, string> P = await Owner.Client.FindComponentAsync(ThingDomain, ProvisioningClient.NamespacesProvisioningOwner);
							string Component = P.Key;

							if (!string.IsNullOrEmpty(Component))
							{
								Question2.Sender = Component;
								await Database.Update(Question2);
							}
						}

						Questions.AddLast(Question2);

						if (!Found)
							Found = Question2.ObjectId == Question.ObjectId;
					}

					if (!Found)
						Questions.AddLast(Question);

					if (Questions.First is not null)
					{
						UpdateGui(() =>
						{
							QuestionView ??= this.CreateQuestionTab(Owner, ProvisioningClient);

							foreach (Question Question2 in Questions)
								QuestionView.NewQuestion(Question2);

							return Task.CompletedTask;
						});
					}
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			});
		}

		private QuestionView FindQuestionTab(XmppAccountNode Owner, ProvisioningClient ProvisioningClient)
		{
			foreach (TabItem TabItem in this.Tabs.Items)
			{
				if (TabItem.Content is QuestionView QuestionView &&
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
			TabItem TabItem = NewTab("Questions (" + Owner.BareJID + ")");
			this.Tabs.Items.Add(TabItem);

			QuestionView QuestionView = new(Owner, ProvisioningClient);
			TabItem.Content = QuestionView;

			return QuestionView;
		}

		internal static TabItem NewTab(string? HeaderText)
		{
			return NewTab(HeaderText, out TextBlock _);
		}

		internal static TabItem NewTab(string? HeaderText, out TextBlock HeaderLabel)
		{
			TabItem Result = new();
			NewHeader(HeaderText, Result, out HeaderLabel);
			return Result;
		}

		internal static void NewHeader(string HeaderText, TabItem Tab)
		{
			NewHeader(HeaderText, Tab, out TextBlock _);
		}

		internal static void NewHeader(string? HeaderText, TabItem Tab, out TextBlock HeaderLabel)
		{
			StackPanel Header = new()
			{
				Orientation = Orientation.Horizontal
			};

			Image CloseImage = new()
			{
				Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative)),
				Width = 16,
				Height = 16,
				ToolTip = "Close tab"
			};

			HeaderLabel = new TextBlock()
			{
				Text = HeaderText ?? string.Empty,
				Margin = new Thickness(0, 0, 5, 0)
			};

			Header.Children.Add(HeaderLabel);
			Header.Children.Add(CloseImage);

			CloseImage.MouseLeftButtonDown += CloseImage_MouseLeftButtonDown;
			CloseImage.MouseEnter += CloseImage_MouseEnter;
			CloseImage.MouseLeave += CloseImage_MouseLeave;
			CloseImage.Tag = Tab;

			Tab.Header = Header;
		}

		private static void CloseImage_MouseLeave(object Sender, MouseEventArgs e)
		{
			if (Sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon-gray.png", UriKind.Relative));
		}

		private static void CloseImage_MouseEnter(object Sender, MouseEventArgs e)
		{
			if (Sender is Image Image)
				Image.Source = new BitmapImage(new Uri("../Graphics/symbol-delete-icon.png", UriKind.Relative));
		}

		private static void CloseImage_MouseLeftButtonDown(object Sender, MouseButtonEventArgs e)
		{
			if (Sender is Image { Tag: TabItem Item })
			{
				currentInstance?.Tabs?.Items.Remove(Item);
				if (Item.Content is not null && Item.Content is IDisposable Disposable)
					Disposable.Dispose();
			}
		}

		public static void ErrorBox(string ErrorMessage)
		{
			MessageBox(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		public static void SuccessBox(string Message)
		{
			MessageBox(Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public static void ShowStatus(string Message)
		{
			UpdateGui(() =>
			{
				currentInstance!.MainView.ShowStatus(Message);
				return Task.CompletedTask;
			});
		}

		public static void MessageBox(string Text, string Caption, MessageBoxButton Button, MessageBoxImage Icon)
		{
			UpdateGui(() =>
			{
				Mouse.OverrideCursor = null;
				System.Windows.MessageBox.Show(currentInstance, Text, Caption, Button, Icon);
				return Task.CompletedTask;
			});
		}

		public static async Task<bool?> ShowParameterDialog(XmppClient Client, 
			object FormObject, string Title)
		{
			Language Language = await Translator.GetDefaultLanguageAsync();
			DataForm Form = await Parameters.GetEditableForm(Client,
				Language, string.Empty, string.Empty, FormObject, Title, null);

			Form.SetMethodHandlers(
				(sender, e) => Task.CompletedTask,
				(sender, e) => Task.CompletedTask);

			while (true)
			{
				bool? Result = await ShowParameterDialog(Form);

				if (!Result.HasValue || !Result.Value)
					return Result;

				SetEditableFormResult EditResult = await Parameters.SetEditableForm(Language,
					FormObject, Form, false, null);

				if (EditResult.Errors is null || EditResult.Errors.Length == 0)
					return true;

				foreach (KeyValuePair<string, string> Error in EditResult.Errors)
				{
					Field? Field = Form[Error.Key];
					if (Field is null)
						continue;

					Field.Error = Error.Value;
				}
			}
		}

		public static async Task<bool?> ShowParameterDialog(DataForm Form)
		{
			TaskCompletionSource<bool?> Result = new();

			UpdateGui(async () =>
			{
				try
				{
					ParameterDialog Dialog = await ParameterDialog.CreateAsync(Form);

					if (Dialog.Empty)
					{
						await Form.Submit();
						Result.TrySetResult(true);
					}
					else
					{
						Dialog.Owner = currentInstance;
						Result.TrySetResult(Dialog.ShowDialog());
					}
				}
				catch (Exception ex)
				{
					Result.TrySetException(ex);
				}
			});

			return await Result.Task;
		}

		public static void MouseDefault()
		{
			UpdateGui(() =>
			{
				Mouse.OverrideCursor = null;
				return Task.CompletedTask;
			});
		}

		public static void UpdateGui(GuiDelegate Method)
		{
			UpdateGui((State) => ((GuiDelegate)State)(), Method.Method.DeclaringType + "." + Method.Method.Name, Method);
		}

		public static void UpdateGui(GuiDelegateWithParameter Method, object State)
		{
			UpdateGui(Method, Method.Method.DeclaringType + "." + Method.Method.Name, State);
		}

		private static async void UpdateGui(GuiDelegateWithParameter Method, string Name, object State)
		{
			if (currentInstance?.Dispatcher.CheckAccess() ?? false)
			{
				try
				{
					await Method(State);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
			else
			{
				bool Start;
				GuiUpdateTask Rec = new()
				{
					Method = Method,
					State = State,
					Name = Name,
					Requested = DateTime.Now
				};

				lock (guiUpdateQueue)
				{
					Start = guiUpdateQueue.First is null;
					guiUpdateQueue.AddLast(Rec);
				}

				if (Start)
					_ = currentInstance!.Dispatcher.BeginInvoke(new GuiDelegate(DoUpdates));
			}
		}

		private static async Task DoUpdates()
		{
			try
			{
				GuiUpdateTask? Rec = null;
				GuiUpdateTask? Prev;

				while (true)
				{
					lock (guiUpdateQueue)
					{
						if (Rec is not null)
							guiUpdateQueue.RemoveFirst();

						Prev = Rec;
						Rec = guiUpdateQueue.First?.Value;
						if (Rec is null)
							return;
					}

					try
					{
						Rec.Started = DateTime.Now;
						await Rec.Method!(Rec.State);
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
					finally
					{
						Rec.Ended = DateTime.Now;
					}

					TimeSpan TS;

					if ((TS = (Rec.Ended - Rec.Started)).TotalSeconds >= 1)
						Log.Notice("GUI update method is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
					else if ((TS = (Rec.Ended - Rec.Requested)).TotalSeconds >= 1)
						Log.Notice("GUI update pipeline is slow: " + TS.ToString(), Rec.Name, Prev?.Name);
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);

				lock (guiUpdateQueue)
				{
					guiUpdateQueue.Clear();
				}
			}
		}

		private class GuiUpdateTask
		{
			public GuiDelegateWithParameter? Method;
			public object? State;
			public string? Name;
			public DateTime Requested;
			public DateTime Started;
			public DateTime Ended;
		}

	}
}
