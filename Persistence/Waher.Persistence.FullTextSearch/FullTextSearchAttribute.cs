using System;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// This attribute defines that objects of this type should be indexed in the full-text-search index.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class FullTextSearchAttribute : Attribute
	{
		private readonly string indexCollection;
		private readonly string[] propertyNames;

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		/// <param name="PropertyNames">Array of property (or field) names to index.</param>
		public FullTextSearchAttribute(string IndexCollection, params string[] PropertyNames)
		{
			this.indexCollection = IndexCollection;
			this.propertyNames = PropertyNames;
		}

		/// <summary>
		/// Name of full-text-search index collection.
		/// </summary>
		public string IndexCollection => this.indexCollection;

		/// <summary>
		/// Array of property (or field) names to index.
		/// </summary>
		public string[] PropertyNames => this.propertyNames;
	}
}
