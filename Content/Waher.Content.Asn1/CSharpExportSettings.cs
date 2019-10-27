using System;
using System.Collections.Generic;
using Waher.Content.Asn1.Model;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// C# export settings
	/// </summary>
	public class CSharpExportSettings
	{
		private readonly Dictionary<string, KeyValuePair<Asn1Document, string>> imported =
			new Dictionary<string, KeyValuePair<Asn1Document, string>>();

		/// <summary>
		/// What encoders and decoders to include in the generation of C# code.
		/// </summary>
		public EncodingSchemes Codecs = EncodingSchemes.All;

		/// <summary>
		/// Base Namespace
		/// </summary>
		public string BaseNamespace;

		/// <summary>
		/// Import folders.
		/// </summary>
		public string[] ImportFolders;

		/// <summary>
		/// What encoders and decoders to include in the generation of C# code.
		/// </summary>
		/// <param name="BaseNamespace">Base namespace of generated code.</param>
		/// <param name="ImportFolders">Import folders.</param>
		public CSharpExportSettings(string BaseNamespace, params string[] ImportFolders)
		{
			this.BaseNamespace = BaseNamespace;
			this.ImportFolders = ImportFolders;
		}

		/// <summary>
		/// Namespace for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>Namespace</returns>
		public string Namespace(string Identifier)
		{
			return this.BaseNamespace + "." + Asn1Node.ToCSharp(Identifier);
		}

		/// <summary>
		/// If C# code is available for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>If C# code exists.</returns>
		public bool ContainsCode(string Identifier)
		{
			return this.imported.ContainsKey(Identifier);
		}

		/// <summary>
		/// Adds C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <param name="Code">C# Code</param>
		/// <param name="Document">Document</param>
		public void AddCode(string Identifier, string Code, Asn1Document Document)
		{
			this.imported[Identifier] = new KeyValuePair<Asn1Document, string>(Document, Code);
		}

		/// <summary>
		/// Gets C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>C# code</returns>
		public string GetCode(string Identifier)
		{
			return this.imported[Identifier].Value;
		}

		/// <summary>
		/// Gets C# code for a given identifier.
		/// </summary>
		/// <param name="Identifier">Identifier.</param>
		/// <returns>ASN.1 document.</returns>
		public Asn1Document GetDocument(string Identifier)
		{
			return this.imported[Identifier].Key;
		}

		/// <summary>
		/// Modules exported.
		/// </summary>
		public string[] Modules
		{
			get
			{
				string[] Result = new string[this.imported.Count];
				this.imported.Keys.CopyTo(Result, 0);
				return Result;
			}
		}
	}
}
