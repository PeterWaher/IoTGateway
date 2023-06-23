using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Client.WPF.Controls;
using Waher.Client.WPF.Controls.Sniffers;
using Waher.Client.WPF.Dialogs;
using Waher.Client.WPF.Dialogs.IoT;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model.Concentrator
{
	/// <summary>
	/// Represents a node in a concentrator.
	/// </summary>
	public class Node : TreeNode, IMenuAggregator
	{
		private NodeInformation nodeInfo;
		private NodeCommand[] commands = null;

		/// <summary>
		/// Represents a node in a concentrator.
		/// </summary>
		/// <param name="Parent">Parent node</param>
		/// <param name="NodeInfo">Node information</param>
		public Node(TreeNode Parent, NodeInformation NodeInfo)
			: base(Parent)
		{
			this.nodeInfo = NodeInfo;

			if (this.nodeInfo.ParameterList is null)
				this.parameters = null;
			else
				this.parameters = new DisplayableParameters(this.nodeInfo.ParameterList);

			if (this.nodeInfo.HasChildren)
			{
				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};
			}
		}

		/// <summary>
		/// Node ID
		/// </summary>
		public string NodeId => this.nodeInfo.NodeId;

		/// <summary>
		/// Source ID
		/// </summary>
		public string SourceId => this.nodeInfo.SourceId;

		/// <summary>
		/// Partition ID
		/// </summary>
		public string Partition => this.nodeInfo.Partition;

		/// <summary>
		/// Key in parent child collection.
		/// </summary>
		public override string Key => this.nodeInfo.NodeId;

		/// <summary>
		/// Tree Node header text.
		/// </summary>
		public override string Header => this.nodeInfo.LocalId;

		/// <summary>
		/// Tool Tip for node.
		/// </summary>
		public override string ToolTip => "Node";

		/// <summary>
		/// If the node can be recycled.
		/// </summary>
		public override bool CanRecycle => false;

		/// <summary>
		/// Information about the node.
		/// </summary>
		public NodeInformation NodeInformation
		{
			get => this.nodeInfo;
			internal set => this.nodeInfo = value;
		}

		/// <summary>
		/// Node Type Name.
		/// </summary>
		public override string TypeName
		{
			get
			{
				if (!(this.parameters is null))
				{
					string s = this.parameters["Type"];
					if (!string.IsNullOrEmpty(s))
						return s;
				}

				return this.nodeInfo.NodeType;
			}
		}

		/// <summary>
		/// Image resource for the node.
		/// </summary>
		public override ImageSource ImageResource
		{
			get
			{
				if (this.nodeInfo.HasChildren)
				{
					if (this.IsExpanded)
						return XmppAccountNode.folderYellowOpen;
					else
						return XmppAccountNode.folderYellowClosed;
				}
				else
					return XmppAccountNode.box;
			}
		}

		/// <summary>
		/// Saves the object to a file.
		/// </summary>
		/// <param name="Output">Output</param>
		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		/// <summary>
		/// Reference to the concentrator node.
		/// </summary>
		public XmppConcentrator Concentrator
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (!(Loop is null))
				{
					if (Loop is XmppConcentrator Concentrator)
						return Concentrator;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		/// <summary>
		/// Reference to the data source node.
		/// </summary>
		public DataSource DataSource
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (!(Loop is null))
				{
					if (Loop is DataSource DataSource)
						return DataSource;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		private bool loadingChildren = false;

		/// <summary>
		/// Reference to the concentrator client
		/// </summary>
		public ConcentratorClient ConcentratorClient
		{
			get
			{
				XmppConcentrator Concentrator = this.Concentrator;
				if (Concentrator is null)
					return null;

				XmppAccountNode AccountNode = Concentrator.XmppAccountNode;
				if (AccountNode is null)
					return null;

				return AccountNode.ConcentratorClient;
			}
		}

		/// <summary>
		/// Method is called to make sure children are loaded.
		/// </summary>
		protected override void LoadChildren()
		{
			if (!this.loadingChildren && !this.IsLoaded)
			{
				string FullJid = this.Concentrator?.FullJid;
				ConcentratorClient ConcentratorClient = this.ConcentratorClient;

				if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
				{
					if (this.nodeInfo.HasChildren)
					{
						Mouse.OverrideCursor = Cursors.Wait;

						this.loadingChildren = true;
						ConcentratorClient.GetChildNodes(FullJid, this.nodeInfo, true, true, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
						{
							this.loadingChildren = false;
							MainWindow.MouseDefault();

							if (e.Ok)
							{
								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (NodeInformation Ref in e.NodesInformation)
									Children[Ref.NodeId] = new Node(this, Ref);

								this.children = Children;

								this.OnUpdated();
								this.DataSource?.NodesAdded(Children.Values, this);
							}
							else
								MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get child nodes." : e.ErrorText);

							return Task.CompletedTask;

						}, null);
					}
					else
					{
						if (!(this.children is null))
							this.DataSource?.NodesRemoved(this.children.Values, this);

						this.children = null;

						this.OnUpdated();
					}
				}
			}

			base.LoadChildren();
		}

		/// <summary>
		/// Method is called to notify children can be unloaded.
		/// </summary>
		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.nodeInfo.HasChildren && this.IsLoaded)
			{
				if (!(this.children is null))
					this.DataSource?.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}

		/// <summary>
		/// If it's possible to read sensor data from the node.
		/// </summary>
		public override bool CanReadSensorData => this.nodeInfo.IsReadable && this.IsOnline;

		/// <summary>
		/// If it's possible to subscribe to sensor data from the node.
		/// </summary>
		public override bool CanSubscribeToSensorData => this.nodeInfo.IsReadable && this.Concentrator.SupportsEvents && this.IsOnline;

		/// <summary>
		/// Starts readout of momentary sensor data values.
		/// </summary>
		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (!(XmppAccountNode is null) && !((SensorClient = XmppAccountNode.SensorClient) is null))
			{
				return SensorClient.RequestReadout(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[] { new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.Partition) }, FieldType.Momentary);
			}
			else
				return null;
		}

		/// <summary>
		/// Starts readout of all sensor data values.
		/// </summary>
		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (!(XmppAccountNode is null) && !((SensorClient = XmppAccountNode.SensorClient) is null))
			{
				return SensorClient.RequestReadout(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[] { new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.Partition) }, FieldType.All);
			}
			else
				throw new NotSupportedException();
		}

		/// <summary>
		/// Starts subscription of momentary sensor data values.
		/// </summary>
		public override SensorDataSubscriptionRequest SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (!(XmppAccountNode is null) && !((SensorClient = XmppAccountNode.SensorClient) is null))
			{
				return SensorClient.Subscribe(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[]
					{
						new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.Partition)
					},
					FieldType.Momentary, Rules, Duration.FromSeconds(1), Duration.FromMinutes(1), false);
			}
			else
				return null;
		}

		/// <summary>
		/// If it's possible to configure control parameters on the node.
		/// </summary>
		public override bool CanConfigure => this.nodeInfo.IsControllable && this.IsOnline;

		/// <summary>
		/// Gets the configuration form for the node.
		/// </summary>
		/// <param name="Callback">Method called when form is returned or when operation fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		public override void GetConfigurationForm(DataFormResultEventHandler Callback, object State)
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			ControlClient ControlClient;

			if (!(XmppAccountNode is null) && !((ControlClient = XmppAccountNode.ControlClient) is null))
			{
				ControlClient.GetForm(Concentrator.RosterItem.LastPresenceFullJid, "en", Callback, State,
					new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.Partition));
			}
			else
				throw new NotSupportedException();
		}

		/// <summary>
		/// If the concentrator hosting the node is online.
		/// </summary>
		public bool IsOnline
		{
			get
			{
				XmppConcentrator Concentrator = this.Concentrator;
				if (Concentrator is null)
					return false;

				XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
				if (XmppAccountNode is null)
					return false;

				XmppClient Client = XmppAccountNode.Client;
				return !(Client is null) && Client.State == XmppState.Connected;
			}
		}

		/// <summary>
		/// If children can be added to the node.
		/// </summary>
		public override bool CanAddChildren => this.IsOnline;

		/// <summary>
		/// If the node can be edited.
		/// </summary>
		public override bool CanEdit => this.IsOnline;

		/// <summary>
		/// If the node can be deleted.
		/// </summary>
		public override bool CanDelete => this.IsOnline;

		/// <summary>
		/// Is called when the user wants to add a node to the current node.
		/// </summary>
		public override void Add()
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ConcentratorClient.GetAddableNodeTypes(FullJid, this.nodeInfo, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						switch (e.Result.Length)
						{
							case 0:
								MainWindow.ErrorBox("No nodes can be added to this type of node.");
								break;

							case 1:
								MainWindow.UpdateGui(() =>
									{
										this.Add(e.Result[0].Unlocalized);
										return Task.CompletedTask;
									});
								break;

							default:
								MainWindow.UpdateGui(() =>
								{
									SelectItemDialog Form = new SelectItemDialog("Add node", "Select type of node to add:",
										"Add node of selected type.", "Type", "Class", e.Result)
									{
										Owner = MainWindow.currentInstance
									};

									bool? Result = Form.ShowDialog();

									if (Result.HasValue && Result.Value)
									{
										LocalizedString? Item = Form.SelectedItem;
										if (Item.HasValue)
											this.Add(Item.Value.Unlocalized);
									}

									return Task.CompletedTask;
								});
								break;
						}
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to add nodes." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}
		}

		private void Add(string Type)
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ConcentratorClient.GetParametersForNewNode(FullJid, this.nodeInfo, Type, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						MainWindow.UpdateGui(async () =>
						{
							ParameterDialog Dialog = await ParameterDialog.CreateAsync(e.Form);
							Dialog.ShowDialog();
						});
					}
					else
						MainWindow.ErrorBox(e.ErrorText);

					return Task.CompletedTask;

				}, (sender, e) =>
				{
					if (e.Ok)
						this.Add(new Node(this, e.NodeInformation));
					else if (!string.IsNullOrEmpty(e.From))
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to set parameters." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}
		}

		internal void Add(Node Node)
		{
			if (!this.loadingChildren && this.IsLoaded)
			{
				SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

				if (!(this.children is null))
				{
					foreach (KeyValuePair<string, TreeNode> P in this.children)
						Children[P.Key] = P.Value;
				}

				Children[Node.NodeId] = Node;
				this.children = Children;

				this.OnUpdated();
				this.DataSource?.NodesAdded(Children.Values, this);
			}
		}

		/// <summary>
		/// Method called when a node is to be deleted.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		/// <param name="OnDeleted">Method called when node has been successfully deleted.</param>
		public override void Delete(TreeNode Parent, EventHandler OnDeleted)
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ConcentratorClient.DestroyNode(FullJid, this.nodeInfo, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						try
						{
							base.Delete(Parent, OnDeleted);
						}
						catch (Exception ex)
						{
							Log.Critical(ex);
						}
					}
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to destroy node." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}
		}

		/// <summary>
		/// Is called when the user wants to edit a node.
		/// </summary>
		public override void Edit()
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;
			string OldKey = this.Key;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				ConcentratorClient.GetNodeParametersForEdit(FullJid, this.nodeInfo, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						MainWindow.UpdateGui(async () =>
						{
							ParameterDialog Dialog = await ParameterDialog.CreateAsync(e.Form);
							Dialog.ShowDialog();
						});
					}
					else
						MainWindow.ErrorBox(e.ErrorText);

					return Task.CompletedTask;

				}, (sender, e) =>
				{
					if (e.Ok)
					{
						this.nodeInfo = e.NodeInformation;
						this.OnUpdated();

						string NewKey = this.Key;
						TreeNode Parent = this.Parent;

						if (NewKey != OldKey && !(Parent is null))
							Parent.RenameChild(OldKey, NewKey, this);
					}
					else if (!string.IsNullOrEmpty(e.From))
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to set properties." : e.ErrorText);

					return Task.CompletedTask;

				}, null);
			}
		}

		/// <summary>
		/// If the node can be sniffed.
		/// </summary>
		public override bool IsSniffable => this.nodeInfo.Sniffable && this.IsOnline;

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer object.</param>
		public override void AddSniffer(ISniffer Sniffer)
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				this.ConcentratorClient.RegisterSniffer(FullJid, this.nodeInfo, DateTime.Now.AddHours(1), Sniffer,
					string.Empty, string.Empty, string.Empty, (sender, e) =>
					{
						MainWindow.MouseDefault();

						if (e.Ok)
						{
							if (Sniffer is TabSniffer TabSniffer)
								TabSniffer.SnifferId = e.SnifferrId;
						}
						else
							MainWindow.ErrorBox(e.ErrorText);

						return Task.CompletedTask;

					}, null);
			}
		}

		/// <summary>
		/// Removes a sniffer from the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer object.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		public override bool RemoveSniffer(ISniffer Sniffer)
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (Sniffer is TabSniffer TabSniffer &&
				!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid) &&
				!string.IsNullOrEmpty(TabSniffer.SnifferId))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				return this.ConcentratorClient.UnregisterSniffer(FullJid, this.nodeInfo, TabSniffer.SnifferId,
					string.Empty, string.Empty, string.Empty, (sender, e) =>
					{
						MainWindow.MouseDefault();
						return Task.CompletedTask;
					}, null);
			}
			else
				return false;
		}

		/// <summary>
		/// Method called when selection has been changed.
		/// </summary>
		public override void SelectionChanged()
		{
			base.SelectionChanged();

			if (!(this.nodeInfo is null) && this.nodeInfo.HasCommands && this.commands is null)
			{
				string FullJid = this.Concentrator?.FullJid;
				ConcentratorClient ConcentratorClient = this.ConcentratorClient;

				if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
				{
					this.commands = new NodeCommand[0];

					this.ConcentratorClient.GetNodeCommands(FullJid, this.nodeInfo, string.Empty, string.Empty, string.Empty, (sender, e) =>
					{
						if (e.Ok)
							this.commands = e.Result;
						else
							MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get commands." : e.ErrorText);

						return Task.CompletedTask;

					}, null);
				}
			}
		}

		/// <summary>
		/// Adds context sensitive menu items to a context menu.
		/// </summary>
		/// <param name="CurrentGroup">Current group.</param>
		/// <param name="Menu">Menu being built.</param>
		public void AddContexMenuItems(TreeNode Node, ref string CurrentGroup, ContextMenu Menu)
		{
			if (Node == this && !(this.commands is null))
			{
				MenuItem Item;

				foreach (NodeCommand Command in this.commands)
				{
					if (Command.Command == "Search")
						continue;

					this.GroupSeparator(ref CurrentGroup, Command.SortCategory, Menu);

					Menu.Items.Add(Item = new MenuItem()
					{
						Header = Command.Name,
						IsEnabled = true,
						Tag = Command
					});

					Item.Click += this.NodeCommandClick;
				}
			}
		}

		private void NodeCommandClick(object sender, System.Windows.RoutedEventArgs e)
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				MenuItem Item = (MenuItem)sender;
				NodeCommand Command = (NodeCommand)Item.Tag;

				if (!string.IsNullOrEmpty(Command.ConfirmationString))
				{
					if (System.Windows.MessageBox.Show(MainWindow.currentInstance, Command.ConfirmationString, "Confirm",
						System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Question) != System.Windows.MessageBoxResult.Yes)
					{
						return;
					}
				}

				switch (Command.Type)
				{
					case CommandType.Simple:
						Mouse.OverrideCursor = Cursors.Wait;

						ConcentratorClient.ExecuteCommand(FullJid, this.NodeId, this.SourceId, this.Partition, Command.Command,
							ConcentratorClient.Client.Language, string.Empty, string.Empty, string.Empty, (sender2, e2) =>
							{
								MainWindow.MouseDefault();

								this.ShowCommandResult(e2, Command);

								return Task.CompletedTask;

							}, null);
						break;

					case CommandType.Parametrized:
						Mouse.OverrideCursor = Cursors.Wait;

						ConcentratorClient.GetCommandParameters(FullJid, this.NodeId, this.SourceId, this.Partition, Command.Command,
							ConcentratorClient.Client.Language, string.Empty, string.Empty, string.Empty, (sender2, e2) =>
							{
								MainWindow.MouseDefault();

								if (e2.Ok)
								{
									MainWindow.UpdateGui(async () =>
									{
										ParameterDialog Dialog = await ParameterDialog.CreateAsync(e2.Form);
										Dialog.ShowDialog();
									});
								}
								else
									MainWindow.ErrorBox(e2.ErrorText);

								return Task.CompletedTask;
							},
							(sender2, e2) =>
							{
								this.ShowCommandResult(e2, Command);
								return Task.CompletedTask;
							}, null);
						break;

					case CommandType.Query:
						Mouse.OverrideCursor = Cursors.Wait;

						ConcentratorClient.GetQueryParameters(FullJid, this.NodeId, this.SourceId, this.Partition, Command.Command,
							ConcentratorClient.Client.Language, string.Empty, string.Empty, string.Empty, (sender2, e2) =>
							{
								MainWindow.MouseDefault();

								if (e2.Ok)
								{
									MainWindow.UpdateGui(async () =>
									{
										ParameterDialog Dialog = await ParameterDialog.CreateAsync(e2.Form);
										Dialog.ShowDialog();
									});
								}
								else
									MainWindow.ErrorBox(e2.ErrorText);

								return Task.CompletedTask;
							},
							(sender2, e2) =>
							{
								if (e2.Ok)
								{
									MainWindow.UpdateGui(async () =>
									{
										TabItem TabItem = MainWindow.NewTab(Command.Name, out TextBlock HeaderLabel);
										MainWindow.currentInstance.Tabs.Items.Add(TabItem);

										QueryResultView ResultView = await QueryResultView.CreateAsync(this, e2.Query, HeaderLabel);
										TabItem.Content = ResultView;

										TabItem.Focus();
									});
								}

								this.ShowCommandResult(e2, Command);

								return Task.CompletedTask;

							}, null);
						break;
				}
			}
		}

		private void ShowCommandResult(IqResultEventArgs e, NodeCommand Command)
		{
			if (!(this.commands is null))
			{
				this.commands = null;
				this.SelectionChanged();
			}

			if (e.Ok)
			{
				if (!string.IsNullOrEmpty(Command.SuccessString))
					MainWindow.MessageBox(Command.SuccessString, "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
			}
			else if (!string.IsNullOrEmpty(e.From))     // If error not sent from node, user has cancelled the command.
			{
				if (!string.IsNullOrEmpty(Command.FailureString))
					MainWindow.MessageBox(Command.FailureString, "Failure", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
				else
					MainWindow.ErrorBox(e.ErrorText);
			}
		}

		/// <summary>
		/// If it's possible to search for data on the node.
		/// </summary>
		public override bool CanSearch
		{
			get
			{
				if (!(this.commands is null))
				{
					foreach (NodeCommand Command in this.commands)
					{
						if (Command.Command == "Search")
							return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Performs a search on the node.
		/// </summary>
		public override void Search()
		{
			if (!(this.commands is null))
			{
				foreach (NodeCommand Command in this.commands)
				{
					if (Command.Command == "Search")
					{
						MenuItem Item = new MenuItem()
						{
							Header = Command.Name,
							IsEnabled = true,
							Tag = Command
						};

						this.NodeCommandClick(Item, new System.Windows.RoutedEventArgs());
						return;
					}
				}
			}

			base.Search();
		}

		/// <summary>
		/// If node can be copied to clipboard.
		/// </summary>
		public override bool CanCopy => this.IsOnline;

		/// <summary>
		/// Is called when the user wants to copy the node to the clipboard.
		/// </summary>
		public override async void Copy()
		{
			string FullJid = this.Concentrator?.FullJid;
			ConcentratorClient ConcentratorClient = this.ConcentratorClient;

			if (!(ConcentratorClient is null) && !string.IsNullOrEmpty(FullJid))
			{
				Mouse.OverrideCursor = Cursors.Wait;
				bool Error = false;

				try
				{
					StringBuilder sb = new StringBuilder();
					await ExportToXml(FullJid, ConcentratorClient, (this.Parent as Node)?.nodeInfo, this.nodeInfo, sb);
					System.Windows.Clipboard.SetText(XML.PrettyXml(sb.ToString()));
					MainWindow.MouseDefault();
				}
				catch (Exception ex)
				{
					MainWindow.ErrorBox(ex.Message);
					Error = true;
				}
				finally
				{
					if (!Error)
						MainWindow.ShowStatus("Copy placed in clipboard.");
				}
			}
		}

		private static async Task ExportToXml(string FullJid, ConcentratorClient ConcentratorClient,
			NodeInformation Parent, NodeInformation Node, StringBuilder sb)
		{
			TaskCompletionSource<DataForm> Request = new TaskCompletionSource<DataForm>();
			Task ParametersResult(object Sender, DataFormEventArgs e)
			{
				if (e.Ok)
				{
					if (e.Form.CanCancel)
						e.Form.Cancel();

					Request.TrySetResult(e.Form);
				}
				else
					Request.TrySetException(e.StanzaError ?? new Exception("Unable to get node information."));

				return Task.CompletedTask;
			};

			MainWindow.ShowStatus("Copying " + Node.NodeId + "...");

			ConcentratorClient.GetNodeParametersForEdit(FullJid, Node, "en", string.Empty, string.Empty, string.Empty,
				ParametersResult, null, null);

			DataForm Form = await Request.Task;

			sb.Append("<createNewNode xmlns='");
			sb.Append(ConcentratorServer.NamespaceConcentrator);
			sb.Append("' type='");
			sb.Append(XML.Encode(Node.NodeType));

			if (!(Parent is null))
			{
				sb.Append("' id='");
				sb.Append(XML.Encode(Parent.NodeId));

				if (!string.IsNullOrEmpty(Parent.SourceId))
				{
					sb.Append("' src='");
					sb.Append(XML.Encode(Parent.SourceId));
				}

				if (!string.IsNullOrEmpty(Parent.Partition))
				{
					sb.Append("' pt='");
					sb.Append(XML.Encode(Parent.Partition));
				}
			}
			sb.Append("'>");
			Form.SerializeSubmit(sb, true);

			if (Node.HasChildren)
			{
				TaskCompletionSource<NodeInformation[]> NodesInformation = new TaskCompletionSource<NodeInformation[]>();

				ConcentratorClient.GetChildNodes(FullJid, Node, true, false, "en", string.Empty, string.Empty, string.Empty, (sender, e) =>
				{
					if (e.Ok)
						NodesInformation.TrySetResult(e.NodesInformation);
					else
						NodesInformation.TrySetException(e.StanzaError ?? new Exception("Unable to get information about children."));

					return Task.CompletedTask;

				}, null);

				NodeInformation[] Children = await NodesInformation.Task;

				if (!(Children is null))
				{
					foreach (NodeInformation Child in Children)
						await ExportToXml(FullJid, ConcentratorClient, Node, Child, sb);
				}
			}

			sb.Append("</createNewNode>");
		}

		/// <summary>
		/// If node can be pasted to, from the clipboard.
		/// </summary>
		public override bool CanPaste
		{
			get
			{
				string FullJid = this.Concentrator?.FullJid;
				if (string.IsNullOrEmpty(FullJid))
					return false;

				ConcentratorClient ConcentratorClient = this.ConcentratorClient;
				if (ConcentratorClient is null)
					return false;

				if (!this.IsOnline)
					return false;

				if (!System.Windows.Clipboard.ContainsText())
					return false;

				string s = System.Windows.Clipboard.GetText();
				if (!XML.IsValidXml(s))
					return false;

				try
				{
					XmlDocument Doc = new XmlDocument();
					Doc.LoadXml(s);

					return
						!(Doc.DocumentElement is null) &&
						Doc.DocumentElement.LocalName == "createNewNode" &&
						Doc.DocumentElement.NamespaceURI == ConcentratorServer.NamespaceConcentrator;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Is called when the user wants to paste data from the clipboard to the node.
		/// </summary>
		public override async void Paste()
		{
			bool Error = false;

			try
			{
				string FullJid = this.Concentrator?.FullJid;
				if (string.IsNullOrEmpty(FullJid))
					return;

				ConcentratorClient ConcentratorClient = this.ConcentratorClient;
				if (ConcentratorClient is null)
					return;

				if (!this.IsOnline)
					return;

				if (!System.Windows.Clipboard.ContainsText())
					return;

				string s = System.Windows.Clipboard.GetText();
				if (string.IsNullOrEmpty(s))
					return;

				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(s);

				Mouse.OverrideCursor = Cursors.Wait;

				await ImportFromXml(FullJid, ConcentratorClient, this, Doc.DocumentElement);
			}
			catch (Exception ex)
			{
				MainWindow.ErrorBox(ex.Message);
				Error = true;
			}
			finally
			{
				MainWindow.MouseDefault();

				if (!Error)
					MainWindow.ShowStatus("Contents of clipboard pasted to node.");
			}
		}

		private static async Task ImportFromXml(string FullJid, ConcentratorClient ConcentratorClient,
			Node Parent, XmlElement Xml)
		{
			if (Xml is null ||
				Xml.LocalName != "createNewNode" ||
				Xml.NamespaceURI != ConcentratorServer.NamespaceConcentrator)
			{
				throw new Exception("Clipboard does not contain node information.");
			}

			string NodeType = XML.Attribute(Xml, "type");
			if (string.IsNullOrEmpty(NodeType))
				throw new Exception("Node type missing.");

			DataForm ImportForm = null;
			LinkedList<XmlElement> ChildElements = null;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				if (!(N is XmlElement E))
					continue;

				switch (E.LocalName)
				{
					case "x":
						if (ImportForm is null)
							ImportForm = new DataForm(ConcentratorClient.Client, E, null, null, string.Empty, string.Empty);
						else
							throw new Exception("Multiple form elements in XML.");
						break;

					case "createNewNode":
						if (ChildElements is null)
							ChildElements = new LinkedList<XmlElement>();

						ChildElements.AddLast(E);
						break;

					default:
						throw new Exception("Unrecognized XML element: " + E.LocalName);
				}
			}

			if (ImportForm is null)
				throw new Exception("Parameter form element missing from XML.");

			MainWindow.ShowStatus("Adding " + NodeType + " to " + Parent.NodeId + "...");

			TaskCompletionSource<Node> Request = new TaskCompletionSource<Node>();
			int IdCounter = 0;

			ConcentratorClient.GetParametersForNewNode(FullJid, Parent.nodeInfo, NodeType, "en", string.Empty, string.Empty, string.Empty,
				(object Sender, DataFormEventArgs e) =>
				{
					try
					{
						if (e.Ok)
						{
							foreach (Networking.XMPP.DataForms.Field Field in e.Form.Fields)
							{
								Networking.XMPP.DataForms.Field InputField = ImportForm[Field.Var];

								if (Field.HasError)
								{
									if (InputField is null)
									{
										Request.TrySetException(new Exception("Unable to add node of type " +
											NodeType + " to node " + Parent.NodeId + ". Required field " + Field.Var +
											" did not have a value in the node being pasted from the clipboard. " +
											"Error reported: " + Field.Error));
										return Task.CompletedTask;
									}
									else if (Field.Var == "NodeId")
									{
										if (IdCounter++ == 0)
											Field.SetValue(InputField.ValueString);
										else
											Field.SetValue(InputField.ValueString + " (" + IdCounter.ToString() + ")");
									}
									else if (IdCounter > 1)
									{
										Request.TrySetException(new Exception("Unable to add node of type " +
											NodeType + " to node " + Parent.NodeId + ". Value in clipboard for field " +
											Field.Var + " was not acceptable. Error reported: " + Field.Error));
										return Task.CompletedTask;
									}
									else
										Field.SetValue(InputField.ValueStrings);
								}
								else if (!(InputField is null))
									Field.SetValue(InputField.ValueStrings);
							}

							e.Form.Submit();
						}
						else
						{
							Request.TrySetException(e.StanzaError ?? new Exception("Unable to add a node of type " +
								NodeType + " to node " + Parent.NodeId + "."));
						}
					}
					catch (Exception ex)
					{
						Request.TrySetException(ex);
					}

					return Task.CompletedTask;
				}, (object Sender, NodeInformationEventArgs e) =>
				{
					if (e.Ok)
					{
						Node NewNode = new Node(Parent, e.NodeInformation);
						Parent.Add(NewNode);
						Request.TrySetResult(NewNode);
					}
					else if (!string.IsNullOrEmpty(e.From))
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to set parameters." : e.ErrorText);

					return Task.CompletedTask;
				}, null);

			Node CreatedNode = await Request.Task;

			if (!(ChildElements is null))
			{
				foreach (XmlElement Child in ChildElements)
					await ImportFromXml(FullJid, ConcentratorClient, CreatedNode, Child);
			}
		}

	}
}
