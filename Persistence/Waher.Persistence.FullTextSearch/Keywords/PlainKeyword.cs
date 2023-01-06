using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Represents a plain text keyword.
	/// </summary>
	public class PlainKeyword : Keyword
	{
		/// <summary>
		/// Represents a plain text keyword.
		/// </summary>
		/// <param name="Keyword">Keyword</param>
		public PlainKeyword(string Keyword)
		{
			this.Keyword = Keyword;
		}

		/// <summary>
		/// Keyword
		/// </summary>
		public string Keyword { get; }

		/// <summary>
		/// Order complexity (within category) of keyword
		/// </summary>
		public override int OrderComplexity => this.Keyword.Length;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is PlainKeyword k && this.Keyword == k.Keyword;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return this.Keyword;
		}

		/// <summary>
		/// Gets available token references.
		/// </summary>
		/// <param name="Index">Dictionary containing token references.</param>
		/// <returns>Enumerable set of token references.</returns>
		public override async Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(IPersistentDictionary Index)
		{
			LinkedList<KeyValuePair<string, TokenReferences>> Result = new LinkedList<KeyValuePair<string, TokenReferences>>();
			KeyValuePair<string, object>[] Records = await Index.GetEntriesAsync(this.Keyword, this.Keyword + "!");

			foreach (KeyValuePair<string, object> Rec in Records)
			{
				if (Rec.Value is TokenReferences References)
					Result.AddLast(new KeyValuePair<string, TokenReferences>(this.Keyword, References));
			}

			return Result;
		}
	}
}
