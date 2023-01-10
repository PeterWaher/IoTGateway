using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class SearchCustomTests : SearchTestsBase<CustomTokenizationTestClass, TestClassSetter>
	{
		public SearchCustomTests()
			: base("FullTextSearchCustom")
		{
		}

		[ClassInitialize]
		public static Task ClassInitialize(TestContext _)
		{
			return Initialize("TestCustom", "FullTextSearchCustom");
		}
	}
}