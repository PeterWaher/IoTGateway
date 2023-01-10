using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Tokenizers
{
	/// <summary>
	/// Tokenizes contents defined in a Markdown document.
	/// </summary>
	public class MarkdownTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes contents defined in a Markdown document.
		/// </summary>
		public MarkdownTokenizer()
		{
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			if (Object == typeof(MarkdownDocument))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public async Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is MarkdownDocument Doc)
			{
				string Text = await Doc.GeneratePlainText();
				StringTokenizer.Tokenize(Text, Process);
			}
		}
	}
}
