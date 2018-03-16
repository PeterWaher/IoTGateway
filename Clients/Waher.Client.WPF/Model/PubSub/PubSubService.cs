using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.DataForms.FieldTypes;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.ServiceDiscovery;

namespace Waher.Client.WPF.Model.PubSub
{
	public class PubSubService : XmppComponent
	{
		private PubSubClient pubSubClient;
		private bool supportsAccessAuthorize;
		private bool supportsAccessOpen;
		private bool supportsAccessPresence;
		private bool supportsAccessRoster;
		private bool supportsAccessWhitelist;
		private bool supportsCollections;
		private bool supportsNodeConfiguration;
		private bool supportsCreateAndConfigure;
		private bool supportsCreateNodes;
		private bool supportsDeleteItems;
		private bool supportsDeleteNodes;
		private bool supportsItemIds;
		private bool supportsLastPublished;
		private bool supportsLeasedSubscription;
		private bool supportsManageSubscriptions;
		private bool supportsemberAffiliation;
		private bool supportsMetaData;
		private bool supportsModifyAffiliations;
		private bool supportsMultiCollecton;
		private bool supportsMultiItems;
		private bool supportsOutcastAffiliation;
		private bool supportsPersistentItems;
		private bool supportsOresenceSubscribe;
		private bool supportsPublish;
		private bool supportsPublishOnlyAffiliation;
		private bool supportsPublisheAffiliation;
		private bool supportsPurgeNodes;
		private bool supportsRetractItems;
		private bool supportsRetrieveAffiliations;
		private bool supportsRetrieveDefault;
		private bool supportsRetrieveDefaultSub;
		private bool supportsRetrieveItems;
		private bool supportsRetrieveSubscriptions;
		private bool supportsSubscribe;
		private bool supportsSubscriptionOptions;
		private bool supportsSubscriptionNotifications;

		public PubSubService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features)
			: base(Parent, JID, Name, Node, Features)
		{
			this.supportsAccessAuthorize = Features.ContainsKey(PubSubClient.NamespacePubSubAccessAuthorize);
			this.supportsAccessOpen = Features.ContainsKey(PubSubClient.NamespacePubSubAccessOpen);
			this.supportsAccessPresence = Features.ContainsKey(PubSubClient.NamespacePubSubAccessPresence);
			this.supportsAccessRoster = Features.ContainsKey(PubSubClient.NamespacePubSubAccessRoster);
			this.supportsAccessWhitelist = Features.ContainsKey(PubSubClient.NamespacePubSubAccessWhitelist);
			this.supportsCollections = Features.ContainsKey(PubSubClient.NamespacePubSubCollections);
			this.supportsNodeConfiguration = Features.ContainsKey(PubSubClient.NamespacePubSubNodeConfiguration);
			this.supportsCreateAndConfigure = Features.ContainsKey(PubSubClient.NamespacePubSubCreateAndConfigure);
			this.supportsCreateNodes = Features.ContainsKey(PubSubClient.NamespacePubSubCreateNodes);
			this.supportsDeleteItems = Features.ContainsKey(PubSubClient.NamespacePubSubDeleteItems);
			this.supportsDeleteNodes = Features.ContainsKey(PubSubClient.NamespacePubSubDeleteNodes);
			this.supportsItemIds = Features.ContainsKey(PubSubClient.NamespacePubSubItemIds);
			this.supportsLastPublished = Features.ContainsKey(PubSubClient.NamespacePubSubLastPublished);
			this.supportsLeasedSubscription = Features.ContainsKey(PubSubClient.NamespacePubSubLeasedSubscription);
			this.supportsManageSubscriptions = Features.ContainsKey(PubSubClient.NamespacePubSubManageSubscriptions);
			this.supportsemberAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubemberAffiliation);
			this.supportsMetaData = Features.ContainsKey(PubSubClient.NamespacePubSubMetaData);
			this.supportsModifyAffiliations = Features.ContainsKey(PubSubClient.NamespacePubSubModifyAffiliations);
			this.supportsMultiCollecton = Features.ContainsKey(PubSubClient.NamespacePubSubMultiCollecton);
			this.supportsMultiItems = Features.ContainsKey(PubSubClient.NamespacePubSubMultiItems);
			this.supportsOutcastAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubOutcastAffiliation);
			this.supportsPersistentItems = Features.ContainsKey(PubSubClient.NamespacePubSubPersistentItems);
			this.supportsOresenceSubscribe = Features.ContainsKey(PubSubClient.NamespacePubSubOresenceSubscribe);
			this.supportsPublish = Features.ContainsKey(PubSubClient.NamespacePubSubPublish);
			this.supportsPublishOnlyAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubPublishOnlyAffiliation);
			this.supportsPublisheAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubPublisheAffiliation);
			this.supportsPurgeNodes = Features.ContainsKey(PubSubClient.NamespacePubSubPurgeNodes);
			this.supportsRetractItems = Features.ContainsKey(PubSubClient.NamespacePubSubRetractItems);
			this.supportsRetrieveAffiliations = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveAffiliations);
			this.supportsRetrieveDefault = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveDefault);
			this.supportsRetrieveDefaultSub = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveDefaultSub);
			this.supportsRetrieveItems = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveItems);
			this.supportsRetrieveSubscriptions = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveSubscriptions);
			this.supportsSubscribe = Features.ContainsKey(PubSubClient.NamespacePubSubSubscribe);
			this.supportsSubscriptionOptions = Features.ContainsKey(PubSubClient.NamespacePubSubSubscriptionOptions);
			this.supportsSubscriptionNotifications = Features.ContainsKey(PubSubClient.NamespacePubSubSubscriptionNotifications);

			this.pubSubClient = new PubSubClient(this.Account.Client, JID);

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};
		}

		public PubSubClient PubSubClient
		{
			get { return this.pubSubClient; }
		}

		public override void Dispose()
		{
			if (this.pubSubClient != null)
			{
				this.pubSubClient.Dispose();
				this.pubSubClient = null;
			}

			base.Dispose();
		}

		public override ImageSource ImageResource => XmppAccountNode.database;

		public override string ToolTip
		{
			get
			{
				return "Publish/Subscribe Service";
			}
		}

		private bool loadingChildren = false;

		protected override void LoadChildren()
		{
			if (!this.loadingChildren && this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				Mouse.OverrideCursor = Cursors.Wait;

				this.loadingChildren = true;
				this.Account.Client.SendServiceItemsDiscoveryRequest(this.pubSubClient.ComponentAddress, (sender, e) =>
				{
					this.loadingChildren = false;
					MainWindow.MouseDefault();

					if (e.Ok)
					{
						SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

						this.NodesRemoved(this.children.Values, this);

						foreach (Item Item in e.Items)
						{
							this.Account.Client.SendServiceDiscoveryRequest(this.PubSubClient.ComponentAddress, Item.Node, (sender2, e2) =>
							{
								if (e2.Ok)
								{
									Item Item2 = (Item)e2.State;
									string Jid = Item2.JID;
									string Node = Item2.Node;
									string Name = Item2.Name;
									NodeType NodeType = NodeType.leaf;
									PubSubNode NewNode;

									foreach (Identity Identity in e2.Identities)
									{
										if (Identity.Category == "pubsub")
										{
											if (!Enum.TryParse<NodeType>(Identity.Type, out NodeType))
												NodeType = NodeType.leaf;

											if (!string.IsNullOrEmpty(Identity.Name))
												Name = Identity.Name;
										}
									}

									lock (Children)
									{
										Children[Item2.Node] = NewNode = new PubSubNode(this, Jid, Node, Name, NodeType);
										this.children = new SortedDictionary<string, TreeNode>(Children);
									}

									this.OnUpdated();
									this.NodesAdded(new TreeNode[] { NewNode }, this);
								}
							}, Item);
						}
					}
				}, null);
			}

			base.LoadChildren();
		}

	}
}
