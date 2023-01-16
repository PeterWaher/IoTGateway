using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Persistence.FullTextSearch.Tokenizers;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Files
{
	/// <summary>
	/// Tokenizes XML files.
	/// </summary>
	public class XmlFileTokenizer : IFileTokenizer
	{
		/// <summary>
		/// Tokenizes XML files.
		/// </summary>
		public XmlFileTokenizer()
		{
		}

		/// <summary>
		/// How well the file tokenizer supports files of a given extension.
		/// </summary>
		/// <param name="Extension">File extension (in lower case).</param>
		/// <returns>How well the tokenizer supports files having this extension.</returns>
		public Grade Supports(string Extension)
		{
			if (Extension == "xml")
				return Grade.Ok;
			else
				return Grade.NotAtAll;
		}

		/// <summary>
		/// Tokenizes an object.
		/// </summary>
		/// <param name="Reference">Reference to file to tokenize.</param>
		/// <param name="Process">Current tokenization process.</param>
		public async Task Tokenize(FileReference Reference, TokenizationProcess Process)
		{
			string Text = await Resources.ReadAllTextAsync(Reference.FileName);
			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(Text);

			XmlTokenizer.Tokenize(Doc, Process);
		}
	}
}
