using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	[DoNotParallelize]
	public class SearchCustomTestsReindexation : SearchTestsBase<CustomTokenizationTestClass, TestClassAccess>
	{
		public SearchCustomTestsReindexation()
			: base("FullTextSearchCustom")
		{
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Initialize("TestCustom", "FullTextSearchCustom");
			await Search.ReindexCollection("FullTextSearchCustom");
		}
	}
}