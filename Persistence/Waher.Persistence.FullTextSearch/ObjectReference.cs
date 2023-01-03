using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains a reference to an indexed object.
	/// </summary>
	[CollectionName("FullTextSearchObjects")]
	[TypeName(TypeNameSerialization.None)]
	[Index("IndexCollection", "Index")]
	[Index("Collection", "ObjectInstanceId")]
	public class ObjectReference
	{
		/// <summary>
		/// Contains a reference to an indexed object.
		/// </summary>
		public ObjectReference()
		{
		}

		/// <summary>
		/// Object ID of reference object.
		/// </summary>
		[ObjectId]
		public string ObjectId { get; set; }

		/// <summary>
		/// Collection of full-text-search index.
		/// </summary>
		public string IndexCollection { get; set; }

		/// <summary>
		/// Name of collection hosting object.
		/// </summary>
		public string Collection { get; set; }

		/// <summary>
		/// Object ID of object instance.
		/// </summary>
		public object ObjectInstanceId { get; set; }

		/// <summary>
		/// Reference number to use in full-text-index.
		/// </summary>
		public ulong Index { get; set; }

		/// <summary>
		/// Token count in document.
		/// </summary>
		public TokenCount[] Tokens { get; set; }
	}
}
