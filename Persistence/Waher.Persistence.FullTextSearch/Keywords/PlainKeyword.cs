using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Collections;

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
			this.Ignore = string.IsNullOrEmpty(Keyword) ||
				FullTextSearchModule.IsStopWord(Keyword);
		}

		/// <summary>
		/// Keyword
		/// </summary>
		public string Keyword { get; }

		/// <summary>
		/// If keyword should be ignored.
		/// </summary>
		public override bool Ignore { get; }

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
		/// <param name="Process">Current search process.</param>
		/// <returns>Enumerable set of token references.</returns>
		public override async Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(SearchProcess Process)
		{
			ChunkedList<KeyValuePair<string, TokenReferences>> Result = new ChunkedList<KeyValuePair<string, TokenReferences>>();
			KeyValuePair<string, object>[] Records = await Process.Index.GetEntriesAsync(this.Keyword, this.Keyword + "!");

			foreach (KeyValuePair<string, object> Rec in Records)
			{
				if (Rec.Value is TokenReferences References)
					Result.Add(new KeyValuePair<string, TokenReferences>(this.Keyword, References));
			}

			return Result;
		}
	}
}
