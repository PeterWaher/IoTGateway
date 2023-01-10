using System.Collections.Generic;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Contains information about a tokenization process.
	/// </summary>
	public class TokenizationProcess
	{
		/// <summary>
		/// Contains information about a tokenization process.
		/// </summary>
		public TokenizationProcess()
		{
			this.TokenCounts = new Dictionary<string, List<uint>>();
		}

		/// <summary>
		/// Accumulated token counts.
		/// </summary>
		public Dictionary<string, List<uint>> TokenCounts { get; }

		/// <summary>
		/// Document Index Offset. Used to identify sequences of tokens in a document.
		/// </summary>
		public uint DocumentIndexOffset { get; set; }
	}
}
