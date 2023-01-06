using System.Collections.Generic;
using System.Threading.Tasks;

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

		/// <inheritdoc/>
		public new abstract bool Equals(object obj);

		/// <inheritdoc/>
		public new abstract string ToString();

		/// <summary>
		/// Gets available token references.
		/// </summary>
		/// <param name="Index">Dictionary containing token references.</param>
		/// <returns>Enumerable set of token references.</returns>
		public abstract Task<IEnumerable<KeyValuePair<string, TokenReferences>>> GetTokenReferences(IPersistentDictionary Index);

		/// <summary>
		/// Processes the keyword in a search process.
		/// </summary>
		/// <param name="Process"></param>
		/// <returns>If the process can continue (true) or if an empty result is concluded (false).</returns>
		public virtual async Task<bool> Process(SearchProcess Process)
		{
			IEnumerable<KeyValuePair<string, TokenReferences>> Records = await this.GetTokenReferences(Process.Index);

			foreach (KeyValuePair<string, TokenReferences> Rec in Records)
			{
				string Token = Rec.Key;
				TokenReferences References = Rec.Value;

				int j, d = References.ObjectReferences.Length;

				for (j = 0; j < d; j++)
				{
					ulong ObjectReference = References.ObjectReferences[j];

					if (!Process.ReferencesByObject.TryGetValue(ObjectReference, out LinkedList<TokenReference> ByObjectReference))
					{
						if (Process.IsRestricted)
							continue;

						ByObjectReference = new LinkedList<TokenReference>();
						Process.ReferencesByObject[ObjectReference] = ByObjectReference;
					}

					ByObjectReference.AddLast(new TokenReference()
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
