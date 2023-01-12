using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Orders;

namespace Waher.Persistence.FullTextSearch.Keywords
{
	/// <summary>
	/// Abstract base class for keywords.
	/// </summary>
	public abstract class Keyword
	{
		/// <summary>
		/// Abstract base class for keywords.
		/// </summary>
		public Keyword()
		{
		}

		/// <summary>
		/// If keyword is optional
		/// </summary>
		public virtual bool Optional => true;

		/// <summary>
		/// If keyword is required
		/// </summary>
		public virtual bool Required => false;

		/// <summary>
		/// If keyword is prohibited
		/// </summary>
		public virtual bool Prohibited => false;

		/// <summary>
		/// Order category of keyword
		/// </summary>
		public virtual int OrderCategory => 0;

		/// <summary>
		/// Order complexity (within category) of keyword
		/// </summary>
		public virtual int OrderComplexity => 0;

		/// <summary>
		/// If keyword should be ignored.
		/// </summary>
		public virtual bool Ignore => false;

		/// <inheritdoc/>
		public new abstract bool Equals(object obj);

		/// <inheritdoc/>
		public new abstract string ToString();

		/// <summary>
		/// Gets available token references.
		/// </summary>
		/// <param name="Process">Current search process.</param>
		/// <returns>Enumerable set of token references.</returns>
		public abstract Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(SearchProcess Process);

		/// <summary>
		/// Processes the keyword in a search process.
		/// </summary>
		/// <param name="Process">Current search process.</param>
		/// <returns>If the process can continue (true) or if an empty result is concluded (false).</returns>
		public virtual async Task<bool> Process(SearchProcess Process)
		{
			IEnumerable<KeyValuePair<string, TokenReferences>> Records = await this.GetTokenReferences(Process);

			foreach (KeyValuePair<string, TokenReferences> Rec in Records)
			{
				string Token = Rec.Key;
				TokenReferences References = Rec.Value;

				int j, d = References.ObjectReferences.Length;

				for (j = 0; j < d; j++)
				{
					ulong ObjectReference = References.ObjectReferences[j];

					if (!Process.ReferencesByObject.TryGetValue(ObjectReference, out MatchInformation ByObjectReference))
					{
						if (Process.IsRestricted)
							continue;

						ByObjectReference = new MatchInformation();
						Process.ReferencesByObject[ObjectReference] = ByObjectReference;
					}

					ByObjectReference.AddTokenReference(new TokenReference()
					{
						Count = References.Counts[j],
						LastBlock = References.LastBlock,
						ObjectReference = ObjectReference,
						Timestamp = References.Timestamps[j],
						Token = Token
					});
				}
			}

			return true;
		}

	}
}
