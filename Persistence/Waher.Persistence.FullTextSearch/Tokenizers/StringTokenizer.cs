using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes strings.
	/// </summary>
	public class StringTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes strings.
		/// </summary>
		public StringTokenizer()
		{
		}

		/// <summary>
		/// How well the tokenizer can tokenize objects of type <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Type of object to tokenize</param>
		/// <returns>How well such objects are tokenized.</returns>
		public Grade Supports(Type Object)
		{
			if (Object == typeof(string))
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
			if (Value is string s)
				Tokenize(s, TokenCounts, ref DocumentIndexOffset);
		}

		/// <summary>
		/// Tokenizes a set of strings.
		/// </summary>
		/// <param name="Text">String to tokenize.</param>
		/// <param name="TokenCounts">Token counts.</param>
		/// <param name="DocumentIndexOffset">Document Index Offset. Used to
		/// identify sequences of tokens in a document.</param>
		public static void Tokenize(string Text, Dictionary<string, List<uint>> TokenCounts,
			ref uint DocumentIndexOffset)
		{
			UnicodeCategory Category;
			StringBuilder sb = new StringBuilder();
			string Token;
			bool First = true;

			foreach (char ch in Text.ToLower().Normalize(NormalizationForm.FormD))
			{
				Category = CharUnicodeInfo.GetUnicodeCategory(ch);
				if (Category == UnicodeCategory.NonSpacingMark)
					continue;

				if (char.IsLetterOrDigit(ch))
				{
					sb.Append(ch);
					First = false;
				}
				else
				{
					if (!First)
					{
						Token = sb.ToString();
						sb.Clear();
						First = true;

						if (!TokenCounts.TryGetValue(Token, out List<uint> DocIndex))
						{
							DocIndex = new List<uint>();
							TokenCounts[Token] = DocIndex;
						}

						DocIndex.Add(++DocumentIndexOffset);
					}
				}
			}

			if (!First)
			{
				Token = sb.ToString();
				sb.Clear();

				if (!TokenCounts.TryGetValue(Token, out List<uint> DocIndex))
				{
					DocIndex = new List<uint>();
					TokenCounts[Token] = DocIndex;
				}

				DocIndex.Add(++DocumentIndexOffset);
			}
		}

	}
}
