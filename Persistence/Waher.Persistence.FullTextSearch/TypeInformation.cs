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
		private Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>> properties = new Dictionary<string, KeyValuePair<PropertyInfo, FieldInfo>>();

		/// <summary>
		/// Contains information about a type, in relation to full-text-search.
		/// </summary>
		/// <param name="Type">Type</param>
		/// <param name="TypeInfo">Type information.</param>
		public TypeInformation(Type Type, TypeInfo TypeInfo)
		{
			this.Type = Type;
			this.TypeInfo = TypeInfo;
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
		/// Information about associated collection.
		/// </summary>
		public CollectionInformation CollectionInformation
		{
			get;
			internal set;
		}

		/// <summary>
		/// Gets the indexable property values from an object. Property values will be returned in lower-case.
		/// </summary>
		/// <param name="Obj">Generic object.</param>
		/// <param name="PropertyNames">Indexable property names.</param>
		/// <returns>Indexable property values found.</returns>
		public Dictionary<string, string> GetIndexableProperties(object Obj, params string[] PropertyNames)
		{
			Dictionary<string, string> Result = new Dictionary<string, string>();
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

					if (Value is string s)
						Result[PropertyName] = s.ToLower();
					else if (Value is CaseInsensitiveString cis)
						Result[PropertyName] = cis.LowerCase;
				}
			}

			return Result;
		}

	}
}
