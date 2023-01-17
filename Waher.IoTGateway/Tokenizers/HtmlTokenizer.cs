using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Content;
using Waher.Content.Html;
using Waher.Persistence.FullTextSearch;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.IoTGateway.Tokenizers
{
	/// <summary>
	/// Tokenizes contents defined in an HTML document.
	/// </summary>
	public class HtmlTokenizer : ITokenizer, IFileTokenizer
	{
		/// <summary>
		/// Tokenizes contents defined in an HTML document.
		/// </summary>
		public HtmlTokenizer()
		{
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Type"/>.
		/// </summary>
		/// <param name="Type">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Type)
		{
			if (Type == typeof(HtmlDocument))
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// How well the file tokenizer supports files of a given extension.
		/// </summary>
		/// <param name="Extension">File extension (in lower case).</param>
		/// <returns>How well the tokenizer supports files having this extension.</returns>
		public Grade Supports(string Extension)
		{
			switch (Extension)
			{
				case "htm":
				case "html":
				case "xhtml":
					return Grade.Ok;

				default:
					return Grade.NotAtAll;
			}
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Value">Object to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public async Task Tokenize(object Value, TokenizationProcess Process)
		{
			if (Value is HtmlDocument Doc)
				await Tokenize(Doc, Process);
		}

		/// <summary>
		/// Tokenizes an HTML document.
		/// </summary>
		/// <param name="Doc">Document to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public static Task Tokenize(HtmlDocument Doc, TokenizationProcess Process)
		{
			StringBuilder sb = new StringBuilder();

			GetText(Doc.Root, sb);

			StringTokenizer.Tokenize(sb.ToString(), Process);

			return Task.CompletedTask;
		}

		private static void GetText(HtmlNode N, StringBuilder Text)
		{
			if (N is HtmlElement E)
			{
				foreach (HtmlAttribute Attr in E.Attributes)
				{
					Text.Append(' ');
					Text.Append(Attr.Value);
				}

				if (E.HasChildren)
				{
					foreach (HtmlNode N2 in E.Children)
						GetText(N2, Text);
				}
			}
			else if (N is HtmlText T)
			{
				Text.Append(' ');
				Text.Append(T.InlineText);
			}
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Reference">Reference to file to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public async Task Tokenize(FileReference Reference, TokenizationProcess Process)
		{
			string Text = await Resources.ReadAllTextAsync(Reference.FileName);
			HtmlDocument Doc = new HtmlDocument(Text);

			await Tokenize(Doc, Process);
		}
	}
}
