using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP;

namespace Waher.Networking.XMPP.Test
{
	[TestFixture]
	public class RosterTests : CommunicationTests
	{
		[Test]
		public void Test_01_GetRoster()
		{
			Assert.IsTrue(this.client1.HasRoster);
			Assert.IsTrue(this.client2.HasRoster);
		}

		[Test]
		public void Test_02_AddRosterItem()
		{
			using (ManualResetEvent Added = new ManualResetEvent(false))
			{
				this.client1.AddRosterItem(new RosterItem(this.client2.BaseJID, "Test Client 2", "Test Clients"),
					(sender, e) => Added.Set(), null);

				Assert.IsTrue(Added.WaitOne(10000), "Roster item not properly added.");
			}
		}

		[Test]
		public void Test_03_UpdateRosterItem()
		{
			using (ManualResetEvent Updated = new ManualResetEvent(false))
			{
				this.client1.UpdateRosterItem(this.client2.BaseJID, "Test Client II", new string[] { "Test Clients" },
					(sender, e) => Updated.Set(), null);

				Assert.IsTrue(Updated.WaitOne(10000), "Roster item not properly updated.");
			}
		}

		[Test]
		public void Test_04_RemoveRosterItem()
		{
			using (ManualResetEvent Removed = new ManualResetEvent(false))
			{
				this.client1.RemoveRosterItem(this.client2.BaseJID, (sender, e) => Removed.Set(), null);

				Assert.IsTrue(Removed.WaitOne(10000), "Roster item not properly removed.");
			}
		}

		[Test]
		public void Test_05_AcceptPresenceSubscription()
		{
			ManualResetEvent Done = new ManualResetEvent(false);

			this.client2.OnPresenceSubscribe += (sender, e) => e.Accept();
			this.client1.OnPresenceSubscribed += (sender, e) => Done.Set();

			this.client1.RequestPresenceSubscription(this.client2.BaseJID);

			Assert.IsTrue(Done.WaitOne(10000), "Presence subscription failed.");
		}

		[Test]
		public void Test_06_AcceptPresenceUnsubscription()
		{
			ManualResetEvent Done = new ManualResetEvent(false);

			this.client2.OnPresenceUnsubscribe += (sender, e) => e.Accept();
			this.client1.OnPresenceUnsubscribed += (sender, e) => Done.Set();

			this.client1.OnPresenceSubscribe += (sender, e) => e.Decline();
			this.client2.OnPresenceSubscribe += (sender, e) => e.Decline();

			this.client1.RequestPresenceUnsubscription(this.client2.BaseJID);

			Assert.IsTrue(Done.WaitOne(10000), "Presence unsubscription failed.");
		}

	}
}
