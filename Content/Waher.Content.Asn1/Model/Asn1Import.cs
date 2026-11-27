using System.IO;
using System.Threading.Tasks;
using Waher.Content.Asn1.Model.Values;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents one import instruction.
	/// </summary>
	public class Asn1Import : Asn1Node
	{
		private readonly Asn1IdentifierReference[] identifiers;
		private readonly string module;
		private Asn1Oid oid;
		private Asn1Document importedDocument;
		private bool withSuccessors = false;

		/// <summary>
		/// Represents one import instruction.
		/// </summary>
		/// <param name="Identifiers">Identifiers to import.</param>
		/// <param name="Module">Module reference.</param>
		/// <param name="Document">ASN.1 Document</param>
		public Asn1Import(Asn1IdentifierReference[] Identifiers, string Module, Asn1Document Document)
			: base(Document)
		{
			this.identifiers = Identifiers;
			this.module = Module;
			this.importedDocument = null;
		}

		/// <summary>
		/// Identifiers to import.
		/// </summary>
		public Asn1IdentifierReference[] Identifiers => this.identifiers;

		/// <summary>
		/// Module reference.
		/// </summary>
		public string Module => this.module;

		/// <summary>
		/// Object Identifier of module.
		/// </summary>
		public Asn1Oid Oid
		{
			get => this.oid;
			internal set => this.oid = value;
		}

		/// <summary>
		/// If a successor module is acceptable.
		/// </summary>
		public bool WithSuccessors
		{
			get => this.withSuccessors;
			internal set => this.withSuccessors = value;
		}

		/// <inheritdoc/>
		public override string ToString() => this.module;

		/// <summary>
		/// Loads the ASN.1 document to import.
		/// </summary>
		/// <exception cref="FileNotFoundException">If the import file is not found.</exception>
		public async Task<Asn1Document> LoadDocument()
		{
			if (!(this.importedDocument is null))
				return this.importedDocument;

			string Folder = Path.GetDirectoryName(this.Document.Location);
			string Extension = Path.GetExtension(this.Document.Location);
			string FileName = Path.Combine(Folder, this.module) + Extension;

			if (!File.Exists(FileName))
			{
				FileName = null;

				foreach (string ImportFolder in this.Document.ImportFolders)
				{
					FileName = Path.Combine(ImportFolder, this.module) + Extension;
					if (File.Exists(FileName))
						break;
					else
						FileName = null;
				}

				if (FileName is null)
					throw new FileNotFoundException("Unable to find import file for module " + this.module);
			}

			this.importedDocument = await Asn1Document.FromFile(FileName, this.Document.ImportFolders);

			return this.importedDocument;
		}

	}
}
