using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Persistence.FullTextSearch.Keywords;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class FilesTests
	{
		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Database.Clear("FullTextSearchFiles");
			await Database.Clear("FullTextSearchObjects");

			await (await Database.GetDictionary("Files")).ClearAsync();
		}

		[TestMethod]
		public async Task Test_01_IndexFolder()
		{
			TaskCompletionSource<bool> Result = new();
			int Count = 0;
			int NrProcessed = 0;

			Task ObjectProcessed(object Sender, ObjectReferenceEventArgs e)
			{
				NrProcessed++;
				if (Count > 0 && NrProcessed == Count)
					Result.TrySetResult(true);

				return Task.CompletedTask;
			};

			Task _ = Task.Delay(10000).ContinueWith((_) => Result.TrySetResult(false));

			Search.ObjectAddedToIndex += ObjectProcessed;
			Search.ObjectRemovedFromIndex += ObjectProcessed;
			Search.ObjectUpdatedInIndex += ObjectProcessed;

			try
			{
				FolderIndexationStatistics Stat = await Search.IndexFolder("Files", "Files", true);

				Console.Out.WriteLine("NrAdded: " + Stat.NrAdded.ToString());
				Console.Out.WriteLine("NrUpdated: " + Stat.NrUpdated.ToString());
				Console.Out.WriteLine("NrDeleted: " + Stat.NrDeleted.ToString());
				Console.Out.WriteLine("NrFiles: " + Stat.NrFiles.ToString());
				Console.Out.WriteLine("TotalChanges: " + Stat.TotalChanges.ToString());

				Count = Stat.TotalChanges;
				if (Count == NrProcessed)
					Result.TrySetResult(true);

				Assert.AreEqual(Stat.TotalChanges, Stat.NrAdded + Stat.NrUpdated + Stat.NrDeleted);
				Assert.IsTrue(await Result.Task, "Objects not processed correctly.");
			}
			finally
			{
				Search.ObjectAddedToIndex -= ObjectProcessed;
				Search.ObjectRemovedFromIndex -= ObjectProcessed;
				Search.ObjectUpdatedInIndex -= ObjectProcessed;
			}
		}

		[TestMethod]
		public async Task Test_02_IndexFile()
		{
			bool Result = await Search.IndexFile("Files", "Files/1/1.txt");

			Console.Out.WriteLine(Result.ToString());
		}

		[TestMethod]
		public async Task Test_03_PlainSearch_1()
		{
			FileReference[] SearchResult = await this.DoSearch("Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(10, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_04_PlainSearch_2()
		{
			FileReference[] SearchResult = await this.DoSearch("Hello Clown Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(25, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_05_Required()
		{
			FileReference[] SearchResult = await this.DoSearch("Hello Clown +Kilroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(10, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_06_Prohibited()
		{
			FileReference[] SearchResult = await this.DoSearch("Hello Clown -Fitzroy");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(15, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_07_Wildcard_1()
		{
			FileReference[] SearchResult = await this.DoSearch("Kil* -Clown");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(5, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_08_Wildcard_2()
		{
			FileReference[] SearchResult = await this.DoSearch("*roy -Clown");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(10, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_09_Regex_1()
		{
			FileReference[] SearchResult = await this.DoSearch("/Kil.*/ -Clown");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(5, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_10_Regex_2()
		{
			FileReference[] SearchResult = await this.DoSearch("/.*roy/ -Clown");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(10, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_11_Accents()
		{
			FileReference[] SearchResult = await this.DoSearch("Pele");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(5, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_12_Regex_Range()
		{
			FileReference[] SearchResult = await this.DoSearch("'number /[1-2]/'");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(10, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_13_Regex_RecurringWords()
		{
			FileReference[] SearchResult = await this.DoSearch("'word document'");

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(5, SearchResult.Length);
		}

		private async Task<FileReference[]> DoSearch(string Query)
		{ 
			Keyword[] Keywords = Search.ParseKeywords(Query, false);
			FileReference[] SearchResult = await Search.FullTextSearch<FileReference>("Files", 0, int.MaxValue,
				FullTextSearchOrder.Relevance, PaginationStrategy.PaginateOverObjectsNullIfIncompatible, Keywords);

			return SearchResult;
		}
	}
}