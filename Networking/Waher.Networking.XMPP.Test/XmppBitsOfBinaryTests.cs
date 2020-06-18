using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.BitsOfBinary;

namespace Waher.Networking.XMPP.Test
{
	[TestClass]
	public class XmppBitsOfBinaryTests : CommunicationTests
	{
		private BobClient bobClient1;
		private BobClient bobClient2;

		public override void ConnectClients()
		{
			base.ConnectClients();

			Assert.AreEqual(XmppState.Connected, this.client1.State);
			Assert.AreEqual(XmppState.Connected, this.client2.State);

			this.bobClient1 = new BobClient(this.client1, "Bob1");
			this.bobClient2 = new BobClient(this.client2, "Bob2");
		}

		public override void DisposeClients()
		{
			if (this.bobClient1 != null)
			{
				this.bobClient1.Dispose();
				this.bobClient1 = null;
			}

			if (this.bobClient2 != null)
			{
				this.bobClient2.Dispose();
				this.bobClient2 = null;
			}

			base.DisposeClients();
		}

		[TestMethod]
		public void BitsOfBinary_Test_01_GetData()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				string s = "Hello world.";
				byte[] Bin = Encoding.UTF8.GetBytes(s);
				string ContentId = this.bobClient2.StoreData(Bin, "text/plain");

				this.bobClient1.GetData(this.client2.FullJID, ContentId, (sender, e) =>
				{
					if (e.Ok && e.ContentId == ContentId && e.ContentType == "text/plain" && Encoding.UTF8.GetString(e.Data) == s)
						Done.Set();
					else
						Error.Set();
				
					return Task.CompletedTask;
				}, null);

				Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 5000));
			}
			finally
			{
				this.DisposeClients();
			}
		}

		[TestMethod]
		public void BitsOfBinary_Test_02_GetData_Expires()
		{
			this.ConnectClients();
			try
			{
				ManualResetEvent Done = new ManualResetEvent(false);
				ManualResetEvent Error = new ManualResetEvent(false);

				string s = "Hello world.";
				byte[] Bin = Encoding.UTF8.GetBytes(s);
				string ContentId = this.bobClient2.StoreData(Bin, "text/plain", DateTime.Now.AddMinutes(1));

				this.bobClient1.GetData(this.client2.FullJID, ContentId, (sender, e) =>
				{
					double d;

					if (e.Ok && e.ContentId == ContentId && e.ContentType == "text/plain" && Encoding.UTF8.GetString(e.Data) == s &&
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
				this.DisposeClients();
			}
		}
	}
}
