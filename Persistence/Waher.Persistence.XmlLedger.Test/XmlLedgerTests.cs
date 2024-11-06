using System.Text;
using Waher.Persistence.Files;
using Waher.Persistence.Serialization;
using Waher.Persistence.XmlLedger.Test.Classes;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.XmlLedger.Test
{
	[TestClass]
	public class XmlLedgerTests
	{
		private static FilesProvider? provider;
		private static TestContext? context;
		private XmlFileLedger? ledger;

		[AssemblyInitialize]
		public static async Task AssemblyInitialize(TestContext _)
		{
			Types.Initialize(
				typeof(Database).Assembly,
				typeof(FilesProvider).Assembly,
				typeof(ObjectSerializer).Assembly,
				typeof(XmlLedgerTests).Assembly);

			provider = await FilesProvider.CreateAsync("Data", "Default", 8192, 10000, 8192, Encoding.UTF8, 10000, true);
			Database.Register(provider, false);

			Assert.IsTrue(await Types.StartAllModules(10000));
			Ledger.StartListeningToDatabaseEvents();
		}

		[ClassInitialize]
		public static Task ClassInitialize(TestContext Context)
		{
			context = Context;
			return Task.CompletedTask;
		}

		[AssemblyCleanup]
		public static async Task AssemblyCleanup()
		{
			await Types.StopAllModules();
			Ledger.StopListeningToDatabaseEvents();
		}

		[TestInitialize]
		public async Task TestInitialize()
		{
			string FileName = Path.Combine("Data", (context?.TestName ?? "Test") + ".xml");

			if (File.Exists(FileName))
				File.Delete(FileName);

			Ledger.Register(this.ledger = new XmlFileLedger(FileName, 7), false);
			await this.ledger.Start();
		}

		[TestCleanup]
		public async Task TestCleanup()
		{
			await Task.Delay(1000);

			if (this.ledger is not null)
				await this.ledger.Stop();
		}

		[TestMethod]
		public async Task Test_01_NewEntry()
		{
			await CreateNewObject();
		}

		private static async Task<SimpleObject> CreateNewObject()
		{
			SimpleObject Result = new()
			{
				Text = "Hello World",
				Number1 = 10,
				Number2 = 3.1415927,
				Flag = true
			};

			await Database.Insert(Result);

			return Result;
		}

		[TestMethod]
		public async Task Test_02_UpdateEntry()
		{
			SimpleObject Obj = await CreateNewObject();
			int i;

			for (i = 0; i < 10; i++)
			{
				Obj.Text += "...";
				Obj.Number1++;
				Obj.Number2--;
				Obj.Flag = !Obj.Flag;

				await Database.Update(Obj);
			}
		}

		[TestMethod]
		public async Task Test_03_DeleteEntry()
		{
			SimpleObject Obj = await CreateNewObject();
			await Database.Delete(Obj);
		}

		[TestMethod]
		public async Task Test_04_ClearCollection()
		{
			await CreateNewObject();
			await Database.Clear("Test");
		}

	}
}