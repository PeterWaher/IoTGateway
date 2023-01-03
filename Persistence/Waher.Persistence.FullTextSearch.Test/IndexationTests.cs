using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Waher.Events;
using Waher.Events.Console;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class IndexationTests
	{
		private static FilesProvider? filesProvider = null;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(FullTextSearchModule).Assembly,
				typeof(TestClass).Assembly);

			filesProvider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(filesProvider);

			Log.Register(new ConsoleEventSink());

			await Types.StartAllModules(10000);
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			Log.Terminate();

			await Types.StopAllModules();

			filesProvider?.Dispose();
			filesProvider = null;
		}

		[TestMethod]
		public async Task Test_01_InsertObject()
		{
			TaskCompletionSource<bool> Result = new();
			Task ObjectIndexed(object Sender, ObjectReferenceEventArgs e)
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			};

			FullTextSearchModule.ObjectAddedToIndex += ObjectIndexed;
			try
			{
				TestClass Obj = new()
				{
					IndexedProperty1 = "Hello World.",
					IndexedProperty2 = "Kilroy was here.",
					NonIndexedProperty1 = "This is a test.",
					NonIndexedProperty2 = "Testing indexation."
				};

				await Database.Insert(Obj);

				Assert.IsTrue(await Result.Task);
			}
			finally
			{
				FullTextSearchModule.ObjectAddedToIndex -= ObjectIndexed;
			}
		}

		// TODO: GenericObject tests
		// TODO: Script-only tests
	}
}