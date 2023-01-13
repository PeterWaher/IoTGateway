using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
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