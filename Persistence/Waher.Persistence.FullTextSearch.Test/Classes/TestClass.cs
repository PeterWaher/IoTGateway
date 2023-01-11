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

		public override int GetHashCode()
		{
			int Result = this.ObjectID?.GetHashCode() ?? 0;

			if (this.IndexedProperty1 is not null)
				Result ^= Result << 5 ^ this.IndexedProperty1.GetHashCode();

			if (this.IndexedProperty2 is not null)
				Result ^= Result << 5 ^ this.IndexedProperty2.GetHashCode();

			if (this.NonIndexedProperty1 is not null)
				Result ^= Result << 5 ^ this.NonIndexedProperty1.GetHashCode();

			if (this.NonIndexedProperty2 is not null)
				Result ^= Result << 5 ^ this.NonIndexedProperty2.GetHashCode();

			return Result;
		}
	}
}
