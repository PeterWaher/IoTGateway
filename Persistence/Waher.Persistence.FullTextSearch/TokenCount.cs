using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Represents a token and a corresponding occurrence count.
	/// </summary>
	[TypeName(TypeNameSerialization.None)]
	public class TokenCount
	{
		/// <summary>
		/// Represents a token and a corresponding occurrence count.
		/// </summary>
		public TokenCount()
		{ 
		}

		/// <summary>
		/// Represents a token and a corresponding occurrence count.
		/// </summary>
		public TokenCount(string Token, uint Count)
		{
			this.Token = Token;
			this.Count = Count;
			this.Block = 0;
		}

		/// <summary>
		/// Token
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Count
		/// </summary>
		public uint Count { get; set; }

		/// <summary>
		/// Reference is stored in this block in the full-text-search index.
		/// </summary>
		public uint Block { get; set; }
	}
}
