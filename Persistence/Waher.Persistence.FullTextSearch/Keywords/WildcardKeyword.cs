using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Represents a wildcard keyword.
	/// </summary>
	public class WildcardKeyword : RegexKeyword
	{
		/// <summary>
		/// Represents a wildcard keyword.
		/// </summary>
		/// <param name="Keyword">Keyword</param>
		/// <param name="Wildcard">Wildcard</param>
		public WildcardKeyword(string Keyword, string Wildcard)
			: base(Database.WildcardToRegex(Keyword, Wildcard))
		{
			this.Keyword = Keyword;
			this.Wildcard = Wildcard;
		}

		/// <summary>
		/// Keyword
		/// </summary>
		public string Keyword { get; }

		/// <summary>
		/// Wildcard
		/// </summary>
		public string Wildcard { get; }

		/// <summary>
		/// Order complexity (within category) of keyword
		/// </summary>
		public override int OrderComplexity => this.Keyword.Length;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is WildcardKeyword k && this.Keyword == k.Keyword && this.Wildcard == k.Wildcard;
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
			int i = this.Keyword.IndexOf(this.Wildcard);
			if (i <= 0)
				return await base.GetTokenReferences(Process);

			string Preamble = this.Keyword.Substring(0, i);
			char[] Characters = Preamble.ToCharArray();
			Characters[i - 1]++;
			string ToExclusive = new string(Characters);

			LinkedList<KeyValuePair<string, TokenReferences>> Result = new LinkedList<KeyValuePair<string, TokenReferences>>();
			KeyValuePair<string, object>[] Records = await Process.Index.GetEntriesAsync(Preamble, ToExclusive);

			foreach (KeyValuePair<string, object> Rec in Records)
			{
				Match M = this.Parsed.Match(Rec.Key);
				if (M.Success && M.Index == 0 && M.Length == Rec.Key.Length && Rec.Value is TokenReferences References)
					Result.AddLast(new KeyValuePair<string, TokenReferences>(Rec.Key, References));
			}

			return Result;
		}
	}
}
