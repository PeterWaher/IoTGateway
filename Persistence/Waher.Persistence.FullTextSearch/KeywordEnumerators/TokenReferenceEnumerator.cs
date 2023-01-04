using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.KeywordEnumerators
{
	/// <summary>
	/// Abstract base class for token reference enumerators
	/// </summary>
	internal abstract class TokenReferenceEnumerator : IEnumerator<TokenReference>, IAsyncEnumerator
	{
		/// <summary>
		/// Reference to current token reference.
		/// </summary>
		protected TokenReference current;

		/// <summary>
		/// Abstract base class for token reference enumerators
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Token">Token being processed.</param>
		public TokenReferenceEnumerator(IPersistentDictionary Index, string Token)
		{
			this.Token = Token;
			this.Index = Index;
		}

		/// <summary>
		/// Token being processed.
		/// </summary>
		public string Token { get; }

		/// <summary>
		/// Index
		/// </summary>
		protected  IPersistentDictionary Index { get; }

		/// <summary>
		/// Current token
		/// </summary>
		public TokenReference Current => this.current;

		/// <summary>
		/// Current token
		/// </summary>
		object IEnumerator.Current => this.current;

		/// <summary>
		/// Disposes of the enumerator
		/// </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// Moves to the next token reference.
		/// </summary>
		/// <returns>If a new token reference was found.</returns>
		public bool MoveNext()
		{
			return this.MoveNextAsync().Result;
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public virtual void Reset()
		{
			this.current = null;
		}

		/// <summary>
		/// Moves to the next token reference.
		/// </summary>
		/// <returns>If a new token reference was found.</returns>
		public abstract Task<bool> MoveNextAsync();
	}
}
