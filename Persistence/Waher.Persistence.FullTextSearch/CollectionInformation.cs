using System.Collections.Generic;

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
		public CollectionInformation()
			: this(string.Empty, string.Empty, false, new string[0])
		{
		}

		/// <summary>
		/// Contains information about a collection, in relation to full-text-search.
		/// </summary>
		/// <param name="IndexCollectionName">Full-text-search index collection name.</param>
		/// <param name="CollectionName">Full Type Name.</param>
		/// <param name="IndexForFullTextSearch">If class should be indexed for full-text-search.</param>
		/// <param name="PropertyNames">Names of properties (or fields) to index.</param>
		public CollectionInformation(string IndexCollectionName, string CollectionName,
			bool IndexForFullTextSearch, params string[] PropertyNames)
		{
			this.IndexCollectionName = IndexCollectionName;
			this.CollectionName = CollectionName;
			this.IndexForFullTextSearch = IndexForFullTextSearch;
			this.PropertyNames = PropertyNames;
		}

		/// <summary>
		/// Index Collection Name
		/// </summary>
		public string IndexCollectionName { get; set; }

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

		/// <summary>
		/// Adds properties for full-text-search indexation.
		/// </summary>
		/// <param name="Properties">Property names to add.</param>
		/// <returns>If new property names were found and added, changing
		/// the state of the object.</returns>
		public bool AddIndexableProperties(params string[] Properties)
		{
			Dictionary<string, bool> ByName = new Dictionary<string, bool>();
			bool New = false;

			foreach (string Name in this.PropertyNames)
				ByName[Name] = true;

			foreach (string Name in Properties)
			{
				if (!ByName.ContainsKey(Name))
				{
					ByName[Name] = true;
					New = true;
				}
			}

			if (!New)
				return false;

			int c = ByName.Count;
			string[] Names = new string[c];

			ByName.Keys.CopyTo(Names, 0);

			this.PropertyNames = Names;
			this.IndexForFullTextSearch = true;

			return true;
		}

		/// <summary>
		/// Removes properties from full-text-search indexation.
		/// </summary>
		/// <param name="Properties">Properties to remove from indexation.</param>
		/// <returns>If new property names were found and added, changing
		/// the state of the object.</returns>
		public bool RemoveIndexableProperties(params string[] Properties)
		{
			Dictionary<string, bool> ByName = new Dictionary<string, bool>();
			bool Removed = false;

			foreach (string Name in this.PropertyNames)
				ByName[Name] = true;

			foreach (string Name in Properties)
			{
				if (ByName.Remove(Name))
					Removed = true;
			}

			if (!Removed)
				return false;

			int c = ByName.Count;
			string[] Names = new string[c];

			ByName.Keys.CopyTo(Names, 0);

			this.PropertyNames = Names;
			this.IndexForFullTextSearch = c > 0;

			return true;
		}
	}
}
