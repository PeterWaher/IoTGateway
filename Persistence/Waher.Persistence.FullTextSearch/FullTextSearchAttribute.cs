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
		private readonly bool hasPropertyNames;

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="IndexCollection">Name of full-text-search index collection.</param>
		/// <param name="PropertyNames">Array of property (or field) names to index. 
		/// If not provided, and a <see cref="ITokenizer"/> exists for objects of this
		/// class, that tokenizer will be used instead of the property array, to extract
		/// tokens from the object.</param>
		public FullTextSearchAttribute(string IndexCollection, params string[] PropertyNames)
		{
			this.indexCollection = IndexCollection;
			this.propertyNames = PropertyNames;
			this.hasPropertyNames = (PropertyNames?.Length ?? 0) > 0;
		}

		/// <summary>
		/// Name of full-text-search index collection.
		/// </summary>
		public string IndexCollection => this.indexCollection;

		/// <summary>
		/// Array of property (or field) names to index.
		/// </summary>
		public string[] PropertyNames => this.propertyNames;

		/// <summary>
		/// If property names are defined for this class (true), or
		/// if objects are to be tokenized using a specialized tokenizer (false).
		/// </summary>
		public bool HasPropertyNames => this.hasPropertyNames;
	}
}
