using System;
using System.Reflection;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Authentication;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestFixture]
	public class HttpServerTests : IUserSource
	{
		private HttpServer server;
		private ConsoleEventSink sink = null;

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			new Waher.Content.Drawing.ImageCodec();

			this.sink = new ConsoleEventSink();
			Log.Register(this.sink);

			X509Certificate2 Certificate = Resources.LoadCertificate("Waher.Networking.HTTP.Test.certificate.pfx", "testexamplecom");	// Certificate from http://www.cert-depot.com/
			this.server = new HttpServer(8080, 8088, Certificate);

			ServicePointManager.ServerCertificateValidationCallback = delegate(Object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
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
		public void Test_01_GET_HTTP_ContentLength()
		{
			this.server.Register("/test01.txt", (req, resp) => resp.Return("hej på dej"));

			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test01.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[Test]
		public void Test_02_GET_HTTP_Chunked()
		{
			this.server.Register("/test02.txt", (req, resp) =>
			{
				int i;

				resp.ContentType = "text/plain";
				for (i = 0; i < 1000; i++)
					resp.Write(new string('a', 100));
			});

			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test02.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual(new string('a', 100000), s);
			}
		}

		[Test]
		public void Test_03_GET_HTTP_Encoding()
		{
			this.server.Register("/test03.png", (req, resp) =>
			{
				resp.Return(new Bitmap(320, 200));
			});

			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("http://localhost:8080/test03.png");
				MemoryStream ms = new MemoryStream(Data);
				Bitmap Bmp = new Bitmap(ms);
				Assert.AreEqual(320, Bmp.Width);
				Assert.AreEqual(200, Bmp.Height);
			}
		}

		[Test]
		public void Test_04_GET_HTTPS()
		{
			this.server.Register("/test04.txt", (req, resp) => resp.Return("hej på dej"));

			using (WebClient Client = new WebClient())
			{
				byte[] Data = Client.DownloadData("https://localhost:8088/test04.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[Test]
		public void Test_05_Authentication_Basic()
		{
			this.server.Register("/test05.txt", (req, resp) => resp.Return("hej på dej"), new BasicAuthentication("Test05", this));

			using (WebClient Client = new WebClient())
			{
				Client.Credentials = new NetworkCredential("User", "Password");
				byte[] Data = Client.DownloadData("http://localhost:8080/test05.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		[Test]
		public void Test_06_Authentication_Digest()
		{
			this.server.Register("/test06.txt", (req, resp) => resp.Return("hej på dej"), new DigestAuthentication("Test06", this));

			using (WebClient Client = new WebClient())
			{
				Client.Credentials = new NetworkCredential("User", "Password");
				byte[] Data = Client.DownloadData("http://localhost:8080/test06.txt");
				string s = Encoding.UTF8.GetString(Data);
				Assert.AreEqual("hej på dej", s);
			}
		}

		public bool TryGetUser(string UserName, out IUser User)
		{
			if (UserName == "User")
			{
				User = new User();
				return true;
			}
			else
			{
				User = null;
				return false;
			}
		}

		class User : IUser
		{
			public string UserName
			{
				get { return "User"; }
			}

			public string PasswordHash
			{
				get { return "Password"; }
			}

			public string PasswordHashType
			{
				get { return string.Empty; }
			}
		}


	}
}
