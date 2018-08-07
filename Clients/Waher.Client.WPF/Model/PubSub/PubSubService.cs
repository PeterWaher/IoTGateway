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
		internal bool SupportsAccessAuthorize;
		internal bool SupportsAccessOpen;
		internal bool SupportsAccessPresence;
		internal bool SupportsAccessRoster;
		internal bool SupportsAccessWhitelist;
		internal bool SupportsCollections;
		internal bool SupportsNodeConfiguration;
		internal bool SupportsCreateAndConfigure;
		internal bool SupportsCreateNodes;
		internal bool SupportsDeleteItems;
		internal bool SupportsDeleteNodes;
		internal bool SupportsItemIds;
		internal bool SupportsLastPublished;
		internal bool SupportsLeasedSubscription;
		internal bool SupportsManageSubscriptions;
		internal bool SupportsemberAffiliation;
		internal bool SupportsMetaData;
		internal bool SupportsModifyAffiliations;
		internal bool SupportsMultiCollecton;
		internal bool SupportsMultiItems;
		internal bool SupportsOutcastAffiliation;
		internal bool SupportsPersistentItems;
		internal bool SupportsPresenceSubscribe;
		internal bool SupportsPublish;
		internal bool SupportsPublishOnlyAffiliation;
		internal bool SupportsPublisheAffiliation;
		internal bool SupportsPurgeNodes;
		internal bool SupportsRetractItems;
		internal bool SupportsRetrieveAffiliations;
		internal bool SupportsRetrieveDefault;
		internal bool SupportsRetrieveDefaultSub;
		internal bool SupportsRetrieveItems;
		internal bool SupportsRetrieveSubscriptions;
		internal bool SupportsSubscribe;
		internal bool SupportsSubscriptionOptions;
		internal bool SupportsSubscriptionNotifications;

		public PubSubService(TreeNode Parent, string JID, string Name, string Node, Dictionary<string, bool> Features, PubSubClient PubSubClient)
			: base(Parent, JID, Name, Node, Features)
		{
			this.SupportsAccessAuthorize = Features.ContainsKey(PubSubClient.NamespacePubSubAccessAuthorize);
			this.SupportsAccessOpen = Features.ContainsKey(PubSubClient.NamespacePubSubAccessOpen);
			this.SupportsAccessPresence = Features.ContainsKey(PubSubClient.NamespacePubSubAccessPresence);
			this.SupportsAccessRoster = Features.ContainsKey(PubSubClient.NamespacePubSubAccessRoster);
			this.SupportsAccessWhitelist = Features.ContainsKey(PubSubClient.NamespacePubSubAccessWhitelist);
			this.SupportsCollections = Features.ContainsKey(PubSubClient.NamespacePubSubCollections);
			this.SupportsNodeConfiguration = Features.ContainsKey(PubSubClient.NamespacePubSubNodeConfiguration);
			this.SupportsCreateAndConfigure = Features.ContainsKey(PubSubClient.NamespacePubSubCreateAndConfigure);
			this.SupportsCreateNodes = Features.ContainsKey(PubSubClient.NamespacePubSubCreateNodes);
			this.SupportsDeleteItems = Features.ContainsKey(PubSubClient.NamespacePubSubDeleteItems);
			this.SupportsDeleteNodes = Features.ContainsKey(PubSubClient.NamespacePubSubDeleteNodes);
			this.SupportsItemIds = Features.ContainsKey(PubSubClient.NamespacePubSubItemIds);
			this.SupportsLastPublished = Features.ContainsKey(PubSubClient.NamespacePubSubLastPublished);
			this.SupportsLeasedSubscription = Features.ContainsKey(PubSubClient.NamespacePubSubLeasedSubscription);
			this.SupportsManageSubscriptions = Features.ContainsKey(PubSubClient.NamespacePubSubManageSubscriptions);
			this.SupportsemberAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubemberAffiliation);
			this.SupportsMetaData = Features.ContainsKey(PubSubClient.NamespacePubSubMetaData);
			this.SupportsModifyAffiliations = Features.ContainsKey(PubSubClient.NamespacePubSubModifyAffiliations);
			this.SupportsMultiCollecton = Features.ContainsKey(PubSubClient.NamespacePubSubMultiCollecton);
			this.SupportsMultiItems = Features.ContainsKey(PubSubClient.NamespacePubSubMultiItems);
			this.SupportsOutcastAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubOutcastAffiliation);
			this.SupportsPersistentItems = Features.ContainsKey(PubSubClient.NamespacePubSubPersistentItems);
			this.SupportsPresenceSubscribe = Features.ContainsKey(PubSubClient.NamespacePubSubPresenceSubscribe);
			this.SupportsPublish = Features.ContainsKey(PubSubClient.NamespacePubSubPublish);
			this.SupportsPublishOnlyAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubPublishOnlyAffiliation);
			this.SupportsPublisheAffiliation = Features.ContainsKey(PubSubClient.NamespacePubSubPublisheAffiliation);
			this.SupportsPurgeNodes = Features.ContainsKey(PubSubClient.NamespacePubSubPurgeNodes);
			this.SupportsRetractItems = Features.ContainsKey(PubSubClient.NamespacePubSubRetractItems);
			this.SupportsRetrieveAffiliations = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveAffiliations);
			this.SupportsRetrieveDefault = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveDefault);
			this.SupportsRetrieveDefaultSub = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveDefaultSub);
			this.SupportsRetrieveItems = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveItems);
			this.SupportsRetrieveSubscriptions = Features.ContainsKey(PubSubClient.NamespacePubSubRetrieveSubscriptions);
			this.SupportsSubscribe = Features.ContainsKey(PubSubClient.NamespacePubSubSubscribe);
			this.SupportsSubscriptionOptions = Features.ContainsKey(PubSubClient.NamespacePubSubSubscriptionOptions);
			this.SupportsSubscriptionNotifications = Features.ContainsKey(PubSubClient.NamespacePubSubSubscriptionNotifications);

			this.pubSubClient = PubSubClient;

			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};
		}

		public PubSubClient PubSubClient
		{
			get { return this.pubSubClient; }
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
					else
						MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get root nodes." : e.ErrorText);
				}, null);
			}

			base.LoadChildren();
		}

	}
}
