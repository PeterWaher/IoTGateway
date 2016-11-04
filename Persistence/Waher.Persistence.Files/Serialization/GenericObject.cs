using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Waher.Persistence.Files.Serialization
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
			internal set { this.typeName = value; }
		}

		/// <summary>
		/// Object ID
		/// </summary>
		public Guid ObjectId
		{
			get { return this.objectId; }
			internal set { this.objectId = value; }
		}

		/// <summary>
		/// <see cref="IEnumerable{String,Object}.GetEnumerator()"/>
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
				if (this.propertiesByName == null)
					this.BuildDictionary();

				object Result;

				if (this.propertiesByName.TryGetValue(PropertyName, out Result))
					return Result;
				else
					return null;
			}

			set
			{
				if (this.propertiesByName == null)
					this.BuildDictionary();

				this.propertiesByName[PropertyName] = value;
				this.propertiesUpdated = true;
			}
		}

		/// <summary>
		/// Removes a named property.
		/// </summary>
		/// <param name="PropertyName">Name of property to remove.</param>
		/// <returns>If the property was found and removed.</returns>
		public bool Remove(string PropertyName)
		{
			if (this.propertiesByName == null)
				this.BuildDictionary();

			if (!this.propertiesByName.Remove(PropertyName))
				return false;

			this.propertiesUpdated = true;

			return true;
		}

		private void BuildDictionary()
		{
			this.propertiesByName = new Dictionary<string, object>();

			foreach (KeyValuePair<string, object> P in this.properties)
				this.propertiesByName[P.Key] = P.Value;
		}

		private void BuildEnumerable()
		{
			LinkedList<KeyValuePair<string, object>> List = new LinkedList<KeyValuePair<string, object>>();
			Dictionary<string, bool> Added = new Dictionary<string, bool>();
			object Value;

			foreach (KeyValuePair<string, object> P in this.properties)
			{
				if (!this.propertiesByName.TryGetValue(P.Key, out Value))
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
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Count"/>
		/// </summary>
		public int Count
		{
			get
			{
				if (this.propertiesByName == null)
					this.BuildDictionary();

				return this.propertiesByName.Count;
			}
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.IsReadOnly"/>
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Add"/>
		/// </summary>
		public void Add(KeyValuePair<string, object> item)
		{
			this[item.Key] = item.Value;
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Clear"/>
		/// </summary>
		public void Clear()
		{
			if (this.propertiesByName == null)
				this.propertiesByName = new Dictionary<string, object>();
			else
				this.propertiesByName.Clear();

			this.propertiesUpdated = true;
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Contains"/>
		/// </summary>
		public bool Contains(KeyValuePair<string, object> item)
		{
			if (this.propertiesByName == null)
				this.BuildDictionary();

			object Value;

			if (!this.propertiesByName.TryGetValue(item.Key, out Value))
				return false;

			if (Value == null)
				return item.Value == null;

			return Value.Equals(item.Value);
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Contains"/>
		/// </summary>
		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			foreach (KeyValuePair<string, object> P in this)
				array[arrayIndex++] = P;
		}

		/// <summary>
		/// <see cref="ICollection{KeyValuePair{String, Object}}.Remove"/>
		/// </summary>
		public bool Remove(KeyValuePair<string, object> item)
		{
			if (this.propertiesByName == null)
				this.BuildDictionary();

			object Value;

			if (!this.propertiesByName.TryGetValue(item.Key, out Value))
				return false;

			if (Value == null)
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

			if (this.propertiesByName == null)
				this.BuildDictionary();

			if (GenObj.propertiesByName == null)
				GenObj.BuildDictionary();

			if (this.propertiesByName.Count != GenObj.propertiesByName.Count)
				return false;

			object Value;

			foreach (KeyValuePair<string, object> P in this.propertiesByName)
			{
				if (!GenObj.propertiesByName.TryGetValue(P.Key, out Value))
					return false;

				if (Value == null ^ P.Value == null)
					return false;

				if (Value != null && !Value.Equals(P.Value))
					return false;
			}

			return true;
		}

		/// <summary>
		/// <see cref="object.GetHashCode()"/>
		/// </summary>
		public override int GetHashCode()
		{
			if (this.propertiesByName == null)
				this.BuildDictionary();

			int Result = this.objectId.GetHashCode() ^
				this.typeName.GetHashCode() ^
				this.collectionName.GetHashCode();

			foreach (KeyValuePair<string, object> P in this.propertiesByName)
			{
				Result ^= P.Key.GetHashCode();
				Result ^= P.Value.GetHashCode();
			}

			return Result;
		}
	}
}
