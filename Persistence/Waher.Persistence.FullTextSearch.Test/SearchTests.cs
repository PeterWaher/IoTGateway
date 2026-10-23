using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	[DoNotParallelize]
	public class SearchTests : SearchTestsBase<TestClass, TestClassAccess>
	{
		public SearchTests()
			: base("FullTextSearch")
		{
		}

		[ClassInitialize]
		public static Task ClassInitialize(TestContext _)
		{
			return Initialize("Test", "FullTextSearch");
		}
	}
}