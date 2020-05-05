using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Serialization
{
	/// <summary>
	/// Generic object. Contains a sequence of properties.
	/// </summary>
	public sealed class GenericObject : ICollection<KeyValuePair<string, object>>
	{
		private IEnumerable<KeyValuePair<string, object>> properties = new LinkedList<KeyValuePair<string, object>>();
		private Dictionary<string, object> propertiesByName = null;
		private string collectionName = null;
		private string typeName = null;
		private bool propertiesUpdated = false;
		private Guid objectId = Guid.Empty;

		internal GenericObject()
		{
		}

		/// <summary>
		/// Generic object. Contains a sequence of properties.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Properties">Ordered sequence of properties.</param>
		public GenericObject(string CollectionName, string TypeName, Guid ObjectId, params KeyValuePair<string, object>[] Properties)
		{
			this.collectionName = CollectionName;
			this.typeName = TypeName;
			this.objectId = ObjectId;
			this.properties = Properties;
		}

		/// <summary>
		/// Generic object. Contains a sequence of properties.
		/// </summary>
		/// <param name="CollectionName">Collection name.</param>
		/// <param name="TypeName">Type name.</param>
		/// <param name="ObjectId">Object ID</param>
		/// <param name="Properties">Ordered sequence of properties.</param>
		public GenericObject(string CollectionName, string TypeName, Guid ObjectId, IEnumerable<KeyValuePair<string, object>> Properties)
		{
			this.collectionName = CollectionName;
			this.typeName = TypeName;
			this.objectId = ObjectId;
			this.properties = Properties;
		}

		/// <summary>
		/// Collection name.
		/// </summary>
		public string CollectionName
		{
			get { return this.collectionName; }
			internal set { this.collectionName = value; }
		}

		/// <summary>
		/// Type name.
		/// </summary>
		public string TypeName
		{
			get { return this.typeName; }
			set { this.typeName = value; }
		}

		/// <summary>
		/// Object ID
		/// </summary>
		public Guid ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// <see cref="IEnumerable{T}.GetEnumerator()"/>
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			if (this.propertiesUpdated)
				this.BuildEnumerable();

			return this.properties.GetEnumerator();
		}

		/// <summary>
		/// <see cref="IEnumerable.GetEnumerator()"/>
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (this.propertiesUpdated)
				this.BuildEnumerable();

			return this.properties.GetEnumerator();
		}

		/// <summary>
		/// Access to property values through the use of their property names.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <returns>Property value.</returns>
		public object this[string PropertyName]
		{
			get
			{
				if (this.propertiesByName is null)
					this.BuildDictionary();

				if (this.propertiesByName.TryGetValue(PropertyName, out object Result))
					return Result;
				else
					return null;
			}

			set
			{
				if (this.propertiesByName is null)
					this.BuildDictionary();

				this.propertiesByName[PropertyName] = value;
				this.propertiesUpdated = true;
			}
		}

		/// <summary>
		/// Gets the value of a field or property of the object, given its name.
		/// </summary>
		/// <param name="PropertyName">Name of field or property.</param>
		/// <param name="Value">Corresponding field or property value, if found, or null otherwise.</param>
		/// <returns>If the corresponding field or property was found.</returns>
		public bool TryGetFieldValue(string PropertyName, out object Value)
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			return this.propertiesByName.TryGetValue(PropertyName, out Value);
		}

		/// <summary>
		/// Removes a named property.
		/// </summary>
		/// <param name="PropertyName">Name of property to remove.</param>
		/// <returns>If the property was found and removed.</returns>
		public bool Remove(string PropertyName)
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			if (!this.propertiesByName.Remove(PropertyName))
				return false;

			this.propertiesUpdated = true;

			return true;
		}

		private void BuildDictionary()
		{
			this.propertiesByName = this.properties as Dictionary<string, object>;
			if (this.propertiesByName is null)
			{
				this.propertiesByName = new Dictionary<string, object>();

				foreach (KeyValuePair<string, object> P in this.properties)
					this.propertiesByName[P.Key] = P.Value;
			}
		}

		private void BuildEnumerable()
		{
			LinkedList<KeyValuePair<string, object>> List = new LinkedList<KeyValuePair<string, object>>();
			Dictionary<string, bool> Added = new Dictionary<string, bool>();

			foreach (KeyValuePair<string, object> P in this.properties)
			{
				if (!this.propertiesByName.TryGetValue(P.Key, out object Value))
					continue;

				List.AddLast(new KeyValuePair<string, object>(P.Key, Value));
				Added[P.Key] = true;
			}

			foreach (KeyValuePair<string, object> P in this.propertiesByName)
			{
				if (Added.ContainsKey(P.Key))
					continue;

				List.AddLast(P);
			}

			this.properties = List;
		}

		/// <summary>
		/// Current set of properties.
		/// </summary>
		public IEnumerable<KeyValuePair<string, object>> Properties
		{
			get
			{
				if (this.propertiesUpdated)
					this.BuildEnumerable();

				return this.properties;
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.Count"/>
		/// </summary>
		public int Count
		{
			get
			{
				if (this.propertiesByName is null)
					this.BuildDictionary();

				return this.propertiesByName.Count;
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.IsReadOnly"/>
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// <see cref="ICollection{T}.Add"/>
		/// </summary>
		public void Add(KeyValuePair<string, object> item)
		{
			this[item.Key] = item.Value;
		}

		/// <summary>
		/// <see cref="ICollection{T}.Clear"/>
		/// </summary>
		public void Clear()
		{
			if (this.propertiesByName is null)
				this.propertiesByName = new Dictionary<string, object>();
			else
				this.propertiesByName.Clear();

			this.propertiesUpdated = true;
		}

		/// <summary>
		/// <see cref="ICollection{T}.Contains"/>
		/// </summary>
		public bool Contains(KeyValuePair<string, object> item)
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			if (!this.propertiesByName.TryGetValue(item.Key, out object Value))
				return false;

			if (Value is null)
				return item.Value is null;

			return Value.Equals(item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{T}.Contains"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			foreach (KeyValuePair<string, object> P in this)
				array[arrayIndex++] = P;
		}

		/// <summary>
		/// <see cref="ICollection{T}.Remove"/>
		/// </summary>
		public bool Remove(KeyValuePair<string, object> item)
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			if (!this.propertiesByName.TryGetValue(item.Key, out object Value))
				return false;

			if (Value is null)
			{
				if (item.Value != null)
					return false;
			}
			else if (!Value.Equals(item.Value))
				return false;

			this.propertiesByName.Remove(item.Key);
			this.propertiesUpdated = true;

			return true;
		}

		/// <summary>
		/// <see cref="object.Equals(object)"/>
		/// </summary>
		public override bool Equals(object obj)
		{
			GenericObject GenObj = obj as GenericObject;

			if (this.collectionName != GenObj.collectionName ||
				this.typeName != GenObj.typeName ||
				this.objectId != GenObj.objectId)
			{
				return false;
			}

			if (this.propertiesByName is null)
				this.BuildDictionary();

			if (GenObj.propertiesByName is null)
				GenObj.BuildDictionary();

			if (this.propertiesByName.Count != GenObj.propertiesByName.Count)
				return false;

			foreach (KeyValuePair<string, object> P in this.propertiesByName)
			{
				if (!GenObj.propertiesByName.TryGetValue(P.Key, out object Value))
					return false;

				if (!PropertyEquals(Value, P.Value))
					return false;
			}

			return true;
		}

		private static bool PropertyEquals(object Value1, object Value2)
		{
			if (Value1 is null ^ Value2 is null)
				return false;

			if (Value1 is null || Value1.Equals(Value2))
				return true;

			if (Value1 is Array A && Value2 is Array B)
			{
				int i, c = A.Length;

				if (c != B.Length)
					return false;

				for (i = 0; i < c; i++)
				{
					if (!PropertyEquals(A.GetValue(i), B.GetValue(i)))
						return false;
				}

				return true;
			}
			else
				return false;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			int Result = this.objectId.GetHashCode();
			Result ^= Result << 5 ^ this.typeName.GetHashCode();
			Result ^= Result << 5 ^ this.collectionName.GetHashCode();

			foreach (KeyValuePair<string, object> P in this.propertiesByName)
			{
				Result ^= Result << 5 ^ P.Key.GetHashCode();
				Result ^= Result << 5 ^ P.Value.GetHashCode();
			}

			return Result;
		}

		/// <summary>
		/// If the object has a property with a given name.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <returns>If the object has a property with the given name.</returns>
		public bool HasProperty(string PropertyName)
		{
			if (this.propertiesByName is null)
				this.BuildDictionary();

			return this.propertiesByName.ContainsKey(PropertyName);
		}

		/// <summary>
		/// Gets the value of a given property.
		/// </summary>
		/// <param name="PropertyName">Property name.</param>
		/// <returns>Property value, if found, null otherwise.</returns>
		public object GetProperty(string PropertyName)
		{
			return this[PropertyName];
		}

	}
}
