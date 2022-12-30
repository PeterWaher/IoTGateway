using System;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// This attribute defines that objects of this type should be indexed in the full-text-search index.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public class FullTextSearchAttribute : Attribute
	{
		private readonly string[] propertyNames;

		/// <summary>
		/// This attribute defines that objects of this type should be indexed in the full-text-search index.
		/// </summary>
		/// <param name="PropertyNames">Array of property (or field) names to index.</param>
		public FullTextSearchAttribute(params string[] PropertyNames)
		{
			this.propertyNames = PropertyNames;
		}

		/// <summary>
		/// Array of property (or field) names to index.
		/// </summary>
		public string[] PropertyNames => this.propertyNames;
	}
}
