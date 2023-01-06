using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Represents a prohibited keyword.
	/// </summary>
	public class ProhibitedKeyword : Keyword
	{
		/// <summary>
		/// Represents a prohibited keyword.
		/// </summary>
		/// <param name="Keyword">Keyword</param>
		public ProhibitedKeyword(Keyword Keyword)
			: base()
		{
			this.Keyword = Keyword;
		}

		/// <summary>
		/// Keyword
		/// </summary>
		public Keyword Keyword { get; }

		/// <summary>
		/// If keyword is optional
		/// </summary>
		public override bool Optional => false;

		/// <summary>
		/// If keyword is prohibited
		/// </summary>
		public override bool Prohibited => true;

		/// <summary>
		/// Order category of keyword
		/// </summary>
		public override int OrderCategory => -1;

		/// <summary>
		/// Order complexity (within category) of keyword
		/// </summary>
		public override int OrderComplexity => this.Keyword.OrderComplexity;

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ProhibitedKeyword k && this.Keyword.Equals(k.Keyword);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return "-" + this.Keyword.ToString();
		}

		/// <summary>
		/// Gets available token references.
		/// </summary>
		/// <param name="Process">Current search process.</param>
		/// <returns>Enumerable set of token references.</returns>
		public override Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(SearchProcess Process)
		{
			return this.Keyword.GetTokenReferences(Process);
		}

		/// <summary>
		/// Processes the keyword in a search process.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns>If the process can continue (true) or if an empty result is concluded (false).</returns>
		public override async Task<bool> Process(SearchProcess Process)
		{
			IEnumerable<KeyValuePair<string, TokenReferences>> Records = await this.GetTokenReferences(Process);

			foreach (KeyValuePair<string, TokenReferences> Rec in Records)
			{
				TokenReferences References = Rec.Value;

				int j, d = References.ObjectReferences.Length;

				for (j = 0; j < d; j++)
				{
					ulong ObjectReference = References.ObjectReferences[j];
					Process.ReferencesByObject.Remove(ObjectReference);
				}

				if (Process.ReferencesByObject.Count == 0)
					return false;
			}

			return true;
		}

	}
}
