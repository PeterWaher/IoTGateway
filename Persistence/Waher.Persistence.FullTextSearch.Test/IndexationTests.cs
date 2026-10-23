using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
    [TestClass]
	[DoNotParallelize]
	public class IndexationTests : IndexationTestsBase<TestClass, TestClassAccess>
	{
		public IndexationTests()
			: base("Test", "FullTextSearch")
		{
		}
	}
}