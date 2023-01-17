using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class SearchGenericTests : SearchTestsBase<GenericObject, GenericAccess>
	{
		public SearchGenericTests()
			: base("FullTextSearchGeneric")
		{
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Search.SetFullTextSearchIndexCollection("FullTextSearchGeneric", "TestGeneric");
			await Search.AddFullTextSearch("TestGeneric",
				new PropertyDefinition("IndexedProperty1"), 
				new PropertyDefinition("IndexedProperty2"));
			await Initialize("TestGeneric", "FullTextSearchGeneric");
		}
	}
}