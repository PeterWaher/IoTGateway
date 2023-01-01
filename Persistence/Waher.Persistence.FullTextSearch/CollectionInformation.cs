namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains information about a collection, in relation to full-text-search.
	/// </summary>
	public class CollectionInformation
	{
		/// <summary>
		/// Contains information about a collection, in relation to full-text-search.
		/// </summary>
		/// <param name="CollectionName">Full Type Name.</param>
		/// <param name="IndexForFullTextSearch">If class should be indexed for full-text-search.</param>
		/// <param name="PropertyNames">Names of properties (or fields) to index.</param>
		public CollectionInformation()
			: this(string.Empty, false, new string[0])
		{
		}

		/// <summary>
		/// Contains information about a collection, in relation to full-text-search.
		/// </summary>
		/// <param name="CollectionName">Full Type Name.</param>
		/// <param name="IndexForFullTextSearch">If class should be indexed for full-text-search.</param>
		/// <param name="PropertyNames">Names of properties (or fields) to index.</param>
		public CollectionInformation(string CollectionName, bool IndexForFullTextSearch, params string[] PropertyNames)
		{
			this.CollectionName = CollectionName;
			this.IndexForFullTextSearch = IndexForFullTextSearch;
			this.PropertyNames = PropertyNames;
		}

		/// <summary>
		/// Collection Name
		/// </summary>
		public string CollectionName { get; set; }

		/// <summary>
		/// If collection should be indexed.
		/// </summary>
		public bool IndexForFullTextSearch { get; set; }

		/// <summary>
		/// Property names to index
		/// </summary>
		public string[] PropertyNames { get; set; }
	}
}
