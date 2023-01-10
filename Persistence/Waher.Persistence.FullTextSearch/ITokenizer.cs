using System;
using System.Collections.Generic;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch
{
	/// <summary>
	/// Interface for full-text-search tokenizers
	/// </summary>
	public interface ITokenizer : IProcessingSupport<Type>
	{
		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="TokenCounts">Token counts.</param>
		/// <returns>Tokens found.</returns>
		void Tokenize(object Value, Dictionary<string, List<uint>> TokenCounts);
	}
}
