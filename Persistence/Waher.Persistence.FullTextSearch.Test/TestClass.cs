using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch.Test
{
	[CollectionName("Test")]
	[FullTextSearch("FullTextSearch", nameof(IndexedProperty1), nameof(IndexedProperty2))]
	public class TestClass
	{
		[ObjectId]
		public string? ObjectID { get; set; }

		public string IndexedProperty1 { get; set; } = string.Empty;
		public string IndexedProperty2 { get; set; } = string.Empty;
		public string NonIndexedProperty1 { get; set; } = string.Empty;
		public string NonIndexedProperty2 { get; set; } = string.Empty;
	}
}
