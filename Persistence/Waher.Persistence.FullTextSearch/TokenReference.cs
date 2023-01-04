using System;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains a reference to a token.
	/// </summary>
	public class TokenReference
	{
		/// <summary>
		/// Contains a reference to a token.
		/// </summary>
		public TokenReference()
		{ 
		}

		/// <summary>
		/// Token
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Index to last block in index representing the same token.
		/// </summary>
		public uint LastBlock { get; set; }

		/// <summary>
		/// Reference to the object containing the token.
		/// </summary>
		public ulong ObjectReference { get; set; }

		/// <summary>
		/// Token count
		/// </summary>
		public uint Count { get; set; }

		/// <summary>
		/// Timestamps when corresponding object refernce was indexed.
		/// </summary>
		public DateTime Timestamp { get; set; }
	}
}
