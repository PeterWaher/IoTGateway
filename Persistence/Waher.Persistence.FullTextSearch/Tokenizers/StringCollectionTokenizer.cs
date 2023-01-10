using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
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
		/// <param name="Process">Current tokenization process.</param>
		public Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is IEnumerable<string> Strings)
			{
				foreach (string s in Strings)
				{
					StringTokenizer.Tokenize(s, Process);
					Process.DocumentIndexOffset++;  // Make sure sequences of keywords don't cross element boundaries.
				}
			}

			return Task.CompletedTask;
		}

	}
}
