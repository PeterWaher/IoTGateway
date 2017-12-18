using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Content;
using Waher.Networking.XMPP.Concentrator;
using Waher.Networking.XMPP.Control;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.Sensor;
using Waher.Things;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents a node in a concentrator.
	/// </summary>
	public class Node : TreeNode
	{
		private NodeInformation nodeInfo;
		private DisplayableParameters parameters;

		public Node(TreeNode Parent, NodeInformation NodeInfo)
			: base(Parent)
		{
			this.nodeInfo = NodeInfo;

			if (this.nodeInfo.ParameterList == null)
				this.parameters = null;
			else
				this.parameters = new DisplayableParameters(this.nodeInfo.ParameterList);

			if (nodeInfo.HasChildren)
			{
				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};
			}
		}

		public string NodeId => this.nodeInfo.NodeId;
		public string SourceId => this.nodeInfo.SourceId;
		public string Partition => this.nodeInfo.Partition;

		public override string Key => this.nodeInfo.NodeId;
		public override string Header => this.nodeInfo.LocalId;
		public override string ToolTip => "Node";
		public override string TypeName => this.nodeInfo.NodeType;
		public override bool CanAddChildren => false;
		public override bool CanRecycle => false;
		public override DisplayableParameters DisplayableParameters => this.parameters;

		public override ImageSource ImageResource
		{
			get
			{
				if (this.nodeInfo.HasChildren)
				{
					if (this.IsExpanded)
						return XmppAccountNode.folderOpen;
					else
						return XmppAccountNode.folderClosed;
				}
				else
					return XmppAccountNode.box;
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}

		public XmppConcentrator Concentrator
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is XmppConcentrator Concentrator)
						return Concentrator;

					Loop = Loop.Parent;
				}

				return null;
			}
		}

		private bool loadingChildren = false;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				string FullJid = this.Concentrator?.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					if (this.nodeInfo.HasChildren)
					{
						Mouse.OverrideCursor = Cursors.Wait;

						this.loadingChildren = true;
						Concentrator.XmppAccountNode.ConcentratorClient.GetChildNodes(FullJid, this.nodeInfo, true, true,
							"en", string.Empty, string.Empty, string.Empty, (sender, e) =>
						{
							this.loadingChildren = false;
							MainWindow.currentInstance.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);

							if (e.Ok)
							{
								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (NodeInformation Ref in e.NodesInformation)
									Children[Ref.NodeId] = new Node(this, Ref);

								this.children = Children;

								this.OnUpdated();
								this.Concentrator?.NodesAdded(Children.Values, this);
							}
						}, null);
					}
					else
					{
						if (this.children != null)
							this.Concentrator?.NodesRemoved(this.children.Values, this);

						this.children = null;

						this.OnUpdated();
					}
				}
			}

			base.LoadChildren();
		}

		protected override void UnloadChildren()
		{
			base.UnloadChildren();

			if (this.nodeInfo.HasChildren && (this.children == null || this.children.Count != 1 || !this.children.ContainsKey(string.Empty)))
			{
				if (this.children != null)
					this.Concentrator?.NodesRemoved(this.children.Values, this);

				this.children = new SortedDictionary<string, TreeNode>()
				{
					{ string.Empty, new Loading(this) }
				};

				this.OnUpdated();
			}
		}

		public override bool CanReadSensorData => this.nodeInfo.IsReadable;
		public override bool CanSubscribeToSensorData => this.nodeInfo.IsReadable && this.Concentrator.SupportsEvents;

		public override SensorDataClientRequest StartSensorDataMomentaryReadout()
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
			{
				return SensorClient.RequestReadout(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[] { new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.ParentId) }, FieldType.Momentary);
			}
			else
				return null;
		}

		public override SensorDataClientRequest StartSensorDataFullReadout()
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
			{
				return SensorClient.RequestReadout(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[] { new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.ParentId) }, FieldType.All);
			}
			else
				throw new NotSupportedException();
		}

		public override SensorDataSubscriptionRequest SubscribeSensorDataMomentaryReadout(FieldSubscriptionRule[] Rules)
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			SensorClient SensorClient;

			if (XmppAccountNode != null && (SensorClient = XmppAccountNode.SensorClient) != null)
			{
				return SensorClient.Subscribe(Concentrator.RosterItem.LastPresenceFullJid,
					new ThingReference[] { new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.ParentId) },
					FieldType.Momentary, Rules, new Duration(false, 0, 0, 0, 0, 0, 1), new Duration(false, 0, 0, 0, 0, 1, 0), false);
			}
			else
				return null;
		}

		public override bool CanConfigure => this.nodeInfo.IsControllable;

		public override void GetConfigurationForm(DataFormResultEventHandler Callback, object State)
		{
			XmppConcentrator Concentrator = this.Concentrator;
			XmppAccountNode XmppAccountNode = Concentrator.XmppAccountNode;
			ControlClient ControlClient;

			if (XmppAccountNode != null && (ControlClient = XmppAccountNode.ControlClient) != null)
			{
				ControlClient.GetForm(Concentrator.RosterItem.LastPresenceFullJid, "en", Callback, State,
					new ThingReference(this.nodeInfo.NodeId, this.nodeInfo.SourceId, this.nodeInfo.ParentId));
			}
			else
				throw new NotSupportedException();
		}

	}
}
