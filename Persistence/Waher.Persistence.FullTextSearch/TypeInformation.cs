using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Persistence.Serialization;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains information about a type, in relation to full-text-search.
	/// </summary>
	internal class TypeInformation
	{
		private readonly IEnumerable<FullTextSearchAttribute> searchAttributes;
		private readonly FullTextSearchAttribute firstSearchAttribute;
		private readonly ITokenizer customTokenizer;
		private readonly PropertyDefinition[] properties;
		private readonly bool hasCustomTokenizer;
		private readonly bool hasPropertyDefinitions;
		private readonly bool dynamicIndexCollection;

		/// <summary>
		/// Contains information about a type, in relation to full-text-search.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="TypeInfo">Type information.</param>
		/// <param name="CollectionName">Collection name for objects of the corresponding type.</param>
		/// <param name="CollectionInformation">Indexing information for a collection.</param>
		/// <param name="CustomTokenizer">Optional custom tokenizer creating tokens for objects
		/// of this type.</param>
		/// <param name="SearchAttributes">Full-text-search attributes.</param>
		public TypeInformation(Type Type, TypeInfo TypeInfo, string CollectionName,
			CollectionInformation CollectionInformation, ITokenizer CustomTokenizer,
			IEnumerable<FullTextSearchAttribute> SearchAttributes)
		{
			this.Type = Type;
			this.TypeInfo = TypeInfo;
			this.CollectionName = CollectionName;
			this.HasCollection = !string.IsNullOrEmpty(CollectionName);
			this.CollectionInformation = CollectionInformation;
			this.customTokenizer = CustomTokenizer;
			this.hasCustomTokenizer = !(CustomTokenizer is null);
			this.searchAttributes = SearchAttributes;
			this.firstSearchAttribute = null;

			this.dynamicIndexCollection = false;
			this.hasPropertyDefinitions = false;

			if (!(this.searchAttributes is null))
			{
				Dictionary<string, bool> HasPropertyDefinition = null;
				List<PropertyDefinition> PropertyDefinitions = null;

				foreach (FullTextSearchAttribute Attribute in this.searchAttributes)
				{
					if (this.firstSearchAttribute is null)
						this.firstSearchAttribute = Attribute;

					if (Attribute.DynamicIndexCollection)
						this.dynamicIndexCollection = true;

					if (Attribute.HasPropertyDefinitions)
					{
						this.hasPropertyDefinitions = true;

						if (PropertyDefinitions is null)
						{
							HasPropertyDefinition = new Dictionary<string, bool>();
							PropertyDefinitions = new List<PropertyDefinition>();
						}

						foreach (PropertyDefinition PDef in Attribute.Properties)
						{
							if (!HasPropertyDefinition.ContainsKey(PDef.Definition))
							{
								HasPropertyDefinition[PDef.Definition] = true;
								PropertyDefinitions.Add(PDef);
							}
						}
					}
				}

				this.properties = PropertyDefinitions?.ToArray();
			}
		}

		/// <summary>
		/// Type
		/// </summary>
		public Type Type { get; }

		/// <summary>
		/// Type information.
		/// </summary>
		public TypeInfo TypeInfo { get; }

		/// <summary>
		/// Collection name for objects of the corresponding type.
		/// </summary>
		public string CollectionName { get; }

		/// <summary>
		/// If a collection-name is defined.
		/// </summary>
		public bool HasCollection { get; }

		/// <summary>
		/// Indexing information for a collection.
		/// </summary>
		public CollectionInformation CollectionInformation { get; }

		/// <summary>
		/// Optional custom tokenizer;
		/// </summary>
		public ITokenizer CustomTokenizer => this.customTokenizer;

		/// <summary>
		/// If the type has a custom tokenizer.
		/// </summary>
		public bool HasCustomTokenizer => this.hasCustomTokenizer;

		/// <summary>
		/// If the index collection is dynamic (i.e. depends on object instance).
		/// </summary>
		public bool DynamicIndexCollection => this.dynamicIndexCollection;

		/// <summary>
		/// If the type has property definitions.
		/// </summary>
		public bool HasPropertyDefinitions => this.hasPropertyDefinitions;

		/// <summary>
		/// Property definitions.
		/// </summary>
		public PropertyDefinition[] Properties => this.properties;

		/// <summary>
		/// Name of full-text-search index collection.
		/// </summary>
		/// <param name="Reference">Object reference.</param>
		public string GetIndexCollection(object Reference)
		{
			return this.firstSearchAttribute?.GetIndexCollection(Reference)
				?? this.CollectionInformation.IndexCollectionName;
		}

		/// <summary>
		/// Name of full-text-search index collection.
		/// </summary>
		/// <param name="Reference">Object reference.</param>
		public string GetIndexCollection(GenericObject Reference)
		{
			return this.firstSearchAttribute?.GetIndexCollection(Reference)
				?? this.CollectionInformation.IndexCollectionName;
		}

		/// <summary>
		/// Tokenizes properties in an object, given a set of property names.
		/// </summary>
		/// <param name="Obj">Object instance to tokenize.</param>
		/// <param name="Properties">Indexable properties.</param>
		/// <returns>Tokens found.</returns>
		public async Task<TokenCount[]> Tokenize(object Obj, params PropertyDefinition[] Properties)
		{
			TokenizationProcess Process = new TokenizationProcess();

			await this.Tokenize(Obj, Process, Properties);

			return Process.ToArray();
		}

		/// <summary>
		/// Tokenizes properties in an object, given a set of property names.
		/// </summary>
		/// <param name="Obj">Object instance to tokenize.</param>
		/// <param name="Process">Tokenization process.</param>
		/// <param name="Properties">Indexable properties.</param>
		public async Task Tokenize(object Obj, TokenizationProcess Process, params PropertyDefinition[] Properties)
		{
			if (this.hasCustomTokenizer)
				await this.customTokenizer.Tokenize(Obj, Process);
			else
			{
				LinkedList<object> Values = new LinkedList<object>();
				object Value;

				foreach (PropertyDefinition Property in Properties)
				{
					Value = await Property.GetValue(Obj);
					if (!(Value is null))
						Values.AddLast(Value);
				}

				if (!(Values.First is null))
					await FullTextSearchModule.Tokenize(Values, Process);
			}
		}

	}
}
