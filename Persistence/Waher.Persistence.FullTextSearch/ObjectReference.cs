using System;
using System.Collections.Generic;
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

		/// <summary>
		/// When object was indexed.
		/// </summary>
		public DateTime Indexed { get; set; }

		/// <summary>
		/// Tries to get a specific token count.
		/// </summary>
		/// <param name="Token">Token</param>
		/// <param name="Count">Count, if found.</param>
		/// <returns>If the corresponding token was found.</returns>
		public bool TryGetCount(string Token, out TokenCount Count)
		{
			if (this.tokensByName is null)
			{
				this.tokensByName = new Dictionary<string, TokenCount>(StringComparer.InvariantCultureIgnoreCase);

				foreach (TokenCount TokenCount in this.Tokens)
					this.tokensByName[TokenCount.Token] = TokenCount;
			}

			return this.tokensByName.TryGetValue(Token, out Count);
		}

		private Dictionary<string, TokenCount> tokensByName = null;
	}
}
