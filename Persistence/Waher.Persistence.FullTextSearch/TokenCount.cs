using System;
using System.Text;
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
		/// <param name="Token">Token</param>
		/// <param name="DocIndex">Document indices for each occurrence of the token.</param>
		public TokenCount(string Token, params uint[] DocIndex)
		{
			this.Token = Token;
			this.DocIndex = DocIndex;
			this.Block = 0;
		}

		/// <summary>
		/// Token
		/// </summary>
		public string Token { get; set; }

		/// <summary>
		/// Index inside document of each occurrence.
		/// </summary>
		public uint[] DocIndex { get; set; }

		/// <summary>
		/// Reference is stored in this block in the full-text-search index.
		/// </summary>
		public uint Block { get; set; }

		/// <summary>
		/// <see cref="Object.ToString()"/>
		/// </summary>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(this.Token);
			sb.Append(':');

			foreach (uint Index in this.DocIndex)
			{
				sb.Append(Index.ToString());
				sb.Append(' ');
			}

			sb.Append('(');
			sb.Append(this.Block.ToString());
			sb.Append(')');

			return sb.ToString();
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			int i, c = this.DocIndex.Length;

			if (!(obj is TokenCount Token) ||
				this.Token != Token.Token ||
				c != Token.DocIndex.Length ||
				this.Block != Token.Block)
			{
				return false;
			}

			for (i = 0; i < c; i++)
			{
				if (this.DocIndex[i] != Token.DocIndex[i])
					return false;
			}

			return true;
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result;

			Result = this.Token?.GetHashCode() ?? 0;
			Result ^= Result << 5 ^ this.Block.GetHashCode();

			if (!(this.DocIndex is null))
			{
				foreach (uint Index in this.DocIndex)
					Result ^= Result << 5 ^ Index.GetHashCode();
			}

			return Result;
		}
	}
}
