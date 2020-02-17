using System;
using System.Collections.Generic;
using System.Text;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.MongoDB
{
	/// <summary>
	/// Contains a reference to a thing
	/// </summary>
	[CollectionName(DictionaryEntry.CollectionName)]
	[Index("Collection", "Key")]
	public class DictionaryEntry
	{
		public const string CollectionName = "PersistentDictionary";

		private string objectId = null;
		private string collection = string.Empty;
		private string key = string.Empty;
		private object value = null;

		/// <summary>
		/// Contains a reference to a thing
		/// </summary>
		public DictionaryEntry()
		{
		}

		/// <summary>
		/// Persisted object ID. Is null if object not persisted.
		/// </summary>
		[ObjectId]
		public string ObjectId
		{
			get { return this.objectId; }
			set { this.objectId = value; }
		}

		/// <summary>
		/// Dictionary collection
		/// </summary>
		[ShortName("c")]
		public string Collection
		{
			get { return this.collection; }
			set { this.collection = value; }
		}

		/// <summary>
		/// Dictionary key
		/// </summary>
		[ShortName("k")]
		public string Key
		{
			get { return this.key; }
			set { this.key = value; }
		}

		/// <summary>
		/// Dictionary value
		/// </summary>
		[DefaultValueStringEmpty]
		[ShortName("v")]
		public object Value
		{
			get { return this.value; }
			set { this.value = value; }
		}
	}
}
