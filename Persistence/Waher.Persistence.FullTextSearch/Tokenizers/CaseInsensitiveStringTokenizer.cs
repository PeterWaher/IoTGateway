using System;
using System.Collections.Generic;
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
		public Grade Supports(Type Object)
		{
			if (Object == typeof(CaseInsensitiveString))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="TokenCounts">Token counts.</param>
		/// <param name="DocumentIndexOffset">Document Index Offset. Used to
		/// identify sequences of tokens in a document.</param>
		public void Tokenize(object Value, Dictionary<string, List<uint>> TokenCounts,
			ref uint DocumentIndexOffset)
		{
			if (Value is CaseInsensitiveString s)
				StringTokenizer.Tokenize(s.LowerCase, TokenCounts, ref DocumentIndexOffset);
		}

	}
}
