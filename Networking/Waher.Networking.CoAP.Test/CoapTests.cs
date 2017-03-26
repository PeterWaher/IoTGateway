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

		private async Task Get(string Uri)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			object Result = null;

			await this.client.GET(Uri, true, (sender, e) =>
			{
				if (e.Ok)
				{
					Result = e.Message.Decode();
					Done.Set();
				}
				else
					Error.Set();
			}, null);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 15000));
			Assert.IsNotNull(Result);

			Console.Out.WriteLine(Result.ToString());
		}

		[Test]
		public async Task Test_01_GET()
		{
			await this.Get("coap://vs0.inf.ethz.ch/");
		}

		[Test]
		public async Task Test_02_Discover()
		{
			await this.Get("coap://vs0.inf.ethz.ch/.well-known/core");
		}
	}
}
