using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class SearchTests : SearchTestsBase<TestClass, TestClassSetter>
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