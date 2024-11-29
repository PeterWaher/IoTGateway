using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.Sniffers;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http2ServerTests : IUserSource
	{
		private static HttpServer server;
		private static ConsoleEventSink sink = null;
		private static XmlFileSniffer xmlSniffer = null;

		[ClassInitialize]
		public static void ClassInitialize(TestContext _)
		{
			sink = new ConsoleEventSink();
			Log.Register(sink);

			if (xmlSniffer is null)
			{
				File.Delete("HTTP2.xml");
				xmlSniffer = xmlSniffer = new XmlFileSniffer("HTTP2.xml",
						@"..\..\..\..\..\Waher.IoTGateway.Resources\Transforms\SnifferXmlToHtml.xslt",
						int.MaxValue, BinaryPresentationMethod.Base64);
			}

			X509Certificate2 Certificate = Resources.LoadCertificate("Waher.Networking.HTTP.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			server = new HttpServer(8081, 8088, Certificate,
				new ConsoleOutSniffer(BinaryPresentationMethod.ByteCount, LineEnding.NewLine),
				xmlSniffer);

			ServicePointManager.ServerCertificateValidationCallback = delegate (Object obj, X509Certificate X509certificate, X509Chain chain, SslPolicyErrors errors)
			{
				return true;
			};
		}

		[ClassCleanup]
		public static void ClassCleanup()
		{
			server?.Dispose();
			server = null;

			xmlSniffer?.Dispose();
			xmlSniffer = null;

			if (sink is not null)
			{
				Log.Unregister(sink);
				sink.Dispose();
				sink = null;
			}
		}

		public Task<IUser> TryGetUser(string UserName)
		{
			if (UserName == "User")
				return Task.FromResult<IUser>(new User());
			else
				return Task.FromResult<IUser>(null);
		}

		[TestMethod]
		public async Task Test_01_Connect_And_GET()
		{
			server.Register("/test01.txt", (req, resp) => resp.Return("hej på dej"));

			SocketsHttpHandler Handler = new()
			{
				AllowAutoRedirect = false,
				UseCookies = true,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				InitialHttp2StreamWindowSize = 65535,
				ConnectTimeout = TimeSpan.FromSeconds(10)
			};

			Handler.SslOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None; // For HTTP/2 cleartext

			using HttpClient Client = new(Handler);
			Client.DefaultRequestVersion = HttpVersion.Version20;
			Client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
			Client.DefaultRequestHeaders.ConnectionClose = false;

			HttpResponseMessage Response = await Client.GetAsync("http://localhost:8081/test01.txt");
			byte[] Data = await Response.Content.ReadAsByteArrayAsync();
			string s = Encoding.UTF8.GetString(Data);
			Assert.AreEqual("hej på dej", s);
		}
	}
}
