using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Tokenizers;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains information about a type, in relation to full-text-search.
	/// </summary>
	internal class TypeInformation
	{
		private readonly Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>> properties = new Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>>();
		private readonly ITokenizer customTokenizer;
		private readonly bool hasCustomTokenizer;

		/// <summary>
		/// Contains information about a type, in relation to full-text-search.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="TypeInfo">Type information.</param>
		/// <param name="CollectionName">Collection name for objects of the corresponding type.</param>
		/// <param name="CollectionInformation">Indexing information for a collection.</param>
		/// <param name="CustomTokenizer">Optional custom tokenizer creating tokens for objects
		/// of this type.</param>
		public TypeInformation(Type Type, TypeInfo TypeInfo, string CollectionName, 
			CollectionInformation CollectionInformation, ITokenizer CustomTokenizer)
		{
			this.Type = Type;
			this.TypeInfo = TypeInfo;
			this.CollectionName = CollectionName;
			this.HasCollection = !string.IsNullOrEmpty(CollectionName);
			this.CollectionInformation = CollectionInformation;
			this.customTokenizer = CustomTokenizer;
			this.hasCustomTokenizer = !(CustomTokenizer is null);
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
		/// Tokenizes properties in an object, given a set of property names.
		/// </summary>
		/// <param name="Obj">Object instance to tokenize.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Tokens found.</returns>
		public async Task<TokenCount[]> Tokenize(object Obj, params string[] PropertyNames)
		{
			TokenizationProcess Process = new TokenizationProcess();

			if (this.hasCustomTokenizer)
			{
				await this.customTokenizer.Tokenize(Obj, Process);
				return Process.ToArray();
			}
			else
			{
				LinkedList<object> Values = new LinkedList<object>();
				object Value;

				lock (this.properties)
				{
					foreach (string PropertyName in PropertyNames)
					{
						if (!this.properties.TryGetValue(PropertyName, out KeyValuePair<PropertyInfo, FieldInfo> P))
						{
							P = new KeyValuePair<PropertyInfo, FieldInfo>(
								this.Type.GetRuntimeProperty(PropertyName),
								this.Type.GetRuntimeField(PropertyName));

							this.properties[PropertyName] = P;
						}

						if (!(P.Key is null))
							Value = P.Key.GetValue(Obj, null);
						else if (!(P.Value is null))
							Value = P.Value.GetValue(Obj);
						else
							continue;

						Values.AddLast(Value);
					}
				}

				if (Values.First is null)
					return null;

				return await FullTextSearchModule.Tokenize(Values);
			}
		}

	}
}
