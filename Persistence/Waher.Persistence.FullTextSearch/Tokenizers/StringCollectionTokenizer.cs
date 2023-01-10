using System;
using System.Collections.Generic;
using System.Reflection;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes a collection of strings.
	/// </summary>
	public class StringCollectionTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes a collection of strings.
		/// </summary>
		public StringCollectionTokenizer()
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

		private static readonly TypeInfo typeInfo = typeof(IEnumerable<string>).GetTypeInfo();

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="TokenCounts">Token counts.</param>
		public void Tokenize(object Value, Dictionary<string, List<uint>> TokenCounts)
		{
			if (Value is IEnumerable<string> Strings)
			{
				foreach (string s in Strings)
					StringTokenizer.Tokenize(s, TokenCounts);
			}
		}

	}
}
