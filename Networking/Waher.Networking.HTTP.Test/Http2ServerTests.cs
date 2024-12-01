using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Waher.Security;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http2ServerTests : HttpServerTests
	{
		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			HttpServerTests.ClassInitialize(_);
		}

		[ClassCleanup]
		public new static void ClassCleanup()
		{
			HttpServerTests.ClassCleanup();

		}

		public override Version ProtocolVersion => HttpVersion.Version20;

		//[TestMethod]
		//public async Task Test_01_Connect_And_GET()
		//{
		//	server.Register("/test01.txt", (req, resp) => resp.Return("hej på dej"));
		//
		//	SocketsHttpHandler Handler = new()
		//	{
		//		AllowAutoRedirect = false,
		//		UseCookies = true,
		//		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
		//		InitialHttp2StreamWindowSize = 65535,
		//		ConnectTimeout = TimeSpan.FromSeconds(10),
		//	};
		//
		//	Handler.SslOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.None; // For HTTP/2 cleartext
		//
		//	using HttpClient Client = new(Handler);
		//	Client.DefaultRequestVersion = HttpVersion.Version20;
		//	Client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
		//	Client.DefaultRequestHeaders.ConnectionClose = false;
		//	Client.Timeout = TimeSpan.FromSeconds(10);
		//
		//	HttpResponseMessage Response = await Client.GetAsync("http://localhost:8081/test01.txt");
		//	byte[] Data = await Response.Content.ReadAsByteArrayAsync();
		//	string s = Encoding.UTF8.GetString(Data);
		//	Assert.AreEqual("hej på dej", s);
		//}

		/* To Test
		 * 
		 * Huffman encoded strings
		 * padding in requests
		 * Long headers (multiple HEADERS & CONTINUATION frames)
		 * data-less responses
		 * data responses (one DATA frame)
		 * long data responses (multiple DATA & CONTINUATION frames)
		 * long headers in data responses (multiple HEADER & CONTINUATION frames)
		 * push promise
		 * stream priorities
		 * stream errors
		 * connection errors
		 * ping/pong
		 */
	}
}
