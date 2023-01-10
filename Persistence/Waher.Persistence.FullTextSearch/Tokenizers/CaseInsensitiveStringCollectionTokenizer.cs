using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes a collection of case insensitive strings.
	/// </summary>
	public class CaseInsensitiveStringCollectionTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes a collection of case insensitive strings.
		/// </summary>
		public CaseInsensitiveStringCollectionTokenizer()
		{
		}

		/// <summary>
		/// How well the tokenizer can tokenize objects of type <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Type of object to tokenize</param>
		/// <returns>How well such objects are tokenized.</returns>
		public Grade Supports(Type Object)
		{
			if (typeInfo.IsAssignableFrom(Object))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		private static readonly TypeInfo typeInfo = typeof(IEnumerable<CaseInsensitiveString>).GetTypeInfo();

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
			if (Value is IEnumerable<CaseInsensitiveString> Strings)
			{
				foreach (CaseInsensitiveString cis in Strings)
				{
					StringTokenizer.Tokenize(cis.LowerCase, TokenCounts, ref DocumentIndexOffset);
					DocumentIndexOffset++;  // Make sure sequences of keywords don't cross element boundaries.
				}
			}
		}

	}
}
