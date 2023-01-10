using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	public abstract class IndexationTestsBase<T>
		where T : class, ITestClass
	{
		private readonly string collectioName;
		private readonly string indexCollection;

		public IndexationTestsBase(string CollectioName, string IndexCollection)
		{
			this.collectioName = CollectioName;
			this.indexCollection = IndexCollection;
		}

		[TestMethod]
		public async Task Test_01_InsertObject()
		{
			await CreateInstance();
		}

		public static Task<T> CreateInstance()
		{
			return CreateInstance("Hello World.", "Kilroy was here.",
				"This is a test.", "Testing indexation.");
		}

		public static async Task<T> CreateInstance(string IndexedProperty1,
			string IndexedProperty2, string NonIndexedProperty1,
			string NonIndexedProperty2)
		{
			TaskCompletionSource<bool> Result = new();
			Task ObjectIndexed(object Sender, ObjectReferenceEventArgs e)
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			};

			Search.ObjectAddedToIndex += ObjectIndexed;
			try
			{
				T Obj = Activator.CreateInstance<T>();
				Obj.IndexedProperty1 = IndexedProperty1;
				Obj.IndexedProperty2 = IndexedProperty2;
				Obj.NonIndexedProperty1 = NonIndexedProperty1;
				Obj.NonIndexedProperty2 = NonIndexedProperty2;

				await Database.Insert(Obj);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);

				return Obj;
			}
			finally
			{
				Search.ObjectAddedToIndex -= ObjectIndexed;
			}
		}

		[TestMethod]
		public async Task Test_02_Search()
		{
			await this.Clear();
			await CreateInstance();

			T[] SearchResult = await Search.FullTextSearch<T>(this.indexCollection, 0, 10,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.IsTrue(SearchResult.Length > 0, "No objects found.");
		}

		[TestMethod]
		public async Task Test_03_DeleteObject()
		{
			await this.Clear();
			await CreateInstance();
			await this.DeleteAllObjects();
		}

		private async Task DeleteAllObjects()
		{
			IEnumerable<T> Objects = await Database.Find<T>();
			int NrObjects = 0;

			foreach (T Obj in Objects)
				NrObjects++;

			if (NrObjects == 0)
				Assert.Fail("No objects to delete. (Make sure you've run test 01 first, to insert at least one object.)");

			TaskCompletionSource<bool> Result = new();
			Task ObjectRemoved(object Sender, ObjectReferenceEventArgs e)
			{
				if (--NrObjects == 0)
					Result.TrySetResult(true);

				return Task.CompletedTask;
			};

			Search.ObjectRemovedFromIndex += ObjectRemoved;
			try
			{
				await Database.Delete(Objects);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);
			}
			finally
			{
				Search.ObjectRemovedFromIndex -= ObjectRemoved;
			}

			T[] SearchResult = await Search.FullTextSearch<T>(this.indexCollection, 0, 10,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Hello Clown Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(0, SearchResult.Length);
		}

		[TestMethod]
		public async Task Test_04_UpdateObject()
		{
			await this.Clear();

			T Obj = await CreateInstance();
			TaskCompletionSource<bool> Result = new();
			Task ObjectUpdated(object Sender, ObjectReferenceEventArgs e)
			{
				Result.TrySetResult(true);
				return Task.CompletedTask;
			};

			Search.ObjectUpdatedInIndex += ObjectUpdated;
			try
			{
				Obj.IndexedProperty2 = "Roy was here.";

				await Database.Update(Obj);

				Task _ = Task.Delay(5000).ContinueWith((_) => Result.TrySetResult(false));

				Assert.IsTrue(await Result.Task);
			}
			finally
			{
				Search.ObjectUpdatedInIndex -= ObjectUpdated;
			}

			T[] SearchResult = await Search.FullTextSearch<T>(this.indexCollection, 0, 10,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Kilroy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(0, SearchResult.Length);

			SearchResult = await Search.FullTextSearch<T>(this.indexCollection, 0, 10,
				FullTextSearchOrder.Relevance, Search.ParseKeywords("Roy"));

			Assert.IsNotNull(SearchResult);
			Assert.AreEqual(1, SearchResult.Length);
		}

		private async Task Clear()
		{
			await Database.Clear(this.collectioName);
			await Database.Clear("FullTextSearchObjects");

			await (await Database.GetDictionary(this.indexCollection)).ClearAsync();
		}

	}
}