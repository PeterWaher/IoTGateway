using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppRosterTests : CommunicationTests
	{
		[TestMethod]
		public void Roster_Test_01_GetRoster()
		{
			this.ConnectClients();
			Assert.IsTrue(this.client1.HasRoster);
			Assert.IsTrue(this.client2.HasRoster);
		}

		[TestMethod]
		public void Roster_Test_02_AddRosterItem()
		{
			this.ConnectClients();
			using (ManualResetEvent Added = new ManualResetEvent(false))
			{
				this.client1.AddRosterItem(new RosterItem(this.client2.BareJID, "Test Client 2", "Test Clients"),
					(sender, e) => Added.Set(), null);

				Assert.IsTrue(Added.WaitOne(10000), "Roster item not properly added.");
			}
		}

		[TestMethod]
		public void Roster_Test_03_UpdateRosterItem()
		{
			this.ConnectClients();
			using (ManualResetEvent Updated = new ManualResetEvent(false))
			{
				this.client1.UpdateRosterItem(this.client2.BareJID, "Test Client II", new string[] { "Test Clients" },
					(sender, e) => Updated.Set(), null);

				Assert.IsTrue(Updated.WaitOne(10000), "Roster item not properly updated.");
			}
		}

		[TestMethod]
		public void Roster_Test_04_RemoveRosterItem()
		{
			this.ConnectClients();
			using (ManualResetEvent Removed = new ManualResetEvent(false))
			{
				this.client1.RemoveRosterItem(this.client2.BareJID, (sender, e) => Removed.Set(), null);

				Assert.IsTrue(Removed.WaitOne(10000), "Roster item not properly removed.");
			}
		}

		[TestMethod]
		public void Roster_Test_05_AcceptPresenceSubscription()
		{
			this.ConnectClients();
			ManualResetEvent Done = new ManualResetEvent(false);

			this.client2.OnPresenceSubscribe += (sender, e) => e.Accept();
			this.client1.OnPresenceSubscribed += (sender, e) => Done.Set();

			this.client1.RequestPresenceSubscription(this.client2.BareJID);

			Assert.IsTrue(Done.WaitOne(10000), "Presence subscription failed.");
		}

		[TestMethod]
		public void Roster_Test_06_AcceptPresenceUnsubscription()
		{
			this.ConnectClients();
			ManualResetEvent Done = new ManualResetEvent(false);

			this.client2.OnPresenceUnsubscribe += (sender, e) => e.Accept();
			this.client1.OnPresenceUnsubscribed += (sender, e) => Done.Set();

			this.client1.OnPresenceSubscribe += (sender, e) => e.Decline();
			this.client2.OnPresenceSubscribe += (sender, e) => e.Decline();

			this.client1.RequestPresenceUnsubscription(this.client2.BareJID);

			Assert.IsTrue(Done.WaitOne(10000), "Presence unsubscription failed.");
		}

		[TestMethod]
		public void Roster_Test_07_FederatedSubscriptionRequest()
		{
			this.ConnectClients();
			ManualResetEvent Done = new ManualResetEvent(false);

			this.client1.OnPresenceSubscribed += (sender, e) => Done.Set();
			//this.client1.RequestPresenceSubscription("energymeter@lsys.p1.im");
			this.client1.RequestPresenceSubscription("viessmann_ostermalm_1@jabber.se");

			Assert.IsTrue(Done.WaitOne(10000), "Presence subscription failed.");
		}

	}
}
