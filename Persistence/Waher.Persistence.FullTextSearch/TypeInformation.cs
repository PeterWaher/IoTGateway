using System;
using System.Collections.Generic;
using System.Reflection;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains information about a type, in relation to full-text-search.
	/// </summary>
	internal class TypeInformation
	{
		private readonly Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>> properties = new Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>>();

		/// <summary>
		/// Contains information about a type, in relation to full-text-search.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="TypeInfo">Type information.</param>
		/// <param name="CollectionName">Collection name for objects of the corresponding type.</param>
		/// <param name="CollectionInformation">Indexing information for a collection.</param>
		public TypeInformation(Type Type, TypeInfo TypeInfo, string CollectionName, 
			CollectionInformation CollectionInformation)
		{
			this.Type = Type;
			this.TypeInfo = TypeInfo;
			this.CollectionName = CollectionName;
			this.HasCollection = !string.IsNullOrEmpty(CollectionName);
			this.CollectionInformation = CollectionInformation;
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
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		public Dictionary<string, object> GetIndexableProperties(object Obj, params string[] PropertyNames)
		{
			Dictionary<string, object> Result = new Dictionary<string, object>();
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

					Result[PropertyName] = Value;
				}
			}

			return Result;
		}

	}
}
