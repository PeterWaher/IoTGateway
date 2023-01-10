using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class IndexationCustomTests : IndexationTestsBase<CustomTokenizationTestClass>
	{
		public IndexationCustomTests()
			: base("TestCustom", "FullTextSearchCustom")
		{
		}
	}
}