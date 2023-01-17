using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes case insensitive strings.
	/// </summary>
	public class CaseInsensitiveStringTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes case insensitive strings.
		/// </summary>
		public CaseInsensitiveStringTokenizer()
		{
		}

		/// <summary>
		/// How well the tokenizer can tokenize objects of type <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Type of object to tokenize</param>
		/// <returns>How well such objects are tokenized.</returns>
		public Grade Supports(Type Type)
		{
			if (Type == typeof(CaseInsensitiveString))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is CaseInsensitiveString s)
				StringTokenizer.Tokenize(s.LowerCase, Process);

			return Task.CompletedTask;
		}

	}
}
