using System;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Contains a sequence of object references that include the token
	/// in its indexed text properties.
	/// </summary>
	[TypeName(TypeNameSerialization.FullName)]
	public class TokenReferences
	{
		/// <summary>
		/// Maximum amount of references in a block (100).
		/// </summary>
		public const int MaxReferences= 100;

		/// <summary>
		/// Contains a sequence of object references that include the token
		/// in its indexed text properties.
		/// </summary>
		public TokenReferences()
		{ 
		}

		/// <summary>
		/// Index to last block in index representing the same token.
		/// </summary>
		public uint LastBlock { get; set; }

		/// <summary>
		/// References to objects containing the token.
		/// </summary>
		public ulong[] ObjectReferences { get; set; }

		/// <summary>
		/// Token counts for respective object reference.
		/// </summary>
		public uint[] Counts { get; set; }

		/// <summary>
		/// Timestamps when corresponding object refernces were indexed.
		/// </summary>
		public DateTime[] Timestamps { get; set; }
	}
}
