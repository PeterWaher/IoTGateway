using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch.Test.Classes
{
    [CollectionName("TestCustom")]
    [FullTextSearch("FullTextSearchCustom")]
    public class CustomTokenizationTestClass : ITestClass
	{
        public CustomTokenizationTestClass()
        {
        }

        [ObjectId]
        public string? ObjectID { get; set; }

        public string IndexedProperty1 { get; set; } = string.Empty;
        public string IndexedProperty2 { get; set; } = string.Empty;
        public string NonIndexedProperty1 { get; set; } = string.Empty;
        public string NonIndexedProperty2 { get; set; } = string.Empty;
    }
}
