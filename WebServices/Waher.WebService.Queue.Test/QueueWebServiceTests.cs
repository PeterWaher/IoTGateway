using System.Text;
using Waher.Content;
using Waher.Events;
using Waher.Events.Console;
using Waher.Networking.HTTP;
using Waher.Persistence;
using Waher.Persistence.Files;
using Waher.Persistence.Queues;
using Waher.Runtime.Inventory;
using Waher.Runtime.IO;

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
			Types.Initialize(typeof(QueueWebServiceTests).Assembly,
				typeof(InternetContent).Assembly,
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(MultiFileQueue).Assembly,
				typeof(Log).Assembly);

			Log.Register(new ConsoleEventSink());

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			server = new HttpServer(8081);

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

		[TestMethod]
		public void TestMethod1()
		{
		}
	}
}
