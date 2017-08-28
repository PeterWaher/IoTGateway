using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;
using Waher.Networking.CoAP.LWM2M;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mBootstrapTests
	{
		private CoapEndpoint coapClient;
		private Lwm2mClient lwm2mClient;

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.coapClient = new CoapEndpoint(new int[] { 5783 }, new int[] { 5784 }, null, null, 
				false, false, new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));

			this.lwm2mClient = new Lwm2mClient("Lwm2mTestClient", this.coapClient,
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject());

			await this.lwm2mClient.LoadBootstrapInfo();
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.lwm2mClient != null)
			{
				this.lwm2mClient.Dispose();
				this.lwm2mClient = null;
			}

			if (this.coapClient != null)
			{
				CoapResource[] Resources = this.coapClient.GetRegisteredResources();
				ulong[] Tokens = this.coapClient.GetActiveTokens();
				ushort[] MessageIDs = this.coapClient.GetActiveMessageIDs();

				this.coapClient.Dispose();
				this.coapClient = null;

				Assert.AreEqual(0, Tokens.Length, "There are tokens that have not been unregistered properly.");
				Assert.AreEqual(0, MessageIDs.Length, "There are message IDs that have not been unregistered properly.");
				Assert.AreEqual(1, Resources.Length, "There are resources still registered on the CoAP client.");
			}
		}

		[TestMethod]
		public async Task LWM2M_Bootstrap_Test_01_BootstrapRequest_Explicit()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			await this.lwm2mClient.RequestBootstrap(
				new Lwm2mServerReference("leshan.eclipse.org", 5783),
				(sender, e)=>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();
				}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));
			Thread.Sleep(5000);
		}

		[TestMethod]
		[Ignore("Bootstrap URI not configured correctly by the Leshan demo server.")]
		public async Task LWM2M_Bootstrap_Test_02_BootstrapRequest_LastServer()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			Assert.IsTrue(await this.lwm2mClient.RequestBootstrap((sender, e) =>
			{
				if (e.Ok)
					Done.Set();
				else
					Error.Set();
			}, null));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));
			Thread.Sleep(5000);
		}

	}
}
