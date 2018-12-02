using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Sensor;
using Waher.Things.DisplayableParameters;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Abstract base class for tree nodes in the connection view.
	/// </summary>
	public abstract class TreeNode : SelectableItem, IDisposable
	{
		private TreeNode parent;
		protected DisplayableParameters parameters = null;
		protected SortedDictionary<string, TreeNode> children = null;
		private object tag = null;
		private bool expanded = false;

		/// <summary>
		/// Abstract base class for tree nodes in the connection view.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		public TreeNode(TreeNode Parent)
		{
			this.parent = Parent;
		}

		/// <summary>
		/// Key in parent child collection.
		/// </summary>
		public abstract string Key
		{
			get;
		}

		/// <summary>
		/// If the node has child nodes or not. If null, the state is undefined, and might need to be checked by consulting with the
		/// back-end service corresponding to the node.
		/// </summary>
		public bool? HasChildren
		{
			get
			{
				if (this.children == null)
					return null;
				else
				{
					lock (this.children)
					{
						return this.children.Count > 0;
					}
				}
			}
		}

		/// <summary>
		/// Children of the node. If null, children are not loaded.
		/// </summary>
		public TreeNode[] Children
		{
			get
			{
				if (this.children == null)
					return null;

				TreeNode[] Children;

				lock (this.children)
				{
					Children = new TreeNode[this.children.Count];
					this.children.Values.CopyTo(Children, 0);
				}

				return Children;
			}
		}

		/// <summary>
		/// Tries to get the child node corresponding to a given key.
		/// </summary>
		/// <param name="ChildKey">Child Key</param>
		/// <param name="Child">Child, if found, or null otherwise.</param>
		/// <returns>If a child with the given key was found.</returns>
		public bool TryGetChild(string ChildKey, out TreeNode Child)
		{
			lock (this.children)
			{
				return this.children.TryGetValue(ChildKey, out Child);
			}
		}

		/// <summary>
		/// Parent node. May be null if a root node.
		/// </summary>
		public TreeNode Parent
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Object tagged to the node.
		/// </summary>
		public object Tag
		{
			get { return this.tag; }
			set { this.tag = value; }
		}

		/// <summary>
		/// Tree Node header text.
		/// </summary>
		public abstract string Header
		{
			get;
		}

		/// <summary>
		/// Disposes of the node and its resources.
		/// </summary>
		public virtual void Dispose()
		{
		}

		/// <summary>
		/// Saves the object to a file.
		/// </summary>
		/// <param name="Output">Output</param>
		public abstract void Write(XmlWriter Output);

		/// <summary>
		/// Image resource for the node.
		/// </summary>
		public abstract ImageSource ImageResource
		{
			get;
		}

		/// <summary>
		/// If the second image resource is visible or not.
		/// </summary>
		public virtual Visibility ImageResourceVisibility
		{
			get { return Visibility.Visible; }
		}

		/// <summary>
		/// Secondary image resource for the node.
		/// </summary>
		public virtual ImageSource ImageResource2
		{
			get { return null; }
		}

		/// <summary>
		/// If the second image resource is visible or not.
		/// </summary>
		public virtual Visibility ImageResource2Visibility
		{
			get { return Visibility.Hidden; }
		}

		/// <summary>
		/// Tool Tip for node.
		/// </summary>
		public abstract string ToolTip
		{
			get;
		}

		/// <summary>
		/// Node Type Name.
		/// </summary>
		public abstract string TypeName
		{
			get;
		}

		/// <summary>
		/// Gets a displayable parameter value.
		/// </summary>
		/// <param name="DisplayableParameter">Name of displayable parameter.</param>
		/// <returns>Parameter value, if exists, or <see cref="string.Empty"/> if not.</returns>
		public virtual string this[string DisplayableParameter]
		{
			get
			{
				if (this.parameters != null)
					return this.parameters[DisplayableParameter];
				else
					return string.Empty;
			}
		}

		public virtual void Add(params Parameter[] Parameters)
		{
			if (this.parameters == null)
				this.parameters = new DisplayableParameters(Parameters);
			else
				this.parameters.AddRange(Parameters);
		}

		/// <summary>
		/// Gets available displayable parameters.
		/// </summary>
		public virtual DisplayableParameters DisplayableParameters
		{
			get { return this.parameters; }
		}

		/// <summary>
		/// Raised when the node has been updated. The sender argument will contain a reference to the node.
		/// </summary>
		public event EventHandler Updated = null;

		/// <summary>
		/// Raises the <see cref="Updated"/> event.
		/// </summary>
		public virtual void OnUpdated()
		{
			this.Raise(this.Updated);
		}

		/// <summary>
		/// If the node is expanded.
		/// </summary>
		public bool IsExpanded
		{
			get { return this.expanded; }
			set
			{
				if (this.expanded != value)
				{
					this.expanded = value;

					if (this.expanded)
						this.OnExpanded();
					else
						this.OnCollapsed();
				}
			}
		}

		/// <summary>
		/// Event raised when the node has been expanded.
		/// </summary>
		public event EventHandler Expanded = null;

		/// <summary>
		/// Event raised when the node has been collapsed.
		/// </summary>
		public event EventHandler Collapsed = null;

		/// <summary>
		/// Raises the <see cref="Expanded"/> event.
		/// </summary>
		protected virtual void OnExpanded()
		{
			this.LoadChildren();
			this.Raise(this.Expanded);
		}

		/// <summary>
		/// Raises the <see cref="Selected"/> event.
		/// </summary>
		protected override void OnSelected()
		{
			this.LoadChildren();
			base.OnSelected();
		}

		protected virtual bool IsLoaded
		{
			get
			{
				if (this.children == null)
					return true;

				lock (this.children)
				{
					return this.children.Count != 1 || !this.children.ContainsKey(string.Empty);
				}
			}
		}

		/// <summary>
		/// Method is called to make sure children are loaded.
		/// </summary>
		protected virtual void LoadChildren()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method is called to notify children can be unloaded.
		/// </summary>
		protected virtual void UnloadChildren()
		{
			// Do nothing by default.
		}

		/// <summary>
		/// Method is called to notify grandchildren can be unloaded.
		/// </summary>
		protected virtual void UnloadGrandchildren()
		{
			if (this.children != null)
			{
				foreach (TreeNode Node in this.children.Values)
					Node.UnloadChildren();
			}
		}

		/// <summary>
		/// Method is called to make sure siblings are loaded.
		/// </summary>
		protected virtual void LoadSiblings()
		{
			if (this.parent != null)
				this.parent.LoadChildren();
		}

		/// <summary>
		/// Raises the <see cref="Collapsed"/> event.
		/// </summary>
		protected virtual void OnCollapsed()
		{
			this.UnloadGrandchildren();
			this.Raise(this.Collapsed);
		}

		/// <summary>
		/// If children can be added to the node.
		/// </summary>
		public abstract bool CanAddChildren
		{
			get;
		}

		/// <summary>
		/// Is called when the user wants to add a node to the current node.
		/// </summary>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void Add()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// If the node can be deleted.
		/// </summary>
		public abstract bool CanDelete
		{
			get;
		}

		/// <summary>
		/// Method called when a node is to be deleted.
		/// </summary>
		/// <param name="Parent">Parent node.</param>
		/// <param name="OnDeleted">Method called when node has been successfully deleted.</param>
		public virtual void Delete(TreeNode Parent, EventHandler OnDeleted)
		{
			Parent?.RemoveChild(this);
			OnDeleted?.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// If the node can be edited.
		/// </summary>
		public abstract bool CanEdit
		{
			get;
		}

		/// <summary>
		/// Is called when the user wants to edit a node.
		/// </summary>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void Edit()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// If the node can be recycled.
		/// </summary>
		public abstract bool CanRecycle
		{
			get;
		}

		/// <summary>
		/// Is called when the user wants to recycle the node.
		/// </summary>
		/// <param name="Window">Window</param>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void Recycle(MainWindow Window)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes a child node.
		/// </summary>
		/// <param name="Node">Child node.</param>
		/// <returns>If the node was found and removed.</returns>
		public virtual bool RemoveChild(TreeNode Node)
		{
			if (this.children != null)
			{
				lock (this.children)
				{
					return this.children.Remove(Node.Key);
				}
			}
			else
				return false;
		}

		internal void RenameChild(string OldKey, string NewKey, TreeNode Node)
		{
			if (this.children != null)
			{
				lock (this.children)
				{
					this.children.Remove(OldKey);
					this.children[NewKey] = Node;
				}
			}
		}

		/// <summary>
		/// If the node can be sniffed.
		/// </summary>
		public virtual bool IsSniffable
		{
			get { return false; }
		}

		/// <summary>
		/// Adds a sniffer to the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer object.</param>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void AddSniffer(ISniffer Sniffer)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes a sniffer from the node.
		/// </summary>
		/// <param name="Sniffer">Sniffer object.</param>
		/// <returns>If the sniffer was found and removed.</returns>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual bool RemoveSniffer(ISniffer Sniffer)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// If it's possible to chat with the node.
		/// </summary>
		public virtual bool CanChat
		{
			get { return false; }
		}

		/// <summary>
		/// Sends a chat message.
		/// </summary>
		/// <param name="Message">Text message to send.</param>
		/// <param name="Markdown">Markdown document, if any, or null if plain text.</param>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void SendChatMessage(string Message, MarkdownDocument Markdown)
		{
			MainWindow.ErrorBox("You are not allowed to chat with this entity.");
		}

		/// <summary>
		/// Is called when the node has been added to the main window.
		/// </summary>
		/// <param name="Window">Window</param>
		public virtual void Added(MainWindow Window)
		{
		}

		/// <summary>
		/// Is called when the node has been removed from the main window.
		/// </summary>
		/// <param name="Window">Window</param>
		public virtual void Removed(MainWindow Window)
		{
		}

		/// <summary>
		/// If it's possible to read sensor data from the node.
		/// </summary>
		public virtual bool CanReadSensorData
		{
			get { return false; }
		}

		/// <summary>
		/// If it's possible to subscribe to sensor data from the node.
		/// </summary>
		public virtual bool CanSubscribeToSensorData
		{
			get { return false; }
		}

		/// <summary>
		/// Starts readout of momentary sensor data values.
		/// </summary>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts readout of all sensor data values.
		/// </summary>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual SensorDataClientRequest StartSensorDataFullReadout()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts subscription of momentary sensor data values.
		/// </summary>
		/// <param name="Rules">Any rules to apply.</param>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual SensorDataSubscriptionRequest SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// If it's possible to configure control parameters on the node.
		/// </summary>
		public virtual bool CanConfigure
		{
			get { return false; }
		}

		/// <summary>
		/// Gets the configuration form for the node.
		/// </summary>
		/// <param name="Callback">Method called when form is returned or when operation fails.</param>
		/// <param name="State">State object to pass on to the callback method.</param>
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual void GetConfigurationForm(DataFormResultEventHandler Callback, object State)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// If it's possible to search for data on the node.
		/// </summary>
		public virtual bool CanSearch
		{
			get { return false; }
		}

		/// <summary>
		/// Performs a search on the node.
		/// </summary>
		public virtual void Search()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Adds context sensitive menu items to a context menu.
		/// </summary>
		/// <param name="CurrentGroup">Current group.</param>
		/// <param name="Menu">Menu being built.</param>
		public virtual void AddContexMenuItems(ref string CurrentGroup, ContextMenu Menu)
		{
			if (this.CanAddChildren)
			{
				CurrentGroup = "Edit";
				Menu.Items.Add(new MenuItem()
				{
					Header = "_Add...",
					IsEnabled = true,
					Command = MainWindow.Add,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Add.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.Parent != null && this.Parent.CanAddChildren)
			{
				CurrentGroup = "Edit";
				Menu.Items.Add(new MenuItem()
				{
					Header = "_Delete...",
					IsEnabled = true,
					Command = MainWindow.Delete,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/delete_32_h.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanRecycle)
			{
				this.GroupSeparator(ref CurrentGroup, "Connection", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "_Refresh",
					IsEnabled = true,
					Command = MainWindow.Refresh,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/refresh_document_16_h.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.IsSniffable)
			{
				this.GroupSeparator(ref CurrentGroup, "Connection", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "_Sniff...",
					IsEnabled = true,
					Command = MainWindow.Sniff,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Spy-icon.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanChat)
			{
				this.GroupSeparator(ref CurrentGroup, "Communication", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "C_hat...",
					IsEnabled = true,
					Command = MainWindow.Chat,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Chat-icon_16.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanReadSensorData)
			{
				this.GroupSeparator(ref CurrentGroup, "Communication", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "Read _Momentary Values...",
					IsEnabled = true,
					Command = MainWindow.ReadMomentary,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/history_16_h.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});

				Menu.Items.Add(new MenuItem()
				{
					Header = "Read _Detailed Values...",
					IsEnabled = true,
					Command = MainWindow.ReadDetailed,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/print_preview_lined_16_h.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanSubscribeToSensorData)
			{
				this.GroupSeparator(ref CurrentGroup, "Communication", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "Su_bscribe to Momentary Values...",
					IsEnabled = true,
					Command = MainWindow.SubscribeToMomentary,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/rss-feed-icon_16.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanConfigure)
			{
				this.GroupSeparator(ref CurrentGroup, "Communication", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "Configure _Parameters...",
					IsEnabled = true,
					Command = MainWindow.Configure,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/Settings-icon_16.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			if (this.CanSearch)
			{
				this.GroupSeparator(ref CurrentGroup, "Database", Menu);
				Menu.Items.Add(new MenuItem()
				{
					Header = "_Search",
					IsEnabled = true,
					Command = MainWindow.Search,
					Icon = new Image()
					{
						Source = new BitmapImage(new Uri("../Graphics/search_16_h.png", UriKind.Relative)),
						Width = 16,
						Height = 16
					}
				});
			}

			TreeNode Loop = this;
			while (Loop != null)
			{
				if (Loop is IMenuAggregator MenuAggregator)
					MenuAggregator.AddContexMenuItems(this, ref CurrentGroup, Menu);

				Loop = Loop.parent;
			}
		}

		protected void GroupSeparator(ref string CurrentGroup, string Group, ContextMenu Menu)
		{
			if (CurrentGroup != Group)
			{
				if (!string.IsNullOrEmpty(CurrentGroup))
					Menu.Items.Add(new MenuItem());

				CurrentGroup = Group;
			}
		}

		public virtual void SelectionChanged()
		{
			// Do nothing by default.
		}

	}
}
