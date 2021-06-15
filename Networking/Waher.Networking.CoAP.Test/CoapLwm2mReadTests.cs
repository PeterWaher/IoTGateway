using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.LWM2M;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mReadTests
	{
		private CoapEndpoint coapClient;
		private Lwm2mClient lwm2mClient;

		[TestInitialize]
		public async Task TestInitialize()
		{
			this.coapClient = new CoapEndpoint(new int[] { CoapEndpoint.DefaultCoapPort },
				new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, false,
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));

			this.lwm2mClient = new Lwm2mClient("Lwm2mTestClient", this.coapClient,
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject("Waher Data AB", "Unit Test", Environment.MachineName,
					Environment.OSVersion.VersionString, "PC", Environment.OSVersion.Platform.ToString(),
					Environment.Version.ToString()));
			
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

				this.coapClient.Dispose();
				this.coapClient = null;

				Assert.AreEqual(1, Resources.Length, "There are resources still registered on the CoAP client.");
			}
		}

		[TestMethod]
		public void LWM2M_Read_Test_01_WaitForReboot()
		{
			ManualResetEvent Done = new ManualResetEvent(false);

			this.lwm2mClient.OnRebootRequest += (sender, e) => Done.Set();
			this.lwm2mClient.RequestBootstrap(new Lwm2mServerReference("leshan.eclipse.org", 5783));

			Done.WaitOne(int.MaxValue);
		}


	}
}
