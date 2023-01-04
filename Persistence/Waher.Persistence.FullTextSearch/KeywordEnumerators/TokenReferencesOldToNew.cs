using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Waher.Persistence.FullTextSearch.KeywordEnumerators
{
	/// <summary>
	/// Enumerates tokens, from oldest to newest
	/// </summary>
	internal class TokenReferencesOldToNew : TokenReferenceEnumerator
	{
		private uint currentBlock = 0;
		private TokenReferences firstReferences = null;
		private TokenReferences currentReferences = null;
		private int currentIndex = 0;
		private uint lastBlock = 0;
		private bool processed = false;

		/// <summary>
		/// Enumerates tokens, from oldest to newest
		/// </summary>
		/// <param name="Index">Index.</param>
		/// <param name="Token">Token being processed.</param>
		public TokenReferencesOldToNew(IPersistentDictionary Index, string Token)
			: base(Index, Token)
		{
		}

		/// <summary>
		/// Moves to the next token reference.
		/// </summary>
		/// <returns>If a new token reference was found.</returns>
		public override async Task<bool> MoveNextAsync()
		{
			if (this.firstReferences is null)
			{
				KeyValuePair<bool, object> P = await this.Index.TryGetValueAsync(this.Token);
				if (!P.Key)
				{
					this.current = null;
					return false;
				}

				this.firstReferences = P.Value as TokenReferences;
				if (this.firstReferences is null)
				{
					this.current = null;
					return false;
				}

				this.lastBlock = this.firstReferences.LastBlock;
				this.currentBlock = this.lastBlock == 0 ? 0u : 1u;
			}

			while (this.currentReferences is null || this.currentIndex >= this.currentReferences.ObjectReferences.Length)
			{
				if (this.currentBlock == 0)
				{
					if (this.processed)
					{
						this.current = null;
						return false;
					}
					else
					{
						this.processed = true;
						this.currentReferences = this.firstReferences;
					}
				}
				else
				{
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
				}

				this.currentIndex = 0;
				this.currentBlock++;
				if (++this.currentBlock > this.lastBlock)
					this.currentBlock = 0;
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
			this.firstReferences = null;
			this.currentReferences = null;
			this.currentIndex = 0;
			this.lastBlock = 0;
			this.processed = false;
		}

	}
}
