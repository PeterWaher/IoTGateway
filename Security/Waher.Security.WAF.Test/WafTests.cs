using System.Security.Cryptography.X509Certificates;
using System.Text;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Networking;
using Waher.Networking.HTTP;
using Waher.Networking.Sniffers;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;
using Waher.Script;
using Waher.Security.LoginMonitor;

namespace Waher.Security.WAF.Test
{
	[TestClass]
	public sealed class WafTests
	{
		private static FilesProvider filesProvider = null;
		private static LoginAuditor auditor = null;
		private HttpServer server = null;
		private X509Certificate2 certificate;
		private ConsoleOutSniffer sniffer;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(HttpServer).Assembly,
				typeof(WafTests).Assembly,
				typeof(Expression).Assembly,
				typeof(WebApplicationFirewall).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(LoginAuditor).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			auditor = new LoginAuditor("Login Auditor",
				new LoginInterval(5, TimeSpan.FromHours(1)),    // Maximum 5 failed login attempts in an hour
				new LoginInterval(2, TimeSpan.FromDays(1)),     // Maximum 2x5 failed login attempts in a day
				new LoginInterval(2, TimeSpan.FromDays(7)),     // Maximum 2x2x5 failed login attempts in a week
				new LoginInterval(2, TimeSpan.MaxValue));       // Maximum 2x2x2x5 failed login attempts in total, then blocked.

			InternetContent.SetDefaultTimeout(3000, false);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}

		public TestContext TestContext { get; set; }

		private void SetupServer(string FirewallFileName)
		{
			WebApplicationFirewall Firewall = WebApplicationFirewall.LoadFromFile(
				Path.Combine("Data", FirewallFileName), auditor, "Data");

			this.SetupServer(Firewall);
		}

		private void SetupServer(WebApplicationFirewall Firewall)
		{
			this.certificate = Certificates.LoadCertificate("Waher.Security.WAF.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			this.server = new([8081], [8088], this.certificate, false,
				ClientCertificates.Optional, true, [], false)
			{
				WebApplicationFirewall = Firewall
			};

			Types.SetModuleParameter("HTTP", this.server);

			this.server.Register("/A", (req, resp) => resp.Return("Hello World!"));
			this.server.Register("/A/C", (req, resp) => resp.Return("Hello again."));
			this.server.Register("/B", (req, resp) => resp.Return("SubPath: " + req.SubPath), true, true);
			this.server.Register("/P", null, async (req, resp) => await resp.Return((await req.DecodeDataAsync()).Decoded));
			this.server.Register(new HttpFolderResource(string.Empty, "Data", false, false, true, false));

			this.sniffer = new ConsoleOutSniffer(BinaryPresentationMethod.Base64, LineEnding.NewLine);
			this.server.Add(this.sniffer);
		}

		private async Task CloseServer()
		{
			if (this.server is not null)
			{
				await this.server?.DisposeAsync();
				this.server = null;
			}

			if (this.sniffer is not null)
			{
				await this.sniffer.FlushAsync();
				await this.sniffer.DisposeAsync();
			}
		}

		[TestInitialize]
		public void TestInitialize()
		{
			string TestName = this.TestContext!.TestName;
			this.SetupServer(TestName + ".xml");
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await this.CloseServer();
		}

		private Task Get(string RelativeUrl, int ExpectedStatusCode, bool Encrypted)
		{
			return this.Get(RelativeUrl, ExpectedStatusCode, null, Encrypted);
		}

		private Task Get(string RelativeUrl, Type ExpectedException, bool Encrypted)
		{
			return this.Get(RelativeUrl, 0, ExpectedException, Encrypted);
		}

		private async Task Get(string RelativeUrl, int ExpectedStatusCode,
			Type ExpectedException, bool Encrypted)
		{
			Uri Uri = GetUri(RelativeUrl, Encrypted);
			ContentResponse Response;

			try
			{
				Response = await InternetContent.GetAsync(Uri, this.certificate,
					new KeyValuePair<string, string>("Accept", "*/*"));
			}
			catch (Exception ex)
			{
				Response = new ContentResponse(ex);
			}

			CheckResponse(Response, ExpectedStatusCode, ExpectedException);
		}

		private Task Post(string RelativeUrl, object Data, int ExpectedStatusCode,
			bool Encrypted)
		{
			return this.Post(RelativeUrl, Data, ExpectedStatusCode, null, Encrypted);
		}

		//private Task Post(string RelativeUrl, object Data, Type ExpectedException, 
		//	bool Encrypted)
		//{
		//	return this.Post(RelativeUrl, Data, 0, ExpectedException, Encrypted);
		//}

		private async Task Post(string RelativeUrl, object Data,
			int ExpectedStatusCode, Type ExpectedException, bool Encrypted)
		{
			Uri Uri = GetUri(RelativeUrl, Encrypted);
			ContentResponse Response;

			try
			{
				Response = await InternetContent.PostAsync(Uri, Data, this.certificate,
					new KeyValuePair<string, string>("Accept", "*/*"));
			}
			catch (Exception ex)
			{
				Response = new ContentResponse(ex);
			}

			CheckResponse(Response, ExpectedStatusCode, ExpectedException);
		}

		private static Uri GetUri(string RelativeUrl, bool Encrypted)
		{
			if (Encrypted)
				return new Uri("https://localhost:8088" + RelativeUrl);
			else
				return new Uri("http://localhost:8081" + RelativeUrl);
		}

		private static void CheckResponse(ContentResponse Response, int ExpectedStatusCode,
			Type ExpectedException)
		{
			if (ExpectedStatusCode == 200)
				Response.AssertOk();
			else
			{
				Assert.IsTrue(Response.HasError, "Error response expected.");

				if (ExpectedException is null)
				{
					WebException ErrorResponse = Response.Error as WebException;
					Assert.IsNotNull(ErrorResponse, "Error response not of type WebException: " + Response.Error.GetType().FullName);

					Assert.AreEqual(ExpectedStatusCode, (int)ErrorResponse.StatusCode, "Unexpected status code in error response.");
				}
				else
					Assert.AreEqual(ExpectedException, Response.Error.GetType());
			}
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 405, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_001_Allow(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_002_Forbid(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 404, false)]
		[DataRow("/A/C", 404, false)]
		[DataRow("/B", 404, false)]
		[DataRow("/B/C", 404, false)]
		[DataRow("/P", 404, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_003_NotFound(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 429, false)]
		[DataRow("/A/C", 429, false)]
		[DataRow("/B", 429, false)]
		[DataRow("/B/C", 429, false)]
		[DataRow("/P", 429, false)]
		[DataRow("/X", 429, false)]
		public async Task Test_004_RateLimited(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", typeof(TaskCanceledException), false)]
		[DataRow("/A/C", typeof(TaskCanceledException), false)]
		[DataRow("/B", typeof(TaskCanceledException), false)]
		[DataRow("/B/C", typeof(TaskCanceledException), false)]
		[DataRow("/P", typeof(TaskCanceledException), false)]
		[DataRow("/X", typeof(TaskCanceledException), false)]
		public async Task Test_005_Ignore(string Resource, Type ExpectedException,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedException, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", typeof(HttpRequestException), false)]
		[DataRow("/A/C", typeof(HttpRequestException), false)]
		[DataRow("/B", typeof(HttpRequestException), false)]
		[DataRow("/B/C", typeof(HttpRequestException), false)]
		[DataRow("/P", typeof(HttpRequestException), false)]
		[DataRow("/X", typeof(HttpRequestException), false)]
		public async Task Test_006_Close(string Resource, Type ExpectedException,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedException, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_007_UriMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_008_PathMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_009_ResourceMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", "Hello", 403, false)]
		[DataRow("/A/C", "Hello", 403, false)]
		[DataRow("/B", "Hello", 403, false)]
		[DataRow("/B/C", "Hello", 403, false)]
		[DataRow("/P", "Hello", 200, false)]
		[DataRow("/X", "Hello", 403, false)]
		public async Task Test_010_MethodMatch(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted)
		{
			await this.Post(Resource, Data, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", "Hello", 405, false)]
		[DataRow("/A/C", "Hello", 405, false)]
		[DataRow("/B", "Hello", 405, false)]
		[DataRow("/B/C", "Hello", 405, false)]
		[DataRow("/P", "Hello", 200, false)]
		[DataRow("/P", true, 403, false)]
		[DataRow("/X", "Hello", 404, false)]
		public async Task Test_011_ContentTypeMatch(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted)
		{
			await this.Post(Resource, Data, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", "Hello", 405, false)]
		[DataRow("/A/C", "Hello", 405, false)]
		[DataRow("/B", "Hello", 405, false)]
		[DataRow("/B/C", "Hello", 405, false)]
		[DataRow("/P", "Hello", 200, false)]
		[DataRow("/P", "Bye", 403, false)]
		[DataRow("/X", "Hello", 404, false)]
		public async Task Test_012_ContentMatch(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted)
		{
			await this.Post(Resource, Data, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 405, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_013_EndpointMatch1(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_014_EndpointMatch2(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, true)]
		[DataRow("/A/C", 200, true)]
		[DataRow("/B", 200, true)]
		[DataRow("/B/C", 200, true)]
		[DataRow("/P", 405, true)]
		[DataRow("/X", 404, true)]
		public async Task Test_015_CertificateSubjectMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, true)]
		[DataRow("/A/C", 200, true)]
		[DataRow("/B", 200, true)]
		[DataRow("/B/C", 200, true)]
		[DataRow("/P", 405, true)]
		[DataRow("/X", 404, true)]
		public async Task Test_016_CertificateIssuerMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, true)]
		[DataRow("/A/C", 200, true)]
		[DataRow("/B", 200, true)]
		[DataRow("/B/C", 200, true)]
		[DataRow("/P", 405, true)]
		[DataRow("/X", 404, true)]
		public async Task Test_017_CertificateSerialNumberMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 405, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_018_HeaderMatch1(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_019_HeaderMatch2(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		[DataRow("/A?x=1", 403, false)]
		[DataRow("/A/C?x=1", 403, false)]
		[DataRow("/B?x=1", 403, false)]
		[DataRow("/B/C?x=1", 403, false)]
		[DataRow("/P?x=1", 403, false)]
		[DataRow("/X?x=1", 403, false)]
		[DataRow("/A?x=2", 200, false)]
		[DataRow("/A/C?x=2", 200, false)]
		[DataRow("/B?x=2", 200, false)]
		[DataRow("/B/C?x=2", 200, false)]
		[DataRow("/P?x=2", 405, false)]
		[DataRow("/X?x=2", 404, false)]
		public async Task Test_020_QueryMatch(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false, false)]
		[DataRow("/A/C", 200, false, false)]
		[DataRow("/B", 200, false, false)]
		[DataRow("/B/C", 200, false, false)]
		[DataRow("/P", 405, false, false)]
		[DataRow("/X", 404, false, false)]
		[DataRow("/A", 403, false, true)]
		[DataRow("/A/C", 403, false, true)]
		[DataRow("/B", 403, false, true)]
		[DataRow("/B/C", 403, false, true)]
		[DataRow("/P", 403, false, true)]
		[DataRow("/X", 403, false, true)]
		public async Task Test_021_IsBlocked(string Resource, int ExpectedStatusCode,
			bool Encrypted, bool Blocked)
		{
			RemoteEndpoint EP = await auditor.GetAnnotatedStateObject("[::1]", true);
			EP.Blocked = Blocked;

			try
			{
				await this.Get(Resource, ExpectedStatusCode, Encrypted);
			}
			finally
			{
				EP.Blocked = false;
			}
		}

		[TestMethod]
		[DataRow("/A", 200, false, false)]
		[DataRow("/A/C", 200, false, false)]
		[DataRow("/B", 200, false, false)]
		[DataRow("/B/C", 200, false, false)]
		[DataRow("/P", 405, false, false)]
		[DataRow("/X", 404, false, false)]
		[DataRow("/A", 403, false, true)]
		[DataRow("/A/C", 403, false, true)]
		[DataRow("/B", 403, false, true)]
		[DataRow("/B/C", 403, false, true)]
		[DataRow("/P", 403, false, true)]
		[DataRow("/X", 403, false, true)]
		public async Task Test_022_IsTemporarilyBlocked(string Resource, int ExpectedStatusCode,
			bool Encrypted, bool Blocked)
		{
			RemoteEndpoint EP = await auditor.GetAnnotatedStateObject("[::1]", true);

			if (Blocked)
			{
				EP.State[0] = 5;
				EP.Timestamps[0] = DateTime.Now.AddMinutes(1);
			}

			try
			{
				await this.Get(Resource, ExpectedStatusCode, Encrypted);
			}
			finally
			{
				EP.State[0] = 0;
				EP.Timestamps[0] = DateTime.MinValue;
			}
		}

		[TestMethod]
		[DataRow("/A", 200, false, false)]
		[DataRow("/A/C", 200, false, false)]
		[DataRow("/B", 200, false, false)]
		[DataRow("/B/C", 200, false, false)]
		[DataRow("/P", 405, false, false)]
		[DataRow("/X", 404, false, false)]
		[DataRow("/A", 403, false, true)]
		[DataRow("/A/C", 403, false, true)]
		[DataRow("/B", 403, false, true)]
		[DataRow("/B/C", 403, false, true)]
		[DataRow("/P", 403, false, true)]
		[DataRow("/X", 403, false, true)]
		public async Task Test_023_IsPermanentlyBlocked(string Resource, int ExpectedStatusCode,
			bool Encrypted, bool Blocked)
		{
			RemoteEndpoint EP = await auditor.GetAnnotatedStateObject("[::1]", true);
			EP.Blocked = Blocked;

			try
			{
				await this.Get(Resource, ExpectedStatusCode, Encrypted);
			}
			finally
			{
				EP.Blocked = false;
			}
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		[DataRow("/A", 200, true)]
		[DataRow("/A/C", 200, true)]
		[DataRow("/B", 200, true)]
		[DataRow("/B/C", 200, true)]
		[DataRow("/P", 405, true)]
		[DataRow("/X", 404, true)]
		public async Task Test_024_IsUnencrypted(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		[DataRow("/A", 200, true)]
		[DataRow("/A/C", 200, true)]
		[DataRow("/B", 200, true)]
		[DataRow("/B/C", 200, true)]
		[DataRow("/P", 405, true)]
		[DataRow("/X", 404, true)]
		public async Task Test_025_IsEncrypted1(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		[DataRow("/A", 403, true)]
		[DataRow("/A/C", 403, true)]
		[DataRow("/B", 403, true)]
		[DataRow("/B/C", 403, true)]
		[DataRow("/P", 403, true)]
		[DataRow("/X", 403, true)]
		public async Task Test_026_IsEncrypted2(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/Test_027_IsContent.xml", 200, false)]
		[DataRow("/Text/Test_027_IsContent.txt", 200, false)]
		[DataRow("/Text/Test_027_IsContent.png", 404, false)]
		[DataRow("/Test_027_IsContent.manifest", 403, false)]
		public async Task Test_027_IsContent(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false, 1)]
		[DataRow("/A", 200, false, 2)]
		[DataRow("/A", 200, false, 3)]
		[DataRow("/A", 429, false, 4)]
		public async Task Test_028_RequestsExceeded1(string Resource, int ExpectedStatusCode,
			bool Encrypted, int NrRequests)
		{
			while (NrRequests-- > 0)
				await this.Get(Resource, NrRequests == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false, 1, 1, 1)]
		[DataRow("/A", 200, false, 2, 1, 2)]
		[DataRow("/A", 200, false, 3, 1, 3)]
		[DataRow("/A", 429, false, 4, 1, 4)]
		public async Task Test_029_RequestsExceeded2(string Resource, int ExpectedStatusCode,
			bool Encrypted, int NrRequests1, int SecondsDelay, int NrRequests2)
		{
			while (NrRequests1-- > 0)
				await this.Get(Resource, NrRequests1 == 0 ? ExpectedStatusCode : 200, Encrypted);

			await Task.Delay(SecondsDelay * 1000);

			while (NrRequests2-- > 0)
				await this.Get(Resource, NrRequests2 == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/P", "Hello", 200, false, 1)]
		[DataRow("/P", "Hello", 200, false, 2)]
		[DataRow("/P", "Hello", 200, false, 3)]
		[DataRow("/P", "Hello", 200, false, 4)]
		[DataRow("/P", "Hello", 429, false, 5)]
		public async Task Test_030_BytesExceeded1(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted, int NrRequests)
		{
			while (NrRequests-- > 0)
				await this.Post(Resource, Data, NrRequests == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/P", "Hello", 200, false, 1, 1, 1)]
		[DataRow("/P", "Hello", 200, false, 2, 1, 2)]
		[DataRow("/P", "Hello", 200, false, 3, 1, 3)]
		[DataRow("/P", "Hello", 200, false, 4, 1, 4)]
		[DataRow("/P", "Hello", 429, false, 5, 1, 5)]
		public async Task Test_031_BytesExceeded2(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted, int NrRequests1, int SecondsDelay,
			int NrRequests2)
		{
			while (NrRequests1-- > 0)
				await this.Post(Resource, Data, NrRequests1 == 0 ? ExpectedStatusCode : 200, Encrypted);

			await Task.Delay(SecondsDelay * 1000);

			while (NrRequests2-- > 0)
				await this.Post(Resource, Data, NrRequests2 == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false, 1)]
		[DataRow("/A", 200, false, 2)]
		[DataRow("/A", 200, false, 3)]
		[DataRow("/A", 429, false, 4)]
		public async Task Test_032_ConnectionsExceeded1(string Resource, int ExpectedStatusCode,
			bool Encrypted, int NrConnections)
		{
			while (NrConnections-- > 0)
				await this.Get(Resource, NrConnections == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false, 1, 1, 1)]
		[DataRow("/A", 200, false, 2, 1, 2)]
		[DataRow("/A", 200, false, 3, 1, 3)]
		[DataRow("/A", 429, false, 4, 1, 4)]
		public async Task Test_033_ConnectionsExceeded2(string Resource, int ExpectedStatusCode,
			bool Encrypted, int NrConnections1, int SecondsDelay, int NrConnections2)
		{
			while (NrConnections1-- > 0)
				await this.Get(Resource, NrConnections1 == 0 ? ExpectedStatusCode : 200, Encrypted);

			await Task.Delay(SecondsDelay * 1000);

			while (NrConnections2-- > 0)
				await this.Get(Resource, NrConnections2 == 0 ? ExpectedStatusCode : 200, Encrypted);
		}

		[TestMethod]
		[DataRow("/P", "Hello", 200, false)]
		[DataRow("/P", "Hello World", 200, false)]
		[DataRow("/P", "Hello World.", 200, false)]
		[DataRow("/P", "Kilroy was here.", 200, false)]
		[DataRow("/P", "Kilroy was here.....", 200, false)]
		[DataRow("/P", "Kilroy was here......", 429, false)]
		public async Task Test_034_ContentSizeExceeded(string Resource, object Data,
			int ExpectedStatusCode, bool Encrypted)
		{
			await this.Post(Resource, Data, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false, "CL", "Chile", "Valparaiso", "Viña del Mar")]
		[DataRow("/A", 403, false, "SE", "Sweden", "Stockholm", "Värmdö")]
		[DataRow("/A", 200, false, "AB", "Abcdef", "ABC", "City")]
		[DataRow("/A", 200, false, "AB", "Abcdef", "ABC", "Town")]
		[DataRow("/A", 200, false, "AB", "Abcdef", "ABD", "City")]
		[DataRow("/A", 200, false, "CD", "Cdefgh", "ABC", "City")]
		public async Task Test_035_FromIpLocation(string Resource, int ExpectedStatusCode,
			bool Encrypted, string CountryCode, string Country, string Region, string City)
		{
			IpLocalizationService.SetLocation(CountryCode, Country, Region, City, 0, 0);
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 405, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_036_FromIp1(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/B/C", 403, false)]
		[DataRow("/P", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_037_FromIp2(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 200, false)]
		[DataRow("/A/C", 200, false)]
		[DataRow("/B", 200, false)]
		[DataRow("/B/C", 200, false)]
		[DataRow("/P", 405, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_038_FromIp3(string Resource, int ExpectedStatusCode,
			bool Encrypted)
		{
			await this.Get(Resource, ExpectedStatusCode, Encrypted);
		}

	}
}
