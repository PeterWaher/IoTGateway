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
		public DateTime Created { get; set; } = DateTime.MinValue;

		public override bool Equals(object? obj)
		{
			return obj is CustomTokenizationTestClass o &&
				this.IndexedProperty1 == o.IndexedProperty1 &&
				this.IndexedProperty2 == o.IndexedProperty2 &&
				this.NonIndexedProperty1 == o.NonIndexedProperty1 &&
				this.NonIndexedProperty2 == o.NonIndexedProperty2 &&
				this.Created == o.Created;
		}

		public override int GetHashCode()
		{
			int Result = this.ObjectID?.GetHashCode() ?? 0;

			Result ^= Result << 5 ^ this.Created.GetHashCode();

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
