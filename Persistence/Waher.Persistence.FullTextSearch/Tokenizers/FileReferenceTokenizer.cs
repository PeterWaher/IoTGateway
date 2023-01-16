using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Waher.Persistence.FullTextSearch.Files;
using Waher.Runtime.Inventory;

namespace Waher.Persistence.FullTextSearch.Tokenizers
{
	/// <summary>
	/// Tokenizes files via <see cref="FileReference"/> object references.
	/// </summary>
	public class FileReferenceTokenizer : ITokenizer
	{
		/// <summary>
		/// Tokenizes files via <see cref="FileReference"/> object references.
		/// </summary>
		public FileReferenceTokenizer()
		{
		}

		/// <summary>
		/// If the interface understands objects such as <paramref name="Object"/>.
		/// </summary>
		/// <param name="Object">Object</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(Type Object)
		{
			if (Object == typeof(FileReference))
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
			if (Value is FileReference Ref)
				await Tokenize(Ref, Process);
		}

		/// <summary>
		/// Tokenizes a file, via its file reference object.
		/// </summary>
		/// <param name="Reference">File reference.</param>
		/// <param name="Process">Current tokenization process.</param>
		public static async Task Tokenize(FileReference Reference, TokenizationProcess Process)
		{
			if (File.Exists(Reference.FileName) &&
				TryGetFileTokenizer(Reference.FileName, out IFileTokenizer Tokenizer))
			{
				await Tokenizer.Tokenize(Reference, Process);
			}
		}

		/// <summary>
		/// Checks if a file has a file tokenizer associated with it.
		/// </summary>
		/// <param name="FileName">File Name</param>
		/// <returns>If a tokenizer was found.</returns>
		public static bool HasTokenizer(string FileName)
		{
			return TryGetFileTokenizer(FileName, out _);
		}

		/// <summary>
		/// Tries to get a file tokenizer for a given file.
		/// </summary>
		/// <param name="FileName">File Name</param>
		/// <param name="Tokenizer">Tokenizer, if found.</param>
		/// <returns>If a tokenizer was found.</returns>
		public static bool TryGetFileTokenizer(string FileName, out IFileTokenizer Tokenizer)
		{
			string Extension = Path.GetExtension(FileName).ToLower();

			if (Extension.StartsWith("."))
				Extension = Extension.Substring(1);

			lock (tokenizers)
			{
				if (!tokenizers.TryGetValue(Extension, out Tokenizer))
					Tokenizer = null;
				else if (Tokenizer is null)
					return false;
			}

			Tokenizer = Types.FindBest<IFileTokenizer, string>(Extension);

			lock (tokenizers)
			{
				tokenizers[Extension] = Tokenizer;
			}

			return !(Tokenizer is null);
		}

		private static readonly Dictionary<string, IFileTokenizer> tokenizers = new Dictionary<string, IFileTokenizer>();

		static FileReferenceTokenizer()
		{
			Types.OnInvalidated += Types_OnInvalidated;
		}

		private static void Types_OnInvalidated(object sender, EventArgs e)
		{
			lock (tokenizers)
			{
				tokenizers.Clear();
			}
		}
	}
}
