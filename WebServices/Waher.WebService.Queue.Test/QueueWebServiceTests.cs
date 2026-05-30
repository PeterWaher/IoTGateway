using System.Text;
using Waher.Content;
using Waher.Content.Html;
using Waher.Content.Markdown;
using Waher.Content.Markdown.Web;
using Waher.Content.Multipart;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;
using Waher.Script;
using Waher.Security.Users;

namespace Waher.WebService.Queue.Test
{
	[TestClass]
	public sealed class QueueWebServiceTests
	{
		private static FilesProvider? filesProvider = null;
		private static HttpServer? server = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(QueueWebServiceTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(QueueServiceModule).Assembly,
				typeof(Log).Assembly,
				typeof(Expression).Assembly,
				typeof(MarkdownDocument).Assembly,
				typeof(MarkdownToHtmlConverter).Assembly,
				typeof(HttpServer).Assembly,
				typeof(HtmlDocument).Assembly);

			Log.Register(new ConsoleEventSink());

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			string RootFolder = Path.GetFullPath("Root");

			server = new HttpServer(8081);
			Types.SetModuleParameter("HTTP", server);
			Types.SetModuleParameter("Root", RootFolder);

			server.Register(new HttpFolderResource(string.Empty, RootFolder, false, false, true, true));

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();

			if (server is not null)
			{
				await server.DisposeAsync();
				server = null;
			}

			if (filesProvider is not null)
			{
				await filesProvider.DisposeAsync();
				filesProvider = null;
			}
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			Role Role = await Roles.GetRole("Test", true);

			if (!Role.HasPrivilege("Admin.Queues.Test"))
			{
				Role.Privileges = [new("Admin\\.Queues\\..*", true)];
				await Database.Update(Role);
			}

			User User = await Users.GetUser("Test", true);

			if (!User.HasPrivilege("Admin.Queues.Test"))
			{
				User.RoleIds = ["Test"];
				User.PasswordHash = Convert.ToBase64String(Users.ComputeHash("Test", "Test"));
				await Database.Update(User);
			}
		}

		[TestMethod]
		public async Task Test_01_GetApiDocumentation()
		{
			ContentResponse Response = await InternetContent.GetAsync(
				new Uri("http://localhost:8081/Queues"),
				new KeyValuePair<string, string>("Accept", "text/html"));

			Response.AssertOk();

			Assert.IsTrue(Response.Decoded is HtmlDocument);
		}

		[TestMethod]
		public async Task Test_02_EnqueueDequeue()
		{
			ContentResponse Response = await InternetContent.PutAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				"Test message",
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);

			Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.AreEqual("Test message", Response.Decoded);
		}

		[TestMethod]
		public async Task Test_03_DequeueEnqueue()
		{
			_ = Task.Delay(2000).ContinueWith(async (_) =>
			{
				await InternetContent.PutAsync(
					new Uri("http://localhost:8081/Queues/Test"),
					"Test message",
					new KeyValuePair<string, string>("Authorization", "Basic " +
					Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));
			});

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.AreEqual("Test message", Response.Decoded);
		}

		[TestMethod]
		public async Task Test_04_Clear()
		{
			ContentResponse Response = await InternetContent.DeleteAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				null,
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);
		}

		[TestMethod]
		public async Task Test_05_MultiEnqueueDequeue()
		{
			ContentResponse Response = await InternetContent.PutAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				new MixedContent(
				[
					await EmbeddedContent.Encode("Test message 1"),
					await EmbeddedContent.Encode("Test message 2")
				]),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);

			Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Count=2"),
				null,
				new KeyValuePair<string, string>("Accept", "multipart/mixed"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			MixedContent? Mixed = Response.Decoded as MixedContent;
			Assert.IsNotNull(Mixed);
			Assert.AreEqual(2, Mixed.Content.Length);
			Assert.AreEqual("Test message 1", Mixed.Content[0].Decoded);
			Assert.AreEqual("Test message 2", Mixed.Content[1].Decoded);
		}

		[TestMethod]
		public async Task Test_06_MinTimeout()
		{
			_ = Task.Delay(2000).ContinueWith(async (_) =>
			{
				await InternetContent.PutAsync(
					new Uri("http://localhost:8081/Queues/Test"),
					"Test message",
					new KeyValuePair<string, string>("Authorization", "Basic " +
					Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));
			});

			ContentResponse Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Count=2&MinTimeout=2000&Timeout=30000"),
				null,
				new KeyValuePair<string, string>("Accept", "multipart/mixed"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			MixedContent? Mixed = Response.Decoded as MixedContent;
			Assert.IsNotNull(Mixed);
			Assert.AreEqual(1, Mixed.Content.Length);
			Assert.AreEqual("Test message", Mixed.Content[0].Decoded);
		}

		[TestMethod]
		public async Task Test_07_Timeout()
		{
			ContentResponse Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Timeout=2000"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);
		}

		[TestMethod]
		public async Task Test_08_Peek()
		{
			ContentResponse Response = await InternetContent.PutAsync(
				new Uri("http://localhost:8081/Queues/Test"),
				"Test message",
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);

			Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Peek=1&Timeout=2000"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.AreEqual("Test message", Response.Decoded);

			Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Timeout=2000"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.AreEqual("Test message", Response.Decoded);

			Response = await InternetContent.PostAsync(
				new Uri("http://localhost:8081/Queues/Test?Timeout=2000"),
				null,
				new KeyValuePair<string, string>("Accept", "*/*"),
				new KeyValuePair<string, string>("Authorization", "Basic " +
				Convert.ToBase64String(Encoding.ASCII.GetBytes("Test:Test"))));

			Response.AssertOk();

			Assert.IsNull(Response.Decoded);
		}

	}
}
