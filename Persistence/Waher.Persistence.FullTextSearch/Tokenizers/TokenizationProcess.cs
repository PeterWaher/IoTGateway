using System.Collections.Generic;
using System.Diagnostics;

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

		/// <summary>
		/// Generates an array of token counts.
		/// </summary>
		/// <returns>Token counts.</returns>
		public TokenCount[] ToArray()
		{
			int c = this.TokenCounts.Count;
			if (c == 0)
				return null;

			int i = 0;
			TokenCount[] Counts = new TokenCount[c];

			foreach (KeyValuePair<string, List<uint>> P in this.TokenCounts)
				Counts[i++] = new TokenCount(P.Key, P.Value.ToArray());

			return Counts;

		}
	}
}
