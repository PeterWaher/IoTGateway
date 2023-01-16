using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Files
{
	/// <summary>
	/// Interface for file tokenizers. Best tokenizer is selected 
	/// </summary>
	public interface IFileTokenizer : IProcessingSupport<string>
	{
		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Reference">Reference to file to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		Task Tokenize(FileReference Reference, TokenizationProcess Process);
	}
}
