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

			this.server = new HttpServer(8080);
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			if (this.server != null)
			{
				this.server.Dispose();
				this.server = null;
			}

			if (this.sink != null)
			{
				Log.Unregister(this.sink);
				this.sink.Dispose();
				this.sink = null;
			}
		}

		[Test]
		public void Test_01_GET_HTTP()
		{
			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/Resource.html");
			}
		}
	}
}
