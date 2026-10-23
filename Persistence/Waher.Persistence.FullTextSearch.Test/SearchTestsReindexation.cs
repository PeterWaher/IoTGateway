using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	[DoNotParallelize]
	public class SearchTestsReindexation : SearchTestsBase<TestClass, TestClassAccess>
	{
		public SearchTestsReindexation()
			: base("FullTextSearch")
		{
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Initialize("Test", "FullTextSearch");
			await Search.ReindexCollection("FullTextSearch");
		}
	}
}