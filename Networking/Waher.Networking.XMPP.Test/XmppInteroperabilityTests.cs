using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Interoperability;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppInteroperabilityTests : CommunicationTests
	{
		private InteroperabilityClient interopClient;
		private InteroperabilityServer interopServer;

		public override void ConnectClients()
		{
			base.ConnectClients();

			Assert.AreEqual(XmppState.Connected, this.client1.State);
			Assert.AreEqual(XmppState.Connected, this.client2.State);

			this.interopClient = new InteroperabilityClient(this.client1);
			this.interopServer = new InteroperabilityServer(this.client2);

			this.interopServer.OnGetInterfaces += (sender, e) =>
			{
				e.Add("Interface A", "Interface B", "Interface C", "Interface D");
				return Task.CompletedTask;
			};
		}

		public override void DisposeClients()
		{
			this.interopServer.Dispose();
			this.interopServer = null;

			this.interopClient.Dispose();
			this.interopClient = null;

			base.DisposeClients();
		}

		[TestMethod]
		public void Interoperability_Test_01_GetInterfaces()
		{
			this.ConnectClients();
			try
			{
				string[] Interfaces = this.interopClient.GetInterfaces(this.client2.FullJID, 10000);

				Assert.AreEqual(4, Interfaces.Length);
				Assert.AreEqual("Interface A", Interfaces[0]);
				Assert.AreEqual("Interface B", Interfaces[1]);
				Assert.AreEqual("Interface C", Interfaces[2]);
				Assert.AreEqual("Interface D", Interfaces[3]);
			}
			finally
			{
				this.DisposeClients();
			}
		}
	}
}
