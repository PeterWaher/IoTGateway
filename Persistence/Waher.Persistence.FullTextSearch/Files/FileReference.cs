using System;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch.Files
{
	/// <summary>
	/// Contains a reference to an indexed file.
	/// </summary>
	[CollectionName("FullTextSearchFiles")]
	[TypeName(TypeNameSerialization.None)]
	[Index("IndexCollection", "FileName")]
	[Index("FileName")]
	[FullTextSearch("IndexCollection", true)]
	public class FileReference
	{
		/// <summary>
		/// Contains a reference to an indexed file.
		/// </summary>
		public FileReference()
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
		public CaseInsensitiveString FileName { get; set; }

		/// <summary>
		/// When object was indexed.
		/// </summary>
		public DateTime Timestamp { get; set; }
	}
}
