using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Waher.Content;
using Waher.Content.Getters;
using Waher.Networking.HTTP;
using Waher.Networking.HTTP.Interfaces;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
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

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Runtime.Inventory.Types.Initialize(
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
			X509Certificate2 Certificate = Certificates.LoadCertificate("Waher.Security.WAF.Test.Data.certificate.pfx", "testexamplecom");  // Certificate from http://www.cert-depot.com/
			this.server = new(8081, 8088, Certificate)
			{
				WebApplicationFirewall = Firewall
			};

			this.server.Register("/A", (req, resp) => resp.Return("Hello World!"));
			this.server.Register("/A/C", (req, resp) => resp.Return("Hello again."));
			this.server.Register("/B", (req, resp) => resp.Return("Kilroy was here..."));
		}

		private async Task CloseServer()
		{
			if (this.server is not null)
			{
				await this.server?.DisposeAsync();
				this.server = null;
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

		private static Task Get(string RelativeUrl, int ExpectedStatusCode, bool Encrypted)
		{
			return Get(RelativeUrl, ExpectedStatusCode, null, Encrypted);
		}

		private static Task Get(string RelativeUrl, Type ExpectedException, bool Encrypted)
		{
			return Get(RelativeUrl, 0, ExpectedException, Encrypted);
		}

		private static async Task Get(string RelativeUrl, int ExpectedStatusCode, 
			Type ExpectedException, bool Encrypted)
		{
			Uri Uri;

			if (Encrypted)
				Uri = new Uri("https://localhost:8088" + RelativeUrl);
			else
				Uri = new Uri("http://localhost:8081" + RelativeUrl);

			ContentResponse Response;


			try
			{
				Response = await InternetContent.GetAsync(Uri);
			}
			catch (Exception ex)
			{
				Response = new ContentResponse(ex);
			}

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
		[DataRow("/X", 404, false)]
		public async Task Test_001_Allow(string Resource, int ExpectedStatusCode, bool Encrypted)
		{
			await Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 403, false)]
		[DataRow("/A/C", 403, false)]
		[DataRow("/B", 403, false)]
		[DataRow("/X", 403, false)]
		public async Task Test_002_Forbid(string Resource, int ExpectedStatusCode, bool Encrypted)
		{
			await Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 404, false)]
		[DataRow("/A/C", 404, false)]
		[DataRow("/B", 404, false)]
		[DataRow("/X", 404, false)]
		public async Task Test_003_NotFound(string Resource, int ExpectedStatusCode, bool Encrypted)
		{
			await Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", 429, false)]
		[DataRow("/A/C", 429, false)]
		[DataRow("/B", 429, false)]
		[DataRow("/X", 429, false)]
		public async Task Test_004_RateLimited(string Resource, int ExpectedStatusCode, bool Encrypted)
		{
			await Get(Resource, ExpectedStatusCode, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", typeof(TaskCanceledException), false)]
		[DataRow("/A/C", typeof(TaskCanceledException), false)]
		[DataRow("/B", typeof(TaskCanceledException), false)]
		[DataRow("/X", typeof(TaskCanceledException), false)]
		public async Task Test_005_Ignore(string Resource, Type ExpectedException, bool Encrypted)
		{
			await Get(Resource, ExpectedException, Encrypted);
		}

		[TestMethod]
		[DataRow("/A", typeof(HttpRequestException), false)]
		[DataRow("/A/C", typeof(HttpRequestException), false)]
		[DataRow("/B", typeof(HttpRequestException), false)]
		[DataRow("/X", typeof(HttpRequestException), false)]
		public async Task Test_006_Close(string Resource, Type ExpectedException, bool Encrypted)
		{
			await Get(Resource, ExpectedException, Encrypted);
		}

	}
}
