using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.LWM2M;

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
				false, false, new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal, LineEnding.NewLine));

			this.lwm2mClient = await Lwm2mClient.Create("Lwm2mTestClient", this.coapClient,
				new Lwm2mSecurityObject(),
				new Lwm2mServerObject(),
				new Lwm2mAccessControlObject(),
				new Lwm2mDeviceObject("Waher Data AB", "Unit Test", Environment.MachineName,
					Environment.OSVersion.VersionString, "PC", Environment.OSVersion.Platform.ToString(),
					Environment.Version.ToString()));

			await this.lwm2mClient.LoadBootstrapInfo();
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			if (this.lwm2mClient is not null)
			{
				await this.lwm2mClient.DisposeAsync();
				this.lwm2mClient = null;
			}

			if (this.coapClient is not null)
			{
				CoapResource[] Resources = this.coapClient.GetRegisteredResources();

				this.coapClient.Dispose();
				this.coapClient = null;

				Assert.AreEqual(1, Resources.Length, "There are resources still registered on the CoAP client.");
			}
		}

		[TestMethod]
		public async Task LWM2M_Bootstrap_Test_01_BootstrapRequest_Explicit()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);
			ManualResetEvent Done3 = new(false);
			ManualResetEvent Error3 = new(false);

			this.lwm2mClient.OnBootstrapCompleted += (Sender, e) =>
			{
				Done2.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnBootstrapFailed += (Sender, e) =>
			{
				Error2.Set();
				return Task.CompletedTask;
			};

			this.lwm2mClient.OnRegistrationSuccessful += (Sender, e) =>
			{
				Done3.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnRegistrationFailed += (Sender, e) =>
			{
				Error3.Set();
				return Task.CompletedTask;
			};

			await this.lwm2mClient.RequestBootstrap(
				new Lwm2mServerReference("leshan.eclipse.org", 5783),
				(Sender, e) =>
				{
					if (e.Ok)
						Done.Set();
					else
						Error.Set();

					return Task.CompletedTask;
				}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done3, Error3 }, 10000));
		}

		[TestMethod]
		[Ignore("Bootstrap URI not configured correctly by the Leshan demo server.")]
		public async Task LWM2M_Bootstrap_Test_02_BootstrapRequest_LastServer()
		{
			ManualResetEvent Done = new(false);
			ManualResetEvent Error = new(false);
			ManualResetEvent Done2 = new(false);
			ManualResetEvent Error2 = new(false);
			ManualResetEvent Done3 = new(false);
			ManualResetEvent Error3 = new(false);

			this.lwm2mClient.OnBootstrapCompleted += (Sender, e) =>
			{
				Done2.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnBootstrapFailed += (Sender, e) =>
			{
				Error2.Set();
				return Task.CompletedTask;
			};

			this.lwm2mClient.OnRegistrationSuccessful += (Sender, e) =>
			{
				Done3.Set();
				return Task.CompletedTask;
			};
			this.lwm2mClient.OnRegistrationFailed += (Sender, e) =>
			{
				Error3.Set();
				return Task.CompletedTask;
			};

			Assert.IsTrue(await this.lwm2mClient.RequestBootstrap((Sender, e) =>
			{
				if (e.Ok)
					Done.Set();
				else
					Error.Set();

				return Task.CompletedTask;
			}, null));

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 10000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000));
			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done3, Error3 }, 10000));
		}

	}
}
