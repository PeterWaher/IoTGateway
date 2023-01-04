using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.KeywordEnumerators
{
	/// <summary>
	/// Enumerates tokens, from newest to oldest
	/// </summary>
	internal class TokenReferencesNewToOld : TokenReferenceEnumerator
	{
		private uint currentBlock = 0;
		private TokenReferences currentReferences = null;
		private int currentIndex = 0;
		private uint lastBlock = 0;
		private bool processed = false;

		/// <summary>
		/// Enumerates tokens, from newest to oldest
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Token">Token being processed.</param>
		public TokenReferencesNewToOld(IPersistentDictionary Index, string Token)
			: base(Index, Token)
		{
		}

		/// <summary>
		/// Moves to the next token reference.
		/// </summary>
		/// <returns>If a new token reference was found.</returns>
		public override async Task<bool> MoveNextAsync()
		{
			while (this.currentReferences is null || this.currentIndex <= 0)
			{
				if (this.currentBlock == 0 && this.processed)
				{
					this.current = null;
					return false;
				}

				string Suffix = this.currentBlock == 0 ? string.Empty : " " + this.currentBlock.ToString();
				KeyValuePair<bool, object> P = await this.Index.TryGetValueAsync(this.Token + Suffix);
				if (!P.Key)
				{
					this.current = null;
					return false;
				}

				this.currentReferences = P.Value as TokenReferences;
				if (this.currentReferences is null)
				{
					this.current = null;
					return false;
				}

				if (this.currentBlock == 0)
				{
					this.lastBlock = this.currentReferences.LastBlock;
					this.currentBlock = this.lastBlock;
				}
				else
					this.currentBlock--;

				this.currentIndex = this.currentReferences.ObjectReferences.Length;
				this.processed = true;
			}

			this.currentIndex--;
			this.current = new TokenReference()
			{
				Count = this.currentReferences.Counts[this.currentIndex],
				ObjectReference = this.currentReferences.ObjectReferences[this.currentIndex],
				Timestamp = this.currentReferences.Timestamps[this.currentIndex],
				Token = this.Token,
				LastBlock = this.lastBlock
			};

			return true;
		}

		/// <summary>
		/// Resets the enumerator.
		/// </summary>
		public override void Reset()
		{
			base.Reset();

			this.currentBlock = 0;
			this.currentReferences = null;
			this.currentIndex = 0;
			this.lastBlock = 0;
			this.processed = false;
		}
	}
}
