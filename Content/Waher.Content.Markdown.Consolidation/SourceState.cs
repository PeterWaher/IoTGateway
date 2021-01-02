using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Maintains the state of one source.
	/// </summary>
	public class SourceState
	{
		private readonly List<DocumentInformation> documents = new List<DocumentInformation>();
		private readonly string source;
		private DocumentType type = DocumentType.Empty;
		private int nrDocuments = 0;

		/// <summary>
		/// Maintains the state of one source.
		/// </summary>
		/// <param name="Source">Source</param>
		public SourceState(string Source)
		{
			this.source = Source;
		}

		/// <summary>
		/// Source
		/// </summary>
		public string Source => this.source;

		/// <summary>
		/// First document text.
		/// </summary>
		public string FirstText => this.FirstDocument?.Markdown?.MarkdownText ?? string.Empty;

		/// <summary>
		/// First document.
		/// </summary>
		public DocumentInformation FirstDocument
		{
			get
			{
				lock (this.documents)
				{
					if (this.documents.Count == 0)
						return null;
					else
						return this.documents[0];
				}
			}
		}

		/// <summary>
		/// Available documents from source.
		/// </summary>
		public DocumentInformation[] Documents
		{
			get
			{
				lock (this.documents)
				{
					return this.documents.ToArray();
				}
			}
		}

		/// <summary>
		/// Adds a markdown document from the source.
		/// </summary>
		/// <param name="Markdown">Markdown document.</param>
		/// <returns>Consolidated document type.</returns>
		public DocumentType Add(MarkdownDocument Markdown)
		{
			DocumentInformation Info = new DocumentInformation(Markdown);

			lock (this.documents)
			{
				this.documents.Add(Info);
				this.nrDocuments++;

				if (this.nrDocuments == 1)
					this.type = Info.Type;
				else
					this.type = DocumentType.Complex;

				return this.type;
			}
		}
	}
}
