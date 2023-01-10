using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class SearchGenericTests : SearchTestsBase<GenericObject, GenericSetter>
	{
		public SearchGenericTests()
			: base("FullTextSearchGeneric")
		{
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Search.SetFullTextSearchIndexCollection("FullTextSearchGeneric", "TestGeneric");
			await Search.AddFullTextSearch("TestGeneric", "IndexedProperty1", "IndexedProperty2");
			await Initialize("TestGeneric", "FullTextSearchGeneric");
		}
	}
}