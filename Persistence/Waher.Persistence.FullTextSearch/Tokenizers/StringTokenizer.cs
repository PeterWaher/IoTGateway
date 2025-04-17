using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
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
		public Grade Supports(Type Type)
		{
			if (Type == typeof(string))
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
			if (Value is string s)
				Tokenize(s, Process);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Tokenizes a string.
		/// </summary>
		/// <param name="Text">String to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public static void Tokenize(string Text, TokenizationProcess Process)
		{
			if (string.IsNullOrEmpty(Text))
				return;

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

						if (!Process.TokenCounts.TryGetValue(Token, out ChunkedList<uint> DocIndex))
						{
							DocIndex = new ChunkedList<uint>();
							Process.TokenCounts[Token] = DocIndex;
						}

						DocIndex.Add(++Process.DocumentIndexOffset);
					}
				}
			}

			if (!First)
			{
				Token = sb.ToString();
				sb.Clear();

				if (!Process.TokenCounts.TryGetValue(Token, out ChunkedList<uint> DocIndex))
				{
					DocIndex = new ChunkedList<uint>();
					Process.TokenCounts[Token] = DocIndex;
				}

				DocIndex.Add(++Process.DocumentIndexOffset);
			}
		}

	}
}
