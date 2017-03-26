using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP;
using Waher.Networking.CoAP.CoRE;

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

		private async Task<object> Get(string Uri)
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

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
			Assert.IsNotNull(Result);

			Console.Out.WriteLine(Result.ToString());

			return Result;
		}

		[Test]
		public async Task Test_01_GET()
		{
			await this.Get("coap://vs0.inf.ethz.ch/");
		}

		[Test]
		public async Task Test_02_Discover()
		{
			LinkDocument Doc = await this.Get("coap://vs0.inf.ethz.ch/.well-known/core") as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[Test]
		public async Task Test_03_Separate()
		{
			// Resource which cannot be served immediately and which cannot be acknowledged in a piggy-backed way
			await this.Get("coap://vs0.inf.ethz.ch/separate");
		}

		[Test]
		public async Task Test_04_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1");
		}

		[Test]
		public async Task Test_05_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1/seg2");
		}

		[Test]
		public async Task Test_06_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1/seg2/seg3");
		}

		[Test]
		public async Task Test_07_Large()
		{
			// Large resource
			await this.Get("coap://vs0.inf.ethz.ch/large");
		}

		[Test]
		public async Task Test_08_LargeSeparate()
		{
			// Large resource
			await this.Get("coap://vs0.inf.ethz.ch/large-separate");
		}

		/*
		</obs>;obs;rt="observe";title="Observable resource which changes every 5 seconds",
		</obs-pumping>;obs;rt="observe";title="Observable resource which changes every 5 seconds",
		</large-create>;rt="block";title="Large resource that can be created using POST method",
		</obs-reset>,
		</multi-format>;ct="0 41";title="Resource that exists in different content formats (text/plain utf8 and application/xml)",
		</path>;ct=40;title="Hierarchical link description entry",
		</path/sub1>;title="Hierarchical link description sub-resource",
		</path/sub2>;title="Hierarchical link description sub-resource",
		</path/sub3>;title="Hierarchical link description sub-resource",
		</link1>;if="If1";rt="Type1 Type2";title="Link test resource",
		</link3>;if="foo";rt="Type1 Type3";title="Link test resource",
		</link2>;if="If2";rt="Type2 Type3";title="Link test resource",
		</obs-large>;obs;rt="observe";title="Observable resource which changes every 5 seconds",
		</validate>;title="Resource which varies",
		</test>;title="Default test resource",
		</obs-pumping-non>;obs;rt="observe";title="Observable resource which changes every 5 seconds",
		</query>;title="Resource accepting query parameters",
		</large-post>;rt="block";title="Handle POST with two-way blockwise transfer",
		</location-query>;title="Perform POST transaction with responses containing several Location-Query options (CON mode)",
		</obs-non>;obs;rt="observe";title="Observable resource which changes every 5 seconds",
		</large-update>;rt="block";sz=1280;title="Large resource that can be updated using PUT method",
		</shutdown>
		*/
	}
}
