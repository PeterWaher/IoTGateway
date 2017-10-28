using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;
using System.Windows;
using System.Windows.Input;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Concentrator;

namespace Waher.Client.WPF.Model
{
	/// <summary>
	/// Represents an XMPP concentrator.
	/// </summary>
	public class XmppConcentrator : XmppContact
	{
		private Dictionary<string, bool> capabilities = null;

		public XmppConcentrator(TreeNode Parent, XmppClient Client, string BareJid)
			: base(Parent, Client, BareJid)
		{
			this.children = new SortedDictionary<string, TreeNode>()
			{
				{ string.Empty, new Loading(this) }
			};

			this.CheckCapabilities();
		}

		private void CheckCapabilities()
		{
			if (this.capabilities == null)
			{
				string FullJid = this.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					this.XmppAccountNode.ConcentratorClient.GetCapabilities(FullJid, (sender, e) =>
					{
						if (e.Ok)
						{
							Dictionary<string, bool> Capabilities = new Dictionary<string, bool>();

							foreach (string s in e.Capabilities)
								Capabilities[s] = true;

							this.capabilities = Capabilities;
						}
					}, null);
				}
			}
		}

		public override string TypeName
		{
			get { return "Concentrator"; }
		}

		public string FullJid
		{
			get
			{
				XmppAccountNode AccountNode = this.XmppAccountNode;
				if (AccountNode == null || !AccountNode.IsOnline)
					return null;

				RosterItem Item = AccountNode.Client[this.BareJID];
				PresenceEventArgs e = Item?.LastPresence;

				if (e == null || e.Availability == Availability.Offline)
					return null;
				else
					return e.From;
			}
		}

		protected override void OnExpanded()
		{
			if (this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty))
			{
				string FullJid = this.FullJid;

				if (!string.IsNullOrEmpty(FullJid))
				{
					Mouse.OverrideCursor = Cursors.Wait;

					this.XmppAccountNode.ConcentratorClient.GetRootDataSources(FullJid, (sender, e) =>
					{
						Mouse.OverrideCursor = null;

						if (e.Ok)
						{
							SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

							foreach (DataSourceReference Ref in e.DataSources)
								Children[Ref.SourceID] = new DataSource(this, Ref.SourceID, Ref.SourceID, Ref.HasChildren);

							this.children = Children;

							this.OnUpdated();
						}
					}, null);
				}
			}

			base.OnExpanded();
		}

	}
}
