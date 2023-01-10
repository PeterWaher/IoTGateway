using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
    [TestClass]
	public class IndexationTests : IndexationTestsBase<TestClass>
	{
		public IndexationTests()
			: base("Test", "FullTextSearch")
		{
		}
	}
}