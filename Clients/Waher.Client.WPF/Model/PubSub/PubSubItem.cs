using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using System.Windows.Input;
using System.Xml;
using Waher.Content;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.DataForms;
using Waher.Networking.XMPP.PubSub;
using Waher.Networking.XMPP.ServiceDiscovery;
using Waher.Things;
using Waher.Things.DisplayableParameters;
using Waher.Things.SensorData;
using Waher.Client.WPF.Dialogs;

namespace Waher.Client.WPF.Model.PubSub
{
	/// <summary>
	/// Represents a node in a Publish/Subscribe service.
	/// </summary>
	public class PubSubItem : TreeNode
	{
		private DisplayableParameters parameters;
		private string jid;
		private string node;
		private string itemId;

		public PubSubItem(TreeNode Parent, string Jid, string Node, string ItemId)
			: base(Parent)
		{
			this.jid = Jid;
			this.node = Node;
			this.itemId = ItemId;

			List<Parameter> Parameters = new List<Parameter>();

			if (!string.IsNullOrEmpty(this.jid))
				Parameters.Add(new StringParameter("JID", "JID", this.jid));

			if (!string.IsNullOrEmpty(this.node))
				Parameters.Add(new StringParameter("Node", "Node", this.node));

			if (!string.IsNullOrEmpty(this.itemId))
				Parameters.Add(new StringParameter("ItemID", "Item ID", this.itemId));

			this.parameters = new DisplayableParameters(Parameters.ToArray());
		}

		public override string Key => this.itemId;
		public override string Header => this.itemId;
		public override string ToolTip => "Item ID " + this.itemId;
		public override bool CanRecycle => false;
		public override DisplayableParameters DisplayableParameters => this.parameters;

		public override string TypeName
		{
			get
			{
				return "Publish/Subscribe Item";
			}
		}

		public override ImageSource ImageResource
		{
			get
			{
				return XmppAccountNode.box;
			}
		}

		public override void Write(XmlWriter Output)
		{
			// Don't output.
		}


		public PubSubService Service
		{
			get
			{
				TreeNode Loop = this.Parent;

				while (Loop != null)
				{
					if (Loop is PubSubService PubSubService)
						return PubSubService;

					Loop = Loop.Parent;
				}

				return null;
			}
		}
		public override bool CanAddChildren => false;   
		public override bool CanDelete => false;    // TODO
		public override bool CanEdit => true;

		public override void Edit()
		{
			this.Service.PubSubClient.GetItems(this.node, new string[] { this.itemId }, (sender, e) =>
			{
				if (e.Ok)
				{
					foreach (Networking.XMPP.PubSub.PubSubItem Item in e.Items)
					{
					}
				}
				else
					MainWindow.ErrorBox(string.IsNullOrEmpty(e.ErrorText) ? "Unable to get item." : e.ErrorText);

			}, null);
		}

	}
}
