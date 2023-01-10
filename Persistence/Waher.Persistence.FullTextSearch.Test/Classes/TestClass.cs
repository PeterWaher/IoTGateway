using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch.Test.Classes
{
	[CollectionName("Test")]
	[FullTextSearch("FullTextSearch", nameof(IndexedProperty1), nameof(IndexedProperty2))]
	public class TestClass : ITestClass
	{
		public TestClass()
		{
		}

		[ObjectId]
		public string? ObjectID { get; set; }

		public string IndexedProperty1 { get; set; } = string.Empty;
		public string IndexedProperty2 { get; set; } = string.Empty;
		public string NonIndexedProperty1 { get; set; } = string.Empty;
		public string NonIndexedProperty2 { get; set; } = string.Empty;

		public override bool Equals(object? obj)
		{
			return obj is TestClass o &&
				this.IndexedProperty1 == o.IndexedProperty1 &&
				this.IndexedProperty2 == o.IndexedProperty2 &&
				this.NonIndexedProperty1 == o.NonIndexedProperty1 &&
				this.NonIndexedProperty2 == o.NonIndexedProperty2;
		}
	}
}
