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
using Waher.Security.DTLS;

namespace Waher.Networking.CoAP.Test
{
	[TestClass]
	public class CoapLwm2mClientTests
	{
		private CoapEndpoint client;

		[TestInitialize]
		public void TestInitialize()
		{
			this.client = new CoapEndpoint(new int[] { CoapEndpoint.DefaultCoapPort },
				new int[] { CoapEndpoint.DefaultCoapsPort }, null, null, false, false,
				new ConsoleOutSniffer(BinaryPresentationMethod.Hexadecimal));
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

		[TestMethod]
		public async Task LWM2M_Client_Test_01_Discover()
		{
			LinkDocument Doc = await this.Get("coaps://leshan.eclipse.org/.well-known/core",
				new PresharedKey("testid", new byte[] { 1, 2, 3, 4 })) as LinkDocument;
			Assert.IsNotNull(Doc);
		}

		[TestMethod]
		public async Task LWM2M_Client_Test_02_Read()
		{
			object Result = await this.Get("coaps://leshan.eclipse.org/rd",
				new PresharedKey("testid", new byte[] { 1, 2, 3, 4 }));
			Assert.IsNotNull(Result);
		}
	}
}
