using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP;

namespace Waher.Networking.CoAP.Test
{
	[TestFixture]
	public class CoapTests
	{
		private CoapClient client;

		[SetUp]
		public void Setup()
		{
			this.client = new CoapClient(new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
		}

		[TearDown]
		public void TearDown()
		{
			this.client.Dispose();
			this.client = null;
		}

		[Test]
		public async Task Test_01_GET()
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			await this.client.GET("vs0.inf.ethz.ch", 5683, true, (sender, e) =>
			{
				if (e.Ok)
					Done.Set();
				else
					Error.Set();
			}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
		}
	}
}
