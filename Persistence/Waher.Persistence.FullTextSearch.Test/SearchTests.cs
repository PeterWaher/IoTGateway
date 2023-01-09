using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class SearchTests
	{
		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Clear();
			await CreateDataset();
		}

		public static async Task Clear()
		{
			await Database.Clear("Test");
			await Database.Clear("FullTextSearch");
			await Database.Clear("FullTextSearchObjects");

			await (await Database.GetDictionary("FullTextSearch")).ClearAsync();
			await (await Database.GetDictionary("FullTextSearchCollections")).ClearAsync();
		}

		public static async Task CreateDataset()
		{
			TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>();
			int i, c = TokenReferences.MaxReferences * 5;
			int NrIndexed = 0;

			Task OnIndexed(object Sender, ObjectReferenceEventArgs e)
			{
				NrIndexed++;
				if (NrIndexed == c)
					Done.TrySetResult(true);

				return Task.CompletedTask;
			};

			Task _ = Task.Delay(10000).ContinueWith((_) => Done.TrySetResult(false));

			Search.ObjectAddedToIndex += OnIndexed;
			try
			{
				for (i = 0; i < c; i++)
				{
					switch (i % 5)
					{
						case 0:
							await IndexationTests.CreateInstance(
								"Hello World number " + i.ToString(),
								"Kilroy was here.",
								"Clowns are fun.",
								"Testing indexation.");
							break;

						case 1:
							await IndexationTests.CreateInstance(
								"Hello World number " + i.ToString(),
								"Fitzroy was here.",
								"Clowns are scary.",
								"Testing indexation.");
							break;

						case 2:
							await IndexationTests.CreateInstance(
								"Hello World number " + i.ToString(),
								"Kilroy is a Clown.",
								"Clowns are fun.",
								"Testing indexation.");
							break;

						case 3:
							await IndexationTests.CreateInstance(
								"Hello World number " + i.ToString(),
								"Fitzroy is not a Clown.",
								"Clowns are scary.",
								"Testing indexation.");
							break;

						case 4:
							await IndexationTests.CreateInstance(
								"Hello World number " + i.ToString(),
								"Testing accents with Pelé.",
								"Clowns is the plural form of Clown.",
								"Testing indexation.");
							break;
					}
				}

				Assert.IsTrue(await Done.Task);
			}
			finally
			{
				Search.ObjectAddedToIndex -= OnIndexed;
			}

			IPersistentDictionary Index = await Database.GetDictionary("FullTextSearch");

			foreach (string Key in Index.Keys)
				Console.Out.WriteLine(Key);
		}

		[TestMethod]
		public async Task Test_01_PlainSearch_1()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_02_PlainSearch_2()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(500, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_03_Required()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown +Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_04_Prohibited()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown -Fitzroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(300, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_05_Wildcard_1()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kil* -Clown"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_06_Wildcard_2()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("*roy -Clown"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_07_Regex_1()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("/Kil.*/ -Clown"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_08_Regex_2()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("/.*roy/ -Clown"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_09_Accents()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Pele"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_10_PlainSearch_1_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kilroy", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_11_PlainSearch_2_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown Kilroy", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(500, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_12_Required_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown +Kilroy", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_13_Prohibited_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown -Fitzroy", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(300, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_14_Wildcard_1_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kil* -Clown", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_15_Wildcard_2_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("*roy -Clown", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_16_Regex_1_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("/Kil.*/ -Clown", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_17_Regex_2_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("/.*roy/ -Clown", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_18_Accents_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Pele", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_19_AsPrefixes()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("TEST", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_20_Sequence1()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("'Kilroy was here'", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_21_Sequence2()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("'Kilroy here was'", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(0, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_22_Sequence3()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kilroy 'was here'", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(300, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_23_Sequence4()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kilroy +'was here'", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_24_Sequence5()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("+Kilroy +'was here'", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_25_Sequence6()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("'Kilroy was' here", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(200, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_26_Sequence7()
		{
			TestClass[] SearchResult = await Search.FullTextSearch<TestClass>("FullTextSearch", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("+'Kilroy was' here", true));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(100, SearchResult.Length);
		}

	}
}