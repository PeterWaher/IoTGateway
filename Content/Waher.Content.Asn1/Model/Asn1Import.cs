using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Waher.Content.Asn1.Exceptions;

namespace Waher.Content.Asn1.Model
{
	/// <summary>
	/// Represents one import instruction.
	/// </summary>
	public class Asn1Import : Asn1Node
	{
		private readonly string[] identifiers;
		private readonly string module;
		private readonly Asn1Document document;

		/// <summary>
		/// Represents one import instruction.
		/// </summary>
		/// <param name="Identifiers">Identifiers to import.</param>
		/// <param name="Module">Module reference.</param>
		/// <param name="Document">ASN.1 Document</param>
		public Asn1Import(string[] Identifiers, string Module, Asn1Document Document)
		{
			this.identifiers = Identifiers;
			this.module = Module;
			this.document = Document;
		}

		/// <summary>
		/// Identifiers to import.
		/// </summary>
		public string[] Identifiers => this.identifiers;

		/// <summary>
		/// Module reference.
		/// </summary>
		public string Module => this.module;

		/// <summary>
		/// Loads the ASN.1 document to import.
		/// </summary>
		/// <param name="Settings">Export settings.</param>
		/// <exception cref="FileNotFoundException">If the import file is not found.</exception>
		public Asn1Document LoadDocument(CSharpExportSettings Settings)
		{
			string Folder = Path.GetDirectoryName(this.document.Location);
			string FileName = Path.Combine(Folder, this.module);
			string Extension = Path.GetExtension(this.document.Location);

			if (!File.Exists(FileName))
			{
				FileName = null;

				foreach (string ImportFolder in Settings.ImportFolders)
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

			return Asn1Document.FromFile(FileName);
		}

	}
}
