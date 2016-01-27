using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using NUnit.Framework;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;

namespace Waher.Networking.HTTP.Test
{
	[TestFixture]
	public class HttpServerTests
	{
		private HttpServer server;
		private ConsoleEventSink sink = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			if (this.sink != null)
			{
				Log.Unregister(this.sink);
				this.sink.Dispose();
				this.sink = null;
			}
		}

		[SetUp]
		public void Setup()
		{
			this.server = new HttpServer(8080);
		}

		[TearDown]
		public void TearDown()
		{
			if (this.server != null)
			{
				this.server.Dispose();
				this.server = null;
			}
		}

		[Test]
		public void Test_01_GET_HTTP()
		{
			this.server.Register("/Resource.txt", (req, resp) => resp.Return("hej på dej"));

			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/Resource.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}
	}
}
