using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains information about a collection, in relation to full-text-search.
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public class CollectionInformation
	{
		/// <summary>
		/// Contains information about a collection, in relation to full-text-search.
		/// </summary>
		public CollectionInformation()
		{
			this.IndexCollectionName = string.Empty;
			this.CollectionName = string.Empty;
			this.IndexForFullTextSearch = false;
			this.Properties = Array.Empty<PropertyDefinition>();
		}

		/// <summary>
		/// Contains information about a collection, in relation to full-text-search.
		/// </summary>
		/// <param name="IndexCollectionName">Full-text-search index collection name.</param>
		/// <param name="CollectionName">Full Type Name.</param>
		/// <param name="IndexForFullTextSearch">If class should be indexed for full-text-search.</param>
		/// <param name="Properties">Properties (or fields) to index.</param>
		public CollectionInformation(string IndexCollectionName, string CollectionName,
			bool IndexForFullTextSearch, params PropertyDefinition[] Properties)
		{
			this.IndexCollectionName = IndexCollectionName;
			this.CollectionName = CollectionName;
			this.IndexForFullTextSearch = IndexForFullTextSearch;
			this.Properties = Properties;
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
		/// Properties to index
		/// </summary>
		public PropertyDefinition[] Properties { get; set; }

		/// <summary>
		/// Types of indexed properties.
		/// </summary>
		public PropertyType[] PropertyTypes { get; set; }

		/// <summary>
		/// Adds properties for full-text-search indexation.
		/// </summary>
		/// <param name="Properties">Properties to add.</param>
		/// <returns>If new property names were found and added, changing
		/// the state of the object.</returns>
		public bool AddIndexableProperties(params PropertyDefinition[] Properties)
		{
			Dictionary<string, PropertyDefinition> ByName = new Dictionary<string, PropertyDefinition>();
			bool New = false;

			foreach (PropertyDefinition Property in this.Properties)
				ByName[Property.Definition] = Property;

			foreach (PropertyDefinition Property in Properties)
			{
				if (!ByName.ContainsKey(Property.Definition))
				{
					ByName[Property.Definition] = Property;
					New = true;
				}
			}

			if (!New)
				return false;

			int c = ByName.Count;
			PropertyDefinition[] Definitions = new PropertyDefinition[c];

			ByName.Values.CopyTo(Definitions, 0);

			this.Properties = Definitions;
			this.IndexForFullTextSearch = true;

			return true;
		}

		/// <summary>
		/// Removes properties from full-text-search indexation.
		/// </summary>
		/// <param name="Properties">Properties to remove from indexation.</param>
		/// <returns>If new property names were found and added, changing
		/// the state of the object.</returns>
		public bool RemoveIndexableProperties(params PropertyDefinition[] Properties)
		{
			Dictionary<string, PropertyDefinition> ByName = new Dictionary<string, PropertyDefinition>();
			bool Removed = false;

			foreach (PropertyDefinition Property in this.Properties)
				ByName[Property.Definition] = Property;

			foreach (PropertyDefinition Property in Properties)
			{
				if (ByName.Remove(Property.Definition))
					Removed = true;
			}

			if (!Removed)
				return false;

			int c = ByName.Count;
			PropertyDefinition[] Definitions = new PropertyDefinition[c];

			ByName.Values.CopyTo(Definitions, 0);

			this.Properties = Definitions;
			this.IndexForFullTextSearch = c > 0;

			return true;
		}
	}
}
