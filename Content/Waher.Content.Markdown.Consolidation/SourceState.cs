using System.Threading.Tasks;
using Waher.Runtime.Collections;

namespace Waher.Content.Markdown.Consolidation
{
	/// <summary>
	/// Maintains the state of one source.
	/// </summary>
	public class SourceState
	{
		private readonly ChunkedList<DocumentInformation> documents = new ChunkedList<DocumentInformation>();
		private readonly string source;
		private DocumentType type = DocumentType.Empty;
		private string firstText = null;
		private int nrDocuments = 0;
		private readonly bool isDefault;

		/// <summary>
		/// Maintains the state of one source.
		/// </summary>
		/// <param name="Source">Source</param>
		/// <param name="IsDefault">If the content is default content (true), or reported content (false).</param>
		public SourceState(string Source, bool IsDefault)
		{
			this.source = Source;
			this.isDefault = IsDefault;
		}

		/// <summary>
		/// Source
		/// </summary>
		public string Source => this.source;

		/// <summary>
		/// If the content is default content (true), or reported content (false).
		/// </summary>
		public bool IsDefault => this.isDefault;

		/// <summary>
		/// First document text.
		/// </summary>
		public async Task<string> GetFirstText()
		{
			if (!(this.firstText is null))
				return this.firstText;

			MarkdownDocument Doc = this.FirstDocument?.Markdown;
			if (Doc is null)
				return string.Empty;

			this.firstText = await Doc.GenerateMarkdown(false);

			return this.firstText;
		}

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
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>Consolidated document type.</returns>
		public async Task<DocumentType> Add(MarkdownDocument Markdown, string Id)
		{
			DocumentInformation Info = await DocumentInformation.CreateAsync(Markdown, Id);

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

		/// <summary>
		/// Updates a markdown document from the source. If not found, it is added.
		/// </summary>
		/// <param name="Markdown">Markdown document.</param>
		/// <param name="Id">Optional ID of document.</param>
		/// <returns>Consolidated document type.</returns>
		public async Task<DocumentType> Update(MarkdownDocument Markdown, string Id)
		{
			if (string.IsNullOrEmpty(Id))
				return await this.Add(Markdown, Id);

			DocumentInformation Info = await DocumentInformation.CreateAsync(Markdown, Id);
			DocumentInformation Info2;
			int i, c;

			lock (this.documents)
			{
				c = this.documents.Count;
				for (i = 0; i < c; i++)
				{
					Info2 = this.documents[i];
					if (Info2.Id == Id)
					{
						this.documents[i] = Info;

						if (this.nrDocuments == 1)
							this.type = Info.Type;

						return this.type;
					}
				}

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
