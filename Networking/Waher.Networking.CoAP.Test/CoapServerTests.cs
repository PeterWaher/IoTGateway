using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapServerTests
	{
		private CoapEndpoint server;
		private CoapEndpoint client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.server = new CoapEndpoint(CoapEndpoint.DefaultCoapPort, false, true, new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
			this.client = new CoapEndpoint(CoapEndpoint.DefaultCoapPort + 1, true, false);

			this.server.Register("/test", (req, resp) =>
			{
				resp.Respond(CoapCode.Content, Encoding.ASCII.GetBytes("Hello world."), 64);
			});
		}

		[TestCleanup]
		public void TestCleanup()
		{
			this.Cleanup(ref this.client);
			this.Cleanup(ref this.server);
		}

		private void Cleanup(ref CoapEndpoint Client)
		{
			if (Client != null)
			{
				ulong[] Tokens = Client.GetActiveTokens();
				ushort[] MessageIDs = Client.GetActiveMessageIDs();

				Client.Dispose();
				Client = null;

				Assert.AreEqual(0, Tokens.Length, "There are tokens that have not been unregistered properly.");
				Assert.AreEqual(0, MessageIDs.Length, "There are message IDs that have not been unregistered properly.");
			}
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

		[TestMethod]
		public async Task CoAP_Server_Test_01_GET()
		{
			// Default test resource
			await this.Get("coap://127.0.0.1/test");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_02_Root()
		{
			await this.Get("coap://127.0.0.1/");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_03_Discover()
		{
			LinkDocument Doc = await this.Get("coap://127.0.0.1/.well-known/core") as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_04_Separate()
		{
			// Resource which cannot be served immediately and which cannot be acknowledged in a piggy-backed way
			await this.Get("coap://127.0.0.1/separate");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_05_LongPath()
		{
			// Long path resource
			await this.Get("coap://127.0.0.1/seg1");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_06_LongPath()
		{
			// Long path resource
			await this.Get("coap://127.0.0.1/seg1/seg2");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_07_LongPath()
		{
			// Long path resource
			await this.Get("coap://127.0.0.1/seg1/seg2/seg3");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_08_Large()
		{
			// Large resource
			await this.Get("coap://127.0.0.1/large");
		}

		[TestMethod]
		public async Task CoAP_Server_Test_09_LargeSeparate()
		{
			// Large resource
			await this.Get("coap://127.0.0.1/large-separate");
		}

		[TestMethod]
		[Ignore]
		public async Task CoAP_Server_Test_10_MultiFormat()
		{
			// Resource that exists in different content formats (text/plain utf8 and application/xml)
			string s = await this.Get("coap://127.0.0.1/multi-format", new CoapOptionAccept(0)) as string;
			AssertNotNull(s);

			XmlDocument Xml = await this.Get("coap://127.0.0.1/multi-format", new CoapOptionAccept(41)) as XmlDocument;
			AssertNotNull(Xml);
		}

		private static void AssertNotNull(object Obj)
		{
			Assert.IsTrue(Obj != null);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_11_Hierarchical()
		{
			// Hierarchical link description entry
			AssertNotNull(await this.Get("coap://127.0.0.1/path") as LinkDocument);
			AssertNotNull(await this.Get("coap://127.0.0.1/path/sub1") as string);
			AssertNotNull(await this.Get("coap://127.0.0.1/path/sub2") as string);
			AssertNotNull(await this.Get("coap://127.0.0.1/path/sub3") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_12_Query()
		{
			// Hierarchical link description entry
			AssertNotNull(await this.Get("coap://127.0.0.1/query?A=1&B=2") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_13_Observable()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://127.0.0.1/obs") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_14_Observable_Large()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://127.0.0.1/obs-large") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_15_Observable_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://127.0.0.1/obs-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_16_Observable_Pumping()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://127.0.0.1/obs-pumping") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_17_Observable_Pumping_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://127.0.0.1/obs-pumping-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_18_POST()
		{
			// Perform POST transaction with responses containing several Location-Query options (CON mode)
			await this.Post("coap://127.0.0.1/location-query", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_19_POST_Large()
		{
			// Large resource that can be created using POST method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://127.0.0.1/large-create", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_20_POST_Large_Response()
		{
			// Handle POST with two-way blockwise transfer

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://127.0.0.1/large-post", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_21_PUT()
		{
			// Large resource that can be updated using PUT method
			await this.Put("coap://127.0.0.1/large-update", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_22_PUT_Large()
		{
			// Large resource that can be updated using PUT method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Put("coap://127.0.0.1/large-update", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		/*
		</obs-reset>,
		</validate>;title="Resource which varies",	(Test with ETag)
		*/
	}
}
