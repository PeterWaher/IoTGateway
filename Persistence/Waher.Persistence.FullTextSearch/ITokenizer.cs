using System;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Tokenizers;
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
		/// <param name="Process">Current tokenization process.</param>
		Task Tokenize(object Value, TokenizationProcess Process);
	}
}
