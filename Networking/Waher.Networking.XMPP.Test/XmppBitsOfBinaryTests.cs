using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Waher.Content.Text;
using Waher.Networking.XMPP.BitsOfBinary;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppBitsOfBinaryTests : CommunicationTests
	{
		private BobClient bobClient1;
		private BobClient bobClient2;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			SetupSnifferAndLog();
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			DisposeSnifferAndLog();
		}

		public override void ConnectClients()
		{
			base.ConnectClients();

			Assert.AreEqual(XmppState.Connected, this.client1.State);
			Assert.AreEqual(XmppState.Connected, this.client2.State);

			this.bobClient1 = new BobClient(this.client1, "Bob1");
			this.bobClient2 = new BobClient(this.client2, "Bob2");
		}

		public override Task DisposeClients()
		{
			if (this.bobClient1 is not null)
			{
				this.bobClient1.Dispose();
				this.bobClient1 = null;
			}

			if (this.bobClient2 is not null)
			{
				this.bobClient2.Dispose();
				this.bobClient2 = null;
			}

			return base.DisposeClients();
		}

		[TestMethod]
		public async Task BitsOfBinary_Test_01_GetData()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new(false);
				ManualResetEvent Error = new(false);

				string s = "Hello world.";
				byte[] Bin = Encoding.UTF8.GetBytes(s);
				string ContentId = await this.bobClient2.StoreData(Bin, PlainTextCodec.DefaultContentType);

				await this.bobClient1.GetData(this.client2.FullJID, ContentId, (Sender, e) =>
				{
					if (e.Ok && e.ContentId == ContentId && e.ContentType == PlainTextCodec.DefaultContentType && Encoding.UTF8.GetString(e.Data) == s)
						Done.Set();
					else
						Error.Set();
				
					return Task.CompletedTask;
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
			}
			finally
			{
				await this.DisposeClients();
			}
		}

		[TestMethod]
		public async Task BitsOfBinary_Test_02_GetData_Expires()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new(false);
				ManualResetEvent Error = new(false);

				string s = "Hello world.";
				byte[] Bin = Encoding.UTF8.GetBytes(s);
				string ContentId = await this.bobClient2.StoreData(Bin, PlainTextCodec.DefaultContentType, DateTime.Now.AddMinutes(1));

				await this.bobClient1.GetData(this.client2.FullJID, ContentId, (Sender, e) =>
				{
					double d;

					if (e.Ok && e.ContentId == ContentId && e.ContentType == PlainTextCodec.DefaultContentType && Encoding.UTF8.GetString(e.Data) == s &&
						e.Expires.HasValue && (d = (e.Expires.Value - DateTime.Now).TotalSeconds) >= 50 && d <= 60)
					{
						Done.Set();
					}
					else
						Error.Set();

					return Task.CompletedTask;
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
			}
			finally
			{
				await this.DisposeClients();
			}
		}
	}
}
