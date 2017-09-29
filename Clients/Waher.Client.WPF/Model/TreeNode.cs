using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using Waher.Content.Markdown;
using Waher.Events;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Sensor;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Abstract base class for tree nodes in the connection view.
	/// </summary>
	public abstract class TreeNode : SelectableItem, IDisposable
	{
		private TreeNode parent;
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
			this.Raise(this.Expanded);
		}

		/// <summary>
		/// Raises the <see cref="Collapsed"/> event.
		/// </summary>
		protected virtual void OnCollapsed()
		{
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
		public virtual bool Delete(TreeNode Node)
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
			throw new NotSupportedException();
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
		/// <exception cref="NotSupportedException">If the feature is not supported by the node.</exception>
		public virtual SensorDataClientRequest SubscribeSensorDataMomentaryReadout()
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

	}
}
