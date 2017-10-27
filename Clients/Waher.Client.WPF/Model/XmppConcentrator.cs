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
				{ string.Empty, new DataSource(this, string.Empty, "Loading...", false) }
			};
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
			string FullJid = this.FullJid;
			if (string.IsNullOrEmpty(FullJid))
			{
				base.OnExpanded();
				return;
			}

			bool LoadChildren = this.children != null && this.children.Count == 1 && this.children.ContainsKey(string.Empty);

			if (this.capabilities == null || LoadChildren)
			{
				ManualResetEvent Done1 = new ManualResetEvent(this.capabilities != null);
				ManualResetEvent Done2 = new ManualResetEvent(!LoadChildren);

				try
				{
					Mouse.OverrideCursor = Cursors.Wait;

					if (this.capabilities == null)
					{
						this.XmppAccountNode.ConcentratorClient.GetCapabilities(FullJid, (sender, e) =>
						{
							if (e.Ok)
							{
								this.capabilities = new Dictionary<string, bool>();

								foreach (string s in e.Capabilities)
									this.capabilities[s] = true;
							}

							Done1.Set();
						}, null);
					}

					if (LoadChildren)
					{
						this.XmppAccountNode.ConcentratorClient.GetRootDataSources(FullJid, (sender, e) =>
						{
							if (e.Ok)
							{
								this.capabilities = new Dictionary<string, bool>();

								SortedDictionary<string, TreeNode> Children = new SortedDictionary<string, TreeNode>();

								foreach (DataSourceReference Ref in e.DataSources)
									Children[Ref.SourceID] = new DataSource(this, Ref.SourceID, Ref.SourceID, Ref.HasChildren);

								this.children = Children;
							}

							Done2.Set();
						}, null);
					}

					if (!Done1.WaitOne(10000) || !Done2.WaitOne(10000))
					{
						base.OnExpanded();
						return;
					}
				}
				finally
				{
					Done1.Dispose();
					Done1 = null;

					Done2.Dispose();
					Done2 = null;

					Mouse.OverrideCursor = null;
				}
			}

			base.OnExpanded();
		}

	}
}
