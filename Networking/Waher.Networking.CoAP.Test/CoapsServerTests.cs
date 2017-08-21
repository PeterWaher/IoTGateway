using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Networking.Sniffers;
using Waher.Networking.CoAP.ContentFormats;
using Waher.Networking.CoAP.CoRE;
using Waher.Networking.CoAP.Options;
using Waher.Runtime.Inventory;
using Waher.Security;
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapsServerTests : CoapServerTestsBase
	{
		private IUserSource users;

		protected override void SetupClientServer()
		{
			this.users = new Users(new User("testid", "01020304", "HEX", "CoAP"));
			this.server = new CoapEndpoint(null, new int[] { CoapEndpoint.DefaultCoapsPort }, 
				this.users, "CoAP", false, true);
			this.client = new CoapEndpoint(null, new int[] { CoapEndpoint.DefaultCoapsPort + 2 }, 
				null, null, true, false, new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
			this.clientCredentials = new PresharedKey("testid", new byte[] { 1, 2, 3, 4 });
		}

		[TestMethod]
		public async Task CoAP_Server_Test_01_GET()
		{
			// Default test resource
			object Response = await this.Get("coaps://127.0.0.1/test");
			Assert.AreEqual(ResponseTest, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_02_Root()
		{
			object Response = await this.Get("coaps://127.0.0.1/");
			Assert.AreEqual(ResponseRoot, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_03_Discover()
		{
			LinkDocument Doc = await this.Get("coaps://127.0.0.1/.well-known/core") as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_04_Separate()
		{
			// Resource which cannot be served immediately and which cannot be acknowledged in a piggy-backed way
			object Response = await this.Get("coaps://127.0.0.1/separate");
			Assert.AreEqual(ResponseTest, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_05_LongPath()
		{
			// Long path resource
			object Response = await this.Get("coaps://127.0.0.1/seg1");
			Assert.AreEqual(ResponseTest, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_06_LongPath()
		{
			// Long path resource
			object Response = await this.Get("coaps://127.0.0.1/seg1/seg2");
			Assert.AreEqual(ResponseTest, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_07_LongPath()
		{
			// Long path resource
			object Response = await this.Get("coaps://127.0.0.1/seg1/seg2/seg3");
			Assert.AreEqual(ResponseTest, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_08_Large()
		{
			// Large resource
			object Response = await this.Get("coaps://127.0.0.1/large");
			Assert.AreEqual(ResponseLarge, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_09_LargeSeparate()
		{
			// Large resource
			object Response = await this.Get("coaps://127.0.0.1/large-separate");
			Assert.AreEqual(ResponseLarge, Response);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_10_MultiFormat()
		{
			// Resource that exists in different content formats (text/plain utf8 and application/xml)
			string s = await this.Get("coaps://127.0.0.1/multi-format", new CoapOptionAccept(0)) as string;
			AssertNotNull(s);

			XmlDocument Xml = await this.Get("coaps://127.0.0.1/multi-format", new CoapOptionAccept(41)) as XmlDocument;
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
			Assert.AreEqual(ResponseHierarchical, (await this.Get("coaps://127.0.0.1/path")).ToString());
			Assert.AreEqual("/path/sub1", await this.Get("coaps://127.0.0.1/path/sub1"));
			Assert.AreEqual("/path/sub2", await this.Get("coaps://127.0.0.1/path/sub2"));
			Assert.AreEqual("/path/sub3", await this.Get("coaps://127.0.0.1/path/sub3"));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_12_Query()
		{
			// Hierarchical link description entry
			Assert.AreEqual("?A=1&B=2", await this.Get("coaps://127.0.0.1/query?A=1&B=2"));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_13_Observable()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coaps://127.0.0.1/obs") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_14_Observable_Large()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coaps://127.0.0.1/obs-large") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_15_Observable_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coaps://127.0.0.1/obs-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_16_Observable_Pumping()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coaps://127.0.0.1/obs-pumping") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_17_Observable_Pumping_NON()
		{
			// Observable resource which changes every 5 seconds
			AssertNotNull(await this.Observe("coaps://127.0.0.1/obs-pumping-non") as string);
		}

		[TestMethod]
		public async Task CoAP_Server_Test_18_POST()
		{
			// Perform POST transaction with responses containing several Location-Query options (CON mode)
			await this.Post("coaps://127.0.0.1/location-query", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_19_POST_Large()
		{
			// Large resource that can be created using POST method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coaps://127.0.0.1/large-create", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_20_POST_Large_Response()
		{
			// Handle POST with two-way blockwise transfer

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Post("coaps://127.0.0.1/large-post", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_21_PUT()
		{
			// Large resource that can be updated using PUT method
			await this.Put("coaps://127.0.0.1/large-update", Encoding.UTF8.GetBytes("Hello"), 64, new CoapOptionContentFormat(0));
		}

		[TestMethod]
		public async Task CoAP_Server_Test_22_PUT_Large()
		{
			// Large resource that can be updated using PUT method

			string s = "0123456789";
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s + s + s + s + s + s + s + s + s;
			s = s + s;

			await this.Put("coaps://127.0.0.1/large-update", Encoding.UTF8.GetBytes(s), 64, new CoapOptionContentFormat(0));
		}

		/*
		</obs-reset>,
		</validate>;title="Resource which varies",	(Test with ETag)
		*/
	}
}
