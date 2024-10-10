using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Files.Test.Classes;
using Waher.Runtime.Console;

namespace Waher.Persistence.Files.Test
{
	[TestClass]
	public class PersistenceTests
	{
		[ClassInitialize]
		public static async Task ClassInitialize(TestContext Context)
		{
			try
			{
				IDatabaseProvider p = Database.Provider;
			}
			catch
			{
#if LW
				Database.Register(await FilesProvider.CreateAsync("Data", "Default", 8192, 8192, 8192, Encoding.UTF8, 10000));
#else
				Database.Register(await FilesProvider.CreateAsync("Data", "Default", 8192, 8192, 8192, Encoding.UTF8, 10000, true));
#endif
			}
		}

		[TestMethod]
		public async Task PersistenceTests_01_SaveNew()
		{
			int i;

			for (i = 1; i <= 5; i++)
			{
				await Database.Insert(new StringFields()
				{
					A = string.Empty,
					B = i.ToString(),
					C = null
				});
			}
		}

		[TestMethod]
		public async Task PersistenceTests_02_FindAll()
		{
			foreach (StringFields Obj in await Database.Find<StringFields>())
				ConsoleOut.WriteLine(Obj.ToString());
		}

		[TestMethod]
		public async Task PersistenceTests_03_Pagination_NoIndex()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);
			
			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_04_Pagination_A_Asc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "A"), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_04_Pagination_B_Asc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "B"), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_05_Pagination_A_Desc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "-A"), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_06_Pagination_B_Desc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "-B"), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_07_Pagination_C_Asc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "C"), Objects);
		}

		[TestMethod]
		public async Task PersistenceTests_08_Pagination_C_Desc()
		{
			await Database.Clear("StringFields");

			SortedDictionary<Guid, StringFields> Objects = await CreateObjects(100);

			await AssertAllEnumerated(await Database.Enumerate<StringFields>(20, "-C"), Objects);
		}

		private static async Task AssertAllEnumerated(PaginatedEnumerator<StringFields> e,
			SortedDictionary<Guid, StringFields> Objects)
		{
			while (await e.MoveNextAsync())
			{
				Console.Out.WriteLine(e.Current.ObjectId);
				Assert.IsTrue(Objects.Remove(e.Current.ObjectId), "Object not found or has been earlier removed.");
			}

			Assert.AreEqual(0, Objects.Count);
		}

		private static async Task<SortedDictionary<Guid, StringFields>> CreateObjects(int NrObjects)
		{
			SortedDictionary<Guid, StringFields> Result = new();

			await Database.StartBulk();
			try
			{
				while (NrObjects > 0)
				{
					StringFields Obj = new()
					{
						A = DBFilesBTreeTests.GenerateRandomString(10),
						B = DBFilesBTreeTests.GenerateRandomString(10),
						C = DBFilesBTreeTests.GenerateRandomString(10)
					};

					await Database.Insert(Obj);
					Result[Obj.ObjectId] = Obj;
					NrObjects--;
				}
			}
			finally
			{
				await Database.EndBulk();
			}

			return Result;
		}


	}
}
