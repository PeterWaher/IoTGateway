using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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
		/// <param name="Process">Current tokenization process.</param>
		public Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is IEnumerable<CaseInsensitiveString> Strings)
			{
				foreach (CaseInsensitiveString cis in Strings)
				{
					StringTokenizer.Tokenize(cis.LowerCase, Process);
					Process.DocumentIndexOffset++;  // Make sure sequences of keywords don't cross element boundaries.
				}
			}

			return Task.CompletedTask;
		}

	}
}
