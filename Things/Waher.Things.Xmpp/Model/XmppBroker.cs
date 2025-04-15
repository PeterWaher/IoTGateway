using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.XMPP;
using Waher.Networking.XMPP.Events;

namespace Waher.Things.Xmpp.Model
{
	/// <summary>
	/// Represents an XMPP broker.
	/// </summary>
	public class XmppBroker : IDisposable
	{
		private readonly XmppBrokerNode node;
		private XmppClient xmppClient;
		private readonly string host;
		private readonly int port;
		private readonly bool tls;
		private readonly string userName;
		private readonly string password;
		private readonly string passwordMechanism;
		private readonly bool trustServer = false;
		private readonly bool allowInsecureMechanisms = false;

		/// <summary>
		/// Represents an XMPP broker.
		/// </summary>
		/// <param name="Node">XMPP Broker node.</param>
		/// <param name="Host">Host</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS should be used</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="PasswordMechanism">Password authentication mechanism</param>
		/// <param name="TrustServer">If server should be trusted</param>
		/// <param name="AllowInsecureMechanisms">If insecure mechanisms should be permitted.</param>
		private XmppBroker(XmppBrokerNode Node, string Host, int Port, bool Tls, string UserName, string Password,
			string PasswordMechanism, bool TrustServer, bool AllowInsecureMechanisms)
		{
			this.node = Node;
			this.host = Host;
			this.port = Port;
			this.tls = Tls;
			this.userName = UserName;
			this.password = Password;
			this.passwordMechanism = PasswordMechanism;
			this.trustServer = TrustServer;
			this.allowInsecureMechanisms = AllowInsecureMechanisms;
		}

		/// <summary>
		/// Creates an XMPP broker
		/// </summary>
		/// <param name="Node">XMPP Broker node.</param>
		/// <param name="Host">Host</param>
		/// <param name="Port">Port number</param>
		/// <param name="Tls">If TLS should be used</param>
		/// <param name="UserName">User name</param>
		/// <param name="Password">Password</param>
		/// <param name="PasswordMechanism">Password authentication mechanism</param>
		/// <param name="TrustServer">If server should be trusted</param>
		/// <param name="AllowInsecureMechanisms">If insecure mechanisms should be permitted.</param>
		/// <returns>XMPP broker object instance.</returns>
		public static async Task<XmppBroker> Create(XmppBrokerNode Node, string Host, int Port, bool Tls, string UserName, string Password,
			string PasswordMechanism, bool TrustServer, bool AllowInsecureMechanisms)
		{
			XmppBroker Result = new XmppBroker(Node, Host, Port, Tls, UserName, Password, PasswordMechanism, TrustServer, AllowInsecureMechanisms);

			await Result.Open();

			return Result;
		}

		internal XmppClient Client => this.xmppClient;

		private async Task Open()
		{
			if (string.IsNullOrEmpty(this.passwordMechanism))
			{
				this.xmppClient = new XmppClient(this.host, this.port, this.userName, this.password, "en",
					typeof(XmppBroker).Assembly);
			}
			else
			{
				this.xmppClient = new XmppClient(this.host, this.port, this.userName, this.password, this.passwordMechanism, "en",
					typeof(XmppBroker).Assembly);
			}

			this.xmppClient.TrustServer = this.trustServer;
			this.xmppClient.AllowEncryption = this.tls;
			this.xmppClient.AllowCramMD5 = this.allowInsecureMechanisms;
			this.xmppClient.AllowDigestMD5 = this.allowInsecureMechanisms;
			this.xmppClient.AllowPlain = this.allowInsecureMechanisms;
			this.xmppClient.AllowScramSHA1 = this.allowInsecureMechanisms;

			this.xmppClient.OnStateChanged += this.XmppClient_OnStateChanged;
			this.xmppClient.OnPresence += this.XmppClient_OnPresence;
			this.xmppClient.OnPresenceSubscribe += this.XmppClient_OnPresenceSubscribe;
			this.xmppClient.OnPresenceSubscribed += this.XmppClient_OnPresenceSubscribed;
			this.xmppClient.OnPresenceUnsubscribe += this.XmppClient_OnPresenceUnsubscribe;
			this.xmppClient.OnPresenceUnsubscribed += this.XmppClient_OnPresenceUnsubscribed;
			this.xmppClient.OnRosterItemAdded += this.XmppClient_OnRosterItemAdded;
			this.xmppClient.OnRosterItemRemoved += this.XmppClient_OnRosterItemRemoved;
			this.xmppClient.OnRosterItemUpdated += this.XmppClient_OnRosterItemUpdated;

			foreach (INode Node in await this.node.ChildNodes)
			{
				if (Node is XmppExtensionNode Extension &&
					!Extension.IsRegisteredExtension(this.xmppClient))
				{
					await Extension.RegisterExtension(this.xmppClient);
				}
			}

			await this.xmppClient.Connect();
		}

		private async Task Close()
		{
			if (!(this.xmppClient is null))
			{
				foreach (INode Node in await this.node.ChildNodes)
				{
					if (Node is XmppExtensionNode Extension &&
						Extension.IsRegisteredExtension(this.xmppClient))
					{
						await Extension.UnregisterExtension(this.xmppClient);
					}
				}

				this.xmppClient.OnStateChanged -= this.XmppClient_OnStateChanged;
				this.xmppClient.OnPresence -= this.XmppClient_OnPresence;
				this.xmppClient.OnPresenceSubscribe -= this.XmppClient_OnPresenceSubscribe;
				this.xmppClient.OnPresenceSubscribed -= this.XmppClient_OnPresenceSubscribed;
				this.xmppClient.OnPresenceUnsubscribe -= this.XmppClient_OnPresenceUnsubscribe;
				this.xmppClient.OnPresenceUnsubscribed -= this.XmppClient_OnPresenceUnsubscribed;
				this.xmppClient.OnRosterItemAdded -= this.XmppClient_OnRosterItemAdded;
				this.xmppClient.OnRosterItemRemoved -= this.XmppClient_OnRosterItemRemoved;
				this.xmppClient.OnRosterItemUpdated -= this.XmppClient_OnRosterItemUpdated;

				await this.xmppClient.OfflineAndDisposeAsync();
				this.xmppClient = null;
			}
		}

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			Task.Run(() => this.Close());
		}

		private async Task XmppClient_OnStateChanged(object Sender, XmppState NewState)
		{
			try
			{
				switch (NewState)
				{
					case XmppState.Connected:
						await this.node.RemoveErrorAsync("Offline");
						await this.node.RemoveErrorAsync("Error");
						break;

					case XmppState.Error:
						await this.node.LogErrorAsync("Error", "Connection to broker failed.");
						break;

					case XmppState.Offline:
						await this.node.LogErrorAsync("Offline", "Connection is closed.");
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
			}
		}

		private async Task XmppClient_OnPresenceSubscribe(object Sender, PresenceEventArgs e)
		{
			if (this.node is null)
			{
				await e.Decline();
				return;
			}

			RosterItemNode RosterItem = await this.node.GetRosterItem(e.FromBareJID, false);

			if (string.IsNullOrEmpty(this.node.AutoAcceptPattern))
			{
				await e.Decline();

				if (!(RosterItem is null))
					await RosterItem.LogInformationAsync("Presence subscription received and declined.");

				return;
			}

			try
			{
				Regex Parsed = new Regex(this.node.AutoAcceptPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
				Match M = Parsed.Match(e.FromBareJID);

				if (M.Success && M.Index == 0 && M.Length == e.FromBareJID.Length)
				{
					await e.Accept();

					if (RosterItem is null)
						RosterItem = await this.node.GetRosterItem(e.FromBareJID, true);

					await RosterItem.LogInformationAsync("Presence subscription received and accepted.");
				}
				else
				{
					await e.Decline();

					if (!(RosterItem is null))
						await RosterItem.LogInformationAsync("Presence subscription received and declined.");
				}
			}
			catch (Exception ex)
			{
				await e.Decline();

				if (!(RosterItem is null))
					await RosterItem.LogInformationAsync("Presence subscription received and declined.");

				await this.node.LogErrorAsync("Unable to parse regular expression: " + ex.Message);
			}
		}

		private async Task XmppClient_OnPresenceSubscribed(object Sender, PresenceEventArgs e)
		{
			if (this.node is null)
				return;

			RosterItemNode RosterItem = await this.node.GetRosterItem(e.FromBareJID, true);
			await RosterItem.LogInformationAsync("Subscribed.");
		}

		private async Task XmppClient_OnPresenceUnsubscribe(object Sender, PresenceEventArgs e)
		{
			await e.Accept();

			if (this.node is null)
				return;

			RosterItemNode RosterItem = await this.node.GetRosterItem(e.FromBareJID, false);

			if (!(RosterItem is null))
			{
				await RosterItem.LogInformationAsync("Presence unsubscribed by " + e.FromBareJID);
				await RosterItem.DestroyAsync();
			}
		}

		private async Task XmppClient_OnPresenceUnsubscribed(object Sender, PresenceEventArgs e)
		{
			if (this.node is null)
				return;

			RosterItemNode RosterItem = await this.node.GetRosterItem(e.FromBareJID, false);

			if (!(RosterItem is null))
			{
				await RosterItem.LogInformationAsync("Unsubscribed from " + e.FromBareJID);
				await RosterItem.DestroyAsync();
			}
		}

		private Task XmppClient_OnPresence(object Sender, PresenceEventArgs e)
		{
			return Task.CompletedTask;
		}

		private async Task XmppClient_OnRosterItemAdded(object Sender, RosterItem Item)
		{
			if (this.node is null)
				return;

			RosterItemNode Node = await this.node.GetRosterItem(Item.BareJid, true);
			bool Changed =
				Node.SubscriptionState != Item.State ||
				Node.ContactName != Item.Name ||
				Node.PendingSubscription != Item.PendingSubscription ||
				!AreSame(Node.Groups, Item.Groups);

			if (Changed)
			{
				Node.SubscriptionState = Item.State;
				Node.ContactName = Item.Name;
				Node.PendingSubscription = Item.PendingSubscription;
				Node.Groups = Item.Groups ?? Array.Empty<string>();

				await Node.UpdateAsync();
			}
		}

		private static bool AreSame(string[] A1, string[] A2)
		{
			if ((A1 is null) ^ (A2 is null))
				return false;

			if (A1 is null)
				return true;

			int c = A1.Length;
			if (c != A2.Length)
				return false;

			int i;

			for (i = 0; i < c; i++)
			{
				if (A1[i] != A2[i])
					return false;
			}

			return true;
		}

		private Task XmppClient_OnRosterItemUpdated(object Sender, RosterItem Item)
		{
			return this.XmppClient_OnRosterItemAdded(Sender, Item);
		}

		private async Task XmppClient_OnRosterItemRemoved(object Sender, RosterItem Item)
		{
			if (this.node is null)
				return;

			RosterItemNode Node = await this.node.GetRosterItem(Item.BareJid, false);
			if (Node is null)
				return;

			await Node.DestroyAsync();
		}
	}
}
