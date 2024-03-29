using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Persistence.FullTextSearch.Test.Classes;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch.Test
{
	[TestClass]
	public class IndexationGenericTests : IndexationTestsBase<GenericObject, GenericAccess>
	{
		public IndexationGenericTests()
			: base("TestGeneric", "FullTextSearchGeneric")
		{
		}

		[ClassInitialize]
		public static async Task ClassInitialize(TestContext _)
		{
			await Search.SetFullTextSearchIndexCollection("FullTextSearchGeneric", "TestGeneric");
			await Search.AddFullTextSearch("TestGeneric", 
				new PropertyDefinition("IndexedProperty1"), 
				new PropertyDefinition("IndexedProperty2"));
		}
	}
}