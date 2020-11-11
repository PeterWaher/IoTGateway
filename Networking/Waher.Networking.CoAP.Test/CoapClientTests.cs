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
using Waher.Networking.LWM2M;
using Waher.Networking.CoAP.Options;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapClientTests
	{
		private static ConsoleEventSink consoleEventSink = null;
		private static FilesProvider filesProvider = null;

		private CoapEndpoint client;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(
				typeof(IContentDecoder).Assembly,
				typeof(Waher.Content.Xml.Text.XmlCodec).Assembly,
				typeof(CoapEndpoint).Assembly,
				typeof(Lwm2mClient).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(ICipher).Assembly);

			Log.Register(consoleEventSink = new ConsoleEventSink());

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000);
			Database.Register(filesProvider);
		}

		[AssemblyCleanup]
		public static void AssemblyCleanup()
		{
			if (filesProvider != null)
			{
				filesProvider.Dispose();
				filesProvider = null;
			}

			if (consoleEventSink != null)
			{
				Log.Unregister(consoleEventSink);
				consoleEventSink = null;
			}
		}

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new CoapEndpoint(new int[] { CoapEndpoint.DefaultCoapPort },
				new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, false,
				new TextWriterSniffer(Console.Out, BinaryPresentationMethod.Hexadecimal));
		}

		[TestCleanup]
		public void TestCleanup()
		{
			if (this.client != null)
			{
				ulong[] Tokens = this.client.GetActiveTokens();
				ushort[] MessageIDs = this.client.GetActiveMessageIDs();

				this.client.Dispose();
				this.client = null;

				Assert.AreEqual(0, Tokens.Length, "There are tokens that have not been unregistered properly.");
				Assert.AreEqual(0, MessageIDs.Length, "There are message IDs that have not been unregistered properly.");
			}
		}

		private Task<object> Get(string Uri, params CoapOption[] Options)
		{
			return this.Get(Uri, null, Options);
		}

		private async Task<object> Get(string Uri, IDtlsCredentials Credentials, params CoapOption[] Options)
		{
			ManualResetEvent Done = new ManualResetEvent(false);
			ManualResetEvent Error = new ManualResetEvent(false);
			object Result = null;

			await this.client.GET(Uri, true, Credentials, (sender, e) =>
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

			await this.client.Observe(Uri, true, null, (sender, e) =>
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

			await this.client.UnregisterObservation(Uri, true, Token, null, (sender, e) =>
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

			await this.client.POST(Uri, true, Payload, BlockSize, null, (sender, e) =>
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

			await this.client.PUT(Uri, true, Payload, BlockSize, null, (sender, e) =>
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

			await this.client.DELETE(Uri, true, null, (sender, e) =>
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
		public async Task CoAP_Client_Test_01_GET()
		{
			// Default test resource

			await this.Get("coap://californium.eclipse.org/test");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_02_Root()
		{
			await this.Get("coap://californium.eclipse.org/");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_03_Discover()
		{
			LinkDocument Doc = await this.Get("coap://californium.eclipse.org/.well-known/core") as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_04_Separate()
		{
			// Resource which cannot be served immediately and which cannot be acknowledged in a piggy-backed way
			await this.Get("coap://californium.eclipse.org/separate");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_05_LongPath()
		{
			// Long path resource
			await this.Get("coap://californium.eclipse.org/seg1");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_06_LongPath()
		{
			// Long path resource
			await this.Get("coap://californium.eclipse.org/seg1/seg2");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_07_LongPath()
		{
			// Long path resource
			await this.Get("coap://californium.eclipse.org/seg1/seg2/seg3");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_08_Large()
		{
			// Large resource
			await this.Get("coap://californium.eclipse.org/large");
		}

		[TestMethod]
		public async Task CoAP_Client_Test_09_LargeSeparate()
		{
			// Large resource
			await this.Get("coap://californium.eclipse.org/large-separate");
		}

		[TestMethod]
		[Ignore]
		public async Task CoAP_Client_Test_10_MultiFormat()
		{
			// Resource that exists in different content formats (text/plain utf8 and application/xml)
			string s = await this.Get("coap://californium.eclipse.org/multi-format", new CoapOptionAccept(0)) as string;
			AssertNotNull(s);

			XmlDocument Xml = await this.Get("coap://californium.eclipse.org/multi-format", new CoapOptionAccept(41)) as XmlDocument;
			AssertNotNull(Xml);
		}

		private static void AssertNotNull(object Obj)
		{
			Assert.IsTrue(Obj != null);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_11_Hierarchical()
		{
			// Hierarchical link description entry
			AssertNotNull(await this.Get("coap://californium.eclipse.org/path") as LinkDocument);
			AssertNotNull(await this.Get("coap://californium.eclipse.org/path/sub1") as string);
			AssertNotNull(await this.Get("coap://californium.eclipse.org/path/sub2") as string);
			AssertNotNull(await this.Get("coap://californium.eclipse.org/path/sub3") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_12_Query()
		{
			// Hierarchical link description entry
			AssertNotNull(await this.Get("coap://californium.eclipse.org/query?A=1&B=2") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_13_Observable()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://californium.eclipse.org/obs") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_14_Observable_Large()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://californium.eclipse.org/obs-large") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_15_Observable_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://californium.eclipse.org/obs-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_16_Observable_Pumping()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://californium.eclipse.org/obs-pumping") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_17_Observable_Pumping_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coap://californium.eclipse.org/obs-pumping-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Client_Test_18_POST()
		{
			// Perform POST transaction with responses containing several Location-Query options (CON mode)
			await this.Post("coap://californium.eclipse.org/location-query", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Client_Test_19_POST_Large()
		{
			// Large resource that can be created using POST method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://californium.eclipse.org/large-create", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Client_Test_20_POST_Large_Response()
		{
			// Handle POST with two-way blockwise transfer

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coap://californium.eclipse.org/large-post", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Client_Test_21_PUT()
		{
			// Large resource that can be updated using PUT method
			await this.Put("coap://californium.eclipse.org/large-update", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Client_Test_22_PUT_Large()
		{
			// Large resource that can be updated using PUT method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Put("coap://californium.eclipse.org/large-update", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		/*
		</obs-reset>,
		</validate>;title="Resource which varies",	(Test with ETag)
		*/
	}
}
