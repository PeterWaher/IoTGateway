using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;

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
			ulong[] Tokens = this.client.GetActiveTokens();
			ushort[] MessageIDs = this.client.GetActiveMessageIDs();

			this.client.Dispose();
			this.client = null;

			Assert.AreEqual(0, Tokens.Length, "There are tokens that have not been unregistered properly.");
			Assert.AreEqual(0, MessageIDs.Length, "There are message IDs that have not been unregistered properly.");
		}

		private async Task<object> Get(string Uri, params CoapOption[] Options)
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
			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
			Assert.IsNotNull(Result);

			Console.Out.WriteLine(Result.ToString());

			return Result;
		}

		private async Task<object> Observe(string Uri, params CoapOption[] Options)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			object Result = null;
			ulong Token = 0;
			int Count = 0;

			await this.client.Observe(Uri, true, (sender, e) =>
			{
				if (e.Ok)
				{
					Token = e.Message.Token;
					Result = e.Message.Decode();
					Console.Out.WriteLine(Result.ToString());

					Count++;
					if (Count == 3)
						Done.Set();
				}
				else
					Error.Set();
			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
			Assert.IsNotNull(Result);

			Done.Reset();

			await this.client.UnregisterObservation(Uri, true, Token, (sender, e) =>
			{
				if (e.Ok)
					Done.Set();
				else
					Error.Set();

			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));

			return Result;
		}

		private async Task Post(string Uri, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			await this.client.POST(Uri, true, Payload, BlockSize, (sender, e) =>
			{
				if (e.Ok)
				{
					object Result = e.Message.Decode();
					if (Result != null)
						Console.Out.WriteLine(Result.ToString());

					Done.Set();
				}
				else
					Error.Set();
			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
		}

		private async Task Put(string Uri, byte[] Payload, int BlockSize, params CoapOption[] Options)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			await this.client.PUT(Uri, true, Payload, BlockSize, (sender, e) =>
			{
				if (e.Ok)
				{
					object Result = e.Message.Decode();
					if (Result != null)
						Console.Out.WriteLine(Result.ToString());

					Done.Set();
				}
				else
					Error.Set();
			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
		}

		private async Task Delete(string Uri, params CoapOption[] Options)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);

			await this.client.DELETE(Uri, true, (sender, e) =>
			{
				if (e.Ok)
				{
					object Result = e.Message.Decode();
					if (Result != null)
						Console.Out.WriteLine(Result.ToString());

					Done.Set();
				}
				else
					Error.Set();
			}, null, Options);

			Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 30000));
		}

		[Test]
		public async Task Test_01_GET()
		{
			// Default test resource
			await this.Get("coap://vs0.inf.ethz.ch/test");
		}

		[Test]
		public async Task Test_02_Root()
		{
			await this.Get("coap://vs0.inf.ethz.ch/");
		}

		[Test]
		public async Task Test_03_Discover()
		{
			LinkDocument Doc = await this.Get("coap://vs0.inf.ethz.ch/.well-known/core") as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[Test]
		public async Task Test_04_Separate()
		{
			// Resource which cannot be served immediately and which cannot be acknowledged in a piggy-backed way
			await this.Get("coap://vs0.inf.ethz.ch/separate");
		}

		[Test]
		public async Task Test_05_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1");
		}

		[Test]
		public async Task Test_06_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1/seg2");
		}

		[Test]
		public async Task Test_07_LongPath()
		{
			// Long path resource
			await this.Get("coap://vs0.inf.ethz.ch/seg1/seg2/seg3");
		}

		[Test]
		public async Task Test_08_Large()
		{
			// Large resource
			await this.Get("coap://vs0.inf.ethz.ch/large");
		}

		[Test]
		public async Task Test_09_LargeSeparate()
		{
			// Large resource
			await this.Get("coap://vs0.inf.ethz.ch/large-separate");
		}

		[Test]
		[Ignore("Interop server has invalid XML.")]
		public async Task Test_10_MultiFormat()
		{
			// Resource that exists in different content formats (text/plain utf8 and application/xml)
			string s = await this.Get("coap://vs0.inf.ethz.ch/multi-format", new CoapOptionAccept(0)) as string;
			Assert.NotNull(s);

			XmlDocument Xml = await this.Get("coap://vs0.inf.ethz.ch/multi-format", new CoapOptionAccept(41)) as XmlDocument;
			Assert.NotNull(Xml);
		}

		[Test]
		public async Task Test_11_Hierarchical()
		{
			// Hierarchical link description entry
			Assert.NotNull(await this.Get("coap://vs0.inf.ethz.ch/path") as LinkDocument);
			Assert.NotNull(await this.Get("coap://vs0.inf.ethz.ch/path/sub1") as string);
			Assert.NotNull(await this.Get("coap://vs0.inf.ethz.ch/path/sub2") as string);
			Assert.NotNull(await this.Get("coap://vs0.inf.ethz.ch/path/sub3") as string);
		}

		[Test]
		public async Task Test_12_Query()
		{
			// Hierarchical link description entry
			Assert.NotNull(await this.Get("coap://vs0.inf.ethz.ch/query?A=1&B=2") as string);
		}

		[Test]
		public async Task Test_13_Observable()
		{
			// Observable resource which changes every 5 seconds
			Assert.NotNull(await this.Observe("coap://vs0.inf.ethz.ch/obs") as string);
		}

		[Test]
		public async Task Test_14_Observable_Large()
		{
			// Observable resource which changes every 5 seconds
			Assert.NotNull(await this.Observe("coap://vs0.inf.ethz.ch/obs-large") as string);
		}

		[Test]
		public async Task Test_15_Observable_NON()
		{
			// Observable resource which changes every 5 seconds
			Assert.NotNull(await this.Observe("coap://vs0.inf.ethz.ch/obs-non") as string);
		}

		[Test]
		public async Task Test_16_Observable_Pumping()
		{
			// Observable resource which changes every 5 seconds
			Assert.NotNull(await this.Observe("coap://vs0.inf.ethz.ch/obs-pumping") as string);
		}

		[Test]
		public async Task Test_17_Observable_Pumping_NON()
		{
			// Observable resource which changes every 5 seconds
			Assert.NotNull(await this.Observe("coap://vs0.inf.ethz.ch/obs-pumping-non") as string);
		}

		[Test]
		public async Task Test_18_POST()
		{
			// Perform POST transaction with responses containing several Location-Query options (CON mode)
			await this.Post("coap://vs0.inf.ethz.ch/location-query", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[Test]
		public async Task Test_19_POST_Large()
		{
			// Large resource that can be created using POST method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://vs0.inf.ethz.ch/large-create", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[Test]
		public async Task Test_20_POST_Large_Response()
		{
			// Handle POST with two-way blockwise transfer

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://vs0.inf.ethz.ch/large-post", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[Test]
		public async Task Test_21_PUT()
		{
			// Large resource that can be updated using PUT method
			await this.Put("coap://vs0.inf.ethz.ch/large-update", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[Test]
		public async Task Test_22_PUT_Large()
		{
			// Large resource that can be updated using PUT method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Put("coap://vs0.inf.ethz.ch/large-update", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		/*
		</obs-reset>,
		</validate>;title="Resource which varies",	(Test with ETag)
		*/
	}
}
